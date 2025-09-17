using System.Net.Sockets;

Console.WriteLine(@"
 _______ _____ _____     _____ _ _            _   
|__   __/ ____|  __ \   / ____| (_)          | |  
   | | | |    | |__) | | |    | |_  ___ _ __ | |_ 
   | | | |    |  ___/  | |    | | |/ _ \ '_ \| __|
   | | | |____| |      | |____| | |  __/ | | | |_ 
   |_|  \_____|_|       \_____|_|_|\___|_| |_|\__|
");

const string ServerAddress = "127.0.0.1"; // Localhost
const int Port = 5000; // Same as the server

using var client = new TcpClient();
await client.ConnectAsync(ServerAddress, Port);
using var stream = client.GetStream();
using var reader = new StreamReader(stream);
using var writer = new StreamWriter(stream) { AutoFlush = true };

Console.WriteLine("Connected to server!");
Console.WriteLine("Type 'Random', 'Add', or 'Subtract'. 'Quit' to exit. \n");

await ProcessCommandsAsync(reader, writer);

static async Task ProcessCommandsAsync(StreamReader reader, StreamWriter writer)
{
    // Ask the server 3 times and stop
    for (int i = 0; i < 3; i++)
    {
        Console.Write("Enter command: ");
        string? command = Console.ReadLine();

        if (string.IsNullOrEmpty(command))
        {
            i--; // Empty input doesn't count
            continue;
        }

        if (command.Equals("quit", StringComparison.OrdinalIgnoreCase))
        {
            Environment.Exit(0); // Exit immediately when quit is entered
        }

        await writer.WriteLineAsync(command);

        string? serverResponse = await reader.ReadLineAsync();
        Console.WriteLine(serverResponse);

        // Check if we received a response and if it's an error
        if (serverResponse is null || serverResponse.StartsWith("Error"))
        {
            i--; // Don't count invalid commands or errors
            continue;
        }

        // If there was no error, proceed with getting numbers
        Console.Write("Enter two numbers (a b): ");
        string? numbers = Console.ReadLine();
        await writer.WriteLineAsync(numbers);

        string? result = await reader.ReadLineAsync();
        if (result?.StartsWith("Error") == true)
        {
            Console.WriteLine(result);
            i--; // Don't count number input errors
        }
        else
            Console.WriteLine($"Result: {result}");
    }

    Console.WriteLine("\nThree successful operations completed. Press any key to exit.");
    Console.ReadKey();
    Environment.Exit(0);
}