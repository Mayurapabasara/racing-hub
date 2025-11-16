using System.Collections.Generic;
using RacingHubCarRental; // Ensure 'Role' and other models are available

namespace RacingHubCarRental.Interfaces
{
    /// <summary>
    /// Defines the contract for all role and user-role management operations.
    /// This separation ensures flexibility and future maintenance stability.
    /// </summary>
    public interface IRolesLogic
    {
        // Role Management (Write Operations)
        void CreateRole(string roleName);
        void UpdateRole(int roleID, string newRoleName);
        bool DeleteRole(string roleName, bool throwOnPopulatedRole);

        // User-Role Assignment (Write Operations)
        void AddUserToRole(string username, string roleName);
        void AddUsersToRoles(string[] usernames, string[] roleNames);
        void RemoveUsersFromRoles(string[] usernames, string[] roleNames);
        void UpdateRoleToUser(string username, string roleName);

        // Retrieval/Query Operations (Delegated to service, but included in contract)
        string[] GetAllRoleNames();
        string[] GetRolesForUser(string username);
        string[] GetUsersInRole(string roleName);
        string[] FindUsersInRole(string roleName, string usernameToMatch);
        bool IsUserInRole(string username, string roleName);
        bool RoleExists(string roleName);
        List<Role> GetAllRoles();
    }
}
