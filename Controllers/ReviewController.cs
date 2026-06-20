using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Medicare.Services;
using Medicare.Models;
using System;
using System.Threading.Tasks;

namespace Medicare.Controllers
{
    [Authorize(Roles = "Patient")]
    public class ReviewController : Controller
    {
        private readonly IReviewService _reviewService;
        private readonly IAppointmentService _appointmentService;
        private readonly IPatientService _patientService;
        private readonly UserManager<User> _userManager;

        public ReviewController(
            IReviewService reviewService,
            IAppointmentService appointmentService,
            IPatientService patientService,
            UserManager<User> userManager)
        {
            _reviewService = reviewService;
            _appointmentService = appointmentService;
            _patientService = patientService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int appointmentId)
        {
            var appointment = await _appointmentService.GetByIdAsync(appointmentId);
            if (appointment == null)
            {
                return NotFound("الموعد غير موجود.");
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var patient = await _patientService.GetByUserIdAsync(currentUser.Id);
            if (patient == null || appointment.PatientId != patient.Id)
            {
                return Forbid("لا تملك صلاحية تقييم هذا الموعد.");
            }

            // Check if already reviewed
            var alreadyReviewed = await _reviewService.HasPatientReviewedDoctorAsync(patient.Id, appointment.DoctorId, appointment.Id);
            if (alreadyReviewed)
            {
                TempData["ErrorMessage"] = "لقد قمت بتقييم هذا الموعد مسبقاً.";
                return RedirectToAction("MyAppointments", "Appointment");
            }

            var model = new Review
            {
                AppointmentId = appointment.Id,
                DoctorId = appointment.DoctorId,
                PatientId = patient.Id,
                Rating = 5 // Default rating
            };

            ViewBag.DoctorName = $"د. {appointment.Doctor.User.FirstName} {appointment.Doctor.User.LastName}";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Review model)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var patient = await _patientService.GetByUserIdAsync(currentUser.Id);
                if (patient == null || model.PatientId != patient.Id)
                {
                    return Forbid();
                }

                // Check again to prevent duplicate submissions
                var alreadyReviewed = await _reviewService.HasPatientReviewedDoctorAsync(patient.Id, model.DoctorId, model.AppointmentId);
                if (alreadyReviewed)
                {
                    TempData["ErrorMessage"] = "لقد قمت بتقييم هذا الموعد مسبقاً.";
                    return RedirectToAction("MyAppointments", "Appointment");
                }

                await _reviewService.AddReviewAsync(model);
                TempData["SuccessMessage"] = "تم إضافة تقييمك بنجاح. شكراً لك!";
                return RedirectToAction("MyAppointments", "Appointment");
            }

            var appointment = await _appointmentService.GetByIdAsync(model.AppointmentId ?? 0);
            if (appointment != null)
            {
                ViewBag.DoctorName = $"د. {appointment.Doctor.User.FirstName} {appointment.Doctor.User.LastName}";
            }
            return View(model);
        }
    }
}
