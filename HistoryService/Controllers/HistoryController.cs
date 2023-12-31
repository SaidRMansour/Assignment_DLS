﻿using System.Text.Json;
using FireSharp.Config;
using FireSharp.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Monitoring;
using SharedModels.Models;
using System.Diagnostics;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry;
using Helpers;


namespace HistoryService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HistoryController : ControllerBase
    {
        // RabbitMQ/CloudMQ setup
        private readonly RabbitMqService _rabbitMqService; // Declare the RabbitMqService

        // Firebase authentication secret
        private static readonly string authSecret = "6WQRS3cA7lh67gJ9uacEX5e3Hnf9Rd9aEQ1QjsYm";
        // Base path for the Firebase database
        private static readonly string basePath = "https://dls-assignment-efd5b-default-rtdb.europe-west1.firebasedatabase.app";
        IFirebaseClient client;
        // Configuration for Firebase client
        IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = authSecret,
            BasePath = basePath
        };

        // Get all entries from the database
        [HttpGet]
        public string GetAll()
        {
            var propagator = new TraceContextPropagator();
            var contextToInject = HttpContext.Request.Headers;
            var parentContext = propagator.Extract(default, contextToInject, (r, key) =>
            {
                return new List<string>(new[] { r.ContainsKey(key) ? r[key].ToString() : String.Empty });
            });
            Baggage.Current = parentContext.Baggage;

            using (var activity = MonitorService.ActivitySource.StartActivity("History service GETALL received", ActivityKind.Consumer, parentContext.ActivityContext))
            {
                client = new FireSharp.FirebaseClient(config);
                var dbResponse = client.Get("doc/");
                if (dbResponse.Body == "null")
                {
                    NotFound("No data found in the database");
                }
                var resp = dbResponse.Body;
                return resp;
            }
        }

        // Push new data to the database
        [HttpPost("DatabasePush")]
        public IActionResult DatabasePush([FromBody] CalculationData data)
        {
            var propagator = new TraceContextPropagator();
            var contextToInject = HttpContext.Request.Headers;
            var parentContext = propagator.Extract(default, contextToInject, (r, key) =>
            {
                return new List<string>(new[] { r.ContainsKey(key) ? r[key].ToString() : String.Empty });
            });
            Baggage.Current = parentContext.Baggage;

            using (var activity = MonitorService.ActivitySource.StartActivity("History service DATABASEPUSH received", ActivityKind.Consumer, parentContext.ActivityContext))
            {
                client = new FireSharp.FirebaseClient(config);
                MonitorService.Log.Here().Debug("Entered DatabasePush with {Data}", data);

                if (client != null && !string.IsNullOrEmpty(basePath) && !string.IsNullOrEmpty(authSecret))
                {
                    MonitorService.Log.Here().Debug("{Client} existing", client);
                    var newData = new
                    {
                        Id = data.Id.ToString(),
                        ListOfNumbers = data.ListOfNumbers,
                        Operation = data.Operation.ToString(),
                        Result = data.Result,
                        Time = data.Time
                    };

                    if (data != null)
                    {
                        MonitorService.Log.Here().Debug("{Data} data is not null: ", data);
                        try
                        {
                            string jsonString = JsonSerializer.Serialize(newData);
                            MonitorService.Log.Here().Debug("{JSON} data serialized: ", jsonString);
                            var resp = client.Push("doc/", jsonString);
                        }
                        catch (Exception ex)
                        {
                            MonitorService.Log.Here().Error("Error pushing to Firebase: {Message}", ex.Message);
                            return BadRequest(ex.Message);
                        }
                    }
                }
                return Ok();
            }
        }
    }
}
