// See https://aka.ms/new-console-template for more information

//#define LOCAL

using MessageClient_BaseCode;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

#region Global Constants

/// <summary>
/// HttpClient for sending Requests to the webserver API.
/// the BaseAddress is the root url for the webserver that is hosting the API for this project.
/// </summary>
HttpClient httpClient = new HttpClient() { BaseAddress = new Uri(
#if LOCAL
    "http://localhost:5190/"
#else
    "https://csplayground.winsor.edu/"
#endif
    ) };

#endregion // Global Constants


#region Program

Console.Write("Register a new account? [y/N] ");
AuthorizationResponse authResponse;

if (Console.ReadLine()!.ToLowerInvariant().StartsWith("y"))
{
    var info = RegisterAccount();
    if (info is not null)
    {
        Console.WriteLine($"Your new account has ID:  {info.clientId}");
        authResponse = info!;
    }
    else
    {
        Console.WriteLine("Authenticaiton Failed");
        return; // end program.
    }
}
else
{
    Console.Write("Login: ");
    var userName = Console.ReadLine()!;
    Console.Write("Passcode:  ");
    var pwd = Console.ReadLine()!;

    var attempt = GetAuthorization(userName, pwd);
    if(attempt is not null)
    {
        authResponse = attempt!;
    }
    else
    {
        Console.WriteLine("Authenticaiton Failed");
        return; // end program.
    }
}

string choice = "";

while (choice != "quit")
{
    Console.WriteLine("What would you like to do? [send/CHECK] mail");
    choice = Console.ReadLine()!.ToLowerInvariant();

    if (choice == "send")
    {
        Console.WriteLine("Who would you like to send a message to? (Type a list of user names separated by commas)");
        string recipString = Console.ReadLine()!;

        //take the comma separated list of names, split on commas, and throw away leading and trailing spaces.
        var recipients = recipString.Split(',').Select(recip => recip.Trim());

        Console.WriteLine("Enter your message!");
        string messageText = Console.ReadLine()!;

        var messageData = Encoding.UTF32.GetBytes(messageText);

        SendMessage(authResponse.clientId, recipients, messageData);
    }
    else if(choice == "check")
    {
        foreach(var message in GetMessages())
        {
            Console.WriteLine($"From: {message.sender}");
            Console.Write($"To: ");
            foreach (var recipient in message.recipients)
                Console.Write($"{recipient} ");
            Console.WriteLine();

            var messageText = Encoding.UTF32.GetString(message.message);
            Console.WriteLine(messageText);
            Console.WriteLine("____________________________________________________________");
        }
    }
    else if(choice != "quit")
    {
        Console.WriteLine("Please type: 'send', 'check', or 'quit'");
    }
}



#endregion // The Program Execution Ends Here

// This is where Helper methods are declared so you can use them in the program above!
#region Helper Methods

IEnumerable<GetMessageRecord> GetMessages()
{
    // check out the Endpoint in this call.  this is how you embed your authentication token in the request!
    var response = ApiCall<IEnumerable<GetMessageRecord>>(HttpMethod.Get, $"api/messages/{authResponse.clientId}?authToken={Uri.EscapeDataString(authResponse.token)}");
    if (response is null) // if there was an error, return an empty list.
        return Array.Empty<GetMessageRecord>();

    return response;
}

void SendMessage(string senderId, IEnumerable<string> recipients, byte[] messageData)
{
    Message message = new(senderId, recipients, messageData);
    string jsonData = JsonSerializer.Serialize(message);
    // check out the Endpoint in this call.  this is how you embed your authentication token in the request!
    var response = StringApiCall(HttpMethod.Post, $"api/messages?authToken={Uri.EscapeDataString(authResponse.token)}", jsonData);
    if (!string.IsNullOrEmpty(response))
        Debug.WriteLine($"Success:  Message Id {response}");
}

AuthorizationResponse? GetAuthorization(string username, string pwd)
{
    LoginRequestRecord login = new(username, pwd);
    string jsonData = JsonSerializer.Serialize(login);
    return ApiCall<AuthorizationResponse>(HttpMethod.Post,"api/auth", jsonData);
}

AuthorizationResponse? RegisterAccount()
{
    string username, realName, pwd;
    Console.WriteLine("Enter the username you want to use:");
    username = Console.ReadLine()!;

    Console.WriteLine("Enter your real name:");
    realName = Console.ReadLine()!;

    Console.WriteLine("Enter a passcode [THIS IS NOT SECURE, DON'T USE A REAL PASSWORD]:");
    pwd = Console.ReadLine()!;

    CreateClientRecord record = new(username, realName, pwd);
    string jsonData = JsonSerializer.Serialize(record);
    ApiCall<ClientRecord>(HttpMethod.Post, "api/clients", jsonData);

    return GetAuthorization(username, pwd);
}


string StringApiCall(HttpMethod method, string endpoint, string? jsonContent = null)
{
    HttpRequestMessage request = new HttpRequestMessage(method, endpoint);
    if (jsonContent is not null)
        request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

    HttpResponseMessage response = httpClient.SendAsync(request).Result;
    

    string message = response.Content.ReadAsStringAsync().Result;

    Debug.WriteLine(message);
    return message;
}

T? ApiCall<T>(HttpMethod method, string endpoint, string? jsonContent = null)
{
    HttpRequestMessage request = new HttpRequestMessage(method, endpoint);
    if(jsonContent is not null)
        request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

    HttpResponseMessage response = httpClient.SendAsync(request).Result;
    if (!response.IsSuccessStatusCode)
    {
        string message = response.Content.ReadAsStringAsync().Result;
        Console.WriteLine(message);
        return default(T);
    }

    T? result = default(T);

    if (!typeof(T).IsPrimitive)
         result = (T?)JsonSerializer.Deserialize(response.Content.ReadAsStream(), typeof(T));

    Debug.WriteLine(result);
    return result;
}

#endregion // Helper Methods
