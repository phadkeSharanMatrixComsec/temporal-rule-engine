using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RuleEngine.Services;
using Temporalio.Client;
using Worker.Models;
using Worker.Workflows;

namespace RuleEngine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RuleController : ControllerBase
    {
        private readonly IMongoRepository<RuleModel> _ruleRepository;
        private readonly RuleEngineService _ruleEngineService;

        public RuleController(IMongoRepository<RuleModel> ruleRepository, RuleEngineService ruleEngineService)
        {
            _ruleRepository = ruleRepository;
            _ruleEngineService = ruleEngineService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateRule([FromBody] RuleModel rule)
        {
            if (string.IsNullOrWhiteSpace(rule.Event) || rule.Actions == null || !rule.Actions.Any())
            {
                return BadRequest("Invalid input. EventName and Actions are required.");
            }

            await _ruleRepository.InsertAsync(rule);

            return Ok(rule);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRuleById(string id)
        {
            var rule = await _ruleRepository.GetByIdAsync(id);
            if (rule == null)
            {
                return NotFound($"Rule with ID {id} not found.");
            }
            return Ok(rule);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRules()
        {
            var rules = await _ruleRepository.GetAllAsync();
            return Ok(rules);
        }

        [HttpGet("activate/{ruleName}")]
        public async Task<IActionResult> ActivateRule(string ruleName)
        {
            var ruleModel = await _ruleRepository.GetByRuleName(ruleName);

            await _ruleEngineService.ActivateRule(ruleModel);

            return Ok();
        }

        [HttpGet("deactivate/{ruleName}")]
        public async Task<IActionResult> DeactivateRule(string ruleName)
        {
            var ruleModel = await _ruleRepository.GetByRuleName(ruleName);

            await _ruleEngineService.DeactivateRule(ruleModel);

            return Ok();
        }

    }
}
