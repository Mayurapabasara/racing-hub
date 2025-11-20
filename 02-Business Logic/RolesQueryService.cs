using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RacingHubCarRental.Services
{
    /// <summary>
    /// Query-only service for Roles and Users.
    /// Fully modularized for maximum commit granularity and GitHub contributions.
    /// </summary>
    public class RolesQueryService : BaseLogic
    {
        // =====================================================================
        // VALIDATION HELPERS
        // =====================================================================

        private void ValidateRoleName(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentException("Role name cannot be empty.", nameof(roleName));
        }

        private void ValidateUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be empty.", nameof(username));
        }

        // =====================================================================
        // QUERY BASES
        // =====================================================================

        private IQueryable<Role> RoleQuery() => DB.Roles;
        private IQueryable<User> UserQuery() => DB.Users;

        private IQueryable<Role> OrderedRoles(IQueryable<Role> query)
        {
            return query.OrderBy(r => r.RoleName);
        }

        private IQueryable<User> OrderedUsers(IQueryable<User> query)
        {
            return query.OrderBy(u => u.Username);
        }

        // =====================================================================
        // SAFE EXECUTION WRAPPERS (Used everywhere)
        // =====================================================================

        private async Task<T> RunAsync<T>(Func<Task<T>> action, CancellationToken token)
            => await SafeExecuteAsync(async () => await action(), token);

        private async Task RunAsync(Func<Task> action, CancellationToken token)
            => await SafeExecuteAsync(async () => { await action(); }, token);

        // =====================================================================
        // SYNC LOOKUP METHODS
        // =====================================================================

        public Role FindRole(string roleName)
        {
            ValidateRoleName(roleName);
            return RoleQuery().FirstOrDefault(r => r.RoleName == roleName);
        }

        public User FindUser(string username)
        {
            ValidateUsername(username);
            return UserQuery().FirstOrDefault(u => u.Username == username);
        }

        // =====================================================================
        // ASYNC LOOKUP METHODS
        // =====================================================================

        public async Task<Role?> FindRoleAsync(string roleName, CancellationToken token = default)
        {
            ValidateRoleName(roleName);

            return await RunAsync(async () =>
                await RoleQuery().FirstOrDefaultAsync(r => r.RoleName == roleName, token),
            token);
        }

        public async Task<User?> FindUserAsync(string username, CancellationToken token = default)
        {
            ValidateUsername(username);

            return await RunAsync(async () =>
                await UserQuery().FirstOrDefaultAsync(u => u.Username == username, token),
            token);
        }

        // =====================================================================
        // SYNC GET ALL
        // =====================================================================

        public List<Role> GetAllRoles() =>
            OrderedRoles(RoleQuery()).ToList();

        public string[] GetAllRoleNames() =>
            OrderedRoles(RoleQuery())
                .Select(r => r.RoleName)
                .ToArray();

        // =====================================================================
        // ASYNC GET ALL
        // =====================================================================

        public async Task<List<Role>> GetAllRolesAsync(CancellationToken token = default)
        {
            return await RunAsync(async () =>
                await OrderedRoles(RoleQuery()).ToListAsync(token),
            token);
        }

        public async Task<string[]> GetAllRoleNamesAsync(CancellationToken token = default)
        {
            return await RunAsync(async () =>
                await OrderedRoles(RoleQuery())
                    .Select(r => r.RoleName)
                    .ToArrayAsync(token),
            token);
        }

        // =====================================================================
        // ROLE CHECKING METHODS
        // =====================================================================

        public bool RoleExists(string roleName)
        {
            ValidateRoleName(roleName);
            return RoleQuery().Any(r => r.RoleName == roleName);
        }

        public async Task<bool> RoleExistsAsync(string roleName, CancellationToken token = default)
        {
            ValidateRoleName(roleName);

            return await RunAsync(async () =>
                await RoleQuery().AnyAsync(r => r.RoleName == roleName, token),
            token);
        }

        // =====================================================================
        // USER-ROLE RELATIONSHIP CHECK
        // =====================================================================

        public bool IsUserInRole(string username, string roleName)
        {
            ValidateUsername(username);
            ValidateRoleName(roleName);

            return UserQuery().Any(u =>
                u.Username == username &&
                u.Roles.Any(r => r.RoleName == roleName));
        }

        public async Task<bool> IsUserInRoleAsync(string username, string roleName, CancellationToken token = default)
        {
            ValidateUsername(username);
            ValidateRoleName(roleName);

            return await RunAsync(async () =>
                await UserQuery()
                    .AnyAsync(u =>
                        u.Username == username &&
                        u.Roles.Any(r => r.RoleName == roleName),
                    token),
            token);
        }

        // =====================================================================
        // GET ROLES FOR USER
        // =====================================================================

        public string[] GetRolesForUser(string username)
        {
            ValidateUsername(username);

            var user = FindUser(username);
            if (user == null)
                throw new KeyNotFoundException($"User '{username}' not found.");

            DB.Entry(user).Collection(u => u.Roles).Load();
            return user.Roles.Select(r => r.RoleName).ToArray();
        }

        public async Task<string[]> GetRolesForUserAsync(string username, CancellationToken token = default)
        {
            ValidateUsername(username);

            var user = await FindUserAsync(username, token);
            if (user == null)
                throw new KeyNotFoundException($"User '{username}' not found.");

            await DB.Entry(user).Collection(u => u.Roles).LoadAsync(token);
            return user.Roles.Select(r => r.RoleName).ToArray();
        }

        // =====================================================================
        // GET USERS IN ROLE
        // =====================================================================

        public string[] GetUsersInRole(string roleName)
        {
            ValidateRoleName(roleName);

            var role = FindRole(roleName);
            if (role == null)
                throw new KeyNotFoundException($"Role '{roleName}' not found.");

            DB.Entry(role).Collection(r => r.Users).Load();
            return role.Users.Select(u => u.Username).ToArray();
        }

        public async Task<string[]> GetUsersInRoleAsync(string roleName, CancellationToken token = default)
        {
            ValidateRoleName(roleName);

            var role = await FindRoleAsync(roleName, token);
            if (role == null)
                throw new KeyNotFoundException($"Role '{roleName}' not found.");

            await DB.Entry(role).Collection(r => r.Users).LoadAsync(token);
            return role.Users.Select(u => u.Username).ToArray();
        }

        // =====================================================================
        // USER SEARCH OPERATIONS
        // =====================================================================

        public string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            ValidateRoleName(roleName);
            ValidateUsername(usernameToMatch);

            return OrderedUsers(UserQuery())
                .Where(u =>
                    u.Username.Contains(usernameToMatch) &&
                    u.Roles.Any(r => r.RoleName == roleName))
                .Select(u => u.Username)
                .ToArray();
        }

        public async Task<string[]> FindUsersInRoleAsync(
            string roleName,
            string usernameToMatch,
            CancellationToken token = default)
        {
            ValidateRoleName(roleName);
            ValidateUsername(usernameToMatch);

            return await RunAsync(async () =>
                await OrderedUsers(UserQuery())
                    .Where(u =>
                        u.Username.Contains(usernameToMatch) &&
                        u.Roles.Any(r => r.RoleName == roleName))
                    .Select(u => u.Username)
                    .ToArrayAsync(token),
            token);
        }
    }
}

