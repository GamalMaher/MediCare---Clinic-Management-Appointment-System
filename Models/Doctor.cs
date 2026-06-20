using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medicare.Models
{
    public class Doctor
    {
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public int SpecializationId { get; set; }
        [Required]
        [StringLength(50)]
        public string LicenseNumber { get; set; }
        [Required]
        [Range(0.01, 10000)]
        public decimal ConsultationFee { get; set; }
        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        [ForeignKey("SpecializationId")]
        public virtual Specialization Specialization { get; set; }
        public virtual ICollection<Appointment> Appointments { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
        public virtual ICollection<WorkingHours> WorkingHours { get; set; }
    }
}
