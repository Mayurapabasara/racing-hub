using System;
using System.Data.Entity;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace RacingHubCarRental
{
    /// <summary>
    /// Base class for all logic/service classes.
    /// Provides:
    /// - Lazy-loaded EF DbContext
    /// - Async helper wrappers
    /// - Centralized logging
    /// - Safe disposal pattern
    /// - Future extensibility hooks
    /// </summary>
    public abstract class BaseLogic : IDisposable
    {
        // ============================================================
        // Private Fields
        // ============================================================
        private bool _disposed;
        private RacingHubCarRentalEntities? _db;

        // ============================================================
        // Protected Properties
        // ============================================================

        /// <summary>
        /// Lazy-created database context instance.
        /// Derived classes should always use DB instead of creating their own.
        /// </summary>
        protected RacingHubCarRentalEntities DB
        {
            get
            {
                if (_db == null)
                {
                    _db = new RacingHubCarRentalEntities();
                    ConfigureContext(_db);
                }

                return _db;
            }
        }

        // ============================================================
        // Constructor
        // ============================================================
        protected BaseLogic()
        {
            // Reserved for dependency injection extensions (future)
        }

        // ============================================================
        // Virtual Hooks (Extensible by child classes)
        // ============================================================

        /// <summary>
        /// Allows child classes to override and configure the DbContext.
        /// Example: DB.Configuration.LazyLoadingEnabled = false;
        /// </summary>
        protected virtual void ConfigureContext(RacingHubCarRentalEntities context)
        {
            // Default configuration
            context.Configuration.LazyLoadingEnabled = true;
            context.Configuration.ProxyCreationEnabled = true;
        }

        /// <summary>
        /// Optional hook for logging database operations.
        /// Child classes can override this.
        /// </summary>
        protected virtual void Log(string message)
        {
            Debug.WriteLine($"[BaseLogic] {message}");
        }

        // ============================================================
        // Async Helper Methods (boosts code reuse)
        // ============================================================

        /// <summary>
        /// Runs a database operation safely using async/await.
        /// Centralizes error handling and optional logging.
        /// </summary>
        protected async Task<T> SafeExecuteAsync<T>(
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
                throw; // Rethrow to allow controller-level handling
            }
        }

        /// <summary>
        /// Async version without return value.
        /// </summary>
        protected async Task SafeExecuteAsync(
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
        // Save Changes Helpers
        // ============================================================

        /// <summary>
        /// Saves changes asynchronously with cancellation token support.
        /// </summary>
        protected async Task<int> SaveAsync(CancellationToken token = default)
        {
            Log("Saving changes to database.");
            return await DB.SaveChangesAsync(token);
        }

        /// <summary>
        /// Synchronous SaveChanges wrapper.
        /// </summary>
        protected int Save()
        {
            Log("Saving changes (sync).");
            return DB.SaveChanges();
        }

        // ============================================================
        // IDisposable Implementation
        // ============================================================

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _db?.Dispose();
                }

                _disposed = true;
            }
        }

        /// <summary>
        /// Disposes the underlying DbContext and resources.
        </summary>
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
}

