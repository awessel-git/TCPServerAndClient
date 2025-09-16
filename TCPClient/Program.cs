using System.Net.Sockets;

const string ServerAddress = "127.0.0.1"; // Localhost
const int Port = 5000; // Same as the server

using var client = new TcpClient();
await client.ConnectAsync(ServerAddress, Port);

using var stream = client.GetStream();
using var reader = new StreamReader(stream);
using var writer = new StreamWriter(stream) { AutoFlush = true };

Console.WriteLine("Connected to server!");
Console.WriteLine("Type 'Random', 'Add', or 'Subtract'. 'Quit' to exit. \n");

while (true)
{
    Console.Write("Enter command: ");
    string? command = Console.ReadLine();

    if (string.IsNullOrEmpty(command))
        continue;
    else
        await writer.WriteLineAsync(command);

    string? prompt = await reader.ReadLineAsync(); // Waits for the "Input numbers" response
    Console.WriteLine(prompt);

    Console.Write("Enter two numbers (a b): ");
    string? numbers = Console.ReadLine();
    await writer.WriteLineAsync(numbers);

    string? result = await reader.ReadLineAsync();
    Console.WriteLine($"The result is {result}");
}