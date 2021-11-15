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
            public Dictionary<string, List<Job>> Jobs = new Dictionary<string, List<Job>>();
            public Dictionary<Guid, List<OpSpec>> OpSpecs = new Dictionary<Guid, List<OpSpec>>();
            public Dictionary<Guid, List<QualityClause>> QualityClauses = new Dictionary<Guid, List<QualityClause>>();
            public Dictionary<int, List<Op>> Ops = new Dictionary<int, List<Op>>();
            public Dictionary<Guid, List<WorkInstruction>> WorkInstructions = new Dictionary<Guid, List<WorkInstruction>>();

            public List<string> JobRevs = new List<string>();
            public List<Guid> QualityClauseRevs = new List<Guid>();
            public List<Guid> OpSpecRevs = new List<Guid>();
            public List<Guid> WorkInstructionRevs = new List<Guid>();

            public Dictionary<string, List<Guid>> JobRefToQualityClauseRefs = new Dictionary<string, List<Guid>>();
            public Dictionary<string, List<string>> JobRefToJobRevRefs = new Dictionary<string, List<string>>();
            public Dictionary<string, List<Guid>> JobRevRefToQualityClauseRevRefs = new Dictionary<string, List<Guid>>();
            public Dictionary<string, List<int>> JobRevRefToOpRefs = new Dictionary<string, List<int>>();
            public Dictionary<Guid, List<int>> OpSpecRevRefToOpRefs = new Dictionary<Guid, List<int>>();
            public Dictionary<int, List<Guid>> OpRefToOpSpecRevRefs = new Dictionary<int, List<Guid>>();
            public Dictionary<Guid, List<Guid>> OpSpecRefToOpSpecRevRefs = new Dictionary<Guid, List<Guid>>();
            public Dictionary<Guid, List<string>> QualityClauseRevRefToJobRevRefs = new Dictionary<Guid, List<string>>();
            public Dictionary<Guid, List<Guid>> QualityClauseRefToQualityClauseRevRefs = new Dictionary<Guid, List<Guid>>();
            public Dictionary<int, Guid> OpRefToWorkInstructionRef = new Dictionary<int, Guid>();
            public Dictionary<Guid, List<Guid>> WorkInstructionRefToWorkInstructionRevRefs = new Dictionary<Guid, List<Guid>>();

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

        public Job GetJob(string jobId, string customerRev, string internalRev) =>
                db.Jobs[jobId + "-" + customerRev].First(y => y.RevPlan == internalRev);
        public WorkInstruction GetWorkInstruction(Guid groupId, Guid instructionId) =>
                db.WorkInstructions.First(y => y.Key == groupId).Value.First(y => y.Id == instructionId);

        public void CreateJob(Job newJob)
        {
            if (!db.Jobs.ContainsKey(newJob.Id + "-" + newJob.RevCustomer))
            {
                newJob.RevSeq = 0;
                db.Jobs.Add(newJob.Id + "-" + newJob.RevCustomer, new List<Job> { newJob });
                db.JobRefToJobRevRefs[newJob.Id + "-" + newJob.RevCustomer] = new List<string> { newJob.RevPlan };

                var args = new Dictionary<string, string>();
                args["NewJob"] = JsonSerializer.Serialize(newJob);
                db.AuditLog.Add(new Event
                {
                    Action = "CreateJob",
                    Args = args,
                    When = DateTime.Now,
                });
            }
            else
            {
                throw new Exception("Job already exists in the database");
            }
        }

        public void UpdateJob(Job newJob)
        {
            if (db.Jobs.ContainsKey(newJob.Id + "-" + newJob.RevCustomer))
            {
                if (!db.Jobs[newJob.Id + "-" + newJob.RevCustomer].Any(y => y.RevPlan == newJob.RevPlan))
                {
                    newJob.RevSeq = db.Jobs[newJob.Id + "-" + newJob.RevCustomer].Count;
                    db.JobRefToJobRevRefs.Keys.Where(y => y == db.Jobs[newJob.Id + "-" + newJob.RevCustomer].Last().Id).Select(y => y = newJob.Id);
                    db.Jobs[newJob.Id + "-" + newJob.RevCustomer].Add(newJob);

                    var args = new Dictionary<string, string>();
                    args["NewJob"] = JsonSerializer.Serialize(newJob);
                    db.AuditLog.Add(new Event
                    {
                        Action = "UpdateJob",
                        Args = args,
                        When = DateTime.Now,
                    });
                }
                else
                {
                    throw new Exception("A job with the same revision identifier already exists");
                }
            }
            else
            {
                throw new Exception("Job doesn't exist in the database");
            }
        }

        public void DeleteJob(Job job)
        {
            if (db.Jobs.ContainsKey(job.Id + "-" + job.RevCustomer))
            {
                db.Jobs.Remove(job.Id + "-" + job.RevCustomer);
                db.JobRefToJobRevRefs.Remove(job.Id + "-" + job.RevCustomer);

                var args = new Dictionary<string, string>();
                args["Job"] = JsonSerializer.Serialize(job);
                db.AuditLog.Add(new Event
                {
                    Action = "RemoveJob",
                    Args = args,
                    When = DateTime.Now,
                });
            }
            else
            {
                throw new Exception("Job doesn't exist in the database");
            }
        }
        public void CreateJobRev(string jobRev)
        {
            if (!db.JobRevs.Contains(jobRev))
            {
                db.JobRevs.Add(jobRev);
                db.JobRevRefToQualityClauseRevRefs[jobRev] = new List<Guid>();
                db.JobRevRefToOpRefs[jobRev] = new List<int>();

                var args = new Dictionary<string, string>();
                args["JobRev"] = jobRev;
                db.AuditLog.Add(new Event
                {
                    Action = "CreateJobRev",
                    Args = args,
                    When = DateTime.Now
                });
            }
            else
            {
                throw new Exception("Job revision already exists in the database");
            }
        }

        public void UpdateJobRev(string targetRev, string sourceRev)
        {
            if (db.JobRevs.Contains(targetRev))
            {
                int objIndex = db.JobRevs.FindIndex(y => y == targetRev);
                if (objIndex != -1)
                {
                    db.JobRevs[objIndex] = sourceRev;
                }
                db.JobRefToJobRevRefs.Values.Select(y => y = y.Where(y => y == targetRev).Select(y => y = sourceRev).ToList());
                db.QualityClauseRevRefToJobRevRefs.Values.Select(y => y = y.Where(y => y == targetRev).Select(y => y = sourceRev).ToList());
                db.JobRevRefToOpRefs.Keys.Where(y => y == targetRev).Select(y => y = sourceRev);
                db.JobRevRefToQualityClauseRevRefs.Keys.Where(y => y == targetRev).Select(y => y = sourceRev);

                var args = new Dictionary<string, string>();
                args["TargetRev"] = targetRev;
                args["SourceRev"] = sourceRev;
                db.AuditLog.Add(new Event
                {
                    Action = "UpdateJobRev",
                    Args = args,
                    When = DateTime.Now
                });
            }
            else
            {
                throw new Exception("The target job revision doesn't exist in the database");
            }
        }

        public void DeleteJobRev(string jobRev)
        {
            if (db.JobRevs.Contains(jobRev))
            {
                db.JobRevs.Remove(jobRev);
                db.JobRefToJobRevRefs.Values.Select(y => y = y.Where(y => y != jobRev).ToList());
                db.QualityClauseRevRefToJobRevRefs.Values.Select(y => y = y.Where(y => y != jobRev).ToList());
                db.JobRevRefToQualityClauseRevRefs.Remove(jobRev);
                db.JobRevRefToOpRefs.Remove(jobRev);

                var args = new Dictionary<string, string>();
                args["JobRev"] = jobRev;
                db.AuditLog.Add(new Event
                {
                    Action = "DeleteJobRev",
                    Args = args,
                    When = DateTime.Now
                });
            }
            else
            {
                throw new Exception("The job revision doesn't exist in the database");
            }
        }

        public void CreateQualityClauseRev(Guid qualityClauseRev)
        {
            if (!db.QualityClauseRevs.Contains(qualityClauseRev))
            {
                db.QualityClauseRevs.Add(qualityClauseRev);
                db.QualityClauseRevRefToJobRevRefs[qualityClauseRev] = new List<string>();

                var args = new Dictionary<string, string>();
                args["QualityClauseRev"] = qualityClauseRev.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "CreateQualityClauseRev",
                    Args = args,
                    When = DateTime.Now
                });
            }
            else
            {
                throw new Exception("The quality clause is already in the database");
            }
        }

        public void UpdateQualityClauseRev(Guid targetRev, Guid sourceRev)
        {
            if (db.QualityClauseRevs.Contains(targetRev))
            {
                int objIndex = db.QualityClauseRevs.FindIndex(y => y == targetRev);
                if (objIndex != -1)
                {
                    db.QualityClauseRevs[objIndex] = sourceRev;
                }
                db.JobRevRefToQualityClauseRevRefs.Values.Select(y => y = y.Where(y => y == targetRev).Select(y => y = sourceRev).ToList());
                db.QualityClauseRefToQualityClauseRevRefs.Values.Select(y => y = y.Where(y => y == targetRev).Select(y => y = sourceRev).ToList());
                db.QualityClauses.Values.Select(y => y = y.Where(y => y.Rev == targetRev).Select(y => { y.Rev = sourceRev; return y; }).ToList());
                db.QualityClauseRevRefToJobRevRefs.Keys.Where(y => y == targetRev).Select(y => y = sourceRev);

                var args = new Dictionary<string, string>();
                args["TargetRev"] = targetRev.ToString();
                args["SourceRev"] = sourceRev.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "UpdateQualityClauseRev",
                    Args = args,
                    When = DateTime.Now
                });
            }
            else
            {
                throw new Exception("The target revision doesn't exist in the database");
            }
        }

        public void DeleteQualityClauseRev(Guid qualityClauseRev)
        {
            if (db.QualityClauseRevs.Contains(qualityClauseRev))
            {
                db.QualityClauseRevs.Remove(qualityClauseRev);
                db.JobRevRefToQualityClauseRevRefs.Values.Select(y => y = y.Where(y => y != qualityClauseRev).ToList());
                db.QualityClauseRefToQualityClauseRevRefs.Values.Select(y => y = y.Where(y => y != qualityClauseRev).ToList());
                db.QualityClauseRevRefToJobRevRefs.Remove(qualityClauseRev);
                db.QualityClauses.Values.Select(y => y = y.Where(y => y.Rev != qualityClauseRev).ToList());

                var args = new Dictionary<string, string>();
                args["QualityClauseRev"] = qualityClauseRev.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "DeleteQualityClauseRev",
                    Args = args,
                    When = DateTime.Now
                });
            }
            else
            {
                throw new Exception("The quality clause revision doesn't exist in the database");
            }
        }

        public void CreateQualityClause(QualityClause newQualityClause)
        {
            if (!db.QualityClauses.ContainsKey(newQualityClause.Id))
            {
                newQualityClause.RevSeq = 0;
                db.QualityClauses.Add(newQualityClause.Id, new List<QualityClause> { newQualityClause });
                db.QualityClauseRefToQualityClauseRevRefs[newQualityClause.Id] = new List<Guid> { newQualityClause.Rev };

                var args = new Dictionary<string, string>();
                args["NewQualityClause"] = JsonSerializer.Serialize(newQualityClause);
                db.AuditLog.Add(new Event
                {
                    Action = "CreateQualityClause",
                    Args = args,
                    When = DateTime.Now,
                });
            }
            else
            {
                throw new Exception("The quality clause already exists in the database");
            }
        }

        public void UpdateQualityClause(QualityClause qualityClause)
        {
            if (db.QualityClauses.ContainsKey(qualityClause.Id))
            {
                if (!db.QualityClauses[qualityClause.Id].Any(y => y.Rev == qualityClause.Rev))
                {
                    qualityClause.RevSeq = db.QualityClauses[qualityClause.Id].Count;
                    db.QualityClauses[qualityClause.Id].Add(qualityClause);
                    db.QualityClauseRefToQualityClauseRevRefs[qualityClause.Id].Add(qualityClause.Rev);

                    var args = new Dictionary<string, string>();
                    args["QualityClause"] = JsonSerializer.Serialize(qualityClause);
                    db.AuditLog.Add(new Event
                    {
                        Action = "UpdateQualityClause",
                        Args = args,
                        When = DateTime.Now
                    });
                }
                else
                {
                    throw new Exception("A quality clause with the same revision identifier already exists");
                }
            }
            else
            {
                throw new Exception("The quality clause doesn't exist in the database");
            }
        }

        public void DeleteQualityClause(Guid clauseId)
        {
            if (db.QualityClauses.ContainsKey(clauseId))
            {
                db.QualityClauses.Remove(clauseId);
                db.QualityClauseRefToQualityClauseRevRefs.Remove(clauseId);

                var args = new Dictionary<string, string>();
                args["ClauseId"] = clauseId.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "DeleteQualityClause",
                    Args = args,
                    When = DateTime.Now
                });
            }
            else
            {
                throw new Exception("The quality clause doesn't exist in the database");
            }
        }

        public void CreateJobOp(Op op)
        {
            if (!db.Ops.ContainsKey(op.Id))
            {
                db.Ops.Add(op.Id, new List<Op> { op });
                db.OpRefToOpSpecRevRefs[op.Id] = new List<Guid> { op.Rev };

                var args = new Dictionary<string, string>();
                args["Op"] = JsonSerializer.Serialize(op);
                db.AuditLog.Add(new Event
                {
                    Action = "CreateJobOp",
                    Args = args,
                    When = DateTime.Now
                });
            }
            else
            {
                throw new Exception("The operation already exists in the database");
            }
        }

        public void UpdateJobOp(Op op)
        {
            if (db.Ops.ContainsKey(op.Id))
            {
                if (!db.Ops[op.Id].Any(y => y.Rev == op.Rev))
                {
                    op.RevSeq = db.Ops[op.Id].Count;
                    db.Ops[op.Id].Add(op);

                    var args = new Dictionary<string, string>();
                    args["Op"] = JsonSerializer.Serialize(op);
                    db.AuditLog.Add(new Event
                    {
                        Action = "UpdateJobOp",
                        Args = args,
                        When = DateTime.Now
                    });
                }
                else
                {
                    throw new Exception("An operation with the same revision identifier already exists");
                }
            }
            else
            {
                throw new Exception("The operation being changed doesn't exist in the database");
            }
        }

        public void DeleteJobOp(int opId)
        {
            if (db.Ops.ContainsKey(opId))
            {
                db.Ops.Remove(opId);
                db.OpRefToOpSpecRevRefs.Remove(opId);
                db.OpRefToWorkInstructionRef.Remove(opId);
                db.JobRevRefToOpRefs.Values.Select(y => y = y.Where(y => y != opId).ToList());
                db.OpSpecRevRefToOpRefs.Values.Select(y => y = y.Where(y => y != opId).ToList());

                var args = new Dictionary<string, string>();
                args["OpId"] = opId.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "DeleteJobOp",
                    Args = args,
                    When = DateTime.Now
                });
            }
            else
            {
                throw new Exception("Job op doesn't exist in the database");
            }
        }

        public void CreateOpSpec(OpSpec newSpec)
        {
            db.OpSpecs[newSpec.IdRevGroup] = new List<OpSpec> { newSpec };
            db.OpSpecRefToOpSpecRevRefs[newSpec.Id] = new List<Guid>();

            var args = new Dictionary<string, string>();
            args["newSpec"] = JsonSerializer.Serialize(newSpec);
            db.AuditLog.Add(new Event
            {
                Action = "CreateOpSpec",
                Args = args,
                When = DateTime.Now,
            });
        }

        public void UpdateOpSpec(OpSpec newSpec)
        {
            if (db.OpSpecs.ContainsKey(newSpec.IdRevGroup))
            {
                newSpec.RevSeq = db.OpSpecs[newSpec.IdRevGroup].Count;
                db.OpSpecRefToOpSpecRevRefs.Keys.Where(y => y == db.OpSpecs[newSpec.IdRevGroup].Last().Id).Select(y => y = newSpec.Id);
                db.OpSpecs[newSpec.IdRevGroup].Add(newSpec);

                var args = new Dictionary<string, string>();
                args["newSpec"] = JsonSerializer.Serialize(newSpec);
                db.AuditLog.Add(new Event
                {
                    Action = "UpdateOpSpec",
                    Args = args,
                    When = DateTime.Now,
                });
            }
            else
            {
                throw new Exception("The spec being updated doesn't exist in the database");
            }
        }

        public void DeleteOpSpec(Guid targetGroupId, Guid targetSpecId)
        {
            if (db.OpSpecs.ContainsKey(targetGroupId))
            {
                db.OpSpecs[targetGroupId].Remove(db.OpSpecs[targetGroupId].First(y => y.Id == targetSpecId));
                db.OpSpecRefToOpSpecRevRefs.Remove(targetSpecId);

                var args = new Dictionary<string, string>();
                args["TargetGroupId"] = targetGroupId.ToString();
                args["TargetSpecId"] = targetSpecId.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "DeleteOpSpec",
                    Args = args,
                    When = DateTime.Now,
                });
            }
            else
            {
                throw new Exception("The spec doesn't exist in the database");
            }
        }

        public void CreateOpSpecRev(Guid specRev)
        {
            if (!db.OpSpecRevs.Contains(specRev))
            {
                db.OpSpecRevs.Add(specRev);
                db.OpSpecRevRefToOpRefs[specRev] = new List<int>();

                var args = new Dictionary<string, string>();
                args["specRev"] = specRev.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "CreateOpSpecRev",
                    Args = args,
                    When = DateTime.Now
                });
            }
            else
            {
                throw new Exception("OpSpec Revision already exists in the database");
            }
        }

        public void UpdateOpSpecRev(Guid targetSpecRev, Guid sourceSpecRev)
        {
            if (db.OpSpecRevs.Contains(targetSpecRev))
            {
                db.OpSpecRevs[db.OpSpecRevs.FindIndex(y => y == targetSpecRev)] = sourceSpecRev;
                db.OpSpecRevRefToOpRefs.Keys.ToList()[db.OpSpecRevRefToOpRefs.Keys.ToList().FindIndex(y => y == targetSpecRev)] = sourceSpecRev;
                db.OpRefToOpSpecRevRefs.Values.Where(y => y.Contains(targetSpecRev)).Select(y => y.Where(y => y == targetSpecRev).Select(y => y = sourceSpecRev));
                db.OpSpecRefToOpSpecRevRefs.Values.Where(y => y.Contains(targetSpecRev)).Select(y => y.Where(y => y == targetSpecRev).Select(y => y = sourceSpecRev));

                var args = new Dictionary<string, string>();
                args["TargetSpecRev"] = targetSpecRev.ToString();
                args["SourceSpecRev"] = sourceSpecRev.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "UpdateOpSpecRev",
                    Args = args,
                    When = DateTime.Now
                });
            }
            else
            {
                throw new Exception("OpSpec Revision already exists in the datbase");
            }
        }

        public void DeleteOpSpecRev(Guid specRev)
        {
            if (db.OpSpecRevs.Contains(specRev))
            {
                db.OpSpecRevs.Remove(specRev);
                db.OpSpecRevRefToOpRefs.Remove(specRev);
                db.OpRefToOpSpecRevRefs.Values.Select(y => y = y.Where(y => y != specRev).ToList());
                db.OpSpecRefToOpSpecRevRefs.Values.Select(y => y = y.Where(y => y != specRev).ToList());

                var args = new Dictionary<string, string>();
                args["SpecRev"] = specRev.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "DeleteOpSpecRev",
                    Args = args,
                    When = DateTime.Now
                });
            }
            else
            {
                throw new Exception("The op spec revision doesn't exist in the database");
            }
        }

        public void CreateWorkInstruction(WorkInstruction workInstruction)
        {
            if (!db.WorkInstructions.ContainsKey(workInstruction.IdRevGroup))
            {
                workInstruction.RevSeq = 0;
                db.WorkInstructions[workInstruction.IdRevGroup] = new List<WorkInstruction> { workInstruction };
                db.OpRefToWorkInstructionRef[workInstruction.OpId] = workInstruction.Id;
                db.WorkInstructionRefToWorkInstructionRevRefs[workInstruction.Id] =  new List<Guid> { workInstruction.Id };

                var args = new Dictionary<string, string>();
                args["WorkInstruction"] = JsonSerializer.Serialize(workInstruction);
                db.AuditLog.Add(new Event
                {
                    Action = "CreateWorkInstruction",
                    Args = args,
                    When = DateTime.Now,
                });
            }
            else
            {
                throw new Exception("Work instruction already exists in the database.");
            }
        }

        public void UpdateWorkInstruction(WorkInstruction newWorkInstruction)
        {
            if (db.WorkInstructions.ContainsKey(newWorkInstruction.IdRevGroup))
            {
                if (!db.WorkInstructions[newWorkInstruction.IdRevGroup].Any(y => y.Equals(newWorkInstruction)))
                {
                    newWorkInstruction.RevSeq = db.WorkInstructions[newWorkInstruction.IdRevGroup].Count;
                    db.WorkInstructionRefToWorkInstructionRevRefs.Keys.Where(y => y == db.WorkInstructions[newWorkInstruction.IdRevGroup].Last().Id).Select(y => y = newWorkInstruction.Id);
                    db.WorkInstructions[newWorkInstruction.IdRevGroup].Add(newWorkInstruction);
                    db.OpRefToWorkInstructionRef[newWorkInstruction.OpId] = newWorkInstruction.Id;

                    var args = new Dictionary<string, string>();
                    args["newWorkInstruction"] = JsonSerializer.Serialize(newWorkInstruction);
                    db.AuditLog.Add(new Event
                    {
                        Action = "UpdateWorkInstruction",
                        Args = args,
                        When = DateTime.Now,
                    });
                }
                else
                    throw new Exception("The new work instruction already exists in the database");
            }
            else
            {
                throw new Exception("This old work instruction doesn't exist in the database");
            }
        }

        public void DeleteWorkInstruction(Guid targetGroupId, Guid targetWorkId)
        {
            if (db.WorkInstructions.ContainsKey(targetGroupId))
            {
                WorkInstruction targetWorkInstruction = db.WorkInstructions[targetGroupId].First(y => y.Id == targetWorkId);
                db.WorkInstructions[targetGroupId].Remove(targetWorkInstruction);
                db.OpRefToWorkInstructionRef.Remove(targetWorkInstruction.OpId);
                db.WorkInstructionRefToWorkInstructionRevRefs.Values.Where(y => y.Contains(targetWorkInstruction.Id)).Select(y => y = y.Where(y => y != targetWorkInstruction.Id).ToList());

                var args = new Dictionary<string, string>();
                args["TargetGroupId"] = targetGroupId.ToString();
                args["TargetWorkId"] = targetWorkId.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "DeleteWorkInstruction",
                    Args = args,
                    When = DateTime.Now,
                });
            }
            else
            {
                throw new Exception("This work instruction group doesn't exist in the database");
            }
        }

        public void CreateWorkInstructionRev(Guid workInstructionRev)
        {
            if (!db.WorkInstructionRevs.Contains(workInstructionRev))
            {
                db.WorkInstructionRevs.Add(workInstructionRev);

                var args = new Dictionary<string, string>();
                args["WorkInstructionRev"] = workInstructionRev.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "CreateWorkInstructionRev",
                    Args = args,
                    When = DateTime.Now
                });
            }
            else
            {
                throw new Exception("The work instruction revision already exists in the database.");
            }
        }

        public void UpdateWorkInstructionRev(Guid targetWorkInstructionRev, Guid sourceWorkInstructionRev)
        {
            if (db.WorkInstructionRevs.Contains(targetWorkInstructionRev))
            {
                db.WorkInstructionRevs[db.WorkInstructionRevs.FindIndex(y => y == targetWorkInstructionRev)] = sourceWorkInstructionRev;
                db.WorkInstructionRefToWorkInstructionRevRefs.Values.Where(y => y.Contains(targetWorkInstructionRev)).Select(y => y.Where(y => y == targetWorkInstructionRev).Select(y => y = sourceWorkInstructionRev));

                var args = new Dictionary<string, string>();
                args["TargetWorkInstructionRev"] = targetWorkInstructionRev.ToString();
                args["SourceWorkInstructionRev"] = sourceWorkInstructionRev.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "UpdateWorkInstructionRev",
                    Args = args,
                    When = DateTime.Now
                });
            }
            else
            {
                throw new Exception("The work instruction revision doesn't exist in the database.");
            }
        }

        public void DeleteWorkInstructionRev(Guid targetWorkInstructionRev)
        {
            if (db.WorkInstructionRevs.Contains(targetWorkInstructionRev))
            {
                db.WorkInstructionRevs.Remove(targetWorkInstructionRev);
                db.WorkInstructionRefToWorkInstructionRevRefs.Values.Select(y => y = y.Where(y => y != targetWorkInstructionRev).ToList());

                var args = new Dictionary<string, string>();
                args["TargetWorkInstructionRev"] = targetWorkInstructionRev.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "DeleteWorkInstructionRev",
                    Args = args,
                    When = DateTime.Now
                });
            }
            else
            {
                throw new Exception("The work instruction revision doesn't exist in the database");
            }
        }

        public void LinkJobRevToJob(string jobId, string jobRev)
        {
            if (db.Jobs.ContainsKey(jobId))
            {
                if (!db.JobRefToJobRevRefs[jobId].Contains(jobRev))
                {
                    db.JobRefToJobRevRefs[jobId].Add(jobRev);

                    var args = new Dictionary<string, string>();
                    args["JobId"] = jobId;
                    args["JobRev"] = jobRev;
                    db.AuditLog.Add(new Event
                    {
                        Action = "LinkJobRevToJob",
                        Args = args,
                        When = DateTime.Now,
                    });
                }
                else
                {
                    throw new Exception("Job revision already has an association to the given job");
                }
            }
            else
            {
                throw new Exception("Job doesn't exist in the database");
            }
        }

        public void UnlinkJobRevFromJob(string jobId, string jobRev)
        {
            if (db.Jobs.ContainsKey(jobId))
            {
                if (db.JobRefToJobRevRefs[jobId].Contains(jobRev))
                {
                    db.JobRefToJobRevRefs[jobId].Remove(jobRev);

                    var args = new Dictionary<string, string>();
                    args["JobId"] = jobId;
                    args["JobRev"] = jobRev;
                    db.AuditLog.Add(new Event
                    {
                        Action = "UnlinkJobRevFromJob",
                        Args = args,
                        When = DateTime.Now,
                    });
                }
                else
                {
                    throw new Exception("Job revision doesn't have an association with the given job");
                }
            }
            else
            {
                throw new Exception("Job doesn't exist in the database");
            }
        }

        public void MergeJobRevsBasedOnJob(string jobId1, string jobId2)
        {
            if(db.Jobs.ContainsKey(jobId1) && db.Jobs.ContainsKey(jobId2))
            {
                List<string> mergedList = db.JobRevs.Where(y => db.JobRefToJobRevRefs[jobId1].Contains(y) || db.JobRefToJobRevRefs[jobId2].Contains(y)).ToList();
                db.JobRefToJobRevRefs[jobId1] = mergedList;
                db.JobRefToJobRevRefs[jobId2] = mergedList;
                db.Jobs.Values.Select(y => y.Where(y => mergedList.Contains(y.RevPlan)).Select(y => y.RevSeq = mergedList.IndexOf(y.RevPlan)));

                var args = new Dictionary<string, string>();
                args["JobId1"] = jobId1;
                args["JobId2"] = jobId2;
                db.AuditLog.Add(new Event
                {
                    Action = "MergeJobRevsBasedOnJob",
                    Args = args,
                    When = DateTime.Now,
                });
            }
            else
            {
                throw new Exception("One or both of the jobs doesn't exist in the database");
            }
        }

        public void SplitJobRevInJob(string jobId, string jobRev)
        {
            if(db.Jobs.ContainsKey(jobId))
            {
                if (db.JobRefToJobRevRefs[jobId].Contains(jobRev))
                {
                    db.JobRefToJobRevRefs[jobId].Add(jobRev);

                    var args = new Dictionary<string, string>();
                    args["JobId"] = jobId;
                    args["JobRev"] = jobRev;
                    db.AuditLog.Add(new Event
                    {
                        Action = "SplitJobRevInJob",
                        Args = args,
                        When = DateTime.Now,
                    });
                }
                else
                {
                    throw new Exception("Job revision doesn't have an association with the given job");
                }
            }
            else
            {
                throw new Exception("The job doesn't exist in the database");
            }
        }

        public void CloneJobRevsBasedOnJob(string sourceJob, string targetJob, bool additive)
        {
            if(db.Jobs.ContainsKey(sourceJob) && db.Jobs.ContainsKey(targetJob))
            {
                if(!additive)
                {
                    db.JobRefToJobRevRefs[targetJob] = db.JobRefToJobRevRefs[sourceJob];
                }
                else
                {
                    List<string> mergedList = db.JobRevs.Where(y => db.JobRefToJobRevRefs[sourceJob].Contains(y) || db.JobRefToJobRevRefs[targetJob].Contains(y)).ToList();
                    db.JobRefToJobRevRefs[targetJob] = mergedList;
                    db.Jobs.Values.Select(y => y.Where(y => mergedList.Contains(y.RevPlan)).Select(y => y.RevSeq = mergedList.IndexOf(y.RevPlan)));
                }

                var args = new Dictionary<string, string>();
                args["SourceJob"] = sourceJob;
                args["TargetJob"] = targetJob;
                args["Additive"] = additive.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "CloneJobRevsBasedOnJob",
                    Args = args,
                    When = DateTime.Now,
                });
            }
            else
            {
                throw new Exception("One or both of the jobs doesn't exist in the database");
            }
        }

        public void LinkJobRevToQualityClauseRev(Guid qualityClauseRev, string jobRev)
        {
            if (db.QualityClauseRevs.Contains(qualityClauseRev))
            {
                if (!db.QualityClauseRevRefToJobRevRefs[qualityClauseRev].Contains(jobRev))
                {
                    db.QualityClauseRevRefToJobRevRefs[qualityClauseRev].Add(jobRev);

                    var args = new Dictionary<string, string>();
                    args["QualityClauseRev"] = qualityClauseRev.ToString();
                    args["JobRev"] = jobRev;
                    db.AuditLog.Add(new Event
                    {
                        Action = "LinkJobRevToQualityClauseRev",
                        Args = args,
                        When = DateTime.Now
                    });
                }
                else
                {
                    throw new Exception("Job revsion already has an association with the given quality clause revision");
                }
            }
            else
            {
                throw new Exception("The quality clause revision doesn't exist in the database");
            }
        }

        public void UnlinkJobRevFromQualityClauseRev(Guid qualityClauseRev, string jobRev)
        {
            if (db.QualityClauseRevs.Contains(qualityClauseRev))
            {
                if (db.QualityClauseRevRefToJobRevRefs[qualityClauseRev].Contains(jobRev))
                {
                    db.QualityClauseRevRefToJobRevRefs[qualityClauseRev].Remove(jobRev);

                    var args = new Dictionary<string, string>();
                    args["JobRev"] = jobRev;
                    args["QualityClauseRev"] = qualityClauseRev.ToString();
                    db.AuditLog.Add(new Event
                    {
                        Action = "UnlinkJobRevFromQualityClauseRev",
                        Args = args,
                        When = DateTime.Now
                    });
                }
                else
                {
                    throw new Exception("Job revision doesn't have an association with the given quality clause revision");
                }
            }
            else
            {
                throw new Exception("The quality clause revision doesn't exist for that job revision");
            }
        }

        public void MergeJobRevsBasedOnQualityClauseRev(Guid qualityClauseRev1, Guid qualityClauseRev2)
        {
            if (db.QualityClauseRevs.Contains(qualityClauseRev1) && db.QualityClauseRevs.Contains(qualityClauseRev2))
            {
                List<string> mergedList = db.JobRevs.Where(y => db.QualityClauseRevRefToJobRevRefs[qualityClauseRev1].Contains(y) || db.QualityClauseRevRefToJobRevRefs[qualityClauseRev2].Contains(y)).ToList();
                db.QualityClauseRevRefToJobRevRefs[qualityClauseRev1] = mergedList;
                db.QualityClauseRevRefToJobRevRefs[qualityClauseRev2] = mergedList;
                db.Jobs.Values.Select(y => y.Where(y => mergedList.Contains(y.RevPlan)).Select(y => y.RevSeq = mergedList.IndexOf(y.RevPlan)));

                var args = new Dictionary<string, string>();
                args["QualityClauseRev1"] = qualityClauseRev1.ToString();
                args["QualityClauseRev2"] = qualityClauseRev2.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "MergeJobRevsBasedOnQualityClauseRev",
                    Args = args,
                    When = DateTime.Now
                });
            }
            else
            {
                throw new Exception("One or both of the quality clause revisions doesn't exist in the database");
            }
        }

        public void SplitJobRevInQualityClauseRev(Guid qualityClauseRev, string jobRev)
        {
            if (db.QualityClauseRevs.Contains(qualityClauseRev))
            {
                if (db.QualityClauseRevRefToJobRevRefs[qualityClauseRev].Contains(jobRev))
                {
                    db.QualityClauseRevRefToJobRevRefs[qualityClauseRev].Add(jobRev);

                    var args = new Dictionary<string, string>();
                    args["QualityClauseRev"] = qualityClauseRev.ToString();
                    args["JobRev"] = jobRev;
                    db.AuditLog.Add(new Event
                    {
                        Action = "SplitJobRevToQualityClauseRev",
                        Args = args,
                        When = DateTime.Now
                    });
                }
                else
                {
                    throw new Exception("Job revision doesn't have an association with the given quality clause revision");
                }
            }
            else
            {
                throw new Exception("The quality clause revision doesn't exist in the database");
            }
        }

        public void CloneJobRevsBasedOnQualityClauseRev(Guid sourceQualityClauseRev, Guid targetQualityClauseRev, bool additive)
        {
            if (db.QualityClauseRevs.Contains(sourceQualityClauseRev) && db.QualityClauseRevs.Contains(targetQualityClauseRev))
            {
                if (!additive)
                {
                    db.QualityClauseRevRefToJobRevRefs[targetQualityClauseRev] = db.QualityClauseRevRefToJobRevRefs[sourceQualityClauseRev];
                }
                else
                {
                    List<string> mergedList = db.JobRevs.Where(y => db.QualityClauseRevRefToJobRevRefs[sourceQualityClauseRev].Contains(y) || db.QualityClauseRevRefToJobRevRefs[targetQualityClauseRev].Contains(y)).ToList();
                    db.QualityClauseRevRefToJobRevRefs[targetQualityClauseRev] = mergedList;
                    db.Jobs.Values.Select(y => y.Where(y => mergedList.Contains(y.RevPlan)).Select(y => y.RevSeq = mergedList.IndexOf(y.RevPlan)));
                }

                var args = new Dictionary<string, string>();
                args["SourceQualityClauseRev"] = sourceQualityClauseRev.ToString();
                args["TargetQualityClauseRev"] = targetQualityClauseRev.ToString();
                args["Additive"] = additive.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "CloneJobRevsBasedOnQualityClauseRev",
                    Args = args,
                    When = DateTime.Now
                });
            }
            else
            {
                throw new Exception("One or both of the quality clause revisions doesn't exist in the database");
            }
        }

        public void LinkQualityClauseRevToJobRev(string jobRev, Guid qualityClauseRev)
        {
            if (db.JobRevs.Contains(jobRev))
            {
                if (!db.JobRevRefToQualityClauseRevRefs[jobRev].Contains(qualityClauseRev))
                {
                    db.JobRevRefToQualityClauseRevRefs[jobRev].Add(qualityClauseRev);

                    var args = new Dictionary<string, string>();
                    args["JobRev"] = jobRev;
                    args["QualityClauseRev"] = qualityClauseRev.ToString();
                    db.AuditLog.Add(new Event
                    {
                        Action = "LinkQualityClauseRevToJobRev",
                        Args = args,
                        When = DateTime.Now
                    });
                }
                else
                {
                    throw new Exception("Quality clause revision already has an association with the given job revision");
                }
            }
            else
            {
                throw new Exception("The job doesn't have that revision");
            }
        }

        public void UnlinkQualityClauseRevFromJobRev(string jobRev, Guid qualityClauseRev)
        {
            if (db.JobRevs.Contains(jobRev))
            {
                if (db.JobRevRefToQualityClauseRevRefs[jobRev].Contains(qualityClauseRev))
                {
                    db.JobRevRefToQualityClauseRevRefs[jobRev].Remove(qualityClauseRev);

                    var args = new Dictionary<string, string>();
                    args["JobRev"] = jobRev;
                    args["QualityClauseRev"] = qualityClauseRev.ToString();
                    db.AuditLog.Add(new Event
                    {
                        Action = "UnlinkQualityClauseRevToJobRev",
                        Args = args,
                        When = DateTime.Now
                    });
                }
                else
                {
                    throw new Exception("Quality clause revision doesn't have an association with the given job revision");
                }
            }
            else
            {
                throw new Exception("This job revision doesn't exist in the database");
            }
        }


        public void MergeQualityClauseRevsBasedOnJobRev(string jobRev1, string jobRev2)
        {
            if (db.JobRevs.Contains(jobRev1) && db.JobRevs.Contains(jobRev2))
            {
                List<Guid> mergedList = db.QualityClauseRevs.Where(y => db.JobRevRefToQualityClauseRevRefs[jobRev1].Contains(y) || db.JobRevRefToQualityClauseRevRefs[jobRev2].Contains(y)).ToList();
                db.JobRevRefToQualityClauseRevRefs[jobRev1] = mergedList;
                db.JobRevRefToQualityClauseRevRefs[jobRev2] = mergedList;
                db.QualityClauses.Values.Select(y => y.Where(y => mergedList.Contains(y.Rev)).Select(y => y.RevSeq = mergedList.IndexOf(y.Rev)));

                var args = new Dictionary<string, string>();
                args["JobRev1"] = jobRev1;
                args["JobRev2"] = jobRev2;
                db.AuditLog.Add(new Event
                {
                    Action = "MergeQualityClauseRevsBasedOnJobRev",
                    Args = args,
                    When = DateTime.Now
                });
            }
            else
            {
                throw new Exception("One or both of the job revisions doesn't exist in the database");
            }
        }

        public void SplitQualityClauseRevInJobRev(string jobRev, Guid qualityClauseRev)
        {
            if (db.JobRevRefToQualityClauseRevRefs.ContainsKey(jobRev))
            {
                if (db.JobRevRefToQualityClauseRevRefs[jobRev].Contains(qualityClauseRev))
                {
                    db.JobRevRefToQualityClauseRevRefs[jobRev].Add(qualityClauseRev);

                    var args = new Dictionary<string, string>();
                    args["JobRev"] = jobRev;
                    args["QualityClauseRev"] = qualityClauseRev.ToString();
                    db.AuditLog.Add(new Event
                    {
                        Action = "SplitQualityClauseRevInJobRev",
                        Args = args,
                        When = DateTime.Now
                    });
                }
                else
                {
                    throw new Exception("Quality clause revision doesn't have an association with the given job revision");
                }
            }
            else
            {
                throw new Exception("The job revision doesn't exist in the database");
            }
        }

        public void CloneQualityClauseRevsBasedOnJobRev(string sourceJobRev, string targetJobRev, bool additive)
        {
            if (db.JobRevs.Contains(sourceJobRev) && db.JobRevs.Contains(targetJobRev))
            {
                if (!additive)
                {
                    db.JobRevRefToQualityClauseRevRefs[targetJobRev] = db.JobRevRefToQualityClauseRevRefs[sourceJobRev];
                }
                else
                {
                    List<Guid> mergedList = db.QualityClauseRevs.Where(y => db.JobRevRefToQualityClauseRevRefs[targetJobRev].Contains(y) || db.JobRevRefToQualityClauseRevRefs[sourceJobRev].Contains(y)).ToList();
                    db.JobRevRefToQualityClauseRevRefs[targetJobRev] = mergedList;
                    db.QualityClauses.Values.Select(y => y.Where(y => mergedList.Contains(y.Rev)).Select(y => y.RevSeq = mergedList.IndexOf(y.Rev)));
                }

                var args = new Dictionary<string, string>();
                args["SourceJobRev"] = sourceJobRev;
                args["TargetJobRev"] = targetJobRev;
                args["Additive"] = additive.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "CloneQualityClauseRevsBasedOnJobRev",
                    Args = args,
                    When = DateTime.Now
                });
            }
            else
            {
                throw new Exception("One or both of the job revisions doesn't exist in the database");
            }
        }

        public void LinkQualityClauseRevToQualityClause(Guid clauseRev, Guid clauseId)
        {
            if (db.QualityClauses.ContainsKey(clauseId))
            {
                if (!db.QualityClauseRefToQualityClauseRevRefs[clauseId].Contains(clauseRev))
                {
                    db.QualityClauseRefToQualityClauseRevRefs[clauseId].Add(clauseRev);

                    var args = new Dictionary<string, string>();
                    args["ClauseRev"] = clauseRev.ToString();
                    args["ClauseId"] = clauseId.ToString();
                    db.AuditLog.Add(new Event
                    {
                        Action = "LinkQualityClauseRevToQualityClause",
                        Args = args,
                        When = DateTime.Now
                    });
                }
                else
                {
                    throw new Exception("Quality clause revision already has an association with the given quality clause");
                }
            }
            else
            {
                throw new Exception("The quality clause doesn't exist in the database");
            }
        }

        public void UnlinkQualityClauseRevFromQualityClause(Guid clauseRev, Guid clauseId)
        {
            if (db.QualityClauses.ContainsKey(clauseId))
            {
                if (db.QualityClauseRefToQualityClauseRevRefs[clauseId].Contains(clauseRev))
                {
                    db.QualityClauseRefToQualityClauseRevRefs[clauseId].Remove(clauseRev);

                    var args = new Dictionary<string, string>();
                    args["ClauseRev"] = clauseRev.ToString();
                    args["ClauseId"] = clauseId.ToString();
                    db.AuditLog.Add(new Event
                    {
                        Action = "UnlinkQualityClauseRevFromQualityClause",
                        Args = args,
                        When = DateTime.Now
                    });
                }
                else
                {
                    throw new Exception("Quality clause revision doesn't have an association with the given quality clause");
                }
            }
            else
            {
                throw new Exception("The quality clause doesn't exist in the database");
            }
        }

        public void MergeQualityClauseRevsBasedOnQualityClause(Guid clauseId1, Guid clauseId2)
        {
            if (db.QualityClauses.ContainsKey(clauseId1) && db.QualityClauses.ContainsKey(clauseId2))
            {
                List<Guid> mergedList = db.QualityClauseRevs.Where(y => db.QualityClauseRefToQualityClauseRevRefs[clauseId1].Contains(y) || db.QualityClauseRefToQualityClauseRevRefs[clauseId2].Contains(y)).ToList();
                db.QualityClauseRefToQualityClauseRevRefs[clauseId1] = mergedList;
                db.QualityClauseRefToQualityClauseRevRefs[clauseId2] = mergedList;
                db.QualityClauses.Values.Select(y => y.Where(y => mergedList.Contains(y.Rev)).Select(y => y.RevSeq = mergedList.IndexOf(y.Rev)));

                var args = new Dictionary<string, string>();
                args["ClauseId1"] = clauseId1.ToString();
                args["ClauseId2"] = clauseId2.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "MergeQualityClauseRevsBasedOnQualityClause",
                    Args = args,
                    When = DateTime.Now
                });
            }
            else
            {
                throw new Exception("One or both of the quality clauses doesn't exist in the database");
            }
        }

        public void SplitQualityClauseRevInQualityClause(Guid clause, Guid clauseRev)
        {
            if (db.QualityClauses.ContainsKey(clause))
            {
                if (db.QualityClauseRefToQualityClauseRevRefs[clause].Contains(clauseRev))
                {
                    db.QualityClauseRefToQualityClauseRevRefs[clause].Add(clauseRev);

                    var args = new Dictionary<string, string>();
                    args["Clause"] = clause.ToString();
                    args["ClauseRev"] = clauseRev.ToString();
                    db.AuditLog.Add(new Event
                    {
                        Action = "SplitQualityClauseRevInQualityClause",
                        Args = args,
                        When = DateTime.Now
                    });
                }
                else
                {
                    throw new Exception("Quality clause revision doesn't have an association with the given quality clause");
                }
            }
            else
            {
                throw new Exception("The quality clause doesn't exist in the database");
            }
        }

        public void CloneQualityClauseRevsBasedOnQualityClause(Guid sourceClause, Guid targetClause, bool additive)
        {
            if (db.QualityClauses.ContainsKey(sourceClause) && db.QualityClauses.ContainsKey(targetClause))
            {
                if (!additive)
                {
                    db.QualityClauseRefToQualityClauseRevRefs[targetClause] = db.QualityClauseRefToQualityClauseRevRefs[sourceClause];
                }
                else
                {
                    List<Guid> mergedList = db.QualityClauseRevs.Where(y => db.QualityClauseRefToQualityClauseRevRefs[sourceClause].Contains(y) || db.QualityClauseRefToQualityClauseRevRefs[targetClause].Contains(y)).ToList();
                    db.QualityClauseRefToQualityClauseRevRefs[targetClause] = mergedList;
                    db.QualityClauses.Values.Select(y => y.Where(y => mergedList.Contains(y.Rev)).Select(y => y.RevSeq = mergedList.IndexOf(y.Rev)));
                }

                var args = new Dictionary<string, string>();
                args["SourceClause"] = sourceClause.ToString();
                args["TargetClause"] = targetClause.ToString();
                args["Additive"] = additive.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "CloneQualityClauseRevsBasedOnQualityClause",
                    Args = args,
                    When = DateTime.Now
                });
            }
            else
            {
                throw new Exception("One or both of the quality clauses doesn't exist in the database.");
            }
        }

        public void LinkJobOpToJobRev(string jobRev, int opId)
        {
            if(db.JobRevs.Contains(jobRev))
            {
                if (!db.JobRevRefToOpRefs[jobRev].Contains(opId))
                {
                    db.JobRevRefToOpRefs[jobRev].Add(opId);

                    var args = new Dictionary<string, string>();
                    args["JobRev"] = jobRev;
                    args["OpId"] = opId.ToString();
                    db.AuditLog.Add(new Event
                    {
                        Action = "LinkJobOpToJobRev",
                        Args = args,
                        When = DateTime.Now
                    });
                }
                else
                {
                    throw new Exception("Job op already has an association with the given job revision");
                }
            }
            else
            {
                throw new Exception("The job revision doesn't exist in the database");
            }
        }

        public void UnlinkJobOpFromJobRev(string jobRev, int opId)
        {
            if (db.JobRevs.Contains(jobRev))
            {
                if (db.JobRevRefToOpRefs[jobRev].Contains(opId))
                {
                    db.JobRevRefToOpRefs[jobRev].Remove(opId);

                    var args = new Dictionary<string, string>();
                    args["JobRev"] = jobRev;
                    args["OpId"] = opId.ToString();
                    db.AuditLog.Add(new Event
                    {
                        Action = "UnlinkJobOpFromJobRev",
                        Args = args,
                        When = DateTime.Now
                    });
                }
                else
                {
                    throw new Exception("Job op doesn't have an association with the given job revision");
                }
            }
            else
            {
                throw new Exception("The job revision doesn't exist in the database");
            }
        }

        public void MergeJobOpBasedOnJobRev(string jobRev1, string jobRev2)
        {
            if (db.JobRevs.Contains(jobRev1) && db.JobRevs.Contains(jobRev2))
            {

            }
        }

        public void SplitJobOpToJobRev()
        {

        }

        public void CloneJobOpToJobRev()
        {

        }

        public void LinkJobOpToOpSpecRev()
        {

        }

        public void UnlinkJobOpToOpSpecRev()
        {

        }

        public void MergeJobOpToOpSpecRev()
        {

        }

        public void SplitJobOpToOpSpecRev()
        {

        }

        public void CloneJobOpToOpSpecRev()
        {

        }

        public void LinkOpSpecRevToJobOp()
        {

        }

        public void UnlinkOpSpecRevToJobOp()
        {

        }

        public void MergeOpSpecRevToJobOp()
        {

        }

        public void SplitOpSpecRevToJobOp()
        {

        }

        public void CloneOpSpecRevToJobOp()
        {

        }

        public void LinkOpSpecRevToOpSpec()
        {

        }

        public void UnlinkOpSpecRevToOpSpec()
        {

        }

        public void MergeOpSpecRevToOpSpec()
        {

        }

        public void SplitOpSpecRevToOpSpec()
        {

        }

        public void CloneOpSpecRevToOpSpec()
        {

        }

        public void LinkWorkInstructionToJobOp()
        {

        }

        public void UnlinkWorkInstructionToJobOp()
        {

        }

        public void DeleteQualityClauseFromJob(Guid clauseId, string job)
        {
            if (db.JobRefToQualityClauseRefs[job].Contains(clauseId)) {
                db.JobRefToQualityClauseRefs[job].Remove(clauseId);

                var args = new Dictionary<string, string>();
                args["QualityClause"] = clauseId.ToString();
                args["Job"] = job;
                db.AuditLog.Add(new Event
                {
                    Action = "DeleteQualityClauseFromJob",
                    Args = args,
                    When = DateTime.Now,
                });
            } else
            {
                throw new Exception("This quality clause doesn't exist within this Job");
            }
        }

        public void LinkWorkInstructionRevToWorkInstruction()
        {

        }

        public void UnlinkWorkInstructionRevToWorkInstruction()
        {

        }

        public void MergeWorkInstructionRevToWorkInstruction()
        {

        }

        public void SplitWorkInstructionRevToWorkInstruction()
        {

        }

        public void CloneWorkInstructionRevToWorkInstruction()
        {

        }

        public List<WorkInstruction> DisplayPriorRevisionsOfWorkInstruction(WorkInstruction workInstruction)
        {
            return db.WorkInstructions[workInstruction.IdRevGroup];
        }

        public List<QualityClause> DisplayPriorRevisionsOfQualityClauses(QualityClause qualityClause)
        {
            return db.QualityClauses[qualityClause.Id];
        }

        public List<OpSpec> DisplayPriorRevisionsOfSpecs(OpSpec opSpec)
        {
            return db.OpSpecs[opSpec.IdRevGroup];
        }

        /*
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
        */

    }
}
