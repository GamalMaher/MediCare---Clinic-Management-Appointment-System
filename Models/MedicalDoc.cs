using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medicare.Models
{
    public class MedicalDoc
    {
        public int Id { get; set; }
        [Required]
        public int MedicalRecordId { get; set; }
        [Required]
        [StringLength(255)]
        public string FileName { get; set; }
        [Required]
        [StringLength(500)]
        public string FilePath { get; set; }
        [StringLength(50)]
        public string FileType { get; set; }
        public DateTime UploadDate { get; set; }

        [ForeignKey("MedicalRecordId")]
        public virtual MedicalRecord MedicalRecord { get; set; }
    }
}
