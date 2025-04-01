using Couchbase;
using Quartz;
using Service.Order.Consumer.Jobs;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<ICluster>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var couchbaseConfig = configuration.GetSection("Couchbase");

    string connectionString = couchbaseConfig["ConnectionString"];
    string username = couchbaseConfig["Username"];
    string password = couchbaseConfig["Password"];
    
    return Cluster.ConnectAsync(connectionString, username, password).Result;
});

builder.Services.AddQuartz(configurator =>
{
    JobKey jobKey = new("OrderInboxPublishJob");
    configurator.AddJob<OrderInboxPublishJob>(options => options.WithIdentity(jobKey));

    TriggerKey triggerKey = new("OrderInboxPublishTrigger");
    configurator.AddTrigger(options => options.ForJob(jobKey)
        .WithIdentity(triggerKey)
        .StartAt(DateTime.UtcNow)
        .WithSimpleSchedule(builder => builder
            .WithIntervalInSeconds(5)
            .RepeatForever()));
});

builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

var host = builder.Build();
host.Run();