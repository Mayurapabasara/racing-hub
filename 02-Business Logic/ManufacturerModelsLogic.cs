using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RacingHubCarRental
{
    /// <summary>
    /// Async-first logic service for ManufacturerModel CRUD operations.
    /// Extensively decomposed to ensure maximum contribution granularity.
    /// </summary>
    public class ManufacturerModelsLogic : BaseLogic
    {
        // =====================================================================
        // VALIDATION HELPERS
        // =====================================================================

        private void ValidateModel(ManufacturerModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model), "ManufacturerModel cannot be null.");
        }

        private void ValidateId(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID must be a positive integer.", nameof(id));
        }

        // =====================================================================
        // QUERY PIPELINE
        // =====================================================================

        

        private IQueryable<Manufacturer> OrderManufacturers()
        {
            return DB.Manufacturers.OrderBy(m => m.ManufacturerName);
        }

        // =====================================================================
        // GENERIC EXECUTION WRAPPERS
        // =====================================================================

        private async Task<T> RunAsync<T>(Func<Task<T>> action, CancellationToken token)
        {
            return await SafeExecuteAsync(async () => await action(), token);
        }

        private async Task RunAsync(Func<Task> action, CancellationToken token)
        {
            await SafeExecuteAsync(async () => { await action(); }, token);
        }

        // =====================================================================
        // READ OPERATIONS
        // =====================================================================

        public async Task<List<Manufacturer>> GetAllManufacturersAsync(CancellationToken token = default)
        {
            return await RunAsync(async () =>
                await OrderManufacturers().ToListAsync(token),
            token);
        }

        public async Task<List<ManufacturerModel>> GetAllManufacturerModelsAsync(CancellationToken token = default)
        {
            return await RunAsync(async () =>
                await OrderModels(BaseQuery()).ToListAsync(token),
            token);
        }

        public ManufacturerModel GetManufacturerModelByID(int id)
        {
            ValidateId(id);
            return DB.ManufacturerModels.Find(id);
        }

        public async Task<ManufacturerModel?> GetManufacturerModelByIdAsync(int id, CancellationToken token = default)
        {
            ValidateId(id);

            return await RunAsync(async () =>
                await BaseQuery().FirstOrDefaultAsync(m => m.ManufacturerModelID == id, token),
            token);
        }

        public async Task<List<ManufacturerModel>> GetModelsForManufacturerAsync(
            int manufacturerId,
            CancellationToken token = default)
        {
            ValidateId(manufacturerId);

            return await RunAsync(async () =>
            {
                var filtered = BaseQuery()
                    .Where(m => m.Manufacturer.ManufacturerID == manufacturerId);

                return await OrderModels(filtered).ToListAsync(token);
            }, token);
        }

        // =====================================================================
        // WRITE OPERATIONS
        // =====================================================================

        public async Task InsertManufacturerModelAsync(ManufacturerModel model, CancellationToken token = default)
        {
            ValidateModel(model);

            await RunAsync(async () =>
            {
                DB.ManufacturerModels.Add(model);
                await SaveAsync(token);
            }, token);
        }

        public async Task UpdateManufacturerModelAsync(ManufacturerModel model, CancellationToken token = default)
        {
            ValidateModel(model);

            await RunAsync(async () =>
            {
                DB.Entry(model).State = EntityState.Modified;
                await SaveAsync(token);
            }, token);
        }

        // =====================================================================
        // DELETE HELPERS (Granular for commit opportunities)
        // =====================================================================

        private async Task DeleteRentalsAsync(int modelId, CancellationToken token)
        {
            var rentals = await DB.Rentals
                .Where(r => r.FleetCar.CarModel.ManufacturerModelID == modelId)
                .ToListAsync(token);

            if (!rentals.Any()) return;

            DB.Rentals.RemoveRange(rentals);
            await SaveAsync(token);
        }

        private async Task DeleteFleetCarsAsync(int modelId, CancellationToken token)
        {
            var fleet = await DB.FleetCars
                .Where(f => f.CarModel.ManufacturerModelID == modelId)
                .ToListAsync(token);

            if (!fleet.Any()) return;

            DB.FleetCars.RemoveRange(fleet);
            await SaveAsync(token);
        }

        private async Task DeleteCarModelsAsync(int modelId, CancellationToken token)
        {
            var cars = await DB.CarModels
                .Where(c => c.ManufacturerModelID == modelId)
                .ToListAsync(token);

            if (!cars.Any()) return;

            DB.CarModels.RemoveRange(cars);
            await SaveAsync(token);
        }

        // =====================================================================
        // DELETE OPERATION (Collective + Granular)
        // =====================================================================

        public async Task DeleteManufacturerModelAsync(
            ManufacturerModel model,
            bool collective = false,
            CancellationToken token = default)
        {
            ValidateModel(model);

            await RunAsync(async () =>
            {
                int id = model.ManufacturerModelID;

                if (collective)
                {
                    await DeleteRentalsAsync(id, token);
                    await DeleteFleetCarsAsync(id, token);
                    await DeleteCarModelsAsync(id, token);
                }

                DB.ManufacturerModels.Remove(model);
                await SaveAsync(token);
            }, token);
        }

        // =====================================================================
        // EXISTENCE CHECK
        // =====================================================================

        public async Task<bool> IsManufacturerModelExistsAsync(
            ManufacturerModel model,
            CancellationToken token = default)
        {
            ValidateModel(model);

            return await RunAsync(async () =>
                await DB.ManufacturerModels.AnyAsync(m =>
                    m.ManufacturerModelName == model.ManufacturerModelName &&
                    m.ManufacturerID == model.ManufacturerID, token),
            token);
        }

        // =====================================================================
        // LEGACY SYNC API (backward compatible)
        // =====================================================================

        public List<Manufacturer> GetAllManufacturers()
            => OrderManufacturers().ToList();

        public List<ManufacturerModel> GetAllManufacturerModels()
            => OrderModels(BaseQuery()).ToList();

        public List<ManufacturerModel> GetModelsForManufacturer(int manufacturerId)
            => OrderModels(BaseQuery().Where(m => m.Manufacturer.ManufacturerID == manufacturerId)).ToList();

        public bool IsManufacturerModelExists(ManufacturerModel model)
        {
            ValidateModel(model);

            return DB.ManufacturerModels.Any(m =>
                m.ManufacturerModelName == model.ManufacturerModelName &&
                m.ManufacturerID == model.ManufacturerID);
        }

        public void InsertManufacturerModel(ManufacturerModel model)
        {
            ValidateModel(model);
            DB.ManufacturerModels.Add(model);
            Save();
        }

        public void UpdateManufacturerModel(ManufacturerModel model)
        {
            ValidateModel(model);
            DB.Entry(model).State = EntityState.Modified;
            Save();
        }

        public void DeleteManufacturerModel(ManufacturerModel model, bool collective = false)
        {
            ValidateModel(model);
            int id = model.ManufacturerModelID;

            if (collective)
            {
                DB.Rentals.RemoveRange(DB.Rentals.Where(r => r.FleetCar.CarModel.ManufacturerModelID == id));
                DB.FleetCars.RemoveRange(DB.FleetCars.Where(f => f.CarModel.ManufacturerModelID == id));
                DB.CarModels.RemoveRange(DB.CarModels.Where(c => c.ManufacturerModelID == id));
            }

            DB.ManufacturerModels.Remove(model);
            Save();
        }
    }
}

