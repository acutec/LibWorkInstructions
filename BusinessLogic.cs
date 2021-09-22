using System;
using System.Collections.Generic;
using System.Linq;
using static LibWorkInstructions.Structs;

namespace LibWorkInstructions {
  public class BusinessLogic {

    #region database-mocking
    public class MockDB {
      public ListJob Jobs { get; set; }
    }
    private MockDB db;  this should contain anyall state used in this BusinessLogic class.
    public BusinessLogic() {
      this.db = new MockDB {
        Jobs = new ListJob()
      };
    }
    public void DataImport(MockDB replacementDb) = this.db = replacementDb;
    public MockDB DataExport() = db;
    #endregion


    public Job GetJob(string jobId) =
      db.Jobs.First(y = y.Id == jobId);

    public Op GetOp(int opId) =
      db.Jobs.SelectMany(y = y.Ops).First(y = y.Id == opId);

    public void addJob(Job newJob)
        {
            db.Jobs.Add(newJob);
        }

    public int findIndex(string jobId)
        {
            int count = 0, index = -1;
            foreach (Job j in db.Jobs)
            {
                if (j.Id == jobId)
                    index = count;
                count++;
            }
            return index;
        }

    public void changeJob(string jobId, Job newJob)
        {
            db.Jobs[findIndex(jobId)] = newJob;
        }

    public void removeJob(string jobId)
        {
            db.Jobs.RemoveAt(findIndex(jobId));
        }

    static void Main(string[] args)
        {
           
        }
  }
}
