using System.Collections.Generic;
using RacingHubCarRental.DataModels; // Assuming data models are here

namespace RacingHubCarRental.Interfaces
{
    /// <summary>
    /// Defines the contract for all manufacturer business logic operations.
    /// </summary>
    public interface IManufacturersLogic
    {
        // CRUD Operations
        void InsertManufacturer(Manufacturer manufacturer);
        void UpdateManufacturer(Manufacturer manufacturer);
        void DeleteManufacturer(Manufacturer manufacturer, bool isCollective = false);
        
        // Retrieval Operations (Often moved to a Query Service, but defined here for contract)
        List<Manufacturer> GetAllManufacturers();
        Manufacturer GetManufacturerByID(int id);
        bool IsManufacturerExists(string manufacturerName);
    }
}
