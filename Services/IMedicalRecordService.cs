using System.Collections.Generic;
using System.Threading.Tasks;
using Medicare.Models;

namespace Medicare.Services
{
    public interface IMedicalRecordService
    {
        Task<IEnumerable<MedicalRecord>> GetPatientHistoryAsync(int patientId);
        Task<MedicalRecord> GetByIdAsync(int id);
        Task AddMedicalRecordAsync(MedicalRecord record, List<Prescription> prescriptions, List<MedicalDoc> docs);
    }
}
