using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RacingHubCarRental.Services.Abstractions;

namespace RacingHubCarRental.Services
{
    /// <summary>
    /// Provides read-only manufacturer-related queries.
    /// Extends BaseLogic for safe DB access, logging and async execution.
    /// </summary>
    public class ManufacturerQueryService : BaseLogic
    {
        // ============================================================
        // VALIDATION
        // ============================================================

        private void ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Manufacturer name must not be empty.", nameof(name));
        }

        private void ValidateId(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid manufacturer ID.", nameof(id));
        }

        // ============================================================
        // QUERY HELPERS
        // ============================================================

        private IQueryable<Manufacturer> QueryBase()
        {
            return DB.Manufacturers;
        }

        private IQueryable<Manufacturer> Ordered(IQueryable<Manufacturer> query)
        {
            return query.OrderBy(m => m.ManufacturerName);
        }

        // ============================================================
        // ASYNC OPERATIONS
        // ============================================================

        /// <summary>
        /// Retrieves all manufacturers ordered alphabetically.
        /// </summary>
        public async Task<List<Manufacturer>> GetOrderedManufacturersAsync(
            CancellationToken token = default)
        {
            return await SafeExecuteAsync(async () =>
            {
                return await Ordered(QueryBase()).ToListAsync(token);
            }, token);
        }

        /// <summary>
        /// Retrieves a manufacturer by its ID asynchronously.
        /// </summary>
        public async Task<Manufacturer?> GetManufacturerByIdAsync(
            int id,
            CancellationToken token = default)
        {
            ValidateId(id);

            return await SafeExecuteAsync(async () =>
            {
                return await QueryBase()
                    .FirstOrDefaultAsync(m => m.ManufacturerID == id, token);
            }, token);
        }

        /// <summary>
        /// Checks if a manufacturer with the given name exists.
        /// Case-insensitive.
        /// </summary>
        public async Task<bool> ManufacturerExistsByNameAsync(
            string name,
            CancellationToken token = default)
        {
            ValidateName(name);

            var normalized = name.Trim().ToLower();

            return await SafeExecuteAsync(async () =>
            {
                return await QueryBase()
                    .AnyAsync(m => m.ManufacturerName.ToLower() == normalized, token);
            }, token);
        }

        // ============================================================
        // SYNC - BACKWARD COMPATIBILITY
        // ============================================================

        /// <summary>
        /// Returns all manufacturers (sync version).
        /// </summary>
        public List<Manufacturer> GetOrderedManufacturers()
        {
            return Ordered(QueryBase()).ToList();
        }

        /// <summary>
        /// Returns a manufacturer by ID (sync).
        /// </summary>
        public Manufacturer GetManufacturerById(int id)
        {
            ValidateId(id);
            return DB.Manufacturers.Find(id);
        }

        /// <summary>
        /// Checks if a manufacturer exists by name (sync).
        /// </summary>
        public bool CheckIfManufacturerExistsByName(string manufacturerName)
        {
            ValidateName(manufacturerName);

            var normalized = manufacturerName.Trim().ToLower();

            return DB.Manufacturers
                .Any(m => m.ManufacturerName.ToLower() == normalized);
        }
    }
}

