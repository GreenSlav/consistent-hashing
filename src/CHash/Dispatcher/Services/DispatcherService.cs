using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Dispatcher.Helpers;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using ProtosInterfaceDispatcher.Protos;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Dispatcher.Services
{
    public class DispatcherService : ProtosInterfaceDispatcher.Protos.Dispatcher.DispatcherBase
    {
        private readonly NodeRegistry _nodeRegistry;
        private const string DefaultNodeExecutablePath = "/path/to/default/node/executable";
        private static readonly ILogger _logger = Log.ForContext<DispatcherService>();

        public DispatcherService(NodeRegistry nodeRegistry)
        {
            _nodeRegistry = nodeRegistry;
        }

        public override Task<ListNodesResponse> ListNodes(ListNodesRequest request, ServerCallContext context)
        {
            var response = new ListNodesResponse();
            foreach (var node in _nodeRegistry.GetAllNodes())
            {
                response.Nodes.Add(new NodeInfo { NodeId = node.NodeId, Port = node.Port });
            }
            _logger.Information("Получен список всех нод. Кол-во: {Count}", response.Nodes.Count);
            return Task.FromResult(response);
        }

        public override async Task<CreateNodeResponse> CreateNode(CreateNodeRequest request, ServerCallContext context)
        {
            int preferredPort = request.PreferredPort;
            string nodePath = string.IsNullOrWhiteSpace(request.NodePath) ? DefaultNodeExecutablePath : request.NodePath;

            var psi = new ProcessStartInfo
            {
                FileName = nodePath,
                Arguments = $"--urls=https://localhost:{preferredPort}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process process;
            try
            {
                process = Process.Start(psi) ?? throw new Exception("Process.Start вернул null");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка при запуске узла на порту {Port}", preferredPort);
                return await Task.FromResult(new CreateNodeResponse
                {
                    Success = false,
                    Message = $"Ошибка при запуске узла: {ex.Message}"
                });
            }

            var nodeId = process.Id.ToString();
            _nodeRegistry.AddNode(nodeId, new NodeInfo { NodeId = nodeId, Port = preferredPort });
            _logger.Information("Нода {NodeId} успешно добавлена на порт {Port}", nodeId, preferredPort);

            int migrated = 0;
            foreach (var oldNode in _nodeRegistry.GetAllNodes())
            {
                if (oldNode.NodeId == nodeId) continue;

                var oldAddress = $"https://localhost:{oldNode.Port}";
                _logger.Debug("Начинаем миграцию данных с ноды {OldNodeId} ({Address})", oldNode.NodeId, oldAddress);
                using var oldChannel = GrpcChannel.ForAddress(oldAddress);

                var orderClient = new ProtosInterfaceDispatcher.Protos.OrderService.OrderServiceClient(oldChannel);
                var customerClient = new ProtosInterfaceDispatcher.Protos.CustomerService.CustomerServiceClient(oldChannel);
                var productClient = new ProtosInterfaceDispatcher.Protos.ProductService.ProductServiceClient(oldChannel);

                var ordersResp = await orderClient.ListOrdersAsync(new Empty());
                var customersResp = await customerClient.ListCustomersAsync(new Empty());
                var productsResp = await productClient.ListProductsAsync(new Empty());

                foreach (var order in ordersResp.Orders)
                {
                    var newTarget = _nodeRegistry.GetNodeByKey(order.Id);
                    if (newTarget.NodeId != oldNode.NodeId)
                    {
                        var newChannel = GrpcChannel.ForAddress($"https://localhost:{newTarget.Port}");
                        var newClient = new ProtosInterfaceDispatcher.Protos.OrderService.OrderServiceClient(newChannel);

                        await newClient.CreateOrderAsync(new CreateOrderRequest
                        {
                            CustomerId = order.CustomerId,
                            OrderDate = order.OrderDate,
                            TotalAmount = order.TotalAmount
                        });
                        await orderClient.DeleteOrderAsync(new OrderIdRequest { Id = order.Id });
                        migrated++;
                    }
                }

                foreach (var cust in customersResp.Customers)
                {
                    var newTarget = _nodeRegistry.GetNodeByKey(cust.Id);
                    if (newTarget.NodeId != oldNode.NodeId)
                    {
                        var newChannel = GrpcChannel.ForAddress($"https://localhost:{newTarget.Port}");
                        var newClient = new ProtosInterfaceDispatcher.Protos.CustomerService.CustomerServiceClient(newChannel);

                        await newClient.CreateCustomerAsync(new CreateCustomerRequest
                        {
                            FullName = cust.FullName,
                            Email = cust.Email,
                            PhoneNumber = cust.PhoneNumber
                        });
                        await customerClient.DeleteCustomerAsync(new CustomerIdRequest { Id = cust.Id });
                        migrated++;
                    }
                }

                foreach (var prod in productsResp.Products)
                {
                    var newTarget = _nodeRegistry.GetNodeByKey(prod.Id);
                    if (newTarget.NodeId != oldNode.NodeId)
                    {
                        var newChannel = GrpcChannel.ForAddress($"https://localhost:{newTarget.Port}");
                        var newClient = new ProtosInterfaceDispatcher.Protos.ProductService.ProductServiceClient(newChannel);

                        await newClient.CreateProductAsync(new CreateProductRequest
                        {
                            Name = prod.Name,
                            Price = prod.Price,
                            StockQuantity = prod.StockQuantity
                        });
                        await productClient.DeleteProductAsync(new ProductIdRequest { Id = prod.Id });
                        migrated++;
                    }
                }
            }

            _logger.Information("Нода {NodeId} успешно создана. Мигрировано сущностей: {Count}", nodeId, migrated);

            return await Task.FromResult(new CreateNodeResponse
            {
                Success = true,
                NodeId = nodeId,
                Port = preferredPort,
                Message = "Узел успешно создан."
            });
        }

        public override async Task<DeleteNodeResponse> DeleteNode(DeleteNodeRequest request, ServerCallContext context)
        {
            var nodeId = request.NodeId;
            if (string.IsNullOrWhiteSpace(nodeId) || !_nodeRegistry.TryGetNode(nodeId, out var info))
            {
                _logger.Warning("Попытка удалить несуществующую ноду: {NodeId}", nodeId);
                return new DeleteNodeResponse { Success = false, Message = $"Узел {nodeId} не найден." };
            }

            var address = $"https://localhost:{info!.Port}";
            using var oldCh = GrpcChannel.ForAddress(address);

            var orderClient = new ProtosInterfaceDispatcher.Protos.OrderService.OrderServiceClient(oldCh);
            var customerClient = new ProtosInterfaceDispatcher.Protos.CustomerService.CustomerServiceClient(oldCh);
            var productClient = new ProtosInterfaceDispatcher.Protos.ProductService.ProductServiceClient(oldCh);

            var ordersResp = await orderClient.ListOrdersAsync(new Empty());
            var customersResp = await customerClient.ListCustomersAsync(new Empty());
            var productsResp = await productClient.ListProductsAsync(new Empty());

            _nodeRegistry.RemoveNode(nodeId);
            _logger.Information("Удалена нода {NodeId} из реестра", nodeId);

            int migrated = 0;

            foreach (var order in ordersResp.Orders)
            {
                var target = _nodeRegistry.GetNodeByKey(order.Id);
                var ch = GrpcChannel.ForAddress($"https://localhost:{target.Port}");
                var cli = new ProtosInterfaceDispatcher.Protos.OrderService.OrderServiceClient(ch);

                await cli.CreateOrderAsync(new CreateOrderRequest
                {
                    CustomerId = order.CustomerId,
                    OrderDate = order.OrderDate,
                    TotalAmount = order.TotalAmount
                });
                migrated++;
            }

            foreach (var cust in customersResp.Customers)
            {
                var target = _nodeRegistry.GetNodeByKey(cust.Id);
                var ch = GrpcChannel.ForAddress($"https://localhost:{target.Port}");
                var cli = new ProtosInterfaceDispatcher.Protos.CustomerService.CustomerServiceClient(ch);

                await cli.CreateCustomerAsync(new CreateCustomerRequest
                {
                    FullName = cust.FullName,
                    Email = cust.Email,
                    PhoneNumber = cust.PhoneNumber
                });
                migrated++;
            }

            foreach (var prod in productsResp.Products)
            {
                var target = _nodeRegistry.GetNodeByKey(prod.Id);
                var ch = GrpcChannel.ForAddress($"https://localhost:{target.Port}");
                var cli = new ProtosInterfaceDispatcher.Protos.ProductService.ProductServiceClient(ch);

                await cli.CreateProductAsync(new CreateProductRequest
                {
                    Name = prod.Name,
                    Price = prod.Price,
                    StockQuantity = prod.StockQuantity
                });
                migrated++;
            }

            if (int.TryParse(nodeId, out var pid))
            {
                try
                {
                    var proc = Process.GetProcessById(pid);
                    if (!proc.HasExited) proc.Kill();
                    _logger.Information("Процесс ноды {NodeId} (PID {Pid}) успешно завершён", nodeId, pid);
                }
                catch (Exception ex)
                {
                    _logger.Warning(ex, "Не удалось убить процесс {NodeId}", nodeId);
                }
            }

            _logger.Information("Нода {NodeId} удалена. Мигрировано сущностей: {Count}", nodeId, migrated);
            return new DeleteNodeResponse
            {
                Success = true,
                Message = $"Нода {nodeId} удалена, мигрировано {migrated} записей."
            };
        }

        public override Task<ShutdownResponse> Shutdown(ShutdownRequest request, ServerCallContext context)
        {
            _logger.Warning("Инициировано завершение работы диспетчера. Попытка убить все процессы нод...");

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
                            _logger.Information("Процесс {NodeId} успешно завершён", node.NodeId);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Warning: не удалось убить узел {node.NodeId}: {ex.Message}");
                        _logger.Warning(ex, "Ошибка при завершении ноды {NodeId}", node.NodeId);
                    }
                }
            }

            _ = Task.Run(async () =>
            {
                await Task.Delay(500);
                _logger.Warning("Завершение процесса диспетчера...");
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
