using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Abstractions;
using Core.Keys;
using Core.Enums;
using Grpc.Net.Client;
using ProtosInterfaceDispatcher.Protos;

namespace Core.Commands
{
    /// <summary>
    /// Вывод списка запущенных узлов диспетчера
    /// </summary>
    public class ListCommand : CommandBase
    {
        public override string? Value { get; set; }

        public override required IEnumerable<KeyValuePair<KeyBase, string?>> KeyAndValues { get; set; }

        public override string Name { get; } = "list";

        public override string Description { get; } = "Список запущенных узлов диспетчера";

        public override KeyBase[] AllowedKeys { get; } =
        {
            new DispatcherPortKey(),
        };

        public override KeyBase[] RequiredKeys { get; } =
        {
            new DispatcherPortKey(),
        };

        public override bool ValueIsRequired { get; } = false;

        public override string ExpectedValue { get; } = string.Empty;

        public override string[] Examples { get; } =
        {
            "list -p 8080"
        };

        public override Type CommandType { get; } = typeof(ListCommand);

        public override async Task ExecuteAsync()
        {
            // 1) Проверяем, что задан port
            var portEntry = KeyAndValues.FirstOrDefault(x => x.Key.KeyName == CommandKey.Port);
            if (portEntry.Value == null || !int.TryParse(portEntry.Value, out var dispatcherPort))
            {
                throw new Exception("Неверно указан порт диспетчера. Используйте ключ --port или -p.");
            }

            // 2) Создаём gRPC-канал
            var address = $"https://localhost:{dispatcherPort}";
            // если вы используете plaintext HTTP/2, раскомментируйте:
            // AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            using var channel = GrpcChannel.ForAddress(address);
            var client  = new Dispatcher.DispatcherClient(channel);

            // 3) Вызываем ListNodes
            ListNodesResponse reply;
            try
            {
                reply = await client.ListNodesAsync(new ListNodesRequest());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при вызове ListNodes: {ex.Message}");
                return;
            }

            // 4) Вывод результата
            if (reply.Nodes.Count == 0)
            {
                Console.WriteLine("Узлы не найдены.");
            }
            else
            {
                Console.WriteLine("Список узлов:");
                foreach (var node in reply.Nodes)
                {
                    Console.WriteLine($" • ID = {node.NodeId}, Port = {node.Port}");
                }
            }
        }
    }
}