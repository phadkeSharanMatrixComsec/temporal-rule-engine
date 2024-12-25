// Create a client to connect to localhost on "default" namespace
using Temporalio.Client;
using Temporalio.Worker;
using Worker.Activities;
using Worker.Workflows;

var client = await TemporalClient.ConnectAsync(new("localhost:7233"));

// Cancellation token to shutdown worker on ctrl+c
using var tokenSource = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) =>
{
    tokenSource.Cancel();
    eventArgs.Cancel = true;
};

// Create a worker with the activity and workflow registered
using var worker = new TemporalWorker(
    client, // client
    new TemporalWorkerOptions(taskQueue: "RULE_TASK_QUEUE")
        .AddWorkflow<EventWorkflow>()
        .AddWorkflow<EventBroadcasterWorkflow>()
        .AddAllActivities(new EmailActivities())
        .AddAllActivities(new LiveViewActivities())
        .AddAllActivities(new NotificationActivities())
        .AddAllActivities(new RuleEngineActivities())
    );

Console.WriteLine("Running worker...");
try
{
    await worker.ExecuteAsync(tokenSource.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("Worker cancelled");
}