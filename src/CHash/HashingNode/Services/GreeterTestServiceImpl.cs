using Grpc.Core;
using ProtosInterfaceDispatcher.Shared;
using Microsoft.Extensions.Logging;

namespace HashingNode.Services;

/// <summary>
/// Тестовый сервис-приветствие для проверки работы gRPC
/// </summary>
public class GreeterTestServiceImpl : GreeterTest.GreeterTestBase
{
    private readonly ILogger<GreeterTestServiceImpl> _logger;

    public GreeterTestServiceImpl(ILogger<GreeterTestServiceImpl> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public override Task<HelloReplyTest> SayHelloTest(HelloRequestTest request, ServerCallContext context)
    {
        _logger.LogInformation("Received greeting request from {Name}", request.Name);
        
        var response = new HelloReplyTest
        {
            Message = $"Привет, {request.Name}! Сообщение от ноды: {Environment.MachineName}"
        };

        _logger.LogDebug("Sending response: {Response}", response.Message);
        
        return Task.FromResult(response);
    }
}