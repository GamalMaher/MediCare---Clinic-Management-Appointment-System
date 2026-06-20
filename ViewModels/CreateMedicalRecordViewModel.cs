using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Medicare.ViewModels
{
    public class CreatePrescriptionViewModel
    {
        [Required(ErrorMessage = "اسم الدواء مطلوب")]
        public string MedicationName { get; set; }

        [Required(ErrorMessage = "الجرعة مطلوبة")]
        public string Dosage { get; set; }

        [Required(ErrorMessage = "التكرار مطلوب")]
        public string Frequency { get; set; }

        [Required(ErrorMessage = "المدة بالأيام مطلوبة")]
        [Range(1, 365, ErrorMessage = "يجب أن تكون المدة بين 1 و 365 يوماً")]
        public int DurationDays { get; set; }
    }

    public class CreateMedicalRecordViewModel
    {
        public int PatientId { get; set; }
        public string PatientName { get; set; }

        [Required(ErrorMessage = "التشخيص مطلوب")]
        [StringLength(500, ErrorMessage = "التشخيص يجب ألا يتجاوز 500 حرف")]
        public string Diagnosis { get; set; }

        [Required(ErrorMessage = "العلاج الموصوف مطلوب")]
        [StringLength(1000, ErrorMessage = "العلاج الموصوف يجب ألا يتجاوز 1000 حرف")]
        public string Treatment { get; set; }

        public List<CreatePrescriptionViewModel> Prescriptions { get; set; } = new List<CreatePrescriptionViewModel>();

        public List<IFormFile>? Attachments { get; set; }
    }
}
