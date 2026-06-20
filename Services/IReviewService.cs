using Medicare.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Medicare.Services
{
    public interface IReviewService
    {
        Task<IEnumerable<Review>> GetDoctorReviewsAsync(int doctorId);
        Task AddReviewAsync(Review review);
        Task<bool> HasPatientReviewedDoctorAsync(int patientId, int doctorId, int? appointmentId = null);
    }
}
