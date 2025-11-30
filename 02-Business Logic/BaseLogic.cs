using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace RacingHubCarRental
{
    /// <summary>
    /// Base abstraction for all logic/service classes.
    /// Modernized to match a clean service-oriented architecture:
    /// - Centralized exception handling
    /// - Async-first execution model
    /// - Pluggable data providers (future microservice compatibility)
    /// - Unified logging hooks
    /// - Lightweight and test-friendly design
    /// </summary>
    public abstract class BaseLogic : IDisposable
    {
        // ============================================================
        // Private State
        // ============================================================
        private bool _disposed;

        /// <summary>
        /// Event fired when any service triggers a data change.
        /// Helps keep UI/admin dashboards in sync.
        /// </summary>
        public event Action<DataChangeType>? OnStateChanged;

        // ============================================================
        // Constructors
        // ============================================================

        protected BaseLogic()
        {
            // Reserved for future dependency injection
            Log("BaseLogic initialized.");
        }

        // ============================================================
        // Logging Utilities
        // ============================================================

        /// <summary>
        /// Writes a log entry using Debug trace.
        /// Derived services may override to implement:
        /// - File logging
        /// - External telemetry
        /// - ELK / CloudWatch pipelines
        /// </summary>
        protected virtual void Log(string message)
        {
            Debug.WriteLine($"[{GetType().Name}] {message}");
        }

        // ============================================================
        // Unified Async Execution Wrappers
        // ============================================================

        /// <summary>
        /// Safely executes async operations with automatic logging.
        /// Recommended for all database or external API calls.
        /// </summary>
        protected async Task<T> ExecuteAsync<T>(
            Func<Task<T>> action,
            CancellationToken token = default)
        {
            try
            {
                return await action();
            }
            catch (Exception ex)
            {
                Log($"Error: {ex.Message}");
                throw; // Allow controller or API layer to handle
            }
        }

        /// <summary>
        /// Safe wrapper for void async operations.
        /// </summary>
        protected async Task ExecuteAsync(
            Func<Task> action,
            CancellationToken token = default)
        {
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                Log($"Error: {ex.Message}");
                throw;
            }
        }

        // ============================================================
        // Shared Event Trigger Methods
        // ============================================================

        /// <summary>
        /// Notify subscribers that a data mutation occurred.
        /// Useful for dashboards & future notification microservice hooks.
        /// </summary>
        protected void Notify(DataChangeType type)
        {
            Log($"State changed: {type}");
            OnStateChanged?.Invoke(type);
        }

        public void RefreshData()
        {
            Notify(DataChangeType.DataRefreshed);
        }

        // ============================================================
        // Cleanup & Resource Management
        // ============================================================

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                // In updated architecture, no EF DbContext here.
                // Reserved for future disposable dependencies.
                Log("Service resources cleaned.");
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~BaseLogic()
        {
            Dispose(false);
        }
    }

    /// <summary>
    /// Enum representing different system-wide change events.
    /// Extended for richer admin dashboard integrations.
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
}

