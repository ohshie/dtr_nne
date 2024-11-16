using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Fixtures;

public class GenericLoggerFixture<TEntity> where TEntity : class
{
    internal ILogger<TEntity> Logger { get; private set; } = new Mock<ILogger<TEntity>>().Object;
}