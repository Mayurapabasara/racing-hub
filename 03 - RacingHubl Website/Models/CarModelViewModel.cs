using System;

namespace RacingHubCarRental
{
    /// <summary>
    /// ViewModel used for creating or editing car models.
    /// Contains UI dropdown data and the CarModel entity.
    /// Completely rewritten following SRP, immutability, and clarity principles.
    /// </summary>
    public class CarModelViewModel
    {
        /// <summary>
        /// Gets dropdown values for Manufacturers and Manufacturer Models.
        /// </summary>
        public ManufacturerModelDDLs ManufacturerModelDropdowns { get; }

        /// <summary>
        /// Gets dropdown values for Gear Types.
        /// </summary>
        public GearDDL GearDropdown { get; }

        /// <summary>
        /// Gets or sets the CarModel domain object.
        /// </summary>
        public CarModel CarModel { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CarModelViewModel"/> class.
        /// Safely initializes all dropdowns and ensures CarModel is never null.
        /// </summary>
        public CarModelViewModel()
            : this(new CarModel(), new ManufacturerModelDDLs(), new GearDDL())
        {
        }

        /// <summary>
        /// Initializes a new instance with provided domain object and dropdowns.
        /// Supports dependency injection and unit testing.
        /// </summary>
        public CarModelViewModel(
            CarModel carModel,
            ManufacturerModelDDLs manufacturerModelDDLs,
            GearDDL gearDDL)
        {
            CarModel = carModel ?? throw new ArgumentNullException(nameof(carModel));

            ManufacturerModelDropdowns = manufacturerModelDDLs
                ?? throw new ArgumentNullException(nameof(manufacturerModelDDLs));

            GearDropdown = gearDDL
                ?? throw new ArgumentNullException(nameof(gearDDL));
        }
    }
}

