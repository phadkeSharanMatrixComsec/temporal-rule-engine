using Temporalio.Activities;
using Temporalio.Common;
using Temporalio.Workflows;
using Worker.Models;
using Worker.Workflows;

namespace Worker.Activities;

public class RuleEngineActivities
{
    public RuleEngineActivities()
    {

    }

    [Activity]
    public async Task<string> LongRunningActivity()
    {
        await Task.Delay(1000 * 3600 * 24);
        return "Completed";
    }

    [Activity]
    public async Task ParseEvent(RuleModel ruleModel, RetryPolicy retryPolicy)
    {
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

    }


    public static async Task ExecuteActions(List<string> actionList, EventModel eventModel, RetryPolicy retryPolicy)
    {
        try
        {
            var tasks = new List<Task>();

            foreach (var action in actionList)
            {
                switch (action)
                {
                    case "EmailAction":
                        tasks.Add(
                            Workflow.ExecuteActivityAsync(
                                (EmailActivities emailActivities)
                                => emailActivities.SendEmailActivity(eventModel),
                                new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5), RetryPolicy = retryPolicy }
                            )
                        );
                        break;
                    case "NotificationAction":
                        tasks.Add(
                            Workflow.ExecuteActivityAsync(
                                (NotificationActivities notificationActivities)
                                => notificationActivities.SendNotificationActivity(eventModel),
                                new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5), RetryPolicy = retryPolicy }
                            )
                        );
                        break;
                    case "LiveViewAction":
                        tasks.Add(
                            Workflow.ExecuteActivityAsync(
                                (LiveViewActivities liveViewActivities)
                                => liveViewActivities.StartLiveViewActivity(eventModel),
                                new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5), RetryPolicy = retryPolicy }
                            )
                        );
                        break;
                    default:
                        break;
                }
            }

            // Execute all tasks concurrently
            await Task.WhenAll(tasks);

        }
        catch (Exception e)
        {
            Console.WriteLine($"Action failed {e.Message} {eventModel.EventName}");
        }

    }

}
