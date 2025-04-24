using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using Dispatcher.Helpers;
using Grpc.Core;
using ProtosInterfaceDispatcher.Protos;

namespace Dispatcher.Services
{
    public class DispatcherService : ProtosInterfaceDispatcher.Protos.Dispatcher.DispatcherBase
    {
        private readonly NodeRegistry _nodeRegistry;
        
        // Коллекция для хранения информации о запущенных узлах.
        // Используем thread-safe структуру для простоты.
        // private static readonly ConcurrentDictionary<string, NodeInfo> _nodes =
        //     new ConcurrentDictionary<string, NodeInfo>();
        
        public DispatcherService(NodeRegistry nodeRegistry)
        {
            _nodeRegistry = nodeRegistry;
        }
        
        public override async Task<ShutdownResponse> Shutdown(ShutdownRequest request, ServerCallContext context)
        {
            var response = new ShutdownResponse
            {
                Success = true,
                Message = "Диспетчер завершает работу и убивает все запущенные узлы."
            };

            // Итерация по коллекции запущенных узлов
            foreach (var node in _nodeRegistry.GetAllNodes())
            {
                // В нашем случае ключ – это PID в виде строки
                if (int.TryParse(node.NodeId, out int pid))
                {
                    try
                    {
                        // Получаем процесс по PID
                        var nodeProcess = Process.GetProcessById(pid);

                        // Если процесс ещё не завершён — пробуем его убить
                        if (!nodeProcess.HasExited)
                        {
                            Console.WriteLine($"Завершаю узел с PID: {pid}");
                            nodeProcess.Kill();
                            // Можно ждать немного для гарантии, что процесс завершился
                            nodeProcess.WaitForExit(2000);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Если процесс уже завершился или произошла ошибка, можно залогировать и продолжить
                        Console.WriteLine($"Не удалось завершить узел с PID: {pid}. Ошибка: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"Не удалось преобразовать идентификатор узла '{node.NodeId}' в число.");
                }
            }

            // Запускаем завершение работы диспетчера с задержкой, чтобы gRPC-ответ успел уйти клиенту.
            _ = Task.Run(async () =>
            {
                await Task.Delay(500);
                Environment.Exit(0);
            });

            return response;
        }
        
        // Пусть диспетчер получает путь к ноде из запроса или уже зафиксированный в конфигурации.
        // Если в запросе node_path не передан, можно использовать значение по умолчанию.
        private static readonly string DefaultNodeExecutablePath = "/path/to/default/node/executable"; // Замените на реальное значение.

        public override async Task<CreateNodeResponse> CreateNode(CreateNodeRequest request, ServerCallContext context)
        {
            int preferredPort = request.PreferredPort;
            string nodePath = string.IsNullOrWhiteSpace(request.NodePath) ? DefaultNodeExecutablePath : request.NodePath;
            
            // Формируем аргументы для запуска узла. Допустим, нода слушает по HTTP,
            // поэтому передаём порт через параметр --urls (можно использовать HTTPS при необходимости).
            string urlsArg = $"--urls=https://localhost:{preferredPort}";
            
            var psi = new ProcessStartInfo
            {
                FileName = nodePath,
                Arguments = urlsArg,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            Process? process;
            try
            {
                process = Process.Start(psi);
                if (process == null)
                {
                    return new CreateNodeResponse
                    {
                        Success = false,
                        Message = "Не удалось запустить процесс узла."
                    };
                }
            }
            catch (Exception ex)
            {
                return new CreateNodeResponse
                {
                    Success = false,
                    Message = $"Ошибка при запуске узла: {ex.Message}"
                };
            }

            // Используем PID как уникальный идентификатор узла.
            string nodeId = process.Id.ToString();

            // Формируем информацию об узле.
            var nodeInfo = new NodeInfo
            {
                NodeId = nodeId,
                Port = preferredPort
            };

            // Сохраняем информацию о узле.
            _nodeRegistry.AddNode(nodeInfo.NodeId, nodeInfo);
            
            return new CreateNodeResponse
            {
                Success = true,
                NodeId = nodeId,
                Port = preferredPort,
                Message = "Узел успешно создан."
            };
        }

        // Остальные методы (DeleteNode, ListNodes, Shutdown и т.д.) остаются по аналогии.
    }
}