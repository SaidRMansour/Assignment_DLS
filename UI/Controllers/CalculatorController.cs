using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Polly;
using Polly.Retry;
using RestSharp;
using UI.Models;

namespace UI.Controllers;

public class CalculatorController : Controller
{
    private static RestClient restClient = new RestClient("http://add-service/Add");
    //private static RestClient restClientSub = new RestClient("http://sub-service/Sub");

    private static readonly AsyncRetryPolicy retryPolicy = Policy
        .Handle<Exception>()
        .RetryAsync(3, (exception, retryCount) =>
        {
            Console.WriteLine($"Retry {retryCount} due to {exception.Message}");
        });

    [HttpGet]
    public IActionResult Index()
    {
        return View(new CalculationModel());
    }

    [HttpPost]
    public IActionResult Calculate(CalculationModel model, string operation)
    {
        string[] numbersArray = model.Numbers.Split(',');
        List<int> numbersList = new List<int>();
        foreach (string numberString in numbersArray)
        {
            if (int.TryParse(numberString, out int number))
            {
                numbersList.Add(number);
            }
            else
            {
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        if (operation == "add")
        {
            model.Result = FetchAdd(numbersList);
        }
        //else if (operation == "subtract")
        //{
        //    model.Result = FetchSub(numbersList);
        //}

        // Save operation in history if needed

        return View("Index", model);
    }
   

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private static int FetchAdd(List<int> input)
    {
        string test = "1";
        var task = restClient.GetAsync<int>(new RestRequest("/add?input="+test));

        Console.WriteLine(task.Status);
        Console.WriteLine(task.Status);
        {
            if (task?.Status == TaskStatus.WaitingForActivation)
            {
                var stringTask = task.Result;

                try
                {
                    var addNumber = Convert.ToInt32(stringTask);
                    return addNumber;
                }
                catch (Exception)
                {
                    Console.WriteLine($"Unable to convert {stringTask} to int");
                }

            }
            return 0;
        }
        

    }
    //private static int FetchSub(List<int> input)
    //{

    //    var task = restClientSub.GetAsync<int>(new RestRequest("?input=" + input));

    //    if (task?.Status == TaskStatus.RanToCompletion)
    //    {
    //        var stringTask = task.Result;

    //        try
    //        {
    //            var subNumber = Convert.ToInt32(stringTask);
    //            return subNumber;
    //        }
    //        catch (Exception)
    //        {
    //            Console.WriteLine($"Unable to convert {stringTask} to int");
    //        }

    //    }
    //    return 0;

    //}
}
