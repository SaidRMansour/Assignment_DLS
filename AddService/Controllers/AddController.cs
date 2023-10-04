using Microsoft.AspNetCore.Mvc;

namespace AddService.Controllers;

[ApiController]
[Route("[controller]")]
public class AddController : ControllerBase
{
    [HttpGet]
    public int Get([FromQuery] List<int> input)
    {
        if(input == null || !input.Any())
        {
            BadRequest("Invalid input");
            return 0;
        }
        Console.WriteLine(Environment.MachineName);
        var result = input.Sum();

        return result;
    }
}
