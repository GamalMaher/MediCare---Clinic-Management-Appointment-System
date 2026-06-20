using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medicare.Models
{
    public class Prescription
    {
        public int Id { get; set; }
        [Required]
        public int MedicalRecordId { get; set; }
        [Required]
        [StringLength(100)]
        public string MedicationName { get; set; }
        [Required]
        [StringLength(100)]
        public string Dosage { get; set; }
        [Required]
        [StringLength(100)]
        public string Frequency { get; set; }
        [Required]
        public int DurationDays { get; set; }
        public DateTime CreatedDate { get; set; }

        [ForeignKey("MedicalRecordId")]
        public virtual MedicalRecord MedicalRecord { get; set; }
    }
}
