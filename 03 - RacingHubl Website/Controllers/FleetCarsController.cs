using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web.Mvc;

namespace RacingHubCarRental
{
    /// <summary>
    /// Controller responsible for managing fleet cars (Admin/Manager)
    /// and exposing public search operations for customers.
    /// </summary>
    [Authorize(Roles = "Admin, Manager")]
    public class FleetCarsController : Controller
    {
        private readonly FleetCarsLogic _logic = new FleetCarsLogic();

        // --------------------------------------------------------------------
        // HELPERS
        // --------------------------------------------------------------------

        private ActionResult SafeView(Func<ActionResult> action)
        {
            try { return action(); }
            catch (Exception)
            {
                ViewBag.ErrorMessage = "An error occurred. Please try again later.";
                return View(new FleetCar());
            }
        }

        private ActionResult SafeView<T>(Func<T> modelFactory)
        {
            try { return View(modelFactory()); }
            catch (Exception)
            {
                ViewBag.ErrorMessage = "An error occurred. Please try again later.";
                return View(default(T));
            }
        }

        private ActionResult SafeRedirect(Func<ActionResult> action)
        {
            try { return action(); }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred.";
                return RedirectToAction("Index");
            }
        }

        // ====================================================================
        //                          ADMIN / MANAGER CRUD
        // ====================================================================

        public ActionResult Index() =>
            SafeView(() => _logic.GetAllFleetCars());

        public ActionResult Details(string licensePlate) =>
            SafeView(() =>
            {
                var car = _logic.GetFleetCarByLicensePlate(licensePlate);
                return car == null ? HttpNotFound() : View(car);
            });

        public ActionResult Create() => View(new FleetCarViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(FleetCarViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            return SafeView(() =>
            {
                if (_logic.IsFleetCarExists(model.FleetCar.LicensePlate))
                {
                    ViewBag.ErrorMessage = "Car already exists in the fleet.";
                    return View(model);
                }

                _logic.InsertFleetCar(model.FleetCar);
                return RedirectToAction("Index");
            });
        }

        public ActionResult Edit(string licensePlate) =>
            SafeView(() =>
            {
                var car = _logic.GetFleetCarByLicensePlate(licensePlate);
                if (car == null)
                    return HttpNotFound();

                return View(new FleetCarViewModel { FleetCar = car });
            });

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit(FleetCarViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            return SafeRedirect(() =>
            {
                _logic.UpdateFleetCar(model.FleetCar);
                return RedirectToAction("Index");
            });
        }

        public ActionResult Delete(string licensePlate) =>
            SafeView(() =>
            {
                var car = _logic.GetFleetCarByLicensePlate(licensePlate);
                return car == null ? HttpNotFound() : View(car);
            });

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string licensePlate)
        {
            try
            {
                var car = _logic.GetFleetCarByLicensePlate(licensePlate);
                _logic.DeleteFleetCar(car);
                return RedirectToAction("Index");
            }
            catch (DbUpdateException)
            {
                return RedirectToAction("DeleteFailed", new { licensePlate });
            }
            catch (Exception)
            {
                return SafeView(() => _logic.GetFleetCarByLicensePlate(licensePlate));
            }
        }

        public ActionResult DeleteFailed(string licensePlate) =>
            SafeView(() =>
            {
                var car = _logic.GetFleetCarByLicensePlate(licensePlate);
                return car == null ? HttpNotFound() : View(car);
            });

        [HttpPost, ActionName("DeleteFailed"), ValidateAntiForgeryToken]
        public ActionResult DeleteFailedProceed(string licensePlate) =>
            RedirectToAction("DeleteCollective", new { licensePlate });

        public ActionResult DeleteCollective(string licensePlate) =>
            SafeView(() =>
            {
                var car = _logic.GetFleetCarByLicensePlate(licensePlate);
                return car == null ? HttpNotFound() : View(car);
            });

        [HttpPost, ActionName("DeleteCollective"), ValidateAntiForgeryToken]
        public ActionResult DeleteCollectiveConfirmed(string licensePlate) =>
            SafeRedirect(() =>
            {
                var car = _logic.GetFleetCarByLicensePlate(licensePlate);
                _logic.DeleteFleetCar(car, true);
                return RedirectToAction("Index");
            });

        // ====================================================================
        //                           PUBLIC SEARCH
        // ====================================================================

        [AllowAnonymous]
        public ActionResult Watch() =>
            View("Search", new SearchViewModel());

        [AllowAnonymous]
        public ActionResult CarDetails(string licensePlate) =>
            SafeView(() =>
            {
                var car = _logic.GetFleetCarByLicensePlate(licensePlate);
                return car == null ? HttpNotFound() : View(car);
            });

        [AllowAnonymous]
        public ActionResult Search(string freeText, DateTime? startDate, DateTime? returnDate)
        {
            var viewModel = new SearchViewModel
            {
                FreeText = freeText ?? "",
                StartDate = startDate ?? DateTime.MinValue,
                ReturnDate = returnDate ?? DateTime.MinValue
            };

            return View(viewModel);
        }

        // ====================================================================
        //                           AJAX ENDPOINTS
        // ====================================================================

        [AllowAnonymous]
        [HttpGet]
        public JsonResult AdvancedSearch(SearchViewModel model)
        {
            if (!ModelState.IsValid)
                return Json("INVALID", JsonRequestBehavior.AllowGet);

            try
            {
                var cars = _logic.SearchCars(
                    model.ManufacturerID,
                    model.ManufacturerModelID,
                    model.ProductionYear,
                    model.ManualGear,
                    model.StartDate,
                    model.ReturnDate,
                    model.FreeText
                );

                var result = cars.Select(c => new
                {
                    c.LicensePlate,
                    c.ManufacturerName,
                    c.ManufacturerModelName,
                    Year = c.ProductionYear,
                    c.ManualGear,
                    c.DailyPrice,
                    c.DayDelayPrice,
                    c.CarImage
                });

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json("ERROR", JsonRequestBehavior.AllowGet);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public JsonResult CheckCarAvailability(string licensePlate, DateTime startDate, DateTime returnDate)
        {
            try
            {
                return Json(
                    _logic.CheckCarAvailability(licensePlate, startDate, returnDate),
                    JsonRequestBehavior.AllowGet
                );
            }
            catch
            {
                return Json("ERROR", JsonRequestBehavior.AllowGet);
            }
        }
    }
}

