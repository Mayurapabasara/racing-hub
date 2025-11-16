using System;
using System.Collections.Generic;
using System.Linq;

namespace RacingHubCarRental.Services
{
    /// <summary>
    /// Dedicated service for retrieving and querying role and user data. 
    /// This isolates database read concerns from write logic.
    /// </summary>
    public class RolesQueryService : BaseLogic
    {
        // --- Core Lookup Methods (for reuse in Logic class) ---

        /// <summary>
        /// Finds a Role by name.
        /// </summary>
        public Role FindRole(string roleName)
        {
            return DB.Roles.FirstOrDefault(r => r.RoleName == roleName);
        }
        
        /// <summary>
        /// Finds a User by username.
        /// </summary>
        public User FindUser(string username)
        {
            return DB.Users.FirstOrDefault(u => u.Username == username);
        }

        // --- Retrieval/Query Methods ---

        public string[] GetAllRoleNames()
        {
            return DB.Roles.Select(r => r.RoleName).ToArray();
        }

        public List<Role> GetAllRoles()
        {
            return DB.Roles.ToList();
        }

        public bool RoleExists(string roleName)
        {
            return DB.Roles.Any(r => r.RoleName == roleName);
        }
        
        public bool IsUserInRole(string username, string roleName)
        {
            // Highly optimized query for existence check
            return DB.Users
                     .Any(u => u.Username == username && 
                                u.Roles.Any(r => r.RoleName == roleName));
        }

        public string[] GetRolesForUser(string username)
        {
            var user = FindUser(username);
            if (user == null) throw new KeyNotFoundException($"User '{username}' not found.");
            
            // Explicit loading ensures we get the associated roles
            DB.Entry(user).Collection(u => u.Roles).Load();

            return user.Roles.Select(r => r.RoleName).ToArray();
        }

        public string[] GetUsersInRole(string roleName)
        {
            var role = FindRole(roleName);
            if (role == null) throw new KeyNotFoundException($"Role '{roleName}' not found.");

            // Explicit loading of associated users
            DB.Entry(role).Collection(r => r.Users).Load();

            return role.Users.Select(u => u.Username).ToArray();
        }

        public string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            return DB.Users
                     .Where(u => u.Username.Contains(usernameToMatch) && 
                                 u.Roles.Any(r => r.RoleName == roleName))
                     .Select(u => u.Username)
                     .ToArray();
        }
    }
}
