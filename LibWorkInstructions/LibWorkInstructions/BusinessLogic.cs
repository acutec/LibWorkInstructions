using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using static LibWorkInstructions.Structs;

namespace LibWorkInstructions
{
    public class BusinessLogic
    {

        #region database-mocking
        public class MockDB
        {
            public Dictionary<string, Job> Jobs = new Dictionary<string, Job>();
            public Dictionary<Guid, List<OpSpec>> OpSpecs = new Dictionary<Guid, List<OpSpec>>();
            public Dictionary<Guid, List<QualityClause>> QualityClauses = new Dictionary<Guid, List<QualityClause>>();
            // revs clone an existing one under a new id, with a same groupid
            public Dictionary<Guid, List<WorkInstruction>> WorkInstructions = new Dictionary<Guid, List<WorkInstruction>>();
            public Dictionary<string, List<List<Guid>>> JobRefToWorkInstructionRefs = new Dictionary<string, List<List<Guid>>>();
            public Dictionary<string, List<Guid>> JobRefToQualityClauseRefs = new Dictionary<string, List<Guid>>();
            public List<Event> AuditLog = new List<Event>();
        }
        private MockDB db;  // this should contain any/all state used in this BusinessLogic class.
        public BusinessLogic()
        {
            this.db = new MockDB();
        }
        public void DataImport(MockDB replacementDb) => this.db = replacementDb;
        public MockDB DataExport() => db;
        #endregion

        public Job GetJob(string jobId) =>
                db.Jobs.First(y => y.Key == jobId).Value;

        public void AddJob(Job newJob)
        {
            if (!db.Jobs.ContainsKey(newJob.Id))
            {
                db.Jobs.Add(newJob.Id, newJob);
                db.JobRefToWorkInstructionRefs.Add(newJob.Id, new List<List<Guid>>(new List<Guid>[newJob.Ops.Count]));
                db.JobRefToQualityClauseRefs.Add(newJob.Id, new List<Guid>());

                var args = new Dictionary<string, string>();
                args["NewJob"] = JsonSerializer.Serialize(newJob);
                db.AuditLog.Add(new Event
                {
                    Action = "AddJob",
                    Args = args,
                    When = DateTime.Now,
                });
            }
            else
            {
                throw new Exception("Job already exists in the database");
            }
        }
        
        public WorkInstruction GetWorkInstruction(Guid groupId, Guid instructionId) =>
            db.WorkInstructions.First(y => y.Key == groupId).Value.First(y => y.Id == instructionId);

        public WorkInstruction AddWorkInstruction(WorkInstruction workInstruction)
        {
            WorkInstruction newWorkInstruction = workInstruction;
            if(!db.WorkInstructions.ContainsKey(workInstruction.IdRevGroup))
                db.WorkInstructions[newWorkInstruction.IdRevGroup] = new List<WorkInstruction>();
            if (db.WorkInstructions[newWorkInstruction.IdRevGroup].FindIndex(y => y.Equals(newWorkInstruction)) < 0)
                db.WorkInstructions[newWorkInstruction.IdRevGroup].Add(newWorkInstruction);
            else
                throw new Exception("Work instruction already exists in the database.");
            string jobId = db.Jobs.First(y => y.Value.Ops.FindIndex(y => y.Id == newWorkInstruction.OpId) >= 0).Key;
            int opIndex = db.Jobs[jobId].Ops.FindIndex(y => y.Id == newWorkInstruction.OpId);
            if (opIndex >= 0)
                db.JobRefToWorkInstructionRefs[jobId][opIndex].Add(newWorkInstruction.Id);

            var args = new Dictionary<string, string>();
            args["workInstruction"] = JsonSerializer.Serialize(newWorkInstruction);
            db.AuditLog.Add(new Event
            {
                Action = "AddWorkInstruction",
                Args = args,
                When = DateTime.Now,
            }) ;

            return newWorkInstruction;
         }
        public WorkInstruction ChangeWorkInstruction(WorkInstruction newWorkInstruction)
        {
            if (db.WorkInstructions.ContainsKey(newWorkInstruction.IdRevGroup))
            {
                if (db.WorkInstructions[newWorkInstruction.IdRevGroup].FindIndex(y => y.Equals(newWorkInstruction)) < 0)
                {
                    db.WorkInstructions[newWorkInstruction.IdRevGroup].Add(newWorkInstruction);
                    string jobId = db.Jobs.First(y => y.Value.Ops.FindIndex(y => y.Id == newWorkInstruction.OpId) >= 0).Key;
                    int opIndex = db.Jobs[jobId].Ops.FindIndex(y => y.Id == newWorkInstruction.OpId);
                    if (opIndex >= 0)
                        db.JobRefToWorkInstructionRefs[jobId][opIndex].Add(newWorkInstruction.Id);
                }
                else
                    throw new Exception("The new work instruction already exists in the database");

                var args = new Dictionary<string, string>();
                args["newWorkInstruction"] = JsonSerializer.Serialize(db.WorkInstructions[newWorkInstruction.IdRevGroup].Last());
                db.AuditLog.Add(new Event
                {
                    Action = "ChangeWorkInstruction",
                    Args = args,
                    When = DateTime.Now,
                });
                return newWorkInstruction;
            }
            else
            {
                throw new Exception("This old work instruction doesn't exist in the database");
            }
        }
        public void RemoveWorkInstruction(Guid targetGroupId, Guid targetWorkId)
        {
            if (db.WorkInstructions.ContainsKey(targetGroupId)) 
            {
                WorkInstruction targetWorkInstruction = db.WorkInstructions[targetGroupId].First(y => y.Id == targetWorkId);
                foreach (var value in db.JobRefToWorkInstructionRefs.Values.Where(y => y.FindIndex(y => y.Contains(targetWorkInstruction.Id)) >= 0))
                {
                    foreach (List<Guid> list in value)
                    {
                        list.Remove(targetWorkInstruction.Id);
                    }
                }
                db.WorkInstructions[targetGroupId].Remove(targetWorkInstruction);

                var args = new Dictionary<string, string>();
                args["TargetGroupId"] = targetGroupId.ToString();
                args["TargetWorkId"] = targetWorkId.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "RemoveWorkInstruction",
                    Args = args,
                    When = DateTime.Now,
                });
            }
            else
            {
                throw new Exception("This work instruction group doesn't exist in the database");
            }
        }
        public void MergeWorkInstructions(string jobId1, string jobId2)
        {
            if (db.Jobs.ContainsKey(jobId1) && db.Jobs.ContainsKey(jobId2))
            {
                List<List<Guid>> mergedList = db.JobRefToWorkInstructionRefs[jobId1].Union(db.JobRefToWorkInstructionRefs[jobId2]).ToList();
                db.JobRefToWorkInstructionRefs[jobId1] = mergedList;
                db.JobRefToWorkInstructionRefs[jobId2] = mergedList;

                var args = new Dictionary<string, string>();
                args["jobId1"] = jobId1;
                args["jobId2"] = jobId2;
                db.AuditLog.Add(new Event
                {
                    Action = "MergeWorkInstructions",
                    Args = args,
                    When = DateTime.Now,
                });
            }
            else
            {
                throw new Exception("One(or both) of the jobs doesn't exist in the database");
            }
        }

        public void SplitWorkInstruction(Guid groupId, Guid workId)
        {
            if (db.WorkInstructions.ContainsKey(groupId))
            {
                WorkInstruction duplicate = new WorkInstruction();

                foreach (List<WorkInstruction> workInstructionGroup in db.WorkInstructions.Values)
                {
                    foreach (WorkInstruction workInstruction in workInstructionGroup)
                    {
                        if (workInstruction.Id == workId)
                        {
                            duplicate = workInstruction;
                        }
                    }
                }
                string jobId = db.Jobs.First(y => y.Value.Ops.FindIndex(y => y.Id == duplicate.OpId) >= 0).Key;
                int opIndex = db.Jobs[jobId].Ops.FindIndex(y => y.Id == duplicate.OpId);
                if (opIndex >= 0)
                    db.JobRefToWorkInstructionRefs[jobId][opIndex].Add(duplicate.Id);

                var args = new Dictionary<string, string>();
                args["GroupId"] = groupId.ToString();
                args["WorkId"] = workId.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "SplitWorkInstruction",
                    Args = args,
                    When = DateTime.Now,
                });
            }
            else
            {
                throw new Exception("The work instruction doesn't exist in the database");
            }
        }

        public void CloneWorkInstruction(Guid sourceWorkId, string targetJobId)
        {
            if (db.Jobs.ContainsKey(targetJobId))
            {
                db.JobRefToWorkInstructionRefs[targetJobId].Add(new List<Guid> { sourceWorkId });

                var args = new Dictionary<string, string>();
                args["SourceWorkId"] = sourceWorkId.ToString();
                args["TargetJobId"] = targetJobId;
                db.AuditLog.Add(new Event
                {
                    Action = "CloneWorkInstruction",
                    Args = args,
                    When = DateTime.Now,
                });
            }
            else
            {
                throw new Exception("The job doesn't exist in the database");
            }
        }

        public OpSpec AddSpec(OpSpec newSpec)
        {
            OpSpec opSpec = newSpec;
            db.OpSpecs.Add(opSpec.IdRevGroup, new List<OpSpec> { opSpec });


            var args = new Dictionary<string, string>();
            args["newSpec"] = JsonSerializer.Serialize(opSpec);
            db.AuditLog.Add(new Event
            {
                Action = "AddSpec",
                Args = args,
                When = DateTime.Now,
            });
            return opSpec;
        }

        public void ChangeSpec(OpSpec newSpec)
        {
            if (db.OpSpecs.ContainsKey(newSpec.IdRevGroup))
            {
                db.OpSpecs[newSpec.IdRevGroup].Add(newSpec);

                var args = new Dictionary<string, string>();
                args["newSpec"] = JsonSerializer.Serialize(newSpec);
                db.AuditLog.Add(new Event
                {
                    Action = "ChangeSpec",
                    Args = args,
                    When = DateTime.Now,
                });
            }
            else
            {
                throw new Exception("The spec being changed doesn't exist in the database");
            }
        }

        public void DeleteSpec(Guid targetGroupId, Guid targetSpecId)
        {
            if (db.OpSpecs.ContainsKey(targetGroupId))
            {
                OpSpec targetOpSpec = db.OpSpecs[targetGroupId].First(y => y.Id == targetSpecId);
                db.OpSpecs[targetGroupId].Remove(targetOpSpec);

                var args = new Dictionary<string, string>();
                args["TargetGroupId"] = targetGroupId.ToString();
                args["TargetSpecId"] = targetSpecId.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "DeleteSpec",
                    Args = args,
                    When = DateTime.Now,
                });
            }
            else
            {
                throw new Exception("The spec doesn't exist in the database");
            }
        }

        public void MergeSpecs(int opId1, int opId2)
        {
            int opIndex1 = db.Jobs.Values.ToList().FindIndex(y => y.Ops.FindIndex(y => y.Id == opId1) >= 0);
            int opIndex2 = db.Jobs.Values.ToList().FindIndex(y => y.Ops.FindIndex(y => y.Id == opId2) >= 0);
            if (opIndex1 >= 0 && opIndex2 >= 0)
            {
                List<OpSpec> list1 = db.Jobs.Values.ToList()[opIndex1].Ops.First(y => y.Id == opId1).OpSpecs;
                List<OpSpec> list2 = db.Jobs.Values.ToList()[opIndex2].Ops.First(y => y.Id == opId2).OpSpecs;
                List<OpSpec> mergedList = list1.Union(list2).ToList();

                db.Jobs.Values.ToList()[opIndex1].Ops.First(y => y.Id == opId1).OpSpecs = mergedList;
                db.Jobs.Values.ToList()[opIndex2].Ops.First(y => y.Id == opId2).OpSpecs = mergedList;

                var args = new Dictionary<string, string>();
                args["opId1"] = opId1.ToString();
                args["opId2"] = opId2.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "MergeSpecs",
                    Args = args,
                    When = DateTime.Now,
                });
            }
            else
            {
                throw new Exception("One(or both) of the operations doesn't exist in the database");
            }
        }

        public void SplitSpec(Guid sourceGroupId, Guid sourceSpecId)
        {
            if (db.OpSpecs.ContainsKey(sourceGroupId))
            {
                OpSpec duplicate = db.OpSpecs[sourceGroupId].First(y => y.Id == sourceSpecId);
                db.OpSpecs[sourceGroupId].Add(duplicate);

                var args = new Dictionary<string, string>();
                args["SourceGroupId"] = sourceGroupId.ToString();
                args["SourceSpecId"] = sourceSpecId.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "SplitSpecs",
                    Args = args,
                    When = DateTime.Now,
                });
            }
            else
            {
                throw new Exception("The spec doesn't exist in the database");
            }
        }

        public void CloneSpecs(int sourceOpId, int targetOpId, bool overwrite)
        {
            int sourceOpIndex = db.Jobs.Values.ToList().FindIndex(y => y.Ops.FindIndex(y => y.Id == sourceOpId) >= 0);
            int targetOpIndex = db.Jobs.Values.ToList().FindIndex(y => y.Ops.FindIndex(y => y.Id == targetOpId) >= 0);
            if (sourceOpIndex >= 0 && targetOpIndex >= 0)
            {
                List<OpSpec> sourceList = db.Jobs.Values.ToList()[sourceOpIndex].Ops.First(y => y.Id == sourceOpIndex).OpSpecs;
                List<OpSpec> targetList = db.Jobs.Values.ToList()[targetOpIndex].Ops.First(y => y.Id == targetOpIndex).OpSpecs;
                List<OpSpec> mergedList = sourceList.Union(targetList).ToList();
                if (overwrite) 
                {
                    db.Jobs.Values.ToList()[targetOpIndex].Ops.First(y => y.Id == targetOpIndex).OpSpecs = sourceList;
                }
                else
                {
                    db.Jobs.Values.ToList()[targetOpIndex].Ops.First(y => y.Id == targetOpIndex).OpSpecs = mergedList;
                }

                var args = new Dictionary<string, string>();
                args["SourceOpId"] = sourceOpId.ToString();
                args["TargetOpId"] = targetOpId.ToString();
                args["Overwrite"] = overwrite.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "CloneSpecs",
                    Args = args,
                    When = DateTime.Now,
                });
            }
            else
            {
                throw new Exception("One(or both) of the operations doesn't exist in the database");
            }
        }

        public QualityClause AddQualityClause(QualityClause newQualityClause)
        {
            db.QualityClauses.Add(newQualityClause.IdRevGroup, new List<QualityClause> { newQualityClause });

            var args = new Dictionary<string, string>();
            args["NewQualityClause"] = JsonSerializer.Serialize(newQualityClause);
            db.AuditLog.Add(new Event
            {
                Action = "CreateQualityClause",
                Args = args,
                When = DateTime.Now,
            });
            return newQualityClause;
        }

        public void MergeQualityClauses(string job1, string job2)
        {
            if (db.Jobs.ContainsKey(job1) && db.Jobs.ContainsKey(job2))
            {
                db.JobRefToQualityClauseRefs[job1] =
                    db.JobRefToQualityClauseRefs[job1].Union(db.JobRefToQualityClauseRefs[job2]).ToList();
                db.JobRefToQualityClauseRefs[job2] = db.JobRefToQualityClauseRefs[job1];

                var args = new Dictionary<string, string>();
                args["Job1"] = job1;
                args["Job2"] = job2;
                db.AuditLog.Add(new Event
                {
                    Action = "MergeQualityClauses",
                    Args = args,
                    When = DateTime.Now,
                });
            }
            else
            {
                throw new Exception("One(or both) of the jobs doesn't exist in the database");
            }
        }

        public void DeleteQualityClause(Guid ClauseId)
        {
            if (db.QualityClauses.ContainsKey(ClauseId))
            {
                QualityClause targetQualityClause = db.QualityClauses[ClauseId].First(y => y.Id == ClauseId);
                db.QualityClauses[ClauseId].Remove(targetQualityClause);
                foreach (var clause in db.JobRefToQualityClauseRefs.Where(y => y.Value.Contains(targetQualityClause.Id)))
                {
                    db.JobRefToQualityClauseRefs[clause.Key].Remove(targetQualityClause.Id);
                }

                var args = new Dictionary<string, string>();
                args["QualityClause"] = ClauseId.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "DeleteQualityClauses",
                    Args = args,
                    When = DateTime.Now,
                });
            }
            else
            {
                throw new Exception("This Quality Clause doesn't exist within the database");
            }
        }

        public void DeleteQualityClauseFromJob(Guid ClauseId, string Job)
        {
            if (db.JobRefToQualityClauseRefs[Job].Contains(ClauseId)) {
                db.JobRefToQualityClauseRefs[Job].Remove(ClauseId);
                var args = new Dictionary<string, string>();
                args["QualityClause"] = ClauseId.ToString();
                args["Job"] = Job;
                db.AuditLog.Add(new Event
                {
                    Action = "DeleteQualityClauseFromJob",
                    Args = args,
                    When = DateTime.Now,
                });
            } else
            {
                throw new Exception("This Quality Clause doesn't exist within this Job");
            }
        }

        public void SplitQualityClause(Guid sourceGroupId, Guid sourceClauseId)
        {
            if (db.QualityClauses.ContainsKey(sourceGroupId))
            {
                db.QualityClauses[sourceGroupId].Add(db.QualityClauses[sourceGroupId].First(y => y.Id == sourceClauseId));

                var args = new Dictionary<string, string>();
                args["SourceGroupId"] = sourceGroupId.ToString();
                args["SourceClauseId"] = sourceClauseId.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "SplitQualityClauses",
                    Args = args,
                    When = DateTime.Now,
                });
            }
            else
            {
                throw new Exception("This group id doesn't exist in the database");
            }
        }

        public void CloneQualityClauses(string sourceJobId, string targetJobId, bool overwrite)
        {
            if (db.Jobs.ContainsKey(sourceJobId) && db.Jobs.ContainsKey(targetJobId))
            {
                if(overwrite)
                {
                    db.JobRefToQualityClauseRefs[targetJobId] = db.JobRefToQualityClauseRefs[sourceJobId];
                }
                else
                {
                    db.JobRefToQualityClauseRefs[targetJobId] =
                        db.JobRefToQualityClauseRefs[sourceJobId].Union(db.JobRefToQualityClauseRefs[targetJobId]).ToList();

                }

                var args = new Dictionary<string, string>();
                args["SourceJobId"] = sourceJobId;
                args["TargetJobId"] = targetJobId;
                args["Overwrite"] = overwrite.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "CloneQualityClauses",
                    Args = args,
                    When = DateTime.Now,
                });
            }
            else
            {
                throw new Exception("One(or both) of the jobs doesn't exist in the database");
            }
        }

        public List<WorkInstruction> DisplayPriorRevisionsOfWorkInstruction(WorkInstruction workInstruction)
        {
            return db.WorkInstructions[workInstruction.IdRevGroup];
        }

        public List<QualityClause> DisplayPriorRevisionsOfQualityClauses(QualityClause qualityClause)
        {
            return db.QualityClauses[qualityClause.IdRevGroup];
        }

        public List<OpSpec> DisplayPriorRevisionsOfSpecs(OpSpec opSpec)
        {
            return db.OpSpecs[opSpec.IdRevGroup];
        }

        public WorkInstruction DisplayLatestRevisionOfWorkInstruction(string jobId, string jobRev, int opId)
        {
            if (db.Jobs.ContainsKey(jobId))
            {
                Job job = db.Jobs[jobId];
                WorkInstruction latestRevision = new WorkInstruction();
                foreach (List<WorkInstruction> list in db.WorkInstructions.Values)
                {
                    foreach (WorkInstruction workInstruction in list)
                    {
                        if (workInstruction.Approved && job.RevCustomer == jobRev 
                            && workInstruction.OpId == opId && job.Ops.FindIndex(y => y.JobId == jobId) >= 0)
                        {
                            latestRevision = workInstruction;
                        }
                    }
                }
                return latestRevision;
            }
            else
            {
                throw new Exception("The job doesn't exist in the database");
            }
        }

    }
}
