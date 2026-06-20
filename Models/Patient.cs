using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medicare.Models
{
    public class Patient
    {
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        [StringLength(10)]
        public string BloodType { get; set; }
        [StringLength(500)]
        public string Allergies { get; set; }
        [Required]
        public DateTime DateOfBirth { get; set; }
        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        public virtual ICollection<Appointment> Appointments { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
    }
}
