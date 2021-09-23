using System;
using System.Collections.Generic;
using System.Linq;
using static LibWorkInstructions.Structs;

namespace LibWorkInstructions {
  public class BusinessLogic {

    #region database-mocking
    public class MockDB {
      public List<Job> Jobs { get; set; }
      // TODO: Add more structure to Dictionary
      Dictionary<string, Job> pastJobs = new Dictionary<string, Job>();
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

    public void addJob(Job newJob) {
            db.Jobs.Add(newJob);
        }

    public int findIndex(string jobId) {
            // TODO: Probably don't need indexing, possibly find another solution
            int count = 0, index = -1;
            foreach (Job j in db.Jobs)
            {
                if (j.Id == jobId)
                    index = count;
                count++;
            }
            return index;
        }

    public void changeJob(Job job, Job newJob) {
            db.Jobs[findIndex(job.Id)] = newJob;
            // To be changed with clarification
            // string jobID = (job.id) + job.RevCustomer;
            // pastJobs.Add(job.Id, job);
        }

    public void removeJob(string jobId) {
            db.Jobs.RemoveAt(findIndex(jobId));
    }
  }
}

