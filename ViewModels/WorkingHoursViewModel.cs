using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Medicare.ViewModels
{
    public class DayWorkingHoursViewModel
    {
        public DayOfWeek DayOfWeek { get; set; }
        public string? DayName { get; set; }
        public bool IsWorking { get; set; }
        public string? StartTime { get; set; } // e.g. "09:00"
        public string? EndTime { get; set; }   // e.g. "17:00"
    }

    public class WorkingHoursViewModel
    {
        public int DoctorId { get; set; }
        public string? DoctorName { get; set; }
        public List<DayWorkingHoursViewModel> Days { get; set; } = new List<DayWorkingHoursViewModel>();
    }
}
