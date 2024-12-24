using MongoDB.Driver;
using RuleEngine.Services;
using Temporalio.Client;
using Worker.Models;

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

// app.UseHttpsRedirection();
app.MapControllers();

app.Run();