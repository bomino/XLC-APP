using System.ComponentModel.DataAnnotations;

namespace EmployeeScheduling.API.Models
{
    public class Employee
    {
        public int EmployeeId { get; set; }

        // Link to User account
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string EmployeeNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public DateTime HireDate { get; set; } = DateTime.UtcNow;

        public DateTime? TerminationDate { get; set; }

        [Required]
        [StringLength(100)]
        public string Department { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Position { get; set; } = string.Empty;

        [Range(0, 999999.99)]
        public decimal HourlyRate { get; set; }

        [StringLength(50)]
        public string EmploymentType { get; set; } = "Full-Time"; // Full-Time, Part-Time, Contract, Temporary

        [StringLength(50)]
        public string Status { get; set; } = "Active"; // Active, Inactive, On Leave, Terminated

        public int? ManagerId { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        // Work preferences
        public int MaxHoursPerWeek { get; set; } = 40;
        public int MinHoursPerWeek { get; set; } = 0;

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual Employee? Manager { get; set; }
        public virtual ICollection<Employee> Subordinates { get; set; } = new List<Employee>();
        public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
        public virtual ICollection<EmployeeQualification> EmployeeQualifications { get; set; } = new List<EmployeeQualification>();
        public virtual ICollection<Availability> Availabilities { get; set; } = new List<Availability>();

        // Computed properties
        public string FullName => $"{FirstName} {LastName}";
        public int Age => DateOfBirth.HasValue ? DateTime.Today.Year - DateOfBirth.Value.Year : 0;
        public int YearsOfService => DateTime.Today.Year - HireDate.Year;
    }
}
