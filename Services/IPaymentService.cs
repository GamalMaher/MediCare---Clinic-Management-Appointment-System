using Medicare.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Medicare.Services
{
    public interface IPaymentService
    {
        Task<Payment> GetByAppointmentIdAsync(int appointmentId);
        Task<IEnumerable<Payment>> GetAllPaymentsAsync();
        Task CreatePaymentAsync(Payment payment);
        Task<decimal> GetTotalRevenueAsync();
        Task<int> GetPaymentCountByStatusAsync(PaymentStatus status);
    }
}
