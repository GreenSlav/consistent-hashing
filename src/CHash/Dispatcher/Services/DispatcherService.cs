using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Dispatcher.Helpers;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using ProtosInterfaceDispatcher.Protos;

namespace Dispatcher.Services
{
    public class DispatcherService : ProtosInterfaceDispatcher.Protos.Dispatcher.DispatcherBase
    {
        private readonly NodeRegistry _nodeRegistry;
        private const string DefaultNodeExecutablePath = "/path/to/default/node/executable";

        public DispatcherService(NodeRegistry nodeRegistry)
        {
            _nodeRegistry = nodeRegistry;
        }

        // 1) Отдать список всех нод
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

        // 2) Создать новую ноду
        public override Task<CreateNodeResponse> CreateNode(CreateNodeRequest request, ServerCallContext context)
        {
            int preferredPort = request.PreferredPort;
            string nodePath  = string.IsNullOrWhiteSpace(request.NodePath)
                ? DefaultNodeExecutablePath
                : request.NodePath;

            var psi = new ProcessStartInfo
            {
                FileName               = nodePath,
                Arguments              = $"--urls=https://localhost:{preferredPort}",
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                UseShellExecute        = false,
                CreateNoWindow         = true
            };

            Process process;
            try
            {
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

        // 3) Удалить ноду с миграцией её данных на другие ноды
        public override async Task<DeleteNodeResponse> DeleteNode(DeleteNodeRequest request, ServerCallContext context)
        {
            var nodeId = request.NodeId;
            if (string.IsNullOrWhiteSpace(nodeId) || !_nodeRegistry.TryGetNode(nodeId, out var info))
            {
                return new DeleteNodeResponse
                {
                    Success = false,
                    Message = $"Узел {nodeId} не найден."
                };
            }

            // 1) Получаем все данные с этой ноды
            var address = $"https://localhost:{info!.Port}";
            using var oldCh = GrpcChannel.ForAddress(address);
            
            var orderClient    = new ProtosInterfaceDispatcher.Protos.OrderService.OrderServiceClient(oldCh);
            var customerClient = new ProtosInterfaceDispatcher.Protos.CustomerService.CustomerServiceClient(oldCh);
            var productClient  = new ProtosInterfaceDispatcher.Protos.ProductService.ProductServiceClient(oldCh);

            var ordersResp = await orderClient.ListOrdersAsync(new Empty());
            var customersResp = await customerClient.ListCustomersAsync(new Empty());
            var productsResp = await productClient.ListProductsAsync(new Empty());

            // 2) Убираем ноду из кольца/реестра
            _nodeRegistry.RemoveNode(nodeId);

            int migrated = 0;

            // 3a) Мигрируем заказы
            foreach (var order in ordersResp.Orders)
            {
                // определяем целевую ноду
                var target = _nodeRegistry.GetNodeByKey(order.Id);
                var ch = GrpcChannel.ForAddress($"https://localhost:{target.Port}");
                var cli = new ProtosInterfaceDispatcher.Protos.OrderService.OrderServiceClient(ch);

                await cli.CreateOrderAsync(new CreateOrderRequest {
                    CustomerId  = order.CustomerId,
                    OrderDate   = order.OrderDate,
                    TotalAmount = order.TotalAmount
                });
                migrated++;
            }

            // 3b) Клиенты
            foreach (var cust in customersResp.Customers)
            {
                var target = _nodeRegistry.GetNodeByKey(cust.Id);
                var ch = GrpcChannel.ForAddress($"https://localhost:{target.Port}");
                var cli = new ProtosInterfaceDispatcher.Protos.CustomerService.CustomerServiceClient(ch);

                await cli.CreateCustomerAsync(new CreateCustomerRequest {
                    FullName    = cust.FullName,
                    Email       = cust.Email,
                    PhoneNumber = cust.PhoneNumber
                });
                migrated++;
            }

            // 3c) Товары
            foreach (var prod in productsResp.Products)
            {
                var target = _nodeRegistry.GetNodeByKey(prod.Id);
                var ch     = GrpcChannel.ForAddress($"https://localhost:{target.Port}");
                var cli    = new ProtosInterfaceDispatcher.Protos.ProductService.ProductServiceClient(ch);

                await cli.CreateProductAsync(new CreateProductRequest {
                    Name          = prod.Name,
                    Price         = prod.Price,
                    StockQuantity = prod.StockQuantity
                });
                migrated++;
            }

            // 4) Убиваем процесс старой ноды
            if (int.TryParse(nodeId, out var pid))
            {
                try
                {
                    var proc = Process.GetProcessById(pid);
                    if (!proc.HasExited) proc.Kill();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: не смогли убить процесс {nodeId}: {ex.Message}");
                }
            }

            return new DeleteNodeResponse
            {
                Success = true,
                Message = $"Нода {nodeId} удалена, мигрировано {migrated} записей."
            };
        }
        
        
        // 4) Shutdown: убиваем все ноды и завершаем диспетчер
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

            // даём клиенту ответ, затем выходим
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