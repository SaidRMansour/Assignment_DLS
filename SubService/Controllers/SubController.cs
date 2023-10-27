using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Monitoring;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

namespace SubService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SubController : ControllerBase
    {
        /// <summary>
        /// Endpoint to calculate the subtraction of numbers.
        /// </summary>
        /// <param name="input">List of numbers to subtract.</param>
        /// <returns>Subtraction result of the numbers.</returns>
        [HttpGet]
        public IActionResult Get([FromQuery] List<int> input)
        {
            // Extracts telemetry context from incoming request.
            var propagator = new TraceContextPropagator();
            var contextToInject = HttpContext.Request.Headers;
            var parentContext = propagator.Extract(default, contextToInject, (headers, key) =>
            {
                return headers.ContainsKey(key) ? new List<string> { headers[key].ToString() } : new List<string>();
            });

            Baggage.Current = parentContext.Baggage;

            using (var activity = MonitorService.ActivitySource.StartActivity("Get Sub service received", ActivityKind.Consumer, parentContext.ActivityContext))
            {
                MonitorService.Log.Here().Verbose("Entered Sub method with {Input}", input);

                // Validate the input.
                if (input == null || !input.Any())
                {
                    MonitorService.Log.Here().Error("Invalid input - List empty or is null");
                    return BadRequest("Invalid input");
                }

                // Log the machine name (useful for distributed tracing in clustered environments).
                Console.WriteLine(Environment.MachineName);

                var result = input[0];
                for (int i = 1; i < input.Count; i++)
                {
                    result -= input[i];
                }

                MonitorService.Log.Here().Debug("Sub method calculated this result: {Result}", result);
                return Ok(result);
            }
        }
    }
}
