var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.AspireIssueTriageApp_FrontEnd>("aspireissuetriageapp-frontend");

builder.Build().Run();
