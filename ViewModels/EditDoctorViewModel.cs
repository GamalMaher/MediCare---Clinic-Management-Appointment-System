using System.ComponentModel.DataAnnotations;

namespace Medicare.ViewModels
{
    public class EditDoctorViewModel
    {
        public int Id { get; set; }
        [Required]
        public int SpecializationId { get; set; }
        [Required]
        [Range(0.01, 10000)]
        public decimal ConsultationFee { get; set; }
        [Required]
        [Phone]
        public string PhoneNumber { get; set; }
    }
}
