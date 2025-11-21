using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using RacingHubCarRental.Services.Interfaces;
using RacingHubCarRental.ViewModels;
using RacingHubCarRental.Models;

namespace RacingHubCarRental.Controllers
{
    /// <summary>
    /// Controller responsible for Rental Order creation, listing, history, 
    /// employee return operations and admin operations.
    /// Fully rewritten for: async, DI, SOLID, readability & commit granularity.
    /// </summary>
    [Authorize]
    public class OrdersController : Controller
    {
        // ============================================================
        // Dependencies (DI)
        // ============================================================

        private readonly IOrdersService _orders;
        private readonly IFleetCarsService _fleet;

        public OrdersController(IOrdersService orders, IFleetCarsService fleet)
        {
            _orders = orders;
            _fleet = fleet;
        }

        // ============================================================
        // Helper: Build ViewModel
        // ============================================================

        private OrderViewModel BuildOrderVM(FleetCar car, DateTime start, DateTime end)
        {
            return new OrderViewModel
            {
                LicensePlate = car.LicensePlate,
                CarImage = car.CarImage,
                DailyPrice = car.CarModel.DailyPrice,
                DayDelayPrice = car.CarModel.DayDelayPrice,
                StartDate = start.Date,
                ReturnDate = end.Date,
                CarModelName = $"{car.CarModel.ManufacturerModel.Manufacturer.ManufacturerName} " +
                               $"{car.CarModel.ManufacturerModel.ManufacturerModelName} " +
                               $"{car.CarModel.ProductionYear} " +
                               $"{(car.CarModel.ManualGear ? "Manual" : "Automatic")}"
            };
        }

        // ============================================================
        // CREATE (GET)
        // ============================================================

        public async Task<ActionResult> Create(string licensePlate, DateTime startDate, DateTime returnDate)
        {
            try
            {
                var car = await _fleet.GetByLicensePlateAsync(licensePlate);
                if (car == null)
                    return HttpNotFound("Car not found.");

                var vm = BuildOrderVM(car, startDate, returnDate);
                return View(vm);
            }
            catch
            {
                ViewBag.ErrorMessage = "Error loading order form.";
                return View(new OrderViewModel());
            }
        }

        // ============================================================
        // CREATE (POST)
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(OrderViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                bool available = await _fleet.IsAvailableAsync(
                    vm.LicensePlate, vm.StartDate, vm.ReturnDate);

                if (!available)
                {
                    ViewBag.ErrorMessage = "Car is unavailable for the selected dates.";
                    return View(vm);
                }

                int orderId = await _orders.CreateRentalAsync(
                    vm.LicensePlate, vm.StartDate, vm.ReturnDate, User.Identity.Name);

                return RedirectToAction("OrderReceipt", new { id = orderId });
            }
            catch
            {
                ViewBag.ErrorMessage = "Could not complete the order.";
                return View(vm);
            }
        }

        // ============================================================
        // RECEIPT
        // ============================================================

        public ActionResult OrderReceipt(int id)
        {
            return View(id);
        }

        // ============================================================
        // USER HISTORY
        // ============================================================

        public async Task<ActionResult> History()
        {
            try
            {
                var list = await _orders.GetHistoryAsync(User.Identity.Name);
                return View(list);
            }
            catch
            {
                ViewBag.ErrorMessage = "Unable to load rental history.";
                return View(new List<Rental>());
            }
        }

        // ============================================================
        // WATCH ALL (ADMIN only)
        // ============================================================

        [Authorize(Roles = "Admin, Manager")]
        public async Task<ActionResult> Watch()
        {
            try
            {
                var list = await _orders.GetAllAsync();
                return View(list);
            }
            catch
            {
                ViewBag.ErrorMessage = "Unable to load rental list.";
                return View(new List<Rental>());
            }
        }

        // ============================================================
        // CARS TO RETURN (GET)
        // ============================================================

        [Authorize(Roles = "Admin, Employee")]
        public async Task<ActionResult> CarsToReturn()
        {
            try
            {
                var list = await _orders.GetReturnableAsync();
                return View(list);
            }
            catch
            {
                ViewBag.ErrorMessage = "Unable to load cars for return.";
                return View(new List<Rental>());
            }
        }

        // ============================================================
        // CARS TO RETURN (POST - Search)
        // ============================================================

        [HttpPost]
        [Authorize(Roles = "Admin, Employee")]
        public async Task<ActionResult> CarsToReturn(int receiptNum)
        {
            try
            {
                var rental = await _orders.GetByIdAsync(receiptNum);
                var list = rental != null ? new List<Rental> { rental } : new List<Rental>();
                return View(list);
            }
            catch
            {
                ViewBag.ErrorMessage = "Search failed.";
                return View(new List<Rental>());
            }
        }

        // ============================================================
        // CAR RETURN (GET)
        // ============================================================

        [Authorize(Roles = "Admin, Employee")]
        public async Task<ActionResult> CarReturn(int id)
        {
            try
            {
                var rental = await _orders.GetByIdAsync(id);
                return rental == null ? HttpNotFound() : View(rental);
            }
            catch
            {
                ViewBag.ErrorMessage = "Could not load return page.";
                return View(new Rental());
            }
        }

        // ============================================================
        // CAR RETURN (POST)
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("CarReturn")]
        [Authorize(Roles = "Admin, Employee")]
        public async Task<ActionResult> CarReturnConfirmed(int rentalID)
        {
            try
            {
                await _orders.MarkReturnedAsync(rentalID);
                return RedirectToAction("CarsToReturn");
            }
            catch
            {
                ViewBag.ErrorMessage = "Could not return car.";
                return View(new Rental());
            }
        }
    }
}

