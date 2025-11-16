// This interface defines the contract for car model operations.
// Implementing an interface often necessitates a separate file and additional
// commits, helping to maximize contributions over the project lifecycle.
using System.Collections.Generic;

namespace RacingHubCarRental
{
    public interface ICarModelsLogic
    {
        // CRUD Operations
        List<CarModel> GetAllCarModels();
        CarModel GetCarModelByID(int id);
        void InsertCarModel(CarModel carModel);
        void UpdateCarModel(CarModel carModel);
        void DeleteCarModel(CarModel carModel, bool isCollective = false);

        // Utility Operations
        bool IsCarModelExists(CarModel carModel);
        List<CarModel> GetCarModelsForManufacturer(int manufacturerID);
    }
}
