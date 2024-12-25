using Temporalio.Client;
using Temporalio.Common;
using Worker.Common;
using Worker.Models;
using Worker.Workflows;

namespace RuleEngine.Services;

public class RuleEngineService
{
    private readonly RetryPolicy _retryPolicy;
    private readonly ITemporalClient _temporalClient;
    private readonly IMongoRepository<RuleModel> _mongoRepository;


    public RuleEngineService(ITemporalClient temporalClient, IMongoRepository<RuleModel> mongoRepository)
    {
        _retryPolicy = new RetryPolicy
        {
            InitialInterval = TimeSpan.FromSeconds(10),
            MaximumInterval = TimeSpan.FromSeconds(20),
            BackoffCoefficient = 2,
            MaximumAttempts = 2,
        };

        _temporalClient = temporalClient;
        _mongoRepository = mongoRepository;
    }

    public async Task ActivateRule(RuleModel rule)
    {
        var rawEvent = rule.Event;
        string workflow_id = $"rule-{rule.Name}-{Guid.NewGuid()}";

        var handle = await _temporalClient.StartWorkflowAsync(
            (EventWorkflow eventWorkflow) => eventWorkflow.StartWorkflow(rule.Actions),
            new(id: workflow_id, taskQueue: "RULE_TASK_QUEUE")
        );

        // switch (rawEvent)
        // {
        //     case "CameraOfflineEvent":
        //         handle = await _temporalClient.StartWorkflowAsync(
        //             (CameraOfflineEventWorkflow cameraOfflineEventWorkflow) => cameraOfflineEventWorkflow.StartWorkflow(rule.Actions),
        //             new(id: workflow_id, taskQueue: "RULE_TASK_QUEUE")
        //         );
        //         break;
        //     case "RecordingStoppedEvent":
        //         handle = await _temporalClient.StartWorkflowAsync(
        //             (RecordingStoppedEventWorkflow recordingStoppedEventWorkflow) => recordingStoppedEventWorkflow.StartWorkflow(rule.Actions),
        //             new(id: workflow_id, taskQueue: "RULE_TASK_QUEUE")
        //         );
        //         break;
        //     case "MotionDetectedEvent":
        //         handle = await _temporalClient.StartWorkflowAsync(
        //             (MotionDetectedEventWorkflow motionDetectedEventWorkflow) => motionDetectedEventWorkflow.StartWorkflow(rule.Actions),
        //             new(id: workflow_id, taskQueue: "RULE_TASK_QUEUE")
        //         );
        //         break;
        //     default:
        //         break;
        // }

        var updatedModel = new RuleModel()
        {
            Id = rule.Id,
            Name = rule.Name,
            Event = rule.Event,
            Actions = rule.Actions,
            WorkflowId = workflow_id
        };

        await _mongoRepository.UpdateAsync(rule.Id, updatedModel);

    }

    public async Task DeactivateRule(RuleModel ruleModel)
    {
        var workflowHandle = _temporalClient.GetWorkflowHandle(ruleModel.WorkflowId);
        var broadcasterWorkflowId = TemporalConstants.EventBroadcasterWorkflowId;

        var broadcastEventWorkflow = _temporalClient.GetWorkflowHandle(broadcasterWorkflowId);
        await broadcastEventWorkflow.SignalAsync<EventBroadcasterWorkflow>(broadcastEventWorkflow => broadcastEventWorkflow.DeregisterWorkflow(ruleModel.WorkflowId));

        await workflowHandle.TerminateAsync();
    }

}
