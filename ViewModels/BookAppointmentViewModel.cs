using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Medicare.Models;

namespace Medicare.ViewModels
{
    public class BookAppointmentViewModel
    {
        public int DoctorId { get; set; }
        public string? DoctorName { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; }
        [Required]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }
        [StringLength(500)]
        public string? Reason { get; set; }
        public DateTime MinDate { get; set; }

        public List<WorkingHours>? WorkingHours { get; set; }
    }
}
