using System;
using System.Web.Mvc;

namespace RacingHubCarRental
{
    /// <summary>
    /// Centralized MVC controller responsible for rendering error pages.
    /// Designed with extensibility, commit granularity, and clean layering in mind.
    /// </summary>
    public class ErrorController : Controller
    {
        // ============================================================
        // Constants (Separate commits for adding constants)
        // ============================================================

        private const string GeneralErrorView = "Index";
        private const string ForbiddenErrorView = "HttpError403";
        private const string NotFoundErrorView = "HttpError404";
        private const string InternalErrorView = "HttpError500";

        // ============================================================
        // Public Entry Points
        // ============================================================

        /// <summary>
        /// Default fallback error page (General Error).
        /// </summary>
        public ActionResult Index()
        {
            return RenderSafeView(GeneralErrorView);
        }

        /// <summary>
        /// Renders the 403 Forbidden error page.
        /// </summary>
        public ActionResult HttpError403()
        {
            return RenderSafeView(ForbiddenErrorView);
        }

        /// <summary>
        /// Renders the 404 Not Found error page.
        /// </summary>
        public ActionResult HttpError404()
        {
            return RenderSafeView(NotFoundErrorView);
        }

        /// <summary>
        /// Renders the 500 Internal Server Error page.
        /// Defaults to general error view.
        /// </summary>
        public ActionResult HttpError500()
        {
            return RenderSafeView(GeneralErrorView);
        }

        // ============================================================
        // Helper Methods (Each one commit-friendly)
        // ============================================================

        /// <summary>
        /// Attempts to render a view by name.
        /// Fallback to the general error page if view is missing.
        /// </summary>
        private ActionResult RenderSafeView(string viewName)
        {
            try
            {
                return View(viewName);
            }
            catch
            {
                // Future commit: Add logging here.
                return View(GeneralErrorView);
            }
        }

        /// <summary>
        /// Allows easy future mapping of error codes to views.
        /// You can extend this to support dynamic routing.
        /// </summary>
        protected string ResolveView(int statusCode)
        {
            return statusCode switch
            {
                403 => ForbiddenErrorView,
                404 => NotFoundErrorView,
                500 => InternalErrorView,
                _ => GeneralErrorView
            };
        }
    }
}

