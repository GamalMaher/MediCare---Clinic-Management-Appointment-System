using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Medicare.Models;
using Medicare.Services;
using Medicare.ViewModels;
using Medicare.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Medicare.Controllers
{
    [Authorize]
    public class DoctorController : Controller
    {
        private readonly IDoctorService _doctorService;
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public DoctorController(
            IDoctorService doctorService,
            UserManager<User> userManager,
            ApplicationDbContext context)
        {
            _doctorService = doctorService;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var doctors = await _doctorService.GetAllAsync();
            return View(doctors);
        }

        public async Task<IActionResult> Details(int id)
        {
            var doctor = await _doctorService.GetByIdAsync(id);
            if (doctor == null)
                return NotFound();

            var workingHours = await _context.WorkingHours
                .Where(w => w.DoctorId == id)
                .OrderBy(w => w.DayOfWeek)
                .ToListAsync();

            var model = new DoctorDetailViewModel
            {
                Id = doctor.Id,
                FirstName = doctor.User.FirstName,
                LastName = doctor.User.LastName,
                Specialization = doctor.Specialization.Name,
                LicenseNumber = doctor.LicenseNumber,
                ConsultationFee = doctor.ConsultationFee,
                PhoneNumber = doctor.PhoneNumber,
                WorkingHours = workingHours
            };

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Specializations = _context.Specializations.ToList();
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateDoctorViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    CreatedDate = DateTime.Now
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Doctor");

                    var doctor = new Doctor
                    {
                        UserId = user.Id,
                        SpecializationId = model.SpecializationId,
                        LicenseNumber = model.LicenseNumber,
                        ConsultationFee = model.ConsultationFee,
                        PhoneNumber = model.PhoneNumber
                    };

                    await _doctorService.AddAsync(doctor);
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            ViewBag.Specializations = _context.Specializations.ToList();
            return View(model);
        }

        [Authorize(Roles = "Admin,Doctor")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var doctor = await _doctorService.GetByIdAsync(id);
            if (doctor == null)
                return NotFound();

            ViewBag.Specializations = _context.Specializations.ToList();
            var model = new EditDoctorViewModel
            {
                Id = doctor.Id,
                SpecializationId = doctor.SpecializationId,
                ConsultationFee = doctor.ConsultationFee,
                PhoneNumber = doctor.PhoneNumber
            };

            return View(model);
        }

        [Authorize(Roles = "Admin,Doctor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditDoctorViewModel model)
        {
            if (id != model.Id)
                return BadRequest();

            if (ModelState.IsValid)
            {
                var doctor = await _doctorService.GetByIdAsync(id);
                doctor.SpecializationId = model.SpecializationId;
                doctor.ConsultationFee = model.ConsultationFee;
                doctor.PhoneNumber = model.PhoneNumber;

                await _doctorService.UpdateAsync(doctor);
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Specializations = _context.Specializations.ToList();
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var doctor = await _doctorService.GetByIdAsync(id);
            if (doctor == null)
                return NotFound();

            return View(doctor);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _doctorService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
