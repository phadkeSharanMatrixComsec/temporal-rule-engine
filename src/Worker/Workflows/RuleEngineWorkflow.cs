using Temporalio.Common;
using Temporalio.Workflows;
using Worker.Activities;
using Worker.Models;

namespace Worker.Workflows;

[Workflow]
public class RuleEngineWorkflow
{
    [WorkflowRun]
    public async Task<string> StartWorkflow(RuleModel ruleModel)
    {
        var retryPolicy = new RetryPolicy
        {
            InitialInterval = TimeSpan.FromSeconds(10),
            MaximumInterval = TimeSpan.FromSeconds(20),
            BackoffCoefficient = 2,
            MaximumAttempts = 2,
        };

        var rawEvent = ruleModel.Event;

        switch (rawEvent)
        {
            case "CameraOfflineEvent":
                await Workflow.ExecuteChildWorkflowAsync(
                    (CameraOfflineEventWorkflow cameraOfflineEventWorkflow)
                    => cameraOfflineEventWorkflow.StartWorkflow(ruleModel.Actions),
                    new ChildWorkflowOptions { RetryPolicy = retryPolicy }
                );
                break;
            case "RecordingStoppedEvent":
                await Workflow.ExecuteChildWorkflowAsync(
                    (RecordingStoppedEventWorkflow recordingStoppedEventWorkflow)
                    => recordingStoppedEventWorkflow.StartWorkflow(ruleModel.Actions),
                    new ChildWorkflowOptions { RetryPolicy = retryPolicy }
                );
                break;
            case "MotionDetectedEvent":
                await Workflow.ExecuteChildWorkflowAsync(
                    (MotionDetectedEventWorkflow motionDetectedEventWorkflow)
                    => motionDetectedEventWorkflow.StartWorkflow(ruleModel.Actions),
                    new ChildWorkflowOptions { RetryPolicy = retryPolicy }
                );
                break;
            default:
                break;
        }

        // await Workflow.ExecuteActivityAsync(
        //     (RuleEngineActivities ruleEngineActivities)
        //     => ruleEngineActivities.ParseEvent(ruleModel, retryPolicy),
        //     new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5), RetryPolicy = retryPolicy }
        // );
        return "Done!";
    }
}
