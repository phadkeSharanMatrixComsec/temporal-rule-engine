using Temporalio.Common;
using Temporalio.Workflows;
using Worker.Activities;
using Worker.Common;
using Worker.Models;

namespace Worker.Workflows;

[Workflow]
public class EventWorkflow
{
    private readonly RetryPolicy retryPolicy = new()
    {
        InitialInterval = TimeSpan.FromSeconds(10),
        MaximumInterval = TimeSpan.FromSeconds(20),
        BackoffCoefficient = 2,
        MaximumAttempts = 2,
    };

    private List<string> _actionList = [];

    [WorkflowRun]
    public async Task<string> StartWorkflow(List<string> actions)
    {
        _actionList = actions;
        string broadcasterWorkflowId = TemporalConstants.EventBroadcasterWorkflowId;

        var broadcastworkflowHandle = Workflow.GetExternalWorkflowHandle(broadcasterWorkflowId);
        await broadcastworkflowHandle.SignalAsync<EventBroadcasterWorkflow>(wf => wf.RegisterWorkflow(Workflow.Info.WorkflowId));

        await Workflow.ExecuteActivityAsync(
            (RuleEngineActivities ruleEngineActivities)
            => ruleEngineActivities.LongRunningActivity(),
            new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5), RetryPolicy = retryPolicy }
        );

        return "Done!";
    }

    [WorkflowSignal]
    public async Task HandleEvent(EventModel eventModel)
    {
        if (_actionList.Count > 0)
        {
            await RuleEngineActivities.ExecuteActions(_actionList, eventModel, retryPolicy);
        }
    }
}
