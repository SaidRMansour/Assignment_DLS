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
    private static IDbConnection addDB = new MySqlConnection("Server=add-db;Database=add-database;Uid=addDB;Pwd=S@adb912;");

    public AddController()
    {
        addDB.Open();
        var tables = addDB.Query<string>("SHOW TABLES LIKE 'addOperation'");
        if (!tables.Any())
        {
            addDB.Execute("CREATE TABLE addOperation(numberOfList VARCHAR(255) NOT NULL PRIMARY KEY, operation VARCHAR(255) NOT NULL, result INT NOT NULL)");
        }
    }

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

            //After calculating the result save it in the db:
            var inputAsJson = JsonConvert.SerializeObject(input); // Serialize list to JSON string
            addDB.Execute("INSERT INTO addOperation (numberOfList, operation, result) VALUES (@numberOfList, @operation, @result)",
                          new { numberOfList = inputAsJson, operation = "add", result = result });

            // return the result to the db
            return result;
        }

    }
    [HttpGet]
    public IEnumerable<AddOperation> GetAll()
    {
        var allOperationsAdd = addDB.Query<AddOperation>("SELECT * FROM addOperation");
        return allOperationsAdd;
    }


}

public class AddOperation
{
    public string NumberOfList { get; set; }
    public string Operation { get; set; }
    public int Result { get; set; }
}
