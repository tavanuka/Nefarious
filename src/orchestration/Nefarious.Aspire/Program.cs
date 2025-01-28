using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var cache = builder
    .AddRedis("nefarious-cache")
    .WithRedisCommander()
    .WithDataVolume();

if(builder.Configuration.GetValue<bool>("Host"))
    builder.AddProject<Projects.Nefarious_Host>("nefarious-host")
        .WaitFor(cache)
        .WithReference(cache);
else 
    builder.AddProject<Projects.Nefarious_Api>("nefarious-api")
        .WaitFor(cache)
        .WithReference(cache);

builder.Build().Run();