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
        public void TestDeleteJob()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid clauseRev1 = Guid.NewGuid();
            Guid clauseRev2 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                Jobs = new Dictionary<string, List<LibWorkInstructions.Structs.Job>>
                {
                    {"job1", new List<LibWorkInstructions.Structs.Job>
                    {
                        new LibWorkInstructions.Structs.Job {Id = "job1", Rev = "job1-A", Ops = new List<LibWorkInstructions.Structs.Op> { 
                            new LibWorkInstructions.Structs.Op { Id = 1, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 2, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 3, JobId = "job1"},
                        } },
                        new LibWorkInstructions.Structs.Job {Id = "job1", Rev = "job1-B", Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 4, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 5, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 6, JobId = "job1"},
                        } }
                    } 
                    }
                },
                Ops = new Dictionary<int, LibWorkInstructions.Structs.Op>
                {
                    {1, new LibWorkInstructions.Structs.Op { Id = 1, JobId = "job1"}},
                    {2, new LibWorkInstructions.Structs.Op { Id = 2, JobId = "job1"}},
                    {3, new LibWorkInstructions.Structs.Op { Id = 3, JobId = "job1"}},
                    {4, new LibWorkInstructions.Structs.Op { Id = 4, JobId = "job1"}},
                    {5, new LibWorkInstructions.Structs.Op { Id = 5, JobId = "job1"}},
                    {6, new LibWorkInstructions.Structs.Op { Id = 6, JobId = "job1"}}
                },
                JobRefToJobRevRefs = new Dictionary<string, List<string>>
                {
                    {"job1", new List<string>{"job1-A", "job1-B"} }
                },
                JobRevRefToOpRefs = new Dictionary<string, List<int>>
                {
                    {"job1-A", new List<int>{1, 2, 3}},
                    {"job1-B", new List<int>{4, 5, 6}}
                },
                QualityClauseRevRefToJobRevRefs = new Dictionary<Guid, List<string>>
                {
                    {clauseRev1, new List<string> {"job1-A", "job2-B"} },
                    {clauseRev2, new List<string> {"job1-B", "job2-A"} }
                }
            };
            n.DataImport(sampleData);
            n.DeleteJob("job1");
            var dbPostDelete = n.DataExport();
            Assert.True(dbPostDelete.Jobs.Count == 0);
            Assert.True(dbPostDelete.JobRefToJobRevRefs.Count == 0);
            Assert.True(dbPostDelete.JobRevRefToOpRefs.Count == 0);
            Assert.True(dbPostDelete.QualityClauseRevRefToJobRevRefs[clauseRev1].Count == 1);
            Assert.True(dbPostDelete.QualityClauseRevRefToJobRevRefs[clauseRev1][0] == "job2-B");
            Assert.True(dbPostDelete.QualityClauseRevRefToJobRevRefs[clauseRev2].Count == 1);
            Assert.True(dbPostDelete.QualityClauseRevRefToJobRevRefs[clauseRev2][0] == "job2-A");
        }

        [Test]
        public void TestCreateJobRev()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                Jobs = new Dictionary<string, List<LibWorkInstructions.Structs.Job>>
                {
                    {"job1", new List<LibWorkInstructions.Structs.Job>
                    {
                        new LibWorkInstructions.Structs.Job {Id = "job1", Rev = "job1-A", Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 1, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 2, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 3, JobId = "job1"},
                        } },
                        new LibWorkInstructions.Structs.Job {Id = "job1", Rev = "job1-B", Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 4, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 5, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 6, JobId = "job1"},
                        } }
                    }
                    }
                },
                JobRevs = new List<string> { "job1-A", "job1-B" },
            };
            n.DataImport(sampleData);
            n.CreateJobRev("job1", "job1-A","job1-C");
            var dbPostCreate = n.DataExport();
            Assert.True(dbPostCreate.Jobs["job1"].Count == 3);
            Assert.True(dbPostCreate.Jobs["job1"].Last().Rev == "job1-C");
            Assert.True(dbPostCreate.Jobs["job1"].Last().RevSeq == 2);
        }

        [Test]
        public void TestCreateJobRevFromScratch()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                Jobs = new Dictionary<string, List<LibWorkInstructions.Structs.Job>>
                {
                    {"job1", new List<LibWorkInstructions.Structs.Job>
                    {
                        new LibWorkInstructions.Structs.Job {Id = "job1", Rev = "job1-A", Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 1, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 2, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 3, JobId = "job1"},
                        } },
                        new LibWorkInstructions.Structs.Job {Id = "job1", Rev = "job1-B", Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 4, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 5, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 6, JobId = "job1"},
                        } }
                    }
                    }
                },
                JobRevs = new List<string> { "job1-A", "job1-B" },
            };
            n.DataImport(sampleData);
            n.CreateJobRev(new LibWorkInstructions.Structs.Job
            {
                Id = "job1",
                Rev = "job1-C",
                Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 7, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 8, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 9, JobId = "job1"}
                }
            });
            var dbPostCreate = n.DataExport();
            Assert.True(dbPostCreate.Jobs["job1"].Count == 3);
            Assert.True(dbPostCreate.Jobs["job1"].Last().Rev == "job1-C");
            Assert.True(dbPostCreate.Jobs["job1"].Last().RevSeq == 2);
        }

        [Test]
        public void TestUpdateJobRev()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid groupId1 = Guid.NewGuid();
            Guid groupId2 = Guid.NewGuid();
            Guid clauseId1 = Guid.NewGuid();
            Guid clauseId2 = Guid.NewGuid();
            Guid clauseId3 = Guid.NewGuid();
            Guid clauseId4 = Guid.NewGuid();
            Guid clauseId5 = Guid.NewGuid();
            Guid clauseId6 = Guid.NewGuid();
            Guid clauseId7 = Guid.NewGuid();
            Guid clauseId8 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                Jobs = new Dictionary<string, List<LibWorkInstructions.Structs.Job>>
                {
                    {"job1", new List<LibWorkInstructions.Structs.Job>{
                        new LibWorkInstructions.Structs.Job {RevSeq = 0, Id = "job1", Rev = "job1-A" , QualityClauses = new List<LibWorkInstructions.Structs.QualityClause> {
                            new LibWorkInstructions.Structs.QualityClause {Id = clauseId1},
                            new LibWorkInstructions.Structs.QualityClause {Id = clauseId2}}, Ops = new List<LibWorkInstructions.Structs.Op> {
                                    new LibWorkInstructions.Structs.Op { Id = 1, JobId = "job1"},
                                    new LibWorkInstructions.Structs.Op { Id = 2, JobId = "job1"},
                                    new LibWorkInstructions.Structs.Op { Id = 3, JobId = "job1"},}},
                        new LibWorkInstructions.Structs.Job {RevSeq = 1, Id = "job1", Rev = "job1-B" , QualityClauses = new List<LibWorkInstructions.Structs.QualityClause> {
                            new LibWorkInstructions.Structs.QualityClause {Id = clauseId3},
                            new LibWorkInstructions.Structs.QualityClause {Id = clauseId4}}, Ops = new List<LibWorkInstructions.Structs.Op> {
                                    new LibWorkInstructions.Structs.Op { Id = 4, JobId = "job1"},
                                    new LibWorkInstructions.Structs.Op { Id = 5, JobId = "job1"},
                                    new LibWorkInstructions.Structs.Op { Id = 6, JobId = "job1"},} } } },
                    {"job2", new List<LibWorkInstructions.Structs.Job>{
                        new LibWorkInstructions.Structs.Job {RevSeq = 0, Id = "job2", Rev = "job2-A" , QualityClauses = new List<LibWorkInstructions.Structs.QualityClause> {
                        new LibWorkInstructions.Structs.QualityClause {Id = clauseId5},
                        new LibWorkInstructions.Structs.QualityClause {Id = clauseId6}}, Ops = new List<LibWorkInstructions.Structs.Op> {
                                    new LibWorkInstructions.Structs.Op { Id = 7, JobId = "job2"},
                                    new LibWorkInstructions.Structs.Op { Id = 8, JobId = "job2"},
                                    new LibWorkInstructions.Structs.Op { Id = 9, JobId = "job2"},}},
                        new LibWorkInstructions.Structs.Job {RevSeq = 1, Id = "job2", Rev = "job2-B" , QualityClauses = new List<LibWorkInstructions.Structs.QualityClause> {
                        new LibWorkInstructions.Structs.QualityClause {Id = clauseId7},
                        new LibWorkInstructions.Structs.QualityClause {Id = clauseId8}}, Ops = new List<LibWorkInstructions.Structs.Op> {
                                    new LibWorkInstructions.Structs.Op { Id = 10, JobId = "job2"},
                                    new LibWorkInstructions.Structs.Op { Id = 11, JobId = "job2"},
                                    new LibWorkInstructions.Structs.Op { Id = 12, JobId = "job2"},}} } },
                },
                Ops = new Dictionary<int, LibWorkInstructions.Structs.Op>
                {
                    {1, new LibWorkInstructions.Structs.Op { Id = 1, JobId = "job1"}},
                    {2, new LibWorkInstructions.Structs.Op { Id = 2, JobId = "job1"}},
                    {3, new LibWorkInstructions.Structs.Op { Id = 3, JobId = "job1"}},
                    {4, new LibWorkInstructions.Structs.Op { Id = 4, JobId = "job1"}},
                    {5, new LibWorkInstructions.Structs.Op { Id = 5, JobId = "job1"}},
                    {6, new LibWorkInstructions.Structs.Op { Id = 6, JobId = "job1"}},
                    {7, new LibWorkInstructions.Structs.Op { Id = 7, JobId = "job2"}},
                    {8, new LibWorkInstructions.Structs.Op { Id = 8, JobId = "job2"}},
                    {9, new LibWorkInstructions.Structs.Op { Id = 9, JobId = "job2"}},
                    {10, new LibWorkInstructions.Structs.Op { Id = 10, JobId = "job2"}},
                    {11, new LibWorkInstructions.Structs.Op { Id = 11, JobId = "job2"}},
                    {12, new LibWorkInstructions.Structs.Op { Id = 12, JobId = "job2"}}
                },
                QualityClauses = new Dictionary<Guid, List<LibWorkInstructions.Structs.QualityClause>>
                {
                    {groupId1, new List<LibWorkInstructions.Structs.QualityClause> {
                        new LibWorkInstructions.Structs.QualityClause {Id = clauseId1}} },
                    {groupId2, new List<LibWorkInstructions.Structs.QualityClause> {
                        new LibWorkInstructions.Structs.QualityClause {Id = clauseId2}} },
                },
            };
            n.DataImport(sampleData);
            n.UpdateJobRev(new LibWorkInstructions.Structs.Job
            {
                Id = "job1",
                RevSeq = 1,
                Rev = "job1-B",
                QualityClauses = new List<LibWorkInstructions.Structs.QualityClause>
                {
                    new LibWorkInstructions.Structs.QualityClause {Id = clauseId1},
                    new LibWorkInstructions.Structs.QualityClause {Id = clauseId2}
                },
                Ops = new List<LibWorkInstructions.Structs.Op>
                {
                    new LibWorkInstructions.Structs.Op {Id = 3, JobId = "job1"},
                    new LibWorkInstructions.Structs.Op {Id = 6, JobId = "job1"},
                    new LibWorkInstructions.Structs.Op {Id = 7, JobId = "job1"},
                }
            });
            var dbPostUpdate = n.DataExport();
            Assert.True(dbPostUpdate.Jobs["job1"][1].Ops.Count == 3);
            Assert.True(dbPostUpdate.Jobs["job1"][1].QualityClauses[0].Id == clauseId1);
            Assert.True(dbPostUpdate.Jobs["job1"][1].QualityClauses[1].Id == clauseId2);
            Assert.True(dbPostUpdate.Jobs["job1"][1].Ops[0].Id == 3);
            Assert.True(dbPostUpdate.Jobs["job1"][1].Ops[1].Id == 6);
            Assert.True(dbPostUpdate.Jobs["job1"][1].Ops[2].Id == 7);
        }

        [Test]
        public void TestActivateJobRev()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                Jobs = new Dictionary<string, List<LibWorkInstructions.Structs.Job>>
                {
                    {"job1", new List<LibWorkInstructions.Structs.Job>
                    {
                        new LibWorkInstructions.Structs.Job {Id = "job1", Rev = "job1-A", Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 1, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 2, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 3, JobId = "job1"},
                        }, Active = false },
                        new LibWorkInstructions.Structs.Job {Id = "job1", Rev = "job1-B", Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 4, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 5, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 6, JobId = "job1"},
                        }, Active = false }
                    }
                    }
                },
            };
            n.DataImport(sampleData);
            n.ActivateJobRev("job1", "job1-B");
            var dbPostDeactivate = n.DataExport();
            Assert.True(dbPostDeactivate.Jobs["job1"][1].Active);
        }

        [Test]
        public void TestDeactivateJobRev()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                Jobs = new Dictionary<string, List<LibWorkInstructions.Structs.Job>>
                {
                    {"job1", new List<LibWorkInstructions.Structs.Job>
                    {
                        new LibWorkInstructions.Structs.Job {Id = "job1", Rev = "job1-A", Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 1, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 2, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 3, JobId = "job1"},
                        }, Active = true },
                        new LibWorkInstructions.Structs.Job {Id = "job1", Rev = "job1-B", Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 4, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 5, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 6, JobId = "job1"},
                        }, Active = true }
                    }
                    }
                },
            };
            n.DataImport(sampleData);
            n.DeactivateJobRev("job1", "job1-A");
            var dbPostDeactivate = n.DataExport();
            Assert.False(dbPostDeactivate.Jobs["job1"][0].Active);
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
                        IdRevGroup = groupId1,
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
                        IdRevGroup = groupId1,
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
        public void TestCreateWorkInstructionRev()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid workId1 = Guid.NewGuid();
            Guid workId2 = Guid.NewGuid();
            Guid groupId1 = Guid.NewGuid();
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
                            IdRevGroup = groupId1,
                            Approved = false,
                            HtmlBlob = "<h2>do something</h2>",
                            Images = new List<string> { "jpeg" },
                            Active = false
                        }
                    } }
                },
                WorkInstructionRevs = new List<Guid> { workId1, workId2 },
                WorkInstructionRefToWorkInstructionRevRefs = new Dictionary<Guid, List<Guid>>
                {
                    {groupId1, new List<Guid> {workId1, workId2} }
                }
            };
            n.DataImport(sampleData);
            n.CreateWorkInstructionRev(groupId1, workId1);
            var dbPostCreate = n.DataExport();
            Assert.True(dbPostCreate.WorkInstructions[groupId1].Count == 3);
            Assert.False(dbPostCreate.WorkInstructions[groupId1][2].Id == dbPostCreate.WorkInstructions[groupId1][0].Id);
            Assert.True(dbPostCreate.WorkInstructions[groupId1][2].IdRevGroup == groupId1);
            Assert.True(dbPostCreate.WorkInstructions[groupId1][2].RevSeq == 2);
            Assert.True(dbPostCreate.WorkInstructionRevs.Count == 3);
            Assert.True(dbPostCreate.WorkInstructionRefToWorkInstructionRevRefs[groupId1].Count == 3);
        }

        [Test]
        public void TestCreateWorkInstructionRevFromScratch()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid workId1 = Guid.NewGuid();
            Guid workId2 = Guid.NewGuid();
            Guid workId3 = Guid.NewGuid();
            Guid groupId1 = Guid.NewGuid();
            var workInstruction = new LibWorkInstructions.Structs.WorkInstruction
            {
                Id = workId3,
                IdRevGroup = groupId1,
                Approved = true,
                HtmlBlob = "<h3>do something</h3>",
                Images = new List<string> { "image1" },
                Active = true
            };
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
                        IdRevGroup = groupId1,
                        Approved = false,
                        HtmlBlob = "<h2>do something</h2>",
                        Images = new List<string> { "jpeg" },
                        Active = false
                    }
                    } }
                },
                WorkInstructionRefToWorkInstructionRevRefs = new Dictionary<Guid, List<Guid>>
                {
                    {groupId1, new List<Guid> { workId1, workId2 } }
                },
                WorkInstructionRevs = new List<Guid> { workId1, workId2 }
            };
            n.DataImport(sampleData);
            n.CreateWorkInstructionRev(workInstruction);
            var dbPostCreate = n.DataExport();
            Assert.True(dbPostCreate.WorkInstructions[groupId1].Count == 3);
            Assert.True(dbPostCreate.WorkInstructions[groupId1][2].IdRevGroup == groupId1);
            Assert.True(dbPostCreate.WorkInstructions[groupId1][2].Id == workId3);
            Assert.True(dbPostCreate.WorkInstructionRefToWorkInstructionRevRefs[groupId1].Count == 3);
            Assert.True(dbPostCreate.WorkInstructionRefToWorkInstructionRevRefs[groupId1][2] == workId3);
            Assert.True(dbPostCreate.WorkInstructionRevs.Count == 3);
        }

        [Test]
        public void TestUpdateWorkInstructionRev()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid workId1 = Guid.NewGuid();
            Guid workId2 = Guid.NewGuid();
            Guid groupId1 = Guid.NewGuid();
            var newWorkInstructionRev = new LibWorkInstructions.Structs.WorkInstruction
            {
                Id = workId2,
                IdRevGroup = groupId1,
                Approved = true,
                Images = new List<string> { "image23" },
                Active = true
            };
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
                        IdRevGroup = groupId1,
                        Approved = false,
                        HtmlBlob = "<h2>do something</h2>",
                        Images = new List<string> { "jpeg" },
                        Active = false
                    }
                    } }
                }
            };
            n.DataImport(sampleData);
            n.UpdateWorkInstructionRev(newWorkInstructionRev);
            var dbVarPostUpdate = n.DataExport();
            Assert.True(dbVarPostUpdate.WorkInstructions[groupId1][1].Approved);
            Assert.True(dbVarPostUpdate.WorkInstructions[groupId1][1].Images.SequenceEqual(new List<string> { "image23" }));
            Assert.True(dbVarPostUpdate.WorkInstructions[groupId1].Count == 2);
        }

        [Test]
        public void TestActivateWorkInstructionRev()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid workId1 = Guid.NewGuid();
            Guid workId2 = Guid.NewGuid();
            Guid groupId1 = Guid.NewGuid();
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
                            IdRevGroup = groupId1,
                            Approved = false,
                            HtmlBlob = "<h2>do something</h2>",
                            Images = new List<string> { "jpeg" },
                            Active = false
                        }
                    } }
                }
            };
            n.DataImport(sampleData);
            n.ActivateWorkInstructionRev(groupId1, workId1);
            var dbPostActivate = n.DataExport();
            Assert.True(dbPostActivate.WorkInstructions[groupId1][0].Active);
        }

        [Test]
        public void TestDeactivateWorkInstructionRev()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid workId1 = Guid.NewGuid();
            Guid workId2 = Guid.NewGuid();
            Guid groupId1 = Guid.NewGuid();
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
                            IdRevGroup = groupId1,
                            Approved = false,
                            HtmlBlob = "<h2>do something</h2>",
                            Images = new List<string> { "jpeg" },
                            Active = false
                        }
                    } }
                }
            };
            n.DataImport(sampleData);
            n.DeactivateWorkInstructionRev(groupId1, workId1);
            var dbPostDeactivate = n.DataExport();
            Assert.False(dbPostDeactivate.WorkInstructions[groupId1][0].Active);
        }

        [Test]
        public void TestMergeJobs()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                Jobs = new Dictionary<string, List<LibWorkInstructions.Structs.Job>>
                {
                    {"job1", new List<LibWorkInstructions.Structs.Job>
                    {
                        new LibWorkInstructions.Structs.Job {Id = "job1", Rev = "Rev A[1.2.3]", Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 1, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 2, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 3, JobId = "job1"},
                        } },
                        new LibWorkInstructions.Structs.Job {Id = "job1", Rev = "Rev B[1.2.3]", Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 4, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 5, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 6, JobId = "job1"},
                        } }
                    }
                    },
                    {"job2", new List<LibWorkInstructions.Structs.Job>
                    {
                        new LibWorkInstructions.Structs.Job {Id = "job2", Rev = "Rev A[1.4.2]", Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 7, JobId = "job2"},
                            new LibWorkInstructions.Structs.Op { Id = 8, JobId = "job2"},
                            new LibWorkInstructions.Structs.Op { Id = 9, JobId = "job2"},
                        } },
                        new LibWorkInstructions.Structs.Job {Id = "job2", Rev = "Rev B[1.2.5]", Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 10, JobId = "job2"},
                            new LibWorkInstructions.Structs.Op { Id = 11, JobId = "job2"},
                            new LibWorkInstructions.Structs.Op { Id = 12, JobId = "job2"},
                        } }
                    }
                    }
                },
                JobRevs = new List<string> { "Rev A[1.2.3]", "Rev B[1.2.3]", "Rev A[1.4.2]", "Rev B[1.2.5]" },
            };
            n.DataImport(sampleData);
            n.MergeJobs("job1", "job2");
            var dbPostMerge = n.DataExport();
            Assert.True(dbPostMerge.Jobs.Count == 1);
            Assert.True(dbPostMerge.Jobs["job1"].Count == 4);
            Assert.True(dbPostMerge.Jobs["job1"][2].Rev == "Rev C[1.4.2]");
            Assert.True(dbPostMerge.Jobs["job1"][3].Rev == "Rev D[1.2.5]");
        }

        [Test]
        public void TestSplitJobRev()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                Jobs = new Dictionary<string, List<LibWorkInstructions.Structs.Job>>
                {
                    {"job1", new List<LibWorkInstructions.Structs.Job>
                    {
                        new LibWorkInstructions.Structs.Job {Id = "job1", Rev = "Rev A[1.2.3]", Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 1, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 2, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 3, JobId = "job1"},
                        } },
                        new LibWorkInstructions.Structs.Job {Id = "job1", Rev = "Rev B[1.2.3]", Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 4, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 5, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 6, JobId = "job1"},
                        } }
                    }
                    },
                    {"job2", new List<LibWorkInstructions.Structs.Job>
                    {
                        new LibWorkInstructions.Structs.Job {Id = "job2", Rev = "Rev A[1.4.2]", Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 7, JobId = "job2"},
                            new LibWorkInstructions.Structs.Op { Id = 8, JobId = "job2"},
                            new LibWorkInstructions.Structs.Op { Id = 9, JobId = "job2"},
                        } },
                        new LibWorkInstructions.Structs.Job {Id = "job2", Rev = "Rev B[1.2.5]", Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 10, JobId = "job2"},
                            new LibWorkInstructions.Structs.Op { Id = 11, JobId = "job2"},
                            new LibWorkInstructions.Structs.Op { Id = 12, JobId = "job2"},
                        } }
                    }
                    }
                },
                JobRevs = new List<string> { "Rev A[1.2.3]", "Rev B[1.2.3]", "Rev A[1.4.2]", "Rev B[1.2.5]" },
                JobRefToJobRevRefs = new Dictionary<string, List<string>>
                {
                    { "job1", new List<string>{ "Rev A[1.2.3]", "Rev B[1.2.3]" } },
                    { "job2", new List<string>{ "Rev A[1.4.2]", "Rev B[1.2.5]" } }
                }
            };
            n.DataImport(sampleData);
            n.SplitJobRev("job1", "Rev A[1.2.3]", "Rev C[1.0.0]");
            var dbPostSplit = n.DataExport();
            Assert.True(dbPostSplit.Jobs["job1"].Count == 3);
            Assert.True(dbPostSplit.Jobs["job1"][2].Rev == "Rev C[1.0.0]");
            Assert.True(dbPostSplit.Jobs["job1"][2].RevSeq == 2);
            Assert.True(dbPostSplit.Jobs["job1"][2].Ops.SequenceEqual(dbPostSplit.Jobs["job1"][0].Ops));
            Assert.True(dbPostSplit.JobRefToJobRevRefs["job1"][2] == "Rev C[1.0.0]");
        }

        [Test]
        public void TestCloneJobRevsAdditive()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                Jobs = new Dictionary<string, List<LibWorkInstructions.Structs.Job>>
                {
                    {"job1", new List<LibWorkInstructions.Structs.Job>
                    {
                        new LibWorkInstructions.Structs.Job {Id = "job1", Rev = "Rev A[1.2.3]", Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 1, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 2, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 3, JobId = "job1"},
                        } },
                        new LibWorkInstructions.Structs.Job {Id = "job1", Rev = "Rev B[1.2.3]", Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 4, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 5, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 6, JobId = "job1"},
                        } }
                    }
                    },
                    {"job2", new List<LibWorkInstructions.Structs.Job>
                    {
                        new LibWorkInstructions.Structs.Job {Id = "job2", Rev = "Rev A[1.4.2]", Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 7, JobId = "job2"},
                            new LibWorkInstructions.Structs.Op { Id = 8, JobId = "job2"},
                            new LibWorkInstructions.Structs.Op { Id = 9, JobId = "job2"},
                        } },
                        new LibWorkInstructions.Structs.Job {Id = "job2", Rev = "Rev B[1.2.5]", Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 10, JobId = "job2"},
                            new LibWorkInstructions.Structs.Op { Id = 11, JobId = "job2"},
                            new LibWorkInstructions.Structs.Op { Id = 12, JobId = "job2"},
                        } }
                    }
                    }
                },
                JobRevs = new List<string> { "Rev A[1.2.3]", "Rev B[1.2.3]", "Rev A[1.4.2]", "Rev B[1.2.5]" },
                JobRefToJobRevRefs = new Dictionary<string, List<string>>
                {
                    { "job1", new List<string>{ "Rev A[1.2.3]", "Rev B[1.2.3]" } },
                    { "job2", new List<string>{ "Rev A[1.4.2]", "Rev B[1.2.5]" } }
                }
            };
            n.DataImport(sampleData);
            n.CloneJobRevs("job1", "job2", true);
            var dbPostClone = n.DataExport();
            Assert.True(dbPostClone.Jobs["job2"].Count == 4);
            Assert.True(dbPostClone.Jobs["job2"][2].Rev == "Rev C[1.4.2]");
            Assert.True(dbPostClone.Jobs["job2"][3].Rev == "Rev D[1.2.5]");
            Assert.True(dbPostClone.JobRefToJobRevRefs["job2"].SequenceEqual(new List<string> { "Rev A[1.2.3]", "Rev B[1.2.3]", "Rev C[1.4.2]", "Rev D[1.2.5]" }));
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
                    { groupId1, new List<LibWorkInstructions.Structs.WorkInstruction> {
                        new LibWorkInstructions.Structs.WorkInstruction {
                            Id = workId1, IdRevGroup = groupId1 },
                        new LibWorkInstructions.Structs.WorkInstruction {
                            Id = workId2, IdRevGroup = groupId1 } } },
                    { groupId2, new List<LibWorkInstructions.Structs.WorkInstruction> {
                        new LibWorkInstructions.Structs.WorkInstruction {
                            Id = workId3, IdRevGroup = groupId2 },
                        new LibWorkInstructions.Structs.WorkInstruction {
                            Id = workId4, IdRevGroup = groupId2 } } },
                },
                WorkInstructionRefToWorkInstructionRevRefs = new Dictionary<Guid, List<Guid>>
                {
                    { groupId1, new List<Guid> { workId1, workId2 } },
                    { groupId2, new List<Guid> { workId3, workId4 } }
                },
                WorkInstructionRevs = new List<Guid> { workId1, workId2, workId3, workId4 }
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
                },
                WorkInstructionRevs = new List<Guid> { workId1, workId2, workId3, workId4 }
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
        public void TestCreateOpSpecRev()
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
                },
                OpSpecRevs = new List<Guid> { specId1, specId2 },
                OpSpecRevRefToOpRefs = new Dictionary<Guid, List<int>>
                {
                    {specId1, new List<int>{4,5,6} },
                    {specId2, new List<int>{5,6,7} }
                }
            };

            n.DataImport(sampleData);
            n.CreateOpSpecRev(groupId1, specId1, "spec3");
            var dbPostCreate = n.DataExport();
            Assert.True(dbPostCreate.OpSpecs[groupId1].Count == 3);
            Assert.True(dbPostCreate.OpSpecs[groupId1][2].RevSeq == 2);
            Assert.False(dbPostCreate.OpSpecs[groupId1][2].Id == dbPostCreate.OpSpecs[groupId1][0].Id);
            Assert.False(dbPostCreate.OpSpecs[groupId1][2].Name == dbPostCreate.OpSpecs[groupId1][0].Name);
            Assert.True(dbPostCreate.OpSpecRevs.Count == 3);
            Assert.True(dbPostCreate.OpSpecRevRefToOpRefs[dbPostCreate.OpSpecs[groupId1][2].Id] == dbPostCreate.OpSpecRevRefToOpRefs[specId1]);
        }

        [Test]
        public void TestCreateOpSpecRevFromScratch()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid groupId1 = Guid.NewGuid();
            Guid specId1 = Guid.NewGuid();
            Guid specId2 = Guid.NewGuid();
            Guid specId3 = Guid.NewGuid();
            var opSpec = new LibWorkInstructions.Structs.OpSpec { Id = specId3, IdRevGroup = groupId1, Name = "spec3", Active = true };
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                OpSpecs = new Dictionary<Guid, List<LibWorkInstructions.Structs.OpSpec>>
                {
                    { groupId1, new List<LibWorkInstructions.Structs.OpSpec> {
                        new LibWorkInstructions.Structs.OpSpec{Id = specId1, Name = "spec1", Active = true},
                        new LibWorkInstructions.Structs.OpSpec{Id = specId2, Name = "spec2", Active = true} } }
                },
                OpSpecRevs = new List<Guid> {specId1, specId2},
                OpSpecRevRefToOpRefs = new Dictionary<Guid, List<int>>
                {
                    {specId1, new List<int>{4,5,6} },
                    {specId2, new List<int>{5,6,7} }
                }
            };

            n.DataImport(sampleData);
            n.CreateOpSpec(opSpec);
            var dbPostDelete = n.DataExport();
            Assert.True(dbPostDelete.OpSpecs[groupId1].Count == 3);
            Assert.True(dbPostDelete.OpSpecs[groupId1][2].Name == "spec3");
            Assert.True(dbPostDelete.OpSpecRevs.Count == 3);
            Assert.True(dbPostDelete.OpSpecRevs[2] == specId3);
            Assert.True(dbPostDelete.OpSpecRevRefToOpRefs.ContainsKey(specId3));
        }

        [Test]
        public void TestUpdateOpSpecRev()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid groupId1 = Guid.NewGuid();
            Guid specId1 = Guid.NewGuid();
            Guid specId2 = Guid.NewGuid();
            var newSpec = new LibWorkInstructions.Structs.OpSpec
            {
                Id = specId1,
                IdRevGroup = groupId1,
                Name = "spec56",
                Comment = "This is a test"
            };
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                OpSpecs = new Dictionary<Guid, List<LibWorkInstructions.Structs.OpSpec>>
                {
                    { groupId1, new List<LibWorkInstructions.Structs.OpSpec> {
                        new LibWorkInstructions.Structs.OpSpec{Id = specId1, IdRevGroup = groupId1, Name = "spec1", Active = true, },
                        new LibWorkInstructions.Structs.OpSpec{Id = specId2, IdRevGroup = groupId1, Name = "spec2", Active = true} } }
                },
            };
            n.DataImport(sampleData);
            n.UpdateOpSpecRev(newSpec);
            var dbPostUpdate = n.DataExport();
            Assert.True(dbPostUpdate.OpSpecs.Count == 2);
            Assert.True(dbPostUpdate.OpSpecs[groupId1][0].Name == "spec56");
            Assert.True(dbPostUpdate.OpSpecs[groupId1][0].Comment == "This is a test");
        }

        [Test]
        public void TestActivateOpSpecRev()
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
                        new LibWorkInstructions.Structs.OpSpec{Id = specId1, IdRevGroup = groupId1, Name = "spec1", Active = false},
                        new LibWorkInstructions.Structs.OpSpec{Id = specId2, IdRevGroup = groupId1, Name = "spec2", Active = true} } }
                },
            };
            n.DataImport(sampleData);
            n.ActivateOpSpecRev(groupId1, specId1);
            var dbPostActivate = n.DataExport();
            Assert.True(dbPostActivate.OpSpecs[groupId1][0].Active);
        }

        [Test]
        public void TestDeactivateOpSpecRev()
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
                        new LibWorkInstructions.Structs.OpSpec{Id = specId1, IdRevGroup = groupId1, Name = "spec1", Active = true, },
                        new LibWorkInstructions.Structs.OpSpec{Id = specId2, IdRevGroup = groupId1, Name = "spec2", Active = true} } }
                },
            };
            n.DataImport(sampleData);
            n.DeactivateOpSpecRev(groupId1, specId1);
            var dbPostDeactivate = n.DataExport();
            Assert.False(dbPostDeactivate.OpSpecs[groupId1][0].Active);
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
            Assert.True(dbPostMerge.OpRefToOpSpecRevRefs[1].All(y => dbPostMerge.OpRefToOpSpecRevRefs[2].Contains(y)));
            Assert.True(dbPostMerge.OpRefToOpSpecRevRefs[2].All(y => dbPostMerge.OpRefToOpSpecRevRefs[1].Contains(y)));
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
                    { groupId1, new List<LibWorkInstructions.Structs.OpSpec> {
                        new LibWorkInstructions.Structs.OpSpec { Id = specId1, RevSeq = 0 },
                        new LibWorkInstructions.Structs.OpSpec { Id = specId2, RevSeq = 1 },
                        new LibWorkInstructions.Structs.OpSpec { Id = specId3, RevSeq = 2 }
                    } },
                    { groupId2, new List<LibWorkInstructions.Structs.OpSpec> {
                        new LibWorkInstructions.Structs.OpSpec { Id = specId4, RevSeq = 0 },
                        new LibWorkInstructions.Structs.OpSpec { Id = specId5, RevSeq = 1 },
                        new LibWorkInstructions.Structs.OpSpec { Id = specId6, RevSeq = 2 }
                    } },
                    { groupId3, new List<LibWorkInstructions.Structs.OpSpec> {
                        new LibWorkInstructions.Structs.OpSpec { Id = specId7, RevSeq = 0 },
                        new LibWorkInstructions.Structs.OpSpec { Id = specId8, RevSeq = 1 },
                        new LibWorkInstructions.Structs.OpSpec { Id = specId9, RevSeq = 2 }
                    } },
                },
                OpSpecRefToOpSpecRevRefs = new Dictionary<Guid, List<Guid>>
                {
                    { groupId1, new List<Guid> { specId1, specId2, specId3 } },
                    { groupId2, new List<Guid> { specId4, specId5, specId6 } },
                    { groupId3, new List<Guid> { specId7, specId8, specId9 } }
                },
                OpSpecRevRefToOpRefs = new Dictionary<Guid, List<int>>
                {
                    {specId1, new List<int>{ 3, 4, 5 } }
                },
                OpSpecRevs = new List<Guid> { specId1, specId2, specId3, specId4, specId5, specId6, specId7, specId8, specId9 }
            };

            n.DataImport(sampleData);
            n.SplitOpSpecRev(groupId1, specId1);
            var dbPostSplit = n.DataExport();
            Assert.True(dbPostSplit.OpSpecs[groupId1].Count == 4);
            Assert.True(dbPostSplit.OpSpecs[groupId1].Last().Id != specId1);
            Assert.True(dbPostSplit.OpSpecs[groupId1].Last().IdRevGroup == groupId1);
            Assert.True(dbPostSplit.OpSpecs[groupId1].Last().RevSeq == 3);
            Assert.True(dbPostSplit.OpSpecRevs.Count == 10);
            Assert.True(dbPostSplit.OpSpecRefToOpSpecRevRefs[groupId1].Count == 4);
            Assert.True(dbPostSplit.OpSpecRevRefToOpRefs.Last().Value.SequenceEqual(dbPostSplit.OpSpecRevRefToOpRefs[specId1]));
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
                },
                OpSpecRevs = new List<Guid> { specId1, specId2, specId3, specId4 }
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
                },
                OpSpecRevs = new List<Guid> { specId1, specId2, specId3, specId4 }
            };

            n.DataImport(sampleData);
            n.CloneOpSpecRevsBasedOnJobOp(1, 2, false);
            var dbPostClone = n.DataExport();
            Assert.True(dbPostClone.OpRefToOpSpecRevRefs[2].Count == 2);
            Assert.True(dbPostClone.OpRefToOpSpecRevRefs[2].SequenceEqual(dbPostClone.OpRefToOpSpecRevRefs[1]));
        }

        [Test]
        public void TestCreateQualityClause()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            n.CreateQualityClause("Quality clause 1");
            var dbPostAdd = n.DataExport();
            Assert.True(dbPostAdd.QualityClauses.Count == 1);
            Assert.True(dbPostAdd.QualityClauses.First().Value.First().Clause == "Quality clause 1");
        }

        [Test]
        public void TestMergeQualityClauseRevsBasedOnJobRev()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid groupId1 = Guid.NewGuid();
            Guid groupId2 = Guid.NewGuid();
            Guid clause1 = Guid.NewGuid();
            Guid clause2 = Guid.NewGuid();
            Guid clause3 = Guid.NewGuid();
            Guid clause4 = Guid.NewGuid();
            Guid clause5 = Guid.NewGuid();
            Guid clause6 = Guid.NewGuid();
            Guid clause7 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                Jobs = new Dictionary<string, List<LibWorkInstructions.Structs.Job>>
                {
                    {"job1", new List<LibWorkInstructions.Structs.Job>{
                        new LibWorkInstructions.Structs.Job{
                        Id = "job1", Rev = "job1-A", QualityClauses = new List<LibWorkInstructions.Structs.QualityClause> {
                            new LibWorkInstructions.Structs.QualityClause {Id = clause1, IdRevGroup = groupId1}
                        } },
                        new LibWorkInstructions.Structs.Job{
                        Id = "job1", Rev = "job1-B", QualityClauses = new List<LibWorkInstructions.Structs.QualityClause> {
                            new LibWorkInstructions.Structs.QualityClause {Id = clause2, IdRevGroup = groupId1}
                        } },
                        new LibWorkInstructions.Structs.Job{
                        Id = "job1", Rev = "job1-C", QualityClauses = new List<LibWorkInstructions.Structs.QualityClause> {
                            new LibWorkInstructions.Structs.QualityClause {Id = clause3, IdRevGroup = groupId1}
                        } },
                    } },
                    {"job2", new List<LibWorkInstructions.Structs.Job>{
                        new LibWorkInstructions.Structs.Job{
                        Id = "job2", Rev = "job2-A", QualityClauses = new List<LibWorkInstructions.Structs.QualityClause> {
                            new LibWorkInstructions.Structs.QualityClause {Id = clause4, IdRevGroup = groupId2}
                        } },
                        new LibWorkInstructions.Structs.Job{
                        Id = "job2", Rev = "job2-B", QualityClauses = new List<LibWorkInstructions.Structs.QualityClause> {
                            new LibWorkInstructions.Structs.QualityClause {Id = clause5, IdRevGroup = groupId2}
                        } },
                        new LibWorkInstructions.Structs.Job{
                        Id = "job2", Rev = "job2-C", QualityClauses = new List<LibWorkInstructions.Structs.QualityClause> {
                            new LibWorkInstructions.Structs.QualityClause {Id = clause6, IdRevGroup = groupId2},
                            new LibWorkInstructions.Structs.QualityClause {Id = clause7, IdRevGroup = groupId1}
                        } },
                    } },
                },
                QualityClauses = new Dictionary<Guid, List<LibWorkInstructions.Structs.QualityClause>>
                {
                    {groupId1, new List<LibWorkInstructions.Structs.QualityClause>
                    {
                        new LibWorkInstructions.Structs.QualityClause {Id = clause1, IdRevGroup = groupId1},
                        new LibWorkInstructions.Structs.QualityClause {Id = clause2, IdRevGroup = groupId1},
                        new LibWorkInstructions.Structs.QualityClause {Id = clause3, IdRevGroup = groupId1}
                    } },
                    {groupId2, new List<LibWorkInstructions.Structs.QualityClause>
                    {
                        new LibWorkInstructions.Structs.QualityClause {Id = clause4, IdRevGroup = groupId2},
                        new LibWorkInstructions.Structs.QualityClause {Id = clause5, IdRevGroup = groupId2},
                        new LibWorkInstructions.Structs.QualityClause {Id = clause6, IdRevGroup = groupId2}
                    } }
                },
                QualityClauseRevs = new List<Guid> { clause1, clause2, clause3, clause4, clause5, clause6 },
                JobRevRefToQualityClauseRevRefs = new Dictionary<string, List<Guid>>
                {
                    {"job1-A", new List<Guid> {clause1} },
                    {"job1-B", new List<Guid> {clause2} },
                    {"job1-C", new List<Guid> {clause3} },
                    {"job2-A", new List<Guid> {clause4} },
                    {"job2-B", new List<Guid> {clause5} },
                    {"job2-C", new List<Guid> {clause6, clause7} }
                }
            };
            n.DataImport(sampleData);
            n.MergeQualityClauseRevsBasedOnJobRev("job1-A", "job2-C");
            var dbPostMerge = n.DataExport();
            Assert.True(dbPostMerge.Jobs["job1"][0].QualityClauses.Count == 3);
            Assert.True(dbPostMerge.Jobs["job1"][0].QualityClauses.SequenceEqual(dbPostMerge.Jobs["job2"][3].QualityClauses));
            Assert.True(dbPostMerge.JobRevRefToQualityClauseRevRefs["job1-A"].SequenceEqual(dbPostMerge.JobRevRefToQualityClauseRevRefs["job2-C"]));
        }

        [Test]
        public void TestSplitQualityClauseRev()
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
                },
                QualityClauseRevs = new List<Guid> { clauseId1, clauseId2 },
                QualityClauseRefToQualityClauseRevRefs = new Dictionary<Guid, List<Guid>>
                {
                    {groupId1, new List<Guid> { clauseId1 } },
                    {groupId2, new List<Guid> { clauseId2 } }
                }
            };
            n.DataImport(sampleData);
            n.SplitQualityClauseRev(groupId1, clauseId1);
            var dbPostSplit = n.DataExport();
            Assert.True(dbPostSplit.QualityClauses[groupId1].Count == 2);
            Assert.True(dbPostSplit.QualityClauses[groupId1].Last().Id != null);
            Assert.True(dbPostSplit.QualityClauses[groupId1].Last().IdRevGroup == groupId1);
            Assert.True(dbPostSplit.QualityClauseRefToQualityClauseRevRefs[groupId1].Count == 2);
        }

        [Test]
        public void TestCloneQualityClauseRevsBasedOnJobRevAdditive()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid clauseId1 = Guid.NewGuid();
            Guid clauseId2 = Guid.NewGuid();
            Guid clauseId3 = Guid.NewGuid();
            Guid clauseId4 = Guid.NewGuid();
            Guid clauseId5 = Guid.NewGuid();
            Guid clauseId6 = Guid.NewGuid();
            Guid clauseId7 = Guid.NewGuid();
            Guid clauseId8 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                Jobs = new Dictionary<string, List<LibWorkInstructions.Structs.Job>>
                {
                    {"job1", new List<LibWorkInstructions.Structs.Job>{
                        new LibWorkInstructions.Structs.Job {Rev = "job1-A" , QualityClauses = new List<LibWorkInstructions.Structs.QualityClause> {
                        new LibWorkInstructions.Structs.QualityClause {Id = clauseId1},
                        new LibWorkInstructions.Structs.QualityClause {Id = clauseId2}}, },
                        new LibWorkInstructions.Structs.Job {Rev = "job1-B" , QualityClauses = new List<LibWorkInstructions.Structs.QualityClause> {
                        new LibWorkInstructions.Structs.QualityClause {Id = clauseId3},
                        new LibWorkInstructions.Structs.QualityClause {Id = clauseId4}}, } } },
                    {"job2", new List<LibWorkInstructions.Structs.Job>{
                        new LibWorkInstructions.Structs.Job {Rev = "job2-A" , QualityClauses = new List<LibWorkInstructions.Structs.QualityClause> {
                        new LibWorkInstructions.Structs.QualityClause {Id = clauseId5},
                        new LibWorkInstructions.Structs.QualityClause {Id = clauseId6}}, },
                        new LibWorkInstructions.Structs.Job {Rev = "job2-B" , QualityClauses = new List<LibWorkInstructions.Structs.QualityClause> {
                        new LibWorkInstructions.Structs.QualityClause {Id = clauseId7},
                        new LibWorkInstructions.Structs.QualityClause {Id = clauseId8}}, } } },
                },
                JobRevs = new List<string> { "job1-A", "job1-B", "job2-A", "job2-B" },
                JobRevRefToQualityClauseRevRefs = new Dictionary<string, List<Guid>>
                {
                    {"job1-A", new List<Guid> {clauseId1, clauseId2} },
                    {"job1-B", new List<Guid> {clauseId3, clauseId4} },
                    {"job2-A", new List<Guid> {clauseId5, clauseId6} },
                    {"job2-B", new List<Guid> {clauseId7, clauseId8} },
                }
            };
            n.DataImport(sampleData);
            n.CloneQualityClauseRevsBasedOnJobRev("job1-A", "job2-B", true);
            var dbPostClone = n.DataExport();
            Assert.True(dbPostClone.Jobs["job2"][1].QualityClauses.Count == 4);
            Assert.True(dbPostClone.Jobs["job1"].All(y => dbPostClone.Jobs["job2"].Contains(y)));
        }

        [Test]
        public void TestCloneQualityClauseRevsBasedOnJobRevNotAdditive()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid clauseId1 = Guid.NewGuid();
            Guid clauseId2 = Guid.NewGuid();
            Guid clauseId3 = Guid.NewGuid();
            Guid clauseId4 = Guid.NewGuid();
            Guid clauseId5 = Guid.NewGuid();
            Guid clauseId6 = Guid.NewGuid();
            Guid clauseId7 = Guid.NewGuid();
            Guid clauseId8 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                Jobs = new Dictionary<string, List<LibWorkInstructions.Structs.Job>>
                {
                    {"job1", new List<LibWorkInstructions.Structs.Job>{
                        new LibWorkInstructions.Structs.Job {Rev = "job1-A" , QualityClauses = new List<LibWorkInstructions.Structs.QualityClause> {
                        new LibWorkInstructions.Structs.QualityClause {Id = clauseId1},
                        new LibWorkInstructions.Structs.QualityClause {Id = clauseId2}}, },
                        new LibWorkInstructions.Structs.Job {Rev = "job1-B" , QualityClauses = new List<LibWorkInstructions.Structs.QualityClause> {
                        new LibWorkInstructions.Structs.QualityClause {Id = clauseId3},
                        new LibWorkInstructions.Structs.QualityClause {Id = clauseId4}}, } } },
                    {"job2", new List<LibWorkInstructions.Structs.Job>{
                        new LibWorkInstructions.Structs.Job {Rev = "job2-A" , QualityClauses = new List<LibWorkInstructions.Structs.QualityClause> {
                        new LibWorkInstructions.Structs.QualityClause {Id = clauseId5},
                        new LibWorkInstructions.Structs.QualityClause {Id = clauseId6}}, },
                        new LibWorkInstructions.Structs.Job {Rev = "job2-B" , QualityClauses = new List<LibWorkInstructions.Structs.QualityClause> {
                        new LibWorkInstructions.Structs.QualityClause {Id = clauseId7},
                        new LibWorkInstructions.Structs.QualityClause {Id = clauseId8}}, } } },
                },
                JobRevs = new List<string> { "job1-A", "job1-B", "job2-A", "job2-B"},
                JobRevRefToQualityClauseRevRefs = new Dictionary<string, List<Guid>>
                {
                    {"job1-A", new List<Guid> {clauseId1, clauseId2} },
                    {"job1-B", new List<Guid> {clauseId3, clauseId4} },
                    {"job2-A", new List<Guid> {clauseId5, clauseId6} },
                    {"job2-B", new List<Guid> {clauseId7, clauseId8} },
                }
            };
            n.DataImport(sampleData);
            n.CloneQualityClauseRevsBasedOnJobRev("job1-A", "job2-B", false);
            var dbPostClone = n.DataExport();
            Assert.True(dbPostClone.Jobs["job2"][1].QualityClauses.SequenceEqual(dbPostClone.Jobs["job1"][0].QualityClauses));
            Assert.True(dbPostClone.Jobs["job2"][1].QualityClauses.Count == 2);
        }

        [Test]
        public void TestActivateQualityClause()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid groupId1 = Guid.NewGuid();
            Guid groupId2 = Guid.NewGuid();
            Guid clause1 = Guid.NewGuid();
            Guid clause2 = Guid.NewGuid();
            Guid clause3 = Guid.NewGuid();
            Guid clause4 = Guid.NewGuid();
            Guid clause5 = Guid.NewGuid();
            Guid clause6 = Guid.NewGuid();
            var sampleClause1 = new LibWorkInstructions.Structs.QualityClause
            {
                Id = clause1,
                IdRevGroup = groupId1,
                Clause = "Test",
                Active = true
            };
            var sampleClause2 = new LibWorkInstructions.Structs.QualityClause
            {
                Id = clause2,
                IdRevGroup = groupId1,
                Clause = "Test",
                Active = true
            };
            var sampleClause3 = new LibWorkInstructions.Structs.QualityClause
            {
                Id = clause3,
                IdRevGroup = groupId1,
                Clause = "Test",
                Active = true
            };
            var sampleClause4 = new LibWorkInstructions.Structs.QualityClause
            {
                Id = clause4,
                IdRevGroup = groupId2,
                Clause = "Test",
                Active = true
            };
            var sampleClause5 = new LibWorkInstructions.Structs.QualityClause
            {
                Id = clause5,
                IdRevGroup = groupId2,
                Clause = "Test",
                Active = true
            };
            var sampleClause6 = new LibWorkInstructions.Structs.QualityClause
            {
                Id = clause6,
                IdRevGroup = groupId2,
                Clause = "Test",
                Active = true
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
                    {groupId1, new List<LibWorkInstructions.Structs.QualityClause>{sampleClause1, sampleClause4, sampleClause3} },
                    {groupId2, new List<LibWorkInstructions.Structs.QualityClause>{sampleClause2, sampleClause5, sampleClause6} },
                }
            };
            n.DataImport(sampleData);
            n.ActivateQualityClause(groupId1);
            var dbVar = n.DataExport();
            Assert.True(dbVar.QualityClauses[groupId1].All(y => y.Active));
        }

        [Test]
        public void TestDeactivateQualityClause()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid groupId1 = Guid.NewGuid();
            Guid groupId2 = Guid.NewGuid();
            Guid clause1 = Guid.NewGuid();
            Guid clause2 = Guid.NewGuid();
            Guid clause3 = Guid.NewGuid();
            Guid clause4 = Guid.NewGuid();
            Guid clause5 = Guid.NewGuid();
            Guid clause6 = Guid.NewGuid();
            var sampleClause1 = new LibWorkInstructions.Structs.QualityClause
            {
                Id = clause1,
                IdRevGroup = groupId1,
                Clause = "Test",
                Active = true
            };
            var sampleClause2 = new LibWorkInstructions.Structs.QualityClause
            {
                Id = clause2,
                IdRevGroup = groupId1,
                Clause = "Test",
                Active = true
            };
            var sampleClause3 = new LibWorkInstructions.Structs.QualityClause
            {
                Id = clause3,
                IdRevGroup = groupId1,
                Clause = "Test",
                Active = true
            };
            var sampleClause4 = new LibWorkInstructions.Structs.QualityClause
            {
                Id = clause4,
                IdRevGroup = groupId2,
                Clause = "Test",
                Active = true
            };
            var sampleClause5 = new LibWorkInstructions.Structs.QualityClause
            {
                Id = clause5,
                IdRevGroup = groupId2,
                Clause = "Test",
                Active = true
            };
            var sampleClause6 = new LibWorkInstructions.Structs.QualityClause
            {
                Id = clause6,
                IdRevGroup = groupId2,
                Clause = "Test",
                Active = true
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
                    {groupId1, new List<LibWorkInstructions.Structs.QualityClause>{sampleClause1, sampleClause4, sampleClause3} },
                    {groupId2, new List<LibWorkInstructions.Structs.QualityClause>{sampleClause2, sampleClause5, sampleClause6} },
                }
            };
            n.DataImport(sampleData);
            n.DeactivateQualityClause(groupId1);
            var dbVar = n.DataExport();
            Assert.True(dbVar.QualityClauses[groupId1].All(y => !y.Active));
        }

        [Test]
        public void TestCreateQualityClauseRev()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid groupId1 = Guid.NewGuid();
            Guid groupId2 = Guid.NewGuid();
            Guid clause1 = Guid.NewGuid();
            Guid clause2 = Guid.NewGuid();
            Guid clause3 = Guid.NewGuid();
            Guid clause4 = Guid.NewGuid();
            Guid clause5 = Guid.NewGuid();
            Guid clause6 = Guid.NewGuid();
            var sampleClause1 = new LibWorkInstructions.Structs.QualityClause
            {
                Id = clause1,
                IdRevGroup = groupId1,
                Clause = "Test",
                Active = true
            };
            var sampleClause2 = new LibWorkInstructions.Structs.QualityClause
            {
                Id = clause2,
                IdRevGroup = groupId1,
                Clause = "Test",
                Active = true
            };
            var sampleClause3 = new LibWorkInstructions.Structs.QualityClause
            {
                Id = clause3,
                IdRevGroup = groupId1,
                Clause = "Test",
                Active = true
            };
            var sampleClause4 = new LibWorkInstructions.Structs.QualityClause
            {
                Id = clause4,
                IdRevGroup = groupId2,
                Clause = "Test",
                Active = true
            };
            var sampleClause5 = new LibWorkInstructions.Structs.QualityClause
            {
                Id = clause5,
                IdRevGroup = groupId2,
                Clause = "Test",
                Active = true
            };
            var sampleClause6 = new LibWorkInstructions.Structs.QualityClause
            {
                Id = clause6,
                IdRevGroup = groupId2,
                Clause = "Test",
                Active = true
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
                    {groupId1, new List<LibWorkInstructions.Structs.QualityClause>{sampleClause1, sampleClause4, sampleClause3} },
                    {groupId2, new List<LibWorkInstructions.Structs.QualityClause>{sampleClause2, sampleClause5, sampleClause6} },
                }
            };
            n.DataImport(sampleData);
            n.CreateQualityClauseRev(groupId1, clause2);
            var dbPostCreate = n.DataExport();
            Assert.True(dbPostCreate.QualityClauses[groupId1].Count == 4);
        }

        [Test]
        public void TestCreateQualityClauseRevFromScratch()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid groupId1 = Guid.NewGuid();
            Guid groupId2 = Guid.NewGuid();
            Guid clause1 = Guid.NewGuid();
            Guid clause2 = Guid.NewGuid();
            Guid clause3 = Guid.NewGuid();
            Guid clause4 = Guid.NewGuid();
            Guid clause5 = Guid.NewGuid();
            Guid clause6 = Guid.NewGuid();
            Guid clause7 = Guid.NewGuid();
            var sampleClause1 = new LibWorkInstructions.Structs.QualityClause
            {
                Id = clause1,
                IdRevGroup = groupId1,
                Clause = "Test",
                Active = true
            };
            var sampleClause2 = new LibWorkInstructions.Structs.QualityClause
            {
                Id = clause2,
                IdRevGroup = groupId1,
                Clause = "Test",
                Active = true
            };
            var sampleClause3 = new LibWorkInstructions.Structs.QualityClause
            {
                Id = clause3,
                IdRevGroup = groupId1,
                Clause = "Test",
                Active = true
            };
            var sampleClause4 = new LibWorkInstructions.Structs.QualityClause
            {
                Id = clause4,
                IdRevGroup = groupId2,
                Clause = "Test",
                Active = true
            };
            var sampleClause5 = new LibWorkInstructions.Structs.QualityClause
            {
                Id = clause5,
                IdRevGroup = groupId2,
                Clause = "Test",
                Active = true
            };
            var sampleClause6 = new LibWorkInstructions.Structs.QualityClause
            {
                Id = clause6,
                IdRevGroup = groupId2,
                Clause = "Test",
                Active = true
            };
            var sampleClause7 = new LibWorkInstructions.Structs.QualityClause
            {
                Id = clause7,
                IdRevGroup = groupId2,
                Clause = "Test",
                Active = true
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
                    {groupId1, new List<LibWorkInstructions.Structs.QualityClause>{sampleClause1, sampleClause4, sampleClause3} },
                    {groupId2, new List<LibWorkInstructions.Structs.QualityClause>{sampleClause2, sampleClause5, sampleClause6} },
                }
            };
            n.DataImport(sampleData);
            n.CreateQualityClauseRev(sampleClause7);
            var dbPostCreate = n.DataExport();
            Assert.True(dbPostCreate.QualityClauses[groupId2].Count == 4);
        }

        [Test]
        public void TestUpdateQualityClauseRev()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid groupId1 = Guid.NewGuid();
            Guid groupId2 = Guid.NewGuid();
            Guid clause1 = Guid.NewGuid();
            Guid clause2 = Guid.NewGuid();
            Guid clause3 = Guid.NewGuid();
            Guid clause4 = Guid.NewGuid();
            Guid clause5 = Guid.NewGuid();
            Guid clause6 = Guid.NewGuid();
            var sampleClause1 = new LibWorkInstructions.Structs.QualityClause
            {
                Id = clause1,
                IdRevGroup = groupId1,
                Clause = "Test",
                Active = true
            };
            var sampleClause2 = new LibWorkInstructions.Structs.QualityClause
            {
                Id = clause2,
                IdRevGroup = groupId1,
                Clause = "Test",
                Active = true
            };
            var sampleClause3 = new LibWorkInstructions.Structs.QualityClause
            {
                Id = clause3,
                IdRevGroup = groupId1,
                Clause = "Test",
                Active = true
            };
            var sampleClause4 = new LibWorkInstructions.Structs.QualityClause
            {
                Id = clause4,
                IdRevGroup = groupId2,
                Clause = "Test",
                Active = true
            };
            var sampleClause5 = new LibWorkInstructions.Structs.QualityClause
            {
                Id = clause5,
                IdRevGroup = groupId2,
                Clause = "Test",
                Active = true
            };
            var sampleClause6 = new LibWorkInstructions.Structs.QualityClause
            {
                Id = clause6,
                IdRevGroup = groupId2,
                Clause = "Test",
                Active = true
            };
            var sampleClause7 = new LibWorkInstructions.Structs.QualityClause
            {
                Id = clause6,
                IdRevGroup = groupId2,
                Clause = "TestChanged",
                Active = true
            };
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                QualityClauses = new Dictionary<Guid, List<LibWorkInstructions.Structs.QualityClause>>
                {
                    {groupId1, new List<LibWorkInstructions.Structs.QualityClause>{sampleClause1, sampleClause4, sampleClause3} },
                    {groupId2, new List<LibWorkInstructions.Structs.QualityClause>{sampleClause2, sampleClause5, sampleClause6} },
                }
            };
            n.DataImport(sampleData);
            n.UpdateQualityClauseRev(sampleClause7);
            var dbPostUpdate = n.DataExport();
            Assert.True(dbPostUpdate.QualityClauses[groupId2][2].Id == sampleClause7.Id);
            Assert.True(dbPostUpdate.QualityClauses[groupId2][2].IdRevGroup == sampleClause7.IdRevGroup);
            Assert.True(dbPostUpdate.QualityClauses[groupId2][2].Clause == sampleClause7.Clause);
            Assert.True(dbPostUpdate.QualityClauses[groupId2][2].Active == sampleClause7.Active);
        }

        [Test]
        public void TestActivateQualityClauseRev()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid groupId1 = Guid.NewGuid();
            Guid groupId2 = Guid.NewGuid();
            Guid clause1 = Guid.NewGuid();
            Guid clause2 = Guid.NewGuid();
            Guid clause3 = Guid.NewGuid();
            Guid clause4 = Guid.NewGuid();
            Guid clause5 = Guid.NewGuid();
            Guid clause6 = Guid.NewGuid();
            var sampleClause1 = new LibWorkInstructions.Structs.QualityClause
            {
                Id = clause1,
                IdRevGroup = groupId1,
                Clause = "Test",
                Active = true
            };
            var sampleClause2 = new LibWorkInstructions.Structs.QualityClause
            {
                Id = clause2,
                IdRevGroup = groupId1,
                Clause = "Test",
                Active = false
            };
            var sampleClause3 = new LibWorkInstructions.Structs.QualityClause
            {
                Id = clause3,
                IdRevGroup = groupId1,
                Clause = "Test",
                Active = true
            };
            var sampleClause4 = new LibWorkInstructions.Structs.QualityClause
            {
                Id = clause4,
                IdRevGroup = groupId2,
                Clause = "Test",
                Active = true
            };
            var sampleClause5 = new LibWorkInstructions.Structs.QualityClause
            {
                Id = clause5,
                IdRevGroup = groupId2,
                Clause = "Test",
                Active = true
            };
            var sampleClause6 = new LibWorkInstructions.Structs.QualityClause
            {
                Id = clause6,
                IdRevGroup = groupId2,
                Clause = "Test",
                Active = true
            };
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                QualityClauses = new Dictionary<Guid, List<LibWorkInstructions.Structs.QualityClause>>
                {
                    {groupId1, new List<LibWorkInstructions.Structs.QualityClause>{sampleClause1, sampleClause4, sampleClause3} },
                    {groupId2, new List<LibWorkInstructions.Structs.QualityClause>{sampleClause2, sampleClause5, sampleClause6} },
                }
            };
            n.DataImport(sampleData);
            n.ActivateQualityClauseRev(groupId2, clause2);
            var dbPostDeactivate = n.DataExport();
            Assert.True(dbPostDeactivate.QualityClauses[groupId2][0].Active);
        }

        [Test]
        public void TestDeactivateQualityClauseRev()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid groupId1 = Guid.NewGuid();
            Guid groupId2 = Guid.NewGuid();
            Guid clause1 = Guid.NewGuid();
            Guid clause2 = Guid.NewGuid();
            Guid clause3 = Guid.NewGuid();
            Guid clause4 = Guid.NewGuid();
            Guid clause5 = Guid.NewGuid();
            Guid clause6 = Guid.NewGuid();
            var sampleClause1 = new LibWorkInstructions.Structs.QualityClause
            {
                Id = clause1,
                IdRevGroup = groupId1,
                Clause = "Test",
                Active = true
            };
            var sampleClause2 = new LibWorkInstructions.Structs.QualityClause
            {
                Id = clause2,
                IdRevGroup = groupId1,
                Clause = "Test",
                Active = true
            };
            var sampleClause3 = new LibWorkInstructions.Structs.QualityClause
            {
                Id = clause3,
                IdRevGroup = groupId1,
                Clause = "Test",
                Active = true
            };
            var sampleClause4 = new LibWorkInstructions.Structs.QualityClause
            {
                Id = clause4,
                IdRevGroup = groupId2,
                Clause = "Test",
                Active = true
            };
            var sampleClause5 = new LibWorkInstructions.Structs.QualityClause
            {
                Id = clause5,
                IdRevGroup = groupId2,
                Clause = "Test",
                Active = true
            };
            var sampleClause6 = new LibWorkInstructions.Structs.QualityClause
            {
                Id = clause6,
                IdRevGroup = groupId2,
                Clause = "Test",
                Active = true
            };
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                QualityClauses = new Dictionary<Guid, List<LibWorkInstructions.Structs.QualityClause>>
                {
                    {groupId1, new List<LibWorkInstructions.Structs.QualityClause>{sampleClause1, sampleClause4, sampleClause3} },
                    {groupId2, new List<LibWorkInstructions.Structs.QualityClause>{sampleClause2, sampleClause5, sampleClause6} },
                }
            };
            n.DataImport(sampleData);
            n.DeactivateQualityClauseRev(groupId2, clause2);
            var dbPostDeactivate = n.DataExport();
            Assert.False(dbPostDeactivate.QualityClauses[groupId2][0].Active);
        }

        [Test]
        public void TestCreateJobOp()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            var op = new LibWorkInstructions.Structs.Op
            {
                Id = 4,
                JobId = "Job1",
                OpService = "1.0.1",
                Seq = 0
            };
            n.CreateJobOp(op);
            var dbVar = n.DataExport();
            Assert.True(dbVar.Ops.Count == 1);
            Assert.True(dbVar.Ops[4].OpService == "1.0.1");
            Assert.True(dbVar.Ops[4].JobId == "Job1");
            Assert.True(dbVar.Ops[4].Seq == 0);
            Assert.True(dbVar.OpRefToOpSpecRevRefs.ContainsKey(4));
        }

        [Test]
        public void TestDeleteJobOp()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                Jobs = new Dictionary<string, List<LibWorkInstructions.Structs.Job>>
                {
                    {"job1", new List<LibWorkInstructions.Structs.Job> {
                        new LibWorkInstructions.Structs.Job {Id = "job1", Rev = "jobRev1", Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op {Id = 1},
                            new LibWorkInstructions.Structs.Op {Id = 3}
                        } },
                        new LibWorkInstructions.Structs.Job {Id = "job1", Rev = "jobRev2", Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op {Id = 2},
                            new LibWorkInstructions.Structs.Op {Id = 3}
                        } },
                     } }
                },
                JobRevRefToOpRefs = new Dictionary<string, List<int>>
                {
                    {"jobRev1", new List<int>{1, 3}},
                    {"jobRev2", new List<int>{2, 3}}
                }
            };
            n.DataImport(sampleData);
            n.DeleteJobOp("jobRev2", 3);
            var dbPostDelete = n.DataExport();
            Assert.True(dbPostDelete.Jobs["job1"][1].Ops.Count == 1);
            Assert.True(dbPostDelete.Jobs["job1"][1].Ops[0].Id == 2);
            Assert.True(dbPostDelete.JobRevRefToOpRefs["jobRev2"].Count == 1);
            Assert.True(dbPostDelete.JobRevRefToOpRefs["jobRev1"].Count == 2);
        }
    }
}
