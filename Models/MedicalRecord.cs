using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medicare.Models
{
    public class MedicalRecord
    {
        public int Id { get; set; }
        [Required]
        public int PatientId { get; set; }
        public int? DoctorId { get; set; }
        [Required]
        [StringLength(500)]
        public string Diagnosis { get; set; }
        [Required]
        [StringLength(1000)]
        public string Treatment { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        [ForeignKey("PatientId")]
        public virtual Patient Patient { get; set; }
        [ForeignKey("DoctorId")]
        public virtual Doctor Doctor { get; set; }
        public virtual ICollection<Prescription> Prescriptions { get; set; }
        public virtual ICollection<MedicalDoc> MedicalDocs { get; set; }
    }
}
