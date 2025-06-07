using System.ComponentModel.DataAnnotations;

namespace EmployeeScheduling.API.Models
{
    public class Qualification
    {
        public int QualificationId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<EmployeeQualification> EmployeeQualifications { get; set; } = new List<EmployeeQualification>();
        public virtual ICollection<ShiftQualification> ShiftQualifications { get; set; } = new List<ShiftQualification>();
    }
}

