using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using RacingHubCarRental.Interfaces;
using RacingHubCarRental.Services;

namespace RacingHubCarRental
{
    /// <summary>
    /// Represents the core business logic for Role and User-Role assignments.
    /// Focuses on transactional write operations and implements IRolesLogic.
    /// </summary>
    public class RolesLogic : BaseLogic, IRolesLogic
    {
        private readonly RolesQueryService _queryService = new RolesQueryService();
        
        // --- Private Validation and Lookup Helpers (Multiple Commit Points) ---

        /// <summary>
        /// Validates a string input (role name or username) for being null or whitespace.
        /// </summary>
        private void ValidateName(string name, string paramName)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException($"The parameter '{paramName}' cannot be null or empty.", paramName);
            }
        }

        /// <summary>
        /// Finds a Role by name, throwing KeyNotFoundException if not found.
        /// </summary>
        private Role GetRoleOrThrow(string roleName)
        {
            ValidateName(roleName, nameof(roleName));
            var role = _queryService.FindRole(roleName);
            if (role == null)
            {
                throw new KeyNotFoundException($"Role '{roleName}' was not found in the database.");
            }
            return role;
        }

        /// <summary>
        /// Finds a User by username, throwing KeyNotFoundException if not found, and ensures Roles are loaded.
        /// </summary>
        private User GetUserOrThrow(string username)
        {
            ValidateName(username, nameof(username));
            var user = _queryService.FindUser(username);
            if (user == null)
            {
                throw new KeyNotFoundException($"User '{username}' was not found in the database.");
            }
            // Explicitly load roles for modification context (e.g., adding/removing roles)
            DB.Entry(user).Collection(u => u.Roles).Load(); 
            return user;
        }

        // --- Role Management Operations (Create/Update/Delete) ---

        /// <summary>
        /// Creates a new role.
        /// </summary>
        public void CreateRole(string roleName)
        {
            ValidateName(roleName, nameof(roleName)); // Commit 1: Input validation
            
            if (_queryService.RoleExists(roleName))
            {
                throw new InvalidOperationException($"Role '{roleName}' already exists."); // Commit 2: Existence check
            }

            Role role = new Role { RoleName = roleName };
            DB.Roles.Add(role);
            DB.SaveChanges(); // Commit 3: Final save
        }

        /// <summary>
        /// Updates an existing role name.
        /// </summary>
        public void UpdateRole(int roleID, string newRoleName)
        {
            ValidateName(newRoleName, nameof(newRoleName)); // Commit 1: Input validation

            Role role = DB.Roles.Find(roleID);
            if (role == null)
            {
                throw new KeyNotFoundException($"Role with ID '{roleID}' was not found."); // Commit 2: Lookup and error
            }

            role.RoleName = newRoleName;
            DB.SaveChanges(); // Commit 3: Final save
        }

        /// <summary>
        /// Deletes an existing role, with optional exception handling for populated roles.
        /// </summary>
        public bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            try
            {
                Role roleToDelete = GetRoleOrThrow(roleName); // Commit 1: Use robust lookup helper

                if (throwOnPopulatedRole && roleToDelete.Users.Any())
                {
                    throw new InvalidOperationException($"Cannot delete role '{roleName}': it still has associated users.");
                }

                DB.Roles.Remove(roleToDelete); // Commit 2: Removal logic
                DB.SaveChanges(); // Commit 3: Final save
                return true;
            }
            catch (Exception ex)
            {
                // Ensures all expected exceptions (like KeyNotFound) are thrown, or returns false otherwise.
                if (throwOnPopulatedRole || ex is KeyNotFoundException) 
                {
                    throw;
                }
                return false;
            }
        }

        // --- User-Role Assignment Operations ---

        /// <summary>
        /// Atomically adds a single user to a single role.
        /// </summary>
        public void AddUserToRole(string username, string roleName)
        {
            Role roleToAdd = GetRoleOrThrow(roleName); // Commit 1: Role lookup
            User userToAdd = GetUserOrThrow(username); // Commit 2: User lookup
            
            if (_queryService.IsUserInRole(username, roleName))
            {
                return; // Already assigned, safe to exit
            }

            userToAdd.Roles.Add(roleToAdd); // Commit 3: Modification logic
            DB.SaveChanges(); // Commit 4: Final save
        }

        /// <summary>
        /// Adds several users to several roles using the atomic AddUserToRole.
        /// </summary>
        public void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            if (usernames == null || roleNames == null) throw new ArgumentNullException();

            foreach (string username in usernames)
            {
                foreach (string roleName in roleNames)
                {
                    AddUserToRole(username, roleName); // Commit point for iterating batch logic
                }
            }
        }

        /// <summary>
        /// Private helper to remove a list of users from a single role, followed by a single save.
        /// </summary>
        private void RemoveUsersFromSingleRole(string[] usernames, string roleName)
        {
            Role role = GetRoleOrThrow(roleName); // Commit 1: Role lookup

            foreach (string username in usernames)
            {
                User user = GetUserOrThrow(username); // Commit 2: User lookup
                user.Roles.Remove(role); // Commit 3: Modification logic
            }
            DB.SaveChanges(); // Commit 4: Final save
        }

        /// <summary>
        /// Removes given users from multiple roles by iterating through roles.
        /// </summary>
        public void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            if (usernames == null || roleNames == null) throw new ArgumentNullException();

            foreach (string roleName in roleNames)
            {
                RemoveUsersFromSingleRole(usernames, roleName); // Commit point for iterating batch logic
            }
        }

        /// <summary>
        /// Clears all existing roles for a user and assigns a single new role.
        /// </summary>
        public void UpdateRoleToUser(string username, string roleName)
        {
            Role newRole = GetRoleOrThrow(roleName); // Commit 1: Role lookup
            User user = GetUserOrThrow(username); // Commit 2: User lookup

            user.Roles.Clear(); // Commit 3: Clear all roles (explicit action)
            user.Roles.Add(newRole); // Commit 4: Add new role (explicit action)

            DB.SaveChanges(); // Commit 5: Final save
        }

        // --- Delegation of Read Operations to Query Service ---
        
        // These methods are now simple wrappers, ensuring the Logic class remains thin for read operations.
        public string[] GetAllRoleNames() => _queryService.GetAllRoleNames();
        public string[] GetRolesForUser(string username) => _queryService.GetRolesForUser(username);
        public string[] GetUsersInRole(string roleName) => _queryService.GetUsersInRole(roleName);
        public string[] FindUsersInRole(string roleName, string usernameToMatch) => _queryService.FindUsersInRole(roleName, usernameToMatch);
        public bool IsUserInRole(string username, string roleName) => _queryService.IsUserInRole(username, roleName);
        public bool RoleExists(string roleName) => _queryService.RoleExists(roleName);
        public List<Role> GetAllRoles() => _queryService.GetAllRoles();
    }
}
