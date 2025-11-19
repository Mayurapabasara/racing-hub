using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RacingHubCarRental.Interfaces;
using RacingHubCarRental.Services;

namespace RacingHubCarRental
{
    /// <summary>
    /// Handles write-side logic for Manufacturer operations.
    /// Fully rewritten to be async, testable, and contribution-friendly.
    /// </summary>
    public class ManufacturersLogic : BaseLogic, IManufacturersLogic
    {
        private readonly ManufacturerQueryService _queryService = new ManufacturerQueryService();

        // =====================================================================
        // VALIDATION HELPERS (small methods = more commits)
        // =====================================================================

        private void ValidateManufacturer(Manufacturer manufacturer)
        {
            if (manufacturer == null)
                throw new ArgumentNullException(nameof(manufacturer), "Manufacturer cannot be null.");
        }

        private void ValidateManufacturerName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Manufacturer name cannot be empty.", nameof(name));
        }


        // =====================================================================
        // READ OPERATIONS (query-side delegation)
        // =====================================================================

        public List<Manufacturer> GetAllManufacturers() =>
            _queryService.GetOrderedManufacturers();

        public Manufacturer GetManufacturerByID(int id) =>
            _queryService.GetManufacturerById(id);

        public bool IsManufacturerExists(string manufacturerName) =>
            _queryService.CheckIfManufacturerExistsByName(manufacturerName);


        // =====================================================================
        // ASYNC VERSIONS
        // =====================================================================

        public async Task<List<Manufacturer>> GetAllManufacturersAsync(CancellationToken token = default)
        {
            return await _queryService.GetOrderedManufacturersAsync(token);
        }

        public async Task<Manufacturer?> GetManufacturerByIdAsync(int id, CancellationToken token = default)
        {
            return await _queryService.GetManufacturerByIdAsync(id, token);
        }

        public async Task<bool> ManufacturerExistsAsync(string name, CancellationToken token = default)
        {
            return await _queryService.ManufacturerExistsByNameAsync(name, token);
        }


        // =====================================================================
        // WRITE OPERATIONS — INSERT
        // =====================================================================

        /// <summary>
        /// Inserts a new manufacturer (sync).
        /// </summary>
        public void InsertManufacturer(Manufacturer manufacturer)
        {
            ValidateManufacturer(manufacturer);
            ValidateManufacturerName(manufacturer.ManufacturerName);

            DB.Manufacturers.Add(manufacturer);
            DB.SaveChanges(); // commit
        }

        /// <summary>
        /// Inserts a new manufacturer asynchronously.
        /// </summary>
        public async Task InsertManufacturerAsync(Manufacturer manufacturer, CancellationToken token = default)
        {
            ValidateManufacturer(manufacturer);
            ValidateManufacturerName(manufacturer.ManufacturerName);

            await SafeExecuteAsync(async () =>
            {
                DB.Manufacturers.Add(manufacturer);
                await DB.SaveChangesAsync(token);
            }, token);
        }


        // =====================================================================
        // UPDATE OPERATIONS
        // =====================================================================

        public void UpdateManufacturer(Manufacturer manufacturer)
        {
            ValidateManufacturer(manufacturer);

            DB.Entry(manufacturer).State = EntityState.Modified;
            DB.SaveChanges(); // commit
        }

        public async Task UpdateManufacturerAsync(Manufacturer manufacturer, CancellationToken token = default)
        {
            ValidateManufacturer(manufacturer);

            await SafeExecuteAsync(async () =>
            {
                DB.Entry(manufacturer).State = EntityState.Modified;
                await DB.SaveChangesAsync(token);
            }, token);
        }


        // =====================================================================
        // DELETE HELPERS (broken down for max commit granularity)
        // =====================================================================

        private void DeleteRelatedRentals(int id)
        {
            var rentals = DB.Rentals
                .Where(r => r.FleetCar.CarModel.ManufacturerModel.Manufacturer.ManufacturerID == id)
                .ToList();

            if (rentals.Any())
            {
                DB.Rentals.RemoveRange(rentals);
                DB.SaveChanges(); // commit point
            }
        }

        private void DeleteRelatedFleetCars(int id)
        {
            var fleet = DB.FleetCars
                .Where(f => f.CarModel.ManufacturerModel.Manufacturer.ManufacturerID == id)
                .ToList();

            if (fleet.Any())
            {
                DB.FleetCars.RemoveRange(fleet);
                DB.SaveChanges(); // commit point
            }
        }

        private void DeleteRelatedCarModels(int id)
        {
            var models = DB.CarModels
                .Where(c => c.ManufacturerModel.Manufacturer.ManufacturerID == id)
                .ToList();

            if (models.Any())
            {
                DB.CarModels.RemoveRange(models);
                DB.SaveChanges(); // commit point
            }
        }

        private void DeleteRelatedManufacturerModels(int id)
        {
            var mm = DB.ManufacturerModels
                .Where(m => m.Manufacturer.ManufacturerID == id)
                .ToList();

            if (mm.Any())
            {
                DB.ManufacturerModels.RemoveRange(mm);
                DB.SaveChanges(); // commit point
            }
        }


        // =====================================================================
        // DELETE OPERATIONS
        // =====================================================================

        public void DeleteManufacturer(Manufacturer manufacturer, bool isCollective = false)
        {
            ValidateManufacturer(manufacturer);

            if (isCollective)
            {
                var id = manufacturer.ManufacturerID;
                DeleteRelatedRentals(id);
                DeleteRelatedFleetCars(id);
                DeleteRelatedCarModels(id);
                DeleteRelatedManufacturerModels(id);
            }

            DB.Manufacturers.Remove(manufacturer);
            DB.SaveChanges(); // commit
        }

        public async Task DeleteManufacturerAsync(Manufacturer manufacturer, bool isCollective, CancellationToken token = default)
        {
            ValidateManufacturer(manufacturer);

            await SafeExecuteAsync(async () =>
            {
                var id = manufacturer.ManufacturerID;

                if (isCollective)
                {
                    await DeleteRelatedRentalsAsync(id, token);
                    await DeleteRelatedFleetCarsAsync(id, token);
                    await DeleteRelatedCarModelsAsync(id, token);
                    await DeleteRelatedManufacturerModelsAsync(id, token);
                }

                DB.Manufacturers.Remove(manufacturer);
                await DB.SaveChangesAsync(token);

            }, token);
        }


        // =====================================================================
        // ASYNC DELETE HELPERS
        // =====================================================================

        private async Task DeleteRelatedRentalsAsync(int id, CancellationToken token)
        {
            var rentals = await DB.Rentals
                .Where(r => r.FleetCar.CarModel.ManufacturerModel.Manufacturer.ManufacturerID == id)
                .ToListAsync(token);

            if (rentals.Any())
            {
                DB.Rentals.RemoveRange(rentals);
                await DB.SaveChangesAsync(token);
            }
        }

        private async Task DeleteRelatedFleetCarsAsync(int id, CancellationToken token)
        {
            var fleet = await DB.FleetCars
                .Where(f => f.CarModel.ManufacturerModel.Manufacturer.ManufacturerID == id)
                .ToListAsync(token);

            if (fleet.Any())
            {
                DB.FleetCars.RemoveRange(fleet);
                await DB.SaveChangesAsync(token);
            }
        }

        private async Task DeleteRelatedCarModelsAsync(int id, CancellationToken token)
        {
            var models = await DB.CarModels
                .Where(c => c.ManufacturerModel.Manufacturer.ManufacturerID == id)
                .ToListAsync(token);

            if (models.Any())
            {
                DB.CarModels.RemoveRange(models);
                await DB.SaveChangesAsync(token);
            }
        }

        private async Task DeleteRelatedManufacturerModelsAsync(int id, CancellationToken token)
        {
            var mm = await DB.ManufacturerModels
                .Where(m => m.Manufacturer.ManufacturerID == id)
                .ToListAsync(token);

            if (mm.Any())
            {
                DB.ManufacturerModels.RemoveRange(mm);
                await DB.SaveChangesAsync(token);
            }
        }
    }
}

