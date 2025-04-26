using System.Diagnostics;
using Dispatcher.Helpers;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using ProtosInterfaceDispatcher.Protos;

namespace Dispatcher.Services
{
    /// <summary>
    /// Реализация сервиса диспетчера для управления узлами системы
    /// </summary>
    public class DispatcherServiceImpl : ProtosInterfaceDispatcher.Protos.Dispatcher.DispatcherBase
    {
        private readonly NodeRegistry _nodeRegistry;
        private readonly ILogger<DispatcherServiceImpl> _logger;
        private readonly string _defaultNodePath;

        public DispatcherServiceImpl(
            NodeRegistry nodeRegistry,
            ILogger<DispatcherServiceImpl> logger,
            IConfiguration config)
        {
            _nodeRegistry = nodeRegistry;
            _logger = logger;
            _defaultNodePath = config["NodeSettings:DefaultNodePath"];
        }

        public override async Task<CreateNodeResponse> CreateNode(CreateNodeRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Creating new node on port {Port}", request.PreferredPort);

            var nodePath = string.IsNullOrWhiteSpace(request.NodePath) 
                ? _defaultNodePath 
                : request.NodePath;

            if (!File.Exists(nodePath))
            {
                _logger.LogError("Node executable not found at {Path}", nodePath);
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
                    throw new RpcException(new Status(StatusCode.Internal, "Failed to start node process"));
                }

                var nodeInfo = new NodeInfo
                {
                    NodeId = process.Id.ToString(),
                    Port = request.PreferredPort
                };

                _nodeRegistry.AddNode(nodeInfo.NodeId, nodeInfo);

                _logger.LogInformation("Node {NodeId} created successfully on port {Port}", 
                    nodeInfo.NodeId, nodeInfo.Port);

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
                _logger.LogError(ex, "Error creating node");
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }

        public override async Task<DeleteNodeResponse> DeleteNode(DeleteNodeRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Deleting node {NodeId}", request.NodeId);

            try
            {
                if (!_nodeRegistry.TryGetNode(request.NodeId, out var nodeInfo))
                {
                    throw new RpcException(new Status(StatusCode.NotFound, "Node not found"));
                }

                if (int.TryParse(request.NodeId, out var pid))
                {
                    try
                    {
                        var process = Process.GetProcessById(pid);
                        process.Kill();
                        process.WaitForExit(1000);
                    }
                    catch (ArgumentException)
                    {
                        _logger.LogWarning("Process {NodeId} already terminated", request.NodeId);
                    }
                }

                // Используем новый метод TryRemove вместо RemoveNode
                if (!_nodeRegistry.RemoveNode(request.NodeId))
                {
                    _logger.LogWarning("Node {NodeId} was not found in registry during removal", request.NodeId);
                }

                return new DeleteNodeResponse
                {
                    Success = true,
                    Message = "Node deleted successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting node");
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }

        public override Task<ListNodesResponse> ListNodes(ListNodesRequest request, ServerCallContext context)
        {
            var nodes = _nodeRegistry.GetAllNodes();
            var response = new ListNodesResponse();
            response.Nodes.AddRange(nodes);
            return Task.FromResult(response);
        }

        public override async Task<ShutdownResponse> Shutdown(ShutdownRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Shutdown initiated by {DispatcherId}", request.DispatcherId);

            try
            {
                // Убиваем все ноды
                foreach (var node in _nodeRegistry.GetAllNodes())
                {
                    if (int.TryParse(node.NodeId, out var pid))
                    {
                        try
                        {
                            Process.GetProcessById(pid)?.Kill();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error killing process {Pid}", pid);
                        }
                    }
                }

                // Даем время на отправку ответа
                _ = Task.Run(async () =>
                {
                    await Task.Delay(500);
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
                _logger.LogError(ex, "Error during shutdown");
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }
    }
}