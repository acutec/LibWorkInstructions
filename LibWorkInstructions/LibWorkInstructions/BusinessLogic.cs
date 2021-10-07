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
        }
        
        public WorkInstruction getWorkInstruction(int instructionId) =>
            db.WorkInstructions.First(y => y.Key == instructionId).Value;

        public void AddWorkInstruction(WorkInstruction newWorkInstruction)
        {
            db.WorkInstructions.Add(newWorkInstruction.Id, newWorkInstruction);
        }
        public void ChangeWorkInstruction(int oldWorkId, WorkInstruction newWorkInstruction)
        {
            db.WorkInstructions[oldWorkId] = newWorkInstruction;
        }
        public void RemoveWorkInstruction(int workId)
        {
            db.WorkInstructions.Remove(workId);
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
        }

        public void CloneWorkInstructions(int workId, string newJobId)
        {
            List<List<int>> duplicate = new List<List<int>>();
            Job newJob = new Job();
            newJob.Id = newJobId;

            foreach (List<List<int>> list in db.JobRefToWorkInstructionRefs.Values)
            {
                foreach (List<int> workInstruction in list)
                {
                    if (workInstruction.Contains(workId))
                        duplicate = list;
                }
            }

            db.Jobs.Add(newJobId, newJob);
            db.JobRefToWorkInstructionRefs.Add(newJobId, duplicate);
        }

        public void AddSpec(OpSpec opSpec)
        {
            db.OpSpecs.Add(opSpec.Id, opSpec);
        }

        public void ChangeSpec(int oldSpecId, OpSpec newOpSpec)
        {
            db.OpSpecs[oldSpecId] = newOpSpec;
            List<WorkInstruction> invalidateWorkInstructions =  (from workInstruction in db.WorkInstructions.Values
                                                                where workInstruction.OpSpecs.Contains(oldSpecId)
                                                                select workInstruction).ToList();
            foreach(WorkInstruction workInstruction in invalidateWorkInstructions)
                workInstruction.Approved = false;
        }

        public void DeleteSpec(int specId)
        {
            db.OpSpecs.Remove(specId);
        }

        public void MergeSpecs(int workId1, int workId2)
        {
            db.WorkInstructions[workId1].OpSpecs =
                Enumerable.ToList(db.WorkInstructions[workId1].OpSpecs.Union(db.WorkInstructions[workId2].OpSpecs));
            db.WorkInstructions[workId2].OpSpecs = db.WorkInstructions[workId1].OpSpecs;
        }

        public void SplitSpecs(int workId1, int workId2)
        {
            db.WorkInstructions[workId2].OpSpecs = db.WorkInstructions[workId1].OpSpecs;
        }

        public void CloneSpecs(int workId, int newWorkId)
        {
            WorkInstruction newInstruction = new WorkInstruction();
            newInstruction.Id = newWorkId;
            newInstruction.OpSpecs = db.WorkInstructions[workId].OpSpecs;
            db.WorkInstructions.Add(newInstruction.Id, newInstruction);
        }

        public void CreateQualityClause(QualityClause qualityClause)
        {
            db.QualityClauses.Add(qualityClause.Id, qualityClause);
        }

        public void MergeQualityClauses(string job1, string job2)
        {
            db.JobRefToQualityClauseRefs[job1] =
                Enumerable.ToList(db.JobRefToQualityClauseRefs[job1].Union(db.JobRefToQualityClauseRefs[job2]));
            db.JobRefToQualityClauseRefs[job2] = db.JobRefToQualityClauseRefs[job1];
        }

        public void SplitQualityClauses(string job1, string job2)
        {
            db.JobRefToQualityClauseRefs[job2] = db.JobRefToQualityClauseRefs[job1];
        }

        public void CloneQualityClauses(string sourceJobId, string targetJobId)
        {
            db.JobRefToQualityClauseRefs[targetJobId]
                .AddRange(db.JobRefToQualityClauseRefs[sourceJobId]);
            db.JobRefToQualityClauseRefs[targetJobId] =
                    db.JobRefToQualityClauseRefs[targetJobId].Distinct().ToList();
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
