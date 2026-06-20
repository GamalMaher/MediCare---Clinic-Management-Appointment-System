using System.Collections.Generic;
using Medicare.Models;

namespace Medicare.ViewModels
{
    public class DoctorDetailViewModel
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public string? Specialization { get; set; }
        public string? LicenseNumber { get; set; }
        public decimal ConsultationFee { get; set; }
        public string? PhoneNumber { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }

        public List<WorkingHours>? WorkingHours { get; set; }
    }
}
