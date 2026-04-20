using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Dashik.Shared.Infrastructure.Logging;

/// <summary>
/// Extension methods for <see cref="Microsoft.Extensions.Logging.ILoggingBuilder" />.
/// </summary>
public static class LoggingBuilderExtensions
{
    /// <summary>
    /// Add <see cref="RingBufferLoggerProvider" />.
    /// </summary>
    /// <param name="builder">Logging builder.</param>
    /// <returns><see cref="ILoggingBuilder" /> instance.</returns>
    public static ILoggingBuilder AddRingBufferLogger(
        this ILoggingBuilder builder)
    {
        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<ILoggerProvider, RingBufferLoggerProvider>());
        return builder;
    }

    /// <summary>
    /// Add <see cref="RingBufferLoggerProvider" />.
    /// </summary>
    /// <param name="builder">Logging builder.</param>
    /// <param name="instance">Instance of <see cref="RingBufferLoggerProvider" />.</param>
    /// <returns><see cref="ILoggingBuilder" /> instance.</returns>
    public static ILoggingBuilder AddRingBufferLogger(
        this ILoggingBuilder builder,
        RingBufferLoggerProvider instance)
    {
        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<ILoggerProvider, RingBufferLoggerProvider>(_ => instance));
        return builder;
    }

    /// <summary>
    /// Add <see cref="SimpleFileLoggerProvider" />.
    /// </summary>
    /// <param name="builder">Logging builder.</param>
    /// <param name="fileName">File name path to log.</param>
    /// <returns><see cref="ILoggingBuilder" /> instance.</returns>
    public static ILoggingBuilder AddSimpleFileLogger(
        this ILoggingBuilder builder,
        string fileName)
    {
        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<ILoggerProvider, SimpleFileLoggerProvider>(
                _ => new SimpleFileLoggerProvider(fileName)));
        return builder;
    }

    /// <summary>
    /// Add <see cref="SimpleConsoleLoggerProvider" />.
    /// </summary>
    /// <param name="builder">Logging builder.</param>
    /// <returns><see cref="ILoggingBuilder" /> instance.</returns>
    public static ILoggingBuilder AddSimpleConsoleLogger(this ILoggingBuilder builder)
    {
        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<ILoggerProvider, SimpleConsoleLoggerProvider>(
                _ => new SimpleConsoleLoggerProvider()));
        return builder;
    }
}
