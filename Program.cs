// See https://aka.ms/new-console-template for more information
using MessageClient_BaseCode;
using System.Text;

using static MessageClient_BaseCode.ApiInterface;



#region Program

Console.Write("Register a new account? [y/N] ");

if (Console.ReadLine()!.ToLowerInvariant().StartsWith("y"))
{
    Console.WriteLine("Enter the username you want to use:");
    string username = Console.ReadLine()!;

    Console.WriteLine("Enter your real name:");
    string realName = Console.ReadLine()!;

    Console.WriteLine("Enter a passcode [THIS IS NOT SECURE, DON'T USE A REAL PASSWORD]:");
    string pwd = Console.ReadLine()!;
    RegisterAccount(username, realName, pwd);
}
else
{
    Console.Write("Login: ");
    var userName = Console.ReadLine()!;
    Console.Write("Passcode:  ");
    var pwd = Console.ReadLine()!;

    Login(userName, pwd);
}

if (AuthResponse is null)
{
    Console.WriteLine("Cannot proceed without logging in.");
    return;
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

        SendMessage(AuthResponse.clientId, recipients, messageData);
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


#endregion // Helper Methods
