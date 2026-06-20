using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Medicare.Models;
using Medicare.Services;
using Medicare.ViewModels;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Medicare.Controllers
{
    [Authorize]
    public class MedicalRecordController : Controller
    {
        private readonly IMedicalRecordService _medicalRecordService;
        private readonly IPatientService _patientService;
        private readonly IDoctorService _doctorService;
        private readonly UserManager<User> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public MedicalRecordController(
            IMedicalRecordService medicalRecordService,
            IPatientService patientService,
            IDoctorService doctorService,
            UserManager<User> userManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _medicalRecordService = medicalRecordService;
            _patientService = patientService;
            _doctorService = doctorService;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> PatientHistory(int patientId)
        {
            var patient = await _patientService.GetByIdAsync(patientId);
            if (patient == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            
            // Authorization check: Patients can only view their own history
            if (User.IsInRole("Patient") && patient.UserId != currentUser.Id)
            {
                return Forbid();
            }

            var history = await _medicalRecordService.GetPatientHistoryAsync(patientId);
            
            ViewBag.PatientName = $"{patient.User.FirstName} {patient.User.LastName}";
            ViewBag.PatientId = patientId;

            return View(history);
        }

        [Authorize(Roles = "Admin,Doctor")]
        [HttpGet]
        public async Task<IActionResult> Create(int patientId)
        {
            var patient = await _patientService.GetByIdAsync(patientId);
            if (patient == null) return NotFound();

            var model = new CreateMedicalRecordViewModel
            {
                PatientId = patientId,
                PatientName = $"{patient.User.FirstName} {patient.User.LastName}"
            };

            return View(model);
        }

        [Authorize(Roles = "Admin,Doctor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateMedicalRecordViewModel model)
        {
            var patient = await _patientService.GetByIdAsync(model.PatientId);
            if (patient == null) return NotFound();

            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                int? doctorId = null;

                if (User.IsInRole("Doctor"))
                {
                    var doctor = await _doctorService.GetByUserIdAsync(currentUser.Id);
                    if (doctor != null)
                    {
                        doctorId = doctor.Id;
                    }
                }

                var record = new MedicalRecord
                {
                    PatientId = model.PatientId,
                    DoctorId = doctorId,
                    Diagnosis = model.Diagnosis,
                    Treatment = model.Treatment
                };

                var prescriptions = new List<Prescription>();
                if (model.Prescriptions != null)
                {
                    foreach (var p in model.Prescriptions)
                    {
                        if (!string.IsNullOrWhiteSpace(p.MedicationName))
                        {
                            prescriptions.Add(new Prescription
                            {
                                MedicationName = p.MedicationName,
                                Dosage = p.Dosage,
                                Frequency = p.Frequency,
                                DurationDays = p.DurationDays
                            });
                        }
                    }
                }

                var docs = new List<MedicalDoc>();
                if (model.Attachments != null && model.Attachments.Any())
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    foreach (var file in model.Attachments)
                    {
                        if (file.Length > 0)
                        {
                            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
                            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(fileStream);
                            }

                            docs.Add(new MedicalDoc
                            {
                                FileName = file.FileName,
                                FilePath = "/uploads/" + uniqueFileName,
                                FileType = file.ContentType
                            });
                        }
                    }
                }

                await _medicalRecordService.AddMedicalRecordAsync(record, prescriptions, docs);
                return RedirectToAction(nameof(PatientHistory), new { patientId = model.PatientId });
            }

            return View(model);
        }
    }
}
