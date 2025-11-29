using System;
using System.ComponentModel.DataAnnotations;

namespace RacingHubCarRental
{
    [MetadataType(typeof(ContactMetadata))]
    public partial class Contact
    {
        public class ContactMetadata
        {
            [Display(Name = "First Name *")]
            [StringLength(50, ErrorMessage = "Maximum length is 50 characters.")]
            [Required(ErrorMessage = "First Name is required.")]
            public string FirstName { get; set; }

            [Display(Name = "Last Name *")]
            [StringLength(50, ErrorMessage = "Maximum length is 50 characters.")]
            [Required(ErrorMessage = "Last Name is required.")]
            public string LastName { get; set; }

            [Display(Name = "Email *")]
            [StringLength(50, ErrorMessage = "Maximum length is 50 characters.")]
            [Required(ErrorMessage = "Email is required.")]
            [EmailAddress(ErrorMessage = "Invalid Email Address.")]
            public string Email { get; set; }

            [Display(Name = "Phone Number")]
            [DataType(DataType.PhoneNumber)]
            [StringLength(24, ErrorMessage = "Maximum length is 24 characters.")]
            [RegularExpression(@"^\+?[0-9\s\-\(\)]+$", ErrorMessage = "Invalid Phone Number.")]
            public string Phone { get; set; }

            [Display(Name = "Text *")]
            [Required(ErrorMessage = "Text is required.")]
            public string Text { get; set; }
        }
    }
}

