using System;
using System.ComponentModel.DataAnnotations;

namespace RacingHubCarRental
{
    /// <summary>
    /// Custom logic for CarModel (safe from auto-generation).
    /// </summary>
    public partial class CarModel
    {
        // ------------------------
        //  VALIDATION ATTRIBUTES
        // ------------------------

        [Range(1900, 2100, ErrorMessage = "Production year must be between 1900 and 2100.")]
        public int ValidatedProductionYear => ProductionYear;

        [Range(0, 100000, ErrorMessage = "Daily price must be a positive value.")]
        public decimal ValidatedDailyPrice => DailyPrice;

        [Range(0, 100000, ErrorMessage = "Delay price must be a positive value.")]
        public decimal ValidatedDayDelayPrice => DayDelayPrice;

        // ------------------------
        //  CUSTOM READ-ONLY VALUES
        // ------------------------

        /// <summary>
        /// Returns a formatted name for displaying the model.
        /// </summary>
        public string FullName
        {
            get
            {
                if (ManufacturerModel == null)
                    return $"{ProductionYear} Model"; 

                return $"{ManufacturerModel.ModelName} - {ProductionYear}";
            }
        }

        /// <summary>
        /// True if this model is older than 10 years.
        /// </summary>
        public bool IsOldModel
        {
            get { return ProductionYear < (DateTime.Now.Year - 10); }
        }

        /// <summary>
        /// Returns the formatted daily rental price (e.g., $120.00).
        /// </summary>
        public string FormattedDailyPrice
        {
            get { return DailyPrice.ToString("C"); }
        }

        /// <summary>
        /// Returns the formatted delay price per day.
        /// </summary>
        public string FormattedDelayPrice
        {
            get { return DayDelayPrice.ToString("C"); }
        }

        /// <summary>
        /// Gear description for UI display.
        /// </summary>
        public string GearType
        {

