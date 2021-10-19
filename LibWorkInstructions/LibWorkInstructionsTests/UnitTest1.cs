using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

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
    public void TestJobCalling()
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
    public void TestGetJob()
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
            Assert.True(n.GetJob("F110").Id.Equals(testJob.Id));
            Assert.True(n.GetJob("F110").Rev.Equals(testJob.Rev));
            Assert.True(n.GetJob("F110").RevCustomer.Equals(testJob.RevCustomer));
            Assert.True(n.GetJob("F110").RevPlan.Equals(testJob.RevPlan));
        }

    [Test]
    public void TestAddJob()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            LibWorkInstructions.Structs.Job testJob = new LibWorkInstructions.Structs.Job { Id = "F110", Rev = "A", RevCustomer = "CUSTX", RevPlan = "1.0.0", };
            n.AddJob(testJob);
            var dbVar = n.DataExport();
            // Check that List is empty
            Assert.True(dbVar.Jobs["F110"].Id.Equals(testJob.Id));
            Assert.True(dbVar.Jobs["F110"].Rev.Equals(testJob.Rev));
            Assert.True(dbVar.Jobs["F110"].RevCustomer.Equals(testJob.RevCustomer));
            Assert.True(dbVar.Jobs["F110"].RevPlan.Equals(testJob.RevPlan));
        }

    [Test]
    public void TestGetWorkInstruction()
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
            Assert.True(n.GetWorkInstruction(0).Id.Equals(testWorkInstruction.Id));
            Assert.True(n.GetWorkInstruction(0).IdRevGroup.Equals(testWorkInstruction.IdRevGroup));
            Assert.True(n.GetWorkInstruction(0).Approved.Equals(testWorkInstruction.Approved));
            Assert.True(n.GetWorkInstruction(0).HtmlBlob.Equals(testWorkInstruction.HtmlBlob));
            Assert.True(n.GetWorkInstruction(0).Images[0].Equals(testWorkInstruction.Images[0]));
            Assert.True(n.GetWorkInstruction(0).OpSpecs[0].Equals(testWorkInstruction.OpSpecs[0]));
        }

    [Test]
    public void TestAddWorkInstruction()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            LibWorkInstructions.Structs.WorkInstruction sampleData = new LibWorkInstructions.Structs.WorkInstruction {
                Id = 0,
                IdRevGroup = 0,
                Approved = true,
                HtmlBlob = "<h1>do something</h1>",
                Images = new List<string> { "image" },
                OpSpecs = new List<int> { 1 },
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
            n.AddWorkInstruction(sampleData);
            var dbVar = n.DataExport();
            Assert.True(dbVar.WorkInstructions[0].Id.Equals(testWorkInstruction.Id));
            Assert.True(dbVar.WorkInstructions[0].IdRevGroup.Equals(testWorkInstruction.IdRevGroup));
            Assert.True(dbVar.WorkInstructions[0].Approved.Equals(testWorkInstruction.Approved));
            Assert.True(dbVar.WorkInstructions[0].HtmlBlob.Equals(testWorkInstruction.HtmlBlob));
            Assert.True(dbVar.WorkInstructions[0].Images[0].Equals(testWorkInstruction.Images[0]));
            Assert.True(dbVar.WorkInstructions[0].OpSpecs[0].Equals(testWorkInstruction.OpSpecs[0]));
        }

        [Test]
        public void TestChangeWorkInstruction()
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
            LibWorkInstructions.Structs.WorkInstruction newWorkInstruction = new LibWorkInstructions.Structs.WorkInstruction
            {
                Id = 1,
                IdRevGroup = 1,
                Approved = false,
                HtmlBlob = "<h2>do something</h2>",
                Images = new List<string> { "jpeg" },
                OpSpecs = new List<int> { 0 },
            };
            n.DataImport(sampleData);
            n.ChangeWorkInstruction(0, newWorkInstruction);
            var dbVar = n.DataExport();
            Assert.True(dbVar.WorkInstructions[0].Id.Equals(newWorkInstruction.Id));
            Assert.True(dbVar.WorkInstructions[0].IdRevGroup.Equals(newWorkInstruction.IdRevGroup));
            Assert.True(dbVar.WorkInstructions[0].Approved.Equals(newWorkInstruction.Approved));
            Assert.True(dbVar.WorkInstructions[0].HtmlBlob.Equals(newWorkInstruction.HtmlBlob));
            Assert.True(dbVar.WorkInstructions[0].Images[0].Equals(newWorkInstruction.Images[0]));
            Assert.True(dbVar.WorkInstructions[0].OpSpecs[0].Equals(newWorkInstruction.OpSpecs[0]));
        }

        [Test]
        public void TestRemoveWorkInstruction()
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
                        Images = new List<string> { "image" },
                        OpSpecs = new List<int> { 1 },
                    } },
                    { 1, new LibWorkInstructions.Structs.WorkInstruction {
                        Id = 1,
                        IdRevGroup = 1,
                        Approved = false,
                        HtmlBlob = "<h2>do something</h2>",
                        Images = new List<string> { "jpeg" },
                        OpSpecs = new List<int> { 0 },
                    } },
                }
            };
            n.DataImport(sampleData);
            Assert.True(n.GetWorkInstruction(0).Id.Equals(0));
            Assert.True(n.GetWorkInstruction(1).Id.Equals(1));
            n.RemoveWorkInstruction(0);
            var dbVar = n.DataExport();
            Assert.False(dbVar.WorkInstructions.ContainsKey(0));
            Assert.True(dbVar.WorkInstructions.ContainsKey(1));
        }
        [Test]
        public void TestMergeWorkInstructions()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                JobRefToWorkInstructionRefs = new Dictionary<string, List<List<int>>> {
                    {"job1" , new List<List<int>>()
                    {new List<int> {0001, 0002, 0003}, new List<int> {1001, 1002, 1003} }},
                    {"job2", new List<List<int>>()
                    {new List<int> {2001, 2002, 2003}, new List<int> {3001, 3002, 3003} } },
                }
            };
            n.DataImport(sampleData);
            var dbPreMerge = n.DataExport();
            n.MergeWorkInstructions(2001, 1001);
            var dbPostMerge = n.DataExport();
            var mergedInstruction = new List<int>{ 0001, 0002, 0003, 2001, 2002, 2003 };
            var workInstruction1 = dbPreMerge.JobRefToWorkInstructionRefs["job1"][0];
            var workInstruction2 = dbPreMerge.JobRefToWorkInstructionRefs["job2"][0];
            Assert.True(dbPostMerge.JobRefToWorkInstructionRefs["job1"].Contains(mergedInstruction));
            Assert.True(dbPostMerge.JobRefToWorkInstructionRefs["job1"].Contains(mergedInstruction));
            Assert.False(dbPostMerge.JobRefToWorkInstructionRefs["job1"].Contains(workInstruction1));
            Assert.False(dbPostMerge.JobRefToWorkInstructionRefs["job2"].Contains(workInstruction2));
        }

        [Test]
        public void TestSplitWorkInstruction()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                JobRefToWorkInstructionRefs = new Dictionary<string, List<List<int>>> {
                    {"job1" , new List<List<int>>()
                    {new List<int> {0001, 0002, 0003}, new List<int> {1001, 1002, 1003} }},
                    {"job2", new List<List<int>>()
                    {new List<int> {2001, 2002, 2003}, new List<int> {3001, 3002, 3003} } },
                }
            };
            n.DataImport(sampleData);
            n.SplitWorkInstruction(2001);
            var dbPostSplit = n.DataExport();

            Assert.True(Enumerable.ToList(dbPostSplit.JobRefToWorkInstructionRefs["job2"])
                .Where(y => y.Equals(new List<int> { 2001, 2002, 2003})).Count() == 2);
        }

        [Test]
        public void TestCloneWorkInstruction()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                JobRefToWorkInstructionRefs = new Dictionary<string, List<List<int>>> {
                    {"job1" , new List<List<int>>()
                    {new List<int> {0001, 0002, 0003}, new List<int> {1001, 1002, 1003} }},
                    {"job2", new List<List<int>>()
                    {new List<int> {2001, 2002, 2003}, new List<int> {3001, 3002, 3003} } },
                }
            };

            n.DataImport(sampleData);
            n.CloneWorkInstruction(2001, "job1");
            var dbPostClone = n.DataExport();

            Assert.True(dbPostClone.JobRefToWorkInstructionRefs["job1"].Contains(new List<int> { 2001, 2002, 2003 }));
        }

        [Test]
        public void TestAddSpec()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                OpSpecs = new Dictionary<int, LibWorkInstructions.Structs.OpSpec>()
            };

            n.DataImport(sampleData);
            n.AddSpec(new LibWorkInstructions.Structs.OpSpec { Id = 1 });
            var dbPostAdd = n.DataExport();
            Assert.True(dbPostAdd.OpSpecs.ContainsKey(1));
        }
  }
}