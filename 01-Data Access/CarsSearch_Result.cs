using System;
using System.Globalization;

namespace RacingHubCarRental
{
    /// <summary>
    /// Custom logic for CarsSearch_Result without affecting EF auto-generated code.
    /// </summary>
    public partial class CarsSearch_Result
    {
        /// <summary>
        /// Returns full formatted car name.
        /// Example: "Toyota Corolla 2020"
        /// </summary>
        public string FullCarName
        {
            get
            {
                return $"{ManufacturerName} {ManufacturerModelName} {ProductionYear}";
            }
        }

        /// <summary>
        /// Gear type in readable format.
        /// </summary>
        public string GearType
        {
            get { return ManualGear ? "Manual" : "Automatic"; }
        }

        /// <summary>
        /// Formats the daily rental price.
        /// Example: "$120.00"
        /// </summary>
        public string FormattedDailyPrice
        {
            get { return DailyPrice.ToString("C", CultureInfo.CurrentCulture); }
        }

        /// <summary>
        /// Formats delay price per day.
        /// </summary>
        public string FormattedDelayPrice
        {
            get { return DayDelayPrice.ToString("C", CultureInfo.CurrentCulture); }
        }

        /// <summary>
        /// Returns a safe image URL or a placeholder image if null/empty.
        /// </summary>
        public string ImageUrl
        {
            get
            {
                if (string.IsNullOrWhiteSpace(CarImage))
                    return "/Content/Images/no-image.png";  // fallback

                return CarImage.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                    ? CarImage
                    : $"/Uploads/Cars/{CarImage}";
            }
        }

        /// <summary>
        /// Returns true if the car is older than 10 years.
        /// </summary>
        public bool IsOldModel
        {
            get
            {
                int currentYear = DateTime.Now.Year;
                return ProductionYear < (currentYear - 10);
            }
        }
    }
}

