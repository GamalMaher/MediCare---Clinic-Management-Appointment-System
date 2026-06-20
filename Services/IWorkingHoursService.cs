using System.Collections.Generic;
using System.Threading.Tasks;
using Medicare.Models;

namespace Medicare.Services
{
    public interface IWorkingHoursService
    {
        Task<IEnumerable<WorkingHours>> GetDoctorWorkingHoursAsync(int doctorId);
        Task<WorkingHours> GetByIdAsync(int id);
        Task AddOrUpdateWorkingHoursAsync(WorkingHours workingHours);
        Task DeleteAsync(int id);
        Task<bool> IsDoctorAvailableAsync(int doctorId, System.DateTime date, System.TimeSpan time);
    }
}
