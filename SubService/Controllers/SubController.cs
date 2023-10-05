using Microsoft.AspNetCore.Mvc;
using Monitoring;

namespace SubService.Controllers;

[ApiController]
[Route("[controller]")]
public class SubController : ControllerBase
{
    [HttpGet]
    public int Get([FromQuery] List<int> input)
    {
        //MonitorService.Log.Verbose("Entered Sub method");

        if (input == null || !input.Any())
        {
            //MonitorService.Log.Error("Sub will not work - invalid input");
            BadRequest("Invalid input");
            return 0;
        }

        Console.WriteLine(Environment.MachineName);
        var result = input[0];
        for (int i = 1; i < input.Count; i++)
        {
            result -= input[i];
        }

        return result;
    }

}

