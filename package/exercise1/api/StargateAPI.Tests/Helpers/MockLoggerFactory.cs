using Microsoft.Extensions.Logging;
using Moq;

namespace StargateAPI.Tests.Helpers;

public static class MockLoggerFactory
{
    public static ILogger<T> CreateMockLogger<T>()
    {
        return Mock.Of<ILogger<T>>();
    }
}
