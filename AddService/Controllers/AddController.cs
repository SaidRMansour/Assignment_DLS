using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Monitoring;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

namespace AddService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AddController : ControllerBase
    {
        /// <summary>
        /// Endpoint to calculate the sum of numbers.
        /// </summary>
        /// <param name="input">List of numbers to add.</param>
        /// <returns>Sum of the numbers.</returns>
        [HttpGet]
        public IActionResult Get([FromQuery] List<int> input)
        {
            // Extract telemetry context from the incoming request.
            var propagator = new TraceContextPropagator();
            var contextToInject = HttpContext.Request.Headers;
            var parentContext = propagator.Extract(default, contextToInject, (headers, key) =>
            {
                return headers.ContainsKey(key) ? new List<string> { headers[key].ToString() } : new List<string>();
            });

            Baggage.Current = parentContext.Baggage;

            using (var activity = MonitorService.ActivitySource.StartActivity("Get Add service received", ActivityKind.Consumer, parentContext.ActivityContext))
            {
                MonitorService.Log.Here().Debug("Entered Add method with {Input}", input);

                // Validate the input.
                if (input == null || !input.Any())
                {
                    MonitorService.Log.Here().Error("Invalid input - List empty or is null");
                    return BadRequest("Invalid input");
                }

                // Log the machine name (useful for distributed tracing in clustered environments).
                Console.WriteLine(Environment.MachineName);

                var result = input.Sum();
                MonitorService.Log.Here().Debug("Add method calculated this result: {Result}", result);

                return Ok(result);
            }
        }
    }
}
