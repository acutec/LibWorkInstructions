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
        /// <summary>
        /// Create Job in the database if it doesn't exist.
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="revCustomer"></param>
        /// <param name="revPlan"></param>
        /// <param name="rev"></param>
        public void CreateJob(string jobId, string revCustomer, string revPlan, string rev)
        {
            if (!db.Jobs.ContainsKey(jobId)) // if the job doesn't already exist in the database
            {
                Job job = new Job { Id = jobId, RevSeq = 0, RevCustomer = revCustomer, Rev = rev}; // create and configure it
                db.Jobs[job.Id] = new List<Job> { job }; // add the job to the database
                db.JobRevs.Add(rev);
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
        /// <summary>
        /// Remove Job from database if it exists.
        /// </summary>
        /// <param name="jobId"></param>
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
        /// <summary>
        /// Create JobRev if it doesn't already exist for the given parameters.
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="sourceJobRev"></param>
        /// <param name="newJobRev"></param>
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
        /// <summary>
        /// Create JobRev if it doesn't alreayd exist, given a Job object.
        /// </summary>
        /// <param name="newJobRev"></param>
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
        /// <summary>
        /// Update JobRev in database to given JobRev from parameter if it exists, or is not already changed.
        /// </summary>
        /// <param name="newJobRev"></param>
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
        /// <summary>
        /// Change active status in JobRev to False if it exists.
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="jobRev"></param>
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
        /// <summary>
        /// Change active status in JobRev to True if it exists.
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="jobRev"></param>
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
        /// <summary>
        /// Create QualityClause in database if it doesn't exist.
        /// </summary>
        /// <param name="clause"></param>
        public void CreateQualityClause(string clause)
        {
            if (db.QualityClauses.Values.Any(y => y.Any(x => x.Clause == clause)))
            {
                throw new Exception("Quality clause is already in the database");
            }
            QualityClause newQualityClause = new QualityClause { Id = Guid.NewGuid(), IdRevGroup = Guid.NewGuid(), RevSeq = 0, Clause = clause }; // create and configure the quality clause
            db.QualityClauses[newQualityClause.IdRevGroup] =  new List<QualityClause> { newQualityClause }; // add quality clause to database
            db.QualityClauseRevs.Add(newQualityClause.Id);
            db.QualityClauseRefToQualityClauseRevRefs[newQualityClause.IdRevGroup] = new List<Guid> { newQualityClause.Id }; // manage references

            var args = new Dictionary<string, string>(); // add the event
            args["Clause"] = clause;
            db.AuditLog.Add(new Event
            {
                Action = "CreateQualityClause",
                Args = args,
                When = DateTime.Now,
            });
        }
        /// <summary>
        /// Change active status of the quality clause to True if it exists.
        /// </summary>
        /// <param name="clauseId"></param>
        public void ActivateQualityClause(Guid revGroup)
        {
            if (db.QualityClauses.ContainsKey(revGroup)) // if the rev group exists in the database
            {
                db.QualityClauses[revGroup] = db.QualityClauses[revGroup].Select(y => { y.Active = true; return y; }).ToList();

                var args = new Dictionary<string, string>(); // add the event
                args["RevGroup"] = revGroup.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "ActivateQualityClause",
                    Args = args,
                    When = DateTime.Now
                });
            }
            else
            {
                throw new Exception("The rev group doesn't exist in the database");
            }
        }
        /// <summary>
        /// Change active status of the quality clause to False if it exists.
        /// </summary>
        /// <param name="clauseId"></param>
        public void DeactivateQualityClause(Guid revGroup)
        {
            if (db.QualityClauses.ContainsKey(revGroup)) // if the rev group exists in the database
            {
                db.QualityClauses[revGroup] = db.QualityClauses[revGroup].Select(y => { y.Active = false; return y; }).ToList();

                var args = new Dictionary<string, string>(); // add the event
                args["RevGroup"] = revGroup.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "DeactivateQualityClause",
                    Args = args,
                    When = DateTime.Now
                });
            }
            else
            {
                throw new Exception("The rev group doesn't exist in the database");
            }
        }
        /// <summary>
        /// Create QualityClauseRev in database from given parameters if it doesn't already exist.
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="sourceClauseRev"></param>
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
        /// <summary>
        /// Create QualityClauseRev in database from QualityClause object, if it doesn't exist.
        /// </summary>
        /// <param name="newClauseRev"></param>
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
        /// <summary>
        /// Update QualityClauseRev in database, if it exists and is not already changed.
        /// </summary>
        /// <param name="newClauseRev"></param>
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
        /// <summary>
        /// Activate QualityClauseRev, if it exists.
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="qualityClauseRev"></param>
        public void ActivateQualityClauseRev(Guid groupId, Guid qualityClauseRev)
        {
            if (db.QualityClauses.ContainsKey(groupId)) // if the rev group exists with regard to quality clauses
            {
                if (db.QualityClauses[groupId].Any(y => y.Id == qualityClauseRev)) // if the quality clause revision is in the rev group
                {
                    db.QualityClauses[groupId][db.QualityClauses[groupId].FindIndex(y => y.Id == qualityClauseRev)].Active = true; // activate the revision

                    var args = new Dictionary<string, string>(); // add the event
                    args["GroupId"] = groupId.ToString();
                    args["QualityClauseRev"] = qualityClauseRev.ToString();
                    db.AuditLog.Add(new Event
                    {
                        Action = "ActivateQualityClauseRev",
                        Args = args,
                        When = DateTime.Now
                    });
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
        /// <summary>
        /// Deactivate QualityClauseRev, if it exists.
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="qualityClauseRev"></param>
        public void DeactivateQualityClauseRev(Guid groupId, Guid qualityClauseRev)
        {
            if (db.QualityClauses.ContainsKey(groupId)) // if the rev group exists with regard to quality clauses
            {
                if (db.QualityClauses[groupId].Any(y => y.Id == qualityClauseRev)) // if the quality clause revision is in the rev group
                {
                    db.QualityClauses[groupId][db.QualityClauses[groupId].FindIndex(y => y.Id == qualityClauseRev)].Active = false; // deactivate the revision

                    var args = new Dictionary<string, string>(); // add the event
                    args["GroupId"] = groupId.ToString();
                    args["QualityClauseRev"] = qualityClauseRev.ToString();
                    db.AuditLog.Add(new Event
                    {
                        Action = "DeactivateQualityClauseRev",
                        Args = args,
                        When = DateTime.Now
                    });
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
        /// <summary>
        /// Create JobOp in database if it doesn't exist.
        /// </summary>
        /// <param name="op"></param>
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
        /// <summary>
        /// Remove JobOp from database if it exists.
        /// </summary>
        /// <param name="opId"></param>
        public void DeleteJobOp(string jobRev, int opId)
        {
            if (db.Ops.ContainsKey(opId)) // if the op is in the database
            {
                if (db.JobRevs.Contains(jobRev)) // if the job revision is in the database
                {
                    var revGroup = db.Jobs.First(y => y.Value.Any(x => x.Rev == jobRev));
                    db.Jobs[revGroup.Key][revGroup.Value.FindIndex(y => y.Rev == jobRev)].Ops = db.Jobs[revGroup.Key][revGroup.Value.FindIndex(y => y.Rev == jobRev)].Ops.Where(y => y.Id != opId).ToList(); // remove the op from the job revision
                    db.JobRevRefToOpRefs[jobRev] = db.JobRevRefToOpRefs[jobRev].Where(y => y != opId).ToList(); // manage references

                    var args = new Dictionary<string, string>(); // add the event
                    args["JobRev"] = jobRev;
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
                    throw new Exception("The job revision doesn't exist in the database");
                }
            }
            else
            {
                throw new Exception("The job op doesn't exist in the database");
            }
        }
        /// <summary>
        /// Create OpSpec in database if it doesn't exist.
        /// </summary>
        /// <param name="newSpec"></param>
        public void CreateOpSpec(OpSpec newSpec)
        {
            if (!db.OpSpecs.Values.Any(y => y.Any(x => x.Name == newSpec.Name))) // if the name of the opspec is not already in the database
            {
                newSpec.RevSeq = 0; // configure the op spec
                newSpec.Id = Guid.NewGuid();
                newSpec.IdRevGroup = Guid.NewGuid();
                db.OpSpecs[newSpec.IdRevGroup] = new List<OpSpec> { newSpec }; // add the op spec to the database
                db.OpSpecRevs.Add(newSpec.Id);
                db.OpSpecRefToOpSpecRevRefs[newSpec.IdRevGroup] = new List<Guid> { newSpec.Id }; // manage references

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
                throw new Exception("An op spec with the same name is already in the database");
            }
        }
        /// <summary>
        /// Change active status of OpSpec to True if it exists.
        /// </summary>
        /// <param name="revGroup"></param>
        public void ActivateOpSpec(Guid revGroup)
        {
            if (db.OpSpecs.ContainsKey(revGroup)) // if the rev group exists in the database
            {
                db.OpSpecs[revGroup] = db.OpSpecs[revGroup].Select(y => { y.Active = true; return y; }).ToList();

                var args = new Dictionary<string, string>(); // add the events
                args["RevGroup"] = revGroup.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "ActivateOpSpec",
                    Args = args,
                    When = DateTime.Now,
                });
            }
            else
            {
                throw new Exception("The rev group doesn't exist in the database");
            }
        }
        /// <summary>
        /// Change active status of OpSpec to False if it exists.
        /// </summary>
        /// <param name="revGroup"></param>
        public void DeactivateOpSpec(Guid revGroup)
        {
            if (db.OpSpecs.ContainsKey(revGroup)) // if the rev group exists in the database
            {
                db.OpSpecs[revGroup] = db.OpSpecs[revGroup].Select(y => { y.Active = false; return y; }).ToList();

                var args = new Dictionary<string, string>(); // add the events
                args["RevGroup"] = revGroup.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "DeactivateOpSpec",
                    Args = args,
                    When = DateTime.Now,
                });
            }
            else
            {
                throw new Exception("The rev group doesn't exist in the database");
            }
        }
        /// <summary>
        /// Create OpSpecRev in database from the given parameters if it doesn't already exist.
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="sourceSpecRev"></param>
        public void CreateOpSpecRev(Guid groupId, Guid sourceSpecRev, string name)
        {
            if (db.OpSpecs.ContainsKey(groupId)) // if the rev group exists with regard to op specs
            {
                if (db.OpSpecs[groupId].Any(y => y.Id == sourceSpecRev)) // if the source op spec revision is in the rev group
                {
                    OpSpec opSpec = db.OpSpecs[groupId].First(y => y.Id == sourceSpecRev); // configure the op spec revision
                    Guid sourceId = opSpec.Id;
                    opSpec.Id = Guid.NewGuid();
                    opSpec.RevSeq = db.OpSpecs[groupId].Count;
                    opSpec.Name = name;
                    db.OpSpecs[groupId].Add(opSpec); // add the op spec revision to the database
                    db.OpSpecRevs.Add(opSpec.Id);
                    db.OpSpecRevRefToOpRefs[opSpec.Id] = db.OpSpecRevRefToOpRefs[sourceId]; // manage references
                    db.OpSpecRefToOpSpecRevRefs[groupId].Add(opSpec.Id);

                    var args = new Dictionary<string, string>(); // add the event
                    args["GroupId"] = groupId.ToString();
                    args["SourceSpecRev"] = sourceSpecRev.ToString();
                    args["Name"] = name;
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
        /// <summary>
        /// Create OpSpecRev in database if it doesn't aleady exist.
        /// </summary>
        /// <param name="newSpecRev"></param>
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
                    db.OpSpecRefToOpSpecRevRefs[newSpecRev.IdRevGroup].Add(newSpecRev.Id);

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
        /// <summary>
        /// Update OpSpecRev in database if it exists, and if it is not already changed.
        /// </summary>
        /// <param name="newSpecRev"></param>
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
        /// <summary>
        /// Activate OpSpecRev if it exists.
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="specRev"></param>
        public void ActivateOpSpecRev(Guid groupId, Guid specRev)
        {
            if (db.OpSpecs.ContainsKey(groupId)) // if the rev group exists with regard to op specs
            {
                if (db.OpSpecs[groupId].Any(y => y.Id == specRev)) // if the op spec revision is in the rev group
                {
                    db.OpSpecs[groupId][db.OpSpecs[groupId].FindIndex(y => y.Id == specRev)].Active = true; // activate the op spec revision

                    var args = new Dictionary<string, string>(); // add the event
                    args["GroupId"] = groupId.ToString();
                    args["SpecRev"] = specRev.ToString();
                    db.AuditLog.Add(new Event
                    {
                        Action = "ActivateOpSpecRev",
                        Args = args,
                        When = DateTime.Now
                    });
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
        /// <summary>
        /// Deactivate OpSpecRev if it exists.
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="specRev"></param>
        public void DeactivateOpSpecRev(Guid groupId, Guid specRev)
        {
            if (db.OpSpecs.ContainsKey(groupId)) // if the rev group exists with regard to op specs
            {
                if (db.OpSpecs[groupId].Any(y => y.Id == specRev)) // if the op spec revision is in the rev group
                {
                    db.OpSpecs[groupId][db.OpSpecs[groupId].FindIndex(y => y.Id == specRev)].Active = false; // deactivate the op spec revision

                    var args = new Dictionary<string, string>(); // add the event
                    args["GroupId"] = groupId.ToString();
                    args["SpecRev"] = specRev.ToString();
                    db.AuditLog.Add(new Event
                    {
                        Action = "DeactivateOpSpecRev",
                        Args = args,
                        When = DateTime.Now
                    });
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
        /// <summary>
        /// Create WorkInstruction in database if it doesn't exist.
        /// </summary>
        /// <param name="op"></param>
        public void CreateWorkInstruction(int op)
        {
            if (!db.WorkInstructions.Values.Any(y => y.Any(x => x.OpId == op))) // if there isn't already a work instruction for that op
            {
                WorkInstruction workInstruction = new WorkInstruction { Id = Guid.NewGuid(), IdRevGroup = Guid.NewGuid(), OpId = op, RevSeq = 0 }; // create and configure the work instruction
                db.WorkInstructions[workInstruction.IdRevGroup] = new List<WorkInstruction> { workInstruction }; // add the work instruction to the database
                db.OpRefToWorkInstructionRef[workInstruction.OpId] = workInstruction.IdRevGroup; // manage references
                db.WorkInstructionRefToWorkInstructionRevRefs[workInstruction.IdRevGroup] =  new List<Guid> { workInstruction.Id };

                var args = new Dictionary<string, string>(); // add the event
                args["Op"] = JsonSerializer.Serialize(workInstruction);
                db.AuditLog.Add(new Event
                {
                    Action = "CreateWorkInstruction",
                    Args = args,
                    When = DateTime.Now,
                });
            }
            else
            {
                throw new Exception("The op already has a work instruction");
            }
        }
        /// <summary>
        /// Change active status of WorkInstruction to True if it exists.
        /// </summary>
        /// <param name="idRevGroup"></param>
        public void ActivateWorkInstruction(Guid idRevGroup)
        {
            if (db.WorkInstructions.ContainsKey(idRevGroup)) // if the rev group exists in the database
            {
                db.WorkInstructions[idRevGroup] = db.WorkInstructions[idRevGroup].Select(y => { y.Active = true; return y; }).ToList(); // activate the revisions of the work instruction

                var args = new Dictionary<string, string>(); // add the event
                args["IdRevGroup"] = idRevGroup.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "ActivateWorkInstruction",
                    Args = args,
                    When = DateTime.Now,
                });
            }
            else
            {
                throw new Exception("The rev group doesn't exist in the database");
            }
        }
        /// <summary>
        /// Change active status of WorkInstruction to False if it exists.
        /// </summary>
        /// <param name="idRevGroup"></param>
        public void DeactivateWorkInstruction(Guid idRevGroup)
        {
            if (db.WorkInstructions.ContainsKey(idRevGroup)) // if the rev group exists in the database
            {
                db.WorkInstructions[idRevGroup] = db.WorkInstructions[idRevGroup].Select(y => { y.Active = false; return y; }).ToList(); // deactivate the revisions of the work instruction

                var args = new Dictionary<string, string>(); // add the event
                args["IdRevGroup"] = idRevGroup.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "DeactivateWorkInstruction",
                    Args = args,
                    When = DateTime.Now,
                });
            }
            else
            {
                throw new Exception("The rev group doesn't exist in the database");
            }
        }
        /// <summary>
        /// Create WorkInstructionRev from given parameters if it doesn't exist.
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="sourceWorkInstructionRev"></param>
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
                    db.WorkInstructionRefToWorkInstructionRevRefs[workInstruction.IdRevGroup].Add(workInstruction.Id); // manage references

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
        /// <summary>
        /// Create WorkInstructionRev if it doesn't exist.
        /// </summary>
        /// <param name="newWorkInstructionRev"></param>
        public void CreateWorkInstructionRev(WorkInstruction newWorkInstructionRev)
        {
            if (!db.WorkInstructionRevs.Contains(newWorkInstructionRev.Id)) // if the work instruction revision isn't already in the database
            {
                if (db.WorkInstructions.ContainsKey(newWorkInstructionRev.IdRevGroup)) // if the rev group exists with regard to work instructions
                {
                    newWorkInstructionRev.RevSeq = db.WorkInstructions[newWorkInstructionRev.IdRevGroup].Count; // configure the work instruction revision
                    db.WorkInstructions[newWorkInstructionRev.IdRevGroup].Add(newWorkInstructionRev); // add the work instruction revision to the database
                    db.WorkInstructionRevs.Add(newWorkInstructionRev.Id);
                    db.WorkInstructionRefToWorkInstructionRevRefs[newWorkInstructionRev.IdRevGroup].Add(newWorkInstructionRev.Id); // manage references

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
        /// <summary>
        /// Update WorkInstructionRev if it exists, and isn't already changed.
        /// </summary>
        /// <param name="newWorkInstructionRev"></param>
        public void UpdateWorkInstructionRev(WorkInstruction newWorkInstructionRev)
        {
            if (db.WorkInstructions.ContainsKey(newWorkInstructionRev.IdRevGroup)) // if the rev group exists with regard to work instructions
            {
                if (db.WorkInstructions[newWorkInstructionRev.IdRevGroup].Any(y => y.Id == newWorkInstructionRev.Id)) // if the work instruction revision is in the rev group
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
                    throw new Exception("The work instruction revision being updated isn't in the rev group");
                }
            }
            else
            {
                throw new Exception("The rev group doesn't exist with regard to work instructions");
            }
        }
        /// <summary>
        /// Activate WorkInstructionRev if it exists.
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="workInstructionRev"></param>
        public void ActivateWorkInstructionRev(Guid groupId, Guid workInstructionRev)
        {
            if (db.WorkInstructions.ContainsKey(groupId)) // if the rev group exists with regard to work instructions
            {
                if (db.WorkInstructions[groupId].Any(y => y.Id == workInstructionRev)) // if the work instruction revision is in the rev group
                {
                    db.WorkInstructions[groupId][db.WorkInstructions[groupId].FindIndex(y => y.Id == workInstructionRev)].Active = true; // activate the work instruction revision

                    var args = new Dictionary<string, string>(); // add the event
                    args["GroupId"] = groupId.ToString();
                    args["WorkInstructionRev"] = workInstructionRev.ToString();
                    db.AuditLog.Add(new Event
                    {
                        Action = "ActivateWorkInstructionRev",
                        Args = args,
                        When = DateTime.Now
                    });
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
        /// <summary>
        /// Deactivate WorkInstructionRev if it exists.
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="workInstructionRev"></param>
        public void DeactivateWorkInstructionRev(Guid groupId, Guid workInstructionRev)
        {
            if (db.WorkInstructions.ContainsKey(groupId)) // if the rev group exists with regard to work instructions
            {
                if (db.WorkInstructions[groupId].Any(y => y.Id == workInstructionRev)) // if the work instruction revision is in the rev group
                {
                    if (db.WorkInstructions[groupId][0].Id != workInstructionRev) // if the id indeed refers to a revision, not an original work instruction
                    {
                        db.WorkInstructions[groupId][db.WorkInstructions[groupId].FindIndex(y => y.Id == workInstructionRev)].Active = false; // deactivate the work instruction revision

                        var args = new Dictionary<string, string>(); // add the event
                        args["GroupId"] = groupId.ToString();
                        args["WorkInstructionRev"] = workInstructionRev.ToString();
                        db.AuditLog.Add(new Event
                        {
                            Action = "DeactivateWorkInstructionRev",
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
        /// <summary>
        /// Merge two jobs so that they each have all of the revisions of both jobs.
        /// </summary>
        /// <param name="jobId1"></param>
        /// <param name="jobId2"></param>
        public void MergeJobs(string jobId1, string jobId2)
        {
            if(db.Jobs.ContainsKey(jobId1) && db.Jobs.ContainsKey(jobId2)) // if both jobs exist in the database
            {
                List<string> mergedIdList = db.JobRevs.Where(y => db.JobRefToJobRevRefs[jobId1].Contains(y) || db.JobRefToJobRevRefs[jobId2].Contains(y)).ToList(); // create a merged id list and object list
                List<Job> mergedJobRevList = mergedIdList.Select(y => db.Jobs.Values.First(x => x.Any(z => z.Rev == y)).First(x => x.Rev == y)).ToList();
                mergedJobRevList = mergedJobRevList.Select(y =>
                {
                    y.RevSeq = mergedJobRevList.IndexOf(y);
                    y.Rev = y.Rev.Replace(y.Rev[4], (char)(65 + y.RevSeq));
                    return y;
                }).ToList();
                mergedIdList = mergedIdList.Select(y => y = y.Replace(y[4], (char)(65 + mergedIdList.IndexOf(y)))).ToList();
                db.Jobs[jobId1] = mergedJobRevList; // add merged list to the jobs
                db.Jobs[jobId1] = db.Jobs[jobId1].Select(y => { y.Id = jobId1; return y; }).ToList();
                db.Jobs[jobId2] = mergedJobRevList;
                db.Jobs[jobId2] = db.Jobs[jobId2].Select(y => { y.Id = jobId2; return y; }).ToList();
                db.JobRefToJobRevRefs[jobId1] = mergedIdList; // manage references
                db.JobRefToJobRevRefs[jobId2] = mergedIdList;

                var args = new Dictionary<string, string>(); // add the event
                args["JobId1"] = jobId1;
                args["JobId2"] = jobId2;
                db.AuditLog.Add(new Event
                {
                    Action = "MergeJobs",
                    Args = args,
                    When = DateTime.Now,
                });
            }
            else
            {
                throw new Exception("One or both of the jobs doesn't exist in the database");
            }
        }
        /// <summary>
        /// Split JobRev within a Job into two objects if it exists.
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="jobRev"></param>
        /// <param name="newJobRev"></param>
        public void SplitJobRev(string jobId, string jobRev, string newJobRev)
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
        /// <summary>
        /// Clone JobRevs based on the given job if it exists.  
        /// Behavior changes based on additive parameter
        /// </summary>
        /// <param name="sourceJob"></param>
        /// <param name="targetJob"></param>
        /// <param name="additive"></param>
        public void CloneJobRevs(string sourceJob, string targetJob, bool additive)
        {
            if(db.Jobs.ContainsKey(sourceJob) && db.Jobs.ContainsKey(targetJob)) // if both of the jobs exist in the database
            {
                if(!additive)
                {
                    db.Jobs[targetJob] = db.Jobs[sourceJob]; // replace the target job revisions with the source job revisions
                    db.Jobs[targetJob] = db.Jobs[targetJob].Select(y => { y.Id = targetJob; y.Ops = y.Ops.Select(x => { x.JobId = y.Id; return x; }).ToList(); return y; }).ToList(); // reconfigure the revisions
                    db.JobRefToJobRevRefs[targetJob] = db.JobRefToJobRevRefs[sourceJob]; // manage references
                }
                else
                {
                    List<string> mergedIdList = db.JobRevs.Where(y => db.JobRefToJobRevRefs[sourceJob].Contains(y) || db.JobRefToJobRevRefs[targetJob].Contains(y)).ToList(); // create a merged id list and object list
                    List<Job> mergedJobRevList = mergedIdList.Select(y => db.Jobs.Values.First(x => x.Any(z => z.Rev == y)).First(x => x.Rev == y)).ToList();
                    mergedJobRevList = mergedJobRevList.Select(y => // reconfigure the job revisions
                    {
                        y.RevSeq = mergedJobRevList.IndexOf(y);
                        y.Rev = y.Rev.Replace(y.Rev[4], (char)(65 + y.RevSeq));
                        y.Ops = y.Ops.Select(x => { x.JobId = y.Id; return x; }).ToList();
                        return y;
                    }).ToList();
                    mergedIdList = mergedIdList.Select(y => y = y.Replace(y[4], (char)(65 + mergedIdList.IndexOf(y)))).ToList();
                    db.Jobs[targetJob] = mergedJobRevList; // add the merged revisions to the target job
                    db.Jobs[targetJob] = db.Jobs[targetJob].Select(y => { y.Id = targetJob;  return y; }).ToList(); // reconfigure the revisions
                    db.JobRefToJobRevRefs[targetJob] = mergedIdList; // manage references
                }

                var args = new Dictionary<string, string>(); // add the event
                args["SourceJob"] = sourceJob;
                args["TargetJob"] = targetJob;
                args["Additive"] = additive.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "CloneJobRevs",
                    Args = args,
                    When = DateTime.Now,
                });
            }
            else
            {
                throw new Exception("One or both of the jobs doesn't exist in the database");
            }
        }
        /// <summary>
        /// Link JobRev and given QualityClauseRev if they exist.
        /// </summary>
        /// <param name="jobRev"></param>
        /// <param name="qualityClauseRev"></param>
        public void LinkJobRevAndQualityClauseRev(string jobRev, Guid qualityClauseRev)
        {
            if (db.QualityClauseRevs.Contains(qualityClauseRev)) // if the quality clause revision exists in the database
            {
                if (db.JobRevs.Contains(jobRev)) // if the job revision exists in the database
                {
                    if (!db.QualityClauseRevRefToJobRevRefs[qualityClauseRev].Contains(jobRev)) // if the job revision isn't already linked to the quality clause revision
                    {
                        QualityClause clauseRev = db.QualityClauses.Values.First(y => y.Any(x => x.Id == qualityClauseRev)).First(y => y.Id == qualityClauseRev); // create new instance of quality clause revision
                        clauseRev.RevSeq = db.JobRevRefToQualityClauseRevRefs[jobRev].Count; // configure the quality clause revision
                        db.Jobs[db.Jobs.First(y => y.Value.Any(x => x.Rev == jobRev)).Key] // link the quality clause revision to the job revision
                            [db.Jobs.First(y => y.Value.Any(x => x.Rev == jobRev)).Value.FindIndex(y => y.Rev == jobRev)].QualityClauses.Add(clauseRev);
                        db.QualityClauseRevRefToJobRevRefs[qualityClauseRev].Add(jobRev); // link the job revision to the quality clause revision
                        db.JobRevRefToQualityClauseRevRefs[jobRev].Add(qualityClauseRev); // link the quality clause revision to the job revision

                        var args = new Dictionary<string, string>(); // add the event
                        args["JobRev"] = jobRev;
                        args["QualityClauseRev"] = qualityClauseRev.ToString();
                        db.AuditLog.Add(new Event
                        {
                            Action = "LinkJobRevAndQualityClauseRev",
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
        /// <summary>
        /// Unlink JobRev and given QualityClauseRev if they exist.
        /// </summary>
        /// <param name="jobRev"></param>
        /// <param name="qualityClauseRev"></param>
        public void UnlinkJobRevAndQualityClauseRev(string jobRev, Guid qualityClauseRev)
        {
            if (db.QualityClauseRevs.Contains(qualityClauseRev)) // if the quality clause revision exists in the database
            {
                if (db.QualityClauseRevRefToJobRevRefs[qualityClauseRev].Contains(jobRev)) // if the job revision is linked to the quality clause revision
                {
                    string jobRevKey = db.Jobs.First(y => y.Value.Any(x => x.Rev == jobRev)).Key; // unlink the quality clause revision from the job revision
                    int jobRevIndex = db.Jobs.First(y => y.Value.Any(x => x.Rev == jobRev)).Value.FindIndex(y => y.Rev == jobRev);
                    db.Jobs[jobRevKey][jobRevIndex].QualityClauses.Remove(db.QualityClauses.Values.First(y => y.Any(x => x.Id == qualityClauseRev)).First(y => y.Id == qualityClauseRev));
                    db.JobRevRefToQualityClauseRevRefs[jobRev].Remove(qualityClauseRev); // manage references
                    db.QualityClauseRevRefToJobRevRefs[qualityClauseRev].Remove(jobRev);

                    var args = new Dictionary<string, string>(); // add the event
                    args["JobRev"] = jobRev;
                    args["QualityClauseRev"] = qualityClauseRev.ToString();
                    db.AuditLog.Add(new Event
                    {
                        Action = "UnlinkJobRevAndQualityClauseRev",
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
        /// <summary>
        /// Merege QualityClauses within given JobRevs if they exist.
        /// </summary>
        /// <param name="jobRev1"></param>
        /// <param name="jobRev2"></param>
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
        /// <summary>
        /// Clone QualityClaseRevs in given JobRevs if they exist.
        /// Behavior changes depening on the additive parameter.
        /// </summary>
        /// <param name="sourceJobRev"></param>
        /// <param name="targetJobRev"></param>
        /// <param name="additive"></param>
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
        /// <summary>
        /// Merge quality clauses so that they each have all of the revisions from both quality clauses.
        /// </summary>
        /// <param name="clauseId1"></param>
        /// <param name="clauseId2"></param>
        public void MergeQualityClauses(Guid groupId1, Guid groupId2)
        {
            if (db.QualityClauseRefToQualityClauseRevRefs.ContainsKey(groupId1) && db.QualityClauseRefToQualityClauseRevRefs.ContainsKey(groupId2)) // if both of the rev groups exist in the database
            {
                List<Guid> mergedIdList = db.QualityClauseRevs.Where(y => db.QualityClauseRefToQualityClauseRevRefs[groupId1].Contains(y) || db.QualityClauseRefToQualityClauseRevRefs[groupId2].Contains(y)).ToList(); // create a merged id list and object list
                List<QualityClause> mergedClauseRevList = mergedIdList.Select(y => db.QualityClauses.First(x => x.Value.Any(z => mergedIdList.Contains(y))).Value.First(x => mergedIdList.Contains(y))).ToList();
                db.QualityClauses[groupId1] = mergedClauseRevList; // add merged revisions to database
                db.QualityClauses[groupId2] = mergedClauseRevList;
                db.QualityClauses[groupId1] = db.QualityClauses[groupId1].Select(y => { y.IdRevGroup = groupId1; y.RevSeq = db.QualityClauses[groupId1].IndexOf(y); return y; }).ToList(); // reconfigure the revisions
                db.QualityClauses[groupId2] = db.QualityClauses[groupId2].Select(y => { y.IdRevGroup = groupId2; y.RevSeq = db.QualityClauses[groupId2].IndexOf(y); return y; }).ToList();
                db.QualityClauseRefToQualityClauseRevRefs[groupId1] = mergedIdList; // manage references
                db.QualityClauseRefToQualityClauseRevRefs[groupId2] = mergedIdList;

                var args = new Dictionary<string, string>(); // add the event
                args["GroupId1"] = groupId1.ToString();
                args["GroupId2"] = groupId2.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "MergeQualityClauses",
                    Args = args,
                    When = DateTime.Now
                });
            }
            else
            {
                throw new Exception("One or both of the rev groups doesn't exist in the database");
            }
        }
        /// <summary>
        /// Split QualityClauseRev within given QualityClause if they exist.
        /// </summary>
        /// <param name="qualityClause"></param>
        /// <param name="qualityClauseRev"></param>
        public void SplitQualityClauseRev(Guid revGroup, Guid qualityClauseRev)
        {
            if (db.QualityClauseRefToQualityClauseRevRefs.ContainsKey(revGroup)) // if the clause exists in the database
            {
                if (db.QualityClauseRefToQualityClauseRevRefs[revGroup].Contains(qualityClauseRev)) // if the quality clause has the revision
                {
                    int newRevPosition = db.QualityClauses[revGroup].Count;
                    db.QualityClauses[revGroup].Add(db.QualityClauses[revGroup].First(y => y.Id == qualityClauseRev)); // split the revision in the database
                    db.QualityClauses[revGroup][newRevPosition].Id = Guid.NewGuid(); // reconfigure the revision
                    db.QualityClauses[revGroup][newRevPosition].RevSeq = newRevPosition;
                    db.QualityClauseRevs.Add(db.QualityClauses[revGroup][newRevPosition].Id); // add the new revision to the database
                    db.QualityClauseRefToQualityClauseRevRefs[revGroup].Add(qualityClauseRev); // manage references
                    db.QualityClauseRevRefToJobRevRefs[db.QualityClauses[revGroup][newRevPosition].Id] = db.QualityClauseRevRefToJobRevRefs[qualityClauseRev];

                    var args = new Dictionary<string, string>(); // add the event
                    args["RevGroup"] = revGroup.ToString();
                    args["QualityClauseRev"] = qualityClauseRev.ToString();
                    db.AuditLog.Add(new Event
                    {
                        Action = "SplitQualityClauseRev",
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
        /// <summary>
        /// Clone quality clauses so that each one has all of the revisions from both quality clauses.
        /// Behavior changes depending on the additive parameter.
        /// </summary>
        /// <param name="sourceClause"></param>
        /// <param name="targetClause"></param>
        /// <param name="additive"></param>
        public void CloneQualityClauseRevs(Guid sourceRevGroup, Guid targetRevGroup, bool additive)
        {
            if (db.QualityClauseRefToQualityClauseRevRefs.ContainsKey(sourceRevGroup) && db.QualityClauseRefToQualityClauseRevRefs.ContainsKey(targetRevGroup)) // if both clauses exist in the database
            {
                if (!additive)
                {
                    db.QualityClauses[targetRevGroup] = db.QualityClauses[sourceRevGroup]; // replace the target revisions with the source revisions in the database
                    db.QualityClauses[targetRevGroup] = db.QualityClauses[targetRevGroup].Select(y => { y.IdRevGroup = targetRevGroup; return y; }).ToList(); // reconfigure the revisions
                    db.QualityClauseRefToQualityClauseRevRefs[targetRevGroup] = db.QualityClauseRefToQualityClauseRevRefs[sourceRevGroup]; // manage references
                }
                else
                {
                    List<Guid> mergedIdList = db.QualityClauseRevs.Where(y => db.QualityClauseRefToQualityClauseRevRefs[sourceRevGroup].Contains(y) || db.QualityClauseRefToQualityClauseRevRefs[targetRevGroup].Contains(y)).ToList(); // create a merged id list and object list
                    List<QualityClause> mergedClauseRevList = mergedIdList.Select(y => db.QualityClauses.Values.First(x => x.Any(z => z.Id == y)).First(x => x.Id == y)).ToList();
                    db.QualityClauses[targetRevGroup] = mergedClauseRevList; // add the merged revisions to the target rev group
                    db.QualityClauses[targetRevGroup] = db.QualityClauses[targetRevGroup].Select(y => { y.IdRevGroup = targetRevGroup; y.RevSeq = db.QualityClauses[targetRevGroup].IndexOf(y); return y; }).ToList(); // reconfigure the revisions
                    db.QualityClauseRefToQualityClauseRevRefs[targetRevGroup] = mergedIdList; // manage references
                }

                var args = new Dictionary<string, string>(); // add the event
                args["SourceRevGroup"] = sourceRevGroup.ToString();
                args["TargetRevGroup"] = targetRevGroup.ToString();
                args["Additive"] = additive.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "CloneQualityClauseRevs",
                    Args = args,
                    When = DateTime.Now
                });
            }
            else
            {
                throw new Exception("One or both of the quality clauses doesn't exist in the database.");
            }
        }
        /// <summary>
        /// Link JobOp and JobRev together if they exist.
        /// </summary>
        /// <param name="opId"></param>
        /// <param name="jobRev"></param>
        public void LinkJobOpAndJobRev(int opId, string jobRev)
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
                            Action = "LinkJobOpAndJobRev",
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
        /// <summary>
        /// Unlink JobOp from given JobRev if they exist.
        /// </summary>
        /// <param name="opId"></param>
        /// <param name="jobRev"></param>
        public void UnlinkJobOpAndJobRev(int opId, string jobRev)
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
                        Action = "UnlinkJobOpAndJobRev",
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
        /// <summary>
        /// Merge JobOps within given JobRev if they exist.
        /// </summary>
        /// <param name="jobRev1"></param>
        /// <param name="jobRev2"></param>
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
        /// <summary>
        /// Clone JobOps within given JobRev if they exist.
        /// Behavior changes depending on the additive parameter.
        /// </summary>
        /// <param name="sourceJobRev"></param>
        /// <param name="targetJobRev"></param>
        /// <param name="additive"></param>
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
        /// <summary>
        /// Link JobOp and OpSpecRev together if they exist.
        /// </summary>
        /// <param name="opId"></param>
        /// <param name="opSpecRev"></param>
        public void LinkJobOpAndOpSpecRev(int opId, Guid opSpecRev)
        {
            if (db.OpSpecRevs.Contains(opSpecRev)) // if the op spec revision exists in the database
            {
                if (!db.OpSpecRevRefToOpRefs[opSpecRev].Contains(opId)) // if the op isn't already linked to the op spec
                {
                    db.OpSpecRevRefToOpRefs[opSpecRev].Add(opId); // link the op to the op spec
                    db.OpRefToOpSpecRevRefs[opId].Add(opSpecRev); // link the op spec revision to the op

                    var args = new Dictionary<string, string>(); // add the event
                    args["OpId"] = opId.ToString();
                    args["OpSpecRev"] = opSpecRev.ToString();
                    db.AuditLog.Add(new Event
                    {
                        Action = "LinkJobOpAndOpSpecRev",
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
        /// <summary>
        /// Unlink JobOp from given OpSpecRev if they exist.
        /// </summary>
        /// <param name="opId"></param>
        /// <param name="opSpecRev"></param>
        public void UnlinkJobOpAndOpSpecRev(int opId, Guid opSpecRev)
        {
            if (db.OpSpecRevs.Contains(opSpecRev)) // if the op spec revision exists in the database
            {
                if (db.OpSpecRevRefToOpRefs[opSpecRev].Contains(opId)) // if the op is linked to the op spec revision
                {
                    db.OpSpecRevRefToOpRefs[opSpecRev].Remove(opId); // unlink the op from the op spec revision
                    db.OpRefToOpSpecRevRefs[opId].Remove(opSpecRev); // unlink the op spec revision from the op

                    var args = new Dictionary<string, string>(); // add the event
                    args["OpId"] = opId.ToString();
                    args["OpSpecRev"] = opSpecRev.ToString();
                    db.AuditLog.Add(new Event
                    {
                        Action = "UnlinkJobOpAndOpSpecRev",
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
        /// <summary>
        /// Merge JobOps within given OpSpecRevs if they exist.
        /// </summary>
        /// <param name="opSpecRev1"></param>
        /// <param name="opSpecRev2"></param>
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
        /// <summary>
        /// CLone JobOps within given OpSpecRevs if they exist.
        /// Behavior changes depending on the additive parameter.
        /// </summary>
        /// <param name="sourceOpSpecRev"></param>
        /// <param name="targetOpSpecRev"></param>
        /// <param name="additive"></param>
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
        /// <summary>
        /// Merge given OpSpecRevs if they exist.
        /// </summary>
        /// <param name="opId1"></param>
        /// <param name="opId2"></param>
        public void MergeOpSpecRevs(int opId1, int opId2)
        {
            if (db.OpRefToOpSpecRevRefs.ContainsKey(opId1) && db.OpRefToOpSpecRevRefs.ContainsKey(opId2)) // if both of the ops exist in the database
            {
                List<Guid> mergedList = db.OpSpecRevs.Where(y => db.OpRefToOpSpecRevRefs[opId1].Contains(y) || db.OpRefToOpSpecRevRefs[opId2].Contains(y)).ToList(); // create a merged id list
                db.OpRefToOpSpecRevRefs[opId1] = mergedList; // put the merged list in both op references
                db.OpRefToOpSpecRevRefs[opId2] = mergedList;

                var args = new Dictionary<string, string>(); // add the event
                args["OpId1"] = opId1.ToString();
                args["OpId2"] = opId2.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "MergeOpSpecRevs",
                    Args = args,
                    When = DateTime.Now
                });
            }
            else
            {
                throw new Exception("One or both of the ops doesn't exist in the database");
            }
        }
        /// <summary>
        /// Clone OpSpecRevs within given JobOps if they exist.
        /// Behavior changes depending on the additive parameter.
        /// </summary>
        /// <param name="sourceOp"></param>
        /// <param name="targetOp"></param>
        /// <param name="additive"></param>
        public void CloneOpSpecRevsBasedOnJobOp(int sourceOp, int targetOp, bool additive)
        {
            if (db.OpRefToOpSpecRevRefs.ContainsKey(sourceOp) && db.OpRefToOpSpecRevRefs.ContainsKey(targetOp)) // if both ops exist in the database
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
        /// <summary>
        /// Split OpSpecRec within given OpSpec if they exist.
        /// </summary>
        /// <param name="opSpecRev"></param>
        /// <param name="opSpec"></param>
        public void SplitOpSpecRev(Guid opSpecRev, Guid revGroup)
        {
            if (db.OpSpecRefToOpSpecRevRefs.ContainsKey(revGroup)) // if the op spec exists in the database
            {
                if (db.OpSpecRefToOpSpecRevRefs[revGroup].Contains(opSpecRev)) // if the op spec has the revision
                {
                    int newRevPosition = db.OpSpecs[revGroup].Count;
                    db.OpSpecs[revGroup].Add(db.OpSpecs[revGroup].First(y => y.Id == opSpecRev)); // split the revision in the database
                    db.OpSpecs[revGroup][newRevPosition].Id = Guid.NewGuid(); // configure the new revision
                    db.OpSpecs[revGroup][newRevPosition].RevSeq = newRevPosition;
                    db.OpSpecRevs.Add(db.OpSpecs[revGroup][newRevPosition].Id); // add the new revision to the database
                    db.OpSpecRefToOpSpecRevRefs[revGroup].Add(opSpecRev); // manage references
                    db.OpSpecRevRefToOpRefs[db.OpSpecs[revGroup][newRevPosition].Id] = db.OpSpecRevRefToOpRefs[opSpecRev];

                    var args = new Dictionary<string, string>(); // add the event
                    args["OpSpecRev"] = opSpecRev.ToString();
                    args["RevGroup"] = revGroup.ToString();
                    db.AuditLog.Add(new Event
                    {
                        Action = "SplitOpSpecRev",
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
        /// <summary>
        /// Clone OpSpecRevs within given OpSpecs if they exist.
        /// Behavior changes depending on the additive parameter.
        /// </summary>
        /// <param name="sourceOpSpec"></param>
        /// <param name="targetOpSpec"></param>
        /// <param name="additive"></param>
        public void CloneOpSpecRevsBasedOnOpSpec(Guid sourceRevGroup, Guid targetRevGroup, bool additive)
        {
            if (db.OpSpecRefToOpSpecRevRefs.ContainsKey(sourceRevGroup) && db.OpSpecRefToOpSpecRevRefs.ContainsKey(targetRevGroup)) // if both op specs exist in the database
            {
                if (!additive)
                {
                    db.OpSpecs[targetRevGroup] = db.OpSpecs[sourceRevGroup]; // replace the revisions in the target op spec with the revisions in the source op spec
                    db.OpSpecs[targetRevGroup] = db.OpSpecs[targetRevGroup].Select(y => { y.IdRevGroup = targetRevGroup; return y; }).ToList(); // reconfigure the revisions
                    db.OpSpecRefToOpSpecRevRefs[targetRevGroup] = db.OpSpecRefToOpSpecRevRefs[sourceRevGroup]; // manage references
                }
                else
                {
                    List<Guid> mergedIdList = db.OpSpecRevs.Where(y => db.OpSpecRefToOpSpecRevRefs[targetRevGroup].Contains(y) || db.OpSpecRefToOpSpecRevRefs[sourceRevGroup].Contains(y)).ToList(); // create merged id list and object list
                    List<OpSpec> mergedOpSpecList = mergedIdList.Select(y => db.OpSpecs.Values.First(x => x.Any(z => z.Id == y)).First(x => x.Id == y)).ToList();
                    db.OpSpecs[targetRevGroup] = mergedOpSpecList; // merge the revisions in the target op spec with the revisions in the source op spec
                    db.OpSpecs[targetRevGroup] = db.OpSpecs[targetRevGroup].Select(y => { y.IdRevGroup = targetRevGroup; y.RevSeq = db.OpSpecs[targetRevGroup].IndexOf(y); return y; }).ToList(); // reconfigure the revisions
                    db.OpSpecRefToOpSpecRevRefs[targetRevGroup] = mergedIdList; // manage references
                }

                var args = new Dictionary<string, string>(); // add the event
                args["SourceRevGroup"] = sourceRevGroup.ToString();
                args["TargetRevGroup"] = targetRevGroup.ToString();
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
        /// <summary>
        /// Link WorkInstructiona nd given JobOp together if they exist.
        /// </summary>
        /// <param name="workInstruction"></param>
        /// <param name="opId"></param>
        public void LinkWorkInstructionToJobOp(Guid revGroup, int opId)
        {
            if (db.Ops.ContainsKey(opId)) // if the op exists in the database
            {
                if (db.WorkInstructionRefToWorkInstructionRevRefs.ContainsKey(revGroup)) // if the rev group exists in the database
                {
                    if (!db.OpRefToWorkInstructionRef.ContainsKey(opId)) // if op isn't already linked to a work instruction
                    {
                        db.WorkInstructions[revGroup] = db.WorkInstructions[revGroup].Select(y => { y.OpId = opId; return y; }).ToList(); // link the work instruction to the op
                        db.OpRefToWorkInstructionRef[opId] = revGroup; // manage references

                        var args = new Dictionary<string, string>(); // add the event
                        args["RevGroup"] = revGroup.ToString();
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
                    throw new Exception("The rev group doesn't exist in the database");
                }
            }
            else
            {
                throw new Exception("The op doesn't exist in the database");
            }
        }
        /// <summary>
        /// Unlink given WorkInstruction from given JobOp if they exist.
        /// </summary>
        /// <param name="workInstruction"></param>
        /// <param name="opId"></param>
        public void UnlinkWorkInstructionFromJobOp(Guid revGroup, int opId)
        {
            if (db.Ops.ContainsKey(opId)) // if the op exists in the database
            {
                if (db.WorkInstructionRefToWorkInstructionRevRefs.ContainsKey(revGroup)) // if the rev group exists in the database
                {
                    if (db.OpRefToWorkInstructionRef[opId] == revGroup) // if the work instruction is linked to the op
                    {
                        db.WorkInstructions[revGroup] = db.WorkInstructions[revGroup].Select(y => { y.OpId = -1; return y; }).ToList(); // unlink the work instruction from the op
                        db.OpRefToWorkInstructionRef.Remove(opId); // manage references

                        var args = new Dictionary<string, string>(); // add the event
                        args["RevGroup"] = revGroup.ToString();
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
                    throw new Exception("The rev group doesn't exist in the database");
                }
            }
            else
            {
                throw new Exception("The op doesn't exist in the database");
            }
        }
        /// <summary>
        /// Merge given WorkInstructionRevs if they exist.
        /// </summary>
        /// <param name="workInstruction1"></param>
        /// <param name="workInstruction2"></param>
        public void MergeWorkInstructionRevs(Guid groupId1, Guid groupId2)
        {
            if (db.WorkInstructionRefToWorkInstructionRevRefs.ContainsKey(groupId1) && db.WorkInstructionRefToWorkInstructionRevRefs.ContainsKey(groupId2)) // if both rev groups exist in the database
            {
                List<Guid> mergedIdList = db.WorkInstructionRevs.Where(y => db.WorkInstructionRefToWorkInstructionRevRefs[groupId1].Contains(y) || db.WorkInstructionRefToWorkInstructionRevRefs[groupId2].Contains(y)).ToList(); // create a merged id list and object list
                List<WorkInstruction> mergedWorkInstructionList = mergedIdList.Select(y => db.WorkInstructions.First(x => x.Value.Any(z => mergedIdList.Contains(y))).Value.First(x => mergedIdList.Contains(y))).ToList();
                db.WorkInstructions[groupId1] = mergedWorkInstructionList;
                db.WorkInstructions[groupId2] = mergedWorkInstructionList;
                db.WorkInstructions[groupId1] = db.WorkInstructions[groupId1].Select(y => { y.IdRevGroup = groupId1; y.RevSeq = db.WorkInstructions[groupId1].IndexOf(y); return y; }).ToList(); // reconfigure the revisions
                db.WorkInstructions[groupId2] = db.WorkInstructions[groupId2].Select(y => { y.IdRevGroup = groupId2; y.RevSeq = db.WorkInstructions[groupId2].IndexOf(y); return y; }).ToList();
                db.WorkInstructionRefToWorkInstructionRevRefs[groupId1] = mergedIdList; // manage references
                db.WorkInstructionRefToWorkInstructionRevRefs[groupId2] = mergedIdList;

                var args = new Dictionary<string, string>(); // add the event
                args["GroupId1"] = groupId1.ToString();
                args["GroupId2"] = groupId2.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "MergeWorkInstructionRevs",
                    Args = args,
                    When = DateTime.Now
                });
            }
            else
            {
                throw new Exception("One or both of the rev groups doesn't exist in the database");
            }
        }
        /// <summary>
        /// Split WorkInstructionRev within given WorkInstruction if they exist.
        /// </summary>
        /// <param name="workInstructionRev"></param>
        /// <param name="workInstruction"></param>
        public void SplitWorkInstructionRev(Guid revGroup, Guid workInstructionRev)
        {
            if (db.WorkInstructionRefToWorkInstructionRevRefs.ContainsKey(revGroup)) // if rev group exists in the database
            {
                if (db.WorkInstructionRefToWorkInstructionRevRefs[revGroup].Contains(workInstructionRev)) // if the work instruction has the revision
                {
                    int newRevPosition = db.WorkInstructions[revGroup].Count;
                    db.WorkInstructions[revGroup].Add(db.WorkInstructions[revGroup].First(y => y.Id == workInstructionRev)); // split the revision in the database
                    db.WorkInstructions[revGroup][newRevPosition].Id = Guid.NewGuid(); // configure the new revision
                    db.WorkInstructions[revGroup][newRevPosition].RevSeq = newRevPosition;
                    db.WorkInstructionRevs.Add(db.WorkInstructions[revGroup][newRevPosition].Id); // add the new revision to the database
                    db.WorkInstructionRefToWorkInstructionRevRefs[revGroup].Add(db.WorkInstructions[revGroup][newRevPosition].Id); // manage references

                    var args = new Dictionary<string, string>(); // add the event
                    args["RevGroup"] = revGroup.ToString();
                    args["WorkInstructionRev"] = workInstructionRev.ToString();
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
                throw new Exception("The rev group doesn't exist in the database");
            }
        }
        /// <summary>
        /// Clone given WorkInstructionRev into the target WorkInstructionRev if they exist.
        /// Behavior changes depending on the additive parameter.
        /// </summary>
        /// <param name="sourceWorkInstruction"></param>
        /// <param name="targetWorkInstruction"></param>
        /// <param name="additive"></param>
        public void CloneWorkInstructionRevs(Guid sourceWorkInstruction, Guid targetWorkInstruction, bool additive)
        {
            Guid targetRevGroup = db.OpSpecs.First(y => y.Value[0].Id == targetWorkInstruction).Key;
            Guid sourceRevGroup = db.OpSpecs.First(y => y.Value[0].Id == sourceWorkInstruction).Key;
            if (db.WorkInstructionRefToWorkInstructionRevRefs.ContainsKey(targetRevGroup) && db.WorkInstructionRefToWorkInstructionRevRefs.ContainsKey(sourceRevGroup)) // if both rev groups exist in the database
            {
                if (!additive)
                {
                    db.WorkInstructions[targetRevGroup] = db.WorkInstructions[sourceRevGroup]; // replace the revisions in the target work instruction with the revisions in the source work instruction
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
        /// <summary>
        /// Retrieve QualityClause from given Job if it exists.
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="customerRev"></param>
        /// <param name="internalRev"></param>
        /// <returns></returns>
        public List<QualityClause> PullQualityClausesFromJob(string jobId, string customerRev, string internalRev) =>
        db.Jobs[jobId + "-" + customerRev].First(y => y.RevPlan == internalRev).QualityClauses;
        /// <summary>
        /// Show prior recisions of the selected WorkInstruction if it exists.
        /// </summary>
        /// <param name="workInstruction"></param>
        /// <returns></returns>
        public List<WorkInstruction> DisplayPriorRevisionsOfWorkInstruction(Guid revGroup)
        {
            if (db.WorkInstructionRefToWorkInstructionRevRefs.ContainsKey(revGroup)) // if the rev group exists in the database
            {
                return db.WorkInstructions[revGroup];
            }
            else
            {
                throw new Exception("Rev group doesn't exist in the database");
            }
        }
        /// <summary>
        /// Show prior revisions of the selected QualityClause if it exists.
        /// </summary>
        /// <param name="qualityClause"></param>
        /// <returns></returns>
        public List<QualityClause> DisplayPriorRevisionsOfQualityClauses(Guid revGroup)
        {
            if (db.QualityClauseRefToQualityClauseRevRefs.ContainsKey(revGroup)) // if the rev group exists in the database
            {
                return db.QualityClauses[revGroup];
            }
            else
            {
                throw new Exception("Rev group doesn't exist in the database");
            }
        }
        /// <summary>
        /// Show prior revisions of the selected OpSpec if it exists.
        /// </summary>
        /// <param name="opSpec"></param>
        /// <returns></returns>
        public List<OpSpec> DisplayPriorRevisionsOfSpecs(Guid revGroup)
        {
            if (db.OpSpecRefToOpSpecRevRefs.ContainsKey(revGroup)) // if the rev group exists in the database
            {
                return db.OpSpecs[revGroup];
            }
            else
            {
                throw new Exception("Rev group doesn't exist in the database");
            }
        }
        /// <summary>
        /// Show prior revisions of the selected WorkInstruction if it exists.
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="jobRev"></param>
        /// <param name="opId"></param>
        /// <returns></returns>
        public string[] DisplayLatestRevisionOfWorkInstruction(string jobId, string jobRev, string opService)
        {
            if (db.Jobs.ContainsKey(jobId)) // if the job exists in the database
            {
                if (db.Jobs[jobId].Any(y => y.RevPlan == jobRev)) // if the job has the revision
                {
                    if (db.Jobs[jobId].First(y => y.RevPlan == jobRev).Ops.Any(y => y.OpService == opService)) // if the job revision has the op
                    {
                        string[] data = new string[3];
                        int opId = db.Jobs[jobId].First(y => y.RevPlan == jobRev).Ops.First(y => y.OpService == opService).Id;
                        data[0] = JsonSerializer.Serialize(db.WorkInstructions.Values.First(y => y.Last().Id == db.WorkInstructionRefToWorkInstructionRevRefs[db.OpRefToWorkInstructionRef[opId]].Last()).Last());
                        data[1] = JsonSerializer.Serialize(db.Jobs[jobId].First(y => y.RevPlan == jobRev).QualityClauses);
                        data[2] = JsonSerializer.Serialize(db.OpSpecRevs.Where(y => db.OpRefToOpSpecRevRefs[opId].Contains(y)).Select(y => db.OpSpecs.Values.First(x => x.Any(z => z.Id == y)).First(x => x.Id == y)).ToList());
                        return data;
                    }
                    else
                    {
                        throw new Exception("Job revision doesn't have the op service");
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
