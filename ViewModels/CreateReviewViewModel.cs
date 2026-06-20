using System.ComponentModel.DataAnnotations;

namespace Medicare.ViewModels
{
    public class CreateReviewViewModel
    {
        public int AppointmentId { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }
        [StringLength(500)]
        public string Comment { get; set; }
    }
}
