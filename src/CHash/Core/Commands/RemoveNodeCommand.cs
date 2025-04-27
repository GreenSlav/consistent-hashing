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
            new HashingNodePortKey(), // порт, на котором слушает узел
        };

        public override KeyBase[] RequiredKeys { get; } =
        {
            new DispatcherPortKey(),
            new HashingNodePortKey(), // порт, на котором слушает узел
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

            // 1) обязательный порт диспетчера
            var dispPortEntry = KeyAndValues
                .FirstOrDefault(x => x.Key.KeyName == CommandKey.Port);
            if (dispPortEntry.Value == null
                || !int.TryParse(dispPortEntry.Value, out var dispPort))
            {
                throw new Exception("Обязательный ключ --port указан неверно или отсутствует.");
            }

            // 2) обязательный порт ноды
            var nodePortEntry = KeyAndValues
                .FirstOrDefault(x => x.Key.KeyName == CommandKey.NodePort);
            if (nodePortEntry.Value == null
                || !int.TryParse(nodePortEntry.Value, out var nodePort))
            {
                throw new Exception("Обязательный ключ --node-port указан неверно или отсутствует.");
            }

            // 3) создаём канал к диспетчеру
            var address = $"https://localhost:{dispPort}";
            using var channel = GrpcChannel.ForAddress(address);
            var client = new Dispatcher.DispatcherClient(channel);

            // 4) ищем nodeId по порту
            ListNodesResponse listReply;
            try
            {
                listReply = await client.ListNodesAsync(new ListNodesRequest());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при вызове ListNodes: {ex.Message}");
                return;
            }

            var found = listReply.Nodes.FirstOrDefault(n => n.Port == nodePort);
            if (found == null)
            {
                Console.WriteLine($"Узел на порту {nodePort} не найден в диспетчере.");
                return;
            }

            var nodeId = found.NodeId;

            // 5) удаляем ноду
            DeleteNodeResponse delReply;
            try
            {
                delReply = await client.DeleteNodeAsync(new DeleteNodeRequest { NodeId = nodeId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при вызове DeleteNode: {ex.Message}");
                throw;
            }

            if (delReply.Success)
                Console.WriteLine($"Узел {nodeId} (порт {nodePort}) успешно удалён: {delReply.Message}");
            else
                Console.WriteLine($"Не удалось удалить узел {nodeId}: {delReply.Message}");
        }
    }
}