using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medicare.Models
{
    public class Review
    {
        public int Id { get; set; }
        [Required]
        public int DoctorId { get; set; }
        [Required]
        public int PatientId { get; set; }
        public int? AppointmentId { get; set; }
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }
        [StringLength(500)]
        public string Comment { get; set; }
        [Required]
        public DateTime ReviewDate { get; set; }

        [ForeignKey("DoctorId")]
        public virtual Doctor Doctor { get; set; }
        [ForeignKey("PatientId")]
        public virtual Patient Patient { get; set; }
        [ForeignKey("AppointmentId")]
        public virtual Appointment Appointment { get; set; }
    }
}
