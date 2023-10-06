using System.Collections;
using System.Data;
using Dapper;
using FireSharp.Config;
using FireSharp.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Monitoring;
using MySqlConnector;
using Newtonsoft.Json;

namespace AddService.Controllers;

[ApiController]
[Route("[controller]")]
public class AddController : ControllerBase
{
    private static readonly string authSecret = "6WQRS3cA7lh67gJ9uacEX5e3Hnf9Rd9aEQ1QjsYm";
    private static readonly string basePath = "https://dls-assignment-efd5b-default-rtdb.europe-west1.firebasedatabase.app";

    IFirebaseClient client;
    IFirebaseConfig config = new FirebaseConfig
    {
        AuthSecret = authSecret,
        BasePath = basePath
    };

    [HttpGet]
    public int Get([FromQuery] List<int> input)
    {
        client = new FireSharp.FirebaseClient(config);
        
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
            //DB
            if (client != null && !string.IsNullOrEmpty(basePath) && !string.IsNullOrEmpty(authSecret))
            {
                var data = new
                {
                    ListOfNumbers = input,
                    Operation = "Add",
                    Result = result
                };
                var response = client.Push("doc/", data);

            }

            return result;
        }

    }

    [HttpGet("GetAll")]
    public string GetAll()
    {
        client = new FireSharp.FirebaseClient(config);

        // Hent alle elementer fra "docs/"-noden
        var dbResponse = client.Get("docs/");
        if (dbResponse.Body == "null") // Check hvis databasen er tom
        {
            NotFound("No data found in the database");
        }

        var resp = dbResponse.Body;
        return resp;
    }
    

}
