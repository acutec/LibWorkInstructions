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
            oldWorkInstruction = db.workInstructions.First(i=> i.Id == id);
            var index = db.workInstructions.indexOf(oldWorkInstruction);
            
            if(index != -1)
                db.workInstructions[index] = newWorkInstruction;
        }

    public IEnumerable<WorkInstruction> removeWorkInstruction(string id)
        {
            return (from workInstruction in db.workInstructions 
                    where workInstruction.Id != id 
                    select workInstruction);
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

            if (index != -1)
                workInstruction.opSpecs[index] = newSpec;
        }
  }
}

