using System;
using System.ComponentModel.DataAnnotations;

namespace RacingHubCarRental
{
    /// <summary>
    /// Custom logic and safe extension for the CarModel EF class.
    /// This file will NOT be overwritten by EF.
    /// </summary>
    public partial class CarModel
    {
        // -------------------------
        //  VALIDATION PROPERTIES
        // -------------------------

        [Range(1900, 2100, ErrorMessage = "Production year must be between 1900 and 2100.")]
        public int ValidatedProductionYear => ProductionYear;

        [Range(0, 200000, ErrorMessage = "Daily price must be a positive number.")]
        public decimal ValidatedDailyPrice => DailyPrice;

        [Range(0, 200000, ErrorMessage = "Delay price per day must be a positive number.")]
        public decimal ValidatedDayDelayPrice => DayDelayPrice;

        // -------------------------
        //  COMPUTED DISPLAY VALUES
        // -------------------------

        /// <summary>
        /// Display name: Example → "Toyota Corolla - 2020"
        /// </summary>
        public string FullName
        {
            get
            {
                if (ManufacturerModel == null)
                    return $"Model {CarModelID} ({ProductionYear})";

                return $"{ManufacturerModel.ModelName} - {ProductionYear}";
            }
        }

        /// <summary>
        /// Automatic or Manual text representation.
        /// </summary>
        public string GearType => ManualGear ? "Manual" : "Automatic";

        /// <summary>
        /// Format daily rental price with currency.
        /// </summary>
        public string FormattedDailyPrice => DailyPrice.ToString("C");

        /// <summary>
        /// Format delay fee per day.
        /// </summary>
        public string FormattedDelayPrice => DayDelayPrice.ToString("C");

        /// <summary>
        /// True if the model is older than 10 years.
        /// </summary>
        public bool IsOldModel => ProductionYear < (DateTime.Now.Year - 10);

        /// <summary>
        /// Estimated depreciation (5% per year).
        /// </summary>
        public decimal EstimatedDepreciation
        {
            get
            {
                int age = DateTime.Now.Year - ProductionYear;
                if (age < 0) age = 0;

                decimal depreciationRate = 0.05m;  // 5% per year
                return DailyPrice * depreciationRate * age;
            }
        }
    }
}

