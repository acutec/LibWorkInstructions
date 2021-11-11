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
                if (db.Jobs[newJob.Id + "-" + newJob.RevCustomer].FindIndex(y => y.RevPlan == newJob.RevPlan) == -1)
                {
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

        public void UpdateJobRev(string oldRev, string newRev)
        {
            int objIndex = db.JobRevs.FindIndex(y => y == oldRev);
            int[] refIndices;
            if (objIndex != -1)
            {
                db.JobRevs[objIndex] = newRev;
            }
            for (int i = 0; i < db.JobRefToJobRevRefs.Count; i++)
            {
                for (int j = 0; j < db.JobRefToJobRevRefs.Values.ToList()[i].Count; j++)
                {
                    refIndices = Enumerable.Range(0, db.JobRefToJobRevRefs.Values.ToList()[i].Count()).Where(i => db.JobRefToJobRevRefs.Values.ToList()[i][j] == oldRev).ToArray();
                    Array.ForEach(refIndices, i => db.JobRefToJobRevRefs.Values.ToList()[i][j] = newRev);
                }
            }
            for (int i = 0; i < db.QualityClauseRevRefToJobRevRefs.Count; i++)
            {
                for (int j = 0; j < db.QualityClauseRevRefToJobRevRefs.Values.ToList()[i].Count; j++)
                {
                    refIndices = Enumerable.Range(0, db.QualityClauseRevRefToJobRevRefs.Values.ToList()[i].Count()).Where(i => db.QualityClauseRevRefToJobRevRefs.Values.ToList()[i][j] == oldRev).ToArray();
                    Array.ForEach(refIndices, i => db.QualityClauseRevRefToJobRevRefs.Values.ToList()[i][j] = newRev);
                }
            }
            List<int> jobOpList = db.JobRevRefToOpRefs[oldRev];
            db.JobRevRefToOpRefs.Remove(oldRev);
            db.JobRevRefToOpRefs[newRev] = jobOpList;
            List<Guid> qualityClauseRevList = db.JobRevRefToQualityClauseRevRefs[oldRev];
            db.JobRevRefToQualityClauseRevRefs.Remove(oldRev);
            db.JobRevRefToQualityClauseRevRefs[newRev] = qualityClauseRevList;

            var args = new Dictionary<string, string>();
            args["OldRev"] = oldRev;
            args["NewRev"] = newRev;
            db.AuditLog.Add(new Event
            {
                Action = "UpdateJobRev",
                Args = args,
                When = DateTime.Now
            });
        }

        public void DeleteJobRev(string jobRev)
        {
            db.JobRevs.Remove(jobRev);
            for (int i = db.JobRefToJobRevRefs.Count - 1; i >= 0; i--)
            {
                db.JobRefToJobRevRefs.Values.ToList()[i].RemoveAll(y => y == jobRev);
            }
            for (int i = db.QualityClauseRevRefToJobRevRefs.Count - 1; i >= 0; i--)
            {
                db.QualityClauseRevRefToJobRevRefs.Values.ToList()[i].RemoveAll(y => y == jobRev);
            }
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

        public void LinkJobRevToJob(string jobId, string jobRev)
        {
            if(db.JobRefToJobRevRefs.ContainsKey(jobId))
            {
                if(!db.JobRefToJobRevRefs[jobId].Contains(jobRev))
                {
                    db.JobRefToJobRevRefs[jobId].Add(jobRev);
                }

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
                throw new Exception("Job doesn't exist in the database");
            }
        }

        public void UnlinkJobRevFromJob(string jobId, string jobRev)
        {
            if(db.JobRefToJobRevRefs.ContainsKey(jobId))
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
                throw new Exception("Job doesn't exist in the database");
            }
        }

        public void MergeJobRevsBasedOnJob(string jobId1, string jobId2)
        {
            if(db.JobRefToJobRevRefs.ContainsKey(jobId1) && db.JobRefToJobRevRefs.ContainsKey(jobId2))
            {
                List<string> mergedList = db.JobRefToJobRevRefs[jobId1].Union(db.JobRefToJobRevRefs[jobId2]).ToList();
                db.JobRefToJobRevRefs[jobId1] = mergedList;
                db.JobRefToJobRevRefs[jobId2] = mergedList;

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
            if(db.JobRefToJobRevRefs.ContainsKey(jobId))
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
                    throw new Exception("The job doesn't have a reference to the job revision");
                }
            }
            else
            {
                throw new Exception("The job doesn't exist in the database");
            }
        }

        public void CloneJobRevsToJob(string sourceJob, string targetJob, bool overwrite)
        {
            if(db.JobRefToJobRevRefs.ContainsKey(sourceJob) && db.JobRefToJobRevRefs.ContainsKey(targetJob))
            {
                if(overwrite)
                {
                    db.JobRefToJobRevRefs[targetJob] = db.JobRefToJobRevRefs[sourceJob];
                }
                else
                {
                    db.JobRefToJobRevRefs[targetJob] = db.JobRefToJobRevRefs[targetJob].Union(db.JobRefToJobRevRefs[sourceJob]).ToList();
                }

                var args = new Dictionary<string, string>();
                args["SourceJob"] = sourceJob;
                args["TargetJob"] = targetJob;
                args["Overwrite"] = overwrite.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "CloneJobRevsToJob",
                    Args = args,
                    When = DateTime.Now,
                });
            }
            else
            {
                throw new Exception("One or both of the jobs doesn't exist in the database");
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

        public void UpdateQualityClauseRev(Guid oldRev, Guid newRev)
        {
            if (db.QualityClauseRevs.Contains(oldRev) && db.QualityClauseRevs.Contains(newRev))
            {
                int[] refIndices;
                int objIndex = db.QualityClauseRevs.FindIndex(y => y == oldRev);
                if (objIndex != -1)
                {
                    db.QualityClauseRevs[objIndex] = newRev;
                }
                for (int i = 0; i < db.JobRevRefToQualityClauseRevRefs.Count; i++)
                {
                    for (int j = 0; j < db.JobRevRefToQualityClauseRevRefs.Values.ToList()[i].Count; j++)
                    {
                        refIndices = Enumerable.Range(0, db.JobRevRefToQualityClauseRevRefs.Values.ToList()[i].Count()).Where(i => db.JobRevRefToQualityClauseRevRefs.Values.ToList()[i][j] == oldRev).ToArray();
                        Array.ForEach(refIndices, i => db.JobRevRefToQualityClauseRevRefs.Values.ToList()[i][j] = newRev);
                    }
                }
                for (int i = 0; i < db.QualityClauseRefToQualityClauseRevRefs.Count; i++)
                {
                    for (int j = 0; j < db.QualityClauseRefToQualityClauseRevRefs.Values.ToList()[i].Count; j++)
                    {
                        refIndices = Enumerable.Range(0, db.QualityClauseRefToQualityClauseRevRefs.Values.ToList()[i].Count()).Where(i => db.QualityClauseRefToQualityClauseRevRefs.Values.ToList()[i][j] == oldRev).ToArray();
                        Array.ForEach(refIndices, i => db.QualityClauseRefToQualityClauseRevRefs.Values.ToList()[i][j] = newRev);
                    }
                }
                for (int i = 0; i < db.QualityClauses.Count; i++)
                {
                    for (int j = 0; j < db.QualityClauses.Values.ToList()[i].Count; j++)
                    {
                        refIndices = Enumerable.Range(0, db.QualityClauses.Values.ToList()[i].Count()).Where(i => db.QualityClauses.Values.ToList()[i][j].Rev == oldRev).ToArray();
                        Array.ForEach(refIndices, i => db.QualityClauses.Values.ToList()[i][j].Rev = newRev);
                    }
                }
                List<string> jobRevList = db.QualityClauseRevRefToJobRevRefs[oldRev];
                db.QualityClauseRevRefToJobRevRefs.Remove(oldRev);
                db.QualityClauseRevRefToJobRevRefs[newRev] = jobRevList;

                var args = new Dictionary<string, string>();
                args["OldRev"] = oldRev.ToString();
                args["NewRev"] = newRev.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "UpdateQualityClauseRev",
                    Args = args,
                    When = DateTime.Now
                });
            }
            else
            {
                throw new Exception("One or both of the quality clause revisions doesn't exist in the database");
            }
        }

        public void DeleteQualityClauseRev(Guid qualityClauseRev)
        {
            if (db.QualityClauseRevs.Contains(qualityClauseRev))
            {
                db.QualityClauseRevs.Remove(qualityClauseRev);
                for (int i = db.JobRevRefToQualityClauseRevRefs.Count; i >= 0; i--)
                {
                    db.JobRevRefToQualityClauseRevRefs.Values.ToList()[i].RemoveAll(y => y == qualityClauseRev);
                }
                for (int i = db.QualityClauseRefToQualityClauseRevRefs.Count; i >= 0; i--)
                {
                    db.QualityClauseRefToQualityClauseRevRefs.Values.ToList()[i].RemoveAll(y => y == qualityClauseRev);
                }
                db.QualityClauseRevRefToJobRevRefs.Remove(qualityClauseRev);

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
                throw new Exception("Quality clause revision doesn't exist in the database.");
            }
        }

        public void LinkJobRevToQualityClauseRev(Guid qualityClauseRev, string jobRev)
        {
            if(db.QualityClauseRevRefToJobRevRefs.ContainsKey(qualityClauseRev))
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
                throw new Exception("The quality clause doesn't have that revision");
            }
        }

        public void LinkQualityClauseRevToJobRev(string jobRev, Guid qualityClauseRev)
        {
            if (db.JobRevRefToQualityClauseRevRefs.ContainsKey(jobRev))
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
                throw new Exception("The job doesn't have that revision");
            }
        }

        public void UnlinkJobRevFromQualityClauseRev(Guid qualityClauseRev, string jobRev)
        {
            if (db.QualityClauseRevRefToJobRevRefs.ContainsKey(qualityClauseRev))
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
                throw new Exception("The quality clause revision doesn't exist for that job revision");
            }
        }

        public void UnlinkQualityClauseRevFromJobRev(string jobRev, Guid qualityClauseRev)
        {
            if (db.JobRevRefToQualityClauseRevRefs.ContainsKey(jobRev))
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
                throw new Exception("This job revision doesn't exist in the database");
            }
        }

        public void MergeJobRevsFromQualityClauseRev(Guid qualityClauseRev1, Guid qualityClauseRev2)
        {
            if (db.QualityClauseRevRefToJobRevRefs.ContainsKey(qualityClauseRev1) && db.QualityClauseRevRefToJobRevRefs.ContainsKey(qualityClauseRev2))
            {
                List<string> mergedList = db.QualityClauseRevRefToJobRevRefs[qualityClauseRev1].Union(db.QualityClauseRevRefToJobRevRefs[qualityClauseRev2]).ToList();
                db.QualityClauseRevRefToJobRevRefs[qualityClauseRev1] = mergedList;
                db.QualityClauseRevRefToJobRevRefs[qualityClauseRev2] = mergedList;

                var args = new Dictionary<string, string>();
                args["QualityClauseRev1"] = qualityClauseRev1.ToString();
                args["QualityClauseRev2"] = qualityClauseRev2.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "MergeJobRevsFromQualityClauseRev",
                    Args = args,
                    When = DateTime.Now
                });
            }
            else
            {
                throw new Exception("One or both of the job revisions doesn't exist for that quality clause revision");
            }
        }

        public void MergeQualityClauseRevsFromJobRev(string jobRev1, string jobRev2)
        {
            if (db.JobRevRefToQualityClauseRevRefs.ContainsKey(jobRev1) && db.JobRevRefToQualityClauseRevRefs.ContainsKey(jobRev2))
            {
                List<Guid> mergedList = db.JobRevRefToQualityClauseRevRefs[jobRev1].Union(db.JobRevRefToQualityClauseRevRefs[jobRev2]).ToList();
                db.JobRevRefToQualityClauseRevRefs[jobRev1] = mergedList;
                db.JobRevRefToQualityClauseRevRefs[jobRev2] = mergedList;

                var args = new Dictionary<string, string>();
                args["JobRev1"] = jobRev1;
                args["JobRev2"] = jobRev2;
                db.AuditLog.Add(new Event
                {
                    Action = "MergeQualityClauseRevsFromJobRev",
                    Args = args,
                    When = DateTime.Now
                });

            }
            else
            {
                throw new Exception("One or both of the job revisions doesn't exist in the database");
            }
        }

        public void SplitJobRevInQualityClauseRev(Guid qualityClauseRev, string jobRev)
        {
            if (db.QualityClauseRevRefToJobRevRefs.ContainsKey(qualityClauseRev))
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
                    throw new Exception("The job revision doesn't have a connection to the quality clause revision");
                }
            }
            else
            {
                throw new Exception("The quality clause revision doesn't exist in the database");
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
                        Action = "SplitQualityClauseRevToJobRev",
                        Args = args,
                        When = DateTime.Now
                    });
                }
                else
                {
                    throw new Exception("The quality clause revision doesn't have a reference to the job revision.");
                }
            }
            else
            {
                throw new Exception("The job revision doesn't exist in the database");
            }
        }

        public void CloneJobRevsFromQualityClauseRev(Guid sourceQualityClauseRev, Guid targetQualityClauseRev, bool overwrite)
        {
            if (db.QualityClauseRevRefToJobRevRefs.ContainsKey(sourceQualityClauseRev) && db.QualityClauseRevRefToJobRevRefs.ContainsKey(targetQualityClauseRev))
            {
                if(overwrite)
                {
                    db.QualityClauseRevRefToJobRevRefs[targetQualityClauseRev] = db.QualityClauseRevRefToJobRevRefs[sourceQualityClauseRev];
                }
                else
                {
                    db.QualityClauseRevRefToJobRevRefs[targetQualityClauseRev] = db.QualityClauseRevRefToJobRevRefs[targetQualityClauseRev].Union(db.QualityClauseRevRefToJobRevRefs[sourceQualityClauseRev]).ToList();
                }

                var args = new Dictionary<string, string>();
                args["SourceQualityClauseRev"] = sourceQualityClauseRev.ToString();
                args["TargetQualityClauseRev"] = targetQualityClauseRev.ToString();
                args["Overwrite"] = overwrite.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "CloneJobRevsFromQualityClauseRev",
                    Args = args,
                    When = DateTime.Now
                });
            }
            else
            {
                throw new Exception("One or both of the quality clause revisions doesn't exist in the database");
            }
        }

        public void CloneQualityClauseRevsFromJobRev(string sourceJobRev, string targetJobRev, bool overwrite)
        {
            if (db.JobRevRefToQualityClauseRevRefs.ContainsKey(sourceJobRev) && db.JobRevRefToQualityClauseRevRefs.ContainsKey(targetJobRev))
            {
                if (overwrite)
                {
                    db.JobRevRefToQualityClauseRevRefs[targetJobRev] = db.JobRevRefToQualityClauseRevRefs[sourceJobRev];
                }
                else
                {
                    db.JobRevRefToQualityClauseRevRefs[targetJobRev] = db.JobRevRefToQualityClauseRevRefs[targetJobRev].Union(db.JobRevRefToQualityClauseRevRefs[sourceJobRev]).ToList();
                }

                var args = new Dictionary<string, string>();
                args["SourceJobRev"] = sourceJobRev;
                args["TargetJobRev"] = targetJobRev;
                args["Overwrite"] = overwrite.ToString();
                db.AuditLog.Add(new Event
                {
                    Action = "CloneQualityClauseRevsFromJobRev",
                    Args = args,
                    When = DateTime.Now
                });
            }
            else
            {
                throw new Exception("One or both of the job revisions doesn't exist in the database");
            }
        }

        public void CreateQualityClause(QualityClause newQualityClause)
        {
            if (!db.QualityClauses.ContainsKey(newQualityClause.Id))
            {
                db.QualityClauses.Add(newQualityClause.Id, new List<QualityClause> { newQualityClause });
                db.QualityClauseRefToQualityClauseRevRefs.Add(newQualityClause.Id, new List<Guid> { newQualityClause.Rev });

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
                if(db.QualityClauses[qualityClause.Id].FindIndex(y => y.Rev == qualityClause.Rev) == -1)
                {
                    db.QualityClauses[qualityClause.Id].Add(qualityClause);

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
                    throw new Exception("The revision is already linked to the clause");
                }
            }
            else
            {
                throw new Exception("The quality clause doesn't exist in the database");
            }
        }

        public void UnlinkQualityClauseRevToQualityClause(Guid clauseRev, Guid clauseId)
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
                        Action = "UnlinkQualityClauseRevToQualityClause",
                        Args = args,
                        When = DateTime.Now
                    });
                }
                else
                {
                    throw new Exception("The revision wasn't linked to the clause");
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
                List<Guid> mergedList = db.QualityClauseRefToQualityClauseRevRefs[clauseId1].Union(db.QualityClauseRefToQualityClauseRevRefs[clauseId2]).ToList();
                db.QualityClauseRefToQualityClauseRevRefs[clauseId1] = mergedList;
                db.QualityClauseRefToQualityClauseRevRefs[clauseId2] = mergedList;

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
                throw new Exception("One or both of the quality clauses don't exist in the database");
            }
        }

        public void SplitQualityClauseRevsInQualityClause(Guid clauseId, Guid clauseRev)
        {
            if (db.QualityClauses.ContainsKey(clauseId))
            {
                if (db.QualityClauseRefToQualityClauseRevRefs[clauseId].Contains(clauseRev))
                {
                    db.QualityClauseRefToQualityClauseRevRefs[clauseId].Add(clauseRev);

                    var args = new Dictionary<string, string>();
                    args["ClauseId"] = clauseId.ToString();
                    args["ClauseRev"] = clauseRev.ToString();
                    db.AuditLog.Add(new Event
                    {
                        Action = "SplitQualityClauseRevsInQualityClause",
                        Args = args,
                        When = DateTime.Now
                    });
                }
                else
                {
                    throw new Exception("The quality clause doesn't have a connection to that revision");
                }
            }
            else
            {
                throw new Exception("The quality clause doesn't exist in the database");
            }
        }

        public void CloneQualityClauseRevsFromQualityClause(Guid sourceClause, Guid targetClause, bool overwrite)
        {
            if (db.QualityClauses.ContainsKey(sourceClause) && db.QualityClauses.ContainsKey(targetClause))
            {
                if (overwrite)
                {
                    db.QualityClauseRefToQualityClauseRevRefs[targetClause] = db.QualityClauseRefToQualityClauseRevRefs[sourceClause];
                }
                else
                {
                    db.QualityClauseRefToQualityClauseRevRefs[targetClause] = db.QualityClauseRefToQualityClauseRevRefs[targetClause].Union(db.QualityClauseRefToQualityClauseRevRefs[sourceClause]).ToList();
                }

                var args = new Dictionary<string, string>();
                args["SourceClause"] = sourceClause.ToString();
                args["TargetClause"] = targetClause.ToString();
                args["Overwrite"] = overwrite.ToString();
            }
            else
            {
                throw new Exception("One or both of the quality clauses doesn't exist in the database.");
            }
        }

        public void CreateOp(Op op)
        {
            if(!db.Ops.ContainsKey(op.Id))
            {
                db.Ops.Add(op.Id, new List<Op> { op });
                db.OpRefToOpSpecRevRefs.Add(op.Id, new List<Guid>());

                var args = new Dictionary<string, string>();
                args["Op"] = JsonSerializer.Serialize(op);
                db.AuditLog.Add(new Event
                {
                    Action = "CreateOp",
                    Args = args,
                    When = DateTime.Now
                });
            }
            else
            {
                throw new Exception("The operation already exists in the database");
            }
        }

        public void UpdateOp(Op op)
        {
            if (db.Ops.ContainsKey(op.Id))
            {
                if (db.Ops[op.Id].FindIndex(y => y.Rev == op.Rev) == -1)
                {
                    db.Ops[op.Id].Add(op);

                    var args = new Dictionary<string, string>();
                    args["Op"] = JsonSerializer.Serialize(op);
                    db.AuditLog.Add(new Event
                    {
                        Action = "UpdateOp",
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

        public void DeleteOp(int opId)
        {
            db.Ops.Remove(opId);
            db.OpRefToOpSpecRevRefs.Remove(opId);
            db.OpRefToWorkInstructionRef.Remove(opId);
            for (int i = db.JobRevRefToOpRefs.Count - 1; i >= 0; i--)
            {
                db.JobRevRefToOpRefs.Values.ToList()[i].RemoveAll(y => y == opId);
            }
            for (int i = db.OpSpecRevRefToOpRefs.Count - 1; i >= 0; i--)
            {
                db.OpSpecRevRefToOpRefs.Values.ToList()[i].RemoveAll(y => y == opId);
            }

        }

        public void CreateWorkInstruction(WorkInstruction workInstruction)
        {
            WorkInstruction newWorkInstruction = workInstruction;
            if(!db.WorkInstructions.ContainsKey(workInstruction.IdRevGroup))
            {
                db.WorkInstructions[newWorkInstruction.IdRevGroup] = new List<WorkInstruction>();
            }
            if (db.WorkInstructions[newWorkInstruction.IdRevGroup].FindIndex(y => y.Equals(newWorkInstruction)) < 0)
            {
                db.WorkInstructions[newWorkInstruction.IdRevGroup].Add(newWorkInstruction);
                db.OpRefToWorkInstructionRef[newWorkInstruction.OpId] = newWorkInstruction.Id;
                db.WorkInstructionRefToWorkInstructionRevRefs.Add(newWorkInstruction.Id, new List<Guid> { newWorkInstruction.Id });

                var args = new Dictionary<string, string>();
                args["workInstruction"] = JsonSerializer.Serialize(newWorkInstruction);
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
                if (db.WorkInstructions[newWorkInstruction.IdRevGroup].FindIndex(y => y.Equals(newWorkInstruction)) < 0)
                {
                    db.WorkInstructions[newWorkInstruction.IdRevGroup].Add(newWorkInstruction);
                    List<Guid> revisions = db.WorkInstructions[newWorkInstruction.IdRevGroup].Select(y => y.Id).ToList();
                    foreach (Guid id in revisions)
                    {
                        db.WorkInstructionRefToWorkInstructionRevRefs[id] = revisions;
                    }
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

        public void RemoveWorkInstruction(Guid targetGroupId, Guid targetWorkId)
        {
            if (db.WorkInstructions.ContainsKey(targetGroupId)) 
            {
                WorkInstruction targetWorkInstruction = db.WorkInstructions[targetGroupId].First(y => y.Id == targetWorkId);
                db.WorkInstructions[targetGroupId].Remove(targetWorkInstruction);
                db.OpRefToWorkInstructionRef.Remove(targetWorkInstruction.OpId);
                db.WorkInstructionRefToWorkInstructionRevRefs.Values.First(y => y.Contains(targetWorkInstruction.Id)).Remove(targetWorkInstruction.Id);

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
                if (db.WorkInstructions[groupId1].FindIndex(y => y.Id == workId1) >= 0 && 
                    db.WorkInstructions[groupId2].FindIndex(y => y.Id == workId2) >= 0)
                {
                    List<Guid> mergedList = db.WorkInstructionRefToWorkInstructionRevRefs[workId1].Union(db.WorkInstructionRefToWorkInstructionRevRefs[workId2]).ToList();
                    db.WorkInstructionRefToWorkInstructionRevRefs[workId1] = mergedList;
                    db.WorkInstructionRefToWorkInstructionRevRefs[workId2] = mergedList;

                    var args = new Dictionary<string, string>();
                    args["GroupId1"] = groupId1.ToString();
                    args["GroupId2"] = groupId2.ToString();
                    args["WorkId1"] = workId1.ToString();
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
                    throw new Exception("One or both of the work instructions doesn't exist within the group ids");
                }
            }
            else
            {
                throw new Exception("One or both of the group ids doesn't exist in the database");
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

        public void AddSpec(OpSpec newSpec)
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
        }

        public void UpdateSpec(OpSpec newSpec)
        {
            if (db.OpSpecs.ContainsKey(newSpec.IdRevGroup))
            {
                db.OpSpecs[newSpec.IdRevGroup].Add(newSpec);

                var args = new Dictionary<string, string>();
                args["newSpec"] = JsonSerializer.Serialize(newSpec);
                db.AuditLog.Add(new Event
                {
                    Action = "UpdateSpec",
                    Args = args,
                    When = DateTime.Now,
                });
            }
            else
            {
                throw new Exception("The spec being updated doesn't exist in the database");
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
            /*
            int opIndex1 = db.Jobs.Values.ToList().FindIndex(y => y.Ops.FindIndex(y => y.Id == opId1) >= 0);
            int opIndex2 = db.Jobs.Values.ToList().FindIndex(y => y.Ops.FindIndex(y => y.Id == opId2) >= 0);
            
            if (opIndex1 >= 0 && opIndex2 >= 0)
            */
            {
                /*
                List<OpSpec> list1 = db.Jobs.Values.ToList()[opIndex1].Ops.First(y => y.Id == opId1).OpSpecs;
                List<OpSpec> list2 = db.Jobs.Values.ToList()[opIndex2].Ops.First(y => y.Id == opId2).OpSpecs;
                List<OpSpec> mergedList = list1.Union(list2).ToList();

                db.Jobs.Values.ToList()[opIndex1].Ops.First(y => y.Id == opId1).OpSpecs = mergedList;
                db.Jobs.Values.ToList()[opIndex2].Ops.First(y => y.Id == opId2).OpSpecs = mergedList;
                */
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
            /*
            else
            {
                throw new Exception("One or both of the operations doesn't exist in the database");
            }
            */
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
            /*
            int sourceOpIndex = db.Jobs.Values.ToList().FindIndex(y => y.Ops.FindIndex(y => y.Id == sourceOpId) >= 0);
            int targetOpIndex = db.Jobs.Values.ToList().FindIndex(y => y.Ops.FindIndex(y => y.Id == targetOpId) >= 0);
            if (sourceOpIndex >= 0 && targetOpIndex >= 0)
            */
            {
                /*
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
                */
            }
            /*
            else
            {
                throw new Exception("One or both of the operations doesn't exist in the database");
            }
            */
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
                throw new Exception("One or both of the jobs doesn't exist in the database");
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
                throw new Exception("This quality clause doesn't exist within this Job");
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
                throw new Exception("One or both of the jobs doesn't exist in the database");
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
                    Action = "CreatespecRev",
                    Args = args,
                    When = DateTime.Now
                });
            }
            else
            {
                throw new Exception("OpSpec revision already exists in the database");
            }
        }

        public void UpdateOpSpecRev()
        {

        }

        public void DeleteOpSpecRev()
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
