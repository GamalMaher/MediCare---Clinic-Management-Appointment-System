using Medicare.Data;
using Medicare.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Medicare.Services
{
    public class WorkingHoursService : IWorkingHoursService
    {
        private readonly ApplicationDbContext _context;

        public WorkingHoursService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<WorkingHours>> GetDoctorWorkingHoursAsync(int doctorId)
        {
            return await _context.WorkingHours
                .Where(w => w.DoctorId == doctorId)
                .OrderBy(w => w.DayOfWeek)
                .ToListAsync();
        }

        public async Task<WorkingHours> GetByIdAsync(int id)
        {
            return await _context.WorkingHours
                .FirstOrDefaultAsync(w => w.Id == id);
        }

        public async Task AddOrUpdateWorkingHoursAsync(WorkingHours workingHours)
        {
            var existing = await _context.WorkingHours
                .FirstOrDefaultAsync(w => w.DoctorId == workingHours.DoctorId && w.DayOfWeek == workingHours.DayOfWeek);

            if (existing != null)
            {
                existing.StartTime = workingHours.StartTime;
                existing.EndTime = workingHours.EndTime;
                _context.WorkingHours.Update(existing);
            }
            else
            {
                _context.WorkingHours.Add(workingHours);
            }
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var workingHours = await GetByIdAsync(id);
            if (workingHours != null)
            {
                _context.WorkingHours.Remove(workingHours);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsDoctorAvailableAsync(int doctorId, DateTime date, TimeSpan time)
        {
            // 1. Check if the day of week matches doctor's working hours
            var workingDay = await _context.WorkingHours
                .FirstOrDefaultAsync(w => w.DoctorId == doctorId && w.DayOfWeek == date.DayOfWeek);

            if (workingDay == null)
            {
                return false; // Doctor doesn't work on this day
            }

            // 2. Check if the time falls within working hours
            if (time < workingDay.StartTime || time > workingDay.EndTime)
            {
                return false; // Time is outside working hours
            }

            // 3. Check for existing overlapping appointments (excluding Cancelled)
            var hasConflict = await _context.Appointments
                .AnyAsync(a => a.DoctorId == doctorId 
                               && a.AppointmentDate.Date == date.Date 
                               && a.StartTime == time 
                               && a.Status != AppointmentStatus.Cancelled);

            return !hasConflict;
        }
    }
}
