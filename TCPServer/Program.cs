using System.Net;
using System.Net.Sockets;

Console.WriteLine(@"
 _______ _____ _____     _____                         
|__   __/ ____|  __ \   / ____|                         
   | | | |    | |__) | | (___   ___ _ ____   _____ _ __ 
   | | | |    |  ___/   \___ \ / _ \ '__\ \ / / _ \ '__|
   | | | |____| |       ____) |  __/ |   \ V /  __/ |   
   |_|  \_____|_|      |_____/ \___|_|    \_/ \___|_|   
");

const int ServerPort = 5000;
const string InvalidCommand = "Error, command unknown (has to be 'Random', 'Add', or 'Subtract')";

var server = StartServer();
await AcceptClientsForever(server); // Listen for clients

TcpListener StartServer()
{
    var listener = new TcpListener(IPAddress.Any, ServerPort); // Listen for any IP address on this machine on this port
    listener.Start();
    Console.WriteLine($"Server is running on port {ServerPort}... (:");
    return listener;
}

async Task AcceptClientsForever(TcpListener listener)
{
    while (true)
    {
        TcpClient client = await listener.AcceptTcpClientAsync();
        Console.WriteLine("Yay! A friend connected!");
        _ = HandleClientAsync(client); // Discard return value and remove await to handle multiple clients (concurrent)
    }
}

async Task HandleClientAsync(TcpClient client)
{
    using var stream = client.GetStream(); // Get data stream
    using var reader = new StreamReader(stream); // For reading data
    using var writer = new StreamWriter(stream) { AutoFlush = true }; // For writing data

    try
    {
        await HandleClientCommands(reader, writer);
    }
    catch (Exception)
    {
        // Something went wrong, but it's okay
    }

    Console.WriteLine("Bye bye friend!");
    client.Close();
}

async Task HandleClientCommands(StreamReader reader, StreamWriter writer)
{
    while (true)
    {
        // Listen to what the client wants to do
        string? command = await ReadCommand(reader);
        if (command == null) break;

        // Validate command
        if (!IsValidCommand(command))
        {
            await writer.WriteLineAsync(InvalidCommand);
            continue;
        }

        // Get numbers
        var numbers = await GetNumbersFromUser(reader, writer);
        if (!numbers.HasValue)
        {
            continue;
        }

        // Process command and send result
        await ProcessCommand(command, numbers.Value.num1, numbers.Value.num2, writer);
    }
}

async Task<string?> ReadCommand(StreamReader reader)
{
    try
    {
        string? command = await reader.ReadLineAsync();
        if (string.IsNullOrEmpty(command)) return null;
        if (command.Equals("quit", StringComparison.OrdinalIgnoreCase)) return null;
        return command.ToLower();
    }
    catch (Exception)
    {
        return null;
    }
}

// Has to be one of the 3 valid commands
bool IsValidCommand(string command) =>
    command == "add" || command == "subtract" || command == "random";

async Task<(int num1, int num2)?> GetNumbersFromUser(StreamReader reader, StreamWriter writer)
{
    try
    {
        await writer.WriteLineAsync("Input numbers");
        string? input = await reader.ReadLineAsync();

        if (string.IsNullOrEmpty(input))
            return null;

        string[] numbers = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (numbers.Length != 2 ||
            !int.TryParse(numbers[0], out int num1) ||
            !int.TryParse(numbers[1], out int num2))
        {
            await writer.WriteLineAsync("Error! Please give me two numbers with a space between them, like '5 12'");
            return null;
        }

        return (num1, num2);
    }
    catch (Exception)
    {
        return null;
    }
}

async Task<string> ProcessCommand(string command, int num1, int num2, StreamWriter writer)
{
    string result = command switch
    {
        "add" => (num1 + num2).ToString(),
        "subtract" => (num1 - num2).ToString(),
        "random" => HandleRandomCommand(num1, num2),
        _ => InvalidCommand
    };

    await writer.WriteLineAsync(result);
    return result;
}

string HandleRandomCommand(int min, int max)
{
    if (min == max)
        return min.ToString();

    if (min > max)
        return "Error! The first number has to be smaller than or equal to the second number";

    return Random.Shared.Next(min, max + 1).ToString(); // Thread safe
}