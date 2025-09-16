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
    while (true)
    {
        Console.Write("Enter command: ");
        string? command = Console.ReadLine();

        if (string.IsNullOrEmpty(command))
            continue;

        if (command.Equals("quit", StringComparison.OrdinalIgnoreCase))
            break;

        await writer.WriteLineAsync(command);

        string? serverResponse = await reader.ReadLineAsync();
        Console.WriteLine(serverResponse);

        // Check if we received a response and if it's an error
        bool isErrorResponse = false;
        if (serverResponse is null || serverResponse.StartsWith("Error"))
            isErrorResponse = true;

        // If there was no error, proceed with getting numbers
        if (!isErrorResponse)
        {
            Console.Write("Enter two numbers (a b): ");
            string? numbers = Console.ReadLine();
            await writer.WriteLineAsync(numbers);

            string? result = await reader.ReadLineAsync();
            if (result?.StartsWith("Error") == true)
                Console.WriteLine(result);
            else
                Console.WriteLine($"Result: {result}");
        }
    }
}