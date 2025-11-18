using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using RacingHubCarRental.Services.Interfaces;
using RacingHubCarRental.Models;
using RacingHubCarRental.ViewModels;

namespace RacingHubCarRental.Controllers
{
    /// <summary>
    /// Manages CRUD operations for Manufacturer Models (Admin-only).
    /// Fully rewritten for async, DI, SOLID and clean-architecture support.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class ManufacturerModelsController : Controller
    {
        private readonly IManufacturerModelsService _modelsService;
        private readonly IManufacturersService _manufacturersService;

        public ManufacturerModelsController(
            IManufacturerModelsService modelsService,
            IManufacturersService manufacturersService)
        {
            _modelsService = modelsService;
            _manufacturersService = manufacturersService;
        }

        // =======================================================
        // LIST PAGE
        // =======================================================
        public async Task<ActionResult> Index()
        {
            try
            {
                var list = await _modelsService.GetAllAsync();
                return View(list);
            }
            catch
            {
                ViewBag.ErrorMessage = "Failed to load manufacturer models.";
                return View(new List<ManufacturerModel>());
            }
        }

        // =======================================================
        // DETAILS PAGE
        // =======================================================
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                var entity = await _modelsService.GetByIdAsync(id);
                if (entity == null)
                    return HttpNotFound();

                return View(entity);
            }
            catch
            {
                ViewBag.ErrorMessage = "Could not load details.";
                return View(new ManufacturerModel());
            }
        }

        // =======================================================
        // CREATE (GET)
        // =======================================================
        public async Task<ActionResult> Create()
        {
            try
            {
                var vm = new ManufacturerModelViewModel
                {
                    Manufacturers = await LoadManufacturersAsync()
                };

                return View(vm);
            }
            catch
            {
                ViewBag.ErrorMessage = "Error loading create page.";
                return View(new ManufacturerModelViewModel());
            }
        }

        // =======================================================
        // CREATE (POST)
        // =======================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ManufacturerModelViewModel vm)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    vm.Manufacturers = await LoadManufacturersAsync();
                    return View(vm);
                }

                if (await _modelsService.ExistsAsync(vm.ToEntity()))
                {
                    ViewBag.ErrorMessage = "This manufacturer model already exists.";
                    vm.Manufacturers = await LoadManufacturersAsync();
                    return View(vm);
                }

                await _modelsService.CreateAsync(vm.ToEntity());
                return RedirectToAction("Index");
            }
            catch
            {
                ViewBag.ErrorMessage = "Failed to create manufacturer model.";
                vm.Manufacturers = await LoadManufacturersAsync();
                return View(vm);
            }
        }

        // =======================================================
        // EDIT (GET)
        // =======================================================
        public async Task<ActionResult> Edit(int id)
        {
            try
            {
                var entity = await _modelsService.GetByIdAsync(id);
                if (entity == null)
                    return HttpNotFound();

                var vm = ManufacturerModelViewModel.FromEntity(entity);
                vm.Manufacturers = await LoadManufacturersAsync();

                return View(vm);
            }
            catch
            {
                ViewBag.ErrorMessage = "Error loading edit page.";
                return View(new ManufacturerModelViewModel());
            }
        }

        // =======================================================
        // EDIT (POST)
        // =======================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ManufacturerModelViewModel vm)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    vm.Manufacturers = await LoadManufacturersAsync();
                    return View(vm);
                }

                await _modelsService.UpdateAsync(vm.ToEntity());
                return RedirectToAction("Index");
            }
            catch
            {
                ViewBag.ErrorMessage = "Failed to update manufacturer model.";
                vm.Manufacturers = await LoadManufacturersAsync();
                return View(vm);
            }
        }

        // =======================================================
        // DELETE (GET)
        // =======================================================
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var entity = await _modelsService.GetByIdAsync(id);
                if (entity == null)
                    return HttpNotFound();

                return View(entity);
            }
            catch
            {
                ViewBag.ErrorMessage = "Could not load delete page.";
                return View(new ManufacturerModel());
            }
        }

        // =======================================================
        // DELETE (POST)
        // =======================================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var result = await _modelsService.DeleteAsync(id);
                if (!result.Success)
                {
                    return RedirectToAction("DeleteFailed", new { id });
                }

                return RedirectToAction("Index");
            }
            catch
            {
                ViewBag.ErrorMessage = "Failed to delete manufacturer model.";
                return View(await _modelsService.GetByIdAsync(id));
            }
        }

        // =======================================================
        // DELETE FAILED
        // =======================================================
        public async Task<ActionResult> DeleteFailed(int id)
        {
            try
            {
                var entity = await _modelsService.GetByIdAsync(id);
                if (entity == null)
                    return HttpNotFound();

                return View(entity);
            }
            catch
            {
                ViewBag.ErrorMessage = "Error loading delete failed page.";
                return View(new ManufacturerModel());
            }
        }

        [HttpPost, ActionName("DeleteFailed")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteFailedProceed(int id)
        {
            return RedirectToAction("DeleteCollective", new { id });
        }

        // =======================================================
        // DELETE COLLECTIVE (GET)
        // =======================================================
        public async Task<ActionResult> DeleteCollective(int id)
        {
            try
            {
                var entity = await _modelsService.GetByIdAsync(id);
                if (entity == null)
                    return HttpNotFound();

                return View(entity);
            }
            catch
            {
                ViewBag.ErrorMessage = "Error loading delete collective page.";
                return View(new ManufacturerModel());
            }
        }

        // =======================================================
        // DELETE COLLECTIVE (POST)
        // =======================================================
        [HttpPost, ActionName("DeleteCollective")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteCollectiveConfirmed(int id)
        {
            try
            {
                await _modelsService.DeleteCollectiveAsync(id);
                return RedirectToAction("Index");
            }
            catch
            {
                ViewBag.ErrorMessage = "Could not delete manufacturer model.";
                return View(await _modelsService.GetByIdAsync(id));
            }
        }

        // =======================================================
        // AJAX: Get Models For Manufacturer
        // =======================================================
        [AllowAnonymous]
        public async Task<JsonResult> GetModelsForManufacturer(string manufacturerID)
        {
            try
            {
                if (!int.TryParse(manufacturerID, out int id))
                    return Json(null, JsonRequestBehavior.AllowGet);

                var models = await _modelsService.GetByManufacturerIdAsync(id);

                var result = models.ConvertAll(x => new SelectListItem
                {
                    Text = x.ManufacturerModelName,
                    Value = x.ManufacturerModelID.ToString()
                });

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        // =======================================================
        // HELPER: Load Dropdowns
        // =======================================================
        private async Task<List<SelectListItem>> LoadManufacturersAsync()
        {
            var list = await _manufacturersService.GetAllAsync();
            return list.ConvertAll(m => new SelectListItem
            {
                Text = m.ManufacturerName,
                Value = m.ManufacturerID.ToString()
            });
        }
    }
}

