using System.ComponentModel.DataAnnotations;
using EmployeeScheduling.API.Models;

namespace EmployeeScheduling.API.Models
{
    public class Availability
    {
        public int AvailabilityId { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public DayOfWeek DayOfWeek { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Required]
        public AvailabilityType AvailabilityType { get; set; } = AvailabilityType.Available;

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual Employee Employee { get; set; } = null!;

        // Computed property for backward compatibility
        public bool IsAvailable => AvailabilityType == AvailabilityType.Available || AvailabilityType == AvailabilityType.Preferred;
    }
}
