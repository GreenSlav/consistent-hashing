// See https://aka.ms/new-console-template for more information

using Grpc.Net.Client;

Console.WriteLine("Hello, World!");

var channel = GrpcChannel.ForAddress("https://localhost:5001");
