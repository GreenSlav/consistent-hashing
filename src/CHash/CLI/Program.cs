using Grpc.Net.Client;
using ProtosInterfaceDispatcher.Shared;

namespace CLI
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // // TODO: Добавить DI
            //
            // var command = new CommandLineParser().Parse(args);
            //
            // if (command is not null)
            // {
            //     Console.WriteLine(command!.Name);
            //     Console.WriteLine(command!.Description);
            //     foreach (var pair in command!.KeyAndValues!)
            //     {
            //         Console.WriteLine(pair.Key + " = " + pair.Value);
            //     }
            // }
            
            var channel = GrpcChannel.ForAddress("https://localhost:7194");
            var client = new GreeterTest.GreeterTestClient(channel);

            var reply = await client.SayHelloTestAsync(new HelloRequestTest { Name = "Тестовый клиент" });
            Console.WriteLine($"Ответ от сервера: {reply.Message}");
        }
    }
}