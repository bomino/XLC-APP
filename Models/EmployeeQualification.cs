namespace EmployeeScheduling.API.Models
{
    public class EmployeeQualification
    {
        public int EmployeeQualificationId { get; set; }

        public int EmployeeId { get; set; }
        public int QualificationId { get; set; }

        public DateTime ObtainedDate { get; set; }
        public DateTime? ExpiryDate { get; set; }

        // Navigation properties
        public virtual Employee Employee { get; set; } = null!;
        public virtual Qualification Qualification { get; set; } = null!;
    }
}
