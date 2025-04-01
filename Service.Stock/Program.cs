using Couchbase;
using MassTransit;
using Service.Stock.Consumers;
using Shared.Constants;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<OrderCreatedEventConsumer>();
    configurator.UsingRabbitMq((context, configure) =>
    {
        configure.Host(builder.Configuration["RabbitMQ"]);

        configure.ReceiveEndpoint(RabbitMQSettings.StockOrderCreatedEvent, e => e.ConfigureConsumer<OrderCreatedEventConsumer>(context));
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

var app = builder.Build();
app.Run();