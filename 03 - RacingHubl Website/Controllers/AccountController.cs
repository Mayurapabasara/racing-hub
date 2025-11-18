using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.Owin.Security;
using RacingHubCarRental.Services;
using RacingHubCarRental.Models.ViewModels;

namespace RacingHubCarRental.Controllers
{
    /// <summary>
    /// Handles all authentication & user account operations.
    /// Advanced version rewritten for better maintainability, 
    /// testability, SOLID structure, async performance, and security.
    /// </summary>
    public class AccountController : Controller
    {
        private readonly IUsersService _usersService;
        private IAuthenticationManager Auth => HttpContext.GetOwinContext().Authentication;

        /// <summary>
        /// Uses dependency injection to resolve needed services.
        /// </summary>
        public AccountController(IUsersService usersService)
        {
            _usersService = usersService 
                ?? throw new ArgumentNullException(nameof(usersService));
        }

        // ================================
        // REGISTER (GET)
        // ================================
        public ActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("UserProfile");

            return View(new RegisterViewModel());
        }

        // ================================
        // REGISTER (POST)
        // ================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                // Username uniqueness check
                if (await _usersService.IsUsernameTakenAsync(model.Username))
                {
                    ModelState.AddModelError("", "Username is already taken.");
                    return View(model);
                }

                // Register the user
                var result = await _usersService.RegisterAsync(model);

                if (!result.Success)
                {
                    ModelState.AddModelError("", result.ErrorMessage);
                    return View(model);
                }

                // Auto sign-in
                Auth.SignIn(
                    new Microsoft.Owin.Security.AuthenticationProperties { IsPersistent = false },
                    result.Identity
                );

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                // Log error here (Serilog / NLog)
                ModelState.AddModelError("", "Unexpected error occurred. Please try again later.");
                return View(model);
            }
        }

        // ================================
        // LOGIN (GET)
        // ================================
        public ActionResult Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("UserProfile");

            ViewBag.ReturnUrl = DetermineReturnUrl(returnUrl);
            return View(new LoginViewModel());
        }

        // ================================
        // LOGIN (POST)
        // ================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ReturnUrl = DetermineReturnUrl(returnUrl);
                return View(model);
            }

            try
            {
                var authResult = await _usersService.AuthenticateAsync(model);

                if (!authResult.Success)
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                    ViewBag.ReturnUrl = DetermineReturnUrl(returnUrl);
                    return View(model);
                }

                // Sign in
                Auth.SignIn(
                    new Microsoft.Owin.Security.AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe
                    },
                    authResult.Identity
                );

                return RedirectToLocal(returnUrl);
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Login failed due to a system error.");
                return View(model);
            }
        }

        // ================================
        // LOGOUT
        // ================================
        [Authorize]
        public ActionResult Logout()
        {
            Auth.SignOut();
            return RedirectToAction("Index", "Home");
        }

        // ================================
        // PROFILE
        // ================================
        [Authorize]
        public async Task<ActionResult> UserProfile()
        {
            try
            {
                var user = await _usersService.GetUserByUsernameAsync(User.Identity.Name);
                return View(user);
            }
            catch
            {
                ModelState.AddModelError("", "Failed to load profile.");
                return View();
            }
        }

        // ================================
        // FORGOT PASSWORD
        // ================================
        public ActionResult ForgottenPassword()
        {
            return View();
        }

        // ================================
        // PRIVATE HELPERS
        // ================================
        private string DetermineReturnUrl(string returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl))
                return returnUrl;

            if (Request.UrlReferrer != null)
                return Request.UrlReferrer.ToString();

            return Url.Action("Index", "Home");
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }
    }
}

