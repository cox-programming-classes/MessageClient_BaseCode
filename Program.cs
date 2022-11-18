
using MessageClient_BaseCode;
using System.Text;

// this line just says take all the things I wrote in that file ApiInterface.cs
// and let me use it as if I wrote it all right in this file.
// take a look at that file to see what those methods are actually doing.
using static MessageClient_BaseCode.ApiInterface;



#region Program

// Frist thing we must do is login.
// If you have not created an account, then you need to do that.
// once you have registered an account, you can delete this first if statement and just do the Login step every time.
// it's bad practice in general, but you could even hard code in your username and passcode so you don't have to type
// them every time you start the program.
Console.Write("Register a new account? [y/N] ");

// short hand way to check if a person typed a "yes" word.  (all you have to do is type 'y')
// anything that doesn't start with 'y' is assumed to be a Negative response.
if (Console.ReadLine()!.ToLowerInvariant().StartsWith("y"))
{
    Console.WriteLine("Enter the username you want to use:");
    string username = Console.ReadLine()!;

    Console.WriteLine("Enter your real name:");
    string realName = Console.ReadLine()!;

    Console.WriteLine("Enter a passcode [THIS IS NOT SECURE, DON'T USE A REAL PASSWORD]:");
    string pwd = Console.ReadLine()!;
    // This calls the API and creates a user for you.
    // There are some rules though, so you might not get what what you want!
    // This also logs you in as the newly created user.
    RegisterAccount(username, realName, pwd);
}
else
{
    // if you said no, then you have to login.
    Console.Write("Login: ");
    var userName = Console.ReadLine()!;
    // again, this password is going to be sent in plain-text.  Don't use a real password.
    Console.Write("Passcode:  ");
    Console.ForegroundColor = Console.BackgroundColor;
    var pwd = Console.ReadLine()!;
    Console.ResetColor();

    // Call the API auth endpoint and get a login token back.
    // this is stored in the variable AuthResponse
    Login(userName, pwd);
}

// Set a break point on this if statement to be able to inspect that AuthResponse token!
if (AuthResponse is null)
{
    // if this response is null, it means you failed to login (and it should have printed a message why)
    // since you can't do anything without logging in (except register) the program will end now.
    Console.WriteLine("Cannot proceed without logging in.");
    return; // exit the program.
}

// here's a loop based on your choice input.
string choice = "";

// keep going around until you quit.
while (choice != "quit")
{
    Console.WriteLine("What would you like to do? [send] or [check] mail, or [quit]");
    choice = Console.ReadLine()!.ToLowerInvariant();

    // in this case, you need to type out the whole word, not just the first letter.
    if (choice == "send")
    {
        // you need to know the usernames of the people you want to send a message to.
        // if you don't know someone's name, you can't send them a message!
        // There is a way to find out peoples names though~
        Console.WriteLine("Who would you like to send a message to? (Type a list of user names separated by commas)");
        string recipString = Console.ReadLine()!;

        //take the comma separated list of names, split on commas, and throw away leading and trailing spaces.
        var recipients = recipString.Split(',').Select(recip => recip.Trim());

        Console.WriteLine("Enter your message!");
        string messageText = Console.ReadLine()!;
        
        // convert the message to raw data before you send it.
        // because a message contains raw data, there's nothing that says you MUST send text!
        // you could just as easily send any other kind of data, files, or even complex objects.
        var messageData = Encoding.UTF32.GetBytes(messageText);

        // Call the API Endpoint that sends the message to all the people you indicated.
        // if you didn't provide enough information, then the request will fail and it will
        // give you an explanation of the problem.
        SendMessage(AuthResponse.clientId, recipients, messageData);
    }
    else if(choice == "check")
    {
        // the method GetMessages() calls the API and gets all your unread messages using your currently logged in
        // account.  It gets that info from the AuthResponse object that is saved in the ApiInterface class.
        foreach(var message in GetMessages())
        {
            // display each message on your screen.
            Console.WriteLine($"From: {message.sender}");
            Console.Write($"To: ");
            foreach (var recipient in message.recipients)
                Console.Write($"{recipient} ");
            Console.WriteLine();

            // right now this is assuming that the message is plain text.  If the message contained data other than
            // plain text, this is going to display nonsense because it is Decoding the raw binary data as UTF32 text.
            var messageText = Encoding.UTF32.GetString(message.message);
            Console.WriteLine(messageText);
            Console.WriteLine("____________________________________________________________");
        }
    }
    else if(choice != "quit")
    {
        // you typed something else... maybe we want to add more commands here later?
        Console.WriteLine("Please type: 'send', 'check', or 'quit'");
    }
}
#endregion // The Program Execution Ends Here

// This is where Helper methods are declared so you can use them in the program above!
#region Helper Methods


#endregion // Helper Methods
