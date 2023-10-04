using Microsoft.AspNetCore.Mvc;

namespace SubService.Controllers;

[ApiController]
[Route("[controller]")]
public class SubController : ControllerBase
{
    [HttpGet]
    public int Get([FromQuery] List<int> input)
    {
        if (input == null || !input.Any())
        {
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

