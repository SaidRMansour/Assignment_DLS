using FireSharp.Config;
using FireSharp.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Monitoring;
using SharedModels.Models;

namespace HistoryService.Controllers;

[ApiController]
[Route("[controller]")]
public class HistoryController : ControllerBase
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
    public string GetAll()
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

