using System;
using System.ComponentModel.DataAnnotations;

namespace Medicare.ViewModels
{
    public class EditPatientViewModel
    {
        public int Id { get; set; }
        [Required]
        [StringLength(10)]
        public string BloodType { get; set; }
        [StringLength(500)]
        public string Allergies { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }
        [Required]
        [Phone]
        public string PhoneNumber { get; set; }
    }
}
