using System.ComponentModel.DataAnnotations;

namespace RacingHubCarRental
{
    [MetadataType(typeof(ManufacturerMetadata))]
    public partial class Manufacturer
    {
        public class ManufacturerMetadata
        {
            [Display(Name = "Manufacturer Name")]
            [Required(ErrorMessage = "Manufacturer Name is required.")]
            [StringLength(50, ErrorMessage = "Maximum length is 50 characters.")]
            public string ManufacturerName { get; set; }
        }
    }
}

