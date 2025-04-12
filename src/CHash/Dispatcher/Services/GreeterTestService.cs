using Grpc.Core;
using ProtosInterfaceDispatcher.Shared;

namespace Dispatcher.Services;

public class GreeterTestService : GreeterTest.GreeterTestBase
{
    public override Task<HelloReplyTest> SayHelloTest(HelloRequestTest request, ServerCallContext context)
    {
        var reply = new HelloReplyTest
        {
            Message = $"Привет, {request.Name}!"
        };
        return Task.FromResult(reply);
    }
}