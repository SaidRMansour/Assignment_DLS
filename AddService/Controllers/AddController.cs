using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Text;
using Dapper;
using FireSharp.Config;
using FireSharp.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Monitoring;
using MySqlConnector;
using Newtonsoft.Json;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using SharedModels.Models;

namespace AddService.Controllers;

[ApiController]
[Route("[controller]")]
public class AddController : ControllerBase
{
    
    [HttpGet]
    public async Task<int> Get([FromQuery] List<int> input)
    {
        var propagator = new TraceContextPropagator();
        var contextToInject = HttpContext.Request.Headers;
        var parentContext = propagator.Extract(default, contextToInject, (r, key) =>
        {
            return new List<string>(new[] { r.ContainsKey(key) ? r[key].ToString() : String.Empty });
        });
        Baggage.Current = parentContext.Baggage;

        using (var activity = MonitorService.ActivitySource.StartActivity("Get Add service recieved", ActivityKind.Consumer, parentContext.ActivityContext))
        {
            MonitorService.Log.Here().Debug("Entered Add method with {Input}", input);
            if (input == null || !input.Any())
            {
                MonitorService.Log.Here().Error("Invalid input - List empty or is null");
                BadRequest("Invalid input");
                return 0;
            }
            Console.WriteLine(Environment.MachineName);
            var result = input.Sum();
            MonitorService.Log.Here().Debug("Add method calculated this result: {Result}", result);

            return result;
        }

    }


}
