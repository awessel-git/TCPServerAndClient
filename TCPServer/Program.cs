using System.Net;
using System.Net.Sockets;

const int Port = 5000;

var listener = InitializeServer(Port);

while (true)
{
    TcpClient client = await listener.AcceptTcpClientAsync(); // Client it waits for
    Console.WriteLine("Client connected!");
    await HandleClientAsync(client);
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

static async Task ProcessClientCommandsAsync(StreamReader reader, StreamWriter writer)
{
    while (true)
    {
        string? command = await reader.ReadLineAsync();
        if (command is null || command.Equals("quit", StringComparison.OrdinalIgnoreCase))
            break;

        await writer.WriteLineAsync("Input numbers");

        string? numbersLine = await reader.ReadLineAsync();
        if (numbersLine is null)
            break;

        string[] parts = numbersLine.Split(' ', StringSplitOptions.RemoveEmptyEntries); // Remove empty elements and only keep the numbers
        if (parts.Length != 2 || !int.TryParse(parts[0], out int a) || !int.TryParse(parts[1], out int b))
        { // Make sure its valid values
            await writer.WriteLineAsync("Error: Yo, please enter two integers, with a space in-between (:");
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
    switch (command.ToLower())
    {
        case "add":
            response = (a + b).ToString();
            break;
        case "subtract":
            response = (a - b).ToString();
            break;
        case "random":
            if (a <= b)
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
            response = "Error, command unknown (has to be 'Random', 'Add', or 'Subtract'";
            break;
    }
    await writer.WriteLineAsync(response);
}