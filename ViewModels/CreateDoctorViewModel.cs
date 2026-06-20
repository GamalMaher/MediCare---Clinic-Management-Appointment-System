using System.ComponentModel.DataAnnotations;

namespace Medicare.ViewModels
{
    public class CreateDoctorViewModel
    {
        [Required(ErrorMessage = "الاسم الأول مطلوب")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "الاسم الأخير مطلوب")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
        public string Email { get; set; }

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "يجب ألا تقل كلمة المرور عن 6 أحرف")]
        public string Password { get; set; }

        [Required(ErrorMessage = "التخصص مطلوب")]
        public int SpecializationId { get; set; }

        [Required(ErrorMessage = "رقم الترخيص مطلوب")]
        [StringLength(50)]
        public string LicenseNumber { get; set; }

        [Required(ErrorMessage = "قيمة الكشف مطلوبة")]
        [Range(0.00, 10000.00, ErrorMessage = "يجب أن تكون القيمة بين 0 و 10000")]
        public decimal ConsultationFee { get; set; }

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [Phone(ErrorMessage = "رقم الهاتف غير صحيح")]
        public string PhoneNumber { get; set; }
    }
}
