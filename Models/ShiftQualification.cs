namespace EmployeeScheduling.API.Models
{
    public class ShiftQualification
    {
        public int ShiftQualificationId { get; set; }

        public int ShiftId { get; set; }
        public int QualificationId { get; set; }

        public bool IsRequired { get; set; } = true;

        // Navigation properties
        public virtual Shift Shift { get; set; } = null!;
        public virtual Qualification Qualification { get; set; } = null!;
    }
}
