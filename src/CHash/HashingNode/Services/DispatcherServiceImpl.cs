using System.Diagnostics;
using Dispatcher.Helpers;
using Grpc.Core;
using ProtosInterfaceDispatcher.Protos;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Dispatcher.Services
{
    /// <summary>
    /// Реализация сервиса диспетчера для управления узлами системы
    /// </summary>
    public class DispatcherServiceImpl : ProtosInterfaceDispatcher.Protos.Dispatcher.DispatcherBase
    {
        private readonly NodeRegistry _nodeRegistry;
        private readonly string _defaultNodePath;
        private static readonly ILogger _logger = Log.ForContext<DispatcherServiceImpl>();

        public DispatcherServiceImpl(NodeRegistry nodeRegistry, IConfiguration config)
        {
            _nodeRegistry = nodeRegistry;
            _defaultNodePath = config["NodeSettings:DefaultNodePath"];
        }

        public override async Task<CreateNodeResponse> CreateNode(CreateNodeRequest request, ServerCallContext context)
        {
            _logger.Information("Создание новой ноды на порту {Port}", request.PreferredPort);

            var nodePath = string.IsNullOrWhiteSpace(request.NodePath)
                ? _defaultNodePath
                : request.NodePath;

            if (!File.Exists(nodePath))
            {
                _logger.Error("Файл исполняемой ноды не найден по пути {Path}", nodePath);
                throw new RpcException(new Status(StatusCode.NotFound, "Node executable not found"));
            }

            try
            {
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = nodePath,
                    Arguments = $"--urls=https://localhost:{request.PreferredPort}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });

                if (process == null)
                {
                    _logger.Error("Не удалось запустить процесс ноды");
                    throw new RpcException(new Status(StatusCode.Internal, "Failed to start node process"));
                }

                var nodeInfo = new NodeInfo
                {
                    NodeId = process.Id.ToString(),
                    Port = request.PreferredPort
                };

                _nodeRegistry.AddNode(nodeInfo.NodeId, nodeInfo);

                _logger.Information("Нода {NodeId} успешно создана на порту {Port}", nodeInfo.NodeId, nodeInfo.Port);

                return new CreateNodeResponse
                {
                    Success = true,
                    NodeId = nodeInfo.NodeId,
                    Port = nodeInfo.Port,
                    Message = "Node created successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка при создании ноды");
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }

        public override async Task<DeleteNodeResponse> DeleteNode(DeleteNodeRequest request, ServerCallContext context)
        {
            _logger.Information("Удаление ноды {NodeId}", request.NodeId);

            try
            {
                if (!_nodeRegistry.TryGetNode(request.NodeId, out var nodeInfo))
                {
                    _logger.Warning("Нода {NodeId} не найдена при попытке удаления", request.NodeId);
                    throw new RpcException(new Status(StatusCode.NotFound, "Node not found"));
                }

                if (int.TryParse(request.NodeId, out var pid))
                {
                    try
                    {
                        var process = Process.GetProcessById(pid);
                        process.Kill();
                        process.WaitForExit(1000);
                        _logger.Information("Процесс {NodeId} завершён", request.NodeId);
                    }
                    catch (ArgumentException)
                    {
                        _logger.Warning("Процесс {NodeId} уже завершён до удаления", request.NodeId);
                    }
                }

                if (!_nodeRegistry.RemoveNode(request.NodeId))
                {
                    _logger.Warning("Нода {NodeId} не была найдена в реестре при удалении", request.NodeId);
                }

                return new DeleteNodeResponse
                {
                    Success = true,
                    Message = "Node deleted successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка при удалении ноды");
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }

        public override Task<ListNodesResponse> ListNodes(ListNodesRequest request, ServerCallContext context)
        {
            var nodes = _nodeRegistry.GetAllNodes();
            _logger.Information("Запрошен список нод. Количество: {Count}", nodes.Count());

            var response = new ListNodesResponse();
            response.Nodes.AddRange(nodes);
            return Task.FromResult(response);
        }

        public override async Task<ShutdownResponse> Shutdown(ShutdownRequest request, ServerCallContext context)
        {
            _logger.Information("Инициирована остановка диспетчером {DispatcherId}", request.DispatcherId);

            try
            {
                foreach (var node in _nodeRegistry.GetAllNodes())
                {
                    if (int.TryParse(node.NodeId, out var pid))
                    {
                        try
                        {
                            Process.GetProcessById(pid)?.Kill();
                            _logger.Information("Процесс ноды {Pid} завершён", pid);
                        }
                        catch (Exception ex)
                        {
                            _logger.Warning(ex, "Ошибка при завершении процесса {Pid}", pid);
                        }
                    }
                }

                _ = Task.Run(async () =>
                {
                    await Task.Delay(500);
                    _logger.Information("Завершение приложения инициировано");
                    Environment.Exit(0);
                });

                return new ShutdownResponse
                {
                    Success = true,
                    Message = "Shutdown sequence initiated"
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка при остановке диспетчера");
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }
    }
}
