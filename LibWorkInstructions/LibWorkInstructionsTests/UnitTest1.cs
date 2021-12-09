using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text.Json;
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
        public void TestCreateJob()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid jobRev = Guid.NewGuid();
            n.CreateJob("F110", "CUSTX", "1.0.0", jobRev);
            var dbVar = n.DataExport();
            Assert.True(dbVar.Jobs["F110"][0].Id.Equals("F110"));
            Assert.True(dbVar.Jobs["F110"][0].Rev == jobRev);
            Assert.True(dbVar.Jobs["F110"][0].RevCustomer.Equals("CUSTX"));
            Assert.True(dbVar.Jobs["F110"][0].RevPlan.Equals("1.0.0"));
            Assert.True(dbVar.Jobs["F110"][0].RevSeq == 0);
            Assert.True(dbVar.JobRefToJobRevRefs.ContainsKey("F110"));
            Assert.True(dbVar.JobRefToJobRevRefs["F110"].Contains(jobRev));
        }

        [Test]
        public void TestDeleteJob()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid clauseRev1 = Guid.NewGuid();
            Guid clauseRev2 = Guid.NewGuid();
            Guid jobRev1 = Guid.NewGuid();
            Guid jobRev2 = Guid.NewGuid();
            Guid jobRev3 = Guid.NewGuid();
            Guid jobRev4 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                Jobs = new Dictionary<string, List<LibWorkInstructions.Structs.Job>>
                {
                    {"job1", new List<LibWorkInstructions.Structs.Job>
                    {
                        new LibWorkInstructions.Structs.Job {Id = "job1", Rev = jobRev1, Ops = new List<LibWorkInstructions.Structs.Op> { 
                            new LibWorkInstructions.Structs.Op { Id = 1, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 2, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 3, JobId = "job1"},
                        } },
                        new LibWorkInstructions.Structs.Job {Id = "job1", Rev = jobRev2, Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 4, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 5, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 6, JobId = "job1"},
                        } }
                    } 
                    },
                    {"job2", new List<LibWorkInstructions.Structs.Job>
                    {
                        new LibWorkInstructions.Structs.Job {Id = "job1", Rev = jobRev3, Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 1, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 2, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 3, JobId = "job1"},
                        } },
                        new LibWorkInstructions.Structs.Job {Id = "job1", Rev = jobRev4, Ops = new List<LibWorkInstructions.Structs.Op> {
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
                JobRefToJobRevRefs = new Dictionary<string, List<Guid>>
                {
                    {"job1", new List<Guid>{jobRev1, jobRev2} }
                },
                JobRevRefToOpRefs = new Dictionary<Guid, List<int>>
                {
                    {jobRev1, new List<int>{1, 2, 3}},
                    {jobRev2, new List<int>{4, 5, 6}}
                },
                QualityClauseRevRefToJobRevRefs = new Dictionary<Guid, List<Guid>>
                {
                    {clauseRev1, new List<Guid> {jobRev1, jobRev4} },
                    {clauseRev2, new List<Guid> {jobRev2, jobRev3} }
                }
            };
            n.DataImport(sampleData);
            n.DeleteJob("job1");
            var dbPostDelete = n.DataExport();
            Assert.True(dbPostDelete.Jobs.Count == 1);
            Assert.True(dbPostDelete.JobRefToJobRevRefs.Count == 0);
            Assert.True(dbPostDelete.JobRevRefToOpRefs.Count == 0);
            Assert.True(dbPostDelete.QualityClauseRevRefToJobRevRefs[clauseRev1].Count == 1);
            Assert.True(dbPostDelete.QualityClauseRevRefToJobRevRefs[clauseRev1][0] == jobRev4);
            Assert.True(dbPostDelete.QualityClauseRevRefToJobRevRefs[clauseRev2].Count == 1);
            Assert.True(dbPostDelete.QualityClauseRevRefToJobRevRefs[clauseRev2][0] == jobRev3);
        }

        [Test]
        public void TestCreateJobRev()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid jobRev1 = Guid.NewGuid();
            Guid jobRev2 = Guid.NewGuid();
            Guid jobRev3 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                Jobs = new Dictionary<string, List<LibWorkInstructions.Structs.Job>>
                {
                    {"job1", new List<LibWorkInstructions.Structs.Job>
                    {
                        new LibWorkInstructions.Structs.Job {Id = "job1", Rev = jobRev1, Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 1, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 2, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 3, JobId = "job1"},
                        } },
                        new LibWorkInstructions.Structs.Job {Id = "job1", Rev = jobRev2, Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 4, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 5, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 6, JobId = "job1"},
                        } }
                    }
                    }
                },
                JobRevs = new List<Guid> { jobRev1, jobRev2 },
                JobRevRefToQualityClauseRevRefs = new Dictionary<Guid, List<Guid>>
                {
                    {jobRev1, new List<Guid>() },
                    {jobRev2, new List<Guid>() }
                },
                JobRevRefToOpRefs = new Dictionary<Guid, List<int>>
                {
                    {jobRev1, new List<int>{1, 2, 3} },
                    {jobRev2, new List<int>{4, 5, 6} }
                },
                JobRefToJobRevRefs = new Dictionary<string, List<Guid>>
                {
                    {"job1", new List<Guid>{jobRev1, jobRev2} }
                }
            };
            n.DataImport(sampleData);
            n.CreateJobRev("job1", jobRev1, jobRev3);
            var dbPostCreate = n.DataExport();
            Assert.True(dbPostCreate.Jobs["job1"].Count == 3);
            Assert.True(dbPostCreate.Jobs["job1"].Last().Rev == jobRev3);
            Assert.True(dbPostCreate.Jobs["job1"].Last().RevSeq == 2);
            Assert.False(dbPostCreate.Jobs["job1"][0].Rev == dbPostCreate.Jobs["job1"][2].Rev);
        }

        [Test]
        public void TestCreateJobRevFromScratch()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid jobRev1 = Guid.NewGuid();
            Guid jobRev2 = Guid.NewGuid();
            Guid jobRev3 = Guid.NewGuid();
            Guid clauseId1 = Guid.NewGuid();
            Guid clauseId2 = Guid.NewGuid();
            Guid clauseId3 = Guid.NewGuid();
            Guid clauseId4 = Guid.NewGuid();
            Guid clauseId5 = Guid.NewGuid();
            Guid clauseId6 = Guid.NewGuid();
            Guid clauseId7 = Guid.NewGuid();
            Guid clauseId8 = Guid.NewGuid();
            Guid clauseId9 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                Jobs = new Dictionary<string, List<LibWorkInstructions.Structs.Job>>
                {
                    {"job1", new List<LibWorkInstructions.Structs.Job>
                    {
                        new LibWorkInstructions.Structs.Job {Id = "job1", Rev = jobRev1, Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 1, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 2, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 3, JobId = "job1"},
                        }, QualityClauses = new List<LibWorkInstructions.Structs.QualityClause>{ 
                                new LibWorkInstructions.Structs.QualityClause { Id = clauseId1},
                                new LibWorkInstructions.Structs.QualityClause { Id = clauseId2},
                                new LibWorkInstructions.Structs.QualityClause { Id = clauseId3}} },
                        new LibWorkInstructions.Structs.Job {Id = "job1", Rev = jobRev2, Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 4, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 5, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 6, JobId = "job1"},
                        }, QualityClauses = new List<LibWorkInstructions.Structs.QualityClause>{
                                new LibWorkInstructions.Structs.QualityClause { Id = clauseId4},
                                new LibWorkInstructions.Structs.QualityClause { Id = clauseId5},
                                new LibWorkInstructions.Structs.QualityClause { Id = clauseId6}} }
                    }
                    }
                },
                JobRevs = new List<Guid> { jobRev1, jobRev2 },
                JobRefToJobRevRefs = new Dictionary<string, List<Guid>>
                {
                    {"job1", new List<Guid>{jobRev1, jobRev2} }
                },
                JobRevRefToOpRefs = new Dictionary<Guid, List<int>>
                {
                    {jobRev1, new List<int>{1, 2, 3} },
                    {jobRev2, new List<int>{4, 5, 6} }
                }
            };
            n.DataImport(sampleData);
            n.CreateJobRev(new LibWorkInstructions.Structs.Job
            {
                Id = "job1",
                Rev = jobRev3,
                Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 7, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 8, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 9, JobId = "job1"}
                },
                QualityClauses = new List<LibWorkInstructions.Structs.QualityClause>{
                                new LibWorkInstructions.Structs.QualityClause { Id = clauseId7},
                                new LibWorkInstructions.Structs.QualityClause { Id = clauseId8},
                                new LibWorkInstructions.Structs.QualityClause { Id = clauseId9}}
            });
            var dbPostCreate = n.DataExport();
            Assert.True(dbPostCreate.Jobs["job1"].Count == 3);
            Assert.True(dbPostCreate.Jobs["job1"][2].Rev == jobRev3);
            Assert.True(dbPostCreate.Jobs["job1"][2].RevSeq == 2);
            Assert.True(dbPostCreate.Jobs["job1"][2].Ops.All(y => dbPostCreate.JobRevRefToOpRefs.Count(x => x.Value.Contains(y.Id)) == 1));
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
            Guid jobRev1 = Guid.NewGuid();
            Guid jobRev2 = Guid.NewGuid();
            Guid jobRev3 = Guid.NewGuid();
            Guid jobRev4 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                Jobs = new Dictionary<string, List<LibWorkInstructions.Structs.Job>>
                {
                    {"job1", new List<LibWorkInstructions.Structs.Job>{
                        new LibWorkInstructions.Structs.Job {RevSeq = 0, Id = "job1", Rev = jobRev1 , QualityClauses = new List<LibWorkInstructions.Structs.QualityClause> {
                            new LibWorkInstructions.Structs.QualityClause {Id = clauseId1},
                            new LibWorkInstructions.Structs.QualityClause {Id = clauseId2}}, Ops = new List<LibWorkInstructions.Structs.Op> {
                                    new LibWorkInstructions.Structs.Op { Id = 1, JobId = "job1"},
                                    new LibWorkInstructions.Structs.Op { Id = 2, JobId = "job1"},
                                    new LibWorkInstructions.Structs.Op { Id = 3, JobId = "job1"},}},
                        new LibWorkInstructions.Structs.Job {RevSeq = 1, Id = "job1", Rev = jobRev2 , QualityClauses = new List<LibWorkInstructions.Structs.QualityClause> {
                            new LibWorkInstructions.Structs.QualityClause {Id = clauseId3},
                            new LibWorkInstructions.Structs.QualityClause {Id = clauseId4}}, Ops = new List<LibWorkInstructions.Structs.Op> {
                                    new LibWorkInstructions.Structs.Op { Id = 4, JobId = "job1"},
                                    new LibWorkInstructions.Structs.Op { Id = 5, JobId = "job1"},
                                    new LibWorkInstructions.Structs.Op { Id = 6, JobId = "job1"},} } } },
                    {"job2", new List<LibWorkInstructions.Structs.Job>{
                        new LibWorkInstructions.Structs.Job {RevSeq = 0, Id = "job2", Rev = jobRev3 , QualityClauses = new List<LibWorkInstructions.Structs.QualityClause> {
                        new LibWorkInstructions.Structs.QualityClause {Id = clauseId5},
                        new LibWorkInstructions.Structs.QualityClause {Id = clauseId6}}, Ops = new List<LibWorkInstructions.Structs.Op> {
                                    new LibWorkInstructions.Structs.Op { Id = 7, JobId = "job2"},
                                    new LibWorkInstructions.Structs.Op { Id = 8, JobId = "job2"},
                                    new LibWorkInstructions.Structs.Op { Id = 9, JobId = "job2"},}},
                        new LibWorkInstructions.Structs.Job {RevSeq = 1, Id = "job2", Rev = jobRev4 , QualityClauses = new List<LibWorkInstructions.Structs.QualityClause> {
                        new LibWorkInstructions.Structs.QualityClause {Id = clauseId7},
                        new LibWorkInstructions.Structs.QualityClause {Id = clauseId8}}, Ops = new List<LibWorkInstructions.Structs.Op> {
                                    new LibWorkInstructions.Structs.Op { Id = 10, JobId = "job2"},
                                    new LibWorkInstructions.Structs.Op { Id = 11, JobId = "job2"},
                                    new LibWorkInstructions.Structs.Op { Id = 12, JobId = "job2"},}} } },
                },
                JobRevs = new List<Guid> {jobRev1, jobRev2, jobRev3, jobRev4 },
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
                Rev = jobRev2,
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
            Guid jobRev1 = Guid.NewGuid();
            Guid jobRev2 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                Jobs = new Dictionary<string, List<LibWorkInstructions.Structs.Job>>
                {
                    {"job1", new List<LibWorkInstructions.Structs.Job>
                    {
                        new LibWorkInstructions.Structs.Job {Id = "job1", Rev = jobRev1, Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 1, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 2, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 3, JobId = "job1"},
                        }, Active = false },
                        new LibWorkInstructions.Structs.Job {Id = "job1", Rev = jobRev2, Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 4, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 5, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 6, JobId = "job1"},
                        }, Active = false }
                    }
                    }
                },
            };
            n.DataImport(sampleData);
            n.ActivateJobRev("job1", jobRev2);
            var dbPostDeactivate = n.DataExport();
            Assert.True(dbPostDeactivate.Jobs["job1"][1].Active);
        }

        [Test]
        public void TestDeactivateJobRev()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid jobRev1 = Guid.NewGuid();
            Guid jobRev2 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                Jobs = new Dictionary<string, List<LibWorkInstructions.Structs.Job>>
                {
                    {"job1", new List<LibWorkInstructions.Structs.Job>
                    {
                        new LibWorkInstructions.Structs.Job {Id = "job1", Rev = jobRev1, Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 1, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 2, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 3, JobId = "job1"},
                        }, Active = true },
                        new LibWorkInstructions.Structs.Job {Id = "job1", Rev = jobRev2, Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 4, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 5, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 6, JobId = "job1"},
                        }, Active = true }
                    }
                    }
                },
            };
            n.DataImport(sampleData);
            n.DeactivateJobRev("job1", jobRev1);
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
            Assert.True(dbVarPostUpdate.WorkInstructions[groupId1][1].IdRevGroup == groupId1);
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
            Guid jobRev1 = Guid.NewGuid();
            Guid jobRev2 = Guid.NewGuid();
            Guid jobRev3 = Guid.NewGuid();
            Guid jobRev4 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                Jobs = new Dictionary<string, List<LibWorkInstructions.Structs.Job>>
                {
                    {"job1", new List<LibWorkInstructions.Structs.Job>
                    {
                        new LibWorkInstructions.Structs.Job {Id = "job1", Rev = jobRev1, Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 1, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 2, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 3, JobId = "job1"},
                        } },
                        new LibWorkInstructions.Structs.Job {Id = "job1", Rev = jobRev2, Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 4, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 5, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 6, JobId = "job1"},
                        } }
                    }
                    },
                    {"job2", new List<LibWorkInstructions.Structs.Job>
                    {
                        new LibWorkInstructions.Structs.Job {Id = "job2", Rev = jobRev3, Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 7, JobId = "job2"},
                            new LibWorkInstructions.Structs.Op { Id = 8, JobId = "job2"},
                            new LibWorkInstructions.Structs.Op { Id = 9, JobId = "job2"},
                        } },
                        new LibWorkInstructions.Structs.Job {Id = "job2", Rev = jobRev4, Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 10, JobId = "job2"},
                            new LibWorkInstructions.Structs.Op { Id = 11, JobId = "job2"},
                            new LibWorkInstructions.Structs.Op { Id = 12, JobId = "job2"},
                        } }
                    }
                    }
                },
                JobRevs = new List<Guid> { jobRev1, jobRev2, jobRev3, jobRev4 },
                JobRefToJobRevRefs = new Dictionary<string, List<Guid>>
                {
                    {"job1", new List<Guid>{jobRev1, jobRev2} },
                    {"job2", new List<Guid>{jobRev3, jobRev4} }
                }
            };
            n.DataImport(sampleData);
            n.MergeJobs("job1", "job2");
            var dbPostMerge = n.DataExport();
            Assert.True(dbPostMerge.Jobs["job1"].Count == 4);
            Assert.True(dbPostMerge.Jobs["job2"].Count == 4);
            Assert.True(dbPostMerge.Jobs["job1"].All(y => y.RevSeq == dbPostMerge.Jobs["job1"].IndexOf(y)));
            Assert.True(dbPostMerge.Jobs["job2"].All(y => y.RevSeq == dbPostMerge.Jobs["job2"].IndexOf(y)));
        }

        [Test]
        public void TestSplitJobRev()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid jobRev1 = Guid.NewGuid();
            Guid jobRev2 = Guid.NewGuid();
            Guid jobRev3 = Guid.NewGuid();
            Guid jobRev4 = Guid.NewGuid();
            Guid jobRev5 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                Jobs = new Dictionary<string, List<LibWorkInstructions.Structs.Job>>
                {
                    {"job1", new List<LibWorkInstructions.Structs.Job>
                    {
                        new LibWorkInstructions.Structs.Job {Id = "job1", Rev = jobRev1, Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 1, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 2, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 3, JobId = "job1"},
                        } },
                        new LibWorkInstructions.Structs.Job {Id = "job1", Rev = jobRev2, Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 4, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 5, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 6, JobId = "job1"},
                        } }
                    }
                    },
                    {"job2", new List<LibWorkInstructions.Structs.Job>
                    {
                        new LibWorkInstructions.Structs.Job {Id = "job2", Rev = jobRev3, Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 7, JobId = "job2"},
                            new LibWorkInstructions.Structs.Op { Id = 8, JobId = "job2"},
                            new LibWorkInstructions.Structs.Op { Id = 9, JobId = "job2"},
                        } },
                        new LibWorkInstructions.Structs.Job {Id = "job2", Rev = jobRev4, Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 10, JobId = "job2"},
                            new LibWorkInstructions.Structs.Op { Id = 11, JobId = "job2"},
                            new LibWorkInstructions.Structs.Op { Id = 12, JobId = "job2"},
                        } }
                    }
                    }
                },
                JobRevs = new List<Guid> { jobRev1, jobRev2, jobRev3, jobRev4 },
                JobRefToJobRevRefs = new Dictionary<string, List<Guid>>
                {
                    { "job1", new List<Guid>{ jobRev1, jobRev2 } },
                    { "job2", new List<Guid>{ jobRev3, jobRev4 } }
                },
                JobRevRefToOpRefs = new Dictionary<Guid, List<int>>
                {
                    {jobRev1, new List<int>{1, 2, 3} },
                    {jobRev2, new List<int>{4, 5, 6} },
                    {jobRev3, new List<int>{7, 8, 9} },
                    {jobRev4, new List<int>{10, 11, 12} }
                },
                JobRevRefToQualityClauseRevRefs = new Dictionary<Guid, List<Guid>>
                {
                    {jobRev1, new List<Guid>() },
                    {jobRev2, new List<Guid>() },
                    {jobRev3, new List<Guid>() },
                    {jobRev4, new List<Guid>() },
                }
            };
            n.DataImport(sampleData);
            n.SplitJobRev("job1", jobRev1, jobRev5);
            var dbPostSplit = n.DataExport();
            Assert.True(dbPostSplit.Jobs["job1"].Count == 3);
            Assert.True(dbPostSplit.Jobs["job1"][2].Rev == jobRev5);
            Assert.True(dbPostSplit.Jobs["job1"][2].RevSeq == 2);
            Assert.True(dbPostSplit.Jobs["job1"][2].Ops.SequenceEqual(dbPostSplit.Jobs["job1"][0].Ops));
            Assert.True(dbPostSplit.JobRefToJobRevRefs["job1"][2] == jobRev5);
        }

        [Test]
        public void TestCloneJobAdditive()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid jobRev1 = Guid.NewGuid();
            Guid jobRev2 = Guid.NewGuid();
            Guid jobRev3 = Guid.NewGuid();
            Guid jobRev4 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                Jobs = new Dictionary<string, List<LibWorkInstructions.Structs.Job>>
                {
                    {"job1", new List<LibWorkInstructions.Structs.Job>
                    {
                        new LibWorkInstructions.Structs.Job {Id = "job1", Rev = jobRev1, Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 1, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 2, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 3, JobId = "job1"},
                        } },
                        new LibWorkInstructions.Structs.Job {Id = "job1", Rev = jobRev2, Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 4, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 5, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 6, JobId = "job1"},
                        } }
                    }
                    },
                    {"job2", new List<LibWorkInstructions.Structs.Job>
                    {
                        new LibWorkInstructions.Structs.Job {Id = "job2", Rev = jobRev3, Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 7, JobId = "job2"},
                            new LibWorkInstructions.Structs.Op { Id = 8, JobId = "job2"},
                            new LibWorkInstructions.Structs.Op { Id = 9, JobId = "job2"},
                        } },
                        new LibWorkInstructions.Structs.Job {Id = "job2", Rev = jobRev4, Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 10, JobId = "job2"},
                            new LibWorkInstructions.Structs.Op { Id = 11, JobId = "job2"},
                            new LibWorkInstructions.Structs.Op { Id = 12, JobId = "job2"},
                        } }
                    }
                    }
                },
                JobRevs = new List<Guid> { jobRev1, jobRev2, jobRev3, jobRev4 },
                JobRefToJobRevRefs = new Dictionary<string, List<Guid>>
                {
                    { "job1", new List<Guid>{ jobRev1, jobRev2 } },
                    { "job2", new List<Guid>{ jobRev3, jobRev4 } }
                }
            };
            n.DataImport(sampleData);
            n.CloneJob("job1", "job2", true);
            var dbPostClone = n.DataExport();
            Assert.True(dbPostClone.Jobs["job2"].Count == 4);
            Assert.True(dbPostClone.JobRefToJobRevRefs["job2"].SequenceEqual(new List<Guid> { jobRev1, jobRev2, jobRev3, jobRev4 }));
            Assert.True(dbPostClone.Jobs["job2"].All(y => y.Ops.All(x => x.JobId == "job2")));
            Assert.True(dbPostClone.Jobs["job2"].All(y => y.RevSeq == dbPostClone.Jobs["job2"].IndexOf(y)));
        }

        [Test]
        public void TestCloneJobNotAdditive()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid jobRev1 = Guid.NewGuid();
            Guid jobRev2 = Guid.NewGuid();
            Guid jobRev3 = Guid.NewGuid();
            Guid jobRev4 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                Jobs = new Dictionary<string, List<LibWorkInstructions.Structs.Job>>
                {
                    {"job1", new List<LibWorkInstructions.Structs.Job>
                    {
                        new LibWorkInstructions.Structs.Job {Id = "job1", Rev = jobRev1, Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 1, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 2, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 3, JobId = "job1"},
                        } },
                        new LibWorkInstructions.Structs.Job {Id = "job1", Rev = jobRev2, Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 4, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 5, JobId = "job1"},
                            new LibWorkInstructions.Structs.Op { Id = 6, JobId = "job1"},
                        } }
                    }
                    },
                    {"job2", new List<LibWorkInstructions.Structs.Job>
                    {
                        new LibWorkInstructions.Structs.Job {Id = "job2", Rev = jobRev3, Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 7, JobId = "job2"},
                            new LibWorkInstructions.Structs.Op { Id = 8, JobId = "job2"},
                            new LibWorkInstructions.Structs.Op { Id = 9, JobId = "job2"},
                        } },
                        new LibWorkInstructions.Structs.Job {Id = "job2", Rev = jobRev4, Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 10, JobId = "job2"},
                            new LibWorkInstructions.Structs.Op { Id = 11, JobId = "job2"},
                            new LibWorkInstructions.Structs.Op { Id = 12, JobId = "job2"},
                        } }
                    }
                    }
                },
                JobRevs = new List<Guid> { jobRev1, jobRev2, jobRev3, jobRev4 },
                JobRefToJobRevRefs = new Dictionary<string, List<Guid>>
                {
                    { "job1", new List<Guid>{ jobRev1, jobRev2 } },
                    { "job2", new List<Guid>{ jobRev3, jobRev4 } }
                }
            };
            n.DataImport(sampleData);
            n.CloneJob("job1", "job2", false);
            var dbPostClone = n.DataExport();
            Assert.True(dbPostClone.Jobs["job2"].Count == 2);
            Assert.True(dbPostClone.Jobs["job2"].Select(y => y.Rev).SequenceEqual(dbPostClone.Jobs["job1"].Select(y => y.Rev)));
            Assert.True(dbPostClone.Jobs["job2"].All(y => y.Id == "job2"));
            Assert.True(dbPostClone.Jobs["job2"].All(y => y.Ops.All(x => x.JobId == "job2")));
            Assert.True(dbPostClone.JobRefToJobRevRefs["job2"].SequenceEqual(new List<Guid> {jobRev1, jobRev2}));
        }

        [Test]
        public void TestLinkJobRevAndQualityClauseRev()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid groupId1 = Guid.NewGuid();
            Guid groupId2 = Guid.NewGuid();
            Guid groupId3 = Guid.NewGuid();
            Guid groupId4 = Guid.NewGuid();
            Guid clauseId1 = Guid.NewGuid();
            Guid clauseId2 = Guid.NewGuid();
            Guid clauseId3 = Guid.NewGuid();
            Guid clauseId4 = Guid.NewGuid();
            Guid clauseId5 = Guid.NewGuid();
            Guid clauseId6 = Guid.NewGuid();
            Guid clauseId7 = Guid.NewGuid();
            Guid clauseId8 = Guid.NewGuid();
            Guid clauseId9 = Guid.NewGuid();
            Guid clauseId10 = Guid.NewGuid();
            Guid clauseId11 = Guid.NewGuid();
            Guid clauseId12 = Guid.NewGuid();
            Guid jobRev1 = Guid.NewGuid();
            Guid jobRev2 = Guid.NewGuid();
            Guid jobRev3 = Guid.NewGuid();
            Guid jobRev4 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                Jobs = new Dictionary<string, List<LibWorkInstructions.Structs.Job>>
                {
                    {"job1", new List<LibWorkInstructions.Structs.Job>
                    {
                        new LibWorkInstructions.Structs.Job {Id = "job1", Rev = jobRev1, QualityClauses = new List<LibWorkInstructions.Structs.QualityClause> {
                            new LibWorkInstructions.Structs.QualityClause { Id = clauseId1 },
                            new LibWorkInstructions.Structs.QualityClause { Id = clauseId2 },
                            new LibWorkInstructions.Structs.QualityClause { Id = clauseId3 },
                        } },
                        new LibWorkInstructions.Structs.Job {Id = "job1", Rev = jobRev2, QualityClauses = new List<LibWorkInstructions.Structs.QualityClause> {
                            new LibWorkInstructions.Structs.QualityClause { Id = clauseId4 },
                            new LibWorkInstructions.Structs.QualityClause { Id = clauseId5 },
                            new LibWorkInstructions.Structs.QualityClause { Id = clauseId6 },
                        } }
                    }
                    },
                    {"job2", new List<LibWorkInstructions.Structs.Job>
                    {
                        new LibWorkInstructions.Structs.Job {Id = "job2", Rev = jobRev3, QualityClauses = new List<LibWorkInstructions.Structs.QualityClause> {
                            new LibWorkInstructions.Structs.QualityClause { Id = clauseId7 },
                            new LibWorkInstructions.Structs.QualityClause { Id = clauseId8 },
                            new LibWorkInstructions.Structs.QualityClause { Id = clauseId9 },
                        } },
                        new LibWorkInstructions.Structs.Job {Id = "job2", Rev = jobRev4, QualityClauses = new List<LibWorkInstructions.Structs.QualityClause> {
                            new LibWorkInstructions.Structs.QualityClause { Id = clauseId10 },
                            new LibWorkInstructions.Structs.QualityClause { Id = clauseId11 },
                            new LibWorkInstructions.Structs.QualityClause { Id = clauseId12 },
                        } }
                    }
                    }
                },
                QualityClauseRevs = new List<Guid> { clauseId1, clauseId2, clauseId3, clauseId4, 
                                                     clauseId5, clauseId6, clauseId7, clauseId8, 
                                                     clauseId9, clauseId10, clauseId11, clauseId12},
                JobRevs = new List<Guid> { jobRev1, jobRev2, jobRev3, jobRev4 },
                JobRevRefToQualityClauseRevRefs = new Dictionary<Guid, List<Guid>>
                {
                    {jobRev1, new List<Guid>{clauseId1, clauseId2, clauseId3} },
                    {jobRev2, new List<Guid>{clauseId4, clauseId5, clauseId6} },
                    {jobRev3, new List<Guid>{clauseId7, clauseId8, clauseId9} },
                    {jobRev4, new List<Guid>{clauseId10, clauseId11, clauseId12} }
                },
                QualityClauseRevRefToJobRevRefs = new Dictionary<Guid, List<Guid>>
                {
                    {clauseId1, new List<Guid> { jobRev1 } },
                    {clauseId2, new List<Guid> { jobRev1 } },
                    {clauseId3, new List<Guid> { jobRev1 } },
                    {clauseId4, new List<Guid> { jobRev2 } },
                    {clauseId5, new List<Guid> { jobRev2 } },
                    {clauseId6, new List<Guid> { jobRev2 } },
                    {clauseId7, new List<Guid> { jobRev3 } },
                    {clauseId8, new List<Guid> { jobRev3 } },
                    {clauseId9, new List<Guid> { jobRev3 } },
                    {clauseId10, new List<Guid> { jobRev4 } },
                    {clauseId11, new List<Guid> { jobRev4 } },
                    {clauseId12, new List<Guid> { jobRev4 } },
                },
                 QualityClauses = new Dictionary<Guid, List<LibWorkInstructions.Structs.QualityClause>>
                {
                    {groupId1, new List<LibWorkInstructions.Structs.QualityClause> {
                        new LibWorkInstructions.Structs.QualityClause {Id = clauseId1},
                    new LibWorkInstructions.Structs.QualityClause {Id = clauseId2},
                    new LibWorkInstructions.Structs.QualityClause {Id = clauseId3}} },
                    {groupId2, new List<LibWorkInstructions.Structs.QualityClause> {
                        new LibWorkInstructions.Structs.QualityClause {Id = clauseId4},
                    new LibWorkInstructions.Structs.QualityClause {Id = clauseId5},
                    new LibWorkInstructions.Structs.QualityClause {Id = clauseId6},} },
                    {groupId3, new List<LibWorkInstructions.Structs.QualityClause> {
                        new LibWorkInstructions.Structs.QualityClause {Id = clauseId7},
                    new LibWorkInstructions.Structs.QualityClause {Id = clauseId8},
                    new LibWorkInstructions.Structs.QualityClause {Id = clauseId9}} },
                    {groupId4, new List<LibWorkInstructions.Structs.QualityClause> {
                        new LibWorkInstructions.Structs.QualityClause {Id = clauseId10},
                    new LibWorkInstructions.Structs.QualityClause {Id = clauseId11},
                    new LibWorkInstructions.Structs.QualityClause {Id = clauseId12},} },
                }
            };
            n.DataImport(sampleData);
            n.LinkJobRevAndQualityClauseRev(jobRev4, clauseId8);
            var dbPostLink = n.DataExport();
            Assert.True(dbPostLink.Jobs["job2"][1].QualityClauses.Count == 4);
            Assert.True(dbPostLink.Jobs["job2"][1].QualityClauses[3].Id == clauseId8);
            Assert.True(dbPostLink.JobRevRefToQualityClauseRevRefs[jobRev4].Count == 4);
            Assert.True(dbPostLink.JobRevRefToQualityClauseRevRefs[jobRev4][3] == clauseId8);
            Assert.True(dbPostLink.QualityClauseRevRefToJobRevRefs[clauseId8].Contains(jobRev4));
        }

        [Test]
        public void TestUnlinkJobRevAndQualityClauseRev()
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
            Guid clauseId9 = Guid.NewGuid();
            Guid clauseId10 = Guid.NewGuid();
            Guid clauseId11 = Guid.NewGuid();
            Guid clauseId12 = Guid.NewGuid();
            Guid jobRev1 = Guid.NewGuid();
            Guid jobRev2 = Guid.NewGuid();
            Guid jobRev3 = Guid.NewGuid();
            Guid jobRev4 = Guid.NewGuid();

            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                Jobs = new Dictionary<string, List<LibWorkInstructions.Structs.Job>>
                {
                    {"job1", new List<LibWorkInstructions.Structs.Job>
                    {
                        new LibWorkInstructions.Structs.Job {Id = "job1", Rev = jobRev1, QualityClauses = new List<LibWorkInstructions.Structs.QualityClause> {
                            new LibWorkInstructions.Structs.QualityClause { Id = clauseId1 },
                            new LibWorkInstructions.Structs.QualityClause { Id = clauseId2 },
                            new LibWorkInstructions.Structs.QualityClause { Id = clauseId3 },
                        } },
                        new LibWorkInstructions.Structs.Job {Id = "job1", Rev = jobRev2, QualityClauses = new List<LibWorkInstructions.Structs.QualityClause> {
                            new LibWorkInstructions.Structs.QualityClause { Id = clauseId4 },
                            new LibWorkInstructions.Structs.QualityClause { Id = clauseId5 },
                            new LibWorkInstructions.Structs.QualityClause { Id = clauseId6 },
                        } }
                    }
                    },
                    {"job2", new List<LibWorkInstructions.Structs.Job>
                    {
                        new LibWorkInstructions.Structs.Job {Id = "job2", Rev = jobRev3, QualityClauses = new List<LibWorkInstructions.Structs.QualityClause> {
                            new LibWorkInstructions.Structs.QualityClause { Id = clauseId7 },
                            new LibWorkInstructions.Structs.QualityClause { Id = clauseId8 },
                            new LibWorkInstructions.Structs.QualityClause { Id = clauseId9 },
                        } },
                        new LibWorkInstructions.Structs.Job {Id = "job2", Rev = jobRev4, QualityClauses = new List<LibWorkInstructions.Structs.QualityClause> {
                            new LibWorkInstructions.Structs.QualityClause { Id = clauseId10 },
                            new LibWorkInstructions.Structs.QualityClause { Id = clauseId11 },
                            new LibWorkInstructions.Structs.QualityClause { Id = clauseId12 },
                        } }
                    }
                    }
                },
                QualityClauseRevs = new List<Guid> { clauseId1, clauseId2, clauseId3, clauseId4,
                                                     clauseId5, clauseId6, clauseId7, clauseId8,
                                                     clauseId9, clauseId10, clauseId11, clauseId12,},
                JobRevs = new List<Guid> { jobRev1, jobRev2, jobRev3, jobRev4 },
                JobRevRefToQualityClauseRevRefs = new Dictionary<Guid, List<Guid>>
                {
                    {jobRev1, new List<Guid>{clauseId1, clauseId2, clauseId3} },
                    {jobRev2, new List<Guid>{clauseId4, clauseId5, clauseId6} },
                    {jobRev3, new List<Guid>{clauseId7, clauseId8, clauseId9} },
                    {jobRev4, new List<Guid>{clauseId10, clauseId11, clauseId12} }
                },
                QualityClauseRevRefToJobRevRefs = new Dictionary<Guid, List<Guid>>
                {
                    {clauseId1, new List<Guid> { jobRev1 } },
                    {clauseId2, new List<Guid> { jobRev1 } },
                    {clauseId3, new List<Guid> { jobRev1 } },
                    {clauseId4, new List<Guid> { jobRev2 } },
                    {clauseId5, new List<Guid> { jobRev2 } },
                    {clauseId6, new List<Guid> { jobRev2 } },
                    {clauseId7, new List<Guid> { jobRev3 } },
                    {clauseId8, new List<Guid> { jobRev3 } },
                    {clauseId9, new List<Guid> { jobRev3 } },
                    {clauseId10, new List<Guid> { jobRev4 } },
                    {clauseId11, new List<Guid> { jobRev4 } },
                    {clauseId12, new List<Guid> { jobRev4 } }
                }
            };
            n.DataImport(sampleData);
            n.UnlinkJobRevAndQualityClauseRev(jobRev4, clauseId10);
            var dbPostLink = n.DataExport();
            Assert.True(dbPostLink.Jobs["job2"][1].QualityClauses.Count == 2);
            Assert.False(dbPostLink.Jobs["job2"][1].QualityClauses.Any(y => y.Id == clauseId10));
            Assert.True(dbPostLink.JobRevRefToQualityClauseRevRefs[jobRev4].Count == 2);
            Assert.False(dbPostLink.JobRevRefToQualityClauseRevRefs[jobRev4].Contains(clauseId10));
            Assert.False(dbPostLink.QualityClauseRevRefToJobRevRefs[clauseId10].Contains(jobRev4));
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
            n.CloneWorkInstructionRevs(groupId1, groupId2, true);
            var dbPostClone = n.DataExport();

            Assert.True(dbPostClone.WorkInstructions[groupId2].Count == 4);
            Assert.True(dbPostClone.WorkInstructionRefToWorkInstructionRevRefs[groupId2].OrderBy(y => y).SequenceEqual(new List<Guid> { workId1, workId2, workId3, workId4}.OrderBy(y => y)));
            Assert.True(dbPostClone.WorkInstructions[groupId2].All(y => y.RevSeq == dbPostClone.WorkInstructions[groupId2].IndexOf(y)));
            Assert.True(dbPostClone.WorkInstructions[groupId2].All(y => y.IdRevGroup == groupId2));
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
            n.CloneWorkInstructionRevs(groupId1, groupId2, false);
            var dbPostClone = n.DataExport();

            Assert.True(dbPostClone.WorkInstructions[groupId2].Count == 2);
            Assert.True(dbPostClone.WorkInstructionRefToWorkInstructionRevRefs[groupId2].SequenceEqual(dbPostClone.WorkInstructionRefToWorkInstructionRevRefs[groupId1]));
            Assert.True(dbPostClone.WorkInstructions[groupId2].All(y => y.RevSeq == dbPostClone.WorkInstructions[groupId2].IndexOf(y)));
            Assert.True(dbPostClone.WorkInstructions[groupId2].All(y => y.IdRevGroup == groupId2));
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
                },
                OpSpecRefToOpSpecRevRefs = new Dictionary<Guid, List<Guid>>
                {
                    {groupId1, new List<Guid> {specId1, specId2} }
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
                },
                OpSpecRefToOpSpecRevRefs = new Dictionary<Guid, List<Guid>>
                {
                    {groupId1, new List<Guid>{specId1, specId2} }
                }
            };

            n.DataImport(sampleData);
            n.CreateOpSpecRev(opSpec);
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
            Assert.True(dbPostUpdate.OpSpecs[groupId1].Count == 2);
            Assert.True(dbPostUpdate.OpSpecs[groupId1][0].Name == "spec56");
            Assert.True(dbPostUpdate.OpSpecs[groupId1][0].Comment == "This is a test");
            Assert.True(dbPostUpdate.OpSpecs[groupId1][0].IdRevGroup == groupId1);
            Assert.True(dbPostUpdate.OpSpecs[groupId1][0].Id == specId1);
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
        public void TestCloneOpSpecAdditive()
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
            n.CloneOpSpec(groupId1, groupId2, true);
            var dbPostClone = n.DataExport();
            Assert.True(dbPostClone.OpSpecs[groupId2].Count == 6);
            Assert.True(dbPostClone.OpSpecs[groupId1].All(y => dbPostClone.OpSpecs[groupId2].Contains(y)));
            Assert.True(dbPostClone.OpSpecs[groupId2].All(y => y.IdRevGroup == groupId2));
            Assert.True(dbPostClone.OpSpecs[groupId2].All(y => y.RevSeq == dbPostClone.OpSpecs[groupId2].IndexOf(y)));
        }

        [Test]
        public void TestCloneOpSpecNotAdditive()
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
            n.CloneOpSpec(groupId1, groupId2, false);
            var dbPostClone = n.DataExport();
            Assert.True(dbPostClone.OpSpecs[groupId2].Count == 3);
            Assert.True(dbPostClone.OpSpecs[groupId2].SequenceEqual(dbPostClone.OpSpecs[groupId1]));
            Assert.True(dbPostClone.OpSpecs[groupId2].All(y => y.IdRevGroup == groupId2));
            Assert.True(dbPostClone.OpSpecs[groupId2].All(y => y.RevSeq == dbPostClone.OpSpecs[groupId2].IndexOf(y)));
        }

        [Test]
        public void TestCloneJobOpBasedOnOpSpecRevsAdditive()
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
            n.CloneJobOpBasedOnOpSpecRevs(1, 2, true);
            var dbPostClone = n.DataExport();
            Assert.True(dbPostClone.OpRefToOpSpecRevRefs[2].Count == 4);
            Assert.True(dbPostClone.OpRefToOpSpecRevRefs[1].All(y => dbPostClone.OpRefToOpSpecRevRefs[2].Contains(y)));
        }

        [Test]
        public void TestCloneJobOpBasedOnOpSpecRevsNotAdditive()
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
            n.CloneJobOpBasedOnOpSpecRevs(1, 2, false);
            var dbPostClone = n.DataExport();
            Assert.True(dbPostClone.OpRefToOpSpecRevRefs[2].Count == 2);
            Assert.True(dbPostClone.OpRefToOpSpecRevRefs[2].SequenceEqual(dbPostClone.OpRefToOpSpecRevRefs[1]));
        }

        [Test]
        public void TestLinkWorkInstructionToJobOp()
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
                Ops = new Dictionary<int, LibWorkInstructions.Structs.Op>
                {
                    { 1, new LibWorkInstructions.Structs.Op { Id = 1, JobId = "job1" } },
                    { 2, new LibWorkInstructions.Structs.Op { Id = 2, JobId = "job1" } },
                    { 3, new LibWorkInstructions.Structs.Op { Id = 3, JobId = "job1" } },
                    { 4, new LibWorkInstructions.Structs.Op { Id = 4, JobId = "job1" } },
                    { 5, new LibWorkInstructions.Structs.Op { Id = 5, JobId = "job1" } },
                    { 6, new LibWorkInstructions.Structs.Op { Id = 6, JobId = "job1" } },
                    { 7, new LibWorkInstructions.Structs.Op { Id = 7, JobId = "job2" } },
                    { 8, new LibWorkInstructions.Structs.Op { Id = 8, JobId = "job2" } },
                    { 9, new LibWorkInstructions.Structs.Op { Id = 9, JobId = "job2" } },
                    { 10, new LibWorkInstructions.Structs.Op { Id = 10, JobId = "job2" } },
                    { 11, new LibWorkInstructions.Structs.Op { Id = 11, JobId = "job2" } },
                    { 12, new LibWorkInstructions.Structs.Op { Id = 12, JobId = "job2" } },
                },
                WorkInstructionRefToWorkInstructionRevRefs = new Dictionary<Guid, List<Guid>>
                {
                    { groupId1, new List<Guid>() },
                    { groupId2, new List<Guid>() },
                },
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
                OpRefToWorkInstructionRef = new Dictionary<int, Guid>()
            };
            n.DataImport(sampleData);
            n.LinkWorkInstructionToJobOp(groupId1, 1);
            var dbPostLink = n.DataExport();
            Assert.True(dbPostLink.WorkInstructions[groupId1].All(y => y.OpId == 1));
            Assert.True(dbPostLink.OpRefToWorkInstructionRef[1] == groupId1);
        }

        [Test]
        public void TestUnlinkWorkInstructionFromJobOp()
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
                Ops = new Dictionary<int, LibWorkInstructions.Structs.Op>
                {
                    { 1, new LibWorkInstructions.Structs.Op { Id = 1, JobId = "job1" } },
                    { 2, new LibWorkInstructions.Structs.Op { Id = 2, JobId = "job1" } },
                    { 3, new LibWorkInstructions.Structs.Op { Id = 3, JobId = "job1" } },
                    { 4, new LibWorkInstructions.Structs.Op { Id = 4, JobId = "job1" } },
                    { 5, new LibWorkInstructions.Structs.Op { Id = 5, JobId = "job1" } },
                    { 6, new LibWorkInstructions.Structs.Op { Id = 6, JobId = "job1" } },
                    { 7, new LibWorkInstructions.Structs.Op { Id = 7, JobId = "job2" } },
                    { 8, new LibWorkInstructions.Structs.Op { Id = 8, JobId = "job2" } },
                    { 9, new LibWorkInstructions.Structs.Op { Id = 9, JobId = "job2" } },
                    { 10, new LibWorkInstructions.Structs.Op { Id = 10, JobId = "job2" } },
                    { 11, new LibWorkInstructions.Structs.Op { Id = 11, JobId = "job2" } },
                    { 12, new LibWorkInstructions.Structs.Op { Id = 12, JobId = "job2" } },
                },
                WorkInstructionRefToWorkInstructionRevRefs = new Dictionary<Guid, List<Guid>>
                {
                    { groupId1, new List<Guid>() },
                    { groupId2, new List<Guid>() },
                },
                WorkInstructions = new Dictionary<Guid, List<LibWorkInstructions.Structs.WorkInstruction>>
                {
                    { groupId1, new List<LibWorkInstructions.Structs.WorkInstruction> {
                        new LibWorkInstructions.Structs.WorkInstruction {
                            Id = workId1, IdRevGroup = groupId1, OpId = 5 },
                        new LibWorkInstructions.Structs.WorkInstruction {
                            Id = workId2, IdRevGroup = groupId1, OpId = 5 } } },
                    { groupId2, new List<LibWorkInstructions.Structs.WorkInstruction> {
                        new LibWorkInstructions.Structs.WorkInstruction {
                            Id = workId3, IdRevGroup = groupId2, OpId = 8 },
                        new LibWorkInstructions.Structs.WorkInstruction {
                            Id = workId4, IdRevGroup = groupId2, OpId = 8 } } },
                },
                OpRefToWorkInstructionRef = new Dictionary<int, Guid>
                {
                    {5, groupId1 },
                    {8, groupId2 }
                }
            };
            n.DataImport(sampleData);
            n.UnlinkWorkInstructionFromJobOp(groupId1, 5);
            var dbPostLink = n.DataExport();
            Assert.True(dbPostLink.WorkInstructions[groupId1].All(y => y.OpId == -1));
            Assert.False(dbPostLink.OpRefToWorkInstructionRef.ContainsKey(5));
        }

        [Test]
        public void TestMergeWorkInstructionRevs()
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
                WorkInstructionRefToWorkInstructionRevRefs = new Dictionary<Guid, List<Guid>>
                {
                    { groupId1, new List<Guid> { workId1, workId2 } },
                    { groupId2, new List<Guid> { workId3, workId4 } },
                },
                WorkInstructions = new Dictionary<Guid, List<LibWorkInstructions.Structs.WorkInstruction>>
                {
                    { groupId1, new List<LibWorkInstructions.Structs.WorkInstruction> {
                        new LibWorkInstructions.Structs.WorkInstruction {
                            Id = workId1, IdRevGroup = groupId1, OpId = 5 },
                        new LibWorkInstructions.Structs.WorkInstruction {
                            Id = workId2, IdRevGroup = groupId1, OpId = 5 } } },
                    { groupId2, new List<LibWorkInstructions.Structs.WorkInstruction> {
                        new LibWorkInstructions.Structs.WorkInstruction {
                            Id = workId3, IdRevGroup = groupId2, OpId = 8 },
                        new LibWorkInstructions.Structs.WorkInstruction {
                            Id = workId4, IdRevGroup = groupId2, OpId = 8 } } },
                },
                WorkInstructionRevs = new List<Guid> { workId1, workId2, workId3, workId4 }
            };
            n.DataImport(sampleData);
            n.MergeWorkInstructionRevs(groupId1, groupId2);
            var dbPostMerge = n.DataExport();
            Assert.True(dbPostMerge.WorkInstructions[groupId1].Count == 4);
            Assert.True(dbPostMerge.WorkInstructions[groupId2].Count == 4);
            Assert.True(dbPostMerge.WorkInstructions[groupId1].All(y => y.IdRevGroup == groupId1));
            Assert.True(dbPostMerge.WorkInstructions[groupId2].All(y => y.IdRevGroup == groupId2));
        }

        [Test]
        public void TestSplitWorkInstructionRev()
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
                WorkInstructionRefToWorkInstructionRevRefs = new Dictionary<Guid, List<Guid>>
                {
                    { groupId1, new List<Guid> { workId1, workId2 } },
                    { groupId2, new List<Guid> { workId3, workId4 } },
                },
                WorkInstructions = new Dictionary<Guid, List<LibWorkInstructions.Structs.WorkInstruction>>
                {
                    { groupId1, new List<LibWorkInstructions.Structs.WorkInstruction> {
                        new LibWorkInstructions.Structs.WorkInstruction {
                            Id = workId1, IdRevGroup = groupId1, OpId = 5 },
                        new LibWorkInstructions.Structs.WorkInstruction {
                            Id = workId2, IdRevGroup = groupId1, OpId = 5 } } },
                    { groupId2, new List<LibWorkInstructions.Structs.WorkInstruction> {
                        new LibWorkInstructions.Structs.WorkInstruction {
                            Id = workId3, IdRevGroup = groupId2, OpId = 8 },
                        new LibWorkInstructions.Structs.WorkInstruction {
                            Id = workId4, IdRevGroup = groupId2, OpId = 8 } } },
                }
            };
            n.DataImport(sampleData);
            n.SplitWorkInstructionRev(groupId1, workId1);
            var dbPostSplit = n.DataExport();
            Assert.True(dbPostSplit.WorkInstructions[groupId1].Count == 3);
            Assert.False(dbPostSplit.WorkInstructions[groupId1][0].Id == dbPostSplit.WorkInstructions[groupId1][2].Id);
            Assert.True(dbPostSplit.WorkInstructionRefToWorkInstructionRevRefs[groupId1].Count == 3);
            Assert.False(dbPostSplit.WorkInstructionRefToWorkInstructionRevRefs[groupId1][0] == dbPostSplit.WorkInstructionRefToWorkInstructionRevRefs[groupId1][2]);
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
        public void TestMergeJobRevsBasedOnQualityClauseRevs()
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
            Guid jobRev1 = Guid.NewGuid();
            Guid jobRev2 = Guid.NewGuid();
            Guid jobRev3 = Guid.NewGuid();
            Guid jobRev4 = Guid.NewGuid();
            Guid jobRev5 = Guid.NewGuid();
            Guid jobRev6 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                Jobs = new Dictionary<string, List<LibWorkInstructions.Structs.Job>>
                {
                    {"job1", new List<LibWorkInstructions.Structs.Job>{
                        new LibWorkInstructions.Structs.Job{
                        Id = "job1", Rev = jobRev1, QualityClauses = new List<LibWorkInstructions.Structs.QualityClause> {
                            new LibWorkInstructions.Structs.QualityClause {Id = clause1, IdRevGroup = groupId1}
                        } },
                        new LibWorkInstructions.Structs.Job{
                        Id = "job1", Rev = jobRev2, QualityClauses = new List<LibWorkInstructions.Structs.QualityClause> {
                            new LibWorkInstructions.Structs.QualityClause {Id = clause2, IdRevGroup = groupId1}
                        } },
                        new LibWorkInstructions.Structs.Job{
                        Id = "job1", Rev = jobRev3, QualityClauses = new List<LibWorkInstructions.Structs.QualityClause> {
                            new LibWorkInstructions.Structs.QualityClause {Id = clause3, IdRevGroup = groupId1}
                        } },
                    } },
                    {"job2", new List<LibWorkInstructions.Structs.Job>{
                        new LibWorkInstructions.Structs.Job{
                        Id = "job2", Rev = jobRev4, QualityClauses = new List<LibWorkInstructions.Structs.QualityClause> {
                            new LibWorkInstructions.Structs.QualityClause {Id = clause4, IdRevGroup = groupId2}
                        } },
                        new LibWorkInstructions.Structs.Job{
                        Id = "job2", Rev = jobRev5, QualityClauses = new List<LibWorkInstructions.Structs.QualityClause> {
                            new LibWorkInstructions.Structs.QualityClause {Id = clause5, IdRevGroup = groupId2}
                        } },
                        new LibWorkInstructions.Structs.Job{
                        Id = "job2", Rev = jobRev6, QualityClauses = new List<LibWorkInstructions.Structs.QualityClause> {
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
                JobRevs = new List<Guid> { jobRev1, jobRev2, jobRev3, jobRev4, jobRev5, jobRev6 },
                JobRevRefToQualityClauseRevRefs = new Dictionary<Guid, List<Guid>>
                {
                    {jobRev1, new List<Guid> {clause1} },
                    {jobRev2, new List<Guid> {clause2} },
                    {jobRev3, new List<Guid> {clause3} },
                    {jobRev4, new List<Guid> {clause4} },
                    {jobRev5, new List<Guid> {clause5} },
                    {jobRev6, new List<Guid> {clause6, clause7} }
                }
            };
            n.DataImport(sampleData);
            n.MergeJobRevsBasedOnQualityClauseRevs(jobRev1, jobRev5);
            var dbPostMerge = n.DataExport();
            Assert.True(dbPostMerge.Jobs["job1"][0].QualityClauses.Count == 2);
            Assert.True(dbPostMerge.Jobs["job1"][0].QualityClauses.SequenceEqual(dbPostMerge.Jobs["job2"][1].QualityClauses));
            Assert.True(dbPostMerge.JobRevRefToQualityClauseRevRefs[jobRev1].SequenceEqual(dbPostMerge.JobRevRefToQualityClauseRevRefs[jobRev5]));
            Assert.True(dbPostMerge.Jobs["job1"][0].QualityClauses.All(y => y.RevSeq == dbPostMerge.Jobs["job1"][0].QualityClauses.IndexOf(y)));
        }

        [Test]
        public void TestSplitQualityClauseRev()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid groupId1 = Guid.NewGuid();
            Guid groupId2 = Guid.NewGuid();
            Guid clauseId1 = Guid.NewGuid();
            Guid clauseId2 = Guid.NewGuid();
            Guid jobRev1 = Guid.NewGuid();
            Guid jobRev2 = Guid.NewGuid();
            Guid jobRev3 = Guid.NewGuid();
            Guid jobRev4 = Guid.NewGuid();
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
                },
                QualityClauseRevRefToJobRevRefs = new Dictionary<Guid, List<Guid>>
                {
                    {clauseId1, new List<Guid>{ jobRev1, jobRev2 } },
                    {clauseId2, new List<Guid>{ jobRev3, jobRev4 } }
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
        public void TestCloneQualityClauseRevsAdditive()
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
            n.CloneQualityClauseRevs(groupId1, groupId2, true);
            var dbPostClone = n.DataExport();
            Assert.True(dbPostClone.QualityClauses[groupId2].Count == 2);
            Assert.True(dbPostClone.QualityClauses[groupId2].Last().IdRevGroup == groupId2);
            Assert.True(dbPostClone.QualityClauseRefToQualityClauseRevRefs[groupId2].Count == 2);
            Assert.True(dbPostClone.QualityClauseRefToQualityClauseRevRefs[groupId2][0] == clauseId1);
        }

        [Test]
        public void TestCloneQualityClauseRevsNotAdditive()
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
            n.CloneQualityClauseRevs(groupId1, groupId2, false);
            var dbPostClone = n.DataExport();
            Assert.True(dbPostClone.QualityClauses[groupId2].Count == 1);
            Assert.True(dbPostClone.QualityClauses[groupId2].Last().IdRevGroup == groupId2);
            Assert.True(dbPostClone.QualityClauseRefToQualityClauseRevRefs[groupId2].Count == 1);
            Assert.True(dbPostClone.QualityClauseRefToQualityClauseRevRefs[groupId2][0] == clauseId1);
        }

        [Test]
        public void TestLinkJobOpAndJobRev()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid jobRev1 = Guid.NewGuid();
            Guid jobRev2 = Guid.NewGuid();
            Guid jobRev3 = Guid.NewGuid();
            Guid jobRev4 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                Jobs = new Dictionary<string, List<LibWorkInstructions.Structs.Job>>
                {
                    { "job1", new List<LibWorkInstructions.Structs.Job>
                    {
                        new LibWorkInstructions.Structs.Job { Id = "job1", Rev = jobRev1, Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 1, JobId = "job1" },
                            new LibWorkInstructions.Structs.Op { Id = 2, JobId = "job1" },
                            new LibWorkInstructions.Structs.Op { Id = 3, JobId = "job1" },
                        } },
                        new LibWorkInstructions.Structs.Job { Id = "job1", Rev = jobRev2, Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 4, JobId = "job1" },
                            new LibWorkInstructions.Structs.Op { Id = 5, JobId = "job1" },
                            new LibWorkInstructions.Structs.Op { Id = 6, JobId = "job1" },
                        } }
                    }
                    },
                    { "job2", new List<LibWorkInstructions.Structs.Job>
                    {
                        new LibWorkInstructions.Structs.Job { Id = "job2", Rev = jobRev3, Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 7, JobId = "job2" },
                            new LibWorkInstructions.Structs.Op { Id = 8, JobId = "job2" },
                            new LibWorkInstructions.Structs.Op { Id = 9, JobId = "job2" },
                        } },
                        new LibWorkInstructions.Structs.Job { Id = "job2", Rev = jobRev4, Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 10, JobId = "job2" },
                            new LibWorkInstructions.Structs.Op { Id = 11, JobId = "job2" },
                            new LibWorkInstructions.Structs.Op { Id = 12, JobId = "job2" },
                        } }
                    }
                    }
                },
                JobRevs = new List<Guid> { jobRev1, jobRev2, jobRev3, jobRev4 },
                JobRefToJobRevRefs = new Dictionary<string, List<Guid>>
                {
                    { "job1", new List<Guid> { jobRev1, jobRev2 } },
                    { "job2", new List<Guid> { jobRev3, jobRev4 } }
                },
                JobRevRefToOpRefs = new Dictionary<Guid, List<int>>
                {
                    { jobRev1, new List<int>{ 1, 2, 3 } },
                    { jobRev2, new List<int>{ 4, 5, 6 } },
                    { jobRev3, new List<int>{ 7, 8, 9 } },
                    { jobRev4, new List<int>{ 10, 11, 12 } },
                },
                Ops = new Dictionary<int, LibWorkInstructions.Structs.Op>
                {
                    {1, new LibWorkInstructions.Structs.Op { Id = 1, JobId = "job1"} },
                    {2, new LibWorkInstructions.Structs.Op { Id = 2, JobId = "job1"} },
                    {3, new LibWorkInstructions.Structs.Op { Id = 3, JobId = "job1"} },
                    {4, new LibWorkInstructions.Structs.Op { Id = 4, JobId = "job1"} },
                    {5, new LibWorkInstructions.Structs.Op { Id = 5, JobId = "job1"} },
                    {6, new LibWorkInstructions.Structs.Op { Id = 6, JobId = "job1"} },
                    {7, new LibWorkInstructions.Structs.Op { Id = 7, JobId = "job2"} },
                    {8, new LibWorkInstructions.Structs.Op { Id = 8, JobId = "job2"} },
                    {9, new LibWorkInstructions.Structs.Op { Id = 9, JobId = "job2"} },
                    {10, new LibWorkInstructions.Structs.Op { Id = 10, JobId = "job2"} },
                    {11, new LibWorkInstructions.Structs.Op { Id = 11, JobId = "job2"} },
                    {12, new LibWorkInstructions.Structs.Op { Id = 12, JobId = "job2"} },
                    {13, new LibWorkInstructions.Structs.Op { Id = 13} }
                }
            };
            n.DataImport(sampleData);
            n.LinkJobOpAndJobRev(13, jobRev1);
            var dbPostLink = n.DataExport();
            Assert.True(dbPostLink.JobRevRefToOpRefs[jobRev1].Count == 4);
            Assert.True(dbPostLink.Jobs["job1"][0].Ops.Count == 4);
            Assert.True(dbPostLink.Jobs["job1"][0].Ops[3].Id == 13);
        }

        [Test]
        public void TestUnlinkJobOpAndJobRev()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid jobRev1 = Guid.NewGuid();
            Guid jobRev2 = Guid.NewGuid();
            Guid jobRev3 = Guid.NewGuid();
            Guid jobRev4 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                Jobs = new Dictionary<string, List<LibWorkInstructions.Structs.Job>>
                {
                    { "job1", new List<LibWorkInstructions.Structs.Job>
                    {
                        new LibWorkInstructions.Structs.Job { Id = "job1", Rev = jobRev1, Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 1, JobId = "job1" },
                            new LibWorkInstructions.Structs.Op { Id = 2, JobId = "job1" },
                            new LibWorkInstructions.Structs.Op { Id = 3, JobId = "job1" },
                        } },
                        new LibWorkInstructions.Structs.Job { Id = "job1", Rev = jobRev2, Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 4, JobId = "job1" },
                            new LibWorkInstructions.Structs.Op { Id = 5, JobId = "job1" },
                            new LibWorkInstructions.Structs.Op { Id = 6, JobId = "job1" },
                        } }
                    }
                    },
                    { "job2", new List<LibWorkInstructions.Structs.Job>
                    {
                        new LibWorkInstructions.Structs.Job { Id = "job2", Rev = jobRev3, Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 7, JobId = "job2" },
                            new LibWorkInstructions.Structs.Op { Id = 8, JobId = "job2" },
                            new LibWorkInstructions.Structs.Op { Id = 9, JobId = "job2" },
                        } },
                        new LibWorkInstructions.Structs.Job { Id = "job2", Rev = jobRev4, Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 10, JobId = "job2" },
                            new LibWorkInstructions.Structs.Op { Id = 11, JobId = "job2" },
                            new LibWorkInstructions.Structs.Op { Id = 12, JobId = "job2" },
                        } }
                    }
                    }
                },
                JobRevs = new List<Guid> { jobRev1, jobRev2, jobRev3, jobRev4 },
                JobRefToJobRevRefs = new Dictionary<string, List<Guid>>
                {
                    { "job1", new List<Guid> { jobRev1, jobRev2 } },
                    { "job2", new List<Guid> { jobRev3, jobRev4 } }
                },
                JobRevRefToOpRefs = new Dictionary<Guid, List<int>>
                {
                    { jobRev1, new List<int>{ 1, 2, 3 } },
                    { jobRev2, new List<int>{ 4, 5, 6 } },
                    { jobRev3, new List<int>{ 7, 8, 9 } },
                    { jobRev4, new List<int>{ 10, 11, 12 } },
                },
                Ops = new Dictionary<int, LibWorkInstructions.Structs.Op>
                {
                    {1, new LibWorkInstructions.Structs.Op { Id = 1, JobId = "job1"} },
                    {2, new LibWorkInstructions.Structs.Op { Id = 2, JobId = "job1"} },
                    {3, new LibWorkInstructions.Structs.Op { Id = 3, JobId = "job1"} },
                    {4, new LibWorkInstructions.Structs.Op { Id = 4, JobId = "job1"} },
                    {5, new LibWorkInstructions.Structs.Op { Id = 5, JobId = "job1"} },
                    {6, new LibWorkInstructions.Structs.Op { Id = 6, JobId = "job1"} },
                    {7, new LibWorkInstructions.Structs.Op { Id = 7, JobId = "job2"} },
                    {8, new LibWorkInstructions.Structs.Op { Id = 8, JobId = "job2"} },
                    {9, new LibWorkInstructions.Structs.Op { Id = 9, JobId = "job2"} },
                    {10, new LibWorkInstructions.Structs.Op { Id = 10, JobId = "job2"} },
                    {11, new LibWorkInstructions.Structs.Op { Id = 11, JobId = "job2"} },
                    {12, new LibWorkInstructions.Structs.Op { Id = 12, JobId = "job2"} },
                }
            };
            n.DataImport(sampleData);
            n.UnlinkJobOpAndJobRev(3, jobRev1);
            var dbPostUnlink = n.DataExport();
            Assert.True(dbPostUnlink.Jobs["job1"][0].Ops.Count == 2);
            Assert.True(dbPostUnlink.JobRevRefToOpRefs[jobRev1].Count == 2);
            Assert.False(dbPostUnlink.JobRevRefToOpRefs[jobRev1].Contains(3));
        }

        [Test]
        public void TestMergeJobRevsBasedOnJobOps()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid jobRev1 = Guid.NewGuid();
            Guid jobRev2 = Guid.NewGuid();
            Guid jobRev3 = Guid.NewGuid();
            Guid jobRev4 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                Jobs = new Dictionary<string, List<LibWorkInstructions.Structs.Job>>
                {
                    { "job1", new List<LibWorkInstructions.Structs.Job>
                    {
                        new LibWorkInstructions.Structs.Job { Id = "job1", Rev = jobRev1, Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 1, JobId = "job1" },
                            new LibWorkInstructions.Structs.Op { Id = 2, JobId = "job1" },
                            new LibWorkInstructions.Structs.Op { Id = 3, JobId = "job1" },
                        } },
                        new LibWorkInstructions.Structs.Job { Id = "job1", Rev = jobRev2, Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 4, JobId = "job1" },
                            new LibWorkInstructions.Structs.Op { Id = 5, JobId = "job1" },
                            new LibWorkInstructions.Structs.Op { Id = 6, JobId = "job1" },
                        } }
                    }
                    },
                    { "job2", new List<LibWorkInstructions.Structs.Job>
                    {
                        new LibWorkInstructions.Structs.Job { Id = "job2", Rev = jobRev3, Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 7, JobId = "job2" },
                            new LibWorkInstructions.Structs.Op { Id = 8, JobId = "job2" },
                            new LibWorkInstructions.Structs.Op { Id = 9, JobId = "job2" },
                        } },
                        new LibWorkInstructions.Structs.Job { Id = "job2", Rev = jobRev4, Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 10, JobId = "job2" },
                            new LibWorkInstructions.Structs.Op { Id = 11, JobId = "job2" },
                            new LibWorkInstructions.Structs.Op { Id = 12, JobId = "job2" },
                        } }
                    }
                    }
                },
                JobRevs = new List<Guid> { jobRev1, jobRev2, jobRev3, jobRev4 },
                JobRefToJobRevRefs = new Dictionary<string, List<Guid>>
                {
                    { "job1", new List<Guid> { jobRev1, jobRev2 } },
                    { "job2", new List<Guid> { jobRev3, jobRev4 } }
                },
                JobRevRefToOpRefs = new Dictionary<Guid, List<int>>
                {
                    { jobRev1, new List<int>{ 1, 2, 3 } },
                    { jobRev2, new List<int>{ 4, 5, 6 } },
                    { jobRev3, new List<int>{ 7, 8, 9 } },
                    { jobRev4, new List<int>{ 10, 11, 12 } },
                },
                Ops = new Dictionary<int, LibWorkInstructions.Structs.Op>
                {
                    {1, new LibWorkInstructions.Structs.Op { Id = 1, JobId = "job1"} },
                    {2, new LibWorkInstructions.Structs.Op { Id = 2, JobId = "job1"} },
                    {3, new LibWorkInstructions.Structs.Op { Id = 3, JobId = "job1"} },
                    {4, new LibWorkInstructions.Structs.Op { Id = 4, JobId = "job1"} },
                    {5, new LibWorkInstructions.Structs.Op { Id = 5, JobId = "job1"} },
                    {6, new LibWorkInstructions.Structs.Op { Id = 6, JobId = "job1"} },
                    {7, new LibWorkInstructions.Structs.Op { Id = 7, JobId = "job2"} },
                    {8, new LibWorkInstructions.Structs.Op { Id = 8, JobId = "job2"} },
                    {9, new LibWorkInstructions.Structs.Op { Id = 9, JobId = "job2"} },
                    {10, new LibWorkInstructions.Structs.Op { Id = 10, JobId = "job2"} },
                    {11, new LibWorkInstructions.Structs.Op { Id = 11, JobId = "job2"} },
                    {12, new LibWorkInstructions.Structs.Op { Id = 12, JobId = "job2"} },
                }
            };
            n.DataImport(sampleData);
            n.MergeJobRevsBasedOnJobOps(jobRev1, jobRev2);
            var dbPostMerge = n.DataExport();
            Assert.True(dbPostMerge.Jobs["job1"][0].Ops.Count == 6);
            Assert.True(dbPostMerge.Jobs["job1"][1].Ops.Count == 6);
            Assert.True(dbPostMerge.JobRevRefToOpRefs[jobRev1].Count == 6);
            Assert.True(dbPostMerge.JobRevRefToOpRefs[jobRev2].Count == 6);
        }

        [Test]
        public void TestCloneJobRevBasedOnJobOpsAdditive()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid jobRev1 = Guid.NewGuid();
            Guid jobRev2 = Guid.NewGuid();
            Guid jobRev3 = Guid.NewGuid();
            Guid jobRev4 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                Jobs = new Dictionary<string, List<LibWorkInstructions.Structs.Job>>
                {
                    { "job1", new List<LibWorkInstructions.Structs.Job>
                    {
                        new LibWorkInstructions.Structs.Job { Id = "job1", Rev = jobRev1, Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 1, JobId = "job1" },
                            new LibWorkInstructions.Structs.Op { Id = 2, JobId = "job1" },
                            new LibWorkInstructions.Structs.Op { Id = 3, JobId = "job1" },
                        } },
                        new LibWorkInstructions.Structs.Job { Id = "job1", Rev = jobRev2, Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 4, JobId = "job1" },
                            new LibWorkInstructions.Structs.Op { Id = 5, JobId = "job1" },
                            new LibWorkInstructions.Structs.Op { Id = 6, JobId = "job1" },
                        } }
                    }
                    },
                    { "job2", new List<LibWorkInstructions.Structs.Job>
                    {
                        new LibWorkInstructions.Structs.Job { Id = "job2", Rev = jobRev3, Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 7, JobId = "job2" },
                            new LibWorkInstructions.Structs.Op { Id = 8, JobId = "job2" },
                            new LibWorkInstructions.Structs.Op { Id = 9, JobId = "job2" },
                        } },
                        new LibWorkInstructions.Structs.Job { Id = "job2", Rev = jobRev4, Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 10, JobId = "job2" },
                            new LibWorkInstructions.Structs.Op { Id = 11, JobId = "job2" },
                            new LibWorkInstructions.Structs.Op { Id = 12, JobId = "job2" },
                        } }
                    }
                    }
                },
                JobRevs = new List<Guid> { jobRev1, jobRev2, jobRev3, jobRev4 },
                JobRefToJobRevRefs = new Dictionary<string, List<Guid>>
                {
                    { "job1", new List<Guid> { jobRev1, jobRev2 } },
                    { "job2", new List<Guid> { jobRev3, jobRev4 } }
                },
                JobRevRefToOpRefs = new Dictionary<Guid, List<int>>
                {
                    { jobRev1, new List<int>{ 1, 2, 3 } },
                    { jobRev2, new List<int>{ 4, 5, 6 } },
                    { jobRev3, new List<int>{ 7, 8, 9 } },
                    { jobRev4, new List<int>{ 10, 11, 12 } },
                },
                Ops = new Dictionary<int, LibWorkInstructions.Structs.Op>
                {
                    {1, new LibWorkInstructions.Structs.Op { Id = 1, JobId = "job1"} },
                    {2, new LibWorkInstructions.Structs.Op { Id = 2, JobId = "job1"} },
                    {3, new LibWorkInstructions.Structs.Op { Id = 3, JobId = "job1"} },
                    {4, new LibWorkInstructions.Structs.Op { Id = 4, JobId = "job1"} },
                    {5, new LibWorkInstructions.Structs.Op { Id = 5, JobId = "job1"} },
                    {6, new LibWorkInstructions.Structs.Op { Id = 6, JobId = "job1"} },
                    {7, new LibWorkInstructions.Structs.Op { Id = 7, JobId = "job2"} },
                    {8, new LibWorkInstructions.Structs.Op { Id = 8, JobId = "job2"} },
                    {9, new LibWorkInstructions.Structs.Op { Id = 9, JobId = "job2"} },
                    {10, new LibWorkInstructions.Structs.Op { Id = 10, JobId = "job2"} },
                    {11, new LibWorkInstructions.Structs.Op { Id = 11, JobId = "job2"} },
                    {12, new LibWorkInstructions.Structs.Op { Id = 12, JobId = "job2"} },
                }
            };
            n.DataImport(sampleData);
            n.CloneJobRevBasedOnJobOps(jobRev3, jobRev4, true);
            var dbPostClone = n.DataExport();
            Assert.True(dbPostClone.Jobs["job2"][1].Ops.Count == 6);
            Assert.True(dbPostClone.JobRevRefToOpRefs[jobRev4].Count == 6);
        }

        [Test]
        public void TestCloneJobRevBasedOnJobOpsNotAdditive()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid jobRev1 = Guid.NewGuid();
            Guid jobRev2 = Guid.NewGuid();
            Guid jobRev3 = Guid.NewGuid();
            Guid jobRev4 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                Jobs = new Dictionary<string, List<LibWorkInstructions.Structs.Job>>
                {
                    { "job1", new List<LibWorkInstructions.Structs.Job>
                    {
                        new LibWorkInstructions.Structs.Job { Id = "job1", Rev = jobRev1, Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 1, JobId = "job1" },
                            new LibWorkInstructions.Structs.Op { Id = 2, JobId = "job1" },
                            new LibWorkInstructions.Structs.Op { Id = 3, JobId = "job1" },
                        } },
                        new LibWorkInstructions.Structs.Job { Id = "job1", Rev = jobRev2, Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 4, JobId = "job1" },
                            new LibWorkInstructions.Structs.Op { Id = 5, JobId = "job1" },
                            new LibWorkInstructions.Structs.Op { Id = 6, JobId = "job1" },
                        } }
                    }
                    },
                    { "job2", new List<LibWorkInstructions.Structs.Job>
                    {
                        new LibWorkInstructions.Structs.Job { Id = "job2", Rev = jobRev3, Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 7, JobId = "job2" },
                            new LibWorkInstructions.Structs.Op { Id = 8, JobId = "job2" },
                            new LibWorkInstructions.Structs.Op { Id = 9, JobId = "job2" },
                        } },
                        new LibWorkInstructions.Structs.Job { Id = "job2", Rev = jobRev4, Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op { Id = 10, JobId = "job2" },
                            new LibWorkInstructions.Structs.Op { Id = 11, JobId = "job2" },
                            new LibWorkInstructions.Structs.Op { Id = 12, JobId = "job2" },
                        } }
                    }
                    }
                },
                JobRevs = new List<Guid> { jobRev1, jobRev2, jobRev3, jobRev4 },
                JobRefToJobRevRefs = new Dictionary<string, List<Guid>>
                {
                    { "job1", new List<Guid> { jobRev1, jobRev2 } },
                    { "job2", new List<Guid> { jobRev3, jobRev4 } }
                },
                JobRevRefToOpRefs = new Dictionary<Guid, List<int>>
                {
                    { jobRev1, new List<int>{ 1, 2, 3 } },
                    { jobRev2, new List<int>{ 4, 5, 6 } },
                    { jobRev3, new List<int>{ 7, 8, 9 } },
                    { jobRev4, new List<int>{ 10, 11, 12 } },
                },
                Ops = new Dictionary<int, LibWorkInstructions.Structs.Op>
                {
                    {1, new LibWorkInstructions.Structs.Op { Id = 1, JobId = "job1"} },
                    {2, new LibWorkInstructions.Structs.Op { Id = 2, JobId = "job1"} },
                    {3, new LibWorkInstructions.Structs.Op { Id = 3, JobId = "job1"} },
                    {4, new LibWorkInstructions.Structs.Op { Id = 4, JobId = "job1"} },
                    {5, new LibWorkInstructions.Structs.Op { Id = 5, JobId = "job1"} },
                    {6, new LibWorkInstructions.Structs.Op { Id = 6, JobId = "job1"} },
                    {7, new LibWorkInstructions.Structs.Op { Id = 7, JobId = "job2"} },
                    {8, new LibWorkInstructions.Structs.Op { Id = 8, JobId = "job2"} },
                    {9, new LibWorkInstructions.Structs.Op { Id = 9, JobId = "job2"} },
                    {10, new LibWorkInstructions.Structs.Op { Id = 10, JobId = "job2"} },
                    {11, new LibWorkInstructions.Structs.Op { Id = 11, JobId = "job2"} },
                    {12, new LibWorkInstructions.Structs.Op { Id = 12, JobId = "job2"} },
                }
            };
            n.DataImport(sampleData);
            n.CloneJobRevBasedOnJobOps(jobRev3, jobRev4, false);
            var dbPostClone = n.DataExport();
            Assert.True(dbPostClone.Jobs["job2"][1].Ops.Count == 3);
            Assert.True(dbPostClone.JobRevRefToOpRefs[jobRev4].Count == 3);
        }

        [Test]
        public void TestLinkJobOpAndOpSpecRev()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid opSpecRev1 = Guid.NewGuid();
            Guid opSpecRev2 = Guid.NewGuid();
            Guid opSpecRev3 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                OpSpecRevs = new List<Guid> { opSpecRev1, opSpecRev2, opSpecRev3 },
                OpSpecRevRefToOpRefs = new Dictionary<Guid, List<int>>
                {
                    {opSpecRev1, new List<int>{1, 2, 3} },
                    {opSpecRev2, new List<int>{4, 2, 6} },
                    {opSpecRev3, new List<int>{1, 3, 5} },
                },
                OpRefToOpSpecRevRefs = new Dictionary<int, List<Guid>>
                {
                    {1, new List<Guid>{ opSpecRev1, opSpecRev3} },
                    {2, new List<Guid>{ opSpecRev1, opSpecRev2} },
                    {3, new List<Guid>{ opSpecRev1, opSpecRev3} },
                    {4, new List<Guid>{ opSpecRev2 } },
                    {5, new List<Guid>{ opSpecRev3 } },
                    {6, new List<Guid>{ opSpecRev2 } }
                }
            };
            n.DataImport(sampleData);
            n.LinkJobOpAndOpSpecRev(1, opSpecRev2);
            var dbPostLink = n.DataExport();
            Assert.True(dbPostLink.OpSpecRevRefToOpRefs[opSpecRev2].Contains(1));
            Assert.True(dbPostLink.OpRefToOpSpecRevRefs[1].Contains(opSpecRev2));
        }

        [Test]
        public void TestUnlinkJobOpAndOpSpecRev()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid opSpecRev1 = Guid.NewGuid();
            Guid opSpecRev2 = Guid.NewGuid();
            Guid opSpecRev3 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                OpSpecRevs = new List<Guid> { opSpecRev1, opSpecRev2, opSpecRev3 },
                OpSpecRevRefToOpRefs = new Dictionary<Guid, List<int>>
                {
                    {opSpecRev1, new List<int>{1, 2, 3} },
                    {opSpecRev2, new List<int>{4, 2, 6} },
                    {opSpecRev3, new List<int>{1, 3, 5} },
                },
                OpRefToOpSpecRevRefs = new Dictionary<int, List<Guid>>
                {
                    {1, new List<Guid>{ opSpecRev1, opSpecRev3} },
                    {2, new List<Guid>{ opSpecRev1, opSpecRev2} },
                    {3, new List<Guid>{ opSpecRev1, opSpecRev3} },
                    {4, new List<Guid>{ opSpecRev2 } },
                    {5, new List<Guid>{ opSpecRev3 } },
                    {6, new List<Guid>{ opSpecRev2 } }
                }
            };
            n.DataImport(sampleData);
            n.UnlinkJobOpAndOpSpecRev(1, opSpecRev1);
            var dbPostLink = n.DataExport();
            Assert.False(dbPostLink.OpSpecRevRefToOpRefs[opSpecRev1].Contains(1));
            Assert.False(dbPostLink.OpRefToOpSpecRevRefs[1].Contains(opSpecRev1));
        }

        [Test]
        public void TestMergeOpSpecRevsBasedOnJobOps()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid opSpecRev1 = Guid.NewGuid();
            Guid opSpecRev2 = Guid.NewGuid();
            Guid opSpecRev3 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                Ops = new Dictionary<int, LibWorkInstructions.Structs.Op>
                {
                    {1, new LibWorkInstructions.Structs.Op{Id = 1} },
                    {2, new LibWorkInstructions.Structs.Op{Id = 2} },
                    {3, new LibWorkInstructions.Structs.Op{Id = 3} },
                    {4, new LibWorkInstructions.Structs.Op{Id = 4} },
                    {5, new LibWorkInstructions.Structs.Op{Id = 5} },
                    {6, new LibWorkInstructions.Structs.Op{Id = 6} }
                },
                OpSpecRevs = new List<Guid> { opSpecRev1, opSpecRev2, opSpecRev3 },
                OpSpecRevRefToOpRefs = new Dictionary<Guid, List<int>>
                {
                    {opSpecRev1, new List<int>{1, 2, 3} },
                    {opSpecRev2, new List<int>{4, 2, 6} },
                    {opSpecRev3, new List<int>{1, 3, 5} },
                }
            };
            n.DataImport(sampleData);
            n.MergeOpSpecRevsBasedOnJobOps(opSpecRev1, opSpecRev2);
            var dbPostMerge = n.DataExport();
            Assert.True(dbPostMerge.OpSpecRevRefToOpRefs[opSpecRev1].Count == 5);
            Assert.True(dbPostMerge.OpSpecRevRefToOpRefs[opSpecRev2].Count == 5);
        }

        [Test]
        public void TestCloneOpSpecRevBasedOnJobOpsAdditive()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid opSpecRev1 = Guid.NewGuid();
            Guid opSpecRev2 = Guid.NewGuid();
            Guid opSpecRev3 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                Ops = new Dictionary<int, LibWorkInstructions.Structs.Op>
                {
                    {1, new LibWorkInstructions.Structs.Op{Id = 1} },
                    {2, new LibWorkInstructions.Structs.Op{Id = 2} },
                    {3, new LibWorkInstructions.Structs.Op{Id = 3} },
                    {4, new LibWorkInstructions.Structs.Op{Id = 4} },
                    {5, new LibWorkInstructions.Structs.Op{Id = 5} },
                    {6, new LibWorkInstructions.Structs.Op{Id = 6} }
                },
                OpSpecRevs = new List<Guid> { opSpecRev1, opSpecRev2, opSpecRev3 },
                OpSpecRevRefToOpRefs = new Dictionary<Guid, List<int>>
                {
                    {opSpecRev1, new List<int>{1, 2, 3} },
                    {opSpecRev2, new List<int>{4, 2, 6} },
                    {opSpecRev3, new List<int>{1, 3, 5} },
                }
            };
            n.DataImport(sampleData);
            n.CloneOpSpecRevBasedOnJobOps(opSpecRev1, opSpecRev2, true);
            var dbPostClone = n.DataExport();
            Assert.True(dbPostClone.OpSpecRevRefToOpRefs[opSpecRev2].Count == 5);
        }

        [Test]
        public void TestCloneOpSpecRevBasedOnJobOpsNotAdditive()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid opSpecRev1 = Guid.NewGuid();
            Guid opSpecRev2 = Guid.NewGuid();
            Guid opSpecRev3 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                Ops = new Dictionary<int, LibWorkInstructions.Structs.Op>
                {
                    {1, new LibWorkInstructions.Structs.Op{Id = 1} },
                    {2, new LibWorkInstructions.Structs.Op{Id = 2} },
                    {3, new LibWorkInstructions.Structs.Op{Id = 3} },
                    {4, new LibWorkInstructions.Structs.Op{Id = 4} },
                    {5, new LibWorkInstructions.Structs.Op{Id = 5} },
                    {6, new LibWorkInstructions.Structs.Op{Id = 6} }
                },
                OpSpecRevs = new List<Guid> { opSpecRev1, opSpecRev2, opSpecRev3 },
                OpSpecRevRefToOpRefs = new Dictionary<Guid, List<int>>
                {
                    {opSpecRev1, new List<int>{1, 2, 3} },
                    {opSpecRev2, new List<int>{4, 2, 6} },
                    {opSpecRev3, new List<int>{1, 3, 5} },
                }
            };
            n.DataImport(sampleData);
            n.CloneOpSpecRevBasedOnJobOps(opSpecRev1, opSpecRev2, false);
            var dbPostClone = n.DataExport();
            Assert.True(dbPostClone.OpSpecRevRefToOpRefs[opSpecRev2].Count == 3);
        }

        [Test]
        public void TestCloneJobRevBasedOnQualityClauseRevsAdditive()
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
            Guid jobRev1 = Guid.NewGuid();
            Guid jobRev2 = Guid.NewGuid();
            Guid jobRev3 = Guid.NewGuid();
            Guid jobRev4 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                Jobs = new Dictionary<string, List<LibWorkInstructions.Structs.Job>>
                {
                    { "job1", new List<LibWorkInstructions.Structs.Job> {
                        new LibWorkInstructions.Structs.Job { Rev = jobRev1, QualityClauses = new List<LibWorkInstructions.Structs.QualityClause> {
                            new LibWorkInstructions.Structs.QualityClause { Id = clauseId1 },
                            new LibWorkInstructions.Structs.QualityClause { Id = clauseId2 } }, },
                        new LibWorkInstructions.Structs.Job { Rev = jobRev2, QualityClauses = new List<LibWorkInstructions.Structs.QualityClause> {
                            new LibWorkInstructions.Structs.QualityClause { Id = clauseId3 },
                            new LibWorkInstructions.Structs.QualityClause { Id = clauseId4 } }, } } },
                    { "job2", new List<LibWorkInstructions.Structs.Job> {
                        new LibWorkInstructions.Structs.Job { Rev = jobRev3, QualityClauses = new List<LibWorkInstructions.Structs.QualityClause> {
                            new LibWorkInstructions.Structs.QualityClause { Id = clauseId5 },
                            new LibWorkInstructions.Structs.QualityClause { Id = clauseId6 } }, },
                        new LibWorkInstructions.Structs.Job { Rev = jobRev4, QualityClauses = new List<LibWorkInstructions.Structs.QualityClause> {
                            new LibWorkInstructions.Structs.QualityClause { Id = clauseId7 },
                            new LibWorkInstructions.Structs.QualityClause { Id = clauseId8 } }, } } },
                },
                JobRevs = new List<Guid> { jobRev1, jobRev2, jobRev3, jobRev4 },
                QualityClauseRevs = new List<Guid> { clauseId1, clauseId2, clauseId3, clauseId4, clauseId5, clauseId6, clauseId7, clauseId8 },
                JobRevRefToQualityClauseRevRefs = new Dictionary<Guid, List<Guid>>
                {
                    {jobRev1, new List<Guid> {clauseId1, clauseId2} },
                    {jobRev2, new List<Guid> {clauseId3, clauseId4} },
                    {jobRev3, new List<Guid> {clauseId5, clauseId6} },
                    {jobRev4, new List<Guid> {clauseId7, clauseId8} },
                },
                QualityClauses = new Dictionary<Guid, List<LibWorkInstructions.Structs.QualityClause>>
                {
                    { groupId1, new List<LibWorkInstructions.Structs.QualityClause> {
                            new LibWorkInstructions.Structs.QualityClause { Id = clauseId1, IdRevGroup = groupId1},
                            new LibWorkInstructions.Structs.QualityClause { Id = clauseId2, IdRevGroup = groupId1},
                            new LibWorkInstructions.Structs.QualityClause { Id = clauseId3, IdRevGroup = groupId1},
                            new LibWorkInstructions.Structs.QualityClause { Id = clauseId4, IdRevGroup = groupId1},}
                    },
                    { groupId2, new List<LibWorkInstructions.Structs.QualityClause>
                    {
                        new LibWorkInstructions.Structs.QualityClause { Id = clauseId5, IdRevGroup = groupId2},
                        new LibWorkInstructions.Structs.QualityClause { Id = clauseId6, IdRevGroup = groupId2},
                        new LibWorkInstructions.Structs.QualityClause { Id = clauseId7, IdRevGroup = groupId2},
                        new LibWorkInstructions.Structs.QualityClause { Id = clauseId8, IdRevGroup = groupId2},
                    } }
                },
            };
            n.DataImport(sampleData);
            n.CloneJobRevBasedOnQualityClauseRevs(jobRev1, jobRev4, true);
            var dbPostClone = n.DataExport();
            Assert.True(dbPostClone.Jobs["job2"][1].QualityClauses.Count == 4);
            Assert.True(dbPostClone.Jobs["job1"][0].QualityClauses.All(y => dbPostClone.Jobs["job2"][1].QualityClauses.Any(x => x.Id == y.Id)));
        }

        [Test]
        public void TestCloneJobRevBasedOnQualityClauseRevsNotAdditive()
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
            Guid jobRev1 = Guid.NewGuid();
            Guid jobRev2 = Guid.NewGuid();
            Guid jobRev3 = Guid.NewGuid();
            Guid jobRev4 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                Jobs = new Dictionary<string, List<LibWorkInstructions.Structs.Job>>
                {
                    {"job1", new List<LibWorkInstructions.Structs.Job>{
                        new LibWorkInstructions.Structs.Job {Rev = jobRev1 , QualityClauses = new List<LibWorkInstructions.Structs.QualityClause> {
                        new LibWorkInstructions.Structs.QualityClause {Id = clauseId1},
                        new LibWorkInstructions.Structs.QualityClause {Id = clauseId2}}, },
                        new LibWorkInstructions.Structs.Job {Rev = jobRev2 , QualityClauses = new List<LibWorkInstructions.Structs.QualityClause> {
                        new LibWorkInstructions.Structs.QualityClause {Id = clauseId3},
                        new LibWorkInstructions.Structs.QualityClause {Id = clauseId4}}, } } },
                    {"job2", new List<LibWorkInstructions.Structs.Job>{
                        new LibWorkInstructions.Structs.Job {Rev = jobRev3 , QualityClauses = new List<LibWorkInstructions.Structs.QualityClause> {
                        new LibWorkInstructions.Structs.QualityClause {Id = clauseId5},
                        new LibWorkInstructions.Structs.QualityClause {Id = clauseId6}}, },
                        new LibWorkInstructions.Structs.Job {Rev = jobRev4 , QualityClauses = new List<LibWorkInstructions.Structs.QualityClause> {
                        new LibWorkInstructions.Structs.QualityClause {Id = clauseId7},
                        new LibWorkInstructions.Structs.QualityClause {Id = clauseId8}}, } } },
                },
                JobRevs = new List<Guid> { jobRev1, jobRev2, jobRev3, jobRev4},
                JobRevRefToQualityClauseRevRefs = new Dictionary<Guid, List<Guid>>
                {
                    {jobRev1, new List<Guid> {clauseId1, clauseId2} },
                    {jobRev2, new List<Guid> {clauseId3, clauseId4} },
                    {jobRev3, new List<Guid> {clauseId5, clauseId6} },
                    {jobRev4, new List<Guid> {clauseId7, clauseId8} },
                }
            };
            n.DataImport(sampleData);
            n.CloneJobRevBasedOnQualityClauseRevs(jobRev1, jobRev4, false);
            var dbPostClone = n.DataExport();
            Assert.True(dbPostClone.Jobs["job2"][1].QualityClauses.SequenceEqual(dbPostClone.Jobs["job1"][0].QualityClauses));
            Assert.True(dbPostClone.Jobs["job2"][1].QualityClauses.Count == 2);
        }

        [Test]
        public void TestMergeQualityClauses()
        {
            var n = new LibWorkInstructions.BusinessLogic();
            Guid groupId1 = Guid.NewGuid();
            Guid groupId2 = Guid.NewGuid();
            Guid clauseId1 = Guid.NewGuid();
            Guid clauseId2 = Guid.NewGuid();
            Guid clauseId3 = Guid.NewGuid();
            Guid clauseId4 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                QualityClauses = new Dictionary<Guid, List<LibWorkInstructions.Structs.QualityClause>>
                {
                    { groupId1, new List<LibWorkInstructions.Structs.QualityClause> {
                            new LibWorkInstructions.Structs.QualityClause { Id = clauseId1, IdRevGroup = groupId1},
                            new LibWorkInstructions.Structs.QualityClause { Id = clauseId2, IdRevGroup = groupId1} }
                    },
                    { groupId2, new List<LibWorkInstructions.Structs.QualityClause>
                    {
                        new LibWorkInstructions.Structs.QualityClause { Id = clauseId3, IdRevGroup = groupId2},
                        new LibWorkInstructions.Structs.QualityClause { Id = clauseId4, IdRevGroup = groupId2}
                    } }
                },
                QualityClauseRefToQualityClauseRevRefs = new Dictionary<Guid, List<Guid>>
                {
                    {groupId1, new List<Guid>{clauseId1, clauseId2} },
                    {groupId2, new List<Guid>{clauseId3, clauseId4} }
                },
                QualityClauseRevs = new List<Guid> { clauseId1, clauseId2, clauseId3, clauseId4 }
            };
            n.DataImport(sampleData);
            n.MergeQualityClauses(groupId1, groupId2);
            var dbPostMerge = n.DataExport();
            Assert.True(dbPostMerge.QualityClauses[groupId1].Count == 4);
            Assert.True(dbPostMerge.QualityClauses[groupId2].Count == 4);
            Assert.True(dbPostMerge.QualityClauseRefToQualityClauseRevRefs[groupId1].Count == 4);
            Assert.True(dbPostMerge.QualityClauseRefToQualityClauseRevRefs[groupId2].Count == 4);
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
                },
                QualityClauseRevRefToJobRevRefs = new Dictionary<Guid, List<Guid>>
                {
                    {clause1, new List<Guid>() },
                    {clause2, new List<Guid>() },
                    {clause3, new List<Guid>() },
                    {clause4, new List<Guid>() },
                    {clause5, new List<Guid>() },
                    {clause6, new List<Guid>() }
                }
            };
            n.DataImport(sampleData);
            n.CreateQualityClauseRev(groupId1, clause4);
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
                },
                QualityClauseRevs = new List<Guid> { clause1, clause2, clause3, clause4, clause5, clause6}
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
            Guid jobRev1 = Guid.NewGuid();
            Guid jobRev2 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                Jobs = new Dictionary<string, List<LibWorkInstructions.Structs.Job>>
                {
                    {"job1", new List<LibWorkInstructions.Structs.Job> {
                        new LibWorkInstructions.Structs.Job {Id = "job1", Rev = jobRev1, Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op {Id = 1},
                            new LibWorkInstructions.Structs.Op {Id = 3}
                        } },
                        new LibWorkInstructions.Structs.Job {Id = "job1", Rev = jobRev2, Ops = new List<LibWorkInstructions.Structs.Op> {
                            new LibWorkInstructions.Structs.Op {Id = 2},
                            new LibWorkInstructions.Structs.Op {Id = 3}
                        } },
                     } }
                },
                JobRevRefToOpRefs = new Dictionary<Guid, List<int>>
                {
                    {jobRev1, new List<int>{1, 3}},
                    {jobRev2, new List<int>{2, 3}}
                },
                Ops = new Dictionary<int, LibWorkInstructions.Structs.Op>
                {
                    {1, new LibWorkInstructions.Structs.Op { Id = 1, JobId = "job1"} },
                    {2, new LibWorkInstructions.Structs.Op { Id = 2, JobId = "job1"} },
                    {3, new LibWorkInstructions.Structs.Op { Id = 3, JobId = "job1"} },
                    {4, new LibWorkInstructions.Structs.Op { Id = 4, JobId = "job1"} },
                    {5, new LibWorkInstructions.Structs.Op { Id = 5, JobId = "job1"} },
                    {6, new LibWorkInstructions.Structs.Op { Id = 6, JobId = "job1"} },
                    {7, new LibWorkInstructions.Structs.Op { Id = 7, JobId = "job2"} },
                    {8, new LibWorkInstructions.Structs.Op { Id = 8, JobId = "job2"} },
                    {9, new LibWorkInstructions.Structs.Op { Id = 9, JobId = "job2"} },
                    {10, new LibWorkInstructions.Structs.Op { Id = 10, JobId = "job2"} },
                    {11, new LibWorkInstructions.Structs.Op { Id = 11, JobId = "job2"} },
                    {12, new LibWorkInstructions.Structs.Op { Id = 12, JobId = "job2"} },
                },
                JobRevs = new List<Guid> { jobRev1, jobRev2}
            };
            n.DataImport(sampleData);
            n.DeleteJobOp(jobRev2, 3);
            var dbPostDelete = n.DataExport();
            Assert.True(dbPostDelete.Jobs["job1"][1].Ops.Count == 1);
            Assert.True(dbPostDelete.Jobs["job1"][1].Ops[0].Id == 2);
            Assert.True(dbPostDelete.JobRevRefToOpRefs[jobRev2].Count == 1);
            Assert.True(dbPostDelete.JobRevRefToOpRefs[jobRev1].Count == 2);
        }

        [Test]
        public void TestPullQualityClausesFromJob()
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
            Guid jobRev1 = Guid.NewGuid();
            Guid jobRev2 = Guid.NewGuid();
            Guid jobRev3 = Guid.NewGuid();
            Guid jobRev4 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                Jobs = new Dictionary<string, List<LibWorkInstructions.Structs.Job>>
                {
                    {"job1-cust1", new List<LibWorkInstructions.Structs.Job>{
                        new LibWorkInstructions.Structs.Job {RevPlan = "plan1", RevSeq = 0, Id = "job1", Rev = jobRev1 , QualityClauses = new List<LibWorkInstructions.Structs.QualityClause> {
                            new LibWorkInstructions.Structs.QualityClause {Id = clauseId1},
                            new LibWorkInstructions.Structs.QualityClause {Id = clauseId2}}, Ops = new List<LibWorkInstructions.Structs.Op> {
                                    new LibWorkInstructions.Structs.Op { Id = 1, JobId = "job1"},
                                    new LibWorkInstructions.Structs.Op { Id = 2, JobId = "job1"},
                                    new LibWorkInstructions.Structs.Op { Id = 3, JobId = "job1"},}},
                        new LibWorkInstructions.Structs.Job {RevPlan = "plan2", RevSeq = 1, Id = "job1", Rev = jobRev2 , QualityClauses = new List<LibWorkInstructions.Structs.QualityClause> {
                            new LibWorkInstructions.Structs.QualityClause {Id = clauseId3},
                            new LibWorkInstructions.Structs.QualityClause {Id = clauseId4}}, Ops = new List<LibWorkInstructions.Structs.Op> {
                                    new LibWorkInstructions.Structs.Op { Id = 4, JobId = "job1"},
                                    new LibWorkInstructions.Structs.Op { Id = 5, JobId = "job1"},
                                    new LibWorkInstructions.Structs.Op { Id = 6, JobId = "job1"},} } } },
                    {"job2-cust2", new List<LibWorkInstructions.Structs.Job>{
                        new LibWorkInstructions.Structs.Job {RevSeq = 0, Id = "job2", Rev = jobRev3 , QualityClauses = new List<LibWorkInstructions.Structs.QualityClause> {
                        new LibWorkInstructions.Structs.QualityClause {Id = clauseId5},
                        new LibWorkInstructions.Structs.QualityClause {Id = clauseId6}}, Ops = new List<LibWorkInstructions.Structs.Op> {
                                    new LibWorkInstructions.Structs.Op { Id = 7, JobId = "job2"},
                                    new LibWorkInstructions.Structs.Op { Id = 8, JobId = "job2"},
                                    new LibWorkInstructions.Structs.Op { Id = 9, JobId = "job2"},}},
                        new LibWorkInstructions.Structs.Job {RevSeq = 1, Id = "job2", Rev = jobRev4 , QualityClauses = new List<LibWorkInstructions.Structs.QualityClause> {
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
            var qualityClauses = n.PullQualityClausesFromJob("job1", "cust1", "plan1");
            Assert.True(qualityClauses[0].Id == clauseId1);
            Assert.True(qualityClauses[1].Id == clauseId2);
        }

        [Test]
        public void TestDisplayPriorRevisionsOfWorkInstruction()
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
                WorkInstructionRefToWorkInstructionRevRefs = new Dictionary<Guid, List<Guid>>
                {
                    { groupId1, new List<Guid> { workId1, workId2 } },
                    { groupId2, new List<Guid> { workId3, workId4 } },
                },
                WorkInstructions = new Dictionary<Guid, List<LibWorkInstructions.Structs.WorkInstruction>>
                {
                    { groupId1, new List<LibWorkInstructions.Structs.WorkInstruction> {
                        new LibWorkInstructions.Structs.WorkInstruction {
                            Id = workId1, IdRevGroup = groupId1, OpId = 5 },
                        new LibWorkInstructions.Structs.WorkInstruction {
                            Id = workId2, IdRevGroup = groupId1, OpId = 5 } } },
                    { groupId2, new List<LibWorkInstructions.Structs.WorkInstruction> {
                        new LibWorkInstructions.Structs.WorkInstruction {
                            Id = workId3, IdRevGroup = groupId2, OpId = 8 },
                        new LibWorkInstructions.Structs.WorkInstruction {
                            Id = workId4, IdRevGroup = groupId2, OpId = 8 } } },
                },
            };
            n.DataImport(sampleData);
            var workInstructionList = n.DisplayPriorRevisionsOfWorkInstruction(groupId1);
            Assert.True(workInstructionList[0].Id == workId1);
            Assert.True(workInstructionList[1].Id == workId2);
        }

        [Test]
        public void TestDisplayPriorRevisionsOfQualityClauses()
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
                },
                QualityClauseRefToQualityClauseRevRefs = new Dictionary<Guid, List<Guid>>
                {
                    {groupId1, new List<Guid>{clause1, clause4, clause3} },
                    {groupId2, new List<Guid>{clause2, clause5, clause6} }
                }
            };
            n.DataImport(sampleData);
            var qualityClauseList = n.DisplayPriorRevisionsOfQualityClauses(groupId1);
            Assert.True(qualityClauseList.SequenceEqual(new List<LibWorkInstructions.Structs.QualityClause> { sampleClause1, sampleClause4, sampleClause3 }));
        }

        [Test]
        public void TestDisplayPriorRevisionsOfSpecs()
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
                },
                OpSpecRefToOpSpecRevRefs = new Dictionary<Guid, List<Guid>>
                {
                    {groupId1, new List<Guid>{ specId1, specId2} }
                }
            };

            n.DataImport(sampleData);
            var specList = n.DisplayPriorRevisionsOfSpecs(groupId1);
            Assert.True(specList[0].Id == specId1);
            Assert.True(specList[1].Id == specId2);
        }

        [Test]
        public void TestDisplayLatestRevisionOfWorkInstruction()
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
            Guid groupId3 = Guid.NewGuid();
            Guid groupId4 = Guid.NewGuid();
            Guid workId1 = Guid.NewGuid();
            Guid workId2 = Guid.NewGuid();
            Guid workId3 = Guid.NewGuid();
            Guid workId4 = Guid.NewGuid();
            Guid groupId5 = Guid.NewGuid();
            Guid specId1 = Guid.NewGuid();
            Guid specId2 = Guid.NewGuid();
            Guid jobRev1 = Guid.NewGuid();
            Guid jobRev2 = Guid.NewGuid();
            Guid jobRev3 = Guid.NewGuid();
            Guid jobRev4 = Guid.NewGuid();
            var sampleData = new LibWorkInstructions.BusinessLogic.MockDB
            {
                Jobs = new Dictionary<string, List<LibWorkInstructions.Structs.Job>>
                {
                    { "job1", new List<LibWorkInstructions.Structs.Job> {
                        new LibWorkInstructions.Structs.Job { RevPlan = "RevPlan", RevSeq = 0, Id = "job1", Rev = jobRev1, QualityClauses = new List<LibWorkInstructions.Structs.QualityClause> {
                            new LibWorkInstructions.Structs.QualityClause { Id = clauseId1 },
                            new LibWorkInstructions.Structs.QualityClause { Id = clauseId2 } }, Ops = new List<LibWorkInstructions.Structs.Op> {
                                new LibWorkInstructions.Structs.Op { Id = 1, JobId = "job1", OpService = "OpService" },
                                new LibWorkInstructions.Structs.Op { Id = 2, JobId = "job1" },
                                new LibWorkInstructions.Structs.Op { Id = 3, JobId = "job1" }, } },
                        new LibWorkInstructions.Structs.Job { RevSeq = 1, Id = "job1", Rev = jobRev2, QualityClauses = new List<LibWorkInstructions.Structs.QualityClause> {
                            new LibWorkInstructions.Structs.QualityClause { Id = clauseId3 },
                            new LibWorkInstructions.Structs.QualityClause { Id = clauseId4 } }, Ops = new List<LibWorkInstructions.Structs.Op> {
                                new LibWorkInstructions.Structs.Op { Id = 4, JobId = "job1" },
                                new LibWorkInstructions.Structs.Op { Id = 5, JobId = "job1" },
                                new LibWorkInstructions.Structs.Op { Id = 6, JobId = "job1" }, } } } },
                    { "job2", new List<LibWorkInstructions.Structs.Job> {
                        new LibWorkInstructions.Structs.Job { RevSeq = 0, Id = "job2", Rev = jobRev3, QualityClauses = new List<LibWorkInstructions.Structs.QualityClause> {
                            new LibWorkInstructions.Structs.QualityClause { Id = clauseId5 },
                            new LibWorkInstructions.Structs.QualityClause { Id = clauseId6 } }, Ops = new List<LibWorkInstructions.Structs.Op> {
                                new LibWorkInstructions.Structs.Op { Id = 7, JobId = "job2" },
                                new LibWorkInstructions.Structs.Op { Id = 8, JobId = "job2" },
                                new LibWorkInstructions.Structs.Op { Id = 9, JobId = "job2" }, } },
                        new LibWorkInstructions.Structs.Job { RevSeq = 1, Id = "job2", Rev = jobRev4, QualityClauses = new List<LibWorkInstructions.Structs.QualityClause> {
                            new LibWorkInstructions.Structs.QualityClause { Id = clauseId7 },
                            new LibWorkInstructions.Structs.QualityClause { Id = clauseId8 } }, Ops = new List<LibWorkInstructions.Structs.Op> {
                                new LibWorkInstructions.Structs.Op { Id = 10, JobId = "job2" },
                                new LibWorkInstructions.Structs.Op { Id = 11, JobId = "job2" },
                                new LibWorkInstructions.Structs.Op { Id = 12, JobId = "job2" }, } } } },
                },
                Ops = new Dictionary<int, LibWorkInstructions.Structs.Op>
                {
                    { 1, new LibWorkInstructions.Structs.Op { Id = 1, JobId = "job1", OpService = "OpService" } },
                    { 2, new LibWorkInstructions.Structs.Op { Id = 2, JobId = "job1" } },
                    { 3, new LibWorkInstructions.Structs.Op { Id = 3, JobId = "job1" } },
                    { 4, new LibWorkInstructions.Structs.Op { Id = 4, JobId = "job1" } },
                    { 5, new LibWorkInstructions.Structs.Op { Id = 5, JobId = "job1" } },
                    { 6, new LibWorkInstructions.Structs.Op { Id = 6, JobId = "job1" } },
                    { 7, new LibWorkInstructions.Structs.Op { Id = 7, JobId = "job2" } },
                    { 8, new LibWorkInstructions.Structs.Op { Id = 8, JobId = "job2" } },
                    { 9, new LibWorkInstructions.Structs.Op { Id = 9, JobId = "job2" } },
                    { 10, new LibWorkInstructions.Structs.Op { Id = 10, JobId = "job2" } },
                    { 11, new LibWorkInstructions.Structs.Op { Id = 11, JobId = "job2" } },
                    { 12, new LibWorkInstructions.Structs.Op { Id = 12, JobId = "job2" } }
                },
                QualityClauses = new Dictionary<Guid, List<LibWorkInstructions.Structs.QualityClause>>
                {
                    { groupId1, new List<LibWorkInstructions.Structs.QualityClause> {
                        new LibWorkInstructions.Structs.QualityClause { Id = clauseId1 } } },
                    { groupId2, new List<LibWorkInstructions.Structs.QualityClause> {
                        new LibWorkInstructions.Structs.QualityClause { Id = clauseId2 } } },
                },
                OpRefToWorkInstructionRef = new Dictionary<int, Guid>
                {
                    { 1, groupId3 }
                },
                OpRefToOpSpecRevRefs = new Dictionary<int, List<Guid>>
                {
                    { 1, new List<Guid> { specId1, specId2 } }
                },
                OpSpecs = new Dictionary<Guid, List<LibWorkInstructions.Structs.OpSpec>>
                {
                    { groupId5, new List<LibWorkInstructions.Structs.OpSpec>
                    {
                        new LibWorkInstructions.Structs.OpSpec { Id = specId1 },
                        new LibWorkInstructions.Structs.OpSpec { Id = specId2 }
                    } }
                },
                OpSpecRevs = new List<Guid> { specId1, specId2 },
                WorkInstructionRefToWorkInstructionRevRefs = new Dictionary<Guid, List<Guid>>
                {
                    {groupId3, new List<Guid> {workId1, workId2} }
                },
                WorkInstructions = new Dictionary<Guid, List<LibWorkInstructions.Structs.WorkInstruction>>
                {
                    { groupId3, new List<LibWorkInstructions.Structs.WorkInstruction> {
                        new LibWorkInstructions.Structs.WorkInstruction {
                            Id = workId1, IdRevGroup = groupId3 },
                        new LibWorkInstructions.Structs.WorkInstruction {
                            Id = workId2, IdRevGroup = groupId3 } } },
                    { groupId4, new List<LibWorkInstructions.Structs.WorkInstruction> {
                        new LibWorkInstructions.Structs.WorkInstruction {
                            Id = workId3, IdRevGroup = groupId4 },
                        new LibWorkInstructions.Structs.WorkInstruction {
                            Id = workId4, IdRevGroup = groupId4 } } },
                }
            };
            n.DataImport(sampleData);
            string[] data = n.DisplayLatestRevisionOfWorkInstruction("job1", "RevPlan", "OpService");
            Assert.True(JsonSerializer.Deserialize<LibWorkInstructions.Structs.WorkInstruction>(data[0]).Id == workId2);
            Assert.True(JsonSerializer.Deserialize<List<LibWorkInstructions.Structs.QualityClause>>(data[1])[0].Id == clauseId1);
            Assert.True(JsonSerializer.Deserialize<List<LibWorkInstructions.Structs.QualityClause>>(data[1])[1].Id == clauseId2);
            Assert.True(JsonSerializer.Deserialize<List<LibWorkInstructions.Structs.OpSpec>>(data[2])[0].Id == specId1);
            Assert.True(JsonSerializer.Deserialize<List<LibWorkInstructions.Structs.OpSpec>>(data[2])[1].Id == specId2);
        }
    }
}
