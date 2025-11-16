using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity; // Added for EntityState
using System.Linq;
using RacingHubCarRental.Interfaces; // Using the new interface
using RacingHubCarRental.Services; // Using the new Query Service

namespace RacingHubCarRental
{
    /// <summary>
    /// Represents the core business logic for the CRUD of the manufacturers, focusing on write operations.
    /// Implements IManufacturersLogic.
    /// </summary>
    public class ManufacturersLogic : BaseLogic, IManufacturersLogic
    {
        // --- Private Validation Helpers (Granular commits here) ---

        /// <summary>
        /// Ensures the manufacturer object is not null.
        /// </summary>
        private void ValidateManufacturer(Manufacturer manufacturer)
        {
            if (manufacturer == null)
            {
                throw new ArgumentNullException(nameof(manufacturer), "Manufacturer object cannot be null.");
            }
        }

        /// <summary>
        /// Validates the manufacturer name string for null or white space.
        /// </summary>
        private void ValidateManufacturerName(string manufacturerName)
        {
            if (string.IsNullOrWhiteSpace(manufacturerName))
            {
                throw new ArgumentException("Manufacturer name cannot be empty or whitespace.", nameof(manufacturerName));
            }
        }

        // --- Core CRUD Operations ---
        
        // NOTE: We keep the read methods implemented to satisfy the IManufacturersLogic interface,
        // but we'd delegate the heavy lifting to the Query Service in a real app.
        private readonly ManufacturerQueryService _queryService = new ManufacturerQueryService();

        public List<Manufacturer> GetAllManufacturers() => _queryService.GetOrderedManufacturers();
        public Manufacturer GetManufacturerByID(int id) => _queryService.GetManufacturerById(id);
        public bool IsManufacturerExists(string manufacturerName) => _queryService.CheckIfManufacturerExistsByName(manufacturerName);


        /// <summary>
        /// Inserts a new manufacturer.
        /// </summary>
        /// <param name="manufacturer">The manufacturer to insert.</param>
        public void InsertManufacturer(Manufacturer manufacturer)
        {
            ValidateManufacturer(manufacturer); // Commit 1: Add object validation
            ValidateManufacturerName(manufacturer.ManufacturerName); // Commit 2: Add name validation

            DB.Manufacturers.Add(manufacturer);
            DB.SaveChanges(); // Commit 3: Save changes
        }

        /// <summary>
        /// Updates a manufacturer.
        /// </summary>
        /// <param name="manufacturer">The manufacturer to update.</param>
        public void UpdateManufacturer(Manufacturer manufacturer)
        {
            ValidateManufacturer(manufacturer); // Commit 1: Add validation
            
            DB.Entry(manufacturer).State = EntityState.Modified;
            DB.SaveChanges(); // Commit 2: Save changes
        }

        // --- Deletion Helpers (Maximum Granularity for Collective Delete) ---

        /// <summary>
        /// Step 1 of collective delete: Deletes all Rentals related to this manufacturer.
        /// </summary>
        private void DeleteRelatedRentals(int manufacturerID)
        {
            List<Rental> orders = DB.Rentals
                .Where(r => r.FleetCar.CarModel.ManufacturerModel.Manufacturer.ManufacturerID == manufacturerID)
                .ToList();

            if (orders.Any())
            {
                DB.Rentals.RemoveRange(orders);
                DB.SaveChanges(); // Commit opportunity: "Delete logic for Rentals."
            }
        }

        /// <summary>
        /// Step 2 of collective delete: Deletes all Fleet Cars related to this manufacturer.
        /// </summary>
        private void DeleteRelatedFleetCars(int manufacturerID)
        {
            List<FleetCar> fleet = DB.FleetCars
                .Where(f => f.CarModel.ManufacturerModel.Manufacturer.ManufacturerID == manufacturerID)
                .ToList();
            
            if (fleet.Any())
            {
                DB.FleetCars.RemoveRange(fleet);
                DB.SaveChanges(); // Commit opportunity: "Delete logic for FleetCars."
            }
        }

        /// <summary>
        /// Step 3 of collective delete: Deletes all Car Models related to this manufacturer.
        /// </summary>
        private void DeleteRelatedCarModels(int manufacturerID)
        {
            List<CarModel> carModels = DB.CarModels
                .Where(c => c.ManufacturerModel.Manufacturer.ManufacturerID == manufacturerID)
                .ToList();

            if (carModels.Any())
            {
                DB.CarModels.RemoveRange(carModels);
                DB.SaveChanges(); // Commit opportunity: "Delete logic for CarModels."
            }
        }

        /// <summary>
        /// Step 4 of collective delete: Deletes all Manufacturer Models (sub-models) related to this manufacturer.
        /// </summary>
        private void DeleteRelatedManufacturerModels(int manufacturerID)
        {
            List<ManufacturerModel> mfrModels = DB.ManufacturerModels
                .Where(m => m.Manufacturer.ManufacturerID == manufacturerID)
                .ToList();
            
            if (mfrModels.Any())
            {
                DB.ManufacturerModels.RemoveRange(mfrModels);
                DB.SaveChanges(); // Commit opportunity: "Delete logic for ManufacturerModels."
            }
        }


        /// <summary>
        /// Deletes a manufacturer! Granularly handles collective deletion cascade.
        /// </summary>
        /// <param name="manufacturer">The manufacturer to delete.</param>
        /// <param name="isCollective">Indicator if to perform a collective delete, if to delete all it's related data.</param>
        public void DeleteManufacturer(Manufacturer manufacturer, bool isCollective = false)
        {
            ValidateManufacturer(manufacturer); // Commit 1: Validation

            // Checks if to perform a collective delete, using granular helpers:
            if (isCollective)
            {
                int id = manufacturer.ManufacturerID;
                
                // Each call is a separate, testable step.
                DeleteRelatedRentals(id);          // Commit 2
                DeleteRelatedFleetCars(id);        // Commit 3
                DeleteRelatedCarModels(id);        // Commit 4
                DeleteRelatedManufacturerModels(id); // Commit 5
            }
            
            // Final deletion of the parent entity:
            DB.Manufacturers.Remove(manufacturer);
            DB.SaveChanges(); // Commit 6: Final save
        }
    }
}
