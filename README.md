# LibWorkInstructions

This repository and associated issue tracker are to contain artifacts from the Acutec+Allegheny group internship project
focused around implementing a C#/dotnetcore library handling the data modeling and business logic of Work Instructions.
Work Instructions is the system by which operators on the shop floor are relayed operational details managed primarily
by the engineering team.  Refer to the issue tracker for more in-depth coverage of the requirements gathering process.

# Using the Library
This portion of the README is dedicated to describing each method and how it should be used.  They are categorized into what kind of object they relate to.  For example, all methods referring to `WorkInstructions` will be written under the `WorkInstructions` section, and so on for each section.

## Jobs
### Description
- The `Job` object is the structure that contains information about everything else in the database
- Main overarching object that describes a certain "job".
### Methods
**CreateJob():**
  - **Parameters:**
    - string: jobId 
    - string: revCustomer 
    - string: revPlan
    - string: rev
  - **Description:**
    - Creates a new Job object to put in the database if it doesn't already exist.

**DeleteJob():**
  - **Parameters:**
    - string: jobId 
  - **Description:**
    - Removes the selected Job object from the database, if it exists.

**CloneJob():**
  - **Parameters:**
    - string: sourceJob
    - string: targetJob
    - bool: additive
  - **Description:**
    - Clone the content of the `sourceJob` into the `targetJob`, if they exist.
    - Data transfer behaves differently depending on the `additive` parameter.
      - Data will be added instead of overwritten if `additive` is true

**MergeJobs():**
  - **Parameters:**
    - string: jobId1
    - string: jobId2
  - **Description:**
    - Merges the selected `Job` objects together, if they both exist.

## Job Revisions (JobRevs)
### Description
Revisions for a specific Job.  Many Revisions can exist to one Job, but only one Job can be linked to one `JobRev`.

### Methods
**CreateJobRev():**
  - **Parameters:**
    - string: jobId
    - string: sourceJobRev
    - string: newJobRev 
  - **Description:**
    - Creates a `JobRev` based on which strings were provided, granted the given job exists, and this Revision doesn't already exist.

**CreateJobRev():**
  - **Parameters:**
    - Job: newJobRev 
  - **Description:**
    - Behaves the same as the previous `CreateJobRev` method, but this takes in a `Job` object as a parameter if it doesn't already exist.

**UpdateJobRev():**
  - **Parameters:**
    - Job: newJobRev 
  - **Description:**
    - Changes the specified `JobRev` to the parameter given, if it exists.
    - `JobRev` parameter should have the same id as the `JobRev` that is being updated.

**ActivateJobRev():**
  - **Parameters:**
    - string: jobId
    - string: jobRev 
  - **Description:**
    - Changes the status variable within the selected `jobRev` of a given `jobId` to  True, if they exist.

**DeactivateJobRev():**
  - **Parameters:**
    - string: jobId
    - string: jobRev 
  - **Description:**
    - Changes the status variable within the selected `jobRev` of a given `jobId` to False, if they exist.

**SplitJobRev():**
  - **Parameters:**
    - string: jobId
    - string: jobRev
    - string: newJobRev
  - **Description:**
    - Duplicate the selected Job and Job Revision into a new entity, if they both exist.

**CloneJobRevBasedOnJobOps()**
  - **Parameters:**
    - string: sourceJobRev
    - string: targetJobRev
    - bool: additive
  - **Description:**
    - Clone the content of the `sourceJobRev` into the `targetJobRev`, if they exist.
    - Data transfer behaves differently depending on the `additive` parameter.
      - Data will be added instead of overwritten if `additive` is true

**MergeJobRevBasedOnJobOps()**
  - **Parameters:**
    - string: jobRev1
    - string: jobRev2
  - **Description:**
    - Merge the content of `jobRev1` and `jobRev2` depending on their `JobOps`, if they exist.

### Links

**LinkJobOpAndJobRev():**
  - **Parameters:**
    - int:    opId
    - string: jobRev
  - **Description:**
    - Link selected `JobOp` to selected `JobRev`, if they both exist.

**UnlinkJobOpAndJobRev():**
  - **Parameters:**
    - int:    opId
    - string: jobRev
  - **Description:**
    - Unlink selected `JobOp` to selected `JobRev`, if they both exist.

## Job Operations (JobOps)

### Description
`Job Operations` contain information about the job itself, and what steps need to be taken to provide a finished part

### Methods

**CreateJobOp():**
  - **Parameters:**
    - Op: op
  - **Description:**
    - Creates a `JobOp` if it doesn't already exist.

**DeleteJobOp():**
  - **Parameters:**
    - string: jobRev
    - int: opId
  - **Description:**
    - Removes selected `JobOp` if it exists.

### Links

**LinkWorkInstructionToJobOp():**
  - **Parameters:**
    - guid: revGroup
    - int: opId
  - **Description:**
    - Link selected `WorkInstruction` to selected `JobOp`, if they exist.

**UnlinkWorkInstructionToJobOp():**
  - **Parameters:**
    - guid: revGroup
    - int: opId
  - **Description:**
    - Unlink selected `WorkInstruction` from selected `JobOp`, if they exist.


## Quality Clause Revisions (QualityClauseRevs)

### Description
Revision to a specific `QualityClause`, many `QualityClauseRevs` to a singular `QualityClause`

### Methods

**CreateQualityClauseRev():**
  - **Parameters:**
    - guid:   groupId
    - guid:   sourceClauseId
  - **Description:**
    - Create a `QualityClauseRev` and connect it to a `QualityClause` if it doesn't already exist.

**CreateQualityClauseRev():**
  - **Parameters:**
    - QualityClause: newClauseRev
  - **Description:**
    - Create `QualityClauseRev` with the `QualityClause` object parameter if it doesn't already exists.

**UpdateQualityClauseRev():**
  - **Parameters:**
    - QualityClause: newClauseRev
  - **Description:**
    - Change selected `QualityClauseRev` to the parameter `QualityClause` object if it exists.

**ActivateQualityClauseRev():**
  - **Parameters:**
    - guid: groupId
    - guid: qualityClauseRev
  - **Description:**
    - Changes the active status of the selected `QualityClauseRev` to True, if it exists.

**DeactivateQualityClauseRev():**
  - **Parameters:**
    - guid: groupId
    - guid: qualityClauseRev
  - **Description:**
    - Changes the active status of the selected `QualityClauseRev` to False, if it exists.

**SplitQualityClauseRev():**
  - **Parameters:**
    - guid: revGroup
    - guid: qualityClauseRev
  - **Description:**
    - Duplicate the selected `QualityClause` and `QualityClauseRev` into a new entity, if they both exist.

**CloneJobRevBasedOnQualityClauseRevs():**
  - **Parameters:**
    - string: sourceJobRev
    - string: targetJobRev
    - bool: additive
  - **Description:**
  - Clone the content of the `sourceJobRev` into the `targetJobRev`, if they exist.
    - Data transfer behaves differently depending on the `additive` parameter.
      - Data will be added instead of overwritten if `additive` is true

**MergeJobRevsBasedOnQualityClauseRevs():**
  - **Parameters:**
    - string: jobRev1
    - string: jobRev2
  - **Description:**
    - Merge the selected `JobRevs` into a new entity, if they both exist.

### Links

**LinkJobRevAndQualityClauseRev():**
  - **Parameters:**
    - string: jobRev
    - guid: qualityClauseRev
  - **Description:**
    - Links the selected `JobRev` to the selected `QualityClauseRev`, if they both exist.

**UnlinkJobRevAndQualityClauseRev():**
  - **Parameters:**
    - string: jobRev
    - guid: qualityClauseRev
  - **Description:**
    - Unlinks the selected `JobRev` to the selected `QualityClauseRev`, if they both exist.

## Quality Clauses (QualityClauses)

### Description
Defines a specific quality standard that a given part within a job must adhere to in order to proceed with production.

### Methods

**CreateQualityClause():**
  - **Parameters:**
    - string: clause
  - **Description:**
    - Creates a `QualityClause` with the clause that is defined in the parameter, if it doesn't already exist.

**ActivateQualityClause():**
  - **Parameters:**
    - guid: revGroup
  - **Description:**
    - Changes the active status of the selected `QualityClause` to True, if it exists.

**DeactivateQualityClause():**
  - **Parameters:**
    - guid: revGroup
  - **Description:**
    - Changes the active status of the selected `QualityClause` to False, if it exists.

**MergeQualityClauses():**
  - **Parameters:**
    - guid: groupId1
    - guid: groupId2
  - **Description:**
    - Merge the selected `QualityClauses` together if they both exist.

## Operation Spec Revs (OpSpecRevs)

### Description
Revision for selected `OpSpecs`

### Methods

**CreateOpSpecRev():**
  - **Parameters:**
    - guid: groupId
    - guid: sourceSpecRev
    - string: name
  - **Description:**
    - Create `OpSpecRev` and link it to selected `OpSpec` if they both exist.

**CreateOpSpecRev():**
  - **Parameters:**
    - OpSpec: newSpecRev
  - **Description:**
    - Create `OpSpecRev` if it doesn't already exist.

**UpdateOpSpecRev():**
  - **Parameters:**
    - OpSpec: newSpecRev
  - **Description:**
    - Change `OpSpecRev` to `OpSpecRev` parameter if it exists.

**ActivateQualityClause():**
  - **Parameters:**
    - guid: groupId
    - guid: specRev
  - **Description:**
    - Changes the active status of the selected `OpSpecRev` to True, if it exists.

**DeactivateQualityClause():**
  - **Parameters:**
    - guid: groupId
    - guid: specRev
  - **Description:**
    - Changes the active status of the selected `OpSpecRev` to False, if it exists.

**SplitQualityClauseRev():**
  - **Parameters:**
    - guid: revGroup
    - guid: opSpecRev
  - **Description:**
    - Duplicate the selected `OpSpec` and `OpSpecRev` into a new entity, if they both exist.

### Links

**LinkJobOpAndOpSpecRev():**
  - **Parameters:**
    - int: opId
    - guid: opSpecRev
  - **Description:**
    - Links the selected `JobOp` to the selected `OpSpecRev`, if they both exist.

**UnlinkJobRevAndQualityClauseRev():**
  - **Parameters:**
    - int: opId
    - guid: opSpecRev
  - **Description:**
    - Unlinks the selected `JobOp` to the selected `OpSpecRev`, if they both exist.

## Operation Specs (OpSpecs)

### Description
`OpSpecs` are specifications for their respective `JobOps`

### Methods

**CreateJobOpSpec():**
  - **Parameters:**
    - OpSpec: newSpec
  - **Description:**
    - Create `OpSpec` if it does not already exist.

**ActivateOpSpec():**
  - **Parameters:**
    - guid: revGroup
  - **Description:**
    - Change the active status of selected `OpSpec` to True, if it exists.

**DeactivateOpSpec():**
  - **Parameters:**
    - guid: revGroup
  - **Description:**
    - Change the active status of selected `OpSpec` to False, if it exists.



## Work Instructions (WorkInstructions)

### Description
Holds the information needed to complete a necessary action for the Job

### Methods

**CreateWorkInstruction():**
  - **Parameters:**
    - int: op
  - **Description:**
    - Create `WorkInstruction` if it doesn't already exist.

**ActivateWorkInstruction():**
  - **Parameters:**
    - guid: idRevGroup
  - **Description:**
    - Change the active status of selected `WorkInstruction` to True, if it exists.

**DeactivateWorkInstruction():**
  - **Parameters:**
    - guid: idRevGroup
  - **Description:**
    - Change the active status of selected `WorkInstruction` to False, if it exists.

## Work Instruction Revs (WorkInstructionRevs)

### Description
Revision for Work Instructions.  Many revisions to one Work Instruction

### Methods

**CreateWorkInstructionRev():**
  - **Parameters:**
    - guid: groupId
    - guid: sourceWorkInstructionRev
  - **Description:**
    - Create `WorkInstructionRev` if it does not exist.

**CreateWorkInstructionRev():**
  - **Parameters:**
    - WorkInstruction: newWorkInstruction
  - **Description:**
    - Create `WorkInstructionRev` from `WorkInstruction` object if it does not exist.

**UpdateWorkInstructionRev():**
  - **Parameters:**
  -  WorkInstruction: newWorkInstructionRev
  - **Description:**
    - Update `WorkInstructionRev` to `WorkInstructionRev` in parameter, if it exists.

**ActivateWorkInstruction():**
  - **Parameters:**
    - guid: groupId
    - guid: workInstructionRev
  - **Description:**
    - Change the active status of selected `WorkInstructionRev` to True, if it exists.

**DeactivateWorkInstruction():**
  - **Parameters:**
    - guid: groupId
    - guid: workInstructionRev
  - **Description:**
    - Change the active status of selected `WorkInstructionRev` to False, if it exists.

**MergeWorkInstructionRev():**
  - **Parameters:**
    - guid: groupId1
    - guid: groupId2
  - **Description:**
    - Merge the selected `WorkInstruction` and `WorkInstructionRev` into a new entity, if they both exist.

**SplitWorkInstructionRev():**
  - **Parameters:**
    - guid: revGroup
    - guid: workInstructionRev
  - **Description:**
    - Duplicate the selected `WorkInstruction` and `WorkInstructionRev` into a new entity, if they both exist.

**CloneWorkInstructionRev():**
  - **Parameters:**
    - guid: sourceRevGroup
    - guid: targetRevGroup
    - bool: additive
  - **Description:**
  - Clone the content of the `sourceRevGroup` into the `targetRevGroup`, if they exist.
    - Data transfer behaves differently depending on the `additive` parameter.
      - Data will be added instead of overwritten if `additive` is true


## Display (Pull)

### Description
Collection of all of the display methods found within this library

### Methods
**PullQualityClauseFromJob():**
  - **Parameters:**
    - string: jobId 
    - string: customerRev
    - string internalRev
  - **Description:**
    - Display the `Quality Clause` from a specified Job.

**DisplayPriorRevisionsOfQualityClauses():**
  - **Parameters:**
    - guid: revGroup
  - **Description:**
    - Display all prior revisions of the selected `QualityClause` if it exists.

**DisplayPriorRevisionsOfWorkInstructions():**
  - **Parameters:**
    - guid: revGroup
  - **Description:**
    - Display all prior revisions of the selected `WorkInstruction` if it exists.

**DisplayLatestRevisionsOfWorkInstruction():**
  - **Parameters:**
    - guid: revGroup
  - **Description:**
    - Display the latest revisions of the selected `WorkInstruction` if it exists.