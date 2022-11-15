using System.Diagnostics;
using System.Net.Mail;
using System.Text;
using System.Text.Json;

namespace MessageClient_BaseCode;

// #define LOCAL

public static class ApiInterface
{

    #region Global Constants

    /// <summary>
    /// HttpClient for sending Requests to the webserver API.
    /// the BaseAddress is the root url for the webserver that is hosting the API for this project.
    /// </summary>
    private static readonly HttpClient HttpClient = new HttpClient()
    {
        BaseAddress = new Uri(
#if LOCAL
    "http://localhost:5190/"
#else
            "https://csplayground.winsor.edu/"
#endif
        )
    };

    /// <summary>
    /// Your current Authorization Token.
    /// When it expires, you will need to renew it!
    /// </summary>
    public static AuthorizationResponse? AuthResponse;

    #endregion // Global Constants

    /// <summary>
    /// Get your unread messages
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<GetMessageRecord> GetMessages()
    {
        // check out the Endpoint in this call.  this is how you embed your authentication token in the request!
        var response = ApiCall<IEnumerable<GetMessageRecord>>(HttpMethod.Get,
            $"api/messages/{AuthResponse!.clientId}?authToken={Uri.EscapeDataString(AuthResponse.token)}");
        if (response is null) // if there was an error, return an empty list.
            return Array.Empty<GetMessageRecord>();

        return response;
    }

    /// <summary>
    /// Send a message to people
    /// </summary>
    /// <param name="senderId">your Id</param>
    /// <param name="recipients">usernames of all the people you want to send a message to.</param>
    /// <param name="messageData">the raw data of your message.</param>
    public static void SendMessage(string senderId, IEnumerable<string> recipients, byte[] messageData)
    {
        Message message = new(senderId, recipients, messageData);
        string jsonData = JsonSerializer.Serialize(message);
        // check out the Endpoint in this call.  this is how you embed your authentication token in the request!
        var response = StringApiCall(HttpMethod.Post,
            $"api/messages?authToken={Uri.EscapeDataString(AuthResponse!.token)}", jsonData);
        if (!string.IsNullOrEmpty(response))
            Debug.WriteLine($"Success:  Message Id {response}");
    }

    /// <summary>
    /// Login!
    /// </summary>
    /// <param name="username"></param>
    /// <param name="pwd">don't use a real password here, this isn't secure.</param>
    /// <returns></returns>
    public static void Login(string username, string pwd)
    {
        LoginRequestRecord login = new(username, pwd);
        string jsonData = JsonSerializer.Serialize(login);
        var result = ApiCall<AuthorizationResponse>(HttpMethod.Post, "api/auth", jsonData);
        if (result is not null)
            AuthResponse = result;
    }

    /// <summary>
    /// Call this to register a new account.  It won't let you do things you aren't allowed to ;)
    /// </summary>
    public static void RegisterAccount(string username, string realName, string pwd)
    {

        CreateClientRecord record = new(username, realName, pwd);
        string jsonData = JsonSerializer.Serialize(record);
        ApiCall<ClientRecord>(HttpMethod.Post, "api/clients", jsonData);

        Login(username, pwd);
    }


    /// <summary>
    /// Call an endpoint that specifically returns a String.  WHY?  Ask in class.
    /// </summary>
    /// <param name="method">What type of API Call?</param>
    /// <param name="endpoint">What Endpoint are you calling</param>
    /// <param name="jsonContent">if you need to provide data to the call, put the string content here.</param>
    /// <returns></returns>
    public static string StringApiCall(HttpMethod method, string endpoint, string? jsonContent = null)
    {
        HttpRequestMessage request = new HttpRequestMessage(method, endpoint);
        if (jsonContent is not null)
            request.Content = new StringContent(jsonContent, Encoding.UTF32, "application/json");

        HttpResponseMessage response = HttpClient.SendAsync(request).Result;


        string message = response.Content.ReadAsStringAsync().Result;

        Debug.WriteLine(message);
        return message;
    }

    /// <summary>
    /// Make an API Call where you expect a repsonse back.
    /// </summary>
    /// <param name="method">What kind of request is this?</param>
    /// <param name="endpoint">What endpoint are you calling?</param>
    /// <param name="jsonContent">if you need to provide data, send it here in JSON format</param>
    /// <typeparam name="T">The DataType of your expected response.  you should probably have a record struct defined to catch this.</typeparam>
    /// <returns>A nullable reference to the data that the API Call returned.  If the call was unsuccessful this will be null.</returns>
    public static T? ApiCall<T>(HttpMethod method, string endpoint, string? jsonContent = null)
    {
        // Start an HTTP Request by providing the method and endpoint relative to the base URL
        HttpRequestMessage request = new HttpRequestMessage(method, endpoint);
        
        // if there is data to add to the request, then insert it here.  This expects JSON text format.
        if (jsonContent is not null)
            request.Content = new StringContent(jsonContent, Encoding.UTF32, "application/json");

        // Send the request and wait for the server to respond.  This is a blocking statement and will take 
        // as long as it takes to get the response.
        HttpResponseMessage response = HttpClient.SendAsync(request).Result;
        
        // Check to make sure that the request was successful.
        // for a reference of what different status codes mean, check out https://http.cat
        if (!response.IsSuccessStatusCode)
        {
            // read the error response, the API should provide meaningful information unless you really fed it something
            // unexpected.  A response code of "UnprocessableEntitiy" means something was uninteligible.
            string message = response.Content.ReadAsStringAsync().Result;
            // print the error
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"API Error Returned: {response.StatusCode} {response.ReasonPhrase}");
            Console.WriteLine(message);
            Console.ResetColor();
            return default(T);
        }

        // make a place to catch the result of this object.
        T? result = default(T);

        // if the type you passed is a Primitive, then this is going to fail, so don't even try.
        if (!typeof(T).IsPrimitive)
            try
            {
                result = (T?) JsonSerializer.Deserialize(response.Content.ReadAsStream(), typeof(T));
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed to Deserialize the response from API Web Server....");
                Console.WriteLine(e);
                Console.ResetColor();
            }

        Debug.WriteLine(result);
        return result;
    }

}