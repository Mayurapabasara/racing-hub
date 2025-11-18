using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using RacingHubCarRental.Services;
using RacingHubCarRental.Models;
using RacingHubCarRental.Models.ViewModels;

namespace RacingHubCarRental.Controllers
{
    /// <summary>
    /// Administration operations including user & role management.
    /// Fully rewritten using async, DI, strong validation, and modern patterns.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IUsersService _usersService;
        private readonly IRolesService _rolesService;

        /// <summary>
        /// Constructor with dependency injection.
        /// </summary>
        public AdminController(IUsersService usersService, IRolesService rolesService)
        {
            _usersService = usersService ?? throw new ArgumentNullException(nameof(usersService));
            _rolesService = rolesService ?? throw new ArgumentNullException(nameof(rolesService));
        }

        // ===========================
        // DASHBOARD
        // ===========================
        public ActionResult Index()
        {
            return View();
        }

        // ===========================
        // LIST ALL USERS
        // ===========================
        public async Task<ActionResult> AllUsers()
        {
            try
            {
                var users = await _usersService.GetAllUsersAsync();
                return View(users);
            }
            catch (Exception)
            {
                ViewBag.ErrorMessage = "Unable to load users. Please try again later.";
                return View(new List<User>());
            }
        }

        // ===========================
        // EDIT USER ROLE (GET)
        // ===========================
        public async Task<ActionResult> EditUserRole(int id = 0)
        {
            try
            {
                var user = await _usersService.GetUserByIdAsync(id);
                if (user == null)
                    return HttpNotFound();

                var model = new EditUserRoleViewModel
                {
                    Username = user.Username,
                    RoleName = user.Role?.RoleName
                };

                await LoadRolesDropDown(model.RoleName);

                return View(model);
            }
            catch
            {
                ViewBag.ErrorMessage = "Failed to load user role details.";
                await LoadRolesDropDown(null);
                return View(new EditUserRoleViewModel());
            }
        }

        // ===========================
        // EDIT USER ROLE (POST)
        // ===========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditUserRole(EditUserRoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ErrorMessage = "Invalid fields.";
                await LoadRolesDropDown(model.RoleName);
                return View(model);
            }

            try
            {
                var result = await _rolesService.UpdateUserRoleAsync(model.Username, model.RoleName);

                if (!result.Success)
                {
                    ModelState.AddModelError("", result.ErrorMessage);
                    await LoadRolesDropDown(model.RoleName);
                    return View(model);
                }

                return RedirectToAction("AllUsers");
            }
            catch
            {
                ViewBag.ErrorMessage = "An unexpected error occurred.";
                await LoadRolesDropDown(model.RoleName);
                return View(model);
            }
        }

        // ===========================
        // DELETE USER (GET)
        // ===========================
        public async Task<ActionResult> DeleteUser(int id = 0)
        {
            try
            {
                var user = await _usersService.GetUserByIdAsync(id);
                if (user == null)
                    return HttpNotFound();

                return View(user);
            }
            catch
            {
                ViewBag.ErrorMessage = "Failed to load user.";
                return View();
            }
        }

        // ===========================
        // DELETE USER (POST)
        // ===========================
        [HttpPost, ActionName("DeleteUser")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteUserConfirmed(int id)
        {
            try
            {
                var user = await _usersService.GetUserByIdAsync(id);
                if (user == null)
                    return HttpNotFound();

                await _usersService.DeleteUserAsync(user);

                return RedirectToAction("AllUsers");
            }
            catch
            {
                ViewBag.ErrorMessage = "Failed to delete user.";
                var user = await _usersService.GetUserByIdAsync(id);
                return View(user);
            }
        }

        // ===========================
        // HELPER: Load Role Dropdown
        // ===========================
        private async Task LoadRolesDropDown(string selectedRole)
        {
            var roles = await _rolesService.GetAllRolesAsync();
            ViewBag.RoleName = new SelectList(roles, "RoleName", "RoleName", selectedRole);
        }
    }
}

