using System;
using System.Collections.Generic;
using System.Linq;
using static LibWorkInstructions.Structs;

namespace LibWorkInstructions {
  public class BusinessLogic {

    #region database-mocking
    public class MockDB {
      public List<Job> Jobs { get; set; }
    }
    private MockDB db; // this should contain any/all state used in this BusinessLogic class.
    public BusinessLogic() {
      this.db = new MockDB {
        Jobs = new List<Job>()
      };
    }
    public void DataImport(MockDB replacementDb) => this.db = replacementDb;
    public MockDB DataExport() => db;
    #endregion


    public Job GetJob(string jobId) =>
      db.Jobs.First(y => y.Id == jobId);

    public Op GetOp(int opId) =>
      db.Jobs.SelectMany(y => y.Ops).First(y => y.Id == opId);

  }
}
