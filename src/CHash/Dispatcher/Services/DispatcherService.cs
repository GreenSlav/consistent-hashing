using System.Threading.Tasks;
using Grpc.Core;
using ProtosInterfaceDispatcher.Protos;
using ProtosInterfaceDispatcher.Protos;

namespace Dispatcher.Services
{
    public class DispatcherService : ProtosInterfaceDispatcher.Protos.Dispatcher.DispatcherBase
    {
        // Другие методы CreateNode, DeleteNode и ListNodes…

        public override async Task<ShutdownResponse> Shutdown(ShutdownRequest request, ServerCallContext context)
        {
            // Здесь можно добавить проверки (например, сверку dispatcher_id или токена),
            // но для упрощенного варианта сразу отправляем успех.
            var response = new ShutdownResponse
            {
                Success = true,
                Message = "Диспетчер завершает работу."
            };

            // Запускаем завершение работы с небольшой задержкой,
            // чтобы gRPC-ответ успел уйти клиенту.
            _ = Task.Run(async () =>
            {
                await Task.Delay(500); // 500 мс задержки
                Environment.Exit(0);
            });

            return response;
        }
    }
}