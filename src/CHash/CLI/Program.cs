using Grpc.Net.Client;
using ProtosInterfaceDispatcher.Protos;
using ProtosInterfaceDispatcher.Shared;

namespace CLI
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // TODO: Добавить DI
            
            var command = new CommandLineParser().Parse(args);

            if (command is null)
            {
                Console.WriteLine("Command was not recognized.");
                return;
            }

            await command.ExecuteAsync();
            
            // var channel = GrpcChannel.ForAddress("https://localhost:7194");
            // var client = new GreeterTest.GreeterTestClient(channel);
            // var client2 = new Dispatcher.DispatcherClient(channel);

            // var request = new DeleteNodeRequest();
            // {
            //     
            // }
            //
            // var reply2 = await client2.CreateNode()
            //
            // var reply = await client.SayHelloTestAsync(new HelloRequestTest { Name = "Тестовый клиент" });
            // Console.WriteLine($"Ответ от сервера: {reply.Message}");
        }
    }
}