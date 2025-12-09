using System;
using System.Threading;
using System.Threading.Tasks;

namespace RacingHubCarRental;

/// <summary>
/// Minimal, modern service base class supporting:
/// - Unified async execution
/// - Centralized logging (via ILogger)
/// - Safe disposal
/// - State change notifications
/// Designed for clean architecture & testability.
/// </summary>
public abstract class BaseLogic : IAsyncDisposable, IDisposable
{
    private bool _disposed;

    /// <summary>
    /// Optional logger injected by DI. If null, logging is ignored.
    /// </summary>
    protected ILogger? Logger { get; }

    /// <summary>
    /// Event fired whenever a mutation occurs in the system.
    /// </summary>
    public event Action<DataChangeType>? OnStateChanged;

    /// <summary>
    /// Creates a new service base class.
    /// </summary>
    protected BaseLogic(ILogger? logger = null)
    {
        Logger = logger;
        Log("Service initialized.");
    }

    // -------------------------------------------------------
    // Logging
    // -------------------------------------------------------

    protected void Log(string message)
    {
        Logger?.Log($"{GetType().Name}: {message}");
    }

    protected void LogError(Exception ex)
    {
        Logger?.Log($"ERROR ({GetType().Name}): {ex.Message}");
    }

    // -------------------------------------------------------
    // Async Execution Wrappers
    // -------------------------------------------------------

    /// <summary>
    /// Executes an async function with automatic error logging.
    /// </summary>
    protected async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> action,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await action(cancellationToken);
        }
        catch (Exception ex)
        {
            LogError(ex);
            throw;
        }
    }

    /// <summary>
    /// Executes an async action with unified error handling.
    /// </summary>
    protected async Task ExecuteAsync(
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await action(cancellationToken);
        }
        catch (Exception ex)
        {
            LogError(ex);
            throw;
        }
    }

    // -------------------------------------------------------
    // State Change Notification
    // -------------------------------------------------------

    protected void Notify(DataChangeType type)
    {
        Log($"State changed: {type}");
        OnStateChanged?.Invoke(type);
    }

    public void Refresh() => Notify(DataChangeType.DataRefreshed);

    // -------------------------------------------------------
    // Disposal
    // -------------------------------------------------------

    protected virtual ValueTask DisposeAsyncCore()
    {
        // Override for async cleanup (e.g., DB connections, streams)
        return ValueTask.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        Log("Async disposal started.");

        await DisposeAsyncCore();

        _disposed = true;

        GC.SuppressFinalize(this);
    }

    public void Dispose()
    {
        if (_disposed) return;

        Log("Service disposed.");

        _disposed = true;

        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Represents mutation operations a service can trigger.
/// </summary>
public enum DataChangeType
{
    None,
    ItemCreated,
    ItemUpdated,
    ItemDeleted,
    UserChanged,
    DataRefreshed,
    AllDataCleared
}

/// <summary>
/// Lightweight logger abstraction for testable logging.
/// </summary>
public interface ILogger
{
    void Log(string message);
}

