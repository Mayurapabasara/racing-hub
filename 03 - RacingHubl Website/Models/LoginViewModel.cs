using System;
using System.ComponentModel.DataAnnotations;

namespace RacingHubCarRental
{
    /// <summary>
    /// ViewModel used for user authentication during login.
    /// </summary>
    public class LoginViewModel
    {
        /// <summary>
        /// The username of the user attempting to log in.
        /// </summary>
        [Required(ErrorMessage = "Please enter your username.")]
        [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters.")]
        [MinLength(4, ErrorMessage = "Username must contain at least 4 characters.")]
        [Display(Name = "Username")]
        public string Username { get; set; }

        /// <summary>
        /// The password entered by the user.
        /// </summary>
        [Required(ErrorMessage = "Please enter your password.")]
        [StringLength(50, ErrorMessage = "Password cannot exceed 50 characters.")]
        [MinLength(4, ErrorMessage = "Password must contain at least 4 characters.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        /// <summary>
        /// Indicates whether the system should keep the user logged in.
        /// </summary>
        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
    }
}

