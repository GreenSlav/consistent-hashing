using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Abstractions;
using Core.Enums;
using Core.Keys;
using Grpc.Net.Client;
using ProtosInterfaceDispatcher.Protos;

namespace Core.Commands
{
    /// <summary>
    /// Команда удаления узла
    /// </summary>
    public class RemoveNodeCommand : CommandBase
    {
        public override string? Value { get; set; }

        public override required IEnumerable<KeyValuePair<KeyBase, string?>> KeyAndValues { get; set; }

        public override string Name { get; } = "remove";

        public override string Description { get; } = "Удалить узел хэширования";

        public override KeyBase[] AllowedKeys { get; } =
        {
            new DispatcherPortKey(),
            new HashingNodePortKey(),   // порт, на котором слушает узел
        };

        public override KeyBase[] RequiredKeys { get; } =
        {
            new DispatcherPortKey(),
        };

        public override bool ValueIsRequired { get; } = false;

        public override string ExpectedValue { get; } = "Идентификатор узла (node_id)";

        public override string[] Examples { get; } =
        {
            "remove 12345 -p 8080",
            "remove --node-port 5001 --port 8080"
        };

        public override Type CommandType { get; } = typeof(RemoveNodeCommand);

        public override async Task ExecuteAsync()
        {
            if (KeyAndValues == null)
                throw new Exception("Не переданы параметры для команды.");

            // 1) порт диспетчера
            var dispPortEntry = KeyAndValues.FirstOrDefault(x => x.Key.KeyName == CommandKey.Port);
            if (dispPortEntry.Value == null || !int.TryParse(dispPortEntry.Value, out var dispPort))
                throw new Exception("Неверно указан порт диспетчера.");

            // собираем адрес диспетчера
            var address = $"https://localhost:{dispPort}";
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            using var channel = GrpcChannel.ForAddress(address);
            var client  = new Dispatcher.DispatcherClient(channel);

            // 2) пытаемся получить node_id из позиционного значения
            string? nodeId = Value;

            // 3) если не передали node_id, пробуем поиск по --node-port
            if (string.IsNullOrEmpty(nodeId))
            {
                var nodePortEntry = KeyAndValues.FirstOrDefault(x => x.Key.KeyName == CommandKey.NodePort);
                if (nodePortEntry.Value != null && int.TryParse(nodePortEntry.Value, out var nodePort))
                {
                    // вызываем ListNodes
                    var listReply = await client.ListNodesAsync(new ListNodesRequest());
                    var found = listReply.Nodes.FirstOrDefault(n => n.Port == nodePort);
                    if (found != null)
                    {
                        nodeId = found.NodeId;
                    }
                    else
                    {
                        Console.WriteLine($"Узел на порту {nodePort} не найден в диспетчере.");
                        return;
                    }
                }
            }

            if (string.IsNullOrEmpty(nodeId))
            {
                throw new Exception("Нужно указать либо node_id в качестве значения команды, либо --node-port.");
            }

            // 4) вызывем gRPC DeleteNode
            try
            {
                var delReply = await client.DeleteNodeAsync(new DeleteNodeRequest { NodeId = nodeId });
                if (delReply.Success)
                {
                    Console.WriteLine($"Узел {nodeId} успешно удалён: {delReply.Message}");
                }
                else
                {
                    Console.WriteLine($"Не удалось удалить узел {nodeId}: {delReply.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при вызове DeleteNode: {ex.Message}");
                throw;
            }
        }
    }
}