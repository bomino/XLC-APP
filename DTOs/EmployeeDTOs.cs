using System.ComponentModel.DataAnnotations;

namespace EmployeeScheduling.API.DTOs
{
    // For creating new employees
    public class CreateEmployeeRequest
    {
        public int? UserId { get; set; } // Optional - can create employee without user account

        [Required]
        [StringLength(50)]
        public string EmployeeNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [Required]
        public DateTime HireDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(100)]
        public string Department { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Position { get; set; } = string.Empty;

        [Range(0, 999999.99)]
        public decimal HourlyRate { get; set; }

        [StringLength(50)]
        public string EmploymentType { get; set; } = "Full-Time";

        public int? ManagerId { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        [Range(0, 168)]
        public int MaxHoursPerWeek { get; set; } = 40;

        [Range(0, 168)]
        public int MinHoursPerWeek { get; set; } = 0;
    }

    // For updating existing employees
    public class UpdateEmployeeRequest
    {
        [StringLength(100)]
        public string? FirstName { get; set; }

        [StringLength(100)]
        public string? LastName { get; set; }

        [EmailAddress]
        [StringLength(255)]
        public string? Email { get; set; }

        [Phone]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [StringLength(100)]
        public string? Department { get; set; }

        [StringLength(100)]
        public string? Position { get; set; }

        [Range(0, 999999.99)]
        public decimal? HourlyRate { get; set; }

        [StringLength(50)]
        public string? EmploymentType { get; set; }

        [StringLength(50)]
        public string? Status { get; set; }

        public int? ManagerId { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        [Range(0, 168)]
        public int? MaxHoursPerWeek { get; set; }

        [Range(0, 168)]
        public int? MinHoursPerWeek { get; set; }

        public DateTime? TerminationDate { get; set; }
    }

    // For returning employee data
    public class EmployeeResponse
    {
        public int EmployeeId { get; set; }
        public int? UserId { get; set; }
        public string EmployeeNumber { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public int Age { get; set; }
        public DateTime HireDate { get; set; }
        public DateTime? TerminationDate { get; set; }
        public string Department { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public decimal HourlyRate { get; set; }
        public string EmploymentType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int? ManagerId { get; set; }
        public string? ManagerName { get; set; }
        public string? Notes { get; set; }
        public int MaxHoursPerWeek { get; set; }
        public int MinHoursPerWeek { get; set; }
        public int YearsOfService { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    // For employee search and filtering
    public class EmployeeSearchRequest
    {
        public string? SearchTerm { get; set; }
        public string? Department { get; set; }
        public string? Position { get; set; }
        public string? Status { get; set; }
        public string? EmploymentType { get; set; }
        public int? ManagerId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "LastName";
        public string SortDirection { get; set; } = "asc";
    }

 
}
