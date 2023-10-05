using Microsoft.AspNetCore.Mvc;
using Monitoring;

namespace AddService.Controllers;

[ApiController]
[Route("[controller]")]
public class AddController : ControllerBase
{
    [HttpGet]
    public int Get(string input)
    {
        return Convert.ToInt32(input);
    }
    //public int Get(List<int> input)
    //{
    //    MonitorService.Log.Debug("Entered Add method");
    //    if (input == null || !input.Any())
    //    {
    //        MonitorService.Log.Error("Sub will not work - invalid input");
    //        BadRequest("Invalid input");
    //        return 0;
    //    }
    //    Console.WriteLine(Environment.MachineName);
    //    var result = input.Sum();

    //    return result;
    //}
}
