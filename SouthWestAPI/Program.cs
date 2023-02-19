using System.Net;

namespace SouthWestAPI;

class Program
{
    public static async Task Main(String[] args)
    {
        //args 0 is flight num
        //args 1 is first name
        //args 2 is last name
        
        // If any of the arguements are invalid, we shouldn't let them pass
        if (String.IsNullOrWhiteSpace(args[0]) || String.IsNullOrWhiteSpace(args[1]) ||
            String.IsNullOrWhiteSpace(args[2]))
        {
            Console.WriteLine("Invalid arguments");
            return;
        }
        
        // Get js file
        HttpClient client = new();
        var response = await client.GetAsync("https://mobile.southwest.com/js/config.js");

        // Something is probably wrong with url if this passes
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Failed to get API key with error code {response.StatusCode.ToString()}");
            return;
        }

        // Get the API key from the body so we need to dump the body into a string
        var responseBody = await response.Content.ReadAsStringAsync();

        // Pull API key from response
        int apiLocation = responseBody.IndexOf("API_KEY:\"", StringComparison.Ordinal) + 9;
        string api = responseBody.Substring(apiLocation);
        api = api.Substring(0, api.IndexOf("\"", StringComparison.Ordinal));

        // We need a user key
        string uuid = Guid.NewGuid().ToString();
        
        // Reset the httpClient
        client.Dispose();
        var clientHandler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }; 
        client = new(clientHandler);
        
        // Add all the headers required
        //client.DefaultRequestHeaders.Add("Host", "mobile.southwest.com");
        //client.DefaultRequestHeaders.Add("Content-Type", "application/json");
        client.DefaultRequestHeaders.Add("X-API-Key", api);
        client.DefaultRequestHeaders.Add("X-User-Experience-Id", uuid);
        client.DefaultRequestHeaders.Add("Accept", "*/*");
        client.DefaultRequestHeaders.Add("X-Channel-ID", "MWEB");
        //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        
        var flightResponse = await client.GetAsync(
            $"https://mobile.southwest.com/api/mobile-air-operations/v1/mobile-air-operations/page/check-in/{args[0]}?first-name={args[1]}&last-name={args[2]}");

        var flightDataResponse = await flightResponse.Content.ReadAsStringAsync();
        if (!flightResponse.IsSuccessStatusCode)
        {
            if (flightDataResponse.Contains("Come back"))
            {
                string comeBackDate = flightDataResponse.Substring(flightDataResponse.IndexOf("Come back"));
                comeBackDate = comeBackDate.Substring(0, comeBackDate.IndexOf("\""));
                Console.WriteLine(comeBackDate);
                return;
            }
            Console.WriteLine("Flight currently can't be checked in.");
            return;
        }
        
    }


   
}