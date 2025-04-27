using System;
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

        // Путь по умолчанию к исполняемому файлу ноды (если клиент его не передал).
        private const string DefaultNodeExecutablePath = "/path/to/default/node/executable";

        public DispatcherService(NodeRegistry nodeRegistry)
        {
            _nodeRegistry = nodeRegistry;
        }

        // 1) Метод ListNodes — отдаёт все зарегистрированные узлы.
        public override Task<ListNodesResponse> ListNodes(ListNodesRequest request, ServerCallContext context)
        {
            var response = new ListNodesResponse();
            foreach (var node in _nodeRegistry.GetAllNodes())
            {
                response.Nodes.Add(new NodeInfo
                {
                    NodeId = node.NodeId,
                    Port   = node.Port
                });
            }
            return Task.FromResult(response);
        }

        // 2) Создание нового узла: запускаем процесс, сохраняем в реестр.
        public override Task<CreateNodeResponse> CreateNode(CreateNodeRequest request, ServerCallContext context)
        {
            int preferredPort = request.PreferredPort;
            string nodePath  = string.IsNullOrWhiteSpace(request.NodePath)
                ? DefaultNodeExecutablePath
                : request.NodePath;

            // Формируем аргумент для порта (HTTP/HTTPS зависит от вашей конфигурации)
            string urlsArg = $"--urls=https://localhost:{preferredPort}";

            Process process;
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName               = nodePath,
                    Arguments              = urlsArg,
                    RedirectStandardOutput = true,
                    RedirectStandardError  = true,
                    UseShellExecute        = false,
                    CreateNoWindow         = true
                };
                process = Process.Start(psi)
                          ?? throw new Exception("Process.Start вернул null");
            }
            catch (Exception ex)
            {
                return Task.FromResult(new CreateNodeResponse
                {
                    Success = false,
                    Message = $"Ошибка при запуске узла: {ex.Message}"
                });
            }

            // PID в качестве ID
            var nodeId = process.Id.ToString();
            _nodeRegistry.AddNode(nodeId, new NodeInfo
            {
                NodeId = nodeId,
                Port   = preferredPort
            });

            return Task.FromResult(new CreateNodeResponse
            {
                Success = true,
                NodeId  = nodeId,
                Port    = preferredPort,
                Message = "Узел успешно создан."
            });
        }

        // 3) Удаление узла: убиваем процесс и удаляем из реестра.
        public override Task<DeleteNodeResponse> DeleteNode(DeleteNodeRequest request, ServerCallContext context)
        {
            var nodeId = request.NodeId;
            if (string.IsNullOrWhiteSpace(nodeId) ||
                !_nodeRegistry.TryGetNode(nodeId, out var info))
            {
                return Task.FromResult(new DeleteNodeResponse
                {
                    Success = false,
                    Message = $"Узел с ID '{nodeId}' не найден."
                });
            }

            // Пробуем убить процесс по PID
            if (int.TryParse(nodeId, out var pid))
            {
                try
                {
                    var proc = Process.GetProcessById(pid);
                    if (!proc.HasExited)
                    {
                        proc.Kill();
                        proc.WaitForExit(2000);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: не удалось убить процесс узла {nodeId}: {ex.Message}");
                }
            }

            // Убираем из реестра
            _nodeRegistry.RemoveNode(nodeId);

            return Task.FromResult(new DeleteNodeResponse
            {
                Success = true,
                Message = "Узел успешно удалён."
            });
        }

        // 4) Shutdown: сначала удаляем все узлы, затем завершаем диспетчер.
        public override Task<ShutdownResponse> Shutdown(ShutdownRequest request, ServerCallContext context)
        {
            foreach (var node in _nodeRegistry.GetAllNodes())
            {
                if (int.TryParse(node.NodeId, out var pid))
                {
                    try
                    {
                        var proc = Process.GetProcessById(pid);
                        if (!proc.HasExited)
                        {
                            proc.Kill();
                            proc.WaitForExit(2000);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Warning: не удалось убить узел {node.NodeId}: {ex.Message}");
                    }
                }
            }

            // Даем клиенту время получить ответ, затем завершаем
            _ = Task.Run(async () =>
            {
                await Task.Delay(500);
                Environment.Exit(0);
            });

            return Task.FromResult(new ShutdownResponse
            {
                Success = true,
                Message = "Диспетчер завершает работу и убивает все узлы."
            });
        }
    }
}