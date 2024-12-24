using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Temporalio.Client;
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

        [HttpGet("TriggerEvent/{ruleName}")]
        public async Task<IActionResult> TriggerEvent(string ruleName)
        {
            string cameraAddedEvent = "CameraOfflineEvent";

            var ruleModel = await _mongoRepository.GetByRuleName(ruleName);


            var cameraAddedEventModel = new EventModel()
            {
                EventName = cameraAddedEvent,
                EventData = "camera1",
                EventId = "1",
                TenantId = "1"
            };

            var workflowHandle = _temporalClient.GetWorkflowHandle(ruleModel.WorkflowId);
            switch (ruleModel.Event)
            {
                case "CameraOfflineEvent":
                    await workflowHandle.SignalAsync<CameraOfflineEventWorkflow>(wf => wf.CameraOfflineSignal(cameraAddedEventModel));

                    break;
                case "RecordingStoppedEvent":
                    await workflowHandle.SignalAsync<RecordingStoppedEventWorkflow>(wf => wf.RecordingStoppedSignal(cameraAddedEventModel));

                    break;
                case "MotionDetectedEvent":
                    await workflowHandle.SignalAsync<MotionDetectedEventWorkflow>(wf => wf.MotionDetectedSignal(cameraAddedEventModel));

                    break;
                default:
                    break;

            }
            return Ok();

        }
    }
}
