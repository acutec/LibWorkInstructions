
class Event {
  public string Kind {get; set;}
  public Dictionary<string,string> Args {get;set;}
  public DateTime When {get; set;}
}

class MockDB {
  public List<Event> events {get; set;}
}

class Projections {
  class Job {
    public string JobNumber {get; set;}
    public string Status {get; set;}
  }
}

class Library {
  public void CreateJob(List<Event> events, string jobId) {
    var args = new Dictionary<string,string>();
    args["id"] = jobId;
    events.Add(new Event {
      Kind = "JobCreated",
      Args = args,
      When = DateTime.Now(),
    });
  }

  public List<Job> DeriveJobs(List<Event> events) {
    var ret = new List<Job>();
    foreach (var event in events.OrderBy(z => z.When)) {
      if (event.Kind == "JobCreated") {
        ret.Add(new Job {JobNumber = event.Args["id"]});
      } else if (event.Kind == "JobStatusSet") {
        ret.First(z => z.JobNumber == event.Args["id"]).Status = event.Args["status"];
      }
    }
    return ret;
  }
}
