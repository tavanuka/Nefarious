using Microsoft.Extensions.Logging;

namespace Nefarious.Tests.Spotify;

/// <summary>
/// Workaround abstract class that implements the <see cref="ILogger{TCategoryName}"/> interface for unit testing log calls.
/// </summary>
/// <typeparam name="T">The type whose name is used for the logger category name.</typeparam>
/// <example>
/// Usage:
/// <code>
/// var exampleParam = 300;
/// var logMessage = $"Returned {exampleParam} incorrectly";
/// var _loggerMock = Substitute.For&lt;TestableLogger&lt;PlaylistSubscriptionService&gt;&gt;();
/// _loggerMock.Received().Log(LogLevel.Warning, Arg.Any&lt;EventId&gt;(), logMessage, Arg.Any&lt;Exception&gt;());
/// </code>
/// </example>
/// <remarks>
/// This implementation "simplifies" certain aspects to bypass the internal <c>FormattedLogValues</c> struct which results in a false negative
/// unit test failure.
/// <br/>
/// For more information, refer to this <a href="https://stackoverflow.com/questions/76928114/how-to-unit-test-structured-logging-calls-using-nsubstitute">StackOverflow post</a>
/// and <a href="https://github.com/dotnet/runtime/issues/67577">GitHub issue.</a>
/// </remarks>
public abstract class TestableLogger<T> : ILogger<T>
{
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        Log(logLevel, eventId, state?.ToString(), exception);
    }

    public abstract bool IsEnabled(LogLevel logLevel);

    public abstract IDisposable? BeginScope<TState>(TState state) where TState : notnull;

    public abstract void Log(LogLevel logLevel, EventId eventId, string? state, Exception? exception);
}
