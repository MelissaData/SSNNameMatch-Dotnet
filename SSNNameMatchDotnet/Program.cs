using Newtonsoft.Json;
using System.Runtime.Intrinsics.X86;

namespace SSNNameMatchDotnet
{
  /// <summary>
  /// SSN Name Match looks up a Social Security Number and returns information about it,
  /// such as the issuing state, along with result codes that describe the outcome of the
  /// lookup.
  ///
  /// <para>High-level flow of this sample:</para>
  /// <list type="number">
  ///   <item><description>ARGS    - ParseArguments reads any --flag values off the command line.</description></item>
  ///   <item><description>INPUT   - CallAPI prompts for the SSN if it wasn't supplied.</description></item>
  ///   <item><description>REQUEST - CallAPI builds the REST query string (license + SSN).</description></item>
  ///   <item><description>CALL    - GetContents issues the GET request and pretty-prints the JSON response.</description></item>
  /// </list>
  ///
  /// <para>This sample is a thin HTTP client: it builds a query string, sends a GET
  /// request to the SSN Name Match Cloud API, and prints the JSON response.</para>
  ///
  /// <para>Reference:</para>
  /// <list type="bullet">
  ///   <item><description>Documentation: https://docs.melissa.com/cloud-api/ssn-name-match/ssn-name-match-index.html</description></item>
  ///   <item><description>Release notes: https://releasenotes.melissa.com/cloud-api/ssn-name-match/</description></item>
  ///   <item><description>Result codes: https://docs.melissa.com/melissa/result-codes/result-codes-index.html</description></item>
  /// </list>
  /// </summary>
  static class Program
  {
    /// <summary>
    /// Entry point. Reads the optional command-line arguments, then hands control to
    /// CallAPI, which performs the actual request/response cycle.
    /// </summary>
    /// <param name="args">The raw command-line arguments.</param>
    static void Main(string[] args)
    {
      string baseServiceUrl = @"https://namessn.melissadata.net/";
      string serviceEndpoint = @"v4/web/SSN/doLookup"; //please see https://www.melissa.com/developer/ssn-name-match for more endpoints
      string license = "";
      string ssn = "";

      // Populate any values passed on the command line, then run the lookup.
      ParseArguments(ref license, ref ssn, args);
      CallAPI(baseServiceUrl, serviceEndpoint, license, ssn);
    }

    /// <summary>
    /// Reads the supported command-line options and writes each recognized value into
    /// its matching by-ref parameter. Any parameter left unset here falls back to an
    /// interactive prompt later in <see cref="CallAPI"/> (except the license).
    ///
    /// <para>Recognized flags (each followed by its value, e.g. "--ssn 111223333"):
    /// --license/-l, --ssn.</para>
    /// </summary>
    /// <param name="license">Receives the Melissa license string, if supplied.</param>
    /// <param name="ssn">Receives the SSN to look up, if supplied.</param>
    /// <param name="args">The raw command-line arguments to parse.</param>
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

    /// <summary>
    /// Issues the GET request against the SSN Name Match endpoint and pretty-prints
    /// the API call and the JSON response to the console.
    /// </summary>
    /// <param name="baseServiceUrl">The SSN Name Match Cloud API base URL.</param>
    /// <param name="requestQuery">The endpoint path plus query string built by <see cref="CallAPI"/>.</param>
    public static async Task GetContents(string baseServiceUrl, string requestQuery)
    {
      HttpClient client = new HttpClient();
      client.BaseAddress = new Uri(baseServiceUrl);
      HttpResponseMessage response = await client.GetAsync(requestQuery);

      string text = await response.Content.ReadAsStringAsync();

      // Re-serialize with indentation so the raw response is easier to read.
      var obj = JsonConvert.DeserializeObject(text);
      var prettyResponse = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);

      // Print output
      Console.WriteLine("\n==================================== OUTPUT ====================================\n");
      
      Console.WriteLine("API Call: "); 
      string APICall = Path.Combine(baseServiceUrl, requestQuery);
      for (int i = 0; i < APICall.Length; i += 70)
      {
        if (i + 70 < APICall.Length)
        {
          Console.WriteLine(APICall.Substring(i, 70));
        }
        else
        {
          Console.WriteLine(APICall.Substring(i, APICall.Length - i));
        }
      }

      Console.WriteLine("\nAPI Response:");
      Console.WriteLine(prettyResponse);
    }
    
    /// <summary>
    /// Drives the interactive/CLI loop: gathers the required SSN, builds and submits
    /// the REST query, prints the result, and optionally repeats for another record.
    ///
    /// <para>In interactive mode (no SSN arg) it loops, asking for a new record each pass
    /// until the user answers "N". In one-shot mode (SSN supplied) it runs a single
    /// pass and exits.</para>
    /// </summary>
    /// <param name="baseServiceUrl">The SSN Name Match Cloud API base URL.</param>
    /// <param name="serviceEndPoint">The specific SSN Name Match endpoint path to call.</param>
    /// <param name="license">The Melissa license string sent with every request.</param>
    /// <param name="ssn">An SSN to look up in one-shot mode; if empty, the program prompts interactively.</param>
    static void CallAPI(string baseServiceUrl, string serviceEndPoint, string license, string ssn)
    {
      Console.WriteLine("\n================== WELCOME TO MELISSA SSN NAME MATCH CLOUD API =================\n");

      bool shouldContinueRunning = true;
      while (shouldContinueRunning)
      {
        string inputSSN = "";

        // No SSN was supplied via command line, so prompt for it.
        if (string.IsNullOrEmpty(ssn))
        {
          Console.WriteLine("\nFill in each value to see results");

          Console.Write("SSN: ");
          inputSSN = Console.ReadLine();
        }
        else
        {
          // The SSN was supplied via command line; use it as-is.
          inputSSN = ssn;
        }

        // Keep prompting until the required SSN is non-empty.
        while (string.IsNullOrEmpty(inputSSN))
        {
          Console.WriteLine("\nFill in missing required parameter");

          if (string.IsNullOrEmpty(inputSSN))
          {
            Console.Write("SSN: ");
            inputSSN = Console.ReadLine();
          }
        }

        // Map the input to the API's expected query parameter name and
        // request a JSON response.
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

        // If the SSN came from the command line, treat this as a one-shot run
        // rather than looping for additional records.
        bool isValid = false;
        if (!string.IsNullOrEmpty(ssn))
        {
          isValid = true;
          shouldContinueRunning = false;
        }

        // Otherwise ask whether to test another record. Keep prompting until we get a
        // valid Y/N. "N" ends the program; "Y" falls through to another pass.
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
