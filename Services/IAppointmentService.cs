using Medicare.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Medicare.Services
{
    public interface IAppointmentService
    {
        Task<IEnumerable<Appointment>> GetPatientAppointmentsAsync(int patientId);
        Task<IEnumerable<Appointment>> GetDoctorAppointmentsAsync(int doctorId);
        Task<Appointment> GetByIdAsync(int id);
        Task AddAsync(Appointment appointment);
        Task UpdateStatusAsync(int id, AppointmentStatus status);
        Task DeleteAsync(int id);
    }
}
