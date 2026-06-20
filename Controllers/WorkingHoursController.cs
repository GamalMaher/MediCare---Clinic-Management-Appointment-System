using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Medicare.Models;
using Medicare.Services;
using Medicare.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Medicare.Controllers
{
    [Authorize(Roles = "Admin,Doctor")]
    public class WorkingHoursController : Controller
    {
        private readonly IWorkingHoursService _workingHoursService;
        private readonly IDoctorService _doctorService;
        private readonly UserManager<User> _userManager;

        private static readonly Dictionary<DayOfWeek, string> DayNamesArabic = new Dictionary<DayOfWeek, string>
        {
            { DayOfWeek.Sunday, "الأحد" },
            { DayOfWeek.Monday, "الاثنين" },
            { DayOfWeek.Tuesday, "الثلاثاء" },
            { DayOfWeek.Wednesday, "الأربعاء" },
            { DayOfWeek.Thursday, "الخميس" },
            { DayOfWeek.Friday, "الجمعة" },
            { DayOfWeek.Saturday, "السبت" }
        };

        public WorkingHoursController(
            IWorkingHoursService workingHoursService,
            IDoctorService doctorService,
            UserManager<User> userManager)
        {
            _workingHoursService = workingHoursService;
            _doctorService = doctorService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int? doctorId)
        {
            int selectedDoctorId = 0;
            Doctor doctor = null;

            if (User.IsInRole("Doctor"))
            {
                var user = await _userManager.GetUserAsync(User);
                doctor = await _doctorService.GetByUserIdAsync(user.Id);
                if (doctor == null) return NotFound("Doctor profile not found.");
                selectedDoctorId = doctor.Id;
            }
            else if (User.IsInRole("Admin"))
            {
                if (doctorId == null) return RedirectToAction("Index", "Doctor");
                selectedDoctorId = doctorId.Value;
                doctor = await _doctorService.GetByIdAsync(selectedDoctorId);
                if (doctor == null) return NotFound();
            }

            var workingHours = await _workingHoursService.GetDoctorWorkingHoursAsync(selectedDoctorId);
            ViewBag.DoctorName = $"د. {doctor.User.FirstName} {doctor.User.LastName}";
            ViewBag.DoctorId = selectedDoctorId;
            ViewBag.DayNames = DayNamesArabic;

            return View(workingHours);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int doctorId)
        {
            if (User.IsInRole("Doctor"))
            {
                var user = await _userManager.GetUserAsync(User);
                var doctorProfile = await _doctorService.GetByUserIdAsync(user.Id);
                if (doctorProfile == null || doctorProfile.Id != doctorId)
                {
                    return Forbid();
                }
            }

            var doctor = await _doctorService.GetByIdAsync(doctorId);
            if (doctor == null) return NotFound();

            var existingHours = (await _workingHoursService.GetDoctorWorkingHoursAsync(doctorId)).ToList();

            var model = new WorkingHoursViewModel
            {
                DoctorId = doctorId,
                DoctorName = $"د. {doctor.User.FirstName} {doctor.User.LastName}"
            };

            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
            {
                var matched = existingHours.FirstOrDefault(h => h.DayOfWeek == day);
                model.Days.Add(new DayWorkingHoursViewModel
                {
                    DayOfWeek = day,
                    DayName = DayNamesArabic[day],
                    IsWorking = matched != null,
                    StartTime = matched != null ? matched.StartTime.ToString(@"hh\:mm") : "09:00",
                    EndTime = matched != null ? matched.EndTime.ToString(@"hh\:mm") : "17:00"
                });
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(WorkingHoursViewModel model)
        {
            if (User.IsInRole("Doctor"))
            {
                var user = await _userManager.GetUserAsync(User);
                var doctorProfile = await _doctorService.GetByUserIdAsync(user.Id);
                if (doctorProfile == null || doctorProfile.Id != model.DoctorId)
                {
                    return Forbid();
                }
            }

            // Remove validation errors for StartTime, EndTime, and DayName if the day is not active
            for (int i = 0; i < model.Days.Count; i++)
            {
                if (!model.Days[i].IsWorking)
                {
                    ModelState.Remove($"Days[{i}].StartTime");
                    ModelState.Remove($"Days[{i}].EndTime");
                    ModelState.Remove($"Days[{i}].DayName");
                }
            }
            ModelState.Remove("DoctorName");

            if (ModelState.IsValid)
            {
                var existingHours = (await _workingHoursService.GetDoctorWorkingHoursAsync(model.DoctorId)).ToList();

                foreach (var dayModel in model.Days)
                {
                    var existing = existingHours.FirstOrDefault(h => h.DayOfWeek == dayModel.DayOfWeek);

                    if (dayModel.IsWorking)
                    {
                        if (string.IsNullOrEmpty(dayModel.StartTime) || string.IsNullOrEmpty(dayModel.EndTime))
                        {
                            ModelState.AddModelError("", $"الرجاء تحديد وقت البدء والانتهاء ليوم {dayModel.DayName}");
                            return View(model);
                        }

                        if (!TimeSpan.TryParse(dayModel.StartTime, out var start) || !TimeSpan.TryParse(dayModel.EndTime, out var end))
                        {
                            ModelState.AddModelError("", $"صيغة الوقت غير صالحة ليوم {dayModel.DayName}");
                            return View(model);
                        }

                        if (start >= end)
                        {
                            ModelState.AddModelError("", $"وقت الانتهاء يجب أن يكون بعد وقت البدء ليوم {dayModel.DayName}");
                            return View(model);
                        }

                        var wh = new WorkingHours
                        {
                            DoctorId = model.DoctorId,
                            DayOfWeek = dayModel.DayOfWeek,
                            StartTime = start,
                            EndTime = end
                        };

                        await _workingHoursService.AddOrUpdateWorkingHoursAsync(wh);
                    }
                    else
                    {
                        if (existing != null)
                        {
                            await _workingHoursService.DeleteAsync(existing.Id);
                        }
                    }
                }

                return RedirectToAction(nameof(Index), new { doctorId = model.DoctorId });
            }

            return View(model);
        }
    }
}
