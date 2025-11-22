using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Web.Mvc;

namespace RacingHubCarRental
{
    /// <summary>
    /// MVC Controller responsible for managing Manufacturer entities.
    /// Refactored for clean architecture, commit-friendly granularity, and extensibility.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class ManufacturersController : Controller
    {
        // ============================================================
        // Dependencies
        // ============================================================

        private readonly ManufacturersLogic _logic;

        public ManufacturersController()
        {
            _logic = new ManufacturersLogic();
        }

        // ============================================================
        // Helpers (Commit-Friendly Micro Methods)
        // ============================================================

        private ActionResult SafeView<T>(Func<T> fn)
        {
            try
            {
                return View(fn());
            }
            catch (Exception)
            {
                ViewBag.ErrorMessage = "An error has occurred. Please try again later.";
                return View(default(T));
            }
        }

        private Manufacturer LoadManufacturerOr404(int id)
        {
            var manufacturer = _logic.GetManufacturerByID(id);
            if (manufacturer == null)
                throw new HttpException(404, "Manufacturer not found.");
            return manufacturer;
        }

        private ActionResult SafeRedirect(Func<ActionResult> action)
        {
            try
            {
                return action();
            }
            catch (Exception)
            {
                ViewBag.ErrorMessage = "An unexpected error has occurred.";
                return RedirectToAction("Index");
            }
        }

        // ============================================================
        // Index
        // ============================================================

        public ActionResult Index()
        {
            return SafeView(() =>
                _logic.GetAllManufacturers()
            );
        }

        // ============================================================
        // Details
        // ============================================================

        public ActionResult Details(int id = 0)
        {
            return SafeView(() =>
                LoadManufacturerOr404(id)
            );
        }

        // ============================================================
        // Create (GET)
        // ============================================================

        public ActionResult Create() => View();

        // ============================================================
        // Create (POST)
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Manufacturer manufacturer)
        {
            if (!ModelState.IsValid)
                return View(manufacturer);

            return SafeRedirect(() =>
            {
                if (_logic.IsManufacturerExists(manufacturer.ManufacturerName))
                {
                    ViewBag.ErrorMessage = "Manufacturer already exists.";
                    return View(manufacturer);
                }

                _logic.InsertManufacturer(manufacturer);
                return RedirectToAction("Index");
            });
        }

        // ============================================================
        // Edit (GET)
        // ============================================================

        public ActionResult Edit(int id = 0)
        {
            return SafeView(() =>
                LoadManufacturerOr404(id)
            );
        }

        // ============================================================
        // Edit (POST)
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Manufacturer manufacturer)
        {
            if (!ModelState.IsValid)
                return View(manufacturer);

            return SafeRedirect(() =>
            {
                _logic.UpdateManufacturer(manufacturer);
                return RedirectToAction("Index");
            });
        }

        // ============================================================
        // Delete (GET)
        // ============================================================

        public ActionResult Delete(int id = 0)
        {
            return SafeView(() =>
                LoadManufacturerOr404(id)
            );
        }

        // ============================================================
        // Delete (POST)
        // ============================================================

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            return SafeRedirect(() =>
            {
                try
                {
                    var manufacturer = LoadManufacturerOr404(id);
                    _logic.DeleteManufacturer(manufacturer);
                    return RedirectToAction("Index");
                }
                catch (DbUpdateException)
                {
                    return RedirectToAction("DeleteFailed", new { id });
                }
            });
        }

        // ============================================================
        // Delete Failed
        // ============================================================

        public ActionResult DeleteFailed(int id = 0)
        {
            return SafeView(() =>
                LoadManufacturerOr404(id)
            );
        }

        [HttpPost, ActionName("DeleteFailed")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteFailedProceed(int id)
        {
            return RedirectToAction("DeleteCollective", new { id });
        }

        // ============================================================
        // Delete Collective (GET)
        // ============================================================

        public ActionResult DeleteCollective(int id = 0)
        {
            return SafeView(() =>
                LoadManufacturerOr404(id)
            );
        }

        // ============================================================
        // Delete Collective (POST)
        // ============================================================

        [HttpPost, ActionName("DeleteCollective")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteCollectiveConfirmed(int id)
        {
            return SafeRedirect(() =>
            {
                var manufacturer = LoadManufacturerOr404(id);
                _logic.DeleteManufacturer(manufacturer, true);
                return RedirectToAction("Index");
            });
        }
    }
}

