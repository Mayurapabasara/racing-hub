using System;

namespace RacingHubCarRental
{
    /// <summary>
    /// ViewModel used for creating or editing a car model.
    /// Holds dropdown UI data and the CarModel entity.
    /// Designed to be immutable after construction.
    /// </summary>
    public sealed class CarModelViewModel
    {
        /// <summary>
        /// Dropdown list data for manufacturers and their models.
        /// </summary>
        public ManufacturerModelDDLs ManufacturerDropdowns { get; }

        /// <summary>
        /// Dropdown list data for gear types.
        /// </summary>
        public GearDDL GearDropdowns { get; }

        /// <summary>
        /// Car model entity being created or edited.
        /// </summary>
        public CarModel CarModel { get; }

        /// <summary>
        /// Indicates whether this ViewModel represents an existing car model (edit mode)
        /// or a new one (create mode).
        /// </summary>
        public bool IsEditMode => CarModel is not null && CarModel.Id > 0;

        /// <summary>
        /// Default constructor initializes empty dropdowns and a new domain model.
        /// Useful for view initialization.
        /// </summary>
        public CarModelViewModel()
            : this(new CarModel(), new ManufacturerModelDDLs(), new GearDDL())
        {
        }

        /// <summary>
        /// Main constructor used for dependency injection and testing.
        /// All dependencies must be supplied and are validated.
        /// </summary>
        public CarModelViewModel(
            CarModel carModel,
            ManufacturerModelDDLs manufacturerDropdowns,
            GearDDL gearDropdowns)
        {
            CarModel = carModel ?? throw new ArgumentNullException(nameof(carModel));
            ManufacturerDropdowns = manufacturerDropdowns ??
                                    throw new ArgumentNullException(nameof(manufacturerDropdowns));
            GearDropdowns = gearDropdowns ??
                            throw new ArgumentNullException(nameof(gearDropdowns));
        }
    }
}

