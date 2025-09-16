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

const int Port = 5000;
const string InvalidCommand = "Error, command unknown (has to be 'Random', 'Add', or 'Subtract')";

var listener = InitializeServer(Port);

while (true)
{
    TcpClient client = await listener.AcceptTcpClientAsync(); // Client it waits for
    Console.WriteLine("Client connected!");
    _ = HandleClientAsync(client); // Discard the return value and make it not await so that it can handle more clients (concurrent)
}

static TcpListener InitializeServer(int port)
{
    var listener = new TcpListener(IPAddress.Any, port); // Socket, listen for any IP address on this machine, on this port
    listener.Start();
    Console.WriteLine($"Server is running on port {port}... (:");
    return listener;
}

static async Task HandleClientAsync(TcpClient client)
{
    using var stream = client.GetStream(); // Data stream
    using var reader = new StreamReader(stream); // Used for reading data
    using var writer = new StreamWriter(stream) { AutoFlush = true }; // Used for writing data

    await ProcessClientCommandsAsync(reader, writer);

    Console.WriteLine("Client disconnected.");
    client.Close();
}

static bool IsValidCommand(string command) =>
    command == "add" || command == "subtract" || command == "random";

static async Task ProcessClientCommandsAsync(StreamReader reader, StreamWriter writer)
{
    while (true)
    {
        string? command = await reader.ReadLineAsync();
        if (command is null || command.Equals("quit", StringComparison.OrdinalIgnoreCase))
            break;

        command = command.ToLower();
        if (!IsValidCommand(command))
        {
            await writer.WriteLineAsync(InvalidCommand);
            continue;
        }

        await writer.WriteLineAsync("Input numbers");

        string? numbersLine = await reader.ReadLineAsync();
        if (numbersLine is null)
            break;

        string[] parts = numbersLine.Split(' ', StringSplitOptions.RemoveEmptyEntries); // Remove empty elements and only keep the numbers
        if (parts.Length != 2 || !int.TryParse(parts[0], out int a) || !int.TryParse(parts[1], out int b))
        { // Make sure its valid values
            await writer.WriteLineAsync("Error: Please enter two integers, with a space in-between, e.g., '5 12'");
            continue;
        }
        else
        {
            await ProcessCommandAsync(writer, command, a, b);
        }
    }
}

static async Task ProcessCommandAsync(StreamWriter writer, string command, int a, int b)
{
    string response;
    switch (command)
    {
        case "add":
            response = (a + b).ToString();
            break;
        case "subtract":
            response = (a - b).ToString();
            break;
        case "random":
            if (a >= b)
            {
                response = "Error: For random, the first number has to be less than the second";
                break;
            }
            else
            {
                response = Random.Shared.Next(a, b + 1).ToString(); // Thread safe (:
                break;
            }
        default:
            response = InvalidCommand;
            break;
    }
    await writer.WriteLineAsync(response);
}