using System;
using System.ComponentModel.DataAnnotations;

namespace RacingHubCarRental
{
    /// <summary>
    /// Represents a contact or inquiry submitted by a user.
    /// </summary>
    public class Contact
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Contact"/> class.
        /// Sets default values for IsRead and DateTime.
        /// </summary>
        public Contact()
        {
            IsRead = false;              // Default to unread
            DateTime = DateTime.Now;     // Default to current time
        }

        /// <summary>
        /// Gets or sets the unique identifier for the contact.
        /// </summary>
        [Key]
        public int ContactID { get; set; }

        /// <summary>
        /// Gets or sets the first name of the contact.
        /// </summary>
        [Required(ErrorMessage = "First name is required.")]
        [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name of the contact.
        /// </summary>
        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters.")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the email address of the contact.
        /// </summary>
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [StringLength(150)]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the phone number of the contact.
        /// </summary>
        [Phone(ErrorMessage = "Invalid phone number.")]
        [StringLength(20)]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; }

        /// <summary>
        /// Gets or sets the message content from the contact.
        /// </summary>
        [Required(ErrorMessage = "Message cannot be empty.")]
        [StringLength(1000, ErrorMessage = "Message cannot exceed 1000 characters.")]
        [Display(Name = "Message")]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the contact was submitted.
        /// </summary>
        [Required]
        [Display(Name = "Submitted On")]
        public DateTime DateTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the message has been read.
        /// </summary>
        [Display(Name = "Read Status")]
        public bool IsRead { get; set; }

        /// <summary>
        /// Gets the full name of the contact.
        /// </summary>
        public string FullName => $"{FirstName} {LastName}";
    }
}

