using System;
using System.Web.Mvc;

namespace RacingHubCarRental
{
    /// <summary>
    /// Central controller for all application-level error pages.
    /// Structured for extensibility, clean commits, and future expansion.
    /// </summary>
    public class ErrorController : Controller
    {
        // ============================================================
        // View Name Constants (Easy to track in version control)
        // ============================================================

        private const string ViewGeneralError = "Index";
        private const string ViewForbidden = "HttpError403";
        private const string ViewNotFound = "HttpError404";
        private const string ViewInternal = "HttpError500";

        // ============================================================
        // Public Endpoints
        // ============================================================

        /// <summary>
        /// Default fallback page for general errors.
        /// </summary>
        public ActionResult Index()
        {
            return RenderErrorPage(ViewGeneralError);
        }

        /// <summary>
        /// Page rendered for HTTP 403 Forbidden errors.
        /// </summary>
        public ActionResult Forbidden()
        {
            return RenderErrorPage(ViewForbidden);
        }

        /// <summary>
        /// Page rendered for HTTP 404 Not Found errors.
        /// </summary>
        public ActionResult NotFoundPage()
        {
            return RenderErrorPage(ViewNotFound);
        }

        /// <summary>
        /// Page rendered for HTTP 500 Internal Server Error.
        /// Falls back to general error page if needed.
        /// </summary>
        public ActionResult InternalError()
        {
            return RenderErrorPage(ViewInternal);
        }

        // ============================================================
        // Helper Methods (Small, testable, commit-friendly units)
        // ============================================================

        /// <summary>
        /// Safely renders an error view.  
        /// If the view does not exist, falls back to the general error view.
        /// </summary>
        private ActionResult RenderErrorPage(string viewName)
        {
            try
            {
                return View(viewName);
            }
            catch (Exception)
            {
                // IDEA for future commit: replace with ILogger
                return View(ViewGeneralError);
            }
        }

        /// <summary>
        /// Maps HTTP status codes to view names.
        /// Extend this easily without modifying render logic.
        /// </summary>
        protected string MapErrorView(int statusCode)
        {
            return statusCode switch
            {
                403 => ViewForbidden,
                404 => ViewNotFound,
                500 => ViewInternal,
                _ => ViewGeneralError
            };
        }
    }
}

