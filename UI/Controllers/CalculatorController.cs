using System.Diagnostics;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Monitoring;
using Newtonsoft.Json;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using SharedModels.Models;
using Helpers;

namespace UI.Controllers
{
    // Controller for calculator operations
    public class CalculatorController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly RabbitMqService _rabbitMqService;

        // Constructor to initialize the HTTP client factory
        public CalculatorController(IHttpClientFactory clientFactory, RabbitMqService rabbitMqService)
        {
            _clientFactory = clientFactory;
            _rabbitMqService = rabbitMqService;
        }

        // GET method for loading the Index view
        [HttpGet]
        public async Task<IActionResult> IndexAsync()
        {
            await LoadDataAsync();
            return View();

        }

        // POST method to calculate based on user input and operation
        [HttpPost]
        public async Task<IActionResult> Calculate(string numberInput, string operation)
        {
            using (var activity = MonitorService.ActivitySource.StartActivity())
            {
                if(numberInput == null || numberInput == "")
                {
                    ViewBag.Error = "No numbers inserted";
                    await LoadDataAsync();
                    return View("Index");
                }
                List<int> numbers;
                try
                {
                    // Parse numbers from user input
                    numbers = numberInput.Split(',').Select(int.Parse).ToList();
                }
                catch (FormatException)
                {
                    ViewBag.Error = "Invalid number format.";
                    await LoadDataAsync();
                    return View("Index");
                }
                
                var client = _clientFactory.CreateClient("MyClient");
                // Initialize tracing context
                var activityContext = activity?.Context ?? Activity.Current?.Context ?? default;
                var propagationContext = new PropagationContext(activityContext, Baggage.Current);
                var propagator = new TraceContextPropagator();

                var queryString = string.Join("&input=", numbers.Prepend(0));
                string result = "";
                string resp = "";
                string responseContent = string.Empty;

                // Calculate based on selected operation
                if (operation == "Add")
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, $"http://adding-service/Add?{queryString}");
                    propagator.Inject(propagationContext, request.Headers, (headers, key, value) => headers.Add(key, value));

                    var response = await client.SendAsync(request);
                    result = await response.Content.ReadAsStringAsync();

                    if (result != null || result != "")
                    {
                        var (isFound, data) = await CheckDB(numbers, operation, result);

                        if (!isFound)
                        {
                            // Data
                            var newData = new CalculationData()
                            {
                                Id = $"ListOfNumbers={string.Join(",", numbers)}&Operation={operation}&Result={result}",
                                ListOfNumbers = numbers,
                                Operation = operation,
                                Result = Int32.Parse(result),
                                Time = DateTime.Now
                            };
                            var jsonContent = JsonConvert.SerializeObject(newData);
                            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                            var requestPush = new HttpRequestMessage(HttpMethod.Post, "http://adding-service/Add")
                            {
                                Content = httpContent
                            };

                            var responsePush = await client.SendAsync(requestPush);
                            responseContent = await responsePush.Content.ReadAsStringAsync();

                        }

                        resp = isFound ? "Data fetched from DB." : responseContent;
                    }
                }
                else if (operation == "Sub")
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, $"http://subing-service/Sub?{queryString}");
                    propagator.Inject(propagationContext, request.Headers, (headers, key, value) => headers.Add(key, value));

                    var response = await client.SendAsync(request);
                    result = await response.Content.ReadAsStringAsync();

                    if (result != null || result != "")
                    {
                        var (isFound, data) = await CheckDB(numbers, operation, result);

                        if (!isFound)
                        {
                            // Data
                            var newData = new CalculationData()
                            {
                                Id = $"ListOfNumbers={string.Join(",", numbers)}&Operation={operation}&Result={result}",
                                ListOfNumbers = numbers,
                                Operation = operation,
                                Result = Int32.Parse(result),
                                Time = DateTime.Now
                            };
                            var jsonContent = JsonConvert.SerializeObject(newData);
                            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                            var requestPush = new HttpRequestMessage(HttpMethod.Post, "http://subing-service/Sub")
                            {
                                Content = httpContent
                            };

                            var responsePush = await client.SendAsync(requestPush);
                            responseContent = await responsePush.Content.ReadAsStringAsync();

                        }

                        resp = isFound ? "Data fetched from DB." : responseContent;
                    }
                }
                else
                {
                    ViewBag.Error = "Invalid operation.";
                    return View("Index");
                }

                // Update view with the result
                ViewBag.ResultComment = resp;
                ViewBag.Result = result;
                await LoadDataAsync();

                return View("Index");
            }
        }

        // Load historical calculation data
        private async Task LoadDataAsync()
        {
            using (var activity = MonitorService.ActivitySource.StartActivity())
            {
                var client = _clientFactory.CreateClient("MyClient");

                var propagator = new TraceContextPropagator();
                var activityContext = activity?.Context ?? Activity.Current?.Context ?? default;
                var propagationContext = new PropagationContext(activityContext, Baggage.Current);

                var request = new HttpRequestMessage(HttpMethod.Get, $"http://history-service/History");
                propagator.Inject(propagationContext, request.Headers, (headers, key, value) => headers.Add(key, value));

                var response = await client.SendAsync(request);
                var result = await response.Content.ReadAsStringAsync();

                var rawRecords = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
                var records = new Dictionary<string, CalculationData>();

                if (rawRecords != null)
                {
                    foreach (var key in rawRecords.Keys)
                    {
                        var record = JsonConvert.DeserializeObject<CalculationData>(rawRecords[key]);
                        records.Add(key, record);
                    }
                    ViewBag.ResultData = records;
                }
            }
        }

        // Check if the result already exists in the database
        private async Task<(bool, CalculationData)> CheckDB(List<int> input, string operation, string result)
        {
            var client = _clientFactory.CreateClient("MyClient");
            var propagator = new TraceContextPropagator();
            var activityContext = Activity.Current?.Context ?? default;
            var propagationContext = new PropagationContext(activityContext, Baggage.Current);

            var request = new HttpRequestMessage(HttpMethod.Get, $"http://history-service/History");
            propagator.Inject(propagationContext, request.Headers, (headers, key, value) => headers.Add(key, value));

            var response = await client.SendAsync(request);
            var responseData = await response.Content.ReadAsStringAsync();

            var id = $"ListOfNumbers={string.Join(",", input)}&Operation={operation}&Result={result}";
            var rawRecords = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseData);
            var records = new Dictionary<string, CalculationData>();

            if (rawRecords != null)
            {
                foreach (var key in rawRecords.Keys)
                {
                    var record = JsonConvert.DeserializeObject<CalculationData>(rawRecords[key]);
                    records.Add(key, record);
                }
            }

            foreach (var entry in records.Values)
            {
                if (entry.Id == id)
                {
                    return (true, entry); // Found a match!
                }
            }

            return (false, null); // No match found
        }
    }
}
