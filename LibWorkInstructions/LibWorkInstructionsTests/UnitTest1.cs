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
                QualityClauses = new Dictionary<Guid, List<LibWorkInstructions.Structs.QualityClause>> {
          { Guid.NewGuid(), new List<LibWorkInstructions.Structs.QualityClause> { { new LibWorkInstructions.Structs.QualityClause{
            Id = Guid.NewGuid(),
            IdRevGroup = Guid.NewGuid(),
            Clause = "Workmanship...",
              }
              }
          }},
        },
                OpSpecs = new Dictionary<Guid, List<LibWorkInstructions.Structs.OpSpec>> {
          { Guid.NewGuid(), new List<LibWorkInstructions.Structs.OpSpec> { {new LibWorkInstructions.Structs.OpSpec
          {
            Id = Guid.NewGuid(),
            IdRevGroup = Guid.NewGuid(),
          }
              }
            // FIXME: some more fields could get set.
          }},
        },
                WorkInstructions = new Dictionary<Guid, List<LibWorkInstructions.Structs.WorkInstruction>> {
          { Guid.NewGuid(), new List<LibWorkInstructions.Structs.WorkInstruction> { new LibWorkInstructions.Structs.WorkInstruction
          {
            Id = Guid.NewGuid(),
            IdRevGroup = Guid.NewGuid(),
            Approved = true,
            HtmlBlob = "<h1>do something</h1>",
            Images = new List<string>{ },
            OpSpecs = new List<Guid> { Guid.NewGuid() },
          }
          }},
        },
                JobRefToQualityClauseRefs = new Dictionary<string, List<Guid>> {
          { "F110", new List<Guid> { Guid.NewGuid() } },
        },
                JobRefToWorkInstructionRefs = new Dictionary<string, List<List<Guid>>> {
          { "F110", new List<List<Guid>> {
            new List<Guid> { Guid.NewGuid() }
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
            Guid workId = Guid.NewGuid();
            Guid groupId = Guid.NewGuid();
            Guid specId = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                WorkInstructions = new Dictionary<Guid, List<LibWorkInstructions.Structs.WorkInstruction>> {
                { groupId, new List<LibWorkInstructions.Structs.WorkInstruction> { new LibWorkInstructions.Structs.WorkInstruction
                {
                    Id = workId,
                    IdRevGroup = groupId,
                    Approved = true,
                    HtmlBlob = "<h1>do something</h1>",
                    Images = new List<string>{ "image" },
                    OpSpecs = new List<Guid> { specId },
                }
                }},
                },
            };
            LibWorkInstructions.Structs.WorkInstruction testWorkInstruction = new LibWorkInstructions.Structs.WorkInstruction
            {
                Id = workId,
                IdRevGroup = groupId,
                Approved = true,
                HtmlBlob = "<h1>do something</h1>",
                Images = new List<string> { "image" },
                OpSpecs = new List<Guid> { specId },
            };
            n.DataImport(sampleData);
            Assert.True(n.GetWorkInstruction(groupId, workId).Id.Equals(testWorkInstruction.Id));
            Assert.True(n.GetWorkInstruction(groupId, workId).IdRevGroup.Equals(testWorkInstruction.IdRevGroup));
            Assert.True(n.GetWorkInstruction(groupId, workId).Approved.Equals(testWorkInstruction.Approved));
            Assert.True(n.GetWorkInstruction(groupId, workId).HtmlBlob.Equals(testWorkInstruction.HtmlBlob));
            Assert.True(n.GetWorkInstruction(groupId, workId).Images[0].Equals(testWorkInstruction.Images[0]));
            Assert.True(n.GetWorkInstruction(groupId, workId).OpSpecs[0].Equals(testWorkInstruction.OpSpecs[0]));
        }

        [Test]
        public void TestAddWorkInstruction()
        {
            var n = new LibWorkInstructions.BusinessLogic();

            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                Jobs = new Dictionary<string, LibWorkInstructions.Structs.Job>
                {
                    {"job1", new LibWorkInstructions.Structs.Job { Ops = new List<LibWorkInstructions.Structs.Op>()} },
                },
                JobRefToWorkInstructionRefs = new Dictionary<string, List<List<Guid>>>
                {
                    {"job1", new List<List<Guid>>() }
                }
            };
            n.DataImport(sampleData);
            n.AddWorkInstruction("job1");
            var dbVar = n.DataExport();
            Assert.True(dbVar.WorkInstructions.Count == 1);
        }

        [Test]
        public void TestChangeWorkInstruction()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid workId = Guid.NewGuid();
            Guid groupId = Guid.NewGuid();
            Guid specId = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                WorkInstructions = new Dictionary<Guid, List<LibWorkInstructions.Structs.WorkInstruction>> {
                { groupId, new List<LibWorkInstructions.Structs.WorkInstruction> { new LibWorkInstructions.Structs.WorkInstruction
                {
                Id = workId,
                IdRevGroup = groupId,
                Approved = true,
                HtmlBlob = "<h1>do something</h1>",
                Images = new List<string>{ "image" },
                OpSpecs = new List<Guid> { specId },
                }
                }},
                },
            };
            n.DataImport(sampleData);
            n.ChangeWorkInstruction(groupId);
            var dbVar = n.DataExport();
            Assert.True(dbVar.WorkInstructions[groupId].Count == 2);
            Assert.True(dbVar.WorkInstructions[groupId][1].IdRevGroup == groupId);
        }

        [Test]
        public void TestRemoveWorkInstruction()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid workId1 = Guid.NewGuid();
            Guid workId2 = Guid.NewGuid();
            Guid groupId1 = Guid.NewGuid();
            Guid groupId2 = Guid.NewGuid();
            Guid specId1 = Guid.NewGuid();
            Guid specId2 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                WorkInstructions = new Dictionary<Guid, List<LibWorkInstructions.Structs.WorkInstruction>> {
                    { groupId1, new List<LibWorkInstructions.Structs.WorkInstruction> { new LibWorkInstructions.Structs.WorkInstruction {
                        Id = workId1,
                        IdRevGroup = groupId1,
                        Approved = true,
                        HtmlBlob = "<h1>do something</h1>",
                        Images = new List<string> { "image" },
                        OpSpecs = new List<Guid> { specId1 },
                    }
                    } },
                    { groupId2, new  List<LibWorkInstructions.Structs.WorkInstruction> { new LibWorkInstructions.Structs.WorkInstruction {
                        Id = workId2,
                        IdRevGroup = groupId2,
                        Approved = false,
                        HtmlBlob = "<h2>do something</h2>",
                        Images = new List<string> { "jpeg" },
                        OpSpecs = new List<Guid> { Guid.NewGuid() },
                    }
                    } },
                }
            };
            n.DataImport(sampleData);
            Assert.True(n.GetWorkInstruction(groupId1, workId1).Id.Equals(workId1));
            Assert.True(n.GetWorkInstruction(groupId2, workId2).Id.Equals(workId2));
            n.RemoveWorkInstruction(groupId1, workId1);
            var dbVar = n.DataExport();
            Assert.False(dbVar.WorkInstructions.ContainsKey(groupId1));
            Assert.True(dbVar.WorkInstructions.ContainsKey(groupId2));
        }
        [Test]
        public void TestMergeWorkInstructions()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid groupId1 = Guid.NewGuid();
            Guid workId1 = Guid.NewGuid();
            Guid groupId2 = Guid.NewGuid();
            Guid workId2 = Guid.NewGuid();
            Guid groupId3 = Guid.NewGuid();
            Guid workId3 = Guid.NewGuid();
            Guid groupId4 = Guid.NewGuid();
            Guid workId4 = Guid.NewGuid();
            Guid job1Work1Op1 = Guid.NewGuid();
            Guid job1Work1Op2 = Guid.NewGuid();
            Guid job1Work1Op3 = Guid.NewGuid();
            Guid job1Work2Op1 = Guid.NewGuid();
            Guid job1Work2Op2 = Guid.NewGuid();
            Guid job1Work2Op3 = Guid.NewGuid();
            Guid job2Work1Op1 = Guid.NewGuid();
            Guid job2Work1Op2 = Guid.NewGuid();
            Guid job2Work1Op3 = Guid.NewGuid();
            Guid job2Work2Op1 = Guid.NewGuid();
            Guid job2Work2Op2 = Guid.NewGuid();
            Guid job2Work2Op3 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                WorkInstructions = new Dictionary<Guid, List<LibWorkInstructions.Structs.WorkInstruction>>
                {
                    {groupId1, new List<LibWorkInstructions.Structs.WorkInstruction> {
                        new LibWorkInstructions.Structs.WorkInstruction {
                        Id = workId1, IdRevGroup = groupId1, Ops = new List<Guid>{ job1Work1Op1, job1Work1Op2, job1Work1Op3 } } } },
                    {groupId2, new List<LibWorkInstructions.Structs.WorkInstruction> {
                        new LibWorkInstructions.Structs.WorkInstruction {
                            Id = workId2, IdRevGroup = groupId2, Ops = new List<Guid> {job1Work2Op1, job1Work2Op2, job1Work2Op3} } } },
                    {groupId3, new List<LibWorkInstructions.Structs.WorkInstruction> {
                        new LibWorkInstructions.Structs.WorkInstruction {
                            Id = workId3, IdRevGroup = groupId3, Ops = new List<Guid> {job2Work1Op1, job2Work1Op2, job2Work1Op3} } } },
                    {groupId4, new List<LibWorkInstructions.Structs.WorkInstruction> {
                        new LibWorkInstructions.Structs.WorkInstruction {
                            Id = workId4, IdRevGroup = groupId4, Ops = new List<Guid> {job2Work2Op1, job2Work2Op2, job1Work2Op3} } } }
                },

                JobRefToWorkInstructionRefs = new Dictionary<string, List<List<Guid>>> {
                    {"job1" , new List<List<Guid>>()
                    {new List<Guid> {job1Work1Op1, job1Work1Op2, job1Work1Op3}, new List<Guid> {job1Work2Op1, job1Work2Op2, job1Work2Op3} }},
                    {"job2", new List<List<Guid>>()
                    {new List<Guid> {job2Work1Op1, job2Work1Op2, job2Work1Op3}, new List<Guid> {job2Work2Op1, job2Work2Op2, job1Work2Op3} } },
                },
            };
            n.DataImport(sampleData);
            var dbPreMerge = n.DataExport();
            var workInstruction1Ops = dbPreMerge.JobRefToWorkInstructionRefs["job1"][1];
            var workInstruction2Ops = dbPreMerge.JobRefToWorkInstructionRefs["job2"][0];
            n.MergeWorkInstructions(groupId2, workId2, groupId3, workId3);
            var dbPostMerge = n.DataExport();
            Assert.True(dbPostMerge.JobRefToWorkInstructionRefs["job1"].FindIndex(f =>
            f.SequenceEqual(new List<Guid> { job1Work2Op1, job1Work2Op2, job1Work2Op3, job2Work1Op1, job2Work1Op2, job2Work1Op3})) >= 0);
            Assert.True(dbPostMerge.JobRefToWorkInstructionRefs["job2"].FindIndex(f =>
            f.SequenceEqual(new List<Guid> { job1Work2Op1, job1Work2Op2, job1Work2Op3, job2Work1Op1, job2Work1Op2, job2Work1Op3 })) >= 0);
            Assert.False(dbPostMerge.JobRefToWorkInstructionRefs["job1"].FindIndex(f =>
            f.SequenceEqual(workInstruction1Ops)) >= 0);
            Assert.False(dbPostMerge.JobRefToWorkInstructionRefs["job2"].FindIndex(f =>
            f.SequenceEqual(workInstruction2Ops)) >= 0);
        }

        [Test]
        public void TestSplitWorkInstruction()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid groupId1 = Guid.NewGuid();
            Guid workId1 = Guid.NewGuid();
            Guid groupId2 = Guid.NewGuid();
            Guid workId2 = Guid.NewGuid();
            Guid groupId3 = Guid.NewGuid();
            Guid workId3 = Guid.NewGuid();
            Guid groupId4 = Guid.NewGuid();
            Guid workId4 = Guid.NewGuid();
            Guid job1Work1Op1 = Guid.NewGuid();
            Guid job1Work1Op2 = Guid.NewGuid();
            Guid job1Work1Op3 = Guid.NewGuid();
            Guid job1Work2Op1 = Guid.NewGuid();
            Guid job1Work2Op2 = Guid.NewGuid();
            Guid job1Work2Op3 = Guid.NewGuid();
            Guid job2Work1Op1 = Guid.NewGuid();
            Guid job2Work1Op2 = Guid.NewGuid();
            Guid job2Work1Op3 = Guid.NewGuid();
            Guid job2Work2Op1 = Guid.NewGuid();
            Guid job2Work2Op2 = Guid.NewGuid();
            Guid job2Work2Op3 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                WorkInstructions = new Dictionary<Guid, List<LibWorkInstructions.Structs.WorkInstruction>>
                {
                    {groupId1, new List<LibWorkInstructions.Structs.WorkInstruction> {
                        new LibWorkInstructions.Structs.WorkInstruction {
                        Id = workId1, IdRevGroup = groupId1, Ops = new List<Guid>{ job1Work1Op1, job1Work1Op2, job1Work1Op3 } } } },
                    {groupId2, new List<LibWorkInstructions.Structs.WorkInstruction> {
                        new LibWorkInstructions.Structs.WorkInstruction {
                            Id = workId2, IdRevGroup = groupId2, Ops = new List<Guid> {job1Work2Op1, job1Work2Op2, job1Work2Op3} } } },
                    {groupId3, new List<LibWorkInstructions.Structs.WorkInstruction> {
                        new LibWorkInstructions.Structs.WorkInstruction {
                            Id = workId3, IdRevGroup = groupId3, Ops = new List<Guid> {job2Work1Op1, job2Work1Op2, job2Work1Op3} } } },
                    {groupId4, new List<LibWorkInstructions.Structs.WorkInstruction> {
                        new LibWorkInstructions.Structs.WorkInstruction {
                            Id = workId4, IdRevGroup = groupId4, Ops = new List<Guid> {job2Work2Op1, job2Work2Op2, job2Work2Op3} } } }
                },

                JobRefToWorkInstructionRefs = new Dictionary<string, List<List<Guid>>> {
                    {"job1" , new List<List<Guid>>()
                    {new List<Guid> {job1Work1Op1, job1Work1Op2, job1Work1Op3}, new List<Guid> {job1Work2Op1, job1Work2Op2, job1Work2Op3} }},
                    {"job2", new List<List<Guid>>()
                    {new List<Guid> {job2Work1Op1, job2Work1Op2, job2Work1Op3}, new List<Guid> {job2Work2Op1, job2Work2Op2, job1Work2Op3} } },
                },
                Jobs = new Dictionary<string, LibWorkInstructions.Structs.Job>
                {
                    {"job1", new LibWorkInstructions.Structs.Job()},
                    {"job2", new LibWorkInstructions.Structs.Job()},
                }
            };
            n.DataImport(sampleData);
            n.SplitWorkInstruction(groupId2, workId2);
            var dbPostSplit = n.DataExport();

            Assert.True(dbPostSplit.JobRefToWorkInstructionRefs["job1"].Count == 3);
            Assert.True(dbPostSplit.JobRefToWorkInstructionRefs["job1"].Last().SequenceEqual(new List<Guid> { job1Work2Op1, job1Work2Op2, job1Work2Op3 }));
        }

        [Test]
        public void TestCloneWorkInstruction()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid groupId1 = Guid.NewGuid();
            Guid workId1 = Guid.NewGuid();
            Guid groupId2 = Guid.NewGuid();
            Guid workId2 = Guid.NewGuid();
            Guid groupId3 = Guid.NewGuid();
            Guid workId3 = Guid.NewGuid();
            Guid groupId4 = Guid.NewGuid();
            Guid workId4 = Guid.NewGuid();
            Guid job1Work1Op1 = Guid.NewGuid();
            Guid job1Work1Op2 = Guid.NewGuid();
            Guid job1Work1Op3 = Guid.NewGuid();
            Guid job1Work2Op1 = Guid.NewGuid();
            Guid job1Work2Op2 = Guid.NewGuid();
            Guid job1Work2Op3 = Guid.NewGuid();
            Guid job2Work1Op1 = Guid.NewGuid();
            Guid job2Work1Op2 = Guid.NewGuid();
            Guid job2Work1Op3 = Guid.NewGuid();
            Guid job2Work2Op1 = Guid.NewGuid();
            Guid job2Work2Op2 = Guid.NewGuid();
            Guid job2Work2Op3 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                WorkInstructions = new Dictionary<Guid, List<LibWorkInstructions.Structs.WorkInstruction>>
                {
                    {groupId1, new List<LibWorkInstructions.Structs.WorkInstruction> {
                        new LibWorkInstructions.Structs.WorkInstruction {
                        Id = workId1, IdRevGroup = groupId1, Ops = new List<Guid>{ job1Work1Op1, job1Work1Op2, job1Work1Op3 } } } },
                    {groupId2, new List<LibWorkInstructions.Structs.WorkInstruction> {
                        new LibWorkInstructions.Structs.WorkInstruction {
                            Id = workId2, IdRevGroup = groupId2, Ops = new List<Guid> {job1Work2Op1, job1Work2Op2, job1Work2Op3} } } },
                    {groupId3, new List<LibWorkInstructions.Structs.WorkInstruction> {
                        new LibWorkInstructions.Structs.WorkInstruction {
                            Id = workId3, IdRevGroup = groupId3, Ops = new List<Guid> {job2Work1Op1, job2Work1Op2, job2Work1Op3} } } },
                    {groupId4, new List<LibWorkInstructions.Structs.WorkInstruction> {
                        new LibWorkInstructions.Structs.WorkInstruction {
                            Id = workId4, IdRevGroup = groupId4, Ops = new List<Guid> {job2Work2Op1, job2Work2Op2, job2Work2Op3} } } }
                },

                JobRefToWorkInstructionRefs = new Dictionary<string, List<List<Guid>>> {
                    {"job1" , new List<List<Guid>>()
                    {new List<Guid> {job1Work1Op1, job1Work1Op2, job1Work1Op3}, new List<Guid> {job1Work2Op1, job1Work2Op2, job1Work2Op3} }},
                    {"job2", new List<List<Guid>>()
                    {new List<Guid> {job2Work1Op1, job2Work1Op2, job2Work1Op3}, new List<Guid> {job2Work2Op1, job2Work2Op2, job1Work2Op3} } },
                },
                Jobs = new Dictionary<string, LibWorkInstructions.Structs.Job>
                {
                    {"job1", new LibWorkInstructions.Structs.Job() },
                    {"job2", new LibWorkInstructions.Structs.Job() },
                },
            };

            n.DataImport(sampleData);
            n.CloneWorkInstruction(groupId1, workId1, "job2");
            var dbPostClone = n.DataExport();

            Assert.True(dbPostClone.JobRefToWorkInstructionRefs["job2"].Count == 3);
            Assert.True(dbPostClone.JobRefToWorkInstructionRefs["job2"].Last().SequenceEqual(new List<Guid> { job1Work1Op1, job1Work1Op2, job1Work1Op3 }));
        }

        [Test]
        public void TestAddSpec()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid groupId = Guid.NewGuid();
            Guid workId = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                OpSpecs = new Dictionary<Guid, List<LibWorkInstructions.Structs.OpSpec>>(),
                WorkInstructions = new Dictionary<Guid, List<LibWorkInstructions.Structs.WorkInstruction>>
                {
                    {groupId, new List<LibWorkInstructions.Structs.WorkInstruction> {
                        new LibWorkInstructions.Structs.WorkInstruction {
                        Id = workId, IdRevGroup = groupId, OpSpecs = new List<Guid>() },
                }
                }
                }
            };

            n.DataImport(sampleData);
            n.AddSpec(groupId, workId);
            var dbPostAdd = n.DataExport();

            Assert.True(dbPostAdd.OpSpecs.Count == 1);
        }

        [Test]
        public void TestChangeSpec()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid specId1 = Guid.NewGuid();
            Guid specId2 = Guid.NewGuid();
            Guid specId3 = Guid.NewGuid();
            Guid groupWorkId1 = Guid.NewGuid();
            Guid groupWorkId2 = Guid.NewGuid();
            Guid groupWorkId3 = Guid.NewGuid();
            Guid groupSpecId1 = Guid.NewGuid();
            Guid groupSpecId2 = Guid.NewGuid();
            Guid groupSpecId3 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                OpSpecs = new Dictionary<Guid, List<LibWorkInstructions.Structs.OpSpec>>
                {
                    { groupSpecId1, new List<LibWorkInstructions.Structs.OpSpec> {
                        new LibWorkInstructions.Structs.OpSpec {Id = specId1, Name = "spec1"} } },
                    { groupSpecId2, new List<LibWorkInstructions.Structs.OpSpec> {
                        new LibWorkInstructions.Structs.OpSpec {Id = specId2, Name = "spec2"} } },
                    { groupSpecId3, new List<LibWorkInstructions.Structs.OpSpec> {
                        new LibWorkInstructions.Structs.OpSpec {Id = specId3, Name = "spec3"} } },
                },
                WorkInstructions = new Dictionary<Guid, List<LibWorkInstructions.Structs.WorkInstruction>>
                {
                    { groupWorkId1, new List<LibWorkInstructions.Structs.WorkInstruction> {
                         new LibWorkInstructions.Structs.WorkInstruction {OpSpecs = new List<Guid> { specId1, specId2 }, Approved = true} }},
                    { groupWorkId2, new List<LibWorkInstructions.Structs.WorkInstruction> {
                         new LibWorkInstructions.Structs.WorkInstruction {OpSpecs = new List<Guid> { specId2, specId3 }, Approved = true} }},
                    { groupWorkId3, new List<LibWorkInstructions.Structs.WorkInstruction> {
                         new LibWorkInstructions.Structs.WorkInstruction {OpSpecs = new List<Guid> { specId1, specId3 }, Approved = true} }},
                }
            };
            n.DataImport(sampleData);
            n.ChangeSpec(groupSpecId1);
            var dbPostChange = n.DataExport();

            Assert.True(dbPostChange.OpSpecs.Count == 3);
            Assert.True(dbPostChange.OpSpecs[groupSpecId1].Count == 2);
        }

        [Test]
        public void TestDeleteSpec()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid groupId1 = Guid.NewGuid();
            Guid groupId2 = Guid.NewGuid();
            Guid groupId3 = Guid.NewGuid();
            Guid specId1 = Guid.NewGuid();
            Guid specId2 = Guid.NewGuid();
            Guid specId3 = Guid.NewGuid();
            Guid specId4 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                OpSpecs = new Dictionary<Guid, List<LibWorkInstructions.Structs.OpSpec>>
                {
                    { groupId1, new List<LibWorkInstructions.Structs.OpSpec> {
                        new LibWorkInstructions.Structs.OpSpec{Id = specId1, Name = "spec1"} } },
                    { groupId2, new List<LibWorkInstructions.Structs.OpSpec> {
                        new LibWorkInstructions.Structs.OpSpec{Id = specId2, Name = "spec2"} } },
                    { groupId3, new List<LibWorkInstructions.Structs.OpSpec> {
                        new LibWorkInstructions.Structs.OpSpec{Id = specId3, Name = "spec3"},
                        new LibWorkInstructions.Structs.OpSpec{Id = specId4, Name = "spec4"} } },
                }
            };

            n.DataImport(sampleData);
            n.DeleteSpec(groupId3, specId4);
            var dbPostDelete = n.DataExport();
            Assert.True(dbPostDelete.OpSpecs[groupId3].Count == 1);
            Assert.False(dbPostDelete.OpSpecs[groupId3].Contains(dbPostDelete.OpSpecs[groupId3].First(y => y.Id == specId4)));
        }

        [Test]
        public void TestMergeSpecs()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid groupId1 = Guid.NewGuid();
            Guid groupId2 = Guid.NewGuid();
            Guid groupId3 = Guid.NewGuid();
            Guid workId1 = Guid.NewGuid();
            Guid workId2 = Guid.NewGuid();
            Guid workId3 = Guid.NewGuid();
            Guid specId1 = Guid.NewGuid();
            Guid specId2 = Guid.NewGuid();
            Guid specId3 = Guid.NewGuid();
            Guid specId4 = Guid.NewGuid();
            Guid specId5 = Guid.NewGuid();
            Guid specId6 = Guid.NewGuid();
            Guid specId7 = Guid.NewGuid();
            Guid specId8 = Guid.NewGuid();
            Guid specId9 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                WorkInstructions = new Dictionary<Guid, List<LibWorkInstructions.Structs.WorkInstruction>>
                {
                    {groupId1, new List<LibWorkInstructions.Structs.WorkInstruction> {
                        new LibWorkInstructions.Structs.WorkInstruction{Id = workId1, OpSpecs = new List<Guid>{specId1, specId2, specId3}}} },
                    {groupId2, new List<LibWorkInstructions.Structs.WorkInstruction>{
                        new LibWorkInstructions.Structs.WorkInstruction {Id = workId2, OpSpecs = new List<Guid>{specId4, specId5, specId6}}} },
                    {groupId3, new List <LibWorkInstructions.Structs.WorkInstruction> { 
                        new LibWorkInstructions.Structs.WorkInstruction { Id = workId3, OpSpecs = new List < Guid > { specId7, specId8, specId9 } } }},
                }
            };

            n.DataImport(sampleData);
            n.MergeSpecs(groupId1, workId1, groupId2, workId2);
            var dbPostMerge = n.DataExport();
            Assert.True(dbPostMerge.WorkInstructions[groupId1][0].OpSpecs.SequenceEqual(new List<Guid> { specId1, specId2, specId3, specId4, specId5, specId6 }));
            Assert.True(dbPostMerge.WorkInstructions[groupId2][0].OpSpecs.SequenceEqual(new List<Guid> { specId1, specId2, specId3, specId4, specId5, specId6 }));
        }

        [Test]
        public void TestSplitSpec()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid groupId1 = Guid.NewGuid();
            Guid groupId2 = Guid.NewGuid();
            Guid groupId3 = Guid.NewGuid();
            Guid workId1 = Guid.NewGuid();
            Guid workId2 = Guid.NewGuid();
            Guid workId3 = Guid.NewGuid();
            Guid specId1 = Guid.NewGuid();
            Guid specId2 = Guid.NewGuid();
            Guid specId3 = Guid.NewGuid();
            Guid specId4 = Guid.NewGuid();
            Guid specId5 = Guid.NewGuid();
            Guid specId6 = Guid.NewGuid();
            Guid specId7 = Guid.NewGuid();
            Guid specId8 = Guid.NewGuid();
            Guid specId9 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                WorkInstructions = new Dictionary<Guid, List<LibWorkInstructions.Structs.WorkInstruction>>
                {
                    {groupId1, new List<LibWorkInstructions.Structs.WorkInstruction> {
                        new LibWorkInstructions.Structs.WorkInstruction{Id = workId1, OpSpecs = new List<Guid>{specId1, specId2, specId3}}} },
                    {groupId2, new List<LibWorkInstructions.Structs.WorkInstruction>{
                        new LibWorkInstructions.Structs.WorkInstruction {Id = workId2, OpSpecs = new List<Guid>{specId4, specId5, specId6}}} },
                    {groupId3, new List <LibWorkInstructions.Structs.WorkInstruction> {
                        new LibWorkInstructions.Structs.WorkInstruction { Id = workId3, OpSpecs = new List < Guid > { specId7, specId8, specId9 } } }},
                }
            };

            n.DataImport(sampleData);
            n.SplitSpec(groupId1, workId1);
            var dbPostSplit = n.DataExport();
            Assert.True(dbPostSplit.WorkInstructions[groupId1][0].OpSpecs.Count == 2);
            Assert.True(dbPostSplit.WorkInstructions[groupId1].Last().OpSpecs.SequenceEqual(new List<Guid> { specId1, specId2, specId3 }));
        }

        [Test]
        public void TestCloneSpecs()
        {
            var n1 = new LibWorkInstructions.BusinessLogic();
            var n2 = new LibWorkInstructions.BusinessLogic();
            Guid groupId1 = Guid.NewGuid();
            Guid groupId2 = Guid.NewGuid();
            Guid groupId3 = Guid.NewGuid();
            Guid workId1 = Guid.NewGuid();
            Guid workId2 = Guid.NewGuid();
            Guid workId3 = Guid.NewGuid();
            Guid specId1 = Guid.NewGuid();
            Guid specId2 = Guid.NewGuid();
            Guid specId3 = Guid.NewGuid();
            Guid specId4 = Guid.NewGuid();
            Guid specId5 = Guid.NewGuid();
            Guid specId6 = Guid.NewGuid();
            Guid specId7 = Guid.NewGuid();
            Guid specId8 = Guid.NewGuid();
            Guid specId9 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                WorkInstructions = new Dictionary<Guid, List<LibWorkInstructions.Structs.WorkInstruction>>
                {
                    {groupId1, new List<LibWorkInstructions.Structs.WorkInstruction> {
                        new LibWorkInstructions.Structs.WorkInstruction{Id = workId1, OpSpecs = new List<Guid>{specId1, specId2, specId3}}} },
                    {groupId2, new List<LibWorkInstructions.Structs.WorkInstruction>{
                        new LibWorkInstructions.Structs.WorkInstruction {Id = workId2, OpSpecs = new List<Guid>{specId4, specId5, specId6}}} },
                    {groupId3, new List <LibWorkInstructions.Structs.WorkInstruction> {
                        new LibWorkInstructions.Structs.WorkInstruction { Id = workId3, OpSpecs = new List < Guid > { specId7, specId8, specId9 } } }},
                }
            };

            n1.DataImport(sampleData);
            n1.CloneSpecs(groupId1, workId1, groupId3, groupId3, true);
            var dbPostClone1 = n1.DataExport();
            Assert.True(dbPostClone1.WorkInstructions[groupId3][0].OpSpecs.Count == 3);
            Assert.True(dbPostClone1.WorkInstructions[groupId3][0].OpSpecs.SequenceEqual(new List<Guid> { specId1, specId2, specId3 }));
            n2.DataImport(sampleData);
            n2.CloneSpecs(groupId1, workId1, groupId3, groupId3, false);
            var dbPostClone2 = n2.DataExport();
            Assert.True(dbPostClone2.WorkInstructions[groupId3][0].OpSpecs.Count == 6);
            Assert.True(dbPostClone2.WorkInstructions[groupId3][0].OpSpecs.SequenceEqual(new List<Guid> { specId1, specId2, specId3, specId7, specId8, specId9 }));
        }

        [Test]
        public void TestAddQualityClause()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid groupId1 = Guid.NewGuid();
            Guid groupId2 = Guid.NewGuid();
            Guid clauseId1 = Guid.NewGuid();
            Guid clauseId2 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                QualityClauses = new Dictionary<Guid, List<LibWorkInstructions.Structs.QualityClause>>
                {
                    {groupId1, new List<LibWorkInstructions.Structs.QualityClause> {
                        new LibWorkInstructions.Structs.QualityClause {Id = clauseId1}} },
                    {groupId2, new List<LibWorkInstructions.Structs.QualityClause> {
                        new LibWorkInstructions.Structs.QualityClause {Id = clauseId2}} },
                }
            };
            n.DataImport(sampleData);
            n.AddQualityClause();
            var dbPostAdd = n.DataExport();
            Assert.True(dbPostAdd.QualityClauses.Count == 3);
        }

        [Test]
        public void TestMergeQualityClauses()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid clause1 = Guid.NewGuid();
            Guid clause2 = Guid.NewGuid();
            Guid clause3 = Guid.NewGuid();
            Guid clause4 = Guid.NewGuid();
            Guid clause5 = Guid.NewGuid();
            Guid clause6 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                JobRefToQualityClauseRefs = new Dictionary<string, List<Guid>>
                {
                    {"job1", new List<Guid>{clause1, clause2, clause3}},
                    {"job2", new List<Guid>{clause4, clause5, clause6}},
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
            Assert.True(dbPostMerge.JobRefToQualityClauseRefs["job1"].SequenceEqual(new List<Guid> { clause1, clause2, clause3, clause4, clause5, clause6 }));
            Assert.True(dbPostMerge.JobRefToQualityClauseRefs["job2"].SequenceEqual(new List<Guid> { clause1, clause2, clause3, clause4, clause5, clause6 }));
        }

        [Test]
        public void TestSplitQualityClauses()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid groupId1 = Guid.NewGuid();
            Guid groupId2 = Guid.NewGuid();
            Guid clauseId1 = Guid.NewGuid();
            Guid clauseId2 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                QualityClauses = new Dictionary<Guid, List<LibWorkInstructions.Structs.QualityClause>>
                {
                    {groupId1, new List<LibWorkInstructions.Structs.QualityClause> {
                        new LibWorkInstructions.Structs.QualityClause {Id = clauseId1}} },
                    {groupId2, new List<LibWorkInstructions.Structs.QualityClause> {
                        new LibWorkInstructions.Structs.QualityClause {Id = clauseId2}} },
                }
            };
            n.DataImport(sampleData);
            n.SplitQualityClause(groupId1, clauseId1);
            var dbPostSplit = n.DataExport();
            Assert.True(dbPostSplit.QualityClauses[groupId1].Count == 2);
            Assert.True(dbPostSplit.QualityClauses[groupId1].Last().Id == clauseId1);
        }

        [Test]
        public void TestCloneQualityClauses()
        {
            var n1 = new LibWorkInstructions.BusinessLogic();
            var n2 = new LibWorkInstructions.BusinessLogic();
            Guid clause1 = Guid.NewGuid();
            Guid clause2 = Guid.NewGuid();
            Guid clause3 = Guid.NewGuid();
            Guid clause4 = Guid.NewGuid();
            Guid clause5 = Guid.NewGuid();
            Guid clause6 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                JobRefToQualityClauseRefs = new Dictionary<string, List<Guid>>
                {
                    {"job1", new List<Guid>{clause1, clause2, clause3}},
                    {"job2", new List<Guid>{clause4, clause5, clause6}},
                },
                Jobs = new Dictionary<string, LibWorkInstructions.Structs.Job>
                {
                    {"job1", new LibWorkInstructions.Structs.Job() },
                    {"job2", new LibWorkInstructions.Structs.Job() },
                }
            };
            n1.DataImport(sampleData);
            n1.CloneQualityClauses("job1", "job2", true);
            var dbPostClone1 = n1.DataExport();
            Assert.True(dbPostClone1.JobRefToQualityClauseRefs["job2"].Count == 3);
            Assert.True(dbPostClone1.JobRefToQualityClauseRefs["job2"].SequenceEqual(new List<Guid> { clause1, clause2, clause3}));
            n2.DataImport(sampleData);
            n2.CloneQualityClauses("job1", "job2", false);
            var dbPostClone2 = n2.DataExport();
            Assert.True(dbPostClone2.JobRefToQualityClauseRefs["job2"].Count == 6);
            Assert.True(dbPostClone2.JobRefToQualityClauseRefs["job2"].SequenceEqual(new List<Guid> { clause1, clause2, clause3, clause4, clause5, clause6 }));
        }
    }
}