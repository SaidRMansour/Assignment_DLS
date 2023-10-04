// See https://aka.ms/new-console-template for more information
using Polly;
using Polly.Retry;


public class Program
{
    public static async Task Main(string[] args)
    {
        List<int> numbers = new List<int> { 10, 4, 4};

        while (true)
        {
            Thread.Sleep(300);
            var add = await FetchAdd(numbers);
            var sub = await FetchSub(numbers);

            Console.WriteLine($"Add result: {add}");
            Console.WriteLine($"Sub result: {sub}");
        }
    }

    private static async Task<int> FetchAdd(List<int> input)
    {
        //NEW upload
        var client = new HttpClient();

        // Convert list of integers to query parameters
        var queryString = string.Join("&", input.Select(i => $"input={i}"));

        var response = await client.GetAsync($"http://add-service/Add?{queryString}");
        var stringTask = await response.Content.ReadAsStringAsync();

        try
        {
            var addNumber = Convert.ToInt32(stringTask);
            return addNumber;
        }
        catch (Exception)
        {
            Console.WriteLine($"Unable to convert {stringTask} to int");
        }

        return 0;
    }

    private static async Task<int> FetchSub(List<int> input)
    {
        var client = new HttpClient();

        // Convert list of integers to query parameters
        var queryString = string.Join("&", input.Select(i => $"input={i}"));

        var response = await client.GetAsync($"http://sub-service/Sub?{queryString}");
        var stringTask = await response.Content.ReadAsStringAsync();

        try
        {
            var subNumber = Convert.ToInt32(stringTask);
            return subNumber;
        }
        catch (Exception)
        {
            Console.WriteLine($"Unable to convert {stringTask} to int");
        }

        return 0;
    }

}
