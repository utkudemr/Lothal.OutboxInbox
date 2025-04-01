using System.Text.Json;
using Couchbase;
using Couchbase.Transactions;
using MassTransit;
using Service.Order.Models;
using Service.Order.Models.Entities;
using Service.Order.Publisher.Entities;
using Service.Order.Services;
using Shared.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

builder.Services.AddScoped<ICouchbaseService, CouchbaseService>();


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/create-order", async (CreateOrderRequest request,ICouchbaseService couchbaseService) =>
{
    Order order = new()
    {
        Id = Guid.NewGuid(),
        BuyerId = request.CustomerId,
        CreatedDate = DateTime.UtcNow,
        TotalPrice = request.OrderItems.Sum(oi => oi.Qty * oi.Price),
        OrderItems = request.OrderItems.Select(oi => new OrderItem
        {
            Price = oi.Price,
            Qty = oi.Qty,
            ProductId = oi.ProductId,
        }).ToList()
    };
    var idempotentToken = Guid.NewGuid();
    OrderCreatedEvent orderCreatedEvent = new()
    {
        BuyerId = order.BuyerId,
        OrderId = order.Id,
        TotalPrice = request.OrderItems.Sum(oi => oi.Qty * oi.Price),
        OrderItems = request.OrderItems.Select(oi => new Shared.Messages.OrderItemMessage {
            Price = oi.Price,
            Qty = oi.Qty,
            ProductId = oi.ProductId,
        }).ToList(),
        IdempotentToken = idempotentToken
    };
    OrderOutbox<OrderCreatedEvent> orderOutbox = new()
    {
        OccuredOn = DateTime.UtcNow,
        ProcessedDate = null,
        Payload = orderCreatedEvent,
        //Type = orderCreatedEvent.GetType().Name
        Type = nameof(OrderCreatedEvent),
        IdempotentToken = idempotentToken
    };

    var transactions =  couchbaseService.GetTransactions();
    var orderMetaCollection = await couchbaseService.GetCollectionAsync("lothal", "order", "orderMeta");
    var orderOutboxCollection = await couchbaseService.GetCollectionAsync("lothal", "order", "outbox");
    
    await transactions.RunAsync(async ctx =>
    {
        try
        {
            await ctx.InsertAsync(orderMetaCollection, order.Id.ToString(), order);
            await ctx.InsertAsync(orderOutboxCollection, orderOutbox.IdempotentToken.ToString(), orderOutbox);
            await ctx.CommitAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Transaction başarısız: {ex.Message}");
            throw;
        }
    });
});

app.Run();
