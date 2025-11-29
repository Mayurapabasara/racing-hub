using System;
using System.Collections.Generic;

namespace RacingHubCarRental
{
    /// <summary>
    /// Domain entity representing a car model used within the fleet.
    /// This version replaces the auto-generated template with a clean,
    /// well-structured class suitable for long-term maintenance and contributions.
    /// </summary>
    public class CarModel
    {
        /// <summary>
        /// Creates a new instance of <see cref="CarModel"/> with initialized collections.
        /// </summary>
        public CarModel()
        {
            FleetCars = new HashSet<FleetCar>();
            CreatedAt = DateTime.UtcNow;
        }

        // ---------------------------------------------------------------------
        // Primary Keys & References
        // ---------------------------------------------------------------------

        /// <summary>
        /// Unique identifier for the car model.
        /// </summary>
        public int CarModelId { get; set; }

        /// <summary>
        /// Foreign key to the specific manufacturer model.
        /// </summary>
        public int ManufacturerModelId { get; set; }

        /// <summary>
        /// Navigation reference to the parent manufacturer model.
        /// </summary>
        public virtual ManufacturerModel? ManufacturerModel { get; set; }

        // ---------------------------------------------------------------------
        // Model Specifications
        // ---------------------------------------------------------------------

        /// <summary>
        /// The production year of this model (e.g., 2018, 2020).
        /// </summary>
        public int ProductionYear { get; set; }

        /// <summary>
        /// Indicates whether this model uses manual transmission.
        /// </summary>
        public bool IsManualGear { get; set; }

        // ---------------------------------------------------------------------
        // Pricing & Billing
        // ---------------------------------------------------------------------

        /// <summary>
        /// Standard daily rental price for this model.
        /// </summary>
        public decimal DailyPrice { get; set; }

        /// <summary>
        /// Penalty price applied per delayed day during late returns.
        /// </summary>
        public decimal DayDelayPrice { get; set; }

        // ---------------------------------------------------------------------
        // Metadata
        // ---------------------------------------------------------------------

        /// <summary>
        /// Timestamp for when the model was added to the system.
        /// Useful for audit and sorting operations.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        // ---------------------------------------------------------------------
        // Relationships
        // ---------------------------------------------------------------------

        /// <summary>
        /// Fleet vehicle instances that belong to this model.
        /// </summary>
        public virtual ICollection<FleetCar> FleetCars { get; set; }

        // ---------------------------------------------------------------------
        // Helper Methods
        // ---------------------------------------------------------------------

        /// <summary>
        /// Calculates the total potential late-return charge for a given number of days.
        /// </summary>
        /// <param name="days">Number of late days.</param>
        /// <returns>Total late fee based on <see cref="DayDelayPrice"/>.</returns>
        public decimal CalculateLateFee(int days)
        {
            if (days < 0)
                throw new ArgumentOutOfRangeException(nameof(days), "Days cannot be negative.");

            return days * DayDelayPrice;
        }
    }
}

