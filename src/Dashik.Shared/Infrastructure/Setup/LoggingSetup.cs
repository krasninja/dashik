using Microsoft.Extensions.Logging;
using SimpleInjector;
using Dashik.Shared.Infrastructure.Logging;

namespace Dashik.Shared.Infrastructure.Setup;

/// <summary>
/// Logging setup class.
/// </summary>
internal class LoggingSetup(Container container, AppArguments appArguments)
{
    public ILoggerFactory Setup()
    {
        var ringBufferLoggerProvider = new RingBufferLoggerProvider();
        container.RegisterInstance(ringBufferLoggerProvider);
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddFilter(level => level >= appArguments.MinLogLevel)
                .AddRingBufferLogger(ringBufferLoggerProvider)
                .AddSimpleConsoleLogger();
            if (!string.IsNullOrEmpty(appArguments.LogFile))
            {
                builder.AddSimpleFileLogger(appArguments.LogFile);
            }
        });
        container.Register(() => loggerFactory, Lifestyle.Singleton);
        container.Register(typeof(ILogger<>), typeof(Logger<>));
        global::QueryCat.Backend.Core.Application.LoggerFactory = loggerFactory;
        return loggerFactory;
    }
}
