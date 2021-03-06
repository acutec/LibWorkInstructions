# Library Requirements:

- background: Acutec is a job-shop, meaning there are many distinct types of jobs moving through the shop at any point in time. Each job has a sequence of operations, by the end of which we have a shippable part. Each of those operations should have work instructions detailing the process and specifications needed to complete that operation. Due to process changes and customer changes that are flowed down to us, the operations are changed over time creating multiple revisions, thus work instructions are specific to a particular revision of an operation.
- an individual work instruction
    - is specific to a particular revision of an operation.
    - may be associated with one or more images ("visual work instructions") that were uploaded.
    - may be associated with one or more specifications ("operational specifications") that are managed separately in the work instructions system.
    - has an approval status.
    - has some rich text content with headers, lists, etc. (treat as html blob).
    - has one or more revisions.
- an individual specification
    - has fields (name, rev, notice, class, type, method, grade, level, proctype, servicecond, status, comment)
    - has one or more revisions.
- an individual quality clause
    - is specific to a particular revision of a job.
    - has one or more revisions.
- a revision of a workinstruction/specification/qualityclause
    - follows semantic versioning (major/minor/patch); during creation, user provides which category of change it was.
- management (primarily engineering-facing)
    - create work instructions
    - change work instructions
    - delete work instructions
    - create specifications
    - change specifications
    - delete specifications
    - create quality clauses
    - merge/split/clone quality clauses
    - merge/split/clone specifications
    - merge/split/clone work instructions
    - changes to existing operational specifications invalidate the approval status of any associated work instructions.
    - viewing (read-only) prior revisions of workinstructions/specifications/qualityclauses.
    - audit log of all changes, when they occurred, who made them, etc.
- presentation (primarily production-facing)
    - given job/rev, and op/rev, date, display latest revision of the work instruction, including related images, opspecs, qualityclauses, etc.
    - generate a table of contents based on the header tags found in the content.