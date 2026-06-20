using Medicare.Data;
using Medicare.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Medicare.Services
{
    public class MedicalRecordService : IMedicalRecordService
    {
        private readonly ApplicationDbContext _context;

        public MedicalRecordService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MedicalRecord>> GetPatientHistoryAsync(int patientId)
        {
            return await _context.MedicalRecords
                .Where(r => r.PatientId == patientId)
                .Include(r => r.Doctor)
                    .ThenInclude(d => d.User)
                .Include(r => r.Prescriptions)
                .Include(r => r.MedicalDocs)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
        }

        public async Task<MedicalRecord> GetByIdAsync(int id)
        {
            return await _context.MedicalRecords
                .Include(r => r.Patient)
                    .ThenInclude(p => p.User)
                .Include(r => r.Doctor)
                    .ThenInclude(d => d.User)
                .Include(r => r.Prescriptions)
                .Include(r => r.MedicalDocs)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task AddMedicalRecordAsync(MedicalRecord record, List<Prescription> prescriptions, List<MedicalDoc> docs)
        {
            record.CreatedDate = DateTime.Now;
            _context.MedicalRecords.Add(record);
            await _context.SaveChangesAsync(); // Generates record.Id

            if (prescriptions != null && prescriptions.Any())
            {
                foreach (var p in prescriptions)
                {
                    p.MedicalRecordId = record.Id;
                    p.CreatedDate = DateTime.Now;
                    _context.Prescriptions.Add(p);
                }
            }

            if (docs != null && docs.Any())
            {
                foreach (var d in docs)
                {
                    d.MedicalRecordId = record.Id;
                    d.UploadDate = DateTime.Now;
                    _context.MedicalDocs.Add(d);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
