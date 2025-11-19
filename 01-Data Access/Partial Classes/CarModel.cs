using System;
using System.ComponentModel.DataAnnotations;

namespace RacingHubCarRental
{
    [MetadataType(typeof(CarModelMetadata))]
    public partial class CarModel
    {
        public class CarModelMetadata
        {
            [Display(Name = "Manufacturer Model")]
            [Required(ErrorMessage = "Manufacturer Model is required.")]
            public int ManufacturerModelID { get; set; }

            [Display(Name = "Production Year")]
            [Required(ErrorMessage = "Production Year is required.")]
            [Range(1900, 3000, ErrorMessage = "Must be between 1900 - 3000.")]
            public int ProductionYear { get; set; }

            [Display(Name = "Gear")]
            [Required(ErrorMessage = "Gear is required.")]
            public bool ManualGear { get; set; }

            [Display(Name = "Daily Price")]
            [Required(ErrorMessage = "Daily Price is required.")]
            [DataType(DataType.Currency)]
            public decimal DailyPrice { get; set; }

            [Display(Name = "Day Delay Price")]
            [Required(ErrorMessage = "Day Delay Price is required.")]
            [DataType(DataType.Currency)]
            public decimal DayDelayPrice { get; set; }
        }
    }
}

