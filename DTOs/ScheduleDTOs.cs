using System.ComponentModel.DataAnnotations;
using EmployeeScheduling.API.Models;

namespace EmployeeScheduling.API.DTOs
{
    // ===== SCHEDULE DTOs =====

    public class CreateScheduleRequest
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }
    }

    public class UpdateScheduleRequest
    {
        [StringLength(200)]
        public string? Name { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class ScheduleResponse
    {
        public int ScheduleId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsPublished { get; set; }
        public int CreatedByUserId { get; set; }
        public string CreatedByUserName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int TotalShifts { get; set; }
        public int TotalAssignments { get; set; }
    }

    // ===== SHIFT DTOs =====

    public class CreateShiftRequest
    {
        [Required]
        public int ScheduleId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        [Range(1, 100)]
        public int RequiredEmployees { get; set; } = 1;

        public int? LocationId { get; set; }

        [StringLength(100)]
        public string Department { get; set; } = string.Empty;

        [StringLength(100)]
        public string Position { get; set; } = string.Empty;

        public List<int> RequiredQualificationIds { get; set; } = new();
    }

    public class UpdateShiftRequest
    {
        [StringLength(200)]
        public string? Title { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        [Range(1, 100)]
        public int? RequiredEmployees { get; set; }

        public int? LocationId { get; set; }

        [StringLength(100)]
        public string? Department { get; set; }

        [StringLength(100)]
        public string? Position { get; set; }

        public List<int>? RequiredQualificationIds { get; set; }
    }

    public class ShiftResponse
    {
        public int ShiftId { get; set; }
        public int ScheduleId { get; set; }
        public string ScheduleName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration { get; set; }
        public int RequiredEmployees { get; set; }
        public int AssignedEmployees { get; set; }
        public bool IsFullyStaffed { get; set; }
        public int? LocationId { get; set; }
        public string? LocationName { get; set; }
        public string Department { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public List<QualificationInfo> RequiredQualifications { get; set; } = new();
        public List<AssignmentInfo> Assignments { get; set; } = new();
    }

    // ===== ASSIGNMENT DTOs =====

    public class CreateAssignmentRequest
    {
        [Required]
        public int ShiftId { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }
    }

    public class UpdateAssignmentRequest
    {
        public AssignmentStatus? Status { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }
    }

    public class AssignmentResponse
    {
        public int AssignmentId { get; set; }
        public int ShiftId { get; set; }
        public int ScheduleId { get; set; }
        public string ScheduleName { get; set; } = string.Empty;
        public string ShiftTitle { get; set; } = string.Empty;
        public DateTime ShiftStartTime { get; set; }
        public DateTime ShiftEndTime { get; set; }
        public string? LocationName { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeNumber { get; set; } = string.Empty;
        public AssignmentStatus Status { get; set; }
        public DateTime AssignedAt { get; set; }
        public int AssignedByUserId { get; set; }
        public string AssignedByUserName { get; set; } = string.Empty;
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public decimal? HoursWorked { get; set; }
        public decimal? RegularHours { get; set; }
        public decimal? OvertimeHours { get; set; }
        public decimal? TotalPay { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Computed properties for backward compatibility
        public bool IsConfirmed => Status == AssignmentStatus.Confirmed || Status == AssignmentStatus.InProgress || Status == AssignmentStatus.Completed;
        public DateTime? ConfirmedAt => Status == AssignmentStatus.Confirmed ? UpdatedAt : null;
    }

    // ===== AVAILABILITY DTOs =====

    public class CreateAvailabilityRequest
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public DayOfWeek DayOfWeek { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        public AvailabilityType AvailabilityType { get; set; } = AvailabilityType.Available;

        [StringLength(500)]
        public string? Notes { get; set; }

        // Backward compatibility
        public bool IsAvailable { get; set; } = true;
    }

    public class UpdateAvailabilityRequest
    {
        public DayOfWeek? DayOfWeek { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public AvailabilityType? AvailabilityType { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        // Backward compatibility
        public bool? IsAvailable { get; set; }
    }

    public class AvailabilityResponse
    {
        public int AvailabilityId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public DayOfWeek DayOfWeek { get; set; }
        public string DayOfWeekName { get; set; } = string.Empty;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string TimeRange { get; set; } = string.Empty;
        public AvailabilityType AvailabilityType { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Computed property for backward compatibility
        public bool IsAvailable => AvailabilityType == AvailabilityType.Available || AvailabilityType == AvailabilityType.Preferred;
    }

    // ===== SEARCH AND FILTER DTOs =====

    public class ScheduleSearchRequest
    {
        public string? SearchTerm { get; set; }
        public bool? IsPublished { get; set; }
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
        public int? CreatedByUserId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "StartDate";
        public string SortDirection { get; set; } = "desc";
    }

    public class ShiftSearchRequest
    {
        public string? SearchTerm { get; set; }
        public int? ScheduleId { get; set; }
        public DateTime? StartTimeFrom { get; set; }
        public DateTime? StartTimeTo { get; set; }
        public int? LocationId { get; set; }
        public string? Department { get; set; }
        public string? Position { get; set; }
        public bool? IsFullyStaffed { get; set; }
        public List<int>? QualificationIds { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "StartTime";
        public string SortDirection { get; set; } = "asc";
    }

    public class AssignmentSearchRequest
    {
        public int? ShiftId { get; set; }
        public int? EmployeeId { get; set; }
        public int? ScheduleId { get; set; }
        public AssignmentStatus? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Department { get; set; }
        public string? Position { get; set; }
        public bool? IsConfirmed { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "ShiftStartTime";
        public string SortDirection { get; set; } = "asc";
        public bool SortDescending { get; set; } = false;
    }

    // ===== HELPER DTOs =====

    public class QualificationInfo
    {
        public int QualificationId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsRequired { get; set; }
    }

    public class AssignmentInfo
    {
        public int AssignmentId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeNumber { get; set; } = string.Empty;
        public AssignmentStatus Status { get; set; }
        public DateTime AssignedAt { get; set; }
        public bool IsConfirmed { get; set; }
    }

    public class LocationInfo
    {
        public int LocationId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
    }

    public class EmployeeInfo
    {
        public int EmployeeId { get; set; }
        public string EmployeeNumber { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string Department { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public decimal HourlyRate { get; set; }
        public bool IsActive { get; set; }
    }

    // ===== BULK OPERATIONS =====

    public class BulkAssignmentRequest
    {
        [Required]
        public int ShiftId { get; set; }

        [Required]
        public List<int> EmployeeIds { get; set; } = new();

        [StringLength(1000)]
        public string? Notes { get; set; }
    }

    public class BulkAssignmentResponse
    {
        public int TotalRequested { get; set; }
        public int SuccessfulAssignments { get; set; }
        public int SuccessfulCount { get; set; }
        public int FailedAssignments { get; set; }
        public int ErrorCount { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<AssignmentResponse> CreatedAssignments { get; set; } = new();
    }

    // ===== TIME TRACKING DTOs =====

    public class CheckInRequest
    {
        public DateTime? CheckInTime { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }

    public class CheckOutRequest
    {
        public DateTime? CheckOutTime { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }

    // ===== PAGED RESULT DTO =====

    public class PagedResult<T>
    {
        public List<T> Data { get; set; } = new();
        public List<T> Items { get; set; } = new(); // Alias for backward compatibility
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }
}

