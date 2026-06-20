using Medicare.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Medicare.Services
{
    public interface IPatientService
    {
        Task<IEnumerable<Patient>> GetAllAsync();
        Task<Patient> GetByIdAsync(int id);
        Task<Patient> GetByUserIdAsync(string userId);
        Task AddAsync(Patient patient);
        Task UpdateAsync(Patient patient);
        Task DeleteAsync(int id);
    }
}
