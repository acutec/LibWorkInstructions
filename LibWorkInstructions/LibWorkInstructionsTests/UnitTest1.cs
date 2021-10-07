using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace LibWorkInstructionsTests {
  public class Tests {
    [SetUp]
    public void Setup() {
    }

    [Test]
    public void Test1() {
      Assert.Pass();
    }

    [Test]
    public void IntentionallyFailingTestCaseAsAnExample() {
      Assert.Fail();
    }

    [Test]
    public void ExampleCallingTheLibrary() {
      var n = new LibWorkInstructions.BusinessLogic();
      var sampleData = new LibWorkInstructions.BusinessLogic.MockDB {
        Jobs = new Dictionary<string, LibWorkInstructions.Structs.Job> {
          { "F110", new LibWorkInstructions.Structs.Job {
            Id = "F110",
            Rev = "A",
            RevCustomer = "CUSTX",
            RevPlan = "1.0.0",
          }},
          { "E444", new LibWorkInstructions.Structs.Job {
            Id = "E444",
            Rev = "C",
            RevCustomer = "CUSTY",
            RevPlan = "7.1.12",
          }},
        },
        QualityClauses = new Dictionary<int, LibWorkInstructions.Structs.QualityClause> {
          { 0, new LibWorkInstructions.Structs.QualityClause {
            Id = 0,
            IdRevGroup = 0,
            Clause = "Workmanship...",
          }},
        },
        OpSpecs = new Dictionary<int, LibWorkInstructions.Structs.OpSpec> {
          { 0, new LibWorkInstructions.Structs.OpSpec {
            Id = 0,
            IdRevGroup = 0,
            // FIXME: some more fields could get set.
          }},
          { 1, new LibWorkInstructions.Structs.OpSpec {
            Id = 1,
            IdRevGroup = 0, // another rev of the original id=0 one.
            // FIXME: some more fields could get set.
          }},
        },
        WorkInstructions = new Dictionary<int, LibWorkInstructions.Structs.WorkInstruction> {
          { 0, new LibWorkInstructions.Structs.WorkInstruction {
            Id = 0,
            IdRevGroup = 0,
            Approved = true,
            HtmlBlob = "<h1>do something</h1>",
            Images = new List<string>{ },
            OpSpecs = new List<int> { 1 },
          }},
        },
        JobRefToQualityClauseRefs = new Dictionary<string, List<int>> {
          { "F110", new List<int> { 0 } },
        },
        JobRefToWorkInstructionRefs = new Dictionary<string, List<List<int>>> {
          { "F110", new List<List<int>> {
            new List<int> { }
          }},
        },
      };
      n.DataImport(sampleData);
      // FIXME: call some methods...
      // FIXME: maybe assert some things on returned values...
      var export = n.DataExport();
      // FIXME: assert some things on the exported data...
    }
    

    [Test]
    public void testJobCalling()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                Jobs = new Dictionary<string, LibWorkInstructions.Structs.Job> {
              { "F110", new LibWorkInstructions.Structs.Job {
                Id = "F110",
                Rev = "A",
                RevCustomer = "CUSTX",
                RevPlan = "1.0.0",
              }},
            }
            };
            n.DataImport(sampleData);
            var dbVar = n.DataExport();
            LibWorkInstructions.Structs.Job testJob = new LibWorkInstructions.Structs.Job{Id = "F110", Rev = "A", RevCustomer = "CUSTX", RevPlan = "1.0.0",};
            // Console.WriteLine($"DbVar: {dbVar.Jobs["F110"]}\nTestJob: {testJob}");
            Assert.True(dbVar.Jobs["F110"].Id.Equals(testJob.Id));
            Assert.True(dbVar.Jobs["F110"].Rev.Equals(testJob.Rev));
            Assert.True(dbVar.Jobs["F110"].RevCustomer.Equals(testJob.RevCustomer));
            Assert.True(dbVar.Jobs["F110"].RevPlan.Equals(testJob.RevPlan));
        }

    [Test]
    public void testGetJob()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                Jobs = new Dictionary<string, LibWorkInstructions.Structs.Job> {
              { "F110", new LibWorkInstructions.Structs.Job {
                Id = "F110",
                Rev = "A",
                RevCustomer = "CUSTX",
                RevPlan = "1.0.0",
              }},
            }
            };
            n.DataImport(sampleData);
            LibWorkInstructions.Structs.Job testJob = new LibWorkInstructions.Structs.Job { Id = "F110", Rev = "A", RevCustomer = "CUSTX", RevPlan = "1.0.0", };
            //Console.WriteLine($"DbVar: {dbVar.Jobs["F110"]}\nTestJob: {testJob}");
            Assert.True(n.getJob("F110").Id.Equals(testJob.Id));
            Assert.True(n.getJob("F110").Rev.Equals(testJob.Rev));
            Assert.True(n.getJob("F110").RevCustomer.Equals(testJob.RevCustomer));
            Assert.True(n.getJob("F110").RevPlan.Equals(testJob.RevPlan));
        }

    [Test]
    public void testAddJob()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            LibWorkInstructions.Structs.Job testJob = new LibWorkInstructions.Structs.Job { Id = "F110", Rev = "A", RevCustomer = "CUSTX", RevPlan = "1.0.0", };
            n.addJob(testJob);
            var dbVar = n.DataExport();
            // Check that List is empty
            Assert.True(dbVar.Jobs["F110"].Id.Equals(testJob.Id));
            Assert.True(dbVar.Jobs["F110"].Rev.Equals(testJob.Rev));
            Assert.True(dbVar.Jobs["F110"].RevCustomer.Equals(testJob.RevCustomer));
            Assert.True(dbVar.Jobs["F110"].RevPlan.Equals(testJob.RevPlan));
        }

    [Test]
    public void testGetWorkInstruction()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
            WorkInstructions = new Dictionary<int, LibWorkInstructions.Structs.WorkInstruction> {
                { 0, new LibWorkInstructions.Structs.WorkInstruction {
                Id = 0,
                IdRevGroup = 0,
                Approved = true,
                HtmlBlob = "<h1>do something</h1>",
                Images = new List<string>{ "image" },
                OpSpecs = new List<int> { 1 },
                }},
                },
            };
            LibWorkInstructions.Structs.WorkInstruction testWorkInstruction = new LibWorkInstructions.Structs.WorkInstruction
            {
                Id = 0,
                IdRevGroup = 0,
                Approved = true,
                HtmlBlob = "<h1>do something</h1>",
                Images = new List<string> { "image" },
                OpSpecs = new List<int> { 1 },
            };
            n.DataImport(sampleData);
            Assert.True(n.getWorkInstruction(0).Id.Equals(testWorkInstruction.Id));
            Assert.True(n.getWorkInstruction(0).IdRevGroup.Equals(testWorkInstruction.IdRevGroup));
            Assert.True(n.getWorkInstruction(0).Approved.Equals(testWorkInstruction.Approved));
            Assert.True(n.getWorkInstruction(0).HtmlBlob.Equals(testWorkInstruction.HtmlBlob));
            Assert.True(n.getWorkInstruction(0).Images[0].Equals(testWorkInstruction.Images[0]));
            Assert.True(n.getWorkInstruction(0).OpSpecs[0].Equals(testWorkInstruction.OpSpecs[0]));
        }
  }
}