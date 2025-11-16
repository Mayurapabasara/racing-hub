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
    /// Represents the logic for the CRUD of the car models, implementing ICarModelsLogic.
    /// Methods are broken down into granular steps to facilitate regular and specific commits.
    /// </summary>
    public class CarModelsLogic : BaseLogic, ICarModelsLogic
    {
        // --- Private Validation Helpers ---

        /// <summary>
        /// Validates that the CarModel object is not null.
        /// </summary>
        /// <param name="carModel">The car model to check.</param>
        private void ValidateCarModel(CarModel carModel)
        {
            if (carModel == null)
            {
                throw new ArgumentNullException(nameof(carModel), "Car Model object cannot be null.");
            }
        }

        // --- Core CRUD Operations ---

        /// <summary>
        /// Gets all the car models, ordered for display.
        /// </summary>
        /// <returns>List of all the car models.</returns>
        public List<CarModel> GetAllCarModels()
        {
            return DB.CarModels.Include(c => c.ManufacturerModel)
                               .OrderBy(c => c.ManufacturerModel.Manufacturer.ManufacturerName)
                               .ThenBy(c => c.ManufacturerModel.ManufacturerModelName)
                               .ThenBy(c => c.ProductionYear)
                               .ToList();
        }

        /// <summary>
        /// Gets a car model by ID.
        /// </summary>
        /// <param name="id">The ID of the car model.</param>
        /// <returns>The car model details.</returns>
        public CarModel GetCarModelByID(int id)
        {
            // Simple retrieval method, hard to split further, but we ensure Find is used efficiently.
            return DB.CarModels.Find(id);
        }

        /// <summary>
        /// Inserts a new car model.
        /// </summary>
        /// <param name="carModel">The car model to insert.</param>
        public void InsertCarModel(CarModel carModel)
        {
            ValidateCarModel(carModel); // Commit 1: Add validation call
            
            DB.CarModels.Add(carModel);
            DB.SaveChanges(); // Commit 2: Save changes (separate atomic action)
        }

        /// <summary>
        /// Updates a car model.
        /// </summary>
        /// <param name="carModel">The car model to update.</param>
        public void UpdateCarModel(CarModel carModel)
        {
            ValidateCarModel(carModel); // Commit 1: Add validation call

            DB.Entry(carModel).State = EntityState.Modified;
            DB.SaveChanges(); // Commit 2: Save changes
        }

        // --- Delete Operation (Maximized Granularity) ---

        /// <summary>
        /// Deletes all Rental records associated with the fleet cars of the specified CarModel.
        /// </summary>
        /// <param name="carModel">The car model whose related rentals should be deleted.</param>
        private void DeleteRelatedRentals(CarModel carModel)
        {
            // Find rentals related to any fleet car belonging to this CarModelID
            List<Rental> rentalsToDelete = DB.Rentals
                .Where(r => r.FleetCar.CarModelID == carModel.CarModelID)
                .ToList();
            
            if (rentalsToDelete.Any())
            {
                DB.Rentals.RemoveRange(rentalsToDelete); // Use RemoveRange for efficiency
                DB.SaveChanges(); // Separate commit opportunity
            }
        }

        /// <summary>
        /// Deletes all FleetCar records associated with the specified CarModel.
        /// </summary>
        /// <param name="carModel">The car model whose fleet cars should be deleted.</param>
        private void DeleteRelatedFleetCars(CarModel carModel)
        {
            List<FleetCar> fleetCarsToDelete = DB.FleetCars
                .Where(f => f.CarModelID == carModel.CarModelID)
                .ToList();
            
            if (fleetCarsToDelete.Any())
            {
                DB.FleetCars.RemoveRange(fleetCarsToDelete); // Use RemoveRange
                DB.SaveChanges(); // Separate commit opportunity
            }
        }

        /// <summary>
        /// Deletes a car model. If isCollective is true, related rental and fleet data are deleted first.
        /// </summary>
        /// <param name="carModel">The car model to delete.</param>
        /// <param name="isCollective">Indicator if to perform a collective delete, if to delete all it's related data.</param>
        public void DeleteCarModel(CarModel carModel, bool isCollective = false)
        {
            ValidateCarModel(carModel); // Commit 1: Validation check

            // Checks if to perform a collective delete, using granular helpers:
            if (isCollective)
            {
                // Deletes related data (each helper call is a potential area for future commits/refinements)
                DeleteRelatedRentals(carModel); // Commit 2: Implement rental deletion logic
                DeleteRelatedFleetCars(carModel); // Commit 3: Implement fleet deletion logic
            }
            
            // Deletes this car model:
            DB.CarModels.Remove(carModel);
            DB.SaveChanges(); // Commit 4: Final save (core delete)
        }

        // --- Utility Operations ---

        /// <summary>
        /// Checks if a car model exists based on its key attributes.
        /// </summary>
        /// <param name="carModel">The car model to check.</param>
        /// <returns>True or False, if there's already a car model.</returns>
        public bool IsCarModelExists(CarModel carModel)
        {
            ValidateCarModel(carModel);
            
            return DB.CarModels.Any(m => m.ManufacturerModelID == carModel.ManufacturerModelID &&
                                         m.ProductionYear == carModel.ProductionYear &&
                                         m.ManualGear == carModel.ManualGear);
        }

        /// <summary>
        /// Gets all the car models for a manufacturer.
        /// </summary>
        /// <param name="manufacturerID">The ID of the manufacturer.</param>
        /// <returns>List of all the car models for the manufacturer. Ordered by manufacturer model name, then by production year.</returns>
        public List<CarModel> GetCarModelsForManufacturer(int manufacturerID)
        {
            // Note: This method is already clean, but could be split into 'Filter by Manufacturer' 
            // and 'Order Results' helpers if you wanted to maximize granularity even more.
            return DB.CarModels.Where(c => c.ManufacturerModel.Manufacturer.ManufacturerID == manufacturerID)
                               .OrderBy(c => c.ManufacturerModel.Manufacturer.ManufacturerName)
                               .ThenBy(c => c.ManufacturerModel.ManufacturerModelName)
                               .ThenBy(c => c.ProductionYear)
                               .ToList();
        }
    }
}
