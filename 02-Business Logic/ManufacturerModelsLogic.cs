using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RacingHubCarRental
{
    /// <summary>
    /// Provides asynchronous CRUD operations and utility functions for ManufacturerModel entities.
    /// Uses BaseLogic for shared DB context, safe-execution, and logging mechanisms.
    /// </summary>
    public class ManufacturerModelsLogic : BaseLogic
    {
        // ============================================================
        // VALIDATION
        // ============================================================

        protected void Validate(ManufacturerModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model), "ManufacturerModel cannot be null.");
        }

        protected void ValidateId(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid ManufacturerModel ID.", nameof(id));
        }

        // ============================================================
        // QUERY HELPERS
        // ============================================================

        private IQueryable<ManufacturerModel> QueryBase()
        {
            return DB.ManufacturerModels
                     .Include(mm => mm.Manufacturer);
        }

        private IQueryable<ManufacturerModel> Ordered(IQueryable<ManufacturerModel> query)
        {
            return query
                .OrderBy(mm => mm.Manufacturer.ManufacturerName)
                .ThenBy(mm => mm.ManufacturerModelName);
        }

        // ============================================================
        // MANUFACTURERS (Read-only Helper)
        // ============================================================

        public async Task<List<Manufacturer>> GetAllManufacturersAsync(CancellationToken token = default)
        {
            return await SafeExecuteAsync(async () =>
            {
                return await DB.Manufacturers
                    .OrderBy(m => m.ManufacturerName)
                    .ToListAsync(token);
            }, token);
        }

        // ============================================================
        // READ OPERATIONS
        // ============================================================

        public async Task<List<ManufacturerModel>> GetAllManufacturerModelsAsync(CancellationToken token = default)
        {
            return await SafeExecuteAsync(async () =>
            {
                return await Ordered(QueryBase()).ToListAsync(token);
            }, token);
        }

        public ManufacturerModel GetManufacturerModelByID(int id)
        {
            ValidateId(id);
            return DB.ManufacturerModels.Find(id);
        }

        public async Task<ManufacturerModel?> GetManufacturerModelByIdAsync(int id, CancellationToken token = default)
        {
            ValidateId(id);

            return await SafeExecuteAsync(async () =>
            {
                return await QueryBase()
                    .FirstOrDefaultAsync(m => m.ManufacturerModelID == id, token);
            }, token);
        }

        public async Task<List<ManufacturerModel>> GetModelsForManufacturerAsync(
            int manufacturerId,
            CancellationToken token = default)
        {
            ValidateId(manufacturerId);

            return await SafeExecuteAsync(async () =>
            {
                var query = QueryBase()
                    .Where(mm => mm.Manufacturer.ManufacturerID == manufacturerId);

                return await Ordered(query).ToListAsync(token);
            }, token);
        }

        // ============================================================
        // WRITE OPERATIONS
        // ============================================================

        public async Task InsertManufacturerModelAsync(ManufacturerModel model, CancellationToken token = default)
        {
            Validate(model);

            await SafeExecuteAsync(async () =>
            {
                DB.ManufacturerModels.Add(model);
                await SaveAsync(token);
            }, token);
        }

        public async Task UpdateManufacturerModelAsync(ManufacturerModel model, CancellationToken token = default)
        {
            Validate(model);

            await SafeExecuteAsync(async () =>
            {
                DB.Entry(model).State = EntityState.Modified;
                await SaveAsync(token);
            }, token);
        }

        // ============================================================
        // DELETE HELPERS
        // ============================================================

        private async Task DeleteRelatedRentalsAsync(int modelId, CancellationToken token)
        {
            var rentals = await DB.Rentals
                .Where(r => r.FleetCar.CarModel.ManufacturerModelID == modelId)
                .ToListAsync(token);

            if (rentals.Any())
            {
                DB.Rentals.RemoveRange(rentals);
                await SaveAsync(token);
            }
        }

        private async Task DeleteRelatedFleetCarsAsync(int modelId, CancellationToken token)
        {
            var fleetCars = await DB.FleetCars
                .Where(f => f.CarModel.ManufacturerModelID == modelId)
                .ToListAsync(token);

            if (fleetCars.Any())
            {
                DB.FleetCars.RemoveRange(fleetCars);
                await SaveAsync(token);
            }
        }

        private async Task DeleteRelatedCarModelsAsync(int modelId, CancellationToken token)
        {
            var relatedCars = await DB.CarModels
                .Where(c => c.ManufacturerModelID == modelId)
                .ToListAsync(token);

            if (relatedCars.Any())
            {
                DB.CarModels.RemoveRange(relatedCars);
                await SaveAsync(token);
            }
        }

        // ============================================================
        // DELETE OPERATION
        // ============================================================

        public async Task DeleteManufacturerModelAsync(
            ManufacturerModel model,
            bool collective = false,
            CancellationToken token = default)
        {
            Validate(model);

            await SafeExecuteAsync(async () =>
            {
                var modelId = model.ManufacturerModelID;

                if (collective)
                {
                    await DeleteRelatedRentalsAsync(modelId, token);
                    await DeleteRelatedFleetCarsAsync(modelId, token);
                    await DeleteRelatedCarModelsAsync(modelId, token);
                }

                DB.ManufacturerModels.Remove(model);
                await SaveAsync(token);
            }, token);
        }

        // ============================================================
        // EXISTENCE CHECK
        // ============================================================

        public async Task<bool> IsManufacturerModelExistsAsync(ManufacturerModel model, CancellationToken token = default)
        {
            Validate(model);

            return await SafeExecuteAsync(async () =>
            {
                return await DB.ManufacturerModels.AnyAsync(m =>
                    m.ManufacturerModelName == model.ManufacturerModelName &&
                    m.ManufacturerID == model.ManufacturerID, token);
            }, token);
        }

        // ============================================================
        // LEGACY SYNC API (Optional backward compatibility)
        // ============================================================

        public List<Manufacturer> GetAllManufacturers()
            => DB.Manufacturers.OrderBy(m => m.ManufacturerName).ToList();

        public List<ManufacturerModel> GetAllManufacturerModels()
            => Ordered(QueryBase()).ToList();

        public List<ManufacturerModel> GetModelsForManufacturer(int manufacturerId)
            => Ordered(QueryBase()
                .Where(mm => mm.Manufacturer.ManufacturerID == manufacturerId))
                .ToList();

        public bool IsManufacturerModelExists(ManufacturerModel model)
        {
            Validate(model);

            return DB.ManufacturerModels.Any(m =>
                m.ManufacturerModelName == model.ManufacturerModelName &&
                m.ManufacturerID == model.ManufacturerID);
        }

        public void InsertManufacturerModel(ManufacturerModel model)
        {
            Validate(model);
            DB.ManufacturerModels.Add(model);
            Save();
        }

        public void UpdateManufacturerModel(ManufacturerModel model)
        {
            Validate(model);
            DB.Entry(model).State = EntityState.Modified;
            Save();
        }

        public void DeleteManufacturerModel(ManufacturerModel model, bool collective = false)
        {
            Validate(model);
            var id = model.ManufacturerModelID;

            if (collective)
            {
                var rentals = DB.Rentals.Where(r => r.FleetCar.CarModel.ManufacturerModelID == id);
                DB.Rentals.RemoveRange(rentals);

                var fleet = DB.FleetCars.Where(f => f.CarModel.ManufacturerModelID == id);
                DB.FleetCars.RemoveRange(fleet);

                var cars = DB.CarModels.Where(c => c.ManufacturerModelID == id);
                DB.CarModels.RemoveRange(cars);
            }

            DB.ManufacturerModels.Remove(model);
            Save();
        }
    }
}

