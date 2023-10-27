using System.Diagnostics;
using FireSharp.Config;
using FireSharp.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Monitoring;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

namespace SubService.Controllers;

[ApiController]
[Route("[controller]")]
public class SubController : ControllerBase
{
  
    [HttpGet]
    public int Get([FromQuery] List<int> input)
    {
        var propagator = new TraceContextPropagator();
        var contextToInject = HttpContext.Request.Headers;
        var parentContext = propagator.Extract(default, contextToInject, (r, key) =>
        {
            return new List<string>(new[] { r.ContainsKey(key) ? r[key].ToString() : String.Empty });
        });
        Baggage.Current = parentContext.Baggage;

        using (var activity = MonitorService.ActivitySource.StartActivity("Get Sub service recieved", ActivityKind.Consumer, parentContext.ActivityContext))
        {
            MonitorService.Log.Here().Verbose("Entered Sub method with {Input}", input);

            if (input == null || !input.Any())
            {
                MonitorService.Log.Here().Error("Invalid input - List empty or is null");
                BadRequest("Invalid input");
                return 0;
            }

            Console.WriteLine(Environment.MachineName);
            var result = input[0];
            for (int i = 1; i < input.Count; i++)
            {
                result -= input[i];
            }
            MonitorService.Log.Here().Debug("Sub method calculated this result: {Result}", result);
            return result;
        }
       
    }


}

