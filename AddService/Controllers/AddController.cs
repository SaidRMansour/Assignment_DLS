using System.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Monitoring;
using MySqlConnector;
using Newtonsoft.Json;

namespace AddService.Controllers;

[ApiController]
[Route("[controller]")]
public class AddController : ControllerBase
{

    [HttpGet]
    public int Get([FromQuery] List<int> input)
    {
        using (var activity = MonitorService.ActivitySource.StartActivity())
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
