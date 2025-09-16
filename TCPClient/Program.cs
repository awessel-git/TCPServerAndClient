using System.Net.Sockets;

const string ServerAddress = "127.0.0.1"; // Localhost
const int Port = 5000; // Same as the server

using var client = new TcpClient();
await client.ConnectAsync(ServerAddress, Port);