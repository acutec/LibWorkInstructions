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
        public void TestActivateWorkInstruction()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid workId1 = Guid.NewGuid();
            Guid workId2 = Guid.NewGuid();
            Guid groupId1 = Guid.NewGuid();
            Guid groupId2 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                WorkInstructions = new Dictionary<Guid, List<LibWorkInstructions.Structs.WorkInstruction>> {
                    { groupId1, new List<LibWorkInstructions.Structs.WorkInstruction> { new LibWorkInstructions.Structs.WorkInstruction {
                        Id = workId1,
                        IdRevGroup = groupId1,
                        Approved = true,
                        HtmlBlob = "<h1>do something</h1>",
                        Images = new List<string> { "image" },
                        Active = false
                    },
                     new LibWorkInstructions.Structs.WorkInstruction {
                        Id = workId2,
                        IdRevGroup = groupId2,
                        Approved = false,
                        HtmlBlob = "<h2>do something</h2>",
                        Images = new List<string> { "jpeg" },
                        Active = false
                    }
                    } }
                }
            };
            n.DataImport(sampleData);
            n.ActivateWorkInstruction(groupId1);
            var dbVar = n.DataExport();
            Assert.True(dbVar.WorkInstructions[groupId1].All(y => y.Active));
        }
        [Test]
        public void TestDeactivateWorkInstruction()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid workId1 = Guid.NewGuid();
            Guid workId2 = Guid.NewGuid();
            Guid groupId1 = Guid.NewGuid();
            Guid groupId2 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                WorkInstructions = new Dictionary<Guid, List<LibWorkInstructions.Structs.WorkInstruction>> {
                    { groupId1, new List<LibWorkInstructions.Structs.WorkInstruction> { new LibWorkInstructions.Structs.WorkInstruction {
                        Id = workId1,
                        IdRevGroup = groupId1,
                        Approved = true,
                        HtmlBlob = "<h1>do something</h1>",
                        Images = new List<string> { "image" },
                        Active = true
                    },
                     new LibWorkInstructions.Structs.WorkInstruction {
                        Id = workId2,
                        IdRevGroup = groupId2,
                        Approved = false,
                        HtmlBlob = "<h2>do something</h2>",
                        Images = new List<string> { "jpeg" },
                        Active = true
                    }
                    } }
                }
            };
            n.DataImport(sampleData);
            n.DeactivateWorkInstruction(groupId1);
            var dbVar = n.DataExport();
            Assert.True(dbVar.WorkInstructions[groupId1].All(y => !y.Active));
        }

        [Test]
        public void TestCloneWorkInstructionRevsAdditive()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid groupId1 = Guid.NewGuid();
            Guid groupId2 = Guid.NewGuid();
            Guid workId1 = Guid.NewGuid();
            Guid workId2 = Guid.NewGuid();
            Guid workId3 = Guid.NewGuid();
            Guid workId4 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                WorkInstructions = new Dictionary<Guid, List<LibWorkInstructions.Structs.WorkInstruction>>
                {
                    {groupId1, new List<LibWorkInstructions.Structs.WorkInstruction> {
                        new LibWorkInstructions.Structs.WorkInstruction {
                        Id = workId1, IdRevGroup = groupId1 },
                        new LibWorkInstructions.Structs.WorkInstruction {
                        Id = workId2, IdRevGroup = groupId1} } },
                    {groupId2, new List<LibWorkInstructions.Structs.WorkInstruction> {
                        new LibWorkInstructions.Structs.WorkInstruction {
                            Id = workId3, IdRevGroup = groupId2 },
                        new LibWorkInstructions.Structs.WorkInstruction {
                            Id = workId4, IdRevGroup = groupId2 } } },
                },
                WorkInstructionRefToWorkInstructionRevRefs = new Dictionary<Guid, List<Guid>>
                {
                    {groupId1, new List<Guid>{workId1, workId2}},
                    {groupId2, new List<Guid>{workId3, workId4}}
                }

            };

            n.DataImport(sampleData);
            n.CloneWorkInstructionRevs(workId1, workId3, true);
            var dbPostClone = n.DataExport();

            Assert.True(dbPostClone.WorkInstructions[groupId2].Count == 4);
            Assert.True(dbPostClone.WorkInstructionRefToWorkInstructionRevRefs[groupId2].OrderBy(y => y).SequenceEqual(new List<Guid> { workId1, workId2, workId3, workId4}.OrderBy(y => y)));
        }

        [Test]
        public void TestCloneWorkInstructionRevsNotAdditive()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid groupId1 = Guid.NewGuid();
            Guid groupId2 = Guid.NewGuid();
            Guid workId1 = Guid.NewGuid();
            Guid workId2 = Guid.NewGuid();
            Guid workId3 = Guid.NewGuid();
            Guid workId4 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                WorkInstructions = new Dictionary<Guid, List<LibWorkInstructions.Structs.WorkInstruction>>
                {
                    {groupId1, new List<LibWorkInstructions.Structs.WorkInstruction> {
                        new LibWorkInstructions.Structs.WorkInstruction {
                        Id = workId1, IdRevGroup = groupId1 },
                        new LibWorkInstructions.Structs.WorkInstruction {
                        Id = workId2, IdRevGroup = groupId1} } },
                    {groupId2, new List<LibWorkInstructions.Structs.WorkInstruction> {
                        new LibWorkInstructions.Structs.WorkInstruction {
                            Id = workId3, IdRevGroup = groupId2 },
                        new LibWorkInstructions.Structs.WorkInstruction {
                            Id = workId4, IdRevGroup = groupId2 } } },
                },
                WorkInstructionRefToWorkInstructionRevRefs = new Dictionary<Guid, List<Guid>>
                {
                    {groupId1, new List<Guid>{workId1, workId2}},
                    {groupId2, new List<Guid>{workId3, workId4}}
                }

            };

            n.DataImport(sampleData);
            n.CloneWorkInstructionRevs(workId1, workId3, false);
            var dbPostClone = n.DataExport();

            Assert.True(dbPostClone.WorkInstructions[groupId2].Count == 2);
            Assert.True(dbPostClone.WorkInstructionRefToWorkInstructionRevRefs[groupId2].SequenceEqual(dbPostClone.WorkInstructionRefToWorkInstructionRevRefs[groupId1]));
        }

        [Test]
        public void TestCreateOpSpec()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                OpSpecs = new Dictionary<Guid, List<LibWorkInstructions.Structs.OpSpec>>(),
            };
            var testAddSpec = new LibWorkInstructions.Structs.OpSpec
            {
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
            Assert.True(dbPostAdd.OpSpecs.First().Value[0].Id != null);
            Assert.True(dbPostAdd.OpSpecs.First().Value[0].IdRevGroup != null);
            Assert.True(dbPostAdd.OpSpecs.First().Value[0].RevSeq == 0);
        }

        [Test]
        public void TestActivateOpSpec()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid groupId1 = Guid.NewGuid();
            Guid specId1 = Guid.NewGuid();
            Guid specId2 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                OpSpecs = new Dictionary<Guid, List<LibWorkInstructions.Structs.OpSpec>>
                {
                    { groupId1, new List<LibWorkInstructions.Structs.OpSpec> {
                        new LibWorkInstructions.Structs.OpSpec{Id = specId1, Name = "spec1", Active = false},
                        new LibWorkInstructions.Structs.OpSpec{Id = specId2, Name = "spec2", Active = false} } }
                }
            };

            n.DataImport(sampleData);
            n.ActivateOpSpec(groupId1);
            var dbPostDelete = n.DataExport();
            Assert.True(dbPostDelete.OpSpecs[groupId1].All(y => y.Active));
        }

        [Test]
        public void TestDeactivateOpSpec()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid groupId1 = Guid.NewGuid();
            Guid specId1 = Guid.NewGuid();
            Guid specId2 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                OpSpecs = new Dictionary<Guid, List<LibWorkInstructions.Structs.OpSpec>>
                {
                    { groupId1, new List<LibWorkInstructions.Structs.OpSpec> {
                        new LibWorkInstructions.Structs.OpSpec{Id = specId1, Name = "spec1", Active = true},
                        new LibWorkInstructions.Structs.OpSpec{Id = specId2, Name = "spec2", Active = true} } }
                }
            };

            n.DataImport(sampleData);
            n.DeactivateOpSpec(groupId1);
            var dbPostDelete = n.DataExport();
            Assert.True(dbPostDelete.OpSpecs[groupId1].All(y => !y.Active));
        }

        [Test]
        public void TestMergeOpSpecRevs()
        {
            Guid opSpecRev1 = Guid.NewGuid();
            Guid opSpecRev2 = Guid.NewGuid();
            Guid opSpecRev3 = Guid.NewGuid();
            Guid opSpecRev4 = Guid.NewGuid();
            var n = new LibWorkInstructions.BusinessLogic();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                OpSpecRevs = new List<Guid> { opSpecRev1, opSpecRev2, opSpecRev3, opSpecRev4 },
                OpRefToOpSpecRevRefs = new Dictionary<int, List<Guid>>
                {
                    {1, new List<Guid>{opSpecRev1, opSpecRev2}},
                    {2, new List<Guid>{opSpecRev3, opSpecRev4}}
                }
            };
            n.DataImport(sampleData);
            n.MergeOpSpecRevs(1, 2);
            var dbPostMerge = n.DataExport();
            Assert.True(dbPostMerge.OpRefToOpSpecRevRefs[1].Count == 4);
            Assert.True(dbPostMerge.OpRefToOpSpecRevRefs[2].Count == 4);
        }

        [Test]
        public void TestSplitOpSpecRev()
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
            n.SplitOpSpecRev(groupId1, specId1);
            var dbPostSplit = n.DataExport();
            Assert.True(dbPostSplit.OpSpecs[groupId1].Count == 4);
            Assert.True(dbPostSplit.OpSpecs[groupId1].Last().Id == specId1);
        }

        [Test]
        public void TestCloneOpSpecRevsBasedOnJobOpAdditive()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid specId1 = Guid.NewGuid();
            Guid specId2 = Guid.NewGuid();
            Guid specId3 = Guid.NewGuid();
            Guid specId4 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                OpRefToOpSpecRevRefs = new Dictionary<int, List<Guid>>
                {
                    {1, new List<Guid> { specId1, specId2 } },
                    {2, new List<Guid> { specId3, specId4 } }
                }
            };

            n.DataImport(sampleData);
            n.CloneOpSpecRevsBasedOnJobOp(1, 2, true);
            var dbPostClone = n.DataExport();
            Assert.True(dbPostClone.OpRefToOpSpecRevRefs[2].Count == 4);
            Assert.True(dbPostClone.OpRefToOpSpecRevRefs[1].All(y => dbPostClone.OpRefToOpSpecRevRefs[2].Contains(y)));
        }

        [Test]
        public void TestCloneOpSpecRevsBasedOnJobOpNotAdditive()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid specId1 = Guid.NewGuid();
            Guid specId2 = Guid.NewGuid();
            Guid specId3 = Guid.NewGuid();
            Guid specId4 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                OpRefToOpSpecRevRefs = new Dictionary<int, List<Guid>>
                {
                    {1, new List<Guid> { specId1, specId2 } },
                    {2, new List<Guid> { specId3, specId4 } }
                }
            };

            n.DataImport(sampleData);
            n.CloneOpSpecRevsBasedOnJobOp(1, 2, false);
            var dbPostClone = n.DataExport();
            Assert.True(dbPostClone.OpRefToOpSpecRevRefs[2].Count == 2);
            Assert.True(dbPostClone.OpRefToOpSpecRevRefs[2].SequenceEqual(dbPostClone.OpRefToOpSpecRevRefs[1]));
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
