using Temporalio.Common;
using Temporalio.Workflows;
using Worker.Activities;
using Worker.Models;

namespace Worker.Workflows;

[Workflow]
public class EventBroadcasterWorkflow
{
    private readonly RetryPolicy retryPolicy = new()
    {
        InitialInterval = TimeSpan.FromSeconds(10),
        MaximumInterval = TimeSpan.FromSeconds(20),
        BackoffCoefficient = 2,
        MaximumAttempts = 2,
    };
    private List<string> _registeredWorkflows = [];

    [WorkflowRun]
    public async Task StartAsync()
    {
        await Workflow.ExecuteActivityAsync(
            (RuleEngineActivities ruleEngineActivities)
            => ruleEngineActivities.LongRunningActivity(),
            new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5), RetryPolicy = retryPolicy }
        );
    }

    [WorkflowSignal]
    public async Task RegisterWorkflow(string workflowId)
    {
        if (!_registeredWorkflows.Contains(workflowId))
        {
            _registeredWorkflows.Add(workflowId);
        }
    }

    [WorkflowSignal]
    public async Task DeregisterWorkflow(string workflowId)
    {
        if (_registeredWorkflows.Contains(workflowId))
        {
            _registeredWorkflows.Remove(workflowId);
        }
    }

    [WorkflowSignal]
    public async Task BroadcastEvent(EventModel eventModel)
    {
        foreach (var workflowId in _registeredWorkflows)
        {
            try
            {
                var workflowHandle = Workflow.GetExternalWorkflowHandle(workflowId);
                await workflowHandle.SignalAsync<EventWorkflow>(wf => wf.HandleEvent(eventModel));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to signal workflow {workflowId}: {ex.Message}");
            }
        }
    }
}
