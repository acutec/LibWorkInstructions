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
                db.JobRefToWorkInstructionRefs.Add(newJob.Id, new List<List<Guid>>());
                db.JobRefToQualityClauseRefs.Add(newJob.Id, new List<Guid>());

                var args = new Dictionary<string, string>();
                args["NewJob"] = newJob.Id;
                db.AuditLog.Add(new Event
                {
                    Action = "AddJob",
                    Args = args,
                    NewData = JsonSerializer.Serialize(newJob),
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

        public WorkInstruction AddWorkInstruction(string jobId)
        {
            List<Guid> newOpList = new List<Guid>();
            Guid groupId = Guid.NewGuid();
            foreach (Op op in db.Jobs[jobId].Ops)
            {
                newOpList.Add(op.Id);
            }
            WorkInstruction newWorkInstruction = new WorkInstruction
            {
                Id = Guid.NewGuid(),
                IdRevGroup = groupId,
                OpSpecs = new List<Guid>(),
                Ops = newOpList
            };
            db.WorkInstructions[groupId] = new List<WorkInstruction>();
            db.WorkInstructions[groupId].Add(newWorkInstruction);
            db.JobRefToWorkInstructionRefs[jobId].Add(newWorkInstruction.Ops);

            var args = new Dictionary<string, string>();
            args["JobId"] = jobId;
            db.AuditLog.Add(new Event
            {
                Action = "AddWorkInstruction",
                Args = args,
                NewData = JsonSerializer.Serialize(newWorkInstruction),
                When = DateTime.Now,
            }) ;

            return newWorkInstruction;
         }
        public void ChangeWorkInstruction(Guid targetGroupId)
        {
            if (db.WorkInstructions.ContainsKey(targetGroupId))
            {
                db.WorkInstructions[targetGroupId].Add(new WorkInstruction
                {
                    Id = Guid.NewGuid(),
                    IdRevGroup = targetGroupId
                });

                var args = new Dictionary<string, string>();
                args["targetGroupId"] = targetGroupId.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "ChangeWorkInstruction",
                    Args = args,
                    NewData = JsonSerializer.Serialize(db.WorkInstructions[targetGroupId].Last()),
                    When = DateTime.Now,
                });
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
                db.WorkInstructions[targetGroupId].Remove(targetWorkInstruction);
                foreach (var kvp in db.JobRefToWorkInstructionRefs.Where(y => y.Value.Contains(targetWorkInstruction.Ops)))
                {
                    db.JobRefToWorkInstructionRefs.Remove(kvp.Key);
                }

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
        public void MergeWorkInstructions(Guid groupId1, Guid workId1, Guid groupId2, Guid workId2)
        {
            if (db.WorkInstructions.ContainsKey(groupId1) && db.WorkInstructions.ContainsKey(groupId2))
            {
                WorkInstruction workInstruction1 = new WorkInstruction();
                WorkInstruction workInstruction2 = new WorkInstruction();
                List<Guid> mergedInstructionOps = new List<Guid>();
                foreach (List<WorkInstruction> workInstructionGroup in db.WorkInstructions.Values)
                {
                    foreach (WorkInstruction workInstruction in workInstructionGroup)
                    {
                        if (workInstruction.Id == workId1)
                        {
                            workInstruction1 = workInstruction;
                        }
                        if (workInstruction.Id == workId2)
                        {
                            workInstruction2 = workInstruction;
                        }
                    }
                }
                string job1 = db.JobRefToWorkInstructionRefs.First(y => y.Value.FindIndex( y => y.SequenceEqual(workInstruction1.Ops)) >= 0).Key;
                string job2 = db.JobRefToWorkInstructionRefs.First(y => y.Value.FindIndex( y => y.SequenceEqual(workInstruction2.Ops)) >= 0).Key;
                mergedInstructionOps = workInstruction1.Ops.Union(workInstruction2.Ops).ToList();
                if (job1 != job2)
                {
                    db.JobRefToWorkInstructionRefs[job1].Add(mergedInstructionOps);
                }
                db.JobRefToWorkInstructionRefs[job2].Add(mergedInstructionOps);
                db.JobRefToWorkInstructionRefs[job1].RemoveAt(db.JobRefToWorkInstructionRefs[job1].FindIndex(y => y.SequenceEqual(workInstruction1.Ops)));
                db.JobRefToWorkInstructionRefs[job2].RemoveAt(db.JobRefToWorkInstructionRefs[job2].FindIndex(y => y.SequenceEqual(workInstruction2.Ops)));

                var args = new Dictionary<string, string>();
                args["GroupId1"] = groupId1.ToString();
                args["WorkId1"] = workId1.ToString();
                args["GroupId2"] = groupId2.ToString();
                args["WorkId2"] = workId2.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "MergeWorkInstructions",
                    Args = args,
                    When = DateTime.Now,
                });
            }
            else
            {
                throw new Exception("One(or both) of the work instructions doesn't exist in the database");
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
                string targetJobId = db.JobRefToWorkInstructionRefs.First(y => y.Value.FindIndex(y => y.SequenceEqual(duplicate.Ops)) >= 0).Key;
                db.JobRefToWorkInstructionRefs[targetJobId].Add(duplicate.Ops);

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

        public void CloneWorkInstruction(Guid sourceGroupId, Guid sourceWorkId, string targetJobId)
        {
            if (db.WorkInstructions.ContainsKey(sourceGroupId) && db.Jobs.ContainsKey(targetJobId))
            {
                WorkInstruction duplicate = new WorkInstruction();
                foreach (List<WorkInstruction> workInstructionGroup in db.WorkInstructions.Values)
                {
                    foreach (WorkInstruction workInstruction in workInstructionGroup)
                    {
                        if (workInstruction.Id == sourceWorkId)
                        {
                            duplicate = workInstruction;
                        }
                    }
                }
                db.JobRefToWorkInstructionRefs[targetJobId].Add(duplicate.Ops);

                var args = new Dictionary<string, string>();
                args["SourceGroupId"] = sourceGroupId.ToString();
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
                if (!db.WorkInstructions.ContainsKey(sourceGroupId))
                {
                    throw new Exception("This group id doesn't exist in the database");
                }
                else
                {
                    throw new Exception("The job doesn't exist in the database");
                }
            }
        }

        public OpSpec AddSpec(Guid targetGroupId, Guid targetWorkId)
        {
            WorkInstruction targetWorkInstruction = db.WorkInstructions[targetGroupId].First(y => y.Id == targetWorkId);
            OpSpec opSpec = new OpSpec
            {
                Id = Guid.NewGuid(),
                IdRevGroup = targetGroupId
            };
            db.OpSpecs.Add(opSpec.Id, new List<OpSpec> { opSpec });

            var args = new Dictionary<string, string>();
            args["TargetGroupId"] = targetGroupId.ToString();
            args["TargetWorkId"] = targetWorkId.ToString();
            db.AuditLog.Add(new Event
            {
                Action = "AddSpec",
                Args = args,
                NewData = JsonSerializer.Serialize(opSpec),
                When = DateTime.Now,
            });
            return opSpec;
        }

        public void ChangeSpec(Guid targetGroupId)
        {
            if (db.OpSpecs.ContainsKey(targetGroupId))
            {
                db.OpSpecs[targetGroupId].Add(new OpSpec
                {
                    Id = Guid.NewGuid(),
                    IdRevGroup = targetGroupId
                });

                var args = new Dictionary<string, string>();
                args["TargetGroupId"] = targetGroupId.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "ChangeSpec",
                    Args = args,
                    NewData = JsonSerializer.Serialize(db.OpSpecs[targetGroupId].Last()),
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

        public void MergeSpecs(Guid sourceGroupId, Guid sourceWorkId, Guid targetGroupId, Guid targetWorkId)
        {
            if (db.WorkInstructions.ContainsKey(sourceGroupId) && db.WorkInstructions.ContainsKey(targetGroupId))
            {
                WorkInstruction workInstruction1 = db.WorkInstructions[sourceGroupId].First(y => y.Id == sourceWorkId);
                WorkInstruction workInstruction2 = db.WorkInstructions[targetGroupId].First(y => y.Id == targetWorkId);
                workInstruction1.OpSpecs = workInstruction1.OpSpecs.Union(workInstruction2.OpSpecs).ToList();
                workInstruction2.OpSpecs = workInstruction1.OpSpecs;

                var args = new Dictionary<string, string>();
                args["SourceGroupId"] = sourceGroupId.ToString();
                args["SourceWorkId"] = sourceWorkId.ToString();
                args["TargetGroupId"] = targetGroupId.ToString();
                args["TargetWorkId"] = targetWorkId.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "MergeSpecs",
                    Args = args,
                    When = DateTime.Now,
                });
            }
            else
            {
                throw new Exception("One(or both) of the specs doesn't exist in the database");
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

        public void CloneSpecs(Guid sourceGroupId, Guid sourceWorkId, Guid targetGroupId, Guid targetWorkId, bool overwrite)
        {
            if (db.WorkInstructions.ContainsKey(sourceGroupId) && db.WorkInstructions.ContainsKey(targetGroupId))
            {
                WorkInstruction sourceWorkInstruction = db.WorkInstructions[sourceGroupId].First(y => y.Id == sourceWorkId);
                WorkInstruction targetWorkInstruction = db.WorkInstructions[targetGroupId].First(y => y.Id == targetWorkId);
                if (overwrite) 
                { 
                    targetWorkInstruction.OpSpecs = sourceWorkInstruction.OpSpecs; 
                }
                else
                {
                    targetWorkInstruction.OpSpecs = targetWorkInstruction.OpSpecs.Union(sourceWorkInstruction.OpSpecs).ToList();
                }

                var args = new Dictionary<string, string>();
                args["SourceGroupId"] = sourceGroupId.ToString();
                args["SourceWorkId"] = sourceWorkId.ToString();
                args["TargetGroupId"] = targetWorkId.ToString();
                args["TargetWorkId"] = targetWorkId.ToString();
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
                throw new Exception("One(or both) of the group ids doesn't exist in the database");
            }
        }

        public QualityClause AddQualityClause()
        {
            QualityClause newQualityClause = new QualityClause
            {
                Id = Guid.NewGuid(),
                IdRevGroup = Guid.NewGuid()
            };
            db.QualityClauses.Add(newQualityClause.IdRevGroup, new List<QualityClause> { newQualityClause });

            db.AuditLog.Add(new Event
            {
                Action = "CreateQualityClause",
                NewData = JsonSerializer.Serialize(newQualityClause),
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
                    db.JobRefToQualityClauseRefs.Remove(clause.Key);
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

        public WorkInstruction DisplayLatestRevisionOfWorkInstruction(string jobRev)
        {
            Job job = db.Jobs.Values.First(y => y.Rev == jobRev);
            List<Guid> ops = db.JobRefToWorkInstructionRefs[job.Id].Last();
            WorkInstruction latestRevision = new WorkInstruction();

            foreach (List<WorkInstruction> list in db.WorkInstructions.Values)
            {
                foreach (WorkInstruction workInstruction in list)
                {
                    if(workInstruction.Ops == ops)
                    {
                        latestRevision = workInstruction;
                    }
                }
            }

            return latestRevision;
        }

    }
}
