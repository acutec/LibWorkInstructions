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
      // Operations have a particular revision that needs to be referred to in order to ensure the correct operation is being executed for a job
      public string Rev { get; set; }
      // Position of an operation revision
      public int RevSeq { get; set; }
      // Status of the job revision
      public bool Active { get; set; }
      // Jobs have a sequence of operations.
      public List<Op> Ops { get; set; }
      // Jobs also have a collection of quality clauses.
      public List<QualityClause> QualityClauses { get; set; }
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

    // Work instructions are specific to a particular revision of an operation
    public class WorkInstruction
    {
        // Identifier for work instructions (unique per rev)
        public Guid Id { get; set; }
        // Identifier for work instructions (unique per group of revs)
        public Guid IdRevGroup { get; set; }
        public int RevSeq { get; set; }
        // There's a list of images for a particular work instruction
        public List<string> Images { get; set; }
        // Work instructions have an approval status
        public bool Approved { get; set; }
        // Placeholder for rich content
        public string HtmlBlob { get; set; }
        // op the work instruction is linked to
        public int OpId { get; set; }
        // Work instructions also have an active status
        public bool Active { get; set; }

        public bool Equals(WorkInstruction obj)
        {
            if (Id != obj.Id)
                return false;
            if (IdRevGroup != obj.IdRevGroup)
                return false;
            if (Images != obj.Images)
                return false;
            if (Approved != obj.Approved)
                return false;
            if (HtmlBlob != obj.HtmlBlob)
                return false;
            return true;
        }
    }

    public class OpSpec {
      public Guid Id { get; set; }
      public Guid IdRevGroup { get; set; }
      public int RevSeq { get; set; }
      public string Name { get; set; }
      public string Notice { get; set;  }
      public string Class { get; set; }
      public string Type { get; set; }
      public string Method { get; set; }
      public string Grade { get; set; }
      public string Level { get; set; }
      public string Proctype { get; set; }
      public string Servicecond { get; set; }
      public string Status { get; set; }
      public string Comment { get; set; }
      public bool Active { get; set; }
    }
    
    public class QualityClause {
      public Guid Id { get; set; }
      public Guid IdRevGroup { get; set; }
      public int RevSeq { get; set; }
      public string Clause { get; set; }
      public bool Active { get; set; }
    }

    public class Event
    {
        public string Action { get; set; }
        // Set values below to nullable values, until we find a better way to hanlde these multiple types
        // Should be handled fine, given that each function that will use these will be simple and be able to call upon what it needs
        public Dictionary<string, string> Args { get; set; }
        public DateTime When { get; set; }
    }

  }
}
