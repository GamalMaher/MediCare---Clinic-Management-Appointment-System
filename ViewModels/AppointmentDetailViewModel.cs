using System;
using Medicare.Models;

namespace Medicare.ViewModels
{
    public class AppointmentDetailViewModel
    {
        public int Id { get; set; }
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public string Specialization { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public AppointmentStatus Status { get; set; }
        public string Reason { get; set; }
        public DateTime BookedDate { get; set; }
        public decimal ConsultationFee { get; set; }
    }
}
