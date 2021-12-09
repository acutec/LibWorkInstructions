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
    - Creates a new Job object to put in the database.

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
    - Creates a `JobRev` based on which strings were provided, granted the given job exists.

**CreateJobRev():**
  - **Parameters:**
    - Job: newJobRev 
  - **Description:**
    - Behaves the same as the previous `CreateJobRev` method, but this takes in a `Job` object as a parameter.

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

### Linking Methods

**LinkJobOpAndJobRev():**
  - **Parameters:**
    - int:    opId
    - string: jobRev
  - **Description:**
    - Duplicate the selected Job and Job Revision into a new entity, if they both exist.


## Job Operations (JobOps)

## Quality Clause Revisions (QualityClauseRevs)

## Quality Clauses (QualityClauses)

## Operation Spec Revs (OpSpecRevs)

## Operation Specs (OpSpecs)

## Work Instructions (WorkInstructions)

## Work Instruction Revs (WorkInstructionRevs)

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