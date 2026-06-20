namespace Medicare.ViewModels
{
    public class DoctorItemViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Specialization { get; set; }
        public decimal ConsultationFee { get; set; }
        public double AverageRating { get; set; }
    }
}
