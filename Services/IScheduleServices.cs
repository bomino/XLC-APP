using EmployeeScheduling.API.DTOs;
using EmployeeScheduling.API.Models;

namespace EmployeeScheduling.API.Services
{
    public interface IScheduleService
    {
        Task<PagedResult<ScheduleResponse>> GetSchedulesAsync(ScheduleSearchRequest request);
        Task<ScheduleResponse?> GetScheduleByIdAsync(int scheduleId);
        Task<ScheduleResponse> CreateScheduleAsync(CreateScheduleRequest request);
        Task<ScheduleResponse?> UpdateScheduleAsync(int scheduleId, UpdateScheduleRequest request);
        Task<bool> DeleteScheduleAsync(int scheduleId);
        Task<ScheduleResponse?> PublishScheduleAsync(int scheduleId);
        Task<ScheduleResponse?> UnpublishScheduleAsync(int scheduleId);
        Task<ScheduleResponse?> GetCurrentScheduleAsync();
        Task<List<ScheduleResponse>> GetUpcomingSchedulesAsync(int days = 30);
        Task<List<ScheduleResponse>> GetSchedulesByDateRangeAsync(DateTime startDate, DateTime endDate);
    }

    public interface IShiftService
    {
        Task<PagedResult<ShiftResponse>> GetShiftsAsync(ShiftSearchRequest request);
        Task<ShiftResponse?> GetShiftByIdAsync(int shiftId);
        Task<ShiftResponse> CreateShiftAsync(CreateShiftRequest request);
        Task<ShiftResponse?> UpdateShiftAsync(int shiftId, UpdateShiftRequest request);
        Task<bool> DeleteShiftAsync(int shiftId);
        Task<List<ShiftResponse>> GetShiftsByScheduleAsync(int scheduleId);
        Task<List<ShiftResponse>> GetShiftsByDateAsync(DateTime date);
        Task<List<ShiftResponse>> GetShiftsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<ShiftResponse>> GetTodaysShiftsAsync();
        Task<List<ShiftResponse>> GetTomorrowsShiftsAsync();
        Task<List<ShiftResponse>> GetThisWeeksShiftsAsync();
        Task<List<ShiftResponse>> GetShiftsByDepartmentAsync(string department);
        Task<List<ShiftResponse>> GetShiftsByLocationAsync(int locationId);
        Task<List<EmployeeInfo>> GetAvailableEmployeesForShiftAsync(int shiftId);
        Task<List<EmployeeInfo>> GetPreferredEmployeesForShiftAsync(int shiftId);
        Task<List<ShiftResponse>> GetShiftConflictsAsync(int employeeId, DateTime startTime, DateTime endTime);
        Task<List<ShiftResponse>> GetUnderstaffedShiftsAsync(DateTime? startDate = null, DateTime? endDate = null);
    }

    public interface IAssignmentService
    {
        Task<PagedResult<AssignmentResponse>> GetAssignmentsAsync(AssignmentSearchRequest request);
        Task<AssignmentResponse?> GetAssignmentByIdAsync(int assignmentId);
        Task<AssignmentResponse> CreateAssignmentAsync(CreateAssignmentRequest request);
        Task<BulkAssignmentResponse> CreateBulkAssignmentsAsync(BulkAssignmentRequest request);
        Task<AssignmentResponse> UpdateAssignmentStatusAsync(int assignmentId, AssignmentStatus status, string? notes = null);
        Task<AssignmentResponse> CheckInAsync(int assignmentId, DateTime? checkInTime = null);
        Task<AssignmentResponse> CheckOutAsync(int assignmentId, DateTime? checkOutTime = null);
        Task<bool> DeleteAssignmentAsync(int assignmentId);
        Task<List<AssignmentResponse>> GetEmployeeAssignmentsAsync(int employeeId, DateTime? startDate = null, DateTime? endDate = null);
        Task<List<AssignmentResponse>> GetTodaysAssignmentsAsync(int employeeId);
        Task<List<AssignmentResponse>> GetUpcomingAssignmentsAsync(int employeeId, int days = 7);
    }

    public interface IAvailabilityService
    {
        Task<List<AvailabilityResponse>> GetEmployeeAvailabilityAsync(int employeeId);
        Task<AvailabilityResponse?> GetAvailabilityByIdAsync(int availabilityId);
        Task<AvailabilityResponse> CreateAvailabilityAsync(CreateAvailabilityRequest request);
        Task<AvailabilityResponse> UpdateAvailabilityAsync(int availabilityId, UpdateAvailabilityRequest request);
        Task<bool> DeleteAvailabilityAsync(int availabilityId);
        Task<List<AvailabilityResponse>> GetAllAvailabilityAsync(DayOfWeek? dayOfWeek = null, AvailabilityType? availabilityType = null);
        Task<bool> BulkDeleteAvailabilityAsync(int employeeId, DayOfWeek? dayOfWeek = null);
        Task<List<AvailabilityResponse>> BulkCreateAvailabilityAsync(int employeeId, List<CreateAvailabilityRequest> requests);
        Task<List<AvailabilityResponse>> GetAvailableEmployeesForShiftAsync(int shiftId);
        Task<List<AvailabilityResponse>> GetPreferredEmployeesForShiftAsync(int shiftId);
        Task<bool> IsEmployeeAvailableForShiftAsync(int employeeId, int shiftId);
    }
}

