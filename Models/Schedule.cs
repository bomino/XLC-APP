using System.ComponentModel.DataAnnotations;

namespace EmployeeScheduling.API.Models
{
    public class Schedule
    {
        public int ScheduleId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public bool IsPublished { get; set; } = false;

        public int CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual User CreatedByUser { get; set; } = null!;
        public virtual ICollection<Shift> Shifts { get; set; } = new List<Shift>();
    }
}
