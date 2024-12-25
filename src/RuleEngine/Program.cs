using MongoDB.Driver;
using RuleEngine.Services;
using Temporalio.Api.Enums.V1;
using Temporalio.Client;
using Worker.Common;
using Worker.Models;
using Worker.Workflows;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

// Add MongoDB configuration
var mongoSettings = builder.Configuration.GetSection("MongoDB");
var connectionString = mongoSettings["ConnectionString"];
var databaseName = mongoSettings["DatabaseName"];

// Register MongoDB client and database
builder.Services.AddSingleton<IMongoClient>(sp => new MongoClient(connectionString));
builder.Services.AddScoped(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(databaseName);
});

builder.Services.AddScoped<IMongoRepository<RuleModel>>(sp =>
{
    var database = sp.GetRequiredService<IMongoDatabase>();
    return new MongoRepository<RuleModel>(database, "Rules"); // Provide collection name "Rules" here
});

builder.Services.AddSingleton<ITemporalClient>(serviceProvider =>
{
    var client = TemporalClient.ConnectAsync(new TemporalClientConnectOptions(new("localhost:7233"))).GetAwaiter().GetResult();
    return client;
});

builder.Services.AddScoped<RuleEngineService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Start the Event Broadcaster Workflow
var temporalClient = app.Services.GetRequiredService<ITemporalClient>();
var broadcasterWorkflowId = TemporalConstants.EventBroadcasterWorkflowId;

await EnsureEventBroadcasterWorkflowStartedAsync(temporalClient, broadcasterWorkflowId);

// app.UseHttpsRedirection();
app.MapControllers();

app.Run();



static async Task EnsureEventBroadcasterWorkflowStartedAsync(ITemporalClient temporalClient, string workflowId)
{
    try
    {
        var workflowHandle = temporalClient.GetWorkflowHandle(workflowId);

        // Check if the workflow is already running
        try
        {
            var status = await workflowHandle.DescribeAsync();
            if (status.Status == WorkflowExecutionStatus.Running)
            {
                Console.WriteLine("Event Broadcaster Workflow is already running.");
                return;
            }
        }
        catch (Exception)
        {
            Console.WriteLine("Workflow not found. Starting a new one.");
        }

        // Start the workflow if not found or not running
        var handle = await temporalClient.StartWorkflowAsync(
            (EventBroadcasterWorkflow broadcasterWorkflow) => broadcasterWorkflow.StartAsync(),
            new(id: workflowId, taskQueue: "RULE_TASK_QUEUE")
        );

        Console.WriteLine("Event Broadcaster Workflow started successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to ensure workflow started: {ex.Message}");
    }
}