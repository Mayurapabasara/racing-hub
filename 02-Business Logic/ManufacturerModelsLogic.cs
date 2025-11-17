using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RacingHubCarRental
{
    /// <summary>
    /// Represents the logic for the CRUD operations of the manufacturer models.
    /// This class uses Entity Framework to interact with the database context.
    /// </summary>
    public class ManufacturerModelsLogic : BaseLogic
    {
        // --- READ Operations ---

        /// <summary>
        /// Retrieves all car manufacturers from the database, ordered by name.
        /// </summary>
        /// <returns>A <see cref="List{Manufacturer}"/> containing all manufacturers.</returns>
        public List<Manufacturer> GetAllManufacturers() =>
            DB.Manufacturers
                .OrderBy(m => m.ManufacturerName)
                .ToList();

        /// <summary>
        /// Retrieves all manufacturer models, including the related Manufacturer entity.
        /// Results are ordered by manufacturer name, then by model name.
        /// </summary>
        /// <returns>A <see cref="List{ManufacturerModel}"/> containing all car models.</returns>
        public List<ManufacturerModel> GetAllManufacturerModels() =>
            DB.ManufacturerModels
                .Include(mm => mm.Manufacturer)
                .OrderBy(mm => mm.Manufacturer.ManufacturerName)
                .ThenBy(mm => mm.ManufacturerModelName)
                .ToList();

        /// <summary>
        /// Retrieves manufacturer models associated with a specific manufacturer ID.
        /// </summary>
        /// <param name="manufacturerID">The ID of the manufacturer.</param>
        /// <returns>A <see cref="List{ManufacturerModel}"/> filtered by the given ID, ordered by name.</returns>
        public List<ManufacturerModel> GetModelsForManufacturer(int manufacturerID) =>
            DB.ManufacturerModels
                .Where(mm => mm.Manufacturer.ManufacturerID == manufacturerID)
                .OrderBy(mm => mm.Manufacturer.ManufacturerName)
                .ThenBy(mm => mm.ManufacturerModelName)
                .ToList();

        /// <summary>
        /// Retrieves a single manufacturer model by its primary key ID.
        /// </summary>
        /// <param name="id">The unique ID of the manufacturer model.</param>
        /// <returns>The <see cref="ManufacturerModel"/> entity or null if not found.</returns>
        public ManufacturerModel GetManufacturerModelByID(int id) =>
            DB.ManufacturerModels.Find(id);


        // --- WRITE Operations ---

        /// <summary>
        /// Inserts a new manufacturer model into the database.
        /// </summary>
        /// <param name="manufacturerModel">The manufacturer model entity to insert.</param>
        /// <exception cref="ArgumentNullException">Thrown if the provided model is null.</exception>
        public void InsertManufacturerModel(ManufacturerModel manufacturerModel)
        {
            if (manufacturerModel == null)
                throw new ArgumentNullException(nameof(manufacturerModel));

            DB.ManufacturerModels.Add(manufacturerModel);
            DB.SaveChanges();
        }

        /// <summary>
        /// Updates an existing manufacturer model in the database.
        /// </summary>
        /// <param name="manufacturerModel">The detached manufacturer model entity with updated values.</param>
        /// <exception cref="ArgumentNullException">Thrown if the provided model is null.</exception>
        public void UpdateManufacturerModel(ManufacturerModel manufacturerModel)
        {
            if (manufacturerModel == null)
                throw new ArgumentNullException(nameof(manufacturerModel));

            DB.Entry(manufacturerModel).State = EntityState.Modified;
            DB.SaveChanges();
        }

        /// <summary>
        /// Deletes a manufacturer model, optionally performing a collective delete of related entities (cascading delete).
        /// </summary>
        /// <param name="manufacturerModel">The manufacturer model to delete.</param>
        /// <param name="isCollective">If <c>true</c>, related Rentals, FleetCars, and CarModels are also deleted.</param>
        /// <exception cref="ArgumentNullException">Thrown if the provided model is null.</exception>
        public void DeleteManufacturerModel(ManufacturerModel manufacturerModel, bool isCollective = false)
        {
            if (manufacturerModel == null)
                throw new ArgumentNullException(nameof(manufacturerModel));

            // Perform collective (cascading) delete of dependent data if requested.
            if (isCollective)
            {
                var modelId = manufacturerModel.ManufacturerModelID;

                // 1. Rentals: Delete all rentals related to this manufacturer model.
                var rentalsToDelete = DB.Rentals
                    .Where(r => r.FleetCar.CarModel.ManufacturerModel.ManufacturerModelID == modelId);

                DB.Rentals.RemoveRange(rentalsToDelete);

                // 2. FleetCars: Delete all fleet cars related to this manufacturer model.
                var fleetCarsToDelete = DB.FleetCars
                    .Where(f => f.CarModel.ManufacturerModel.ManufacturerModelID == modelId);

                DB.FleetCars.RemoveRange(fleetCarsToDelete);

                // 3. CarModels: Delete all car models related to this manufacturer model.
                var carModelsToDelete = DB.CarModels
                    .Where(c => c.ManufacturerModel.ManufacturerModelID == modelId);

                DB.CarModels.RemoveRange(carModelsToDelete);
            }

            // Finally, delete the manufacturer model itself.
            DB.ManufacturerModels.Remove(manufacturerModel);
            DB.SaveChanges();
        }

        // --- Utility Operations ---

        /// <summary>
        /// Checks if a manufacturer model with the same name and manufacturer ID already exists.
        /// </summary>
        /// <param name="manufacturerModel">The manufacturer model entity to check for existence.</param>
        /// <returns><c>true</c> if a matching model exists; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the provided model is null.</exception>
        public bool IsManufacturerModelExists(ManufacturerModel manufacturerModel)
        {
            if (manufacturerModel == null)
                throw new ArgumentNullException(nameof(manufacturerModel));

            return DB.ManufacturerModels.Any(m =>
                m.ManufacturerModelName == manufacturerModel.ManufacturerModelName &&
                m.Manufacturer.ManufacturerID == manufacturerModel.ManufacturerID);
        }

    }
}
