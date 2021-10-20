using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibWorkInstructionsTests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            Assert.Pass();
        }

        [Test]
        public void ExampleCallingTheLibrary()
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
            LibWorkInstructions.Structs.Job testJob = new LibWorkInstructions.Structs.Job { Id = "F110", Rev = "A", RevCustomer = "CUSTX", RevPlan = "1.0.0", };
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
            LibWorkInstructions.Structs.WorkInstruction sampleData = new LibWorkInstructions.Structs.WorkInstruction
            {
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
                },
                WorkInstructions = new Dictionary<int, LibWorkInstructions.Structs.WorkInstruction>
                {
                    {0001, new LibWorkInstructions.Structs.WorkInstruction()},
                    {0002, new LibWorkInstructions.Structs.WorkInstruction()},
                    {0003, new LibWorkInstructions.Structs.WorkInstruction()},
                    {1001, new LibWorkInstructions.Structs.WorkInstruction()},
                    {1002, new LibWorkInstructions.Structs.WorkInstruction()},
                    {1003, new LibWorkInstructions.Structs.WorkInstruction()},
                    {2001, new LibWorkInstructions.Structs.WorkInstruction()},
                    {2002, new LibWorkInstructions.Structs.WorkInstruction()},
                    {2003, new LibWorkInstructions.Structs.WorkInstruction()},
                    {3001, new LibWorkInstructions.Structs.WorkInstruction()},
                    {3002, new LibWorkInstructions.Structs.WorkInstruction()},
                    {3003, new LibWorkInstructions.Structs.WorkInstruction()},
                },
            };
            n.DataImport(sampleData);
            var dbPreMerge = n.DataExport();
            var workInstruction1 = dbPreMerge.JobRefToWorkInstructionRefs["job1"][1];
            var workInstruction2 = dbPreMerge.JobRefToWorkInstructionRefs["job2"][0];
            n.MergeWorkInstructions(2001, 1001);
            var dbPostMerge = n.DataExport();
            Assert.True(dbPostMerge.JobRefToWorkInstructionRefs["job1"].FindIndex(f =>
            f.SequenceEqual(new List<int> { 2001, 2002, 2003, 1001, 1002, 1003 })) >= 0);
            Assert.True(dbPostMerge.JobRefToWorkInstructionRefs["job2"].FindIndex(f =>
            f.SequenceEqual(new List<int> { 2001, 2002, 2003, 1001, 1002, 1003 })) >= 0);
            Assert.False(dbPostMerge.JobRefToWorkInstructionRefs["job1"].FindIndex(f =>
            f.SequenceEqual(workInstruction1)) >= 0);
            Assert.False(dbPostMerge.JobRefToWorkInstructionRefs["job2"].FindIndex(f =>
            f.SequenceEqual(workInstruction2)) >= 0);
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
                },
                WorkInstructions = new Dictionary<int, LibWorkInstructions.Structs.WorkInstruction>
                {
                    {0001, new LibWorkInstructions.Structs.WorkInstruction()},
                    {0002, new LibWorkInstructions.Structs.WorkInstruction()},
                    {0003, new LibWorkInstructions.Structs.WorkInstruction()},
                    {1001, new LibWorkInstructions.Structs.WorkInstruction()},
                    {1002, new LibWorkInstructions.Structs.WorkInstruction()},
                    {1003, new LibWorkInstructions.Structs.WorkInstruction()},
                    {2001, new LibWorkInstructions.Structs.WorkInstruction()},
                    {2002, new LibWorkInstructions.Structs.WorkInstruction()},
                    {2003, new LibWorkInstructions.Structs.WorkInstruction()},
                    {3001, new LibWorkInstructions.Structs.WorkInstruction()},
                    {3002, new LibWorkInstructions.Structs.WorkInstruction()},
                    {3003, new LibWorkInstructions.Structs.WorkInstruction()},
                },
                Jobs = new Dictionary<string, LibWorkInstructions.Structs.Job>
                {
                    {"job1", new LibWorkInstructions.Structs.Job()},
                    {"job2", new LibWorkInstructions.Structs.Job()},
                }
            };
            n.DataImport(sampleData);
            n.SplitWorkInstruction(2001, "job1");
            var dbPostSplit = n.DataExport();

            Assert.True(dbPostSplit.JobRefToWorkInstructionRefs["job1"].FindIndex(f =>
            f.SequenceEqual(new List<int> { 2001, 2002, 2003 })) >= 0);
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
                },
                WorkInstructions = new Dictionary<int, LibWorkInstructions.Structs.WorkInstruction>
                {
                    {0001, new LibWorkInstructions.Structs.WorkInstruction()},
                    {0002, new LibWorkInstructions.Structs.WorkInstruction()},
                    {0003, new LibWorkInstructions.Structs.WorkInstruction()},
                    {1001, new LibWorkInstructions.Structs.WorkInstruction()},
                    {1002, new LibWorkInstructions.Structs.WorkInstruction()},
                    {1003, new LibWorkInstructions.Structs.WorkInstruction()},
                    {2001, new LibWorkInstructions.Structs.WorkInstruction()},
                    {2002, new LibWorkInstructions.Structs.WorkInstruction()},
                    {2003, new LibWorkInstructions.Structs.WorkInstruction()},
                    {3001, new LibWorkInstructions.Structs.WorkInstruction()},
                    {3002, new LibWorkInstructions.Structs.WorkInstruction()},
                    {3003, new LibWorkInstructions.Structs.WorkInstruction()},
                },
                Jobs = new Dictionary<string, LibWorkInstructions.Structs.Job>
                {
                    {"job1", new LibWorkInstructions.Structs.Job() },
                    {"job2", new LibWorkInstructions.Structs.Job() },
                },
            };

            n.DataImport(sampleData);
            n.CloneWorkInstruction(2001, "job1");
            var dbPostClone = n.DataExport();

            Assert.True(dbPostClone.JobRefToWorkInstructionRefs["job1"].FindIndex(f => 
            f.SequenceEqual(new List<int> {2001, 2002, 2003})) >= 0);
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

        [Test]
        public void TestChangeSpec()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                OpSpecs = new Dictionary<int, LibWorkInstructions.Structs.OpSpec>
                {
                    { 1, new LibWorkInstructions.Structs.OpSpec() {Id = 1, Name = "spec1"} },
                    { 2, new LibWorkInstructions.Structs.OpSpec() {Id = 2, Name = "spec2"} },
                    { 3, new LibWorkInstructions.Structs.OpSpec() {Id = 3, Name = "spec3"} },
                },
                WorkInstructions = new Dictionary<int, LibWorkInstructions.Structs.WorkInstruction>
                {
                    { 11, new LibWorkInstructions.Structs.WorkInstruction() 
                    {OpSpecs = new List<int> { 1, 2 }, Approved = true} },
                    { 12, new LibWorkInstructions.Structs.WorkInstruction() 
                    {OpSpecs = new List<int>{ 2, 3 }, Approved = true} },
                    { 13, new LibWorkInstructions.Structs.WorkInstruction() 
                    {OpSpecs = new List<int>{ 1, 3 }, Approved = true} },
                }
            };
            n.DataImport(sampleData);
            n.ChangeSpec(2, new LibWorkInstructions.Structs.OpSpec() { Id = 2, Name = "spec2.3" });
            var dbPostChange = n.DataExport();

            Assert.True(dbPostChange.OpSpecs[2].Id == 2);
            Assert.True(dbPostChange.OpSpecs[2].Name == "spec2.3");
            Assert.False(dbPostChange.WorkInstructions[11].Approved);
            Assert.False(dbPostChange.WorkInstructions[12].Approved);
            Assert.True(dbPostChange.WorkInstructions[13].Approved);
        }

        [Test]
        public void TestDeleteSpec()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                OpSpecs = new Dictionary<int, LibWorkInstructions.Structs.OpSpec>
                {
                    { 1, new LibWorkInstructions.Structs.OpSpec() {Id = 1, Name = "spec1"} },
                    { 2, new LibWorkInstructions.Structs.OpSpec() {Id = 2, Name = "spec2"} },
                    { 3, new LibWorkInstructions.Structs.OpSpec() {Id = 3, Name = "spec3"} },
                }
            };

            n.DataImport(sampleData);
            n.DeleteSpec(1);
            var dbPostDelete = n.DataExport();
            Assert.False(dbPostDelete.OpSpecs.ContainsKey(1));
        }

        [Test]
        public void TestMergeSpecs()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                WorkInstructions = new Dictionary<int, LibWorkInstructions.Structs.WorkInstruction>
                {
                    {1, new LibWorkInstructions.Structs.WorkInstruction{Id = 1, OpSpecs = new List<int>{5423, 3217, 1243}}},
                    {2, new LibWorkInstructions.Structs.WorkInstruction{Id = 2, OpSpecs = new List<int>{3245, 7123, 3421}}},
                    {3, new LibWorkInstructions.Structs.WorkInstruction{Id = 3, OpSpecs = new List<int>{3246, 7123, 3422}}},
                }
            };

            n.DataImport(sampleData);
            n.MergeSpecs(2, 3);
            var dbPostMerge = n.DataExport();
            Assert.True(dbPostMerge.WorkInstructions[2].OpSpecs.SequenceEqual(new List<int> { 3245, 7123, 3421, 3246, 3422 }));
            Assert.True(dbPostMerge.WorkInstructions[3].OpSpecs.SequenceEqual(new List<int> { 3245, 7123, 3421, 3246, 3422 }));
        }

        [Test]
        public void TestSplitSpecs()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                WorkInstructions = new Dictionary<int, LibWorkInstructions.Structs.WorkInstruction>
                {
                    {1, new LibWorkInstructions.Structs.WorkInstruction{Id = 1, OpSpecs = new List<int>{5423, 3217, 1243}}},
                    {2, new LibWorkInstructions.Structs.WorkInstruction{Id = 2, OpSpecs = new List<int>{3245, 7123, 3421}}},
                    {3, new LibWorkInstructions.Structs.WorkInstruction{Id = 3, OpSpecs = new List<int>{3246, 7123, 3422}}},
                    {4, new LibWorkInstructions.Structs.WorkInstruction()},
                }
            };

            n.DataImport(sampleData);
            n.SplitSpecs(2, 4);
            var dbPostSplit = n.DataExport();
            Assert.True(dbPostSplit.WorkInstructions[2].OpSpecs.SequenceEqual(new List<int> { 3245, 7123, 3421 }));
            Assert.True(dbPostSplit.WorkInstructions[4].OpSpecs.SequenceEqual(new List<int> { 3245, 7123, 3421 }));
        }

        [Test]
        public void TestCloneSpecs()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                WorkInstructions = new Dictionary<int, LibWorkInstructions.Structs.WorkInstruction>
                {
                    {1, new LibWorkInstructions.Structs.WorkInstruction{Id = 1, OpSpecs = new List<int>{5423, 3217, 1243}}},
                    {2, new LibWorkInstructions.Structs.WorkInstruction{Id = 2, OpSpecs = new List<int>{3245, 7123, 3421}}},
                    {3, new LibWorkInstructions.Structs.WorkInstruction{Id = 3, OpSpecs = new List<int>{3246, 7123, 3422}}},
                }
            };

            n.DataImport(sampleData);
            n.CloneSpecs(2, 3);
            var dbPostClone = n.DataExport();
            Assert.True(dbPostClone.WorkInstructions[2].OpSpecs.SequenceEqual(new List<int> { 3245, 7123, 3421 }));
            Assert.True(dbPostClone.WorkInstructions[3].OpSpecs.SequenceEqual(new List<int> { 3245, 7123, 3421, 3246, 3422 }));
        }

        [Test]
        public void TestCreateQualityClause()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                QualityClauses = new Dictionary<int, LibWorkInstructions.Structs.QualityClause>
                {
                    {1, new LibWorkInstructions.Structs.QualityClause {Id = 1}},
                    {2, new LibWorkInstructions.Structs.QualityClause {Id = 2}},
                }
            };
            n.DataImport(sampleData);
            n.AddQualityClause(new LibWorkInstructions.Structs.QualityClause { Id = 3 });
            var dbPostAdd = n.DataExport();
            Assert.True(dbPostAdd.QualityClauses.ContainsKey(3));
        }

        [Test]
        public void TestMergeQualityClauses()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                JobRefToQualityClauseRefs = new Dictionary<string, List<int>>
                {
                    {"job1", new List<int>{56, 42, 31}},
                    {"job2", new List<int>{65, 42, 13}},
                },
                Jobs = new Dictionary<string, LibWorkInstructions.Structs.Job>
                {
                    {"job1", new LibWorkInstructions.Structs.Job() },
                    {"job2", new LibWorkInstructions.Structs.Job() },
                },
            };
            n.DataImport(sampleData);
            n.MergeQualityClauses("job1", "job2");
            var dbPostMerge = n.DataExport();
            Assert.True(dbPostMerge.JobRefToQualityClauseRefs["job1"].SequenceEqual(new List<int> { 56, 42, 31, 65, 13 }));
            Assert.True(dbPostMerge.JobRefToQualityClauseRefs["job2"].SequenceEqual(new List<int> { 56, 42, 31, 65, 13 }));
        }

        [Test]
        public void TestSplitQualityClauses()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                JobRefToQualityClauseRefs = new Dictionary<string, List<int>>
                {
                    {"job1", new List<int>{56, 42, 31}},
                    {"job2", new List<int>{65, 42, 13}},
                    {"job3", new List<int>()},
                },

                Jobs = new Dictionary<string, LibWorkInstructions.Structs.Job>
                {
                    {"job1", new LibWorkInstructions.Structs.Job()},
                    {"job2", new LibWorkInstructions.Structs.Job()},
                    {"job3", new LibWorkInstructions.Structs.Job()},
                }
            };
            n.DataImport(sampleData);
            n.SplitQualityClauses("job1", "job3");
            var dbPostSplit = n.DataExport();
            Assert.True(dbPostSplit.JobRefToQualityClauseRefs["job1"].SequenceEqual(new List<int> { 56, 42, 31 }));
            Assert.True(dbPostSplit.JobRefToQualityClauseRefs["job3"].SequenceEqual(new List<int> { 56, 42, 31 }));
        }

        [Test]
        public void TestCloneQualityClauses()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                JobRefToQualityClauseRefs = new Dictionary<string, List<int>>
                {
                    {"job1", new List<int>{56, 42, 31}},
                    {"job2", new List<int>{65, 42, 13}},
                },
                Jobs = new Dictionary<string, LibWorkInstructions.Structs.Job>
                {
                    {"job1", new LibWorkInstructions.Structs.Job() },
                    {"job2", new LibWorkInstructions.Structs.Job() },
                }
            };
            n.DataImport(sampleData);
            n.CloneQualityClauses("job1", "job2");
            var dbPostClone = n.DataExport();
            Assert.True(dbPostClone.JobRefToQualityClauseRefs["job1"].SequenceEqual(new List<int> { 56, 42, 31 }));
            Assert.True(dbPostClone.JobRefToQualityClauseRefs["job2"].SequenceEqual(new List<int> { 65, 42, 13, 56, 31}));
        }
    }
}