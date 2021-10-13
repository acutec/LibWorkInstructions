using System;
using System.Collections.Generic;
using System.Linq;
using static LibWorkInstructions.Structs;

namespace LibWorkInstructions
{
    public class BusinessLogic
    {

        #region database-mocking
        public class MockDB
        {
            public Dictionary<string, Job> Jobs = new Dictionary<string, Job>();
            public Dictionary<int, OpSpec> OpSpecs = new Dictionary<int, OpSpec>();
            public Dictionary<int, QualityClause> QualityClauses = new Dictionary<int, QualityClause>();
            // revs clone an existing one under a new id, with a same groupid
            public Dictionary<int, WorkInstruction> WorkInstructions = new Dictionary<int, WorkInstruction>();
            public Dictionary<string, List<List<int>>> JobRefToWorkInstructionRefs = new Dictionary<string, List<List<int>>>();
            public Dictionary<string, List<int>> JobRefToQualityClauseRefs = new Dictionary<string, List<int>>();
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

        public Job getJob(string jobId) =>
                db.Jobs.First(y => y.Key == jobId).Value;

        public void addJob(Job newJob)
        {
            db.Jobs.Add(newJob.Id, newJob);

            var args = new Dictionary<string, string>();
            var jobDict = new Dictionary<string, Job>();
            args["Job"] = newJob.Id;
            jobDict[args["Job"]] = newJob;
            db.AuditLog.Add(new Event
            {
                Action = "AddJob",
                Args = args,
                newJob = jobDict,
                When = DateTime.Now,
            });
        }
        
        public WorkInstruction getWorkInstruction(int instructionId) =>
            db.WorkInstructions.First(y => y.Key == instructionId).Value;

        public void AddWorkInstruction(WorkInstruction newWorkInstruction)
        {
            db.WorkInstructions.Add(newWorkInstruction.Id, newWorkInstruction);

            var args = new Dictionary<string, string>();
            var workIDict = new Dictionary<string, WorkInstruction>();
            args["WorkInstruction"] = newWorkInstruction.Id.ToString();
            workIDict[args["WorkInstruction"]] = newWorkInstruction;
            db.AuditLog.Add(new Event
            {
                Action = "AddWorkInstruction",
                Args = args,
                newWorkI = workIDict,
                When = DateTime.Now,
            });
        }
        public void ChangeWorkInstruction(int oldWorkId, WorkInstruction newWorkInstruction)
        {
            db.WorkInstructions[oldWorkId] = newWorkInstruction;

            var args = new Dictionary<string, string>();
            args["OldWorkInstruction"] = oldWorkId.ToString();
            args["NewWorkInstruction"] = newWorkInstruction.Id.ToString();
            db.AuditLog.Add(new Event
            {
                Action = "ChangeWorkInstruction",
                Args = args,
                When = DateTime.Now,
            });
        }
        public void RemoveWorkInstruction(int workId)
        {
            db.WorkInstructions.Remove(workId);

            var args = new Dictionary<string, string>();
            args["WorkInstruction"] = workId.ToString();
            db.AuditLog.Add(new Event
            {
                Action = "RemoveWorkInstruction",
                Args = args,
                When = DateTime.Now,
            });
        }
        public void MergeWorkInstructions(int workId1, int workId2)
        {
            List<int> workInstruction1 = new List<int>();
            List<int> workInstruction2 = new List<int>();
            List<int> mergedInstruction = new List<int>();

            foreach (List<List<int>> value in db.JobRefToWorkInstructionRefs.Values)
            {
                foreach (List<int> workInstruction in value)
                {
                    if (workInstruction.Contains(workId1))
                        workInstruction1 = workInstruction;
                    if (workInstruction.Contains(workId2))
                        workInstruction2 = workInstruction;
                }
            }

            string job1 = db.JobRefToWorkInstructionRefs.First(y => y.Value.Contains(workInstruction1)).Key;
            string job2 = db.JobRefToWorkInstructionRefs.First(y => y.Value.Contains(workInstruction2)).Key;

            mergedInstruction = Enumerable.ToList(workInstruction1.Union(workInstruction2));

            db.JobRefToWorkInstructionRefs[job1].Add(mergedInstruction);
            db.JobRefToWorkInstructionRefs[job1].Remove(workInstruction1);
            db.JobRefToWorkInstructionRefs[job2].Remove(workInstruction2);

            var args = new Dictionary<string, string>();
            args["WorkInstruction1"] = workId1.ToString();
            args["WorkInstruction2"] = workId2.ToString();
            db.AuditLog.Add(new Event
            {
                Action = "MergeWorkInstructions",
                Args = args,
                When = DateTime.Now,
            });
        }

        public void SplitWorkInstruction(int workId)
        {
            List<int> duplicate = new List<int>();

            foreach (List<List<int>> value in db.JobRefToWorkInstructionRefs.Values)
            {
                foreach (List<int> workInstruction in value)
                {
                    if (workInstruction.Contains(workId))
                        duplicate = workInstruction;
                }
            }

            string job = db.JobRefToWorkInstructionRefs.First(y => y.Value.Contains(duplicate)).Key;

            db.JobRefToWorkInstructionRefs[job].Add(duplicate);

            var args = new Dictionary<string, string>();
            args["WorkInstruction"] = workId.ToString();
            db.AuditLog.Add(new Event
            {
                Action = "SplitWorkInstruction",
                Args = args,
                When = DateTime.Now,
            });
        }

        public void CloneWorkInstruction(int sourceWorkId, string targetJobId)
        {
            List<int> duplicate = new List<int>();
            foreach (List<List<int>> list in db.JobRefToWorkInstructionRefs.Values)
            {
                foreach (List<int> workInstruction in list)
                {
                    if (workInstruction.Contains(sourceWorkId))
                        duplicate = workInstruction;
                }
            }

            db.JobRefToWorkInstructionRefs[targetJobId].Add(duplicate);

            var args = new Dictionary<string, string>();
            args["SourceWorkInstruction"] = sourceWorkId.ToString();
            args["TargetJob"] = targetJobId;
            db.AuditLog.Add(new Event
            {
                Action = "CloneWorkInstruction",
                Args = args,
                When = DateTime.Now,
            });
        }

        public void AddSpec(OpSpec opSpec)
        {
            db.OpSpecs.Add(opSpec.Id, opSpec);

            var args = new Dictionary<string, string>();
            var specDict = new Dictionary<string, OpSpec>();
            args["OpSpec"] = opSpec.Id.ToString();
            specDict[args["OpSpec"]] = opSpec;
            db.AuditLog.Add(new Event
            {
                Action = "AddSpec",
                Args = args,
                newOpSpec = specDict,
                When = DateTime.Now,
            });
        }

        public void ChangeSpec(int oldSpecId, OpSpec newOpSpec)
        {
            db.OpSpecs[oldSpecId] = newOpSpec;
            List<WorkInstruction> invalidateWorkInstructions =  (from workInstruction in db.WorkInstructions.Values
                                                                where workInstruction.OpSpecs.Contains(oldSpecId)
                                                                select workInstruction).ToList();
            foreach(WorkInstruction workInstruction in invalidateWorkInstructions)
                workInstruction.Approved = false;

            var args = new Dictionary<string, string>();
            args["OldSpec"] = oldSpecId.ToString();
            args["NewSpec"] = newOpSpec.Id.ToString();
            db.AuditLog.Add(new Event
            {
                Action = "ChangeSpec",
                Args = args,
                When = DateTime.Now,
            });
        }

        public void DeleteSpec(int specId)
        {
            db.OpSpecs.Remove(specId);

            var args = new Dictionary<string, string>();
            args["DeletedSpec"] = specId.ToString();
            db.AuditLog.Add(new Event
            {
                Action = "DeleteSpec",
                Args = args,
                When = DateTime.Now,
            });
        }

        public void MergeSpecs(int workId1, int workId2)
        {
            db.WorkInstructions[workId1].OpSpecs =
                Enumerable.ToList(db.WorkInstructions[workId1].OpSpecs.Union(db.WorkInstructions[workId2].OpSpecs));
            db.WorkInstructions[workId2].OpSpecs = db.WorkInstructions[workId1].OpSpecs;

            var args = new Dictionary<string, string>();
            args["WorkInstruction1"] = workId1.ToString();
            args["WorkInstruction2"] = workId2.ToString();
            db.AuditLog.Add(new Event
            {
                Action = "MergeSpecs",
                Args = args,
                When = DateTime.Now,
            });
        }

        public void SplitSpecs(int workId1, int workId2)
        {
            db.WorkInstructions[workId2].OpSpecs = db.WorkInstructions[workId1].OpSpecs;

            var args = new Dictionary<string, string>();
            args["WorkInstruction1"] = workId1.ToString();
            args["WorkInstruction2"] = workId2.ToString();
            db.AuditLog.Add(new Event
            {
                Action = "SplitSpecs",
                Args = args,
                When = DateTime.Now,
            });
        }

        public void CloneSpecs(int sourceWorkId, int targetWorkId)
        {
            db.WorkInstructions[targetWorkId].OpSpecs =
                Enumerable.ToList(db.WorkInstructions[sourceWorkId].OpSpecs.Union(db.WorkInstructions[targetWorkId].OpSpecs));

            var args = new Dictionary<string, string>();
            args["SourceWorkId"] = sourceWorkId.ToString();
            args["TargetWorkId"] = targetWorkId.ToString();
            db.AuditLog.Add(new Event
            {
                Action = "CloneSpecs",
                Args = args,
                When = DateTime.Now,
            });
        }

        public void CreateQualityClause(QualityClause qualityClause)
        {
            db.QualityClauses.Add(qualityClause.Id, qualityClause);

            var args = new Dictionary<string, string>();
            var clauseDict = new Dictionary<string, QualityClause>();
            args["QualityClause"] = qualityClause.Id.ToString();
            clauseDict[args["QualityClause"]] = qualityClause;
            db.AuditLog.Add(new Event
            {
                Action = "CreateQualityClause",
                Args = args,
                newQualityC = clauseDict,
                When = DateTime.Now,
            });
        }

        public void MergeQualityClauses(string job1, string job2)
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

        public void SplitQualityClauses(string sourceJob, string targetJob)
        {
            db.JobRefToQualityClauseRefs[targetJob] = db.JobRefToQualityClauseRefs[sourceJob];

            var args = new Dictionary<string, string>();
            args["SourceJob"] = sourceJob;
            args["TargetJob"] = targetJob;
            db.AuditLog.Add(new Event
            {
                Action = "SplitQualityClauses",
                Args = args,
                When = DateTime.Now,
            });
        }

        public void CloneQualityClauses(string sourceJobId, string targetJobId)
        {
            db.JobRefToQualityClauseRefs[targetJobId]
                .AddRange(db.JobRefToQualityClauseRefs[sourceJobId]);
            db.JobRefToQualityClauseRefs[targetJobId] =
                    db.JobRefToQualityClauseRefs[targetJobId].Distinct().ToList();

            var args = new Dictionary<string, string>();
            args["SourceJob"] = sourceJobId;
            args["TargetJob"] = targetJobId;
            db.AuditLog.Add(new Event
            {
                Action = "CloneQualityClauses",
                Args = args,
                When = DateTime.Now,
            });
        }

        public void DisplayPriorRevisionsOfWorkInstruction(string job, int latestWorkId)
        {
            Console.Write(db.JobRefToWorkInstructionRefs[job].First(y => y.Last() == latestWorkId));
        }

        public void DisplayPriorRevisionsOfQualityClauses(int idRevGroup)
        {
            Console.Write((from qualityClause in db.QualityClauses.Values
                           where qualityClause.IdRevGroup == idRevGroup
                           select qualityClause.Id));
        }

        public void DisplayPriorRevisionsOfSpecs(int idRevGroup)
        {
            Console.Write((from spec in db.OpSpecs.Values
                           where spec.IdRevGroup == idRevGroup
                           select spec));
        }

        public void DisplayLatestRevisionOfWorkInstruction(string jobRev)
        {
            Job job = db.Jobs.Values.First(y => y.Rev == jobRev);
        }

    }
}
