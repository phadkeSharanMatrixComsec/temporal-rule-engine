using Microsoft.AspNetCore.Mvc;
using Temporalio.Client;
using Worker.Common;
using Worker.Models;
using Worker.Workflows;

namespace RuleEngine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TriggerEventController : ControllerBase
    {
        private readonly IMongoRepository<RuleModel> _mongoRepository;
        private readonly ITemporalClient _temporalClient;

        public TriggerEventController(ITemporalClient temporalClient, IMongoRepository<RuleModel> mongoRepository)
        {
            _temporalClient = temporalClient;
            _mongoRepository = mongoRepository;
        }

        [HttpGet("TriggerEvent/{eventName}")]
        public async Task<IActionResult> TriggerEvent(string eventName)
        {
            string broadcasterWorkflowId = TemporalConstants.EventBroadcasterWorkflowId;

            var eventModel = new EventModel()
            {
                EventName = eventName,
                EventData = "camera1",
                EventId = "1",
                TenantId = "1"
            };

            var workflowHandle = _temporalClient.GetWorkflowHandle(broadcasterWorkflowId);

            await workflowHandle.SignalAsync<EventBroadcasterWorkflow>(broadcastEventWorkflow => broadcastEventWorkflow.BroadcastEvent(eventModel));
            // switch (ruleModel.Event)
            // {
            //     case "CameraOfflineEvent":
            //         await workflowHandle.SignalAsync<CameraOfflineEventWorkflow>(wf => wf.CameraOfflineSignal(cameraAddedEventModel));

            //         break;
            //     case "RecordingStoppedEvent":
            //         await workflowHandle.SignalAsync<RecordingStoppedEventWorkflow>(wf => wf.RecordingStoppedSignal(cameraAddedEventModel));

            //         break;
            //     case "MotionDetectedEvent":
            //         await workflowHandle.SignalAsync<MotionDetectedEventWorkflow>(wf => wf.MotionDetectedSignal(cameraAddedEventModel));

            //         break;
            //     default:
            //         break;

            // }
            return Ok();

        }
    }
}
