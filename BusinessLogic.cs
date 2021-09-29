using System;
using System.Collections.Generic;
using System.Linq;
using static LibWorkInstructions.Structs;

namespace LibWorkInstructions {
  public class BusinessLogic {

    #region database-mocking
    public class MockDB {
      public List<Job> Jobs { get; set; }
      public List<WorkInstruction> workInstructions { get; set; }
      // TODO: Add more structure to Dictionary
      Dictionary<string, List<Job>> pastJobs = new Dictionary<string, List<Job>>();
    }
    private MockDB db;  // this should contain any/all state used in this BusinessLogic class.
    public BusinessLogic() {
      this.db = new MockDB();
    }
    public void DataImport(MockDB replacementDb) => this.db = replacementDb;
    public MockDB DataExport() => db;
    #endregion

    public Job GetJob(string jobId) =>
      db.Jobs.First(y => y.Id == jobId);

    public Op GetOp(int opId) =>
      db.Jobs.SelectMany(y => y.Ops).First(y => y.Id == opId);

    public void addWorkInstruction(WorkInstruction newWorkInstruction) {
            db.workInstructions.Add(newWorkInstruction);
        }

    public void changeWorkInstruction(string id, WorkInstruction newWorkInstruction)
        {
            WorkInstruction oldWorkInstruction = db.workInstructions.First(i=> i.Id == id);
            var index = db.workInstructions.IndexOf(oldWorkInstruction);
            
            if(index != -1)
                db.workInstructions[index] = newWorkInstruction;
        }

    public void removeWorkInstruction(string id)
        {
            db.workInstructions.Remove(db.workInstructions.First(i => i.Id == id));
        }

    public void mergeWorkInstructions(string id1, string id2)
        {
            WorkInstruction workInstruction1 = db.workInstructions.First(i => i.Id == id1);
            WorkInstruction workInstruction2 = db.workInstructions.First(i => i.Id == id2);
        }

    public void addSpec(string workId, OpSpec spec)
        {
            db.workInstructions.First(y => y.Id == workId).opSpecs.Add(spec);
        }

    public void changeSpec(string workId, string oldSpecName, OpSpec newSpec)
        {
            WorkInstruction workInstruction = db.workInstructions.First(y => y.Id == workId);
            OpSpec oldOpSpec = workInstruction.opSpecs.First(y => y.Name == oldSpecName);

            var index = workInstruction.opSpecs.IndexOf(oldOpSpec);

            if (index != -1) { 
                workInstruction.opSpecs[index] = newSpec;
                workInstruction.Approved = false;
             }
        }

    public void deleteSpec(string workId, string specName)
        {
            db.workInstructions.First(y => y.Id == workId).opSpecs.Remove(
                db.workInstructions.First(y =>y.Id == workId).opSpecs.First(y => y.Name == specName));
        }

    public void mergeSpecs(string workId1, string workId2)
        {
            WorkInstruction workInstruction1 = db.workInstructions.First(y => y.Id == workId1);
            WorkInstruction workInstruction2 = db.workInstructions.First(y => y.Id == workId2);
            workInstruction1.opSpecs = workInstruction1.opSpecs.Union(workInstruction2.opSpecs).ToList();
        }

    public List<IEnumerable<OpSpec>> splitSpecs (string workId) 
        {
            WorkInstruction workInstruction = db.workInstructions.First(y => y.Id == workId);
            List<IEnumerable<OpSpec>> splitList = new List<IEnumerable<OpSpec>>();
            splitList.Add(from spec in workInstruction.opSpecs
                          where workInstruction.opSpecs.IndexOf(spec) < workInstruction.opSpecs.Count / 2
                          select spec);
            splitList.Add(from spec in workInstruction.opSpecs
                          where workInstruction.opSpecs.IndexOf(spec) >= workInstruction.opSpecs.Count / 2
                          select spec);
            return splitList;
        }

    public List<List<OpSpec>> cloneSpecs (string workId)
        {
            WorkInstruction workInstruction = db.workInstructions.First(y => y.Id == workId);
            List<List<OpSpec>> clonedList = new List<List<OpSpec>>() {workInstruction.opSpecs};
            clonedList.Add(workInstruction.opSpecs);
            return clonedList;
        }

    public void createQualityClause(Revision revision, QualityClause qualityClause)
        {
            if(revision.Category == "job")
                revision.Clauses.Add(qualityClause);
            else
                Console.Write("You can only add a quality clause to a particular revision of a job.");
        }

    public void mergeQualityClauses(Revision rev1, Revision rev2)
        {
            rev1.Clauses = rev1.Clauses.Union(rev2.Clauses).ToList();
        }

    public List<IEnumerable<QualityClause>> splitQualityClauses(Revision rev)
        {
            List<IEnumerable<QualityClause>> splitList = new List<IEnumerable<QualityClause>>();
            splitList.Add(from clause in rev.Clauses
                          where rev.Clauses.IndexOf(clause) < rev.Clauses.Count / 2
                          select clause);
            splitList.Add(from clause in rev.Clauses
                          where rev.Clauses.IndexOf(clause) >= rev.Clauses.Count / 2
                          select clause);
            return splitList;
        }

    public List<List<QualityClause>> cloneQualityClauses(Revision rev)
        {
            List<List<QualityClause>> clonedList = new List<List<QualityClause>>() {rev.Clauses};
            clonedList.Add(rev.Clauses);
            return clonedList;
        }



  }
}

