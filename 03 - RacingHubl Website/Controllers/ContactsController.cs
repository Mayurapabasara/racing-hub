using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using RacingHubCarRental.Services.Interfaces;
using RacingHubCarRental.ViewModels;
using RacingHubCarRental.Models;

namespace RacingHubCarRental.Controllers
{
    /// <summary>
    /// Handles all Contact-Us related actions including submit, admin review, and unread updates.
    /// Fully rewritten using async, DI, SOLID, structured commits, and clean architecture.
    /// </summary>
    public class ContactsController : Controller
    {
        // ============================================================
        // Dependencies via Constructor Injection
        // ============================================================

        private readonly IContactsService _contacts;

        public ContactsController(IContactsService contacts)
        {
            _contacts = contacts;
        }

        // ============================================================
        // Helper: Map Model -> ViewModel
        // ============================================================

        private ContactViewModel MapToVm(Contact contact)
        {
            return new ContactViewModel
            {
                ContactID = contact.ContactID,
                Name = contact.Name,
                Email = contact.Email,
                Message = contact.Message,
                DateTime = contact.DateTime,
                IsUnread = contact.IsUnread
            };
        }

        private List<ContactViewModel> MapList(IEnumerable<Contact> list)
        {
            return list.Select(MapToVm).ToList();
        }

        // ============================================================
        // ADMIN CONTACT LIST
        // ============================================================

        [Authorize(Roles = "Admin, Manager")]
        public async Task<ActionResult> Watch()
        {
            try
            {
                var all = await _contacts.GetAllAsync();
                var vm = MapList(all.OrderByDescending(c => c.DateTime));
                return View(vm);
            }
            catch
            {
                ViewBag.ErrorMessage = "Failed to load contact messages.";
                return View(new List<ContactViewModel>());
            }
        }

        // ============================================================
        // CREATE CONTACT MESSAGE (GET)
        // ============================================================

        public ActionResult Create()
        {
            return View(new ContactViewModel());
        }

        // ============================================================
        // CREATE CONTACT MESSAGE (AJAX POST)
        // ============================================================

        [HttpPost]
        public async Task<JsonResult> SubmitContact(ContactViewModel vm)
        {
            if (!ModelState.IsValid)
                return Json(false);

            try
            {
                var model = new Contact
                {
                    Name = vm.Name,
                    Email = vm.Email,
                    Message = vm.Message,
                    IsUnread = true,
                    DateTime = DateTime.Now
                };

                await _contacts.CreateAsync(model);
                return Json(true);
            }
            catch
            {
                return Json(false);
            }
        }

        // ============================================================
        // MARK MESSAGE AS READ (AJAX)
        // ============================================================

        [HttpPost]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<JsonResult> UpdateUnreadContact(int contactID)
        {
            try
            {
                bool result = await _contacts.MarkAsReadAsync(contactID);
                return Json(result);
            }
            catch
            {
                return Json(false);
            }
        }
    }
}

