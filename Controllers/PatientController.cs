using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Medicare.Services;
using Medicare.Models;
using Medicare.ViewModels;
using System.Threading.Tasks;

namespace Medicare.Controllers
{
    [Authorize]
    public class PatientController : Controller
    {
        private readonly IPatientService _patientService;
        private readonly UserManager<User> _userManager;

        public PatientController(
            IPatientService patientService,
            UserManager<User> userManager)
        {
            _patientService = patientService;
            _userManager = userManager;
        }

        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Index()
        {
            var patients = await _patientService.GetAllAsync();
            return View(patients);
        }

        public async Task<IActionResult> Details(int id)
        {
            var patient = await _patientService.GetByIdAsync(id);
            if (patient == null)
                return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (!User.IsInRole("Admin") && patient.UserId != currentUser.Id)
                return Forbid();

            var model = new PatientDetailViewModel
            {
                Id = patient.Id,
                FirstName = patient.User.FirstName,
                LastName = patient.User.LastName,
                BloodType = patient.BloodType,
                Allergies = patient.Allergies,
                DateOfBirth = patient.DateOfBirth,
                PhoneNumber = patient.PhoneNumber
            };

            return View(model);
        }

        [Authorize(Roles = "Admin,Patient")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var patient = await _patientService.GetByIdAsync(id);
            if (patient == null)
                return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (!User.IsInRole("Admin") && patient.UserId != currentUser.Id)
                return Forbid();

            var model = new EditPatientViewModel
            {
                Id = patient.Id,
                BloodType = patient.BloodType,
                Allergies = patient.Allergies,
                DateOfBirth = patient.DateOfBirth,
                PhoneNumber = patient.PhoneNumber
            };

            return View(model);
        }

        [Authorize(Roles = "Admin,Patient")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditPatientViewModel model)
        {
            if (id != model.Id)
                return BadRequest();

            if (ModelState.IsValid)
            {
                var patient = await _patientService.GetByIdAsync(id);
                if (patient == null)
                    return NotFound();

                patient.BloodType = model.BloodType;
                patient.Allergies = model.Allergies;
                patient.DateOfBirth = model.DateOfBirth;
                patient.PhoneNumber = model.PhoneNumber;

                await _patientService.UpdateAsync(patient);
                return RedirectToAction(nameof(Details), new { id });
            }
            return View(model);
        }
    }
}
