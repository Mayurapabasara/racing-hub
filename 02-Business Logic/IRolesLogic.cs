using System.Collections.Generic;
using System.Threading.Tasks;
using RacingHubCarRental.Models;

namespace RacingHubCarRental.Interfaces
{
    /// <summary>
    /// Defines the contract for all role-management operations including:
    /// - Role CRUD
    /// - User-role assignments
    /// - Role queries with async support
    /// - High-level abstraction for testability and clean architecture
    /// </summary>
    public interface IRolesService
    {
        // ============================================================
        // ROLE MANAGEMENT
        // ============================================================

        /// <summary>
        /// Creates a new system role (e.g., Admin, Manager, Customer).
        /// </summary>
        Task CreateAsync(string roleName);

        /// <summary>
        /// Updates an existing role's name.
        /// </summary>
        Task UpdateAsync(int roleId, string newRoleName);

        /// <summary>
        /// Deletes a role. Throws/blocks if users still depend on it.
        /// </summary>
        Task<bool> DeleteAsync(string roleName, bool preventDeleteIfAssigned);

        // ============================================================
        // USER → ROLE ASSIGNMENTS
        // ============================================================

        /// <summary>
        /// Assigns a role to a user.
        /// </summary>
        Task AssignRoleAsync(string username, string roleName);

        /// <summary>
        /// Assigns multiple roles to multiple users.
        /// </summary>
        Task AssignRolesAsync(IEnumerable<string> usernames, IEnumerable<string> roleNames);

        /// <summary>
        /// Removes users from one or more roles.
        /// </summary>
        Task RemoveUsersFromRolesAsync(IEnumerable<string> usernames, IEnumerable<string> roleNames);

        /// <summary>
        /// Replaces the existing role of a user with a new one.
        /// </summary>
        Task UpdateUserRoleAsync(string username, string newRoleName);

        // ============================================================
        // ROLE & MEMBERSHIP QUERIES
        // ============================================================

        /// <summary>
        /// Returns a list of all available roles in the system.
        /// </summary>
        Task<IReadOnlyList<string>> GetRoleNamesAsync();

        /// <summary>
        /// Gets all roles assigned to a given user.
        /// </summary>
        Task<IReadOnlyList<string>> GetUserRolesAsync(string username);

        /// <summary>
        /// Returns all users that belong to the specified role.
        /// </summary>
        Task<IReadOnlyList<string>> GetUsersByRoleAsync(string roleName);

        /// <summary>
        /// Performs a partial matching search inside a role.
        /// </summary>
        Task<IReadOnlyList<string>> SearchUsersInRoleAsync(string roleName, string pattern);

        /// <summary>
        /// Checks if a user belongs to a role.
        /// </summary>
        Task<bool> IsUserInRoleAsync(string username, string roleName);
        Task<bool> IsUserInRole(string username, string roleName);

        /// <summary>
        /// Checks whether a role exists.
        /// </summary>
        Task<bool> ExistsAsync(string roleName);

        /// <summary>
        /// Returns full role entities (for admin dashboards or analytics).
        /// </summary>
        Task<IReadOnlyList<Role>> GetAllAsync();
    }
}

