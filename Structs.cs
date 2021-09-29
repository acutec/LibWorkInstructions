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
      public List<Revision> JobRevs { get; set; }
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

    // Work instructions are specific to a particular revision of an operation
    public class WorkInstruction {
        // Identifier for work instructions
        public string Id { get; set; }
        // There's a list of images for a particular work instruction
        public List<string> Images { get; set; }
        // Work instructions have an approval status
        public bool Approved { get; set; }
        // Work instructions can have one or more revisions
        public Revision Rev { get; set;}
        // Placeholder for rich content
        public List<string> HtmlBlob { get; set;}

        public List<OpSpec> OpSpecs {  get; set; }
        
        }
    // TODO: Possibly add description of classes

    public class OpSpec {
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
        public List<Revision> SpecRevs { get; set; }

        }
    
    public class QualityClause
        {
        public string Clause { get; set; }

        public List<Revision> ClauseRevs { get; set; }
        }

    public class Revision
        {
        public string Category { get; set; }
        public string Version { get; set; }
        public List<QualityClause> Clauses { get; set; }
        #nullable enable
        public WorkInstruction? Instruction { get; set; }
        #nullable disable
        }
    }
}

