using System;
using System.Collections.Generic;
using System.Linq;
using static LibWorkInstructions.Structs;

namespace LibWorkInstructions {
  public class BusinessLogic {

    #region database-mocking
    public class MockDB {
      public Dictionary<string, Job> Jobs = new Dictionary<string, Job>();
      public Dictionary<int, OpSpec> OpSpecs = new Dictionary<int, OpSpec>();
      public Dictionary<int, QualityClause> QualityClauses = new Dictionary<int, QualityClause>();
      // revs clone an existing one under a new id, with a same groupid
      public Dictionary<int, WorkInstruction> WorkInstructions = new Dictionary<int, WorkInstruction>();
      public Dictionary<string, List<List<int>>> JobRefToWorkInstructionRefs = new Dictionary<string, List<List<int>>>();
      public Dictionary<string, List<int>> JobRefToQualityClauseRefs = new Dictionary<string, List<int>>();
    }
    private MockDB db;  // this should contain any/all state used in this BusinessLogic class.
    public BusinessLogic() {
      this.db = new MockDB();
    }
    public void DataImport(MockDB replacementDb) => this.db = replacementDb;
    public MockDB DataExport() => db;
        #endregion

        public Job getJob(string jobId) =>
                db.Jobs.First(y => y.Key == jobId).Value;

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
            List<int> workInstruction1, workInstruction2, mergedInstruction;

            foreach(List<List<int>> list in db.JobRefToWorkInstructionRefs.Values)
            {
                foreach(List<int> workInstruction in list)
                {
                    if(workInstruction.Contains(workId1))
                        workInstruction1 = list;
                    if(workInstruction.Contains(workId2))
                        workInstruction2 = list;
                }
            }

            string job1 = db.JobRefToWorkInstructionRefs.FindFirstKeyByValue(workInstruction1);
            string job2 = db.JobRefToWorkInstructionRefs.FindFirstKeyByValue(workInstruction2);

            mergedInstruction = workInstruction1.Union(workInstruction2);

            db.JobRefToWorkInstructionRefs[job1].Add(mergedInstruction);
            db.JobRefToWorkInstructionRefs[job1].Remove(workInstruction1);
            db.JobRefToWorkInstructionRefs[job2].Remove(workInstruction2);
        }

        public void SplitWorkInstruction(int workId)
        {
            List<int> duplicate;

            foreach(List<List<int>> list in db.JobRefToWorkInstructionRefs.Values)
            {
                foreach(List<int> workInstruction in list)
                {
                    if(workInstruction.Contains(workId))
                        duplicate = workInstruction;
                }
            }

            string job = db.JobRefToWorkInstructionRefs.FindFirstKeyByValue(duplicate);

            db.JobRefToWorkInstructionRefs[job].Add(duplicate);
        }

        public void CloneWorkInstructions(int workId, string newJobId)
        {
            List<List<int>> duplicate;
            Job newJob = new Job();
            newJob.Id = newJobId;

            foreach(List<List<int>> list in db.JobRefToWorkInstructionRefs.Values)
            {
                foreach(List<int> workInstruction in list)
                {
                    if(workInstruction.Contains(workId))
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
        }
        
        public void DeleteSpec(int specId)
        {
            db.OpSpecs.Remove(specId);
        }

        public void MergeSpecs(int workId1, int workId2)
        {
            db.WorkInstructions[workId1].OpSpecs = 
                db.WorkInstructions[workId1].OpSpecs.Union(db.WorkInstructions[workId2].OpSpecs);
            db.WorkInstructions[workId2].OpSpecs = db.WorkInstructions[workId1].OpSpecs;
        }

        public void SplitSpecs(int workId1, int workId2)
        {
            db.WorkInstructions[workId2].opSpecs = db.WorkInstructions[workId1].opSpecs;
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
                db.JobRefToQualityClauseRefs[job1].Union(db.JobRefToQualityClauseRefs[job2]);
            db.JobRefToQualityClauseRefs[job2] = db.JobRefToQualityClauseRefs[job1];
        }

        public void SplitQualityClauses(string job1, string job2)
        {
            db.JobRefToQualityClauseRefs[job2] = db.JobRefToQualityClauseRefs[job1];
        }

        public void CloneQualityClauses(string job, string newJobId)
        {
            Job newJob = new Job();
            newJob.Id = newJobId;
            db.Jobs.Add(newJobId, newJob);
            db.JobRefToQualityClauseRefs.Add(newJob.Id, newJob);
            db.JobRefToQualityClauseRefs[newJobId] = db.JobRefToQualityClauseRefs[job];
        }

        public void DisplayPriorRevisionsOfWorkInstruction(string job, int latestWorkId)
        {
            List<int> workInstructionRevisions = db.JobRefToWorkInstructionRefs[job].First(y => y.Last() == latestWorkId);
            Console.Write(workInstructionRevisions);
        }
    /*
    public Job GetJob(string jobId) =>
      db.JobWorkInstructions.Keys.First(y => y.Id == jobId);

    public void AddWorkInstruction(string jobId, WorkInstruction newWorkInstruction) {
      Job job = db.JobWorkInstructions.Keys.Single(y => y.Id == jobId);
      db.JobWorkInstructions[job].Add(newWorkInstruction);
    }

    public void ChangeWorkInstruction(string jobId, string workId, WorkInstruction newWorkInstruction) {
      Job job = db.JobWorkInstructions.Keys.First(y => y.Id == jobId);
      List<WorkInstruction> instructionList = db.JobWorkInstructions[job];
      int index = instructionList.IndexOf(instructionList.First(y => y.Id == workId));
      if (index != -1)
        db.JobWorkInstructions[job][index] = newWorkInstruction;
    }

    public void RemoveWorkInstruction(string jobId, string workId) {
      Job job = db.JobWorkInstructions.Keys.First(y => y.Id == jobId);
      db.JobWorkInstructions[job].Remove(db.JobWorkInstructions[job].First(y => y.Id == workId));
    }

    public void MergeWorkInstructions(string jobId1, string jobId2) {
      Job job1 = GetJob(jobId1);
      Job job2 = GetJob(jobId2);
      db.JobWorkInstructions[job1] = db.JobWorkInstructions[job1].Union(db.JobWorkInstructions[job2]).ToList();
    }

    public List<IEnumerable<WorkInstruction>> SplitWorkInstructions(string jobId) {
      Job job = db.JobWorkInstructions.Keys.First(y => y.Id == jobId);
      List<IEnumerable<WorkInstruction>> splitList = new List<IEnumerable<WorkInstruction>>();
      splitList.Add(from instruction in db.JobWorkInstructions[job]
                    where db.JobWorkInstructions[job].IndexOf(instruction) < db.JobWorkInstructions[job].Count / 2
                    select instruction);
      splitList.Add(from instruction in db.JobWorkInstructions[job]
                    where db.JobWorkInstructions[job].IndexOf(instruction) >= db.JobWorkInstructions[job].Count / 2
                    select instruction);
      return splitList;
    }

    public List<List<WorkInstruction>> cloneWorkInstructions(string jobId) {
      Job job = db.JobWorkInstructions.Keys.First(y => y.Id == jobId);
      List<List<WorkInstruction>> clonedList = new List<List<WorkInstruction>>() { db.JobWorkInstructions[job] };
      clonedList.Add(db.JobWorkInstructions[job]);
      return clonedList;
    }

    public void AddSpec(string jobId, string workId, OpSpec spec) {
      Job job = db.JobWorkInstructions.Keys.First(y => y.Id == jobId);
      db.JobWorkInstructions[job].First(y => y.Id == workId).OpSpecs.Add(spec);
    }

    public void ChangeSpec(string jobId, string workId, string oldSpecName, OpSpec newSpec) {
      Job job = db.JobWorkInstructions.Keys.First(y => y.Id == jobId);
      WorkInstruction workInstruction = db.JobWorkInstructions[job].First(y => y.Id == workId);
      OpSpec oldOpSpec = workInstruction.OpSpecs.First(y => y.Name == oldSpecName);
      var index = workInstruction.OpSpecs.IndexOf(oldOpSpec);
      if (index != -1) {
        workInstruction.OpSpecs[index] = newSpec;
        workInstruction.Approved = false;
      }
    }

    public void DeleteSpec(string jobId, string workId, string specName) {
      Job job = db.JobWorkInstructions.Keys.First(y => y.Id == jobId);
      db.JobWorkInstructions[job].First(y => y.Id == workId).OpSpecs.Remove(
        db.JobWorkInstructions[job].First(y => y.Id == workId).OpSpecs.First(y => y.Name == specName));
    }

    public void MergeSpecs(string jobId, string workId1, string workId2) {
      Job job = db.JobWorkInstructions.Keys.First(y => y.Id == jobId);
      WorkInstruction workInstruction1 = db.JobWorkInstructions[job].First(y => y.Id == workId1);
      WorkInstruction workInstruction2 = db.JobWorkInstructions[job].First(y => y.Id == workId2);
      workInstruction1.OpSpecs = workInstruction1.OpSpecs.Union(workInstruction2.OpSpecs).ToList();
    }

    public List<IEnumerable<OpSpec>> SplitSpecs(string jobId, string workId) {
      Job job = db.JobWorkInstructions.Keys.First(y => y.Id == jobId);
      WorkInstruction workInstruction = db.JobWorkInstructions[job].First(y => y.Id == workId);
      List<IEnumerable<OpSpec>> splitList = new List<IEnumerable<OpSpec>>();
      splitList.Add(from spec in workInstruction.OpSpecs
                    where workInstruction.OpSpecs.IndexOf(spec) < workInstruction.OpSpecs.Count / 2
                    select spec);
      splitList.Add(from spec in workInstruction.OpSpecs
                    where workInstruction.OpSpecs.IndexOf(spec) >= workInstruction.OpSpecs.Count / 2
                    select spec);
      return splitList;
    }

    public List<List<OpSpec>> CloneSpecs(string jobId, string workId) {
      Job job = db.JobWorkInstructions.Keys.First(y => y.Id == jobId);
      WorkInstruction workInstruction = db.JobWorkInstructions[job].First(y => y.Id == workId);
      List<List<OpSpec>> clonedList = new List<List<OpSpec>>() { workInstruction.OpSpecs };
      clonedList.Add(workInstruction.OpSpecs);
      return clonedList;
    }

    public void CreateQualityClause(Revision revision, QualityClause qualityClause) {
      if (revision.Category == "job") {
        revision.Clauses.Add(qualityClause);
      } else {
        Console.Write("You can only add a quality clause to a particular revision of a job.");
      }
    }

    public void MergeQualityClauses(Revision rev1, Revision rev2) {
      rev1.Clauses = rev1.Clauses.Union(rev2.Clauses).ToList();
    }

    public List<IEnumerable<QualityClause>> SplitQualityClauses(Revision rev) {
      List<IEnumerable<QualityClause>> splitList = new List<IEnumerable<QualityClause>>();
      splitList.Add(from clause in rev.Clauses
                    where rev.Clauses.IndexOf(clause) < rev.Clauses.Count / 2
                    select clause);
      splitList.Add(from clause in rev.Clauses
                    where rev.Clauses.IndexOf(clause) >= rev.Clauses.Count / 2
                    select clause);
      return splitList;
    }

    public List<List<QualityClause>> CloneQualityClauses(Revision rev) {
      List<List<QualityClause>> clonedList = new List<List<QualityClause>>() { rev.Clauses };
      clonedList.Add(rev.Clauses);
      return clonedList;
    }

    public void DisplayPriorRevisions(WorkInstruction input) {
      if (input.Revs.Count > 1) {
        foreach (Revision rev in input.Revs) {
          Console.Write(rev.Version);
        }
      }
    }

    public void DisplayPriorRevisions(QualityClause input) {
      if (input.Revs.Count > 1) {
        foreach (Revision rev in input.Revs) {
          Console.Write(rev.Version);
        }
      }
    }

    public void DisplayPriorRevisions(OpSpec input) {
      if (input.Revs.Count > 1) {
        foreach (Revision rev in input.Revs) {
          Console.Write(rev.Version);
        }
      }
    }

    public void DisplayLatestRevision(Revision JobRev) {
      WorkInstruction latest = new WorkInstruction();
      foreach (Job job in db.JobWorkInstructions.Keys) {
        if (job.Rev == JobRev) {
          latest = db.JobWorkInstructions[job].First();
        }
      }
      Console.Write(latest.Revs.Last().Version + "\n");
      Console.Write(latest.Images + "\n");
      Console.Write(latest.OpSpecs + "\n");
      Console.Write(latest.Revs.Last().Clauses + "\n");
    }
    */

  }
}
