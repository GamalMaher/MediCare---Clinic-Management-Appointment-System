using Medicare.Data;
using Medicare.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Medicare.Services
{
    public class ReviewService : IReviewService
    {
        private readonly ApplicationDbContext _context;

        public ReviewService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Review>> GetDoctorReviewsAsync(int doctorId)
        {
            return await _context.Reviews
                .Where(r => r.DoctorId == doctorId)
                .Include(r => r.Patient)
                    .ThenInclude(p => p.User)
                .OrderByDescending(r => r.ReviewDate)
                .ToListAsync();
        }

        public async Task AddReviewAsync(Review review)
        {
            review.ReviewDate = DateTime.Now;
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasPatientReviewedDoctorAsync(int patientId, int doctorId, int? appointmentId = null)
        {
            if (appointmentId.HasValue)
            {
                return await _context.Reviews.AnyAsync(r => r.PatientId == patientId && r.DoctorId == doctorId && r.AppointmentId == appointmentId.Value);
            }
            return await _context.Reviews.AnyAsync(r => r.PatientId == patientId && r.DoctorId == doctorId);
        }
    }
}
