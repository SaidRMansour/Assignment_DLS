using System.Diagnostics;
using System.Text;
using Helpers;
using Microsoft.AspNetCore.Mvc;
using Monitoring;
using Newtonsoft.Json;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using SharedModels.Models;

namespace SubService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SubController : ControllerBase
    {

        private readonly IHttpClientFactory _clientFactory;
        private readonly RabbitMqService _rabbitMqService;

        // Constructor to initialize the HTTP client factory
        public SubController(IHttpClientFactory clientFactory, RabbitMqService rabbitMqService)
        {
            _clientFactory = clientFactory;
            _rabbitMqService = rabbitMqService;
        }

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

        // Push calculation result into database
        [HttpPost]
        public async Task<string> PushIntoDatabase([FromBody] CalculationData data)
        {
            var client = _clientFactory.CreateClient("MyClient");
            var newData = new CalculationData()
            {
                Id = data.Id.ToString(),
                ListOfNumbers = data.ListOfNumbers,
                Operation = data.Operation.ToString(),
                Result = data.Result,
                Time = data.Time
            };

            var propagator = new TraceContextPropagator();
            var activityContext = Activity.Current?.Context ?? default;
            var propagationContext = new PropagationContext(activityContext, Baggage.Current);

            var jsonContent = JsonConvert.SerializeObject(newData);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, "http://history-service/History/DatabasePush")
            {
                Content = httpContent
            };
            propagator.Inject(propagationContext, request.Headers, (headers, key, value) => headers.Add(key, value));

            var response = await client.SendAsync(request);

            return response.IsSuccessStatusCode ? "Data successfully added to the database." : "Failed to insert data into the database.";
        }
    }
}
