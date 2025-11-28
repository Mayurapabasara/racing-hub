using System.Collections.Generic;
using System.Threading.Tasks;
using RacingHubCarRental.Models; // Updated path for Role/User models

namespace RacingHubCarRental.Interfaces
{
    /// <summary>
    /// Contract defining all operations required for managing roles and user-role assignments.
    /// Refactored for async support, better naming, SOLID compliance, and testability.
    /// </summary>
    public interface IRolesLogic
    {
        // ============================================================
        // ROLE MANAGEMENT (CRUD)
        // ============================================================

        /// <summary>
        /// Creates a new application role.
        /// </summary>
        Task CreateRoleAsync(string roleName);

        /// <summary>
        /// Updates the name of an existing role.
        /// </summary>
        Task UpdateRoleAsync(int roleId, string newRoleName);

        /// <summary>
        /// Deletes a role. 
        /// </summary>
        Task<bool> DeleteRoleAsync(string roleName, bool blockIfUsersAssigned);

        // ============================================================
        // USER → ROLE ASSIGNMENT
        // ============================================================

        /// <summary>
        /// Assigns a user to a specific role.
        /// </summary>
        Task AddUserToRoleAsync(string username, string roleName);

        /// <summary>
        /// Assigns multiple users to multiple roles.
        /// </summary>
        Task AddUsersToRolesAsync(IEnumerable<string> usernames, IEnumerable<string> roleNames);

        /// <summary>
        /// Removes users from roles.
        /// </summary>
        Task RemoveUsersFromRolesAsync(IEnumerable<string> usernames, IEnumerable<string> roleNames);

        /// <summary>
        /// Updates (replaces) the role of a specific user.
        /// </summary>
        Task UpdateUserRoleAsync(string username, string newRoleName);

        // ============================================================
        // READ / QUERY OPERATIONS
        // ============================================================

        /// <summary>
        /// Gets the list of all defined role names.
        /// </summary>
        Task<IEnumerable<string>> GetAllRoleNamesAsync();

        /// <summary>
        /// Gets all roles assigned to a user.
        /// </summary>
        Task<IEnumerable<string>> GetRolesForUserAsync(string username);

        /// <summary>
        /// Gets the usernames of all users within a role.
        /// </summary>
        Task<IEnumerable<string>> GetUsersInRoleAsync(string roleName);

        /// <summary>
        /// Searches inside a role by username pattern.
        /// </summary>
        Task<IEnumerable<string>> FindUsersInRoleAsync(string roleName, string usernamePattern);

        /// <summary>
        /// Checks if user belongs to a role.
        /// </summary>
        Task<bool> IsUserInRoleAsync(string username, string roleName);

        /// <summary>
        /// Checks if a role exists.
        /// </summary>
        Task<bool> RoleExistsAsync(string roleName);

        /// <summary>
        /// Returns the complete list of role entities.
        /// </summary>
        Task<IEnumerable<Role>> GetAllRolesAsync();
    }
}

