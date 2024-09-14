using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Fixtures;

public class GenericLoggerFixture<TEntity> where TEntity : class
{
    public GenericLoggerFixture()
    {
        Logger = new Mock<ILogger<TEntity>>().Object;
    }

    internal ILogger<TEntity> Logger { get; private set; }
}