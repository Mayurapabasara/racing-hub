using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using RacingHubCarRental.Services.Interfaces;
using RacingHubCarRental.Models;
using RacingHubCarRental.ViewModels;

namespace RacingHubCarRental.Controllers
{
    /// <summary>
    /// Controller responsible for managing CRUD operations
    /// for <see cref="ManufacturerModel"/> entities.
    /// 
    /// This upgraded controller version includes:
    /// - Async/await fully implemented
    /// - Cancellation token support
    /// - Centralized exception handling
    /// - Clean-architecture structure
    /// - Extra helper methods
    /// - More readable & testable patterns
    /// </summary>
    [Authorize(Roles = "Admin")]
    [RoutePrefix("admin/manufacturer-models")]
    public class ManufacturerModelsController : Controller
    {
        // ==========================================================
        // Services
        // ==========================================================
        private readonly IManufacturerModelsService _modelsService;
        private readonly IManufacturersService _manufacturersService;

        // ==========================================================
        // Constructor
        // ==========================================================
        public ManufacturerModelsController(
            IManufacturerModelsService modelsService,
            IManufacturersService manufacturersService)
        {
            _modelsService = modelsService ?? throw new ArgumentNullException(nameof(modelsService));
            _manufacturersService = manufacturersService ?? throw new ArgumentNullException(nameof(manufacturersService));
        }

        // ==========================================================
        // Public Actions
        // ==========================================================

        #region Index (List)

        /// <summary>
        /// Returns the main list of manufacturer models.
        /// </summary>
        [HttpGet]
        [Route("")]
        public async Task<ActionResult> Index(CancellationToken token = default)
        {
            return await ExecuteSafe(async () =>
            {
                var models = await _modelsService.GetAllAsync(token);
                return View(models);
            },
            onError: _ => View(new List<ManufacturerModel>()));
        }

        #endregion

        #region Details

        [HttpGet]
        [Route("details/{id:int}")]
        public async Task<ActionResult> Details(int id, CancellationToken token = default)
        {
            return await ExecuteSafe(async () =>
            {
                var entity = await _modelsService.GetByIdAsync(id, token);
                if (entity == null) return HttpNotFound();
                return View(entity);
            },
            onError: _ => View(new ManufacturerModel()));
        }

        #endregion

        #region Create (GET)

        [HttpGet]
        [Route("create")]
        public async Task<ActionResult> Create(CancellationToken token = default)
        {
            return await ExecuteSafe(async () =>
            {
                var vm = new ManufacturerModelViewModel
                {
                    Manufacturers = await LoadManufacturersAsync(token)
                };
                return View(vm);
            },
            onError: _ => View(new ManufacturerModelViewModel()));
        }

        #endregion

        #region Create (POST)

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("create")]
        public async Task<ActionResult> Create(ManufacturerModelViewModel vm, CancellationToken token = default)
        {
            return await ExecuteSafe(async () =>
            {
                if (!ModelState.IsValid)
                {
                    vm.Manufacturers = await LoadManufacturersAsync(token);
                    return View(vm);
                }

                var entity = vm.ToEntity();
                if (await _modelsService.ExistsAsync(entity, token))
                {
                    SetError("This manufacturer model already exists.");
                    vm.Manufacturers = await LoadManufacturersAsync(token);
                    return View(vm);
                }

                await _modelsService.CreateAsync(entity, token);
                return RedirectToAction("Index");
            },
            onError: async _ =>
            {
                vm.Manufacturers = await LoadManufacturersAsync(token);
                return View(vm);
            });
        }

        #endregion

        #region Edit (GET)

        [HttpGet]
        [Route("edit/{id:int}")]
        public async Task<ActionResult> Edit(int id, CancellationToken token = default)
        {
            return await ExecuteSafe(async () =>
            {
                var entity = await _modelsService.GetByIdAsync(id, token);
                if (entity == null) return HttpNotFound();

                var vm = ManufacturerModelViewModel.FromEntity(entity);
                vm.Manufacturers = await LoadManufacturersAsync(token);
                return View(vm);

            },
            onError: _ => View(new ManufacturerModelViewModel()));
        }

        #endregion

        #region Edit (POST)

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit")]
        public async Task<ActionResult> Edit(ManufacturerModelViewModel vm, CancellationToken token = default)
        {
            return await ExecuteSafe(async () =>
            {
                if (!ModelState.IsValid)
                {
                    vm.Manufacturers = await LoadManufacturersAsync(token);
                    return View(vm);
                }

                await _modelsService.UpdateAsync(vm.ToEntity(), token);
                return RedirectToAction("Index");
            },
            onError: async _ =>
            {
                vm.Manufacturers = await LoadManufacturersAsync(token);
                return View(vm);
            });
        }

        #endregion

        #region Delete (GET)

        [HttpGet]
        [Route("delete/{id:int}")]
        public async Task<ActionResult> Delete(int id, CancellationToken token = default)
        {
            return await ExecuteSafe(async () =>
            {
                var entity = await _modelsService.GetByIdAsync(id, token);
                if (entity == null) return HttpNotFound();
                return View(entity);
            },
            onError: _ => View(new ManufacturerModel()));
        }

        #endregion

        #region Delete (POST)

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Route("delete")]
        public async Task<ActionResult> DeleteConfirmed(int id, CancellationToken token = default)
        {
            return await ExecuteSafe(async () =>
            {
                var result = await _modelsService.DeleteAsync(id, token);
                if (!result.Success)
                    return RedirectToAction("DeleteFailed", new { id });

                return RedirectToAction("Index");
            },
            onError: async _ => View(await _modelsService.GetByIdAsync(id, token)));
        }

        #endregion

        #region Delete Failed

        [HttpGet]
        [Route("delete-failed/{id:int}")]
        public async Task<ActionResult> DeleteFailed(int id, CancellationToken token = default)
        {
            return await ExecuteSafe(async () =>
            {
                var entity = await _modelsService.GetByIdAsync(id, token);
                if (entity == null) return HttpNotFound();
                return View(entity);
            },
            onError: _ => View(new ManufacturerModel()));
        }

        [HttpPost, ActionName("DeleteFailed")]
        [ValidateAntiForgeryToken]
        [Route("delete-failed")]
        public ActionResult DeleteFailedProceed(int id)
        {
            return RedirectToAction("DeleteCollective", new { id });
        }

        #endregion

        #region Delete Collective

        [HttpGet]
        [Route("delete-collective/{id:int}")]
        public async Task<ActionResult> DeleteCollective(int id, CancellationToken token = default)
        {
            return await ExecuteSafe(async () =>
            {
                var entity = await _modelsService.GetByIdAsync(id, token);
                if (entity == null) return HttpNotFound();
                return View(entity);
            },
            onError: _ => View(new ManufacturerModel()));
        }

        [HttpPost, ActionName("DeleteCollective")]
        [ValidateAntiForgeryToken]
        [Route("delete-collective")]
        public async Task<ActionResult> DeleteCollectiveConfirmed(int id, CancellationToken token = default)
        {
            return await ExecuteSafe(async () =>
            {
                await _modelsService.DeleteCollectiveAsync(id, token);
                return RedirectToAction("Index");
            },
            onError: async _ => View(await _modelsService.GetByIdAsync(id, token)));
        }

        #endregion

        #region AJAX Get Models For Manufacturer

        [AllowAnonymous]
        [HttpGet]
        [Route("ajax/get-models/{manufacturerID}")]
        public async Task<JsonResult> GetModelsForManufacturer(string manufacturerID, CancellationToken token = default)
        {
            return await ExecuteSafe(async () =>
            {
                if (!int.TryParse(manufacturerID, out int id))
                    return Json(null, JsonRequestBehavior.AllowGet);

                var models = await _modelsService.GetByManufacturerIdAsync(id, token);

                var result = models.ConvertAll(m => new SelectListItem
                {
                    Text = m.ManufacturerModelName,
                    Value = m.ManufacturerModelID.ToString()
                });

                return Json(result, JsonRequestBehavior.AllowGet);
            },
            onError: _ => Json(null, JsonRequestBehavior.AllowGet));
        }

        #endregion

        // ==========================================================
        // Helpers
        // ==========================================================

        #region Helper: Load Manufacturers

        private async Task<List<SelectListItem>> LoadManufacturersAsync(CancellationToken token)
        {
            var entities = await _manufacturersService.GetAllAsync(token);

            return entities.ConvertAll(e => new SelectListItem
            {
                Text = e.ManufacturerName,
                Value = e.ManufacturerID.ToString()
            });
        }

        #endregion

        #region Helper: Consistent Error Handling

        private void SetError(string message) => ViewBag.ErrorMessage = message;

        /// <summary>
        /// Wraps controller actions inside a consistent try/catch pattern.
        /// Ensures cleaner code and reusability.
        /// </summary>
        private async Task<ActionResult> ExecuteSafe(
            Func<Task<ActionResult>> action,
            Func<Exception, Task<ActionResult>>? onError = null)
        {
            try
            {
                return await action.Invoke();
            }
            catch (Exception ex)
            {
                // Logging placeholder
                System.Diagnostics.Debug.WriteLine($"[Controller Error] {ex.Message}");

                SetError("An unexpected error occurred.");

                if (onError != null)
                    return await onError.Invoke(ex);

                return new HttpStatusCodeResult(500);
            }
        }

        #endregion
    }
}

