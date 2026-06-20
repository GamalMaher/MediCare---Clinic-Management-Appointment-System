using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Medicare.Data;
using Medicare.Services;
using Medicare.Models;
using Medicare.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Medicare.Controllers
{
    [Authorize]
    public class AppointmentController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IDoctorService _doctorService;
        private readonly IPatientService _patientService;
        private readonly UserManager<User> _userManager;
        private readonly IWorkingHoursService _workingHoursService;
        private readonly ApplicationDbContext _context;

        public AppointmentController(
            IAppointmentService appointmentService,
            IDoctorService doctorService,
            IPatientService patientService,
            UserManager<User> userManager,
            IWorkingHoursService workingHoursService,
            ApplicationDbContext context)
        {
            _appointmentService = appointmentService;
            _doctorService = doctorService;
            _patientService = patientService;
            _userManager = userManager;
            _workingHoursService = workingHoursService;
            _context = context;
        }

        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> MyAppointments()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var patient = await _patientService.GetByUserIdAsync(currentUser.Id);

            if (patient == null)
                return NotFound("Patient profile not found");

            var appointments = await _appointmentService.GetPatientAppointmentsAsync(patient.Id);
            return View(appointments);
        }

        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> DoctorAppointments()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var doctor = await _doctorService.GetByUserIdAsync(currentUser.Id);

            if (doctor == null)
                return NotFound("Doctor profile not found");

            var appointments = await _appointmentService.GetDoctorAppointmentsAsync(doctor.Id);
            return View(appointments);
        }

        [Authorize(Roles = "Patient")]
        [HttpGet]
        public async Task<IActionResult> Book(int doctorId)
        {
            var doctor = await _doctorService.GetByIdAsync(doctorId);
            if (doctor == null)
                return NotFound();

            var workingHours = await _workingHoursService.GetDoctorWorkingHoursAsync(doctorId);

            var model = new BookAppointmentViewModel
            {
                DoctorId = doctorId,
                DoctorName = $"{doctor.User.FirstName} {doctor.User.LastName}",
                MinDate = DateTime.Now.AddDays(1),
                WorkingHours = workingHours.ToList()
            };

            return View(model);
        }

        [Authorize(Roles = "Patient")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(BookAppointmentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var patient = await _patientService.GetByUserIdAsync(currentUser.Id);

                if (patient == null)
                    return NotFound("Patient profile not found");

                // Check doctor availability
                var isAvailable = await _workingHoursService.IsDoctorAvailableAsync(model.DoctorId, model.AppointmentDate, model.StartTime);
                if (!isAvailable)
                {
                    ModelState.AddModelError("", "عذراً، الطبيب غير متاح في هذا الوقت أو اليوم، أو أن هناك حجزاً متعارضاً بالفعل.");
                    var workingHoursList = await _workingHoursService.GetDoctorWorkingHoursAsync(model.DoctorId);
                    model.WorkingHours = workingHoursList.ToList();
                    return View(model);
                }

                var appointment = new Appointment
                {
                    PatientId = patient.Id,
                    DoctorId = model.DoctorId,
                    AppointmentDate = model.AppointmentDate,
                    StartTime = model.StartTime,
                    Reason = model.Reason ?? string.Empty
                };

                await _appointmentService.AddAsync(appointment);
                return RedirectToAction(nameof(MyAppointments));
            }

            var workingHours = await _workingHoursService.GetDoctorWorkingHoursAsync(model.DoctorId);
            model.WorkingHours = workingHours.ToList();
            return View(model);
        }

        [Authorize(Roles = "Patient")]
        [HttpGet]
        public async Task<IActionResult> Cancel(int id)
        {
            var appointment = await _appointmentService.GetByIdAsync(id);
            if (appointment == null)
                return NotFound();

            return View(appointment);
        }

        [Authorize(Roles = "Patient")]
        [HttpPost, ActionName("Cancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(int id)
        {
            var appointment = await _appointmentService.GetByIdAsync(id);
            if (appointment == null)
                return NotFound();

            await _appointmentService.UpdateStatusAsync(id, AppointmentStatus.Cancelled);
            return RedirectToAction(nameof(MyAppointments));
        }

        [Authorize(Roles = "Admin,Doctor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, AppointmentStatus status)
        {
            var appointment = await _appointmentService.GetByIdAsync(id);
            if (appointment == null)
                return NotFound();

            if (User.IsInRole("Doctor"))
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var doctor = await _doctorService.GetByUserIdAsync(currentUser.Id);
                if (doctor == null || appointment.DoctorId != doctor.Id)
                {
                    return Forbid();
                }
            }

            await _appointmentService.UpdateStatusAsync(id, status);
            return RedirectToAction(nameof(DoctorAppointments));
        }

        [Authorize(Roles = "Doctor,Admin")]
        [HttpGet]
        public async Task<IActionResult> GetPatientSummary(int patientId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            int doctorId = 0;
            if (User.IsInRole("Doctor"))
            {
                var doctor = await _doctorService.GetByUserIdAsync(currentUser.Id);
                if (doctor == null) return Unauthorized();
                doctorId = doctor.Id;
            }

            // Count total completed visits of this patient in our hospital
            int totalVisits = await _context.Appointments
                .CountAsync(a => a.PatientId == patientId && a.Status == AppointmentStatus.Completed);

            // Latest medical record diagnosis
            var latestRecord = await _context.MedicalRecords
                .Where(r => r.PatientId == patientId)
                .OrderByDescending(r => r.CreatedDate)
                .Select(r => new { r.Diagnosis, r.Treatment, CreatedDate = r.CreatedDate })
                .FirstOrDefaultAsync();

            return Json(new {
                visitCount = totalVisits,
                latestDiagnosis = latestRecord?.Diagnosis ?? "لا يوجد سجل تشخيص سابق",
                latestTreatment = latestRecord?.Treatment ?? "لا يوجد علاج مسجل",
                latestRecordDate = latestRecord != null ? latestRecord.CreatedDate.ToString("yyyy-MM-dd") : ""
            });
        }
    }
}
