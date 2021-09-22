using System;
using System.Collections.Generic;
using System.Linq;
using static LibWorkInstructions.Structs;

namespace LibWorkInstructions {
  public class BusinessLogic {

    #region database-mocking
    public class MockDB {
      public List<Job> Jobs { get; set; }
      public List<OpSpec> opSpecs { get; set; }
      public List<QualityClause> qualityClauses { get; set; }
    }
    private MockDB db;  //this should contain any all state used in this BusinessLogic class.
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

    public int findJobIndex(string jobId)
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

    public int findSpecIndex(string name)
        {
            int count = 0, index = -1;
            foreach (OpSpec s in db.opSpecs)
            {
                if(s.Name == name)
                    index = count;
                count++;
            }
            return index
        }

    public void addJob(Job newJob)
        {
            db.Jobs.Add(newJob);
        }

    public void changeJob(Job job, Job newJob)
        {
            db.Jobs[findJobIndex(job.Id)] = newJob;
        }

    public void removeJob(string jobId)
        {
            db.Jobs.RemoveAt(findJobIndex(jobId));
        }

     public void addSpec(OpSpec spec)
        {
            db.opSpecs.Add(spec);
        }

    public void changeSpec(OpSpec spec, OpSpec newSpec)
        {
            db.opSpecs[findSpecIndex(spec.Name)] = newSpec;
        }

    public void removeSpec(string specName)
        {
            db.opSpecs.RemoveAt(findSpecIndex(specName));
        }

    public void addQualityClause(QualityClause clause)
        {
            db.qualityClauses.Add(clause);
        }

    static void Main(string[] args)
        {
           
        }
  }
}
