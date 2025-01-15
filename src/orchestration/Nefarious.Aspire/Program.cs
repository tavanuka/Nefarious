using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var cache = builder
    .AddRedis("nefarious-cache")
    .WithDataVolume();

if(builder.Configuration.GetValue<bool>("Host"))
    builder.AddProject<Projects.Nefarious_Api>("nefarious-api")
        .WithReference(cache);
else 
    builder.AddProject<Projects.Nefarious_Host>("nefarious-host")
        .WithReference(cache);

builder.Build().Run();