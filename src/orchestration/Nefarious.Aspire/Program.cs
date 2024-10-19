var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("nefarious-cache");

builder.AddProject<Projects.Nefarious_Host>("nefarious-host")
    .WithReference(cache);

builder.Build().Run();