using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Medicare.Services;
using Medicare.Models;
using Medicare.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Medicare.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IAppointmentService _appointmentService;
        private readonly IPatientService _patientService;
        private readonly IDoctorService _doctorService;
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public PaymentController(
            IPaymentService paymentService,
            IAppointmentService appointmentService,
            IPatientService patientService,
            IDoctorService doctorService,
            UserManager<User> userManager,
            ApplicationDbContext context)
        {
            _paymentService = paymentService;
            _appointmentService = appointmentService;
            _patientService = patientService;
            _doctorService = doctorService;
            _userManager = userManager;
            _context = context;
        }

        // GET: Patient pays for an appointment
        [Authorize(Roles = "Patient")]
        [HttpGet]
        public async Task<IActionResult> Pay(int appointmentId)
        {
            var appointment = await _appointmentService.GetByIdAsync(appointmentId);
            if (appointment == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            var patient = await _patientService.GetByUserIdAsync(currentUser.Id);
            if (patient == null || appointment.PatientId != patient.Id)
                return Forbid();

            // Check if already paid
            var existingPayment = await _paymentService.GetByAppointmentIdAsync(appointmentId);
            if (existingPayment != null && existingPayment.Status == PaymentStatus.Completed)
            {
                TempData["InfoMessage"] = "تم سداد هذا الموعد مسبقاً.";
                return RedirectToAction("MyAppointments", "Appointment");
            }

            var model = new Payment
            {
                AppointmentId = appointmentId,
                Amount = appointment.Doctor.ConsultationFee,
                Status = PaymentStatus.Pending,
                PaymentMethod = "بطاقة ائتمان"
            };

            ViewBag.DoctorName = $"د. {appointment.Doctor.User.FirstName} {appointment.Doctor.User.LastName}";
            ViewBag.DoctorSpecialization = appointment.Doctor.Specialization?.Name;
            ViewBag.AppointmentDate = appointment.AppointmentDate.ToString("yyyy-MM-dd");
            ViewBag.AppointmentTime = appointment.StartTime.ToString(@"hh\:mm");

            return View(model);
        }

        // POST: Process the payment
        [Authorize(Roles = "Patient")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(Payment model)
        {
            if (ModelState.IsValid)
            {
                var appointment = await _appointmentService.GetByIdAsync(model.AppointmentId);
                if (appointment == null) return NotFound();

                var currentUser = await _userManager.GetUserAsync(User);
                var patient = await _patientService.GetByUserIdAsync(currentUser.Id);
                if (patient == null || appointment.PatientId != patient.Id)
                    return Forbid();

                // Simulate successful payment
                model.Status = PaymentStatus.Completed;
                await _paymentService.CreatePaymentAsync(model);

                TempData["SuccessMessage"] = $"✅ تم سداد مبلغ {model.Amount:F2} جنيه بنجاح! رقم المعاملة: {model.TransactionId}";
                return RedirectToAction("Success", new { appointmentId = model.AppointmentId });
            }

            var appt = await _appointmentService.GetByIdAsync(model.AppointmentId);
            if (appt != null)
            {
                ViewBag.DoctorName = $"د. {appt.Doctor.User.FirstName} {appt.Doctor.User.LastName}";
                ViewBag.AppointmentDate = appt.AppointmentDate.ToString("yyyy-MM-dd");
            }
            return View(model);
        }

        // GET: Payment Success page
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> Success(int appointmentId)
        {
            var payment = await _paymentService.GetByAppointmentIdAsync(appointmentId);
            if (payment == null) return NotFound();
            return View(payment);
        }

        // GET: Admin - All transactions list
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var payments = await _paymentService.GetAllPaymentsAsync();
            return View(payments);
        }

        // GET: Admin - Dashboard with statistics and charts
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Dashboard()
        {
            var totalRevenue = await _paymentService.GetTotalRevenueAsync();
            var completedPayments = await _paymentService.GetPaymentCountByStatusAsync(PaymentStatus.Completed);
            var pendingPayments = await _paymentService.GetPaymentCountByStatusAsync(PaymentStatus.Pending);
            var failedPayments = await _paymentService.GetPaymentCountByStatusAsync(PaymentStatus.Failed);

            var totalDoctors = await _context.Doctors.CountAsync();
            var totalPatients = await _context.Patients.CountAsync();
            var totalAppointments = await _context.Appointments.CountAsync();
            var completedAppointments = await _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Completed);

            // Monthly revenue for the last 6 months
            var sixMonthsAgo = DateTime.Now.AddMonths(-5);
            var monthlyRevenue = await _context.Payments
                .Where(p => p.Status == PaymentStatus.Completed && p.PaymentDate >= sixMonthsAgo)
                .GroupBy(p => new { p.PaymentDate.Year, p.PaymentDate.Month })
                .Select(g => new {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Total = g.Sum(p => p.Amount)
                })
                .OrderBy(g => g.Year).ThenBy(g => g.Month)
                .ToListAsync();

            // Revenue per specialization
            var revenueBySpec = await _context.Payments
                .Where(p => p.Status == PaymentStatus.Completed)
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.Doctor)
                        .ThenInclude(d => d.Specialization)
                .GroupBy(p => p.Appointment.Doctor.Specialization.Name)
                .Select(g => new { Specialization = g.Key, Total = g.Sum(p => p.Amount) })
                .ToListAsync();

            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.CompletedPayments = completedPayments;
            ViewBag.PendingPayments = pendingPayments;
            ViewBag.FailedPayments = failedPayments;
            ViewBag.TotalDoctors = totalDoctors;
            ViewBag.TotalPatients = totalPatients;
            ViewBag.TotalAppointments = totalAppointments;
            ViewBag.CompletedAppointments = completedAppointments;

            // Serialize for chart
            ViewBag.MonthlyLabels = System.Text.Json.JsonSerializer.Serialize(
                monthlyRevenue.Select(m => $"{m.Month}/{m.Year}").ToList());
            ViewBag.MonthlyData = System.Text.Json.JsonSerializer.Serialize(
                monthlyRevenue.Select(m => m.Total).ToList());
            ViewBag.SpecLabels = System.Text.Json.JsonSerializer.Serialize(
                revenueBySpec.Select(s => s.Specialization).ToList());
            ViewBag.SpecData = System.Text.Json.JsonSerializer.Serialize(
                revenueBySpec.Select(s => s.Total).ToList());

            return View();
        }
    }
}
