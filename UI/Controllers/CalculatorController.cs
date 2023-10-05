using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Polly;
using Polly.Retry;
using RestSharp;
using UI.Models;

namespace UI.Controllers;

public class CalculatorController : Controller
{
    private readonly IHttpClientFactory _clientFactory;

    public CalculatorController(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Calculate(string numberInput, string operation)
    {
        List<int> numbers;
        try
        {
            numbers = numberInput.Split(',').Select(int.Parse).ToList();
        }
        catch (FormatException)
        {
            ViewBag.Error = "Invalid number format.";
            return View("Index");
        }

        var client = _clientFactory.CreateClient("MyClient");
        var queryString = string.Join("&input=", numbers.Prepend(0));

        string result = "";
        if (operation == "Add")
        {
            result = await client.GetStringAsync($"http://adding-service/Add?{queryString}");
        }
        else if (operation == "Subtract")
        {
            result = await client.GetStringAsync($"http://subing-service/Sub?{queryString}");
        }
        else
        {
            ViewBag.Error = "Invalid operation.";
            return View("Index");
        }

        ViewBag.Result = result;
        return View("Index");
    }
    
}
