using System;
using System.Collections.Generic;
using System.Linq;
using RacingHubCarRental.Services.Abstractions; // Example namespace for abstraction

namespace RacingHubCarRental.Services
{
    /// <summary>
    /// Service dedicated to retrieving manufacturer data.
    /// </summary>
    public class ManufacturerQueryService : BaseLogic
    {
        /// <summary>
        /// Retrieves all manufacturers, ordered alphabetically by name.
        /// </summary>
        /// <returns>List of all manufacturers.</returns>
        public List<Manufacturer> GetOrderedManufacturers()
        {
            return DB.Manufacturers
                     .OrderBy(m => m.ManufacturerName)
                     .ToList();
        }

        /// <summary>
        /// Retrieves a single manufacturer by its primary key ID.
        /// </summary>
        /// <param name="id">The ID of the manufacturer.</param>
        /// <returns>The Manufacturer object.</returns>
        public Manufacturer GetManufacturerById(int id)
        {
            return DB.Manufacturers.Find(id);
        }

        /// <summary>
        /// Checks if a manufacturer exists by name, ignoring case.
        /// </summary>
        /// <param name="manufacturerName">The manufacturer name to check.</param>
        /// <returns>True if a manufacturer with the given name exists.</returns>
        public bool CheckIfManufacturerExistsByName(string manufacturerName)
        {
            // Future commit: Add .ToLower() or use StringComparer.
            return DB.Manufacturers.Any(m => m.ManufacturerName == manufacturerName);
        }
    }
}
