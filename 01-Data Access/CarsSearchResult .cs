using System;

namespace RacingHubCarRental
{
    /// <summary>
    /// Represents the result of a car search query, including car details,
    /// manufacturer information, pricing, and an optional image path.
    /// </summary>
    public class CarsSearch_Result
    {
        /// <summary>
        /// Gets or sets the car's unique license plate number.
        /// </summary>
        public string LicensePlate { get; set; }

        /// <summary>
        /// Gets or sets the name of the car manufacturer.
        /// </summary>
        public string ManufacturerName { get; set; }

        /// <summary>
        /// Gets or sets the specific model name from the manufacturer.
        /// </summary>
        public string ManufacturerModelName { get; set; }

        /// <summary>
        /// Gets or sets the production year of the car.
        /// </summary>
        public int ProductionYear { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the car uses a manual gearbox.
        /// </summary>
        public bool ManualGear { get; set; }

        /// <summary>
        /// Gets or sets the daily rental price of the car.
        /// </summary>
        public decimal DailyPrice { get; set; }

        /// <summary>
        /// Gets or sets the penalty price per day for late returns.
        /// </summary>
        public decimal DayDelayPrice { get; set; }

        /// <summary>
        /// Gets or sets the image path or URL of the car.
        /// </summary>
        public string CarImage { get; set; }
    }
}

