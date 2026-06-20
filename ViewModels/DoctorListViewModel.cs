using System.Collections.Generic;

namespace Medicare.ViewModels
{
    public class DoctorListViewModel
    {
        public List<DoctorItemViewModel> Doctors { get; set; }
        public int TotalDoctors { get; set; }
        public int SpecializationFilter { get; set; }
        public string SearchTerm { get; set; }
    }
}
