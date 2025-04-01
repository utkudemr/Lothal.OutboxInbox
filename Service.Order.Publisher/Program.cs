using Couchbase;
using MassTransit;
using Quartz;
using Service.Order.Publisher.Jobs;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMassTransit(configurator =>
{
    configurator.UsingRabbitMq((context, _configure) =>
    {
        _configure.Host(builder.Configuration["RabbitMQ"]);
    });
});

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
    JobKey jobKey = new("OrderOutboxPublishJob");
    configurator.AddJob<OrderOutboxPublishJob>(options => options.WithIdentity(jobKey));

    TriggerKey triggerKey = new("OrderOutboxPublishTrigger");
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