using System;
using System.ComponentModel.DataAnnotations;
using RacingHubCarRental.Validations;

namespace RacingHubCarRental
{
    [MetadataType(typeof(UserMetadata))]
    public partial class User
    {
        public class UserMetadata
        {
            [Display(Name = "Username *")]
            [StringLength(50, ErrorMessage = "Maximum length is 50 characters.")]
            [Required(ErrorMessage = "Username is required.")]
            [RegularExpression(@"^.{4,}$", ErrorMessage = "Minimum length is 4 characters.")]
            public string Username { get; set; }

            [Display(Name = "Password *")]
            [StringLength(50, ErrorMessage = "Maximum length is 50 characters.")]
            [Required(ErrorMessage = "Password is required.")]
            [RegularExpression(@"^.{4,}$", ErrorMessage = "Minimum length is 4 characters.")]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "First Name *")]
            [StringLength(50, ErrorMessage = "Maximum length is 50 characters.")]
            [Required(ErrorMessage = "First Name is required.")]
            public string FirstName { get; set; }

            [Display(Name = "Last Name *")]
            [StringLength(50, ErrorMessage = "Maximum length is 50 characters.")]
            [Required(ErrorMessage = "Last Name is required.")]
            public string LastName { get; set; }

            [Display(Name = "Identity Number *")]
            [Required(ErrorMessage = "Identity Number is required.")]
            [Range(0, int.MaxValue, ErrorMessage = "Invalid Identity Number.")]
            [IdentityNumberValidation(ErrorMessage = "Invalid Identity Number.")]
            public string IdentityNumber { get; set; }

            [Display(Name = "Email")]
            [StringLength(50, ErrorMessage = "Maximum length is 50 characters.")]
            [DataType(DataType.EmailAddress)]
            [EmailAddress(ErrorMessage = "Invalid Email Address.")]
            public string Email { get; set; }

            [Display(Name = "Date of Birth")]
            [DataType(DataType.Date)]
            [BirthDateValidation(18, 150, ErrorMessage = "You must be over 18 to register.")]
            public DateTime? BirthDate { get; set; }
        }
    }
}

