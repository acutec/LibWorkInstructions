using System;
using System.Collections.Generic;

namespace LibWorkInstructions {
  public class Structs {

    public class Job {
      // Jobs are uniquely identified by strings, which follow complex naming conventions we needn't cover here; treat as a blob.
      public string Id { get; set; }
      // Jobs have a revision identifier flowed down from the customer, that can change over time.
      public string RevCustomer { get; set; }
      // Jobs have a revision identifier representing our internal process, that can change over time.
      public string RevPlan { get; set; }
      // Jobs have a series of operations associated with them.
      public List<Op> Ops { get; set; }
    }

    public class Op {
      // Operations are uniquely identified by integers, we have over 1.5 million of them in our database.
      public int Id { get; set; }
      // Operations are associated with a single job (this and Seq below are encoded in the in-memory data structure but listed here as the DB would store them).
      public string JobId { get; set; }
      // Operations are known by most people by this displaytext rather than the integer identifier, e.g. "Op 20", "Op 30", etc.
      public string OpService { get; set; }
      // Operations have an ordering within a job, this represents that ordering.
      public int Seq { get; set; }
    }

  }
}
