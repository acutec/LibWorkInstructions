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
                Jobs = new Dictionary<string, List<LibWorkInstructions.Structs.Job>> {
            { "F110", new List<LibWorkInstructions.Structs.Job> { new LibWorkInstructions.Structs.Job {
            Id = "F110",
            Rev = "A",
            RevCustomer = "CUSTX",
            RevPlan = "1.0.0",
          }} },
          { "E444", new List<LibWorkInstructions.Structs.Job> { new LibWorkInstructions.Structs.Job {
            Id = "E444",
            Rev = "C",
            RevCustomer = "CUSTY",
            RevPlan = "7.1.12",
          }} },
        },
                QualityClauses = new Dictionary<Guid, List<LibWorkInstructions.Structs.QualityClause>> {
          { Guid.NewGuid(), new List<LibWorkInstructions.Structs.QualityClause> { { new LibWorkInstructions.Structs.QualityClause{
            Id = Guid.NewGuid(),
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
          }
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
        public void TestCreateJob()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            n.CreateJob("F110", "CUSTX", "1.0.0", "A");
            var dbVar = n.DataExport();
            Assert.True(dbVar.Jobs["F110"][0].Id.Equals("F110"));
            Assert.True(dbVar.Jobs["F110"][0].Rev.Equals("A"));
            Assert.True(dbVar.Jobs["F110"][0].RevCustomer.Equals("CUSTX"));
            Assert.True(dbVar.Jobs["F110"][0].RevPlan.Equals("1.0.0"));
            Assert.True(dbVar.Jobs["F110"][0].RevSeq == 0);
            Assert.True(dbVar.JobRefToJobRevRefs.ContainsKey("F110"));
            Assert.True(dbVar.JobRefToJobRevRefs["F110"].Contains("A"));
        }

        [Test]
        public void TestCreateWorkInstruction()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            n.CreateWorkInstruction(1);
            var dbVar = n.DataExport();
            Assert.True(dbVar.WorkInstructions.Count == 1);
            Assert.True(dbVar.WorkInstructions.First().Value[0].Id != null);
            Assert.True(dbVar.WorkInstructions.First().Value[0].IdRevGroup != null);
            Assert.True(dbVar.WorkInstructions.First().Value[0].OpId == 1);
            Assert.True(dbVar.WorkInstructions.First().Value[0].RevSeq == 0);
            Assert.True(dbVar.WorkInstructionRefToWorkInstructionRevRefs.Count != 0);
            Assert.True(dbVar.OpRefToWorkInstructionRef[1] != null);
        }

        [Test]
        public void TestUpdateWorkInstruction()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid workId = Guid.NewGuid();
            Guid groupId = Guid.NewGuid();
            Guid specId = Guid.NewGuid();
            var workInstructionTest = new LibWorkInstructions.Structs.WorkInstruction { Id = workId, IdRevGroup = groupId, Images = new List<string> { "image" }, Approved = false, HtmlBlob = "html", OpId = 1 };
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
                }
                }},
                },
            };
            n.DataImport(sampleData);
            n.UpdateWorkInstruction(workInstructionTest);
            var dbVar = n.DataExport();
            Assert.True(dbVar.WorkInstructions[groupId].Count == 1);
            Assert.True(dbVar.WorkInstructions[groupId][1].IdRevGroup == groupId);
            Assert.True(dbVar.WorkInstructions[groupId][1].OpId == workInstructionTest.OpId);
            Assert.True(dbVar.WorkInstructionRefToWorkInstructionRevRefs.ContainsKey(workInstructionTest.Id));
            Assert.True(dbVar.OpRefToOpSpecRevRefs.ContainsKey(workInstructionTest.OpId));
        }

        [Test]
        public void TestDeleteWorkInstruction()
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
                    }
                    } },
                    { groupId2, new  List<LibWorkInstructions.Structs.WorkInstruction> { new LibWorkInstructions.Structs.WorkInstruction {
                        Id = workId2,
                        IdRevGroup = groupId2,
                        Approved = false,
                        HtmlBlob = "<h2>do something</h2>",
                        Images = new List<string> { "jpeg" },
                    }
                    } },
                }
            };
            n.DataImport(sampleData);
            // n.DeleteWorkInstruction(groupId1, workId1);
            var dbVar = n.DataExport();
            Assert.True(dbVar.WorkInstructions[groupId1].Count == 0);
            Assert.True(dbVar.WorkInstructions[groupId2].Count == 1);
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
                Jobs = new Dictionary<string, List<LibWorkInstructions.Structs.Job>>
                {
                    {"job1", new List<LibWorkInstructions.Structs.Job>()},
                    {"job2", new List<LibWorkInstructions.Structs.Job>()},
                },

                //JobRefToWorkInstructionRefs = new Dictionary<string, List<List<Guid>>> {
                //    {"job1" , new List<List<Guid>>()
                //    {new List<Guid> {job1Work1Op1, job1Work1Op2, job1Work1Op3}, new List<Guid> {job1Work2Op1, job1Work2Op2, job1Work2Op3} }},
                //    {"job2", new List<List<Guid>>()
                //    {new List<Guid> {job2Work1Op1, job2Work1Op2, job2Work1Op3}, new List<Guid> {job2Work2Op1, job2Work2Op2, job1Work2Op3} } },
                //},
            };
            n.DataImport(sampleData);
            var dbPreMerge = n.DataExport();
        //    var workInstruction1Ops = dbPreMerge.JobRefToWorkInstructionRefs["job1"][1];
        //    var workInstruction2Ops = dbPreMerge.JobRefToWorkInstructionRefs["job2"][0];
        //    n.MergeWorkInstructions("job1", "job2");
        //    var dbPostMerge = n.DataExport();
        //    Assert.True(dbPostMerge.JobRefToWorkInstructionRefs["job1"].FindIndex(f =>
        //    f.SequenceEqual(new List<Guid> { job1Work2Op1, job1Work2Op2, job1Work2Op3, job2Work1Op1, job2Work1Op2, job2Work1Op3})) >= 0);
        //    Assert.True(dbPostMerge.JobRefToWorkInstructionRefs["job2"].FindIndex(f =>
        //    f.SequenceEqual(new List<Guid> { job1Work2Op1, job1Work2Op2, job1Work2Op3, job2Work1Op1, job2Work1Op2, job2Work1Op3 })) >= 0);
        //    Assert.False(dbPostMerge.JobRefToWorkInstructionRefs["job1"].FindIndex(f =>
        //    f.SequenceEqual(workInstruction1Ops)) >= 0);
        //    Assert.False(dbPostMerge.JobRefToWorkInstructionRefs["job2"].FindIndex(f =>
        //    f.SequenceEqual(workInstruction2Ops)) >= 0);
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
                        Id = workId1, IdRevGroup = groupId1, } } },
                    {groupId2, new List<LibWorkInstructions.Structs.WorkInstruction> {
                        new LibWorkInstructions.Structs.WorkInstruction {
                            Id = workId2, IdRevGroup = groupId2, } } },
                    {groupId3, new List<LibWorkInstructions.Structs.WorkInstruction> {
                        new LibWorkInstructions.Structs.WorkInstruction {
                            Id = workId3, IdRevGroup = groupId3, } } },
                    {groupId4, new List<LibWorkInstructions.Structs.WorkInstruction> {
                        new LibWorkInstructions.Structs.WorkInstruction {
                            Id = workId4, IdRevGroup = groupId4, } } }
                },

                //JobRefToWorkInstructionRefs = new Dictionary<string, List<List<Guid>>> {
                //    {"job1" , new List<List<Guid>>()
                //    {new List<Guid> {job1Work1Op1, job1Work1Op2, job1Work1Op3}, new List<Guid> {job1Work2Op1, job1Work2Op2, job1Work2Op3} }},
                //    {"job2", new List<List<Guid>>()
                //    {new List<Guid> {job2Work1Op1, job2Work1Op2, job2Work1Op3}, new List<Guid> {job2Work2Op1, job2Work2Op2, job1Work2Op3} } },
                //},
                Jobs = new Dictionary<string, List<LibWorkInstructions.Structs.Job>>
                {
                    {"job1", new List<LibWorkInstructions.Structs.Job>()},
                    {"job2", new List<LibWorkInstructions.Structs.Job>()},
                }
            };
            n.DataImport(sampleData);
            n.SplitWorkInstructionRev(groupId2, workId2);
            var dbPostSplit = n.DataExport();

            //Assert.True(dbPostSplit.JobRefToWorkInstructionRefs["job1"].Count == 3);
            //Assert.True(dbPostSplit.JobRefToWorkInstructionRefs["job1"].Last().SequenceEqual(new List<Guid> { job1Work2Op1, job1Work2Op2, job1Work2Op3 }));
        }

        [Test]
        public void TestCloneWorkInstructionRevsNotAdditive()
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
                        Id = workId1, IdRevGroup = groupId1, } } },
                    {groupId2, new List<LibWorkInstructions.Structs.WorkInstruction> {
                        new LibWorkInstructions.Structs.WorkInstruction {
                            Id = workId2, IdRevGroup = groupId2, } } },
                    {groupId3, new List<LibWorkInstructions.Structs.WorkInstruction> {
                        new LibWorkInstructions.Structs.WorkInstruction {
                            Id = workId3, IdRevGroup = groupId3, } } },
                    {groupId4, new List<LibWorkInstructions.Structs.WorkInstruction> {
                        new LibWorkInstructions.Structs.WorkInstruction {
                            Id = workId4, IdRevGroup = groupId4, } } }
                },

            };

            n.DataImport(sampleData);
            n.CloneWorkInstructionRevs(workId1, workId2, false);
            var dbPostClone = n.DataExport();

            Assert.True(dbPostClone.WorkInstructions.ContainsKey(workId1));
            Assert.True(dbPostClone.WorkInstructions.ContainsKey(workId2));
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
            };
            var testAddSpec = new LibWorkInstructions.Structs.OpSpec
            {
                Id = workId,
                IdRevGroup = groupId,
                Class = "test",
                Comment = "test",
                Grade = "test",
                Level = "test",
                Method = "test",
                Name = "test",
                Notice = "test",
                Proctype = "test",
                Servicecond = "test",
                Status = "test",
                Type = "test",
            };

            n.DataImport(sampleData);
            n.CreateOpSpec(testAddSpec);
            var dbPostAdd = n.DataExport();

            Assert.True(dbPostAdd.OpSpecs.Count == 1);
            Assert.True(dbPostAdd.OpSpecs.ContainsKey(testAddSpec.Id));
        }

        //[Test]
        //public void TestChangeSpec()
        //{
        //    var n = new LibWorkInstructions.BusinessLogic();
        //    Guid specId1 = Guid.NewGuid();
        //    Guid specId2 = Guid.NewGuid();
        //    Guid specId3 = Guid.NewGuid();
        //    Guid groupWorkId1 = Guid.NewGuid();
        //    Guid groupWorkId2 = Guid.NewGuid();
        //    Guid groupWorkId3 = Guid.NewGuid();
        //    Guid groupSpecId1 = Guid.NewGuid();
        //    Guid groupSpecId2 = Guid.NewGuid();
        //    Guid groupSpecId3 = Guid.NewGuid();
        //    var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
        //    {
        //        OpSpecs = new Dictionary<Guid, List<LibWorkInstructions.Structs.OpSpec>>
        //        {
        //            { groupSpecId1, new List<LibWorkInstructions.Structs.OpSpec> {
        //                new LibWorkInstructions.Structs.OpSpec {Id = specId1, Name = "spec1"} } },
        //            { groupSpecId2, new List<LibWorkInstructions.Structs.OpSpec> {
        //                new LibWorkInstructions.Structs.OpSpec {Id = specId2, Name = "spec2"} } },
        //            { groupSpecId3, new List<LibWorkInstructions.Structs.OpSpec> {
        //                new LibWorkInstructions.Structs.OpSpec {Id = specId3, Name = "spec3"} } },
        //        },
        //        WorkInstructions = new Dictionary<Guid, List<LibWorkInstructions.Structs.WorkInstruction>>
        //        {
        //            { groupWorkId1, new List<LibWorkInstructions.Structs.WorkInstruction> {
        //                 new LibWorkInstructions.Structs.WorkInstruction {OpSpecs = new List<Guid> { specId1, specId2 }, Approved = true} }},
        //            { groupWorkId2, new List<LibWorkInstructions.Structs.WorkInstruction> {
        //                 new LibWorkInstructions.Structs.WorkInstruction {OpSpecs = new List<Guid> { specId2, specId3 }, Approved = true} }},
        //            { groupWorkId3, new List<LibWorkInstructions.Structs.WorkInstruction> {
        //                 new LibWorkInstructions.Structs.WorkInstruction {OpSpecs = new List<Guid> { specId1, specId3 }, Approved = true} }},
        //        }
        //    };
        //    n.DataImport(sampleData);
        //    n.ChangeSpec(groupSpecId1);
        //    var dbPostChange = n.DataExport();

        //    Assert.True(dbPostChange.OpSpecs.Count == 3);
        //    Assert.True(dbPostChange.OpSpecs[groupSpecId1].Count == 2);
        //}

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
            //n.DeleteOpSpec(groupId3, specId4);
            var dbPostDelete = n.DataExport();
            Assert.True(dbPostDelete.OpSpecs[groupId3].Count == 1);
            Assert.True(dbPostDelete.OpSpecs[groupId3].FindAll(y => y.Id == specId4).Count == 0);
        }

        [Test]
        public void TestMergeSpecs()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid groupId1 = Guid.NewGuid();
            Guid groupId2 = Guid.NewGuid();
            Guid workId1 = Guid.NewGuid();
            Guid workId2 = Guid.NewGuid();
            Guid workId3 = Guid.NewGuid();
            Guid workId4 = Guid.NewGuid();
            var opSpec1 = new LibWorkInstructions.Structs.OpSpec
            {
                Id = workId1,
                IdRevGroup = groupId1,
                Class = "test",
                Comment = "test",
                Grade = "test",
                Level = "test",
                Method = "test",
                Name = "test",
                Notice = "test",
                Proctype = "test",
                Servicecond = "test",
                Status = "test",
                Type = "test",
            };
            var opSpec2 = new LibWorkInstructions.Structs.OpSpec
            {
                Id = workId2,
                IdRevGroup = groupId1,
                Class = "test",
                Comment = "test",
                Grade = "test",
                Level = "test",
                Method = "test",
                Name = "test",
                Notice = "test",
                Proctype = "test",
                Servicecond = "test",
                Status = "test",
                Type = "test",
            };
            var opSpec3 = new LibWorkInstructions.Structs.OpSpec
            {
                Id = workId3,
                IdRevGroup = groupId2,
                Class = "test",
                Comment = "test",
                Grade = "test",
                Level = "test",
                Method = "test",
                Name = "test",
                Notice = "test",
                Proctype = "test",
                Servicecond = "test",
                Status = "test",
                Type = "test",
            };
            var opSpec4 = new LibWorkInstructions.Structs.OpSpec
            {
                Id = workId4,
                IdRevGroup = groupId2,
                Class = "test",
                Comment = "test",
                Grade = "test",
                Level = "test",
                Method = "test",
                Name = "test",
                Notice = "test",
                Proctype = "test",
                Servicecond = "test",
                Status = "test",
                Type = "test",
            };
            var opSpecList1 = new List<LibWorkInstructions.Structs.OpSpec> { opSpec1, opSpec2};
            var opSpecList2 = new List<LibWorkInstructions.Structs.OpSpec> { opSpec3, opSpec4};
            var op1 = new LibWorkInstructions.Structs.Op
            {
                Id = 0,
                JobId = "job1",
                OpService = "Op 20",
                Seq = 2,
            };
            var op2 = new LibWorkInstructions.Structs.Op
            {
                Id = 1,
                JobId = "job2",
                OpService = "Op 30",
                Seq = 3,
            };
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                Jobs = new Dictionary<string, List<LibWorkInstructions.Structs.Job>>
                {
                    {"job1", new List<LibWorkInstructions.Structs.Job>
                    { new LibWorkInstructions.Structs.Job {
                        Id = "F110",
                        Rev = "A",
                        RevCustomer = "CUSTX",
                        RevPlan = "1.0.0",
                        Ops = new List<LibWorkInstructions.Structs.Op> {op1},
                    } }
                    },

                    {"job2", new List<LibWorkInstructions.Structs.Job>
                    { new LibWorkInstructions.Structs.Job {
                        Id = "F111",
                        Rev = "A",
                        RevCustomer = "CUSTH",
                        RevPlan = "1.0.1",
                        Ops = new List<LibWorkInstructions.Structs.Op> {op2},
                    } } }
                }
            };

            n.DataImport(sampleData);
            n.MergeOpSpecRevsBasedOnJobOp(op1.Id, op2.Id);
            var dbPostMerge = n.DataExport();
            //Assert.True(dbPostMerge.Jobs["job1"].Ops[0].OpSpecs.SequenceEqual(dbPostMerge.Jobs["job2"][0].Ops[0].OpSpecs));
        }

        [Test]
        public void TestSplitSpec()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid groupId1 = Guid.NewGuid();
            Guid groupId2 = Guid.NewGuid();
            Guid groupId3 = Guid.NewGuid();
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
                OpSpecs = new Dictionary<Guid, List<LibWorkInstructions.Structs.OpSpec>>
                {
                    {groupId1,  new List<LibWorkInstructions.Structs.OpSpec> {
                        new LibWorkInstructions.Structs.OpSpec{Id = specId1},
                        new LibWorkInstructions.Structs.OpSpec{Id = specId2},
                        new LibWorkInstructions.Structs.OpSpec{Id = specId3}
                    } },
                    {groupId2, new List<LibWorkInstructions.Structs.OpSpec> {
                        new LibWorkInstructions.Structs.OpSpec{Id = specId4},
                        new LibWorkInstructions.Structs.OpSpec{Id = specId5},
                        new LibWorkInstructions.Structs.OpSpec{Id = specId6}
                    } },
                    {groupId3, new List<LibWorkInstructions.Structs.OpSpec> {
                        new LibWorkInstructions.Structs.OpSpec{Id = specId7},
                        new LibWorkInstructions.Structs.OpSpec{Id = specId8},
                        new LibWorkInstructions.Structs.OpSpec{Id = specId9}
                    } },
                }
            };

            n.DataImport(sampleData);
            n.SplitOpSpecRevInOpSpec(groupId1, specId1);
            var dbPostSplit = n.DataExport();
            Assert.True(dbPostSplit.OpSpecs[groupId1].Count == 4);
            Assert.True(dbPostSplit.OpSpecs[groupId1].Last().Id == specId1);
        }

        [Test]
        public void TestCloneSpecsOverwrite()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid groupId1 = Guid.NewGuid();
            Guid groupId2 = Guid.NewGuid();
            Guid workId1 = Guid.NewGuid();
            Guid workId2 = Guid.NewGuid();
            Guid workId3 = Guid.NewGuid();
            Guid workId4 = Guid.NewGuid();
            var opSpec1 = new LibWorkInstructions.Structs.OpSpec
            {
                Id = workId1,
                IdRevGroup = groupId1,
                Class = "test",
                Comment = "test",
                Grade = "test",
                Level = "test",
                Method = "test",
                Name = "test",
                Notice = "test",
                Proctype = "test",
                Servicecond = "test",
                Status = "test",
                Type = "test",
            };
            var opSpec2 = new LibWorkInstructions.Structs.OpSpec
            {
                Id = workId2,
                IdRevGroup = groupId1,
                Class = "test",
                Comment = "test",
                Grade = "test",
                Level = "test",
                Method = "test",
                Name = "test",
                Notice = "test",
                Proctype = "test",
                Servicecond = "test",
                Status = "test",
                Type = "test",
            };
            var opSpec3 = new LibWorkInstructions.Structs.OpSpec
            {
                Id = workId3,
                IdRevGroup = groupId2,
                Class = "test",
                Comment = "test",
                Grade = "test",
                Level = "test",
                Method = "test",
                Name = "test",
                Notice = "test",
                Proctype = "test",
                Servicecond = "test",
                Status = "test",
                Type = "test",
            };
            var opSpec4 = new LibWorkInstructions.Structs.OpSpec
            {
                Id = workId4,
                IdRevGroup = groupId2,
                Class = "test",
                Comment = "test",
                Grade = "test",
                Level = "test",
                Method = "test",
                Name = "test",
                Notice = "test",
                Proctype = "test",
                Servicecond = "test",
                Status = "test",
                Type = "test",
            };
            var opSpecList1 = new List<LibWorkInstructions.Structs.OpSpec> { opSpec1, opSpec2 };
            var opSpecList2 = new List<LibWorkInstructions.Structs.OpSpec> { opSpec3, opSpec4 };
            var op1 = new LibWorkInstructions.Structs.Op
            {
                Id = 0,
                JobId = "job1",
                OpService = "Op 20",
                Seq = 2,
            };
            var op2 = new LibWorkInstructions.Structs.Op
            {
                Id = 1,
                JobId = "job2",
                OpService = "Op 30",
                Seq = 3,
            };
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                Jobs = new Dictionary<string, List<LibWorkInstructions.Structs.Job>>
                {
                    {"job1", new List<LibWorkInstructions.Structs.Job>
                    { new LibWorkInstructions.Structs.Job {
                        Id = "F110",
                        Rev = "A",
                        RevCustomer = "CUSTX",
                        RevPlan = "1.0.0",
                        Ops = new List<LibWorkInstructions.Structs.Op> {op1},
                    }
                    } },

                    {"job2", new List<LibWorkInstructions.Structs.Job>
                    { new LibWorkInstructions.Structs.Job {
                        Id = "F111",
                        Rev = "A",
                        RevCustomer = "CUSTH",
                        RevPlan = "1.0.1",
                        Ops = new List<LibWorkInstructions.Structs.Op> {op2},
                    } } }
                }
            };

            n.DataImport(sampleData);
            n.CloneOpSpecRevsBasedOnJobOp(0, 1, true);
            var dbPostClone = n.DataExport();
            //Assert.True(dbPostClone.Jobs["job2"][0].Ops[0].OpSpecs.Count == 2);
            //Assert.True(dbPostClone.Jobs["job2"][0].Ops[0].OpSpecs.SequenceEqual(dbPostClone.Jobs["job1"][0].Ops[0].OpSpecs));
        }

        [Test]
        public void TestCloneSpecsNoOverwrite()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid groupId1 = Guid.NewGuid();
            Guid groupId2 = Guid.NewGuid();
            Guid workId1 = Guid.NewGuid();
            Guid workId2 = Guid.NewGuid();
            Guid workId3 = Guid.NewGuid();
            Guid workId4 = Guid.NewGuid();
            var opSpec1 = new LibWorkInstructions.Structs.OpSpec
            {
                Id = workId1,
                IdRevGroup = groupId1,
                Class = "test",
                Comment = "test",
                Grade = "test",
                Level = "test",
                Method = "test",
                Name = "test",
                Notice = "test",
                Proctype = "test",
                Servicecond = "test",
                Status = "test",
                Type = "test",
            };
            var opSpec2 = new LibWorkInstructions.Structs.OpSpec
            {
                Id = workId2,
                IdRevGroup = groupId1,
                Class = "test",
                Comment = "test",
                Grade = "test",
                Level = "test",
                Method = "test",
                Name = "test",
                Notice = "test",
                Proctype = "test",
                Servicecond = "test",
                Status = "test",
                Type = "test",
            };
            var opSpec3 = new LibWorkInstructions.Structs.OpSpec
            {
                Id = workId3,
                IdRevGroup = groupId2,
                Class = "test",
                Comment = "test",
                Grade = "test",
                Level = "test",
                Method = "test",
                Name = "test",
                Notice = "test",
                Proctype = "test",
                Servicecond = "test",
                Status = "test",
                Type = "test",
            };
            var opSpec4 = new LibWorkInstructions.Structs.OpSpec
            {
                Id = workId4,
                IdRevGroup = groupId2,
                Class = "test",
                Comment = "test",
                Grade = "test",
                Level = "test",
                Method = "test",
                Name = "test",
                Notice = "test",
                Proctype = "test",
                Servicecond = "test",
                Status = "test",
                Type = "test",
            };
            var opSpecList1 = new List<LibWorkInstructions.Structs.OpSpec> { opSpec1, opSpec2 };
            var opSpecList2 = new List<LibWorkInstructions.Structs.OpSpec> { opSpec3, opSpec4 };
            var op1 = new LibWorkInstructions.Structs.Op
            {
                Id = 0,
                JobId = "job1",
                OpService = "Op 20",
                Seq = 2,
            };
            var op2 = new LibWorkInstructions.Structs.Op
            {
                Id = 1,
                JobId = "job2",
                OpService = "Op 30",
                Seq = 3,
            };
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                Jobs = new Dictionary<string, List<LibWorkInstructions.Structs.Job>>
                {
                    {"job1", new List<LibWorkInstructions.Structs.Job>
                    { new LibWorkInstructions.Structs.Job {
                        Id = "F110",
                        Rev = "A",
                        RevCustomer = "CUSTX",
                        RevPlan = "1.0.0",
                        Ops = new List<LibWorkInstructions.Structs.Op> {op1},
                    }
                    } },

                    {"job2", new List<LibWorkInstructions.Structs.Job>
                    { new LibWorkInstructions.Structs.Job {
                        Id = "F111",
                        Rev = "A",
                        RevCustomer = "CUSTH",
                        RevPlan = "1.0.1",
                        Ops = new List<LibWorkInstructions.Structs.Op> {op2},
                    } }}
                }
            };
            var mergedOpSpecsList = new List<LibWorkInstructions.Structs.OpSpec> { opSpec1, opSpec2, opSpec3, opSpec4 };
            n.DataImport(sampleData);
            n.CloneOpSpecRevsBasedOnJobOp(0, 1, false);
            var dbPostClone = n.DataExport();
            //Assert.True(dbPostClone.Jobs["job2"].Ops[0].OpSpecs.Count == 4);
            //Assert.True(dbPostClone.Jobs["job2"].Ops[0].OpSpecs.SequenceEqual(mergedOpSpecsList));
        }
        [Test]
        public void TestAddQualityClause()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid groupId1 = Guid.NewGuid();
            Guid groupId2 = Guid.NewGuid();
            Guid groupId3 = Guid.NewGuid();
            Guid clauseId1 = Guid.NewGuid();
            Guid clauseId2 = Guid.NewGuid();
            Guid clauseId3 = Guid.NewGuid();
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
            n.CreateQualityClause("Quality clause 1");
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
                Jobs = new Dictionary<string, List<LibWorkInstructions.Structs.Job>>
                {
                    {"job1", new List<LibWorkInstructions.Structs.Job>() },
                    {"job2", new List<LibWorkInstructions.Structs.Job>() },
                },
            };
            n.DataImport(sampleData);
            n.MergeJobRevsBasedOnJob("job1", "job2");
            var dbPostMerge = n.DataExport();
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
            n.SplitQualityClauseRevInQualityClause(groupId1, clauseId1);
            var dbPostSplit = n.DataExport();
            Assert.True(dbPostSplit.QualityClauses[groupId1].Count == 2);
            Assert.True(dbPostSplit.QualityClauses[groupId1].Last().Id == clauseId1);
        }

        [Test]
        public void TestCloneQualityClausesOverwrite()
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
                Jobs = new Dictionary<string, List<LibWorkInstructions.Structs.Job>>
                {
                    {"job1", new List<LibWorkInstructions.Structs.Job>() },
                    {"job2", new List<LibWorkInstructions.Structs.Job>() },
                }
            };
            n.DataImport(sampleData);
            n.CloneQualityClauseRevsBasedOnJobRev("job1", "job2", true);
            var dbPostClone1 = n.DataExport();
            sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                Jobs = new Dictionary<string, List<LibWorkInstructions.Structs.Job>>
                {
                    {"job1", new List<LibWorkInstructions.Structs.Job>() },
                    {"job2", new List<LibWorkInstructions.Structs.Job>() },
                }
            };
            n.DataImport(sampleData);
            n.CloneQualityClauseRevsBasedOnJobRev("job1", "job2", false);
            var dbPostClone2 = n.DataExport();
        }

        [Test]
        public void DeleteQualityClause()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid clause1 = Guid.NewGuid();
            Guid clause2 = Guid.NewGuid();
            Guid clause3 = Guid.NewGuid();
            Guid clause4 = Guid.NewGuid();
            Guid clause5 = Guid.NewGuid();
            Guid clause6 = Guid.NewGuid();
            var sampleClause1 = new LibWorkInstructions.Structs.QualityClause
            {
                Id = clause1,
                IdRevGroup = Guid.NewGuid(),
                Clause = "Test",
            };
            var sampleClause2 = new LibWorkInstructions.Structs.QualityClause
            {
                Id = clause2,
                IdRevGroup = Guid.NewGuid(),
                Clause = "Test",
            };
            var sampleClause3 = new LibWorkInstructions.Structs.QualityClause
            {
                Id = clause3,
                IdRevGroup = Guid.NewGuid(),
                Clause = "Test",
            };
            var sampleClause4 = new LibWorkInstructions.Structs.QualityClause
            {
                Id = clause4,
                IdRevGroup = Guid.NewGuid(),
                Clause = "Test",
            };
            var sampleClause5 = new LibWorkInstructions.Structs.QualityClause
            {
                Id = clause5,
                IdRevGroup = Guid.NewGuid(),
                Clause = "Test",
            };
            var sampleClause6 = new LibWorkInstructions.Structs.QualityClause
            {
                Id = clause6,
                IdRevGroup = Guid.NewGuid(),
                Clause = "Test",
            };
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                Jobs = new Dictionary<string, List<LibWorkInstructions.Structs.Job>>
                {
                    {"job1", new List<LibWorkInstructions.Structs.Job>() },
                    {"job2", new List<LibWorkInstructions.Structs.Job>() },
                },
                QualityClauses = new Dictionary<Guid, List<LibWorkInstructions.Structs.QualityClause>>
                {
                    {clause1, new List<LibWorkInstructions.Structs.QualityClause>{sampleClause1, sampleClause4, sampleClause3} },
                    {clause2, new List<LibWorkInstructions.Structs.QualityClause>{sampleClause2, sampleClause5, sampleClause6} },
                }
            };
            n.DataImport(sampleData);
            Assert.True(sampleData.QualityClauses.ContainsKey(clause1));
            Assert.True(sampleData.QualityClauses[clause1].Contains(sampleClause1));
            n.DeleteQualityClause(clause1);
            var dbVar = n.DataExport();
            Assert.False(dbVar.QualityClauses[clause1].Contains(sampleClause1));
        }
    }
}
