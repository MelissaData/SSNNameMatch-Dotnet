﻿using Newtonsoft.Json;
using System.Runtime.Intrinsics.X86;

namespace SSNNameMatchDotnet
{
  static class Program
  {
    static void Main(string[] args)
    {
      string baseServiceUrl = @"https://namessn.melissadata.net/";
      string serviceEndpoint = @"v4/web/SSN/doLookup"; //please see https://www.melissa.com/developer/ssn-name-match for more endpoints
      string license = "";
      string ssn = "";

      ParseArguments(ref license, ref ssn, args);
      CallAPI(baseServiceUrl, serviceEndpoint, license, ssn);
    }

    static void ParseArguments(ref string license, ref string ssn, string[] args)
    {
      for (int i = 0; i < args.Length; i++)
      {
        if (args[i].Equals("--license") || args[i].Equals("-l"))
        {
          if (args[i + 1] != null)
          {
            license = args[i + 1];
          }
        }
        if (args[i].Equals("--ssn"))
        {
          if (args[i + 1] != null)
          {
            ssn = args[i + 1];
          }
        }
      }
    }

    public static async Task GetContents(string baseServiceUrl, string requestQuery)
    {
      HttpClient client = new HttpClient();
      client.BaseAddress = new Uri(baseServiceUrl);
      HttpResponseMessage response = await client.GetAsync(requestQuery);

      string text = await response.Content.ReadAsStringAsync();

      var obj = JsonConvert.DeserializeObject(text);
      var prettyResponse = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);

      // Print output
      Console.WriteLine("\n==================================== OUTPUT ====================================\n");
      
      Console.WriteLine("API Call: "); 
      string APICall = Path.Combine(baseServiceUrl, requestQuery);
      for (int i = 0; i < APICall.Length; i += 70)
      {
        try
        {
          Console.WriteLine(APICall.Substring(i, 70));
        }
        catch
        {
          Console.WriteLine(APICall.Substring(i, APICall.Length - i));
        }
      }

      Console.WriteLine("\nAPI Response:");
      Console.WriteLine(prettyResponse);
    }
    
    static void CallAPI(string baseServiceUrl, string serviceEndPoint, string license, string ssn)
    {
      Console.WriteLine("\n================== WELCOME TO MELISSA SSN NAME MATCH CLOUD API =================\n");

      bool shouldContinueRunning = true;
      while (shouldContinueRunning)
      {
        string inputSSN = "";

        if (string.IsNullOrEmpty(ssn))
        {
          Console.WriteLine("\nFill in each value to see results");

          Console.Write("SSN: ");
          inputSSN = Console.ReadLine();
        }
        else
        {
          inputSSN = ssn;
        }

        while (string.IsNullOrEmpty(inputSSN))
        {
          Console.WriteLine("\nFill in missing required parameter");

          if (string.IsNullOrEmpty(inputSSN))
          {
            Console.Write("SSN: ");
            inputSSN = Console.ReadLine();
          }
        }

        Dictionary<string, string> inputs = new Dictionary<string, string>()
        {
            { "format", "json"},
            { "SSN", inputSSN},     
        };

        Console.WriteLine("\n===================================== INPUTS ===================================\n");
        Console.WriteLine($"\t   Base Service Url: {baseServiceUrl}");
        Console.WriteLine($"\t  Service End Point: {serviceEndPoint}");
        Console.WriteLine($"\t                SSN: {inputSSN}");

        // Create Service Call
        // Set the License String in the Request
        string RESTRequest = "";

        RESTRequest += @"&id=" + Uri.EscapeDataString(license);

        // Set the Input Parameters
        foreach (KeyValuePair<string, string> kvp in inputs)
          RESTRequest += @"&" + kvp.Key + "=" + Uri.EscapeDataString(kvp.Value);

        // Build the final REST String Query
        RESTRequest = serviceEndPoint + @"?" + RESTRequest;

        // Submit to the Web Service. 
        bool success = false;
        int retryCounter = 0;

        do
        {
          try //retry just in case of network failure
          {
            GetContents(baseServiceUrl, $"{RESTRequest}").Wait();
            Console.WriteLine();
            success = true;
          }
          catch (Exception ex)
          {
            retryCounter++;
            Console.WriteLine(ex.ToString());
            return;
          }
        } while ((success != true) && (retryCounter < 5));

        bool isValid = false;
        if (!string.IsNullOrEmpty(ssn))
        {
          isValid = true;
          shouldContinueRunning = false;
        }

        while (!isValid)
        {
          Console.WriteLine("\nTest another record? (Y/N)");
          string testAnotherResponse = Console.ReadLine();

          if (!string.IsNullOrEmpty(testAnotherResponse))
          {
            testAnotherResponse = testAnotherResponse.ToLower();
            if (testAnotherResponse == "y")
            {
              isValid = true;
            }
            else if (testAnotherResponse == "n")
            {
              isValid = true;
              shouldContinueRunning = false;
            }
            else
            {
              Console.Write("Invalid Response, please respond 'Y' or 'N'");
            }
          }
        }
      }
      
      Console.WriteLine("\n===================== THANK YOU FOR USING MELISSA CLOUD API ====================\n");
    }
  }
}