using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RacingHubCarRental
{
    /// <summary>
    /// Provides asynchronous CRUD operations for CarModel entities.
    /// Extends BaseLogic for centralized database, logging & safe execution.
    /// </summary>
    public class CarModelsLogic : BaseLogic, ICarModelsLogic
    {
        // ===============================================================
        // VALIDATION HELPERS
        // ===============================================================

        /// <summary>
        /// Ensures a CarModel object is not null.
        /// </summary>
        protected void Validate(CarModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model), "CarModel cannot be null.");
        }

        /// <summary>
        /// Ensures that the given ID is valid.
        /// </summary>
        protected void ValidateId(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid CarModel ID.", nameof(id));
        }

        // ===============================================================
        // PRIVATE QUERY HELPERS
        // ===============================================================

        private IQueryable<CarModel> QueryBase()
        {
            return DB.CarModels
                     .Include(c => c.ManufacturerModel)
                     .Include(c => c.ManufacturerModel.Manufacturer);
        }

        private IQueryable<CarModel> OrderedQuery(IQueryable<CarModel> query)
        {
            return query
                .OrderBy(c => c.ManufacturerModel.Manufacturer.ManufacturerName)
                .ThenBy(c => c.ManufacturerModel.ManufacturerModelName)
                .ThenBy(c => c.ProductionYear);
        }

        // ===============================================================
        // ASYNC CRUD OPERATIONS
        // ===============================================================

        public async Task<List<CarModel>> GetAllCarModelsAsync(CancellationToken token = default)
        {
            return await SafeExecuteAsync(async () =>
            {
                var query = OrderedQuery(QueryBase());
                return await query.ToListAsync(token);
            }, token);
        }

        public CarModel GetCarModelByID(int id)
        {
            ValidateId(id);
            return DB.CarModels.Find(id);
        }

        public async Task<CarModel?> GetCarModelByIdAsync(int id, CancellationToken token = default)
        {
            ValidateId(id);

            return await SafeExecuteAsync(async () =>
            {
                return await QueryBase().FirstOrDefaultAsync(c => c.CarModelID == id, token);
            }, token);
        }

        public async Task InsertCarModelAsync(CarModel model, CancellationToken token = default)
        {
            Validate(model);

            await SafeExecuteAsync(async () =>
            {
                DB.CarModels.Add(model);
                await SaveAsync(token);
            }, token);
        }

        public async Task UpdateCarModelAsync(CarModel model, CancellationToken token = default)
        {
            Validate(model);

            await SafeExecuteAsync(async () =>
            {
                DB.Entry(model).State = EntityState.Modified;
                await SaveAsync(token);
            }, token);
        }

        // ===============================================================
        // DELETE HELPERS
        // ===============================================================

        private async Task DeleteRelatedRentalsAsync(CarModel model, CancellationToken token)
        {
            var rentals = await DB.Rentals
                .Where(r => r.FleetCar.CarModelID == model.CarModelID)
                .ToListAsync(token);

            if (rentals.Count > 0)
            {
                DB.Rentals.RemoveRange(rentals);
                await SaveAsync(token);
            }
        }

        private async Task DeleteRelatedFleetCarsAsync(CarModel model, CancellationToken token)
        {
            var fleetCars = await DB.FleetCars
                .Where(f => f.CarModelID == model.CarModelID)
                .ToListAsync(token);

            if (fleetCars.Count > 0)
            {
                DB.FleetCars.RemoveRange(fleetCars);
                await SaveAsync(token);
            }
        }

        // ===============================================================
        // DELETE ENTRY POINT
        // ===============================================================

        public async Task DeleteCarModelAsync(CarModel model, bool collective = false, CancellationToken token = default)
        {
            Validate(model);

            await SafeExecuteAsync(async () =>
            {
                if (collective)
                {
                    await DeleteRelatedRentalsAsync(model, token);
                    await DeleteRelatedFleetCarsAsync(model, token);
                }

                DB.CarModels.Remove(model);
                await SaveAsync(token);
            }, token);
        }

        // ===============================================================
        // EXISTENCE CHECK
        // ===============================================================

        public async Task<bool> IsCarModelExistsAsync(CarModel model, CancellationToken token = default)
        {
            Validate(model);

            return await SafeExecuteAsync(async () =>
            {
                return await DB.CarModels.AnyAsync(m =>
                    m.ManufacturerModelID == model.ManufacturerModelID &&
                    m.ProductionYear == model.ProductionYear &&
                    m.ManualGear == model.ManualGear, token);
            }, token);
        }

        // ===============================================================
        // GET FOR MANUFACTURER
        // ===============================================================

        public async Task<List<CarModel>> GetCarModelsForManufacturerAsync(int manufacturerId, CancellationToken token = default)
        {
            ValidateId(manufacturerId);

            return await SafeExecuteAsync(async () =>
            {
                var query = QueryBase()
                    .Where(c => c.ManufacturerModel.Manufacturer.ManufacturerID == manufacturerId);

                return await OrderedQuery(query).ToListAsync(token);
            }, token);
        }

        public List<CarModel> GetAllCarModels()
        {
            return OrderedQuery(QueryBase()).ToList();
        }

        public void InsertCarModel(CarModel model)
        {
            Validate(model);
            DB.CarModels.Add(model);
            Save();
        }

        public void UpdateCarModel(CarModel model)
        {
            Validate(model);
            DB.Entry(model).State = EntityState.Modified;
            Save();
        }

        public bool IsCarModelExists(CarModel model)
        {
            Validate(model);

            return DB.CarModels.Any(m =>
                m.ManufacturerModelID == model.ManufacturerModelID &&
                m.ProductionYear == model.ProductionYear &&
                m.ManualGear == model.ManualGear);
        }

        public List<CarModel> GetCarModelsForManufacturer(int manufacturerId)
        {
            ValidateId(manufacturerId);

            return OrderedQuery(QueryBase()
                .Where(c => c.ManufacturerModel.Manufacturer.ManufacturerID == manufacturerId))
                .ToList();
        }
    }
}

