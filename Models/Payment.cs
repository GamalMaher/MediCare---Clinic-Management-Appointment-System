using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medicare.Models
{
    public enum PaymentStatus
    {
        Pending,
        Completed,
        Failed,
        Refunded
    }

    public class Payment
    {
        public int Id { get; set; }
        [Required]
        public int AppointmentId { get; set; }
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
        [Required]
        public PaymentStatus Status { get; set; }
        [StringLength(50)]
        public string PaymentMethod { get; set; }
        public DateTime PaymentDate { get; set; }
        [StringLength(100)]
        public string TransactionId { get; set; }

        [ForeignKey("AppointmentId")]
        public virtual Appointment Appointment { get; set; }
    }
}
