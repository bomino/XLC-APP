using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeScheduling.API.Models
{
    public class Assignment
    {
        public int AssignmentId { get; set; }

        [Required]
        public int ShiftId { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public AssignmentStatus Status { get; set; } = AssignmentStatus.Assigned;

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public int AssignedByUserId { get; set; }

        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? HoursWorked { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? RegularHours { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? OvertimeHours { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? TotalPay { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual Shift Shift { get; set; } = null!;
        public virtual Employee Employee { get; set; } = null!;
        public virtual User AssignedByUser { get; set; } = null!;

        // Computed properties for backward compatibility
        public bool IsConfirmed => Status == AssignmentStatus.Confirmed || Status == AssignmentStatus.InProgress || Status == AssignmentStatus.Completed;
        public DateTime? ConfirmedAt => Status == AssignmentStatus.Confirmed ? UpdatedAt : null;
    }
}

