// DataModel.cs (Single File Consolidation)

using System;
using System.Collections.Generic;

namespace RacingHubCarRental
{
    // 1. User Class (Original Content)
    public partial class User
    {
        public User()
        {
            // Initialize collections for navigation properties
            this.Rentals = new HashSet<Rental>();
            this.Roles = new HashSet<Role>();
        }
    
        // Primary Properties
        public int UserID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int IdentityNumber { get; set; }
        public string Email { get; set; }
        public Nullable<System.DateTime> BirthDate { get; set; }
    
        // Navigation Properties (Virtual for lazy loading in Entity Framework)
        public virtual ICollection<Rental> Rentals { get; set; }
        public virtual ICollection<Role> Roles { get; set; }
    }

    // 2. Rental Class (Conceptual Definition based on User's dependency)
    // Represents a car rental transaction.
    public partial class Rental
    {
        public Rental()
        {
            // Constructor, if needed (e.g., to initialize nested collections)
        }

        // Primary Properties
        public int RentalID { get; set; }
        public int UserID { get; set; } // Foreign Key to User
        public int CarID { get; set; } // Foreign Key to a conceptual Car table
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalCost { get; set; }

        // Navigation Properties
        public virtual User User { get; set; }
        // public virtual Car Car { get; set; } // Assuming a Car class exists
    }

    // 3. Role Class (Conceptual Definition based on User's dependency)
    // Represents a user's role (e.g., Admin, Customer, Manager).
    public partial class Role
    {
        public Role()
        {
            this.Users = new HashSet<User>();
        }

        // Primary Properties
        public int RoleID { get; set; }
        public string RoleName { get; set; }
        public string Description { get; set; }

        // Navigation Property (Many-to-Many relationship with User)
        public virtual ICollection<User> Users { get; set; }
    }
}
