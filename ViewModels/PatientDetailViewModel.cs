using System;

namespace Medicare.ViewModels
{
    public class PatientDetailViewModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public string BloodType { get; set; }
        public string Allergies { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string PhoneNumber { get; set; }
        public int Age => DateTime.Now.Year - DateOfBirth.Year;
    }
}
