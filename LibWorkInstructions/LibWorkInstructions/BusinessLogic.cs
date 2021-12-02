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
            // entities
            public Dictionary<string, List<Job>> Jobs = new Dictionary<string, List<Job>>();
            public Dictionary<Guid, List<OpSpec>> OpSpecs = new Dictionary<Guid, List<OpSpec>>();
            public Dictionary<Guid, List<QualityClause>> QualityClauses = new Dictionary<Guid, List<QualityClause>>();
            public Dictionary<int, Op> Ops = new Dictionary<int, Op>();
            public Dictionary<Guid, List<WorkInstruction>> WorkInstructions = new Dictionary<Guid, List<WorkInstruction>>();

            // references to revisions
            public List<string> JobRevs = new List<string>();
            public List<Guid> QualityClauseRevs = new List<Guid>();
            public List<Guid> OpSpecRevs = new List<Guid>();
            public List<Guid> WorkInstructionRevs = new List<Guid>();

            // associations between entities
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

            // event tracker
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

        public void CreateJob(string jobId, string revCustomer, string revPlan, string rev)
        {
            if (!db.Jobs.ContainsKey(jobId)) // if the jobs dictionary doesn't already have the job
            {
                Job job = new Job { Id = jobId, RevSeq = 0, RevCustomer = revCustomer, Rev = rev}; // create and configure it
                db.Jobs[job.Id] = new List<Job> { job }; // add the job to the database
                db.JobRefToJobRevRefs[job.Id] = new List<string> { job.Rev }; // manage the references

                var args = new Dictionary<string, string>(); // add the event
                args["JobId"] = jobId;
                args["RevCustomer"] = revCustomer;
                args["RevPlan"] = revPlan;
                args["Rev"] = rev;
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

        public void DeleteJob(string jobId)
        {
            if (db.Jobs.ContainsKey(jobId)) // if the jobs dictionary has the job
            {
                List<string> jobRevs = db.JobRefToJobRevRefs[jobId];
                List<Op> ops = db.Ops.Values.Where(y => db.Jobs[jobId].Any(x => x.Ops.Contains(y))).ToList();
                List<int> opIds = ops.Select(y => y.Id).ToList();
                db.Jobs.Remove(jobId); // remove it and anything related to it from the database
                db.JobRefToJobRevRefs.Remove(jobId); // manage the references
                db.JobRevRefToOpRefs = db.JobRevRefToOpRefs.Where(y => !jobRevs.Contains(y.Key)).ToDictionary(y => y.Key, y => y.Value);
                db.JobRevRefToQualityClauseRevRefs = db.JobRevRefToQualityClauseRevRefs.Where(y => !jobRevs.Contains(y.Key)).ToDictionary(y => y.Key, y => y.Value);
                db.QualityClauseRevRefToJobRevRefs = db.QualityClauseRevRefToJobRevRefs.Select(y => y = new KeyValuePair<Guid, List<string>>(y.Key, y.Value.Where(x => jobRevs.Contains(x)).ToList())).ToDictionary(y => y.Key, y => y.Value);

                var args = new Dictionary<string, string>(); // add the event
                args["JobId"] = jobId;
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

        public void CreateJobRev(string jobId, string sourceJobRev, string newJobRev)
        {
            if (!db.JobRevs.Contains(newJobRev)) // if the job revision isn't already in the database
            {
                if (db.Jobs.ContainsKey(jobId)) // if the job exists in the database
                {
                    if (db.Jobs[jobId].Any(y => y.Rev == sourceJobRev)) // if the job has the revision
                    {
                        Job job = db.Jobs[jobId].First(y => y.Rev == sourceJobRev); // create a new instance of a job revision
                        job.Rev = newJobRev; // configure the job revision
                        job.RevSeq = db.Jobs[jobId].Count;
                        db.Jobs[jobId].Add(job); // add the job revision to the database
                        db.JobRevs.Add(newJobRev);
                        db.JobRevRefToQualityClauseRevRefs[newJobRev] = db.JobRevRefToQualityClauseRevRefs[sourceJobRev]; // manage the references
                        db.JobRevRefToOpRefs[newJobRev] = db.JobRevRefToOpRefs[sourceJobRev];
                        db.JobRefToJobRevRefs[jobId].Add(newJobRev);

                        var args = new Dictionary<string, string>(); // add the event
                        args["JobId"] = jobId;
                        args["SourceJobRev"] = sourceJobRev;
                        args["NewJobRev"] = newJobRev;
                        db.AuditLog.Add(new Event
                        {
                            Action = "CreateJobRev",
                            Args = args,
                            When = DateTime.Now
                        });
                    }
                    else
                    {
                        throw new Exception("The job doesn't have the source revision");
                    }
                }
                else
                {
                    throw new Exception("The job doesn't exist in the database");
                }
            }
            else
            {
                throw new Exception("The new job revision already exists in the database");
            }
        }

        public void CreateJobRev(Job newJobRev)
        {
            if (!db.JobRevs.Contains(newJobRev.Rev)) // if the job revision isn't already in the database
            {
                if (db.Jobs.ContainsKey(newJobRev.Id)) // if the job exists in the database
                {
                    newJobRev.RevSeq = db.Jobs[newJobRev.Id].Count; // configure the job revision
                    db.Jobs[newJobRev.Id].Add(newJobRev); // add the job revision to the database
                    db.JobRevs.Add(newJobRev.Rev);
                    db.JobRevRefToQualityClauseRevRefs[newJobRev.Rev] = newJobRev.QualityClauses.Select(y => y.Id).ToList(); // manage the references
                    db.JobRevRefToOpRefs[newJobRev.Rev] = newJobRev.Ops.Select(y => y.Id).ToList();
                    db.JobRefToJobRevRefs[newJobRev.Id].Add(newJobRev.Rev);

                    var args = new Dictionary<string, string>(); // add the event
                    args["NewJobRev"] = JsonSerializer.Serialize(newJobRev);
                    db.AuditLog.Add(new Event
                    {
                        Action = "CreateJobRev",
                        Args = args,
                        When = DateTime.Now
                    });
                }
                else
                {
                    throw new Exception("The job doesn't exist in the database");
                }
            }
            else
            {
                throw new Exception("The new job revision already exists in the database");
            }
        }

        public void UpdateJobRev(Job newJobRev)
        {
            if (db.JobRevs.Contains(newJobRev.Rev)) // if the job revision exists in the database
            {
                if (db.Jobs[newJobRev.Id].Any(y => y.Rev == newJobRev.Rev)) // if the job has the revision
                {
                    db.Jobs[newJobRev.Id][db.Jobs[newJobRev.Id].FindIndex(y => y.Rev == newJobRev.Rev)] = newJobRev; // update the revision
                    db.JobRevRefToQualityClauseRevRefs[newJobRev.Rev] = newJobRev.QualityClauses.Select(y => y.Id).ToList(); // manage the references
                    db.JobRevRefToOpRefs[newJobRev.Rev] = newJobRev.Ops.Select(y => y.Id).ToList();

                    var args = new Dictionary<string, string>(); // add the event
                    args["NewJobRev"] = JsonSerializer.Serialize(newJobRev);
                    db.AuditLog.Add(new Event
                    {
                        Action = "UpdateJobRev",
                        Args = args,
                        When = DateTime.Now
                    });
                }
                else
                {
                    throw new Exception("The job doesn't have the revision being updated");
                }
            }
            else
            {
                throw new Exception("The target job revision doesn't exist in the database");
            }
        }

        public void DeactivateJobRev(string jobId, string jobRev)
        {
            if (db.Jobs.ContainsKey(jobId)) // if the job exists in the database
            {
                if (db.Jobs[jobId].Any(y => y.Rev == jobRev)) // if the job has the revision
                {
                    db.Jobs[jobId][db.Jobs[jobId].FindIndex(y => y.Rev == jobRev)].Active = false;

                    var args = new Dictionary<string, string>(); // add the event
                    args["JobId"] = jobId;
                    args["JobRev"] = jobRev;
                    db.AuditLog.Add(new Event
                    {
                        Action = "DeactivateJobRev",
                        Args = args,
                        When = DateTime.Now
                    });
                }
                else
                {
                    throw new Exception("The job doesn't have that revision");
                }
            }
            else
            {
                throw new Exception("The job doesn't exist in the database");
            }
        }

        public void ActivateJobRev(string jobId, string jobRev)
        {
            if (db.Jobs.ContainsKey(jobId)) // if the job exists in the database
            {
                if (db.Jobs[jobId].Any(y => y.Rev == jobRev)) // if the job has the revision
                {
                    db.Jobs[jobId][db.Jobs[jobId].FindIndex(y => y.Rev == jobRev)].Active = true;

                    var args = new Dictionary<string, string>(); // add the event
                    args["JobId"] = jobId;
                    args["JobRev"] = jobRev;
                    db.AuditLog.Add(new Event
                    {
                        Action = "ActivateJobRev",
                        Args = args,
                        When = DateTime.Now
                    });
                }
                else
                {
                    throw new Exception("The job doesn't have that revision");
                }
            }
            else
            {
                throw new Exception("The job doesn't exist in the database");
            }
        }

        public void CreateQualityClause(string clause)
        {
            if (db.QualityClauses.Values.Any(y => y.Any(x => x.Clause == clause)))
            {
                throw new Exception("Quality clause is already in the database");
            }
            QualityClause newQualityClause = new QualityClause { Id = Guid.NewGuid(), IdRevGroup = Guid.NewGuid(), RevSeq = 0, Clause = clause }; // create and configure the quality clause
            db.QualityClauses[newQualityClause.IdRevGroup] =  new List<QualityClause> { newQualityClause }; // add quality clause to database
            db.QualityClauseRefToQualityClauseRevRefs[newQualityClause.Id] = new List<Guid>(); // manage references

            var args = new Dictionary<string, string>(); // add the event
            args["Clause"] = clause;
            db.AuditLog.Add(new Event
            {
                Action = "CreateQualityClause",
                Args = args,
                When = DateTime.Now,
            });
        }

        public void DeleteQualityClause(Guid clauseId)
        {
            if (db.QualityClauses.Values.Any(y => y[0].Id == clauseId)) // if any original quality clause has the given id
            {
                QualityClause targetClause = db.QualityClauses.Values.First(y => y[0].Id == clauseId)[0]; // remove the quality clause and its revisions from the database
                List<Guid> revIdList = db.QualityClauses[targetClause.IdRevGroup].Select(y => y.Id).ToList();
                db.QualityClauses.Remove(targetClause.IdRevGroup);
                db.QualityClauseRevs = db.QualityClauseRevs.Where(y => !revIdList.Contains(y)).ToList();
                db.QualityClauseRevRefToJobRevRefs = db.QualityClauseRevRefToJobRevRefs.Where(y => !revIdList.Contains(y.Key)).ToDictionary(y => y.Key, y => y.Value); // manage references
                db.JobRevRefToQualityClauseRevRefs = db.JobRevRefToQualityClauseRevRefs.Select(y => y = new KeyValuePair<string, List<Guid>>(y.Key, y.Value.Where(y => !revIdList.Contains(y)).ToList())).ToDictionary(y => y.Key, y => y.Value);
                db.QualityClauseRefToQualityClauseRevRefs.Remove(targetClause.IdRevGroup);

                var args = new Dictionary<string, string>(); // add the event
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
                throw new Exception("The id doesn't exist with regard to original quality clauses (e.g. it could be an id of a rev)");
            }
        }

        public void CreateQualityClauseRev(Guid groupId, Guid sourceClauseRev)
        {
            if (db.QualityClauses.ContainsKey(groupId)) // if the rev group exists with regard to quality clauses
            {
                if (db.QualityClauses[groupId].Any(y => y.Id == sourceClauseRev)) // if the source clause revision is in the rev group
                {
                    QualityClause clause = db.QualityClauses[groupId].First(y => y.Id == sourceClauseRev); // create new instance of a quality clause revision
                    clause.Id = Guid.NewGuid(); // configure it
                    clause.RevSeq = db.QualityClauses[groupId].Count;
                    db.QualityClauses[groupId].Add(clause); // add the quality clause revision to the database
                    db.QualityClauseRevs.Add(clause.Id);
                    db.QualityClauseRevRefToJobRevRefs[clause.Id] = db.QualityClauseRevRefToJobRevRefs[sourceClauseRev]; // manage references

                    var args = new Dictionary<string, string>(); // add the event
                    args["GroupId"] = groupId.ToString();
                    args["SourceClauseRev"] = sourceClauseRev.ToString();
                    db.AuditLog.Add(new Event
                    {
                        Action = "CreateQualityClauseRev",
                        Args = args,
                        When = DateTime.Now
                    });
                }
                else
                {
                    throw new Exception("The source quality clause revision isn't in the rev group specified");
                }
            }
            else
            {
                throw new Exception("The rev group doesn't exist with regard to quality clauses");
            }
        }

        public void CreateQualityClauseRev(QualityClause newClauseRev)
        {
            if (!db.QualityClauseRevs.Contains(newClauseRev.Id)) // if the quality clause revision isn't already in the database
            {
                if (db.QualityClauses.ContainsKey(newClauseRev.IdRevGroup)) // if the rev group exists with regard to quality clauses
                {
                    newClauseRev.RevSeq = db.QualityClauses[newClauseRev.IdRevGroup].Count; // configure it
                    db.QualityClauses[newClauseRev.IdRevGroup].Add(newClauseRev); // add quality clause revision to the database
                    db.QualityClauseRevs.Add(newClauseRev.Id);
                    db.QualityClauseRevRefToJobRevRefs[newClauseRev.Id] = new List<string>(); // manage references

                    var args = new Dictionary<string, string>(); // add the event
                    args["NewClauseRev"] = JsonSerializer.Serialize(newClauseRev);
                    db.AuditLog.Add(new Event
                    {
                        Action = "CreateQualityClauseRev",
                        Args = args,
                        When = DateTime.Now
                    });
                }
                else
                {
                    throw new Exception("The rev group doesn't exist with regard to quality clauses");
                }
            }
            else
            {
                throw new Exception("The new quality clause revision is already in the database");
            }
        }

        public void UpdateQualityClauseRev(QualityClause newClauseRev)
        {
            if (db.QualityClauseRevs.Contains(newClauseRev.Id)) // if the quality clause revision exists in the database
            {
                if (db.QualityClauses[newClauseRev.IdRevGroup].Any(y => y.Id == newClauseRev.Id)) // if the rev group has the revision
                {
                    if (db.QualityClauses[newClauseRev.IdRevGroup][0].Id != newClauseRev.Id) // if the id indeed refers to a revision, not the original quality clause
                    {
                        db.QualityClauses[newClauseRev.IdRevGroup][db.QualityClauses[newClauseRev.IdRevGroup].FindIndex(y => y.Id == newClauseRev.Id)] = newClauseRev; // update the quality clause revision

                        var args = new Dictionary<string, string>(); // add the event
                        args["NewClauseRev"] = JsonSerializer.Serialize(newClauseRev);
                        db.AuditLog.Add(new Event
                        {
                            Action = "UpdateQualityClauseRev",
                            Args = args,
                            When = DateTime.Now
                        });
                    }
                    else
                    {
                        throw new Exception("The id refers to an original quality clause, not the rev of one");
                    }
                }
                else
                {
                    throw new Exception("The rev group doesn't have the quality clause revision");
                }
            }
            else
            {
                throw new Exception("The quality clause revision doesn't exist in the database");
            }
        }

        public void DeleteQualityClauseRev(Guid groupId, Guid qualityClauseRev)
        {
            if (db.QualityClauses.ContainsKey(groupId)) // if the rev group exists with regard to quality clauses
            {
                if (db.QualityClauses[groupId].Any(y => y.Id == qualityClauseRev)) // if the quality clause revision is in the rev group
                {
                    if (db.QualityClauses[groupId][0].Id != qualityClauseRev) // if the id indeed refers to a revision, not an original quality clause
                    {
                        db.QualityClauseRevs.Remove(qualityClauseRev); // remove the quality clause revision from the database
                        db.QualityClauses[groupId].Remove(db.QualityClauses[groupId].First(y => y.Id == qualityClauseRev));
                        db.QualityClauses[groupId] = db.QualityClauses[groupId].Select(y => { y.RevSeq = db.QualityClauses[groupId].IndexOf(y); return y; }).ToList(); // reconfigure the rev sequences
                        db.JobRevRefToQualityClauseRevRefs = db.JobRevRefToQualityClauseRevRefs.Select(y => y = new KeyValuePair<string, List<Guid>>(y.Key, y.Value.Where(y => y != qualityClauseRev).ToList())).ToDictionary(y => y.Key, y => y.Value); // manage references
                        db.QualityClauseRefToQualityClauseRevRefs[db.QualityClauses[groupId][0].Id].Remove(qualityClauseRev);
                        db.QualityClauseRevRefToJobRevRefs.Remove(qualityClauseRev);

                        var args = new Dictionary<string, string>(); // add the event
                        args["GroupId"] = groupId.ToString();
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
                        throw new Exception("The id refers to an original quality clause, not the rev of one");
                    }
                }
                else
                {
                    throw new Exception("The rev group doesn't have the target quality clause revision");
                }
            }
            else
            {
                throw new Exception("The rev group doesn't exist with regard to quality clauses");
            }
        }

        public void CreateJobOp(Op op)
        {
            if (!db.Ops.ContainsKey(op.Id)) // if the op isn't already in the database
            {
                db.Ops[op.Id] = op; // add the op to the database
                db.OpRefToOpSpecRevRefs[op.Id] = new List<Guid>(); // manage references

                var args = new Dictionary<string, string>(); // add the event
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

        public void DeleteJobOp(int opId)
        {
            if (db.Ops.ContainsKey(opId)) // if the op is in the database
            {
                db.Ops.Remove(opId); // remove the op from the database
                db.Jobs = db.Jobs.Select(y => y = new KeyValuePair<string, List<Job>>(y.Key, y.Value.Select(y => { y.Ops = y.Ops.Where(y => y.Id != opId).ToList(); return y; }).ToList())).ToDictionary(y => y.Key, y => y.Value);
                db.OpRefToOpSpecRevRefs.Remove(opId); // manage references
                db.OpRefToWorkInstructionRef.Remove(opId);
                db.JobRevRefToOpRefs = db.JobRevRefToOpRefs.Select(y => y = new KeyValuePair<string, List<int>>(y.Key, y.Value.Where(y => y != opId).ToList())).ToDictionary(y => y.Key, y => y.Value);
                db.OpSpecRevRefToOpRefs = db.OpSpecRevRefToOpRefs.Select(y => y = new KeyValuePair<Guid, List<int>>(y.Key, y.Value.Where(y => y != opId).ToList())).ToDictionary(y => y.Key, y => y.Value);

                var args = new Dictionary<string, string>(); // add the event
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
                throw new Exception("The job op doesn't exist in the database");
            }
        }

        public void CreateOpSpec(OpSpec newSpec)
        {
            if (!db.OpSpecs.ContainsKey(newSpec.IdRevGroup)) // if the rev group isn't already in the database
            {
                newSpec.RevSeq = 0; // configure the op spec
                db.OpSpecs[newSpec.IdRevGroup] = new List<OpSpec> { newSpec }; // add the op spec to the database
                db.OpSpecRefToOpSpecRevRefs[newSpec.Id] = new List<Guid>(); // manage references

                var args = new Dictionary<string, string>(); // add the event
                args["newSpec"] = JsonSerializer.Serialize(newSpec);
                db.AuditLog.Add(new Event
                {
                    Action = "CreateOpSpec",
                    Args = args,
                    When = DateTime.Now,
                });
            }
            else
            {
                throw new Exception("The rev group is already in the database");
            }
        }

        public void UpdateOpSpec(OpSpec newSpec)
        {
            if (db.OpSpecs.ContainsKey(newSpec.IdRevGroup)) // if the rev group exists in the database
            {
                if (db.OpSpecs[newSpec.IdRevGroup].Any(y => y.Id == newSpec.Id)) // if the op spec is in the rev group
                {
                    if (db.OpSpecs[newSpec.IdRevGroup][0].Id == newSpec.Id) // if the id indeed refers to the original spec, not the rev of one
                    {
                        db.OpSpecs[newSpec.IdRevGroup][0] = newSpec; // update the op spec
                        List<int> ops = db.OpRefToWorkInstructionRef.Keys.Where(y => db.OpSpecRevRefToOpRefs.Any(x => x.Value.Contains(y) && db.OpSpecRefToOpSpecRevRefs[newSpec.Id].Contains(x.Key))).ToList();
                        db.WorkInstructions = db.WorkInstructions.Select(y => y = new KeyValuePair<Guid, List<WorkInstruction>>(y.Key, y.Value.Select(x => { if (ops.Contains(x.OpId)) x.Approved = false; return x; }).ToList())).ToDictionary(y => y.Key, y => y.Value); // invalidate the approval status of the work instruction

                        var args = new Dictionary<string, string>(); // add the event
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
                        throw new Exception("The id refers to a rev of a spec, not an original spec");
                    }
                }
                else
                {
                    throw new Exception("The rev group doesn't have the op spec");
                }
            }
            else
            {
                throw new Exception("The rev group doesn't exist in the database");
            }
        }

        public void DeleteOpSpec(Guid specId)
        {
            if (db.OpSpecs.Values.Any(y => y[0].Id == specId)) // if there's any original spec in the database that has the given spec id
            {
                OpSpec spec = db.OpSpecs.Values.First(y => y[0].Id == specId)[0]; // remove the spec and the revisions of it from the database
                List<Guid> idRevList = db.OpSpecs[spec.IdRevGroup].Select(y => y.Id).ToList();
                db.OpSpecs.Remove(db.OpSpecs.Values.First(y => y[0].Id == specId)[0].IdRevGroup);
                db.OpSpecRevs = db.OpSpecRevs.Where(y => !idRevList.Contains(y)).ToList();
                db.OpSpecRevRefToOpRefs = db.OpSpecRevRefToOpRefs.Where(y => !idRevList.Contains(y.Key)).ToDictionary(y => y.Key, y => y.Value); // manage references
                db.OpRefToOpSpecRevRefs = db.OpRefToOpSpecRevRefs.Select(y => y = new KeyValuePair<int, List<Guid>>(y.Key, y.Value.Where(y => !idRevList.Contains(y)).ToList())).ToDictionary(y => y.Key, y => y.Value);
                db.OpSpecRefToOpSpecRevRefs.Remove(specId);

                var args = new Dictionary<string, string>(); // add the events
                args["SpecId"] = specId.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "DeleteOpSpec",
                    Args = args,
                    When = DateTime.Now,
                });
            }
            else
            {
                throw new Exception("The id doesn't exist with regard to original specs (e.g. it could be an id of a rev)");
            }
        }

        public void CreateOpSpecRev(Guid groupId, Guid sourceSpecRev)
        {
            if (db.OpSpecs.ContainsKey(groupId)) // if the rev group exists with regard to op specs
            {
                if (db.OpSpecs[groupId].Any(y => y.Id == sourceSpecRev)) // if the source op spec revision is in the rev group
                {
                    OpSpec opSpec = db.OpSpecs[groupId].First(y => y.Id == sourceSpecRev); // configure the op spec revision
                    Guid sourceId = opSpec.Id;
                    opSpec.Id = Guid.NewGuid();
                    opSpec.RevSeq = db.OpSpecs[groupId].Count;
                    db.OpSpecs[groupId].Add(opSpec); // add the op spec revision to the database
                    db.OpSpecRevs.Add(opSpec.Id);
                    db.OpSpecRevRefToOpRefs[opSpec.Id] = db.OpSpecRevRefToOpRefs[sourceId]; // manage references
                    db.OpSpecRefToOpSpecRevRefs[db.OpSpecs[groupId][0].Id].Add(opSpec.Id);

                    var args = new Dictionary<string, string>(); // add the event
                    args["GroupId"] = groupId.ToString();
                    args["SourceSpecRev"] = sourceSpecRev.ToString();
                    db.AuditLog.Add(new Event
                    {
                        Action = "CreateOpSpecRev",
                        Args = args,
                        When = DateTime.Now
                    });
                }
                else
                {
                    throw new Exception("The rev group doesn't have the source op spec revision");
                }
            }
            else
            {
                throw new Exception("The rev group doesn't exist with regard to op specs");
            }
        }

        public void CreateOpSpecRev(OpSpec newSpecRev)
        {
            if (!db.OpSpecRevs.Contains(newSpecRev.Id)) // if the op spec revision isn't already in the database
            {
                if (db.OpSpecs.ContainsKey(newSpecRev.IdRevGroup)) // if the rev group exists with regard to op specs
                {
                    newSpecRev.RevSeq = db.OpSpecs[newSpecRev.IdRevGroup].Count; // configure the op spec revision
                    db.OpSpecs[newSpecRev.IdRevGroup].Add(newSpecRev); // add the op spec revision to the database
                    db.OpSpecRevs.Add(newSpecRev.Id);
                    db.OpSpecRevRefToOpRefs[newSpecRev.Id] = new List<int>(); // manage references
                    db.OpSpecRefToOpSpecRevRefs[db.OpSpecs[newSpecRev.IdRevGroup][0].Id].Add(newSpecRev.Id);

                    var args = new Dictionary<string, string>(); // add the event
                    args["newSpecRev"] = JsonSerializer.Serialize(newSpecRev);
                    db.AuditLog.Add(new Event
                    {
                        Action = "CreateOpSpecRev",
                        Args = args,
                        When = DateTime.Now
                    });
                }
                else
                {
                    throw new Exception("The rev group doesn't exist with regard to op specs");
                }
            }
            else
            {
                throw new Exception("The new op spec revision already exists in the database");
            }
        }

        public void UpdateOpSpecRev(OpSpec newSpecRev)
        {
            if (db.OpSpecs.ContainsKey(newSpecRev.IdRevGroup)) // if the rev group exists with regard to op specs
            {
                if (db.OpSpecs[newSpecRev.IdRevGroup].Any(y => y.Id == newSpecRev.Id)) // if the op spec revision is in the rev group
                {
                    if (db.OpSpecs[newSpecRev.IdRevGroup][0].Id != newSpecRev.Id) // if the id refers to a revision of a spec, not an original spec
                    {
                        db.OpSpecs[newSpecRev.IdRevGroup][db.OpSpecs[newSpecRev.IdRevGroup].FindIndex(y => y.Id == newSpecRev.Id)] = newSpecRev; // update the spec

                        var args = new Dictionary<string, string>(); // add the event
                        args["newSpecRev"] = JsonSerializer.Serialize(newSpecRev);
                        db.AuditLog.Add(new Event
                        {
                            Action = "UpdateOpSpecRev",
                            Args = args,
                            When = DateTime.Now,
                        });
                    }
                    else
                    {
                        throw new Exception("The id refers to an original op spec, not a rev of one");
                    }
                }
                else
                {
                    throw new Exception("The rev group doesn't have the op spec revision");
                }
            }
            else
            {
                throw new Exception("The rev group doesn't exist with regard to specs");
            }
        }

        public void DeleteOpSpecRev(Guid groupId, Guid specRev)
        {
            if (db.OpSpecs.ContainsKey(groupId)) // if the rev group exists with regard to op specs
            {
                if (db.OpSpecs[groupId].Any(y => y.Id == specRev)) // if the op spec revision is in the rev group
                {
                    if (db.OpSpecs[groupId][0].Id != specRev) // if the id indeed refers to a revision of an op spec, not an original op spec
                    {
                        db.OpSpecRevs.Remove(specRev); // remove the op spec revision from the database
                        db.OpSpecs[groupId].Remove(db.OpSpecs[groupId].First(y => y.Id == specRev));
                        db.OpSpecs[groupId] = db.OpSpecs[groupId].Select(y => { y.RevSeq = db.OpSpecs[groupId].IndexOf(y); return y; }).ToList(); // reconfigure the rev sequences
                        db.OpSpecRevRefToOpRefs.Remove(specRev); // manage references
                        db.OpRefToOpSpecRevRefs = db.OpRefToOpSpecRevRefs.Select(y => y = new KeyValuePair<int, List<Guid>>(y.Key, y.Value.Where(y => y != specRev).ToList())).ToDictionary(y => y.Key, y => y.Value);
                        db.OpSpecRefToOpSpecRevRefs[db.OpSpecs[groupId][0].Id].Remove(specRev);

                        var args = new Dictionary<string, string>(); // add the event
                        args["GroupId"] = groupId.ToString();
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
                        throw new Exception("The id refers to an original op spec, not a rev of one");
                    }
                }
                else
                {
                    throw new Exception("The op spec revision doesn't exist in the rev group");
                }
            }
            else
            {
                throw new Exception("The rev group doesn't exist with regard to op specs");
            }
        }

        public void CreateWorkInstruction(WorkInstruction workInstruction)
        {
            if (!db.WorkInstructions.ContainsKey(workInstruction.IdRevGroup)) // if the rev group exists with regard to work instructions
            {
                workInstruction.RevSeq = 0; // configure the work instruction
                db.WorkInstructions[workInstruction.IdRevGroup] = new List<WorkInstruction> { workInstruction }; // add the work instruction to the database
                db.OpRefToWorkInstructionRef[workInstruction.OpId] = workInstruction.Id; // manage references
                db.WorkInstructionRefToWorkInstructionRevRefs[workInstruction.Id] =  new List<Guid>();

                var args = new Dictionary<string, string>(); // add the event
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
                throw new Exception("The rev group already exists with regard to work instructions");
            }
        }

        public void UpdateWorkInstruction(WorkInstruction newWorkInstruction)
        {
            if (db.WorkInstructions.ContainsKey(newWorkInstruction.IdRevGroup)) // if the rev group exists with regard to work instructions
            {
                if (db.WorkInstructions[newWorkInstruction.IdRevGroup].Any(y => y.Id == newWorkInstruction.Id)) // if the work instruction exists in the rev group
                {
                    if (db.WorkInstructions[newWorkInstruction.IdRevGroup][0].Id == newWorkInstruction.Id) // if the id refers to an original work instruction, not a rev of one
                    {
                        db.WorkInstructions[newWorkInstruction.IdRevGroup][0] = newWorkInstruction; // update the work instruction
                        db.OpRefToWorkInstructionRef[newWorkInstruction.OpId] = newWorkInstruction.Id; // manage references

                        var args = new Dictionary<string, string>(); // add the evenet
                        args["newWorkInstruction"] = JsonSerializer.Serialize(newWorkInstruction);
                        db.AuditLog.Add(new Event
                        {
                            Action = "UpdateWorkInstruction",
                            Args = args,
                            When = DateTime.Now,
                        });
                    }
                    else
                    {
                        throw new Exception("The id refers to a rev of a work instruction, not an original one");
                    }
                }
                else
                {
                    throw new Exception("The work instruction doesn't exist in the rev group");
                }
            }
            else
            {
                throw new Exception("The group id doesn't exist with regard to original work instructions");
            }
        }

        public void DeleteWorkInstruction(Guid workId)
        {
            if (db.WorkInstructions.Values.Any(y => y[0].Id == workId)) // if any original work instruction has the given id
            {
                WorkInstruction targetWorkInstruction = db.WorkInstructions.Values.First(y => y[0].Id == workId)[0]; // remove the work instruction and the revisions of that work instruction from the database
                List<Guid> idRevsList = db.WorkInstructions[targetWorkInstruction.IdRevGroup].Select(y => y.Id).ToList();
                db.WorkInstructions.Remove(targetWorkInstruction.IdRevGroup);
                db.WorkInstructionRevs = db.WorkInstructionRevs.Where(y => !idRevsList.Contains(y)).ToList();
                db.OpRefToWorkInstructionRef.Remove(targetWorkInstruction.OpId); // manage references
                db.WorkInstructionRefToWorkInstructionRevRefs.Remove(workId);

                var args = new Dictionary<string, string>(); // add the event
                args["WorkId"] = workId.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "DeleteWorkInstruction",
                    Args = args,
                    When = DateTime.Now,
                });
            }
            else
            {
                throw new Exception("The id doesn't exist with regard to original work instructions (e.g. it could be the id of a rev)");
            }
        }

        public void CreateWorkInstructionRev(Guid groupId, Guid sourceWorkInstructionRev)
        {
            if (db.WorkInstructions.ContainsKey(groupId)) // if the rev group exists with regard to work instructions
            {
                if (db.WorkInstructions[groupId].Any(y => y.Id == sourceWorkInstructionRev)) // if the source work instruction revision is in the rev group
                {
                    WorkInstruction workInstruction = db.WorkInstructions[groupId].First(y => y.Id == sourceWorkInstructionRev); // configure the work instruction revision
                    Guid sourceId = workInstruction.Id;
                    workInstruction.Id = Guid.NewGuid();
                    workInstruction.RevSeq = db.WorkInstructions[workInstruction.IdRevGroup].Count;
                    db.WorkInstructions[workInstruction.IdRevGroup].Add(workInstruction); // add the work instruction revision to the database
                    db.WorkInstructionRevs.Add(workInstruction.Id);
                    db.WorkInstructionRefToWorkInstructionRevRefs[db.WorkInstructions[groupId][0].Id].Add(workInstruction.Id); // manage references

                    var args = new Dictionary<string, string>(); // add the event
                    args["GroupId"] = groupId.ToString();
                    args["SourceWorkInstructionRev"] = sourceWorkInstructionRev.ToString();
                    db.AuditLog.Add(new Event
                    {
                        Action = "CreateWorkInstructionRev",
                        Args = args,
                        When = DateTime.Now
                    });
                }
                else
                {
                    throw new Exception("The source work instruction isn't in the rev group");
                }
            }
            else
            {
                throw new Exception("The rev group doesn't exist with regard to work instructions");
            }
        }

        public void CreateWorkInstructionRev(WorkInstruction newWorkInstructionRev)
        {
            if (!db.WorkInstructionRevs.Contains(newWorkInstructionRev.Id)) // if the work instruction revision isn't already in the database
            {
                if (db.WorkInstructions.ContainsKey(newWorkInstructionRev.IdRevGroup)) // if the rev group exists with regard to work instructions
                {
                    newWorkInstructionRev.RevSeq = db.WorkInstructions[newWorkInstructionRev.IdRevGroup].Count; // configure the work instruction revision
                    db.WorkInstructions[newWorkInstructionRev.IdRevGroup].Add(newWorkInstructionRev); // add the work instruction revision to the database
                    db.WorkInstructionRevs.Add(newWorkInstructionRev.Id);
                    db.WorkInstructionRefToWorkInstructionRevRefs[db.WorkInstructions[newWorkInstructionRev.IdRevGroup][0].Id].Add(newWorkInstructionRev.Id); // manage references

                    var args = new Dictionary<string, string>(); // add the event
                    args["NewWorkInstructionRev"] = JsonSerializer.Serialize(newWorkInstructionRev);
                    db.AuditLog.Add(new Event
                    {
                        Action = "CreateWorkInstructionRev",
                        Args = args,
                        When = DateTime.Now
                    });
                }
                else
                {
                    throw new Exception("The rev group doesn't exist with regard to work instructions");
                }
            }
            else
            {
                throw new Exception("The work instruction revision already exists in the database.");
            }
        }

        public void UpdateWorkInstructionRev(WorkInstruction newWorkInstructionRev)
        {
            if (db.WorkInstructions.ContainsKey(newWorkInstructionRev.IdRevGroup)) // if the rev group exists with regard to work instructions
            {
                if (db.WorkInstructions[newWorkInstructionRev.IdRevGroup].Any(y => y.Id == newWorkInstructionRev.Id)) // if the work instruction revision is in the rev group
                {
                    if (db.WorkInstructions[newWorkInstructionRev.IdRevGroup][0].Id != newWorkInstructionRev.Id) // if the id refers to a revision of a work instruction, not an original one
                    {
                        db.WorkInstructions[newWorkInstructionRev.IdRevGroup][db.WorkInstructions[newWorkInstructionRev.IdRevGroup].FindIndex(y => y.Id == newWorkInstructionRev.Id)] = newWorkInstructionRev; // update the work instruction revision

                        var args = new Dictionary<string, string>(); // add the event
                        args["NewWorkInstructionRev"] = JsonSerializer.Serialize(newWorkInstructionRev);
                        db.AuditLog.Add(new Event
                        {
                            Action = "UpdateWorkInstructionRev",
                            Args = args,
                            When = DateTime.Now
                        });
                    }
                    else
                    {
                        throw new Exception("The id refers to an original work instruction, not a rev of one");
                    }
                }
                else
                {
                    throw new Exception("The work instruction revision being updated isn't in the rev group");
                }
            }
            else
            {
                throw new Exception("The rev group doesn't exist with regard to work instructions");
            }
        }

        public void DeleteWorkInstructionRev(Guid groupId, Guid workInstructionRev)
        {
            if (db.WorkInstructions.ContainsKey(groupId)) // if the rev group exists with regard to work instructions
            {
                if (db.WorkInstructions[groupId].Any(y => y.Id == workInstructionRev)) // if the work instruction revision is in the rev group
                {
                    if (db.WorkInstructions[groupId][0].Id != workInstructionRev) // if the id indeed refers to a revision, not an original work instruction
                    {
                        db.WorkInstructionRevs.Remove(workInstructionRev); // remove the work instruction revision from the database
                        db.WorkInstructions[groupId].Remove(db.WorkInstructions[groupId].First(y => y.Id == workInstructionRev));
                        db.WorkInstructions[groupId] = db.WorkInstructions[groupId].Select(y => { y.RevSeq = db.WorkInstructions[groupId].IndexOf(y); return y; }).ToList(); // reconfigure the rev sequences
                        db.WorkInstructionRefToWorkInstructionRevRefs[db.WorkInstructions[groupId][0].Id].Remove(workInstructionRev); // manage references

                        var args = new Dictionary<string, string>(); // add the event
                        args["GroupId"] = groupId.ToString();
                        args["WorkInstructionRev"] = workInstructionRev.ToString();
                        db.AuditLog.Add(new Event
                        {
                            Action = "DeleteWorkInstructionRev",
                            Args = args,
                            When = DateTime.Now
                        });
                    }
                    else
                    {
                        throw new Exception("The id refers to an original work instruction, not a revision of one");
                    }
                }
                else
                {
                    throw new Exception("The work instruction revision isn't in the rev group");
                }
            }
            else
            {
                throw new Exception("The rev group doesn't exist with regard to work instructions");
            }
        }

        public void MergeJobRevsBasedOnJob(string jobId1, string jobId2)
        {
            if(db.Jobs.ContainsKey(jobId1) && db.Jobs.ContainsKey(jobId2)) // if both jobs exist in the database
            {
                List<string> mergedIdList = db.JobRevs.Where(y => db.JobRefToJobRevRefs[jobId1].Contains(y) || db.JobRefToJobRevRefs[jobId2].Contains(y)).ToList(); // create a merged id list and object list
                List<Job> mergedJobRevList = mergedIdList.Select(y => db.Jobs.Values.First(x => x.Any(z => z.Rev == y)).First(x => x.Rev == y)).ToList();
                db.Jobs[jobId1] = new List<Job> { db.Jobs[jobId1][0] }; // add merged list to both jobs
                db.Jobs[jobId1].AddRange(mergedJobRevList);
                db.Jobs[jobId2] = new List<Job> { db.Jobs[jobId2][0] };
                db.Jobs[jobId2].AddRange(mergedJobRevList);
                db.Jobs[jobId1] = db.Jobs[jobId1].Select(y => { y.Id = jobId1; y.RevSeq = db.Jobs[jobId1].IndexOf(y); return y; }).ToList(); // reconfigure the job revisions
                db.Jobs[jobId2] = db.Jobs[jobId2].Select(y => { y.Id = jobId2; y.RevSeq = db.Jobs[jobId2].IndexOf(y); return y; }).ToList();
                db.JobRefToJobRevRefs[jobId1] = mergedIdList; // manage references
                db.JobRefToJobRevRefs[jobId2] = mergedIdList;

                var args = new Dictionary<string, string>(); // add the event
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

        public void SplitJobRevInJob(string jobId, string jobRev, string newJobRev)
        {
            if(db.Jobs.ContainsKey(jobId)) // if the job exists in the database
            {
                if (db.JobRefToJobRevRefs[jobId].Contains(jobRev)) // if the job has the revision
                {
                    int newRevPosition = db.Jobs[jobId].Count;
                    db.Jobs[jobId].Add(db.Jobs[jobId].First(y => y.Rev == jobRev)); // split the revision in the database
                    db.Jobs[jobId][newRevPosition].Rev = newJobRev; // reconfigure the revision
                    db.Jobs[jobId][newRevPosition].RevSeq = newRevPosition;
                    db.JobRevs.Add(newJobRev);
                    db.JobRefToJobRevRefs[jobId].Add(newJobRev); // manage references
                    db.JobRevRefToOpRefs[newJobRev] = db.JobRevRefToOpRefs[jobRev];
                    db.JobRevRefToQualityClauseRevRefs[newJobRev] = db.JobRevRefToQualityClauseRevRefs[jobRev];

                    var args = new Dictionary<string, string>(); // add the event
                    args["JobId"] = jobId;
                    args["JobRev"] = jobRev;
                    args["NewJobRev"] = newJobRev;
                    db.AuditLog.Add(new Event
                    {
                        Action = "SplitJobRevInJob",
                        Args = args,
                        When = DateTime.Now,
                    });
                }
                else
                {
                    throw new Exception("The job doesn't have the revision");
                }
            }
            else
            {
                throw new Exception("The job doesn't exist in the database");
            }
        }

        public void CloneJobRevsBasedOnJob(string sourceJob, string targetJob, bool additive)
        {
            if(db.Jobs.ContainsKey(sourceJob) && db.Jobs.ContainsKey(targetJob)) // if both of the jobs exist in the database
            {
                if(!additive)
                {
                    db.Jobs[targetJob] = new List<Job> { db.Jobs[targetJob][0] }; // replace the target job revisions with the source job revisions
                    db.Jobs[targetJob].AddRange(db.Jobs[sourceJob].Where(y => db.Jobs[sourceJob].IndexOf(y) != 0));
                    db.Jobs[targetJob] = db.Jobs[targetJob].Select(y => { y.Id = targetJob; return y; }).ToList(); // reconfigure the revisions
                    db.JobRefToJobRevRefs[targetJob] = db.JobRefToJobRevRefs[sourceJob]; // manage references
                }
                else
                {
                    List<string> mergedIdList = db.JobRevs.Where(y => db.JobRefToJobRevRefs[sourceJob].Contains(y) || db.JobRefToJobRevRefs[targetJob].Contains(y)).ToList(); // create a merged id list and object list
                    List<Job> mergedJobRevList = mergedIdList.Select(y => db.Jobs.Values.First(x => x.Any(z => z.Rev == y)).First(x => x.Rev == y)).ToList();
                    db.Jobs[targetJob] = new List<Job> { db.Jobs[targetJob][0] }; // add the merged revisions to the target job
                    db.Jobs[targetJob].AddRange(mergedJobRevList);
                    db.Jobs[targetJob] = db.Jobs[targetJob].Select(y => { y.Id = targetJob; y.RevSeq = db.Jobs[targetJob].IndexOf(y);  return y; }).ToList(); // reconfigure the revisions
                    db.JobRefToJobRevRefs[targetJob] = mergedIdList; // manage references
                }

                var args = new Dictionary<string, string>(); // add the event
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

        public void LinkJobRevToQualityClauseRev(string jobRev, Guid qualityClauseRev)
        {
            if (db.QualityClauseRevs.Contains(qualityClauseRev)) // if the quality clause revision exists in the database
            {
                if (db.JobRevs.Contains(jobRev)) // if the job revision exists in the database
                {
                    if (!db.QualityClauseRevRefToJobRevRefs[qualityClauseRev].Contains(jobRev)) // if the job revision isn't already linked to the quality clause revision
                    {
                        db.QualityClauseRevRefToJobRevRefs[qualityClauseRev].Add(jobRev); // link the job revision to the quality clause revision

                        var args = new Dictionary<string, string>(); // add the event
                        args["JobRev"] = jobRev;
                        args["QualityClauseRev"] = qualityClauseRev.ToString();
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
                    throw new Exception("The job revision doesn't exist in the database");
                }
            }
            else
            {
                throw new Exception("The quality clause revision doesn't exist in the database");
            }
        }

        public void UnlinkJobRevFromQualityClauseRev(string jobRev, Guid qualityClauseRev)
        {
            if (db.QualityClauseRevs.Contains(qualityClauseRev)) // if the quality clause revision exists in the database
            {
                if (db.QualityClauseRevRefToJobRevRefs[qualityClauseRev].Contains(jobRev)) // if the job revision is linked to the quality clause revision
                {
                    db.QualityClauseRevRefToJobRevRefs[qualityClauseRev].Remove(jobRev); // unlink the job revision from the quality clause revision

                    var args = new Dictionary<string, string>(); // add the event
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
            if (db.QualityClauseRevs.Contains(qualityClauseRev1) && db.QualityClauseRevs.Contains(qualityClauseRev2)) // if both of the quality clause revisions exist in the database
            {
                List<string> mergedList = db.JobRevs.Where(y => db.QualityClauseRevRefToJobRevRefs[qualityClauseRev1].Contains(y) || db.QualityClauseRevRefToJobRevRefs[qualityClauseRev2].Contains(y)).ToList(); // create id list
                db.QualityClauseRevRefToJobRevRefs[qualityClauseRev1] = mergedList; // manage references
                db.QualityClauseRevRefToJobRevRefs[qualityClauseRev2] = mergedList;

                var args = new Dictionary<string, string>(); // add the event
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

        public void CloneJobRevsBasedOnQualityClauseRev(Guid sourceQualityClauseRev, Guid targetQualityClauseRev, bool additive)
        {
            if (db.QualityClauseRevs.Contains(sourceQualityClauseRev) && db.QualityClauseRevs.Contains(targetQualityClauseRev)) // if both quality clause revisions exist in the database
            {
                if (!additive)
                {
                    db.QualityClauseRevRefToJobRevRefs[targetQualityClauseRev] = db.QualityClauseRevRefToJobRevRefs[sourceQualityClauseRev]; // replace the job revisions in the target clause with the revisions in the source quality clause
                }
                else
                {
                    List<string> mergedList = db.JobRevs.Where(y => db.QualityClauseRevRefToJobRevRefs[sourceQualityClauseRev].Contains(y) || db.QualityClauseRevRefToJobRevRefs[targetQualityClauseRev].Contains(y)).ToList(); // create a merged id list
                    db.QualityClauseRevRefToJobRevRefs[targetQualityClauseRev] = mergedList; // manage references
                }

                var args = new Dictionary<string, string>(); // add the event
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

        public void LinkQualityClauseRevToJobRev(Guid qualityClauseRev, string jobRev)
        {
            if (db.JobRevs.Contains(jobRev)) // if the job revision exists in the database
            {
                if (!db.JobRevRefToQualityClauseRevRefs[jobRev].Contains(qualityClauseRev)) // if the quality clause revision isn't already linked to the job revision
                {
                    QualityClause clauseRev = db.QualityClauses.Values.First(y => y.Any(x => x.Id == qualityClauseRev)).First(y => y.Id == qualityClauseRev); // create new instance of quality clause revision
                    clauseRev.RevSeq = db.JobRevRefToQualityClauseRevRefs[jobRev].Count; // configure the quality clause revision
                    db.Jobs[db.Jobs.First(y => y.Value.Any(x => x.Rev == jobRev)).Key] // link the quality clause revision to the job revision
                        [db.Jobs.First(y => y.Value.Any(x => x.Rev == jobRev)).Value.FindIndex(y => y.Rev == jobRev)].QualityClauses.Add(clauseRev);
                    db.JobRevRefToQualityClauseRevRefs[jobRev].Add(qualityClauseRev); // manage references

                    var args = new Dictionary<string, string>(); // add the event
                    args["QualityClauseRev"] = qualityClauseRev.ToString();
                    args["JobRev"] = jobRev;
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
                throw new Exception("The job revision doesn't exist in the database");
            }
        }

        public void UnlinkQualityClauseRevFromJobRev(Guid qualityClauseRev, string jobRev)
        {
            if (db.JobRevs.Contains(jobRev)) // if the job revision exists in the database
            {
                if (db.JobRevRefToQualityClauseRevRefs[jobRev].Contains(qualityClauseRev)) // if the quality clause revision is linked to the job revision
                {
                    string jobRevKey = db.Jobs.First(y => y.Value.Any(x => x.Rev == jobRev)).Key; // unlink the quality clause revision from the job revision
                    int jobRevIndex = db.Jobs.First(y => y.Value.Any(x => x.Rev == jobRev)).Value.FindIndex(y => y.Rev == jobRev);
                    db.Jobs[jobRevKey][jobRevIndex].QualityClauses.Remove(db.QualityClauses.Values.First(y => y.Any(x => x.Id == qualityClauseRev)).First(y => y.Id == qualityClauseRev));
                    db.JobRevRefToQualityClauseRevRefs[jobRev].Remove(qualityClauseRev); // manage references
                    db.Jobs[jobRevKey][jobRevIndex].QualityClauses.Select(y => { y.RevSeq = db.Jobs[jobRevKey][jobRevIndex].QualityClauses.IndexOf(y); return y; }); // reconfigure the rev sequences

                    var args = new Dictionary<string, string>(); // add the event
                    args["QualityClauseRev"] = qualityClauseRev.ToString();
                    args["JobRev"] = jobRev;
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
            if (db.JobRevs.Contains(jobRev1) && db.JobRevs.Contains(jobRev2)) // if both job revisions exist in the database
            {
                string jobRevKey1 = db.Jobs.First(y => y.Value.Any(x => x.Rev == jobRev1)).Key;
                int jobRevIndex1 = db.Jobs.First(y => y.Value.Any(x => x.Rev == jobRev1)).Value.FindIndex(x => x.Rev == jobRev1);
                string jobRevKey2 = db.Jobs.First(y => y.Value.Any(x => x.Rev == jobRev2)).Key;
                int jobRevIndex2 = db.Jobs.First(y => y.Value.Any(x => x.Rev == jobRev2)).Value.FindIndex(x => x.Rev == jobRev2);
                List<Guid> mergedIdList = db.QualityClauseRevs.Where(y => db.JobRevRefToQualityClauseRevRefs[jobRev1].Contains(y) || db.JobRevRefToQualityClauseRevRefs[jobRev2].Contains(y)).ToList(); // create a merged id list and object list
                List<QualityClause> mergedClauseRevList = mergedIdList.Select(y => db.QualityClauses.Values.First(y => y.Any(x => mergedIdList.Contains(x.Id))).First(y => mergedIdList.Contains(y.Id))).ToList();
                mergedClauseRevList = mergedClauseRevList.Select(y => { y.RevSeq = mergedClauseRevList.IndexOf(y); return y; }).ToList(); // reconfigure the rev sequences
                db.Jobs[jobRevKey1][jobRevIndex1].QualityClauses = mergedClauseRevList; // add the merged list to the database
                db.Jobs[jobRevKey2][jobRevIndex2].QualityClauses = mergedClauseRevList;
                db.JobRevRefToQualityClauseRevRefs[jobRev1] = mergedIdList; // manage references
                db.JobRevRefToQualityClauseRevRefs[jobRev2] = mergedIdList;

                var args = new Dictionary<string, string>(); // add the event 
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
            if (db.JobRevs.Contains(jobRev)) // if the job revision exists in the database
            {
                if (db.JobRevRefToQualityClauseRevRefs[jobRev].Contains(qualityClauseRev)) // if the quality clause revision is linked to the job revision
                {
                    QualityClause clauseRev = db.QualityClauses.Values.First(y => y.Any(x => x.Id == qualityClauseRev)).First(y => y.Id == qualityClauseRev); // create new instance of quality clause revision
                    clauseRev.Id = Guid.NewGuid(); // configure the new revision
                    db.QualityClauseRevs.Add(clauseRev.Id); // add the quality clause revision to the database
                    db.Jobs[db.Jobs.First(y => y.Value.Any(x => x.Rev == jobRev)).Key][db.Jobs.First(y => y.Value.Any(x => x.Rev == jobRev)).Value.FindIndex(x => x.Rev == jobRev)].QualityClauses.Add(clauseRev);
                    db.JobRevRefToQualityClauseRevRefs[jobRev].Add(clauseRev.Id); // manage references
                    db.QualityClauseRevRefToJobRevRefs[clauseRev.Id] = db.QualityClauseRevRefToJobRevRefs[qualityClauseRev];

                    var args = new Dictionary<string, string>(); // add the event
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
            if (db.JobRevs.Contains(sourceJobRev) && db.JobRevs.Contains(targetJobRev)) // if both job revisions exist in the database
            {
                if (!additive)
                {
                    db.Jobs[db.Jobs.First(y => y.Value.Any(x => x.Rev == targetJobRev)).Key][db.Jobs.First(y => y.Value.Any(x => x.Rev == targetJobRev)).Value.FindIndex(y => y.Rev == targetJobRev)].QualityClauses = // replace the quality clause revisions in the target job revision
                        db.Jobs[db.Jobs.First(y => y.Value.Any(x => x.Rev == sourceJobRev)).Key][db.Jobs.First(y => y.Value.Any(x => x.Rev == sourceJobRev)).Value.FindIndex(y => y.Rev == sourceJobRev)].QualityClauses;
                    db.JobRevRefToQualityClauseRevRefs[targetJobRev] = db.JobRevRefToQualityClauseRevRefs[sourceJobRev]; // manage references
                }
                else
                {
                    List<Guid> mergedIdList = db.QualityClauseRevs.Where(y => db.JobRevRefToQualityClauseRevRefs[targetJobRev].Contains(y) || db.JobRevRefToQualityClauseRevRefs[sourceJobRev].Contains(y)).ToList(); // create a merged id list and object list
                    List<QualityClause> mergedClauseRevList = mergedIdList.Select(y => db.QualityClauses.First(x => x.Value.Any(z => mergedIdList.Contains(y))).Value.First(x => mergedIdList.Contains(y))).ToList();
                    db.Jobs[db.Jobs.First(y => y.Value.Any(x => x.Rev == targetJobRev)).Key][db.Jobs.First(y => y.Value.Any(x => x.Rev == targetJobRev)).Value.FindIndex(y => y.Rev == targetJobRev)].QualityClauses = mergedClauseRevList; // clone the quality clause revs in the database
                    db.JobRevRefToQualityClauseRevRefs[targetJobRev] = mergedIdList; // manage references
                }

                var args = new Dictionary<string, string>(); // add the event 
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

        public void MergeQualityClauseRevsBasedOnQualityClause(Guid clauseId1, Guid clauseId2)
        {
            if (db.QualityClauseRefToQualityClauseRevRefs.ContainsKey(clauseId1) && db.QualityClauseRefToQualityClauseRevRefs.ContainsKey(clauseId2)) // if both of the quality clauses exist in the database
            {
                List<Guid> mergedIdList = db.QualityClauseRevs.Where(y => db.QualityClauseRefToQualityClauseRevRefs[clauseId1].Contains(y) || db.QualityClauseRefToQualityClauseRevRefs[clauseId2].Contains(y)).ToList(); // create a merged id list and object list
                List<QualityClause> mergedClauseRevList = mergedIdList.Select(y => db.QualityClauses.First(x => x.Value.Any(z => mergedIdList.Contains(y))).Value.First(x => mergedIdList.Contains(y))).ToList();
                Guid groupId1 = db.QualityClauses.First(y => y.Value[0].Id == clauseId1).Key;
                Guid groupId2 = db.QualityClauses.First(y => y.Value[0].Id == clauseId2).Key;
                db.QualityClauses[groupId1] = new List<QualityClause> { db.QualityClauses[groupId1][0] }; // add merged revisions to database
                db.QualityClauses[groupId1].AddRange(mergedClauseRevList);
                db.QualityClauses[groupId2] = new List<QualityClause> { db.QualityClauses[groupId2][0] };
                db.QualityClauses[groupId2].AddRange(mergedClauseRevList);
                db.QualityClauses[groupId1] = db.QualityClauses[groupId1].Select(y => { y.IdRevGroup = groupId1; y.RevSeq = db.QualityClauses[groupId1].IndexOf(y); return y; }).ToList(); // reconfigure the revisions
                db.QualityClauses[groupId2] = db.QualityClauses[groupId2].Select(y => { y.IdRevGroup = groupId2; y.RevSeq = db.QualityClauses[groupId2].IndexOf(y); return y; }).ToList();
                db.QualityClauseRefToQualityClauseRevRefs[clauseId1] = mergedIdList; // manage references
                db.QualityClauseRefToQualityClauseRevRefs[clauseId2] = mergedIdList;

                var args = new Dictionary<string, string>(); // add the event
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

        public void SplitQualityClauseRevInQualityClause(Guid qualityClause, Guid qualityClauseRev)
        {
            if (db.QualityClauseRefToQualityClauseRevRefs.ContainsKey(qualityClause)) // if the clause exists in the database
            {
                if (db.QualityClauseRefToQualityClauseRevRefs[qualityClause].Contains(qualityClauseRev)) // if the quality clause has the revision
                {
                    Guid revGroup = db.QualityClauses.First(y => y.Value[0].Id == qualityClause).Key;
                    int newRevPosition = db.QualityClauses[revGroup].Count;
                    db.QualityClauses[revGroup].Add(db.QualityClauses[revGroup].First(y => y.Id == qualityClauseRev)); // split the revision in the database
                    db.QualityClauses[revGroup][newRevPosition].Id = Guid.NewGuid(); // reconfigure the revision
                    db.QualityClauses[revGroup][newRevPosition].RevSeq = newRevPosition;
                    db.QualityClauseRevs.Add(db.QualityClauses[revGroup][newRevPosition].Id); // add the new revision to the database
                    db.QualityClauseRefToQualityClauseRevRefs[qualityClause].Add(qualityClauseRev); // manage references
                    db.QualityClauseRevRefToJobRevRefs[db.QualityClauses[revGroup][newRevPosition].Id] = db.QualityClauseRevRefToJobRevRefs[qualityClauseRev];

                    var args = new Dictionary<string, string>(); // add the event
                    args["QualityClause"] = qualityClause.ToString();
                    args["QualityClauseRev"] = qualityClauseRev.ToString();
                    db.AuditLog.Add(new Event
                    {
                        Action = "SplitQualityClauseRevInQualityClause",
                        Args = args,
                        When = DateTime.Now
                    });
                }
                else
                {
                    throw new Exception("Quality clause doesn't have the source revision");
                }
            }
            else
            {
                throw new Exception("The quality clause doesn't exist in the database");
            }
        }

        public void CloneQualityClauseRevsBasedOnQualityClause(Guid sourceClause, Guid targetClause, bool additive)
        {
            if (db.QualityClauseRefToQualityClauseRevRefs.ContainsKey(sourceClause) && db.QualityClauseRefToQualityClauseRevRefs.ContainsKey(targetClause)) // if both clauses exist in the database
            {
                Guid targetRevGroup = db.QualityClauses.First(y => y.Value[0].Id == targetClause).Key;
                Guid sourceRevGroup = db.QualityClauses.First(y => y.Value[0].Id == sourceClause).Key;
                if (!additive)
                {
                    db.QualityClauses[targetRevGroup] = new List<QualityClause> { db.QualityClauses[targetRevGroup][0] }; // replace the target revisions with the source revisions in the database
                    db.QualityClauses[targetRevGroup].AddRange(db.QualityClauses[sourceRevGroup].Where(y => db.QualityClauses[sourceRevGroup].IndexOf(y) != 0));
                    db.QualityClauses[targetRevGroup] = db.QualityClauses[targetRevGroup].Select(y => { y.IdRevGroup = targetRevGroup; return y; }).ToList(); // reconfigure the revisions
                    db.QualityClauseRefToQualityClauseRevRefs[targetClause] = db.QualityClauseRefToQualityClauseRevRefs[sourceClause]; // manage references
                }
                else
                {
                    List<Guid> mergedIdList = db.QualityClauseRevs.Where(y => db.QualityClauseRefToQualityClauseRevRefs[sourceClause].Contains(y) || db.QualityClauseRefToQualityClauseRevRefs[targetClause].Contains(y)).ToList(); // create a merged id list and object list
                    List<QualityClause> mergedClauseRevList = mergedIdList.Select(y => db.QualityClauses.Values.First(x => x.Any(z => z.Id == y)).First(x => x.Id == y)).ToList();
                    db.QualityClauses[targetRevGroup] = new List<QualityClause> { db.QualityClauses[targetRevGroup][0] }; // add the merged revisions to the target rev group
                    db.QualityClauses[targetRevGroup].AddRange(mergedClauseRevList);
                    db.QualityClauses[targetRevGroup] = db.QualityClauses[targetRevGroup].Select(y => { y.IdRevGroup = targetRevGroup; y.RevSeq = db.QualityClauses[targetRevGroup].IndexOf(y); return y; }).ToList(); // reconfigure the revisions
                    db.QualityClauseRefToQualityClauseRevRefs[targetClause] = mergedIdList; // manage references
                }

                var args = new Dictionary<string, string>(); // add the event
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

        public void LinkJobOpToJobRev(int opId, string jobRev)
        {
            if(db.JobRevs.Contains(jobRev)) // if the job revision exists in the database
            {
                if (db.Ops.ContainsKey(opId)) // if the op exists in the database
                {
                    if (!db.JobRevRefToOpRefs.Values.Any(y => y.Contains(opId))) // if an op isn't already linked to a job revision
                    {
                        string job = db.Jobs.First(y => y.Value.Any(x => x.Rev == jobRev)).Key; // link the op to the job revision
                        int revIndex = db.Jobs.First(y => y.Value.Any(x => x.Rev == jobRev)).Value.FindIndex(y => y.Rev == jobRev);
                        db.Jobs[job][revIndex].Ops.Add(db.Ops[opId]);
                        db.Jobs[job][revIndex].Ops = db.Jobs[job][revIndex].Ops.OrderBy(y => y.Seq).ToList(); // reorder the ops by sequence
                        db.JobRevRefToOpRefs[jobRev].Add(opId); // manage references

                        var args = new Dictionary<string, string>(); // add the event
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
                        throw new Exception("Op already has an association with another job revision");
                    }
                }
                else
                {
                    throw new Exception("Op doesn't exist in the database");
                }
            }
            else
            {
                throw new Exception("The job revision doesn't exist in the database");
            }
        }

        public void UnlinkJobOpFromJobRev(int opId, string jobRev)
        {
            if (db.JobRevs.Contains(jobRev))
            {
                if (db.JobRevRefToOpRefs[jobRev].Contains(opId))
                {
                    string job = db.Jobs.First(y => y.Value.Any(x => x.Rev == jobRev)).Key; // unlink the op from the job revision
                    int revIndex = db.Jobs.First(y => y.Value.Any(x => x.Rev == jobRev)).Value.FindIndex(y => y.Rev == jobRev);
                    db.Jobs[job][revIndex].Ops.Remove(db.Ops[opId]);
                    db.JobRevRefToOpRefs[jobRev].Remove(opId); // manage references

                    var args = new Dictionary<string, string>(); // add the event
                    args["OpId"] = opId.ToString();
                    args["JobRev"] = jobRev;
                    db.AuditLog.Add(new Event
                    {
                        Action = "UnlinkJobOpFromJobRev",
                        Args = args,
                        When = DateTime.Now
                    });
                }
                else
                {
                    throw new Exception("The job revision doesn't have the op");
                }
            }
            else
            {
                throw new Exception("The job revision doesn't exist in the database");
            }
        }

        public void MergeJobOpsBasedOnJobRev(string jobRev1, string jobRev2)
        {
            if (db.JobRevs.Contains(jobRev1) && db.JobRevs.Contains(jobRev2)) // if both job revisions exist in the database
            {
                string job1 = db.Jobs.First(y => y.Value.Any(x => x.Rev == jobRev1)).Key;
                int revIndex1 = db.Jobs.First(y => y.Value.Any(x => x.Rev == jobRev1)).Value.FindIndex(y => y.Rev == jobRev1);
                string job2 = db.Jobs.First(y => y.Value.Any(x => x.Rev == jobRev2)).Key;
                int revIndex2 = db.Jobs.First(y => y.Value.Any(x => x.Rev == jobRev2)).Value.FindIndex(y => y.Rev == jobRev2);
                List<int> mergedIdList = db.Ops.Keys.Where(y => db.JobRevRefToOpRefs[jobRev1].Contains(y) || db.JobRevRefToOpRefs[jobRev2].Contains(y)).ToList(); // create a merged id list and object list
                List<Op> mergedOpList = db.Ops.Values.Where(y => db.JobRevRefToOpRefs[jobRev1].Contains(y.Id) || db.JobRevRefToOpRefs[jobRev2].Contains(y.Id)).ToList();
                mergedOpList = mergedOpList.OrderBy(y => y.Seq).ToList(); // order the merged list by sequence
                db.Jobs[job1][revIndex1].Ops = mergedOpList.Select(y => { y.JobId = job1; return y; }).ToList(); // add the merged op list to the job revisions
                db.Jobs[job2][revIndex2].Ops = mergedOpList.Select(y => { y.JobId = job2; return y; }).ToList();
                db.JobRevRefToOpRefs[jobRev1] = mergedIdList; // manage references
                db.JobRevRefToOpRefs[jobRev2] = mergedIdList;

                var args = new Dictionary<string, string>(); // add the event
                args["JobRev1"] = jobRev1;
                args["JobRev2"] = jobRev2;
                db.AuditLog.Add(new Event
                {
                    Action = "MergeJobOpsBasedOnJobRev",
                    Args = args,
                    When = DateTime.Now
                });
            }
            else
            {
                throw new Exception("One or both of the job revisions doesn't exist in the database");
            }
        }

        public void CloneJobOpsBasedOnJobRev(string sourceJobRev, string targetJobRev, bool additive)
        {
            if (db.JobRevs.Contains(sourceJobRev) && db.JobRevs.Contains(targetJobRev)) // if both job revisions exist in the database
            {
                string sourceJob = db.Jobs.First(y => y.Value.Any(x => x.Rev == sourceJobRev)).Key;
                int sourceRevIndex = db.Jobs.First(y => y.Value.Any(x => x.Rev == sourceJobRev)).Value.FindIndex(y => y.Rev == sourceJobRev);
                string targetJob = db.Jobs.First(y => y.Value.Any(x => x.Rev == targetJobRev)).Key;
                int targetRevIndex = db.Jobs.First(y => y.Value.Any(x => x.Rev == targetJobRev)).Value.FindIndex(y => y.Rev == targetJobRev);
                if (!additive)
                {
                    db.Jobs[targetJob][targetRevIndex].Ops = db.Jobs[sourceJob][sourceRevIndex].Ops; // replace the target revision's ops with the source revision's ops
                    db.Jobs[targetJob][targetRevIndex].Ops = db.Jobs[targetJob][targetRevIndex].Ops.Select(y => { y.JobId = targetJob; return y; }).ToList(); // reconfigure the ops
                    db.JobRevRefToOpRefs[targetJobRev] = db.JobRevRefToOpRefs[sourceJobRev]; // manage references
                }
                else
                {
                    List<int> mergedIdList = db.Ops.Keys.Where(y => db.JobRevRefToOpRefs[sourceJobRev].Contains(y) || db.JobRevRefToOpRefs[targetJobRev].Contains(y)).ToList(); // create a merged id list and object list
                    List<Op> mergedOpList = db.Ops.Values.Where(y => db.JobRevRefToOpRefs[sourceJobRev].Contains(y.Id) || db.JobRevRefToOpRefs[targetJobRev].Contains(y.Id)).ToList();
                    mergedOpList = mergedOpList.OrderBy(y => y.Seq).ToList(); // order the merged list by sequence
                    db.Jobs[targetJob][targetRevIndex].Ops = mergedOpList.Select(y => { y.JobId = targetJob; return y; }).ToList(); // add the merged op list to the target job revision
                    db.JobRevRefToOpRefs[targetJobRev] = mergedIdList; // manage references
                }

                var args = new Dictionary<string, string>(); // add the event
                args["SourceJobRev"] = sourceJobRev;
                args["TargetJobRev"] = targetJobRev;
                args["Additive"] = additive.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "CloneJobOpsBasedOnJobRev",
                    Args = args,
                    When = DateTime.Now
                });
            }
            else
            {
                throw new Exception("One or both of the job revisions doesn't exist in the database");
            }
        }

        public void LinkJobOpToOpSpecRev(int opId, Guid opSpecRev)
        {
            if (db.OpSpecRevs.Contains(opSpecRev)) // if the op spec revision exists in the database
            {
                if (!db.OpSpecRevRefToOpRefs[opSpecRev].Contains(opId)) // if the op isn't already linked to the op spec
                {
                    db.OpSpecRevRefToOpRefs[opSpecRev].Add(opId); // link the op to the op spec

                    var args = new Dictionary<string, string>(); // add the event
                    args["OpId"] = opId.ToString();
                    args["OpSpecRev"] = opSpecRev.ToString();
                    db.AuditLog.Add(new Event
                    {
                        Action = "LinkJobOpToOpSpecRev",
                        Args = args,
                        When = DateTime.Now
                    });
                }
                else
                {
                    throw new Exception("Op is already linked to the op spec revision");
                }
            }
            else
            {
                throw new Exception("The op spec revision doesn't exist in the database");
            }
        }

        public void UnlinkJobOpFromOpSpecRev(int opId, Guid opSpecRev)
        {
            if (db.OpSpecRevs.Contains(opSpecRev)) // if the op spec revision exists in the database
            {
                if (db.OpSpecRevRefToOpRefs[opSpecRev].Contains(opId)) // if the op is linked to the op spec revision
                {
                    db.OpSpecRevRefToOpRefs[opSpecRev].Remove(opId); // unlink the op from the op spec revision

                    var args = new Dictionary<string, string>(); // add the event
                    args["OpId"] = opId.ToString();
                    args["OpSpecRev"] = opSpecRev.ToString();
                    db.AuditLog.Add(new Event
                    {
                        Action = "UnlinkJobOpFromOpSpecRev",
                        Args = args,
                        When = DateTime.Now
                    });
                }
                else
                {
                    throw new Exception("Op already isn't linked to the op spec revision");
                }
            }
            else
            {
                throw new Exception("The op spec revision doesn't exist in the database");
            }
        }

        public void MergeJobOpsBasedOnOpSpecRev(Guid opSpecRev1, Guid opSpecRev2)
        {
            if (db.OpSpecRevs.Contains(opSpecRev1) && db.OpSpecRevs.Contains(opSpecRev2)) // if both op spec revisions exist in the database
            {
                List<int> mergedList = db.Ops.Keys.Where(y => db.OpSpecRevRefToOpRefs[opSpecRev1].Contains(y) || db.OpSpecRevRefToOpRefs[opSpecRev2].Contains(y)).ToList(); // create a merged id list
                db.OpSpecRevRefToOpRefs[opSpecRev1] = mergedList; // put the merged list in the references of each op spec revision
                db.OpSpecRevRefToOpRefs[opSpecRev2] = mergedList;

                var args = new Dictionary<string, string>(); // add the event
                args["OpSpecRev1"] = opSpecRev1.ToString();
                args["OpSpecRev2"] = opSpecRev2.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "MergeJobOpsBasedOnOpSpecRev",
                    Args = args,
                    When = DateTime.Now
                });
            }
            else
            {
                throw new Exception("One or both of the op spec revisions doesn't exist in the database");
            }
        }

        public void CloneJobOpsBasedOnOpSpecRev(Guid sourceOpSpecRev, Guid targetOpSpecRev, bool additive)
        {
            if (db.OpSpecRevs.Contains(sourceOpSpecRev) && db.OpSpecRevs.Contains(targetOpSpecRev)) // if both op spec revisions exist in the database
            {
                if (!additive)
                {
                    db.OpSpecRevRefToOpRefs[targetOpSpecRev] = db.OpSpecRevRefToOpRefs[sourceOpSpecRev]; // replace the ops in the target op spec revision with the ops in the source op spec revision
                }
                else
                {
                    db.OpSpecRevRefToOpRefs[targetOpSpecRev] = db.Ops.Keys.Where(y => db.OpSpecRevRefToOpRefs[targetOpSpecRev].Contains(y) || db.OpSpecRevRefToOpRefs[sourceOpSpecRev].Contains(y)).ToList(); // merge the ops in the target op spec revision and the source op spec revision
                }

                var args = new Dictionary<string, string>(); // add the event
                args["SourceOpSpecRev"] = sourceOpSpecRev.ToString();
                args["TargetOpSpecRev"] = targetOpSpecRev.ToString();
                args["Additive"] = additive.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "CloneJobOpsBasedOnOpSpecRev",
                    Args = args,
                    When = DateTime.Now
                });
            }
            else
            {
                throw new Exception("One or both of the op spec revisions doesn't exist in the database");
            }
        }

        public void LinkOpSpecRevToJobOp(Guid opSpecRev, int opId)
        {
            if (db.Ops.ContainsKey(opId)) // if the op exists in the database
            {
                if (!db.OpRefToOpSpecRevRefs[opId].Contains(opSpecRev)) // if the op spec revision isn't already linked to the op
                {
                    db.OpRefToOpSpecRevRefs[opId].Add(opSpecRev); // link the op spec revision to the op

                    var args = new Dictionary<string, string>(); // add the event
                    args["OpSpecRev"] = opSpecRev.ToString();
                    args["OpId"] = opId.ToString();
                    db.AuditLog.Add(new Event
                    {
                        Action = "LinkOpSpecRevToJobOp",
                        Args = args,
                        When = DateTime.Now
                    });
                }
                else
                {
                    throw new Exception("Op spec revision is already linked to the op");
                }
            }
            else
            {
                throw new Exception("The op doesn't exist in the database");
            }
        }

        public void UnlinkOpSpecRevFromJobOp(Guid opSpecRev, int opId)
        {
            if (db.Ops.ContainsKey(opId)) // if the op exists in the database
            {
                if (db.OpRefToOpSpecRevRefs[opId].Contains(opSpecRev)) // if the op spec revision is linked to the op
                {
                    db.OpRefToOpSpecRevRefs[opId].Remove(opSpecRev); // unlink the op spec revision from the op

                    var args = new Dictionary<string, string>(); // add the event
                    args["OpSpecRev"] = opSpecRev.ToString();
                    args["OpId"] = opId.ToString();
                    db.AuditLog.Add(new Event
                    {
                        Action = "UnlinkOpSpecRevFromJobOp",
                        Args = args,
                        When = DateTime.Now
                    });
                }
                else
                {
                    throw new Exception("Op spec revision isn't linked to the given op");
                }
            }
            else
            {
                throw new Exception("The op doesn't exist in the database");
            }
        }

        public void MergeOpSpecRevsBasedOnJobOp(int opId1, int opId2)
        {
            if (db.Ops.ContainsKey(opId1) && db.Ops.ContainsKey(opId2)) // if both of the ops exist in the database
            {
                List<Guid> mergedList = db.OpSpecRevs.Where(y => db.OpRefToOpSpecRevRefs[opId1].Contains(y) || db.OpRefToOpSpecRevRefs[opId2].Contains(y)).ToList(); // create a merged id list
                db.OpRefToOpSpecRevRefs[opId1] = mergedList; // put the merged list in both op references
                db.OpRefToOpSpecRevRefs[opId2] = mergedList;

                var args = new Dictionary<string, string>(); // add the event
                args["OpId1"] = opId1.ToString();
                args["OpId2"] = opId2.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "MergeOpSpecRevsBasedOnJobOp",
                    Args = args,
                    When = DateTime.Now
                });
            }
            else
            {
                throw new Exception("One or both of the ops doesn't exist in the database");
            }
        }

        public void CloneOpSpecRevsBasedOnJobOp(int sourceOp, int targetOp, bool additive)
        {
            if (db.Ops.ContainsKey(sourceOp) && db.Ops.ContainsKey(targetOp)) // if both ops exist in the database
            {
                if (!additive)
                {
                    db.OpRefToOpSpecRevRefs[targetOp] = db.OpRefToOpSpecRevRefs[sourceOp]; // replace the op spec revisions in the target op with the op spec revisions in the source op
                }
                else
                {
                    db.OpRefToOpSpecRevRefs[targetOp] = db.OpSpecRevs.Where(y => db.OpRefToOpSpecRevRefs[targetOp].Contains(y) || db.OpRefToOpSpecRevRefs[sourceOp].Contains(y)).ToList(); // merge the op spec revisions in the target op with teh op spec revisions in the source op
                }

                var args = new Dictionary<string, string>(); // add the event
                args["SourceOp"] = sourceOp.ToString();
                args["TargetOp"] = targetOp.ToString();
                args["Additive"] = additive.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "CloneOpSpecRevsBasedOnJobOp",
                    Args = args,
                    When = DateTime.Now
                });
            }
            else
            {
                throw new Exception("One or both of the ops doesn't exist in the database");
            }
        }

        public void MergeOpSpecRevsBasedOnOpSpec(Guid opSpec1, Guid opSpec2)
        {
            if (db.OpSpecRefToOpSpecRevRefs.ContainsKey(opSpec1) && db.OpSpecRefToOpSpecRevRefs.ContainsKey(opSpec2)) // if both of the op specs exist in the database
            {
                List<Guid> mergedIdList = db.OpSpecRevs.Where(y => db.OpSpecRefToOpSpecRevRefs[opSpec1].Contains(y) || db.QualityClauseRefToQualityClauseRevRefs[opSpec2].Contains(y)).ToList(); // create a merged id list and object list
                List<OpSpec> mergedOpSpecRevList = mergedIdList.Select(y => db.OpSpecs.First(x => x.Value.Any(z => mergedIdList.Contains(y))).Value.First(x => mergedIdList.Contains(y))).ToList();
                Guid groupId1 = db.OpSpecs.First(y => y.Value[0].Id == opSpec1).Key;
                Guid groupId2 = db.OpSpecs.First(y => y.Value[0].Id == opSpec2).Key;
                db.OpSpecs[groupId1] = new List<OpSpec> { db.OpSpecs[groupId1][0] }; // add merged revisions to database
                db.OpSpecs[groupId1].AddRange(mergedOpSpecRevList);
                db.OpSpecs[groupId2] = new List<OpSpec> { db.OpSpecs[groupId2][0] };
                db.OpSpecs[groupId2].AddRange(mergedOpSpecRevList);
                db.OpSpecs[groupId1] = db.OpSpecs[groupId1].Select(y => { y.IdRevGroup = groupId1; y.RevSeq = db.OpSpecs[groupId1].IndexOf(y); return y; }).ToList(); // reconfigure the revisions
                db.OpSpecs[groupId2] = db.OpSpecs[groupId2].Select(y => { y.IdRevGroup = groupId2; y.RevSeq = db.OpSpecs[groupId2].IndexOf(y); return y; }).ToList();
                db.OpSpecRefToOpSpecRevRefs[opSpec1] = mergedIdList; // manage references
                db.OpSpecRefToOpSpecRevRefs[opSpec2] = mergedIdList;

                var args = new Dictionary<string, string>(); // add the event
                args["OpSpec1"] = opSpec1.ToString();
                args["OpSpec2"] = opSpec2.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "MergeOpSpecRevsBasedOnOpSpec",
                    Args = args,
                    When = DateTime.Now
                });
            }
            else
            {
                throw new Exception("One or both of the op specs doesn't exist in the database");
            }
        }

        public void SplitOpSpecRevInOpSpec(Guid opSpecRev, Guid opSpec)
        {
            if (db.OpSpecRefToOpSpecRevRefs.ContainsKey(opSpec)) // if the op spec exists in the database
            {
                if (db.OpSpecRefToOpSpecRevRefs[opSpec].Contains(opSpecRev)) // if the op spec has the revision
                {
                    Guid revGroup = db.OpSpecs.First(y => y.Value[0].Id == opSpec).Key;
                    int newRevPosition = db.OpSpecs[revGroup].Count;
                    db.OpSpecs[revGroup].Add(db.OpSpecs[revGroup].First(y => y.Id == opSpecRev)); // split the revision in the database
                    db.OpSpecs[revGroup][newRevPosition].Id = Guid.NewGuid(); // configure the new revision
                    db.OpSpecs[revGroup][newRevPosition].RevSeq = newRevPosition;
                    db.OpSpecRevs.Add(db.OpSpecs[revGroup][newRevPosition].Id); // add the new revision to the database
                    db.OpSpecRefToOpSpecRevRefs[opSpec].Add(opSpecRev); // manage references
                    db.OpSpecRevRefToOpRefs[db.OpSpecs[revGroup][newRevPosition].Id] = db.OpSpecRevRefToOpRefs[opSpecRev];

                    var args = new Dictionary<string, string>(); // add the event
                    args["OpSpecRev"] = opSpecRev.ToString();
                    args["OpSpec"] = opSpec.ToString();
                    db.AuditLog.Add(new Event
                    {
                        Action = "SplitOpSpecRevInOpSpec",
                        Args = args,
                        When = DateTime.Now
                    });
                }
                else
                {
                    throw new Exception("Op spec revision doesn't have an association with the given op spec");
                }
            }
            else
            {
                throw new Exception("The op spec doesn't exist in the database");
            }
        }

        public void CloneOpSpecRevsBasedOnOpSpec(Guid sourceOpSpec, Guid targetOpSpec, bool additive)
        {
            if (db.OpSpecRefToOpSpecRevRefs.ContainsKey(sourceOpSpec) && db.OpSpecRefToOpSpecRevRefs.ContainsKey(targetOpSpec)) // if both op specs exist in the database
            {
                Guid targetRevGroup = db.OpSpecs.First(y => y.Value[0].Id == targetOpSpec).Key;
                Guid sourceRevGroup = db.OpSpecs.First(y => y.Value[0].Id == sourceOpSpec).Key;
                if (!additive)
                {
                    db.OpSpecs[targetRevGroup] = new List<OpSpec> { db.OpSpecs[targetRevGroup][0] }; // replace the revisions in the target op spec with the revisions in the source op spec
                    db.OpSpecs[targetRevGroup].AddRange(db.OpSpecs[sourceRevGroup].Where(y => db.OpSpecs[sourceRevGroup].IndexOf(y) != 0));
                    db.OpSpecs[targetRevGroup] = db.OpSpecs[targetRevGroup].Select(y => { y.IdRevGroup = targetRevGroup; return y; }).ToList(); // reconfigure the revisions
                    db.OpSpecRefToOpSpecRevRefs[targetOpSpec] = db.OpSpecRefToOpSpecRevRefs[sourceOpSpec]; // manage references
                }
                else
                {
                    List<Guid> mergedIdList = db.OpSpecRevs.Where(y => db.OpSpecRefToOpSpecRevRefs[targetOpSpec].Contains(y) || db.OpSpecRefToOpSpecRevRefs[sourceOpSpec].Contains(y)).ToList(); // create merged id list and object list
                    List<OpSpec> mergedOpSpecList = mergedIdList.Select(y => db.OpSpecs.Values.First(x => x.Any(z => z.Id == y)).First(x => x.Id == y)).ToList();
                    db.OpSpecs[targetRevGroup] = new List<OpSpec> { db.OpSpecs[targetRevGroup][0] }; // merge the revisions in the target op spec with the revisions in the source op spec
                    db.OpSpecs[targetRevGroup].AddRange(mergedOpSpecList);
                    db.OpSpecs[targetRevGroup] = db.OpSpecs[targetRevGroup].Select(y => { y.IdRevGroup = targetRevGroup; y.RevSeq = db.OpSpecs[targetRevGroup].IndexOf(y); return y; }).ToList(); // reconfigure the revisions
                    db.OpSpecRefToOpSpecRevRefs[targetOpSpec] = mergedIdList; // manage references
                }

                var args = new Dictionary<string, string>(); // add the event
                args["SourceOpSpec"] = sourceOpSpec.ToString();
                args["TargetOpSpec"] = targetOpSpec.ToString();
                args["Additive"] = additive.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "CloneOpSpecRevsBasedOnOpSpec",
                    Args = args,
                    When = DateTime.Now
                });
            }
            else
            {
                throw new Exception("One or both of the op specs doesn't exist in the database");
            }
        }

        public void LinkWorkInstructionToJobOp(Guid workInstruction, int opId)
        {
            if (db.Ops.ContainsKey(opId)) // if the op exists in the database
            {
                if (db.WorkInstructionRefToWorkInstructionRevRefs.ContainsKey(workInstruction)) // if the work instruction exists in the database
                {
                    if (!db.OpRefToWorkInstructionRef.ContainsKey(opId)) // if op isn't already linked to a work instruction
                    {
                        Guid revGroup = db.WorkInstructions.First(y => y.Value[0].Id == workInstruction).Key;
                        db.WorkInstructions[revGroup] = db.WorkInstructions[revGroup].Select(y => { y.OpId = opId; return y; }).ToList(); // link the work instruction to the op
                        db.OpRefToWorkInstructionRef[opId] = workInstruction; // manage references

                        var args = new Dictionary<string, string>(); // add the event
                        args["WorkInstruction"] = workInstruction.ToString();
                        args["OpId"] = opId.ToString();
                        db.AuditLog.Add(new Event
                        {
                            Action = "LinkWorkInstructionToJobOp",
                            Args = args,
                            When = DateTime.Now
                        });
                    }
                    else
                    {
                        throw new Exception("Op is already linked to a work instruction");
                    }
                }
                else
                {
                    throw new Exception("The work instruction doesn't exist in the database");
                }
            }
            else
            {
                throw new Exception("The op doesn't exist in the database");
            }
        }

        public void UnlinkWorkInstructionFromJobOp(Guid workInstruction, int opId)
        {
            if (db.Ops.ContainsKey(opId)) // if the op exists in the database
            {
                if (db.WorkInstructionRefToWorkInstructionRevRefs.ContainsKey(workInstruction)) // if the work instruction exists in the database
                {
                    if (db.OpRefToWorkInstructionRef[opId] == workInstruction) // if the work instruction is linked to the op
                    {
                        Guid revGroup = db.WorkInstructions.First(y => y.Value[0].Id == workInstruction).Key;
                        db.WorkInstructions[revGroup] = db.WorkInstructions[revGroup].Select(y => { y.OpId = -1; return y; }).ToList(); // unlink the work instruction from the op
                        db.OpRefToWorkInstructionRef.Remove(opId); // manage references

                        var args = new Dictionary<string, string>(); // add the event
                        args["WorkInstruction"] = workInstruction.ToString();
                        args["OpId"] = opId.ToString();
                        db.AuditLog.Add(new Event
                        {
                            Action = "UnlinkWorkInstructionFromJobOp",
                            Args = args,
                            When = DateTime.Now
                        });
                    }
                    else
                    {
                        throw new Exception("Work instruction isn't linked to the op");
                    }
                }
                else
                {
                    throw new Exception("The work instruction doesn't exist in the database");
                }
            }
            else
            {
                throw new Exception("The op doesn't exist in the database");
            }
        }

        public void MergeWorkInstructionRevs(Guid workInstruction1, Guid workInstruction2)
        {
            if (db.WorkInstructionRefToWorkInstructionRevRefs.ContainsKey(workInstruction1) && db.WorkInstructionRefToWorkInstructionRevRefs.ContainsKey(workInstruction2)) // if both work instructions exist in the database
            {
                List<Guid> mergedIdList = db.WorkInstructionRevs.Where(y => db.WorkInstructionRefToWorkInstructionRevRefs[workInstruction1].Contains(y) || db.WorkInstructionRefToWorkInstructionRevRefs[workInstruction2].Contains(y)).ToList(); // create a merged id list and object list
                List<WorkInstruction> mergedWorkInstructionList = mergedIdList.Select(y => db.WorkInstructions.First(x => x.Value.Any(z => mergedIdList.Contains(y))).Value.First(x => mergedIdList.Contains(y))).ToList();
                Guid groupId1 = db.WorkInstructions.First(y => y.Value[0].Id == workInstruction1).Key;
                Guid groupId2 = db.WorkInstructions.First(y => y.Value[0].Id == workInstruction2).Key;
                db.WorkInstructions[groupId1] = new List<WorkInstruction> { db.WorkInstructions[groupId1][0] }; // add the merged list to the database
                db.WorkInstructions[groupId1].AddRange(mergedWorkInstructionList);
                db.WorkInstructions[groupId2] = new List<WorkInstruction> { db.WorkInstructions[groupId2][0] };
                db.WorkInstructions[groupId2].AddRange(mergedWorkInstructionList);
                db.WorkInstructions[groupId1] = db.WorkInstructions[groupId1].Select(y => { y.IdRevGroup = groupId1; y.RevSeq = db.WorkInstructions[groupId1].IndexOf(y); return y; }).ToList(); // reconfigure the revisions
                db.WorkInstructions[groupId2] = db.WorkInstructions[groupId2].Select(y => { y.IdRevGroup = groupId2; y.RevSeq = db.WorkInstructions[groupId2].IndexOf(y); return y; }).ToList();
                db.WorkInstructionRefToWorkInstructionRevRefs[workInstruction1] = mergedIdList; // manage references
                db.WorkInstructionRefToWorkInstructionRevRefs[workInstruction2] = mergedIdList;

                var args = new Dictionary<string, string>(); // add the event
                args["WorkInstruction1"] = workInstruction1.ToString();
                args["WorkInstruction2"] = workInstruction2.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "MergeWorkInstructionRevs",
                    Args = args,
                    When = DateTime.Now
                });
            }
            else
            {
                throw new Exception("One or both of the work instructions doesn't exist in the database");
            }
        }

        public void SplitWorkInstructionRev(Guid workInstructionRev, Guid workInstruction)
        {
            if (db.WorkInstructionRefToWorkInstructionRevRefs.ContainsKey(workInstruction)) // if work instruction exists in the database
            {
                if (db.WorkInstructionRefToWorkInstructionRevRefs[workInstruction].Contains(workInstructionRev)) // if the work instruction has the revision
                {
                    Guid revGroup = db.WorkInstructions.First(y => y.Value[0].Id == workInstruction).Key;
                    int newRevPosition = db.WorkInstructions[revGroup].Count;
                    db.WorkInstructions[revGroup].Add(db.WorkInstructions[revGroup].First(y => y.Id == workInstructionRev)); // split the revision in the database
                    db.WorkInstructions[revGroup][newRevPosition].Id = Guid.NewGuid(); // configure the new revision
                    db.WorkInstructions[revGroup][newRevPosition].RevSeq = newRevPosition;
                    db.WorkInstructionRevs.Add(db.WorkInstructions[revGroup][newRevPosition].Id); // add the new revision to the database
                    db.WorkInstructionRefToWorkInstructionRevRefs[workInstruction].Add(db.WorkInstructions[revGroup][newRevPosition].Id); // manage references

                    var args = new Dictionary<string, string>(); // add the event
                    args["WorkInstructionRev"] = workInstructionRev.ToString();
                    args["WorkInstruction"] = workInstruction.ToString();
                    db.AuditLog.Add(new Event
                    {
                        Action = "SplitWorkInstructionRev",
                        Args = args,
                        When = DateTime.Now
                    });
                }
                else
                {
                    throw new Exception("Work instruction revision doesn't have an association with the given work instruction");
                }
            }
            else
            {
                throw new Exception("The work instruction doesn't exist in the database");
            }
        }

        public void CloneWorkInstructionRevs(Guid sourceWorkInstruction, Guid targetWorkInstruction, bool additive)
        {
            if (db.WorkInstructionRefToWorkInstructionRevRefs.ContainsKey(sourceWorkInstruction) && db.WorkInstructionRefToWorkInstructionRevRefs.ContainsKey(targetWorkInstruction)) // if both work instructions exist in the database
            {
                Guid targetRevGroup = db.OpSpecs.First(y => y.Value[0].Id == targetWorkInstruction).Key;
                Guid sourceRevGroup = db.OpSpecs.First(y => y.Value[0].Id == sourceWorkInstruction).Key;
                if (!additive)
                {
                    db.WorkInstructions[targetRevGroup] = new List<WorkInstruction> { db.WorkInstructions[targetRevGroup][0] }; // replace the revisions in the target work instruction with the revisions in the source work instruction
                    db.WorkInstructions[targetRevGroup].AddRange(db.WorkInstructions[sourceRevGroup].Where(y => db.WorkInstructions[sourceRevGroup].IndexOf(y) != 0));
                    db.WorkInstructions[targetRevGroup] = db.WorkInstructions[targetRevGroup].Select(y => { y.IdRevGroup = targetRevGroup; y.RevSeq = db.WorkInstructions[targetRevGroup].IndexOf(y); return y; }).ToList(); // reconfigure the revisions
                    db.WorkInstructionRefToWorkInstructionRevRefs[targetWorkInstruction] = db.WorkInstructionRefToWorkInstructionRevRefs[sourceWorkInstruction]; // manage references
                }
                else
                {
                    List<Guid> mergedIdList = db.WorkInstructionRevs.Where(y => db.WorkInstructionRefToWorkInstructionRevRefs[sourceWorkInstruction].Contains(y) || db.WorkInstructionRefToWorkInstructionRevRefs[targetWorkInstruction].Contains(y)).ToList();
                    List<WorkInstruction> mergedWorkInstructionList = mergedIdList.Select(y => db.WorkInstructions.Values.First(x => x.Any(z => z.Id == y)).First(x => x.Id == y)).ToList();
                    db.WorkInstructions[targetRevGroup] = new List<WorkInstruction> { db.WorkInstructions[targetRevGroup][0] }; // merge the revisions in the target work instruction with the revisions in the source work instruction
                    db.WorkInstructions[targetRevGroup].AddRange(mergedWorkInstructionList);
                    db.WorkInstructions[targetRevGroup] = db.WorkInstructions[targetRevGroup].Select(y => { y.IdRevGroup = targetRevGroup; y.RevSeq = db.WorkInstructions[targetRevGroup].IndexOf(y); return y; }).ToList(); // reconfigure the revisions
                    db.WorkInstructionRefToWorkInstructionRevRefs[targetWorkInstruction] = mergedIdList; // manage references
                }

                var args = new Dictionary<string, string>(); // add the event
                args["SourceWorkInstruction"] = sourceWorkInstruction.ToString();
                args["TargetWorkInstruction"] = targetWorkInstruction.ToString();
                args["Additive"] = additive.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "CloneWorkInstructionRevs",
                    Args = args,
                    When = DateTime.Now
                });
            }
            else
            {
                throw new Exception("One or both of the work instructions doesn't exist in the database");
            }
        }

        public List<QualityClause> PullQualityClausesFromJob(string jobId, string customerRev, string internalRev) =>
        db.Jobs[jobId + "-" + customerRev].First(y => y.RevPlan == internalRev).QualityClauses;

        public List<WorkInstruction> DisplayPriorRevisionsOfWorkInstruction(Guid workInstruction)
        {
            if (db.WorkInstructionRefToWorkInstructionRevRefs.ContainsKey(workInstruction)) // if the work instruction exists in the database
            {
                return db.WorkInstructions[db.WorkInstructions.First(y=> y.Value[0].Id == workInstruction).Key];
            }
            else
            {
                throw new Exception("Work instruction doesn't exist in the database");
            }
        }

        public List<QualityClause> DisplayPriorRevisionsOfQualityClauses(Guid qualityClause)
        {
            if (db.QualityClauseRefToQualityClauseRevRefs.ContainsKey(qualityClause)) // if the quality clause exists in the database
            {
                return db.QualityClauses[db.QualityClauses.First(y => y.Value[0].Id == qualityClause).Key];
            }
            else
            {
                throw new Exception("Quality clause doesn't exist in the database");
            }
        }

        public List<OpSpec> DisplayPriorRevisionsOfSpecs(Guid opSpec)
        {
            if (db.OpSpecRefToOpSpecRevRefs.ContainsKey(opSpec)) // if the op spec exists in the database
            {
                return db.OpSpecs[db.OpSpecs.First(y => y.Value[0].Id == opSpec).Key];
            }
            else
            {
                throw new Exception("Op spec doesn't exist in the database");
            }
        }

        public WorkInstruction DisplayLatestRevisionOfWorkInstruction(string jobId, string jobRev, int opId)
        {
            if (db.Jobs.ContainsKey(jobId)) // if the job exists in the database
            {
                if (db.JobRefToJobRevRefs[jobId].Contains(jobRev)) // if the job has the revision
                {
                    if (db.JobRevRefToOpRefs[jobRev].Contains(opId)) // if the job revision has the op
                    {
                        return db.WorkInstructions.Values.First(y => y.Last().Id == db.WorkInstructionRefToWorkInstructionRevRefs[db.OpRefToWorkInstructionRef[opId]].Last()).Last();
                    }
                    else
                    {
                        throw new Exception("Job revision doesn't have the op");
                    }
                }
                else
                {
                    throw new Exception("Job doesn't have the revision");
                }
            }
            else
            {
                throw new Exception("The job doesn't exist in the database");
            }
        }

    }
}
