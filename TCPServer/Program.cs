using System.Net;
using System.Net.Sockets;

const int Port = 5000;

var listener = new TcpListener(IPAddress.Any, Port); // Socket, listen for any IP address on this machine, on this port
listener.Start();
Console.WriteLine($"Server is running on port {Port}... (:");

while (true)
{
    TcpClient client = await listener.AcceptTcpClientAsync(); // Client it waits for
    Console.WriteLine("Client connected!");

}