using System.ComponentModel.DataAnnotations;

namespace EmployeeScheduling.API.Models
{
    public class Shift
    {
        public int ShiftId { get; set; }

        public int ScheduleId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public int RequiredEmployees { get; set; } = 1;

        public int? LocationId { get; set; }

        // NEW PROPERTIES: Added Department and Position
        [StringLength(100)]
        public string Department { get; set; } = string.Empty;

        [StringLength(100)]
        public string Position { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual Schedule Schedule { get; set; } = null!;
        public virtual Location? Location { get; set; }
        public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
        public virtual ICollection<ShiftQualification> ShiftQualifications { get; set; } = new List<ShiftQualification>();
    }
}

