using Microsoft.EntityFrameworkCore;
using EmployeeScheduling.API.Data;
using EmployeeScheduling.API.DTOs;
using EmployeeScheduling.API.Models;

namespace EmployeeScheduling.API.Services
{
    public class AssignmentService : IAssignmentService
    {
        private readonly ApplicationDbContext _context;

        public AssignmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<AssignmentResponse>> GetAssignmentsAsync(AssignmentSearchRequest request)
        {
            var query = _context.Assignments
                .Include(a => a.Shift)
                    .ThenInclude(s => s.Schedule)
                .Include(a => a.Shift)
                    .ThenInclude(s => s.Location)
                .Include(a => a.Employee)
                .Include(a => a.AssignedByUser)
                .AsQueryable();

            // Apply filters
            if (request.ShiftId.HasValue)
            {
                query = query.Where(a => a.ShiftId == request.ShiftId.Value);
            }

            if (request.EmployeeId.HasValue)
            {
                query = query.Where(a => a.EmployeeId == request.EmployeeId.Value);
            }

            if (request.ScheduleId.HasValue)
            {
                query = query.Where(a => a.Shift.ScheduleId == request.ScheduleId.Value);
            }

            if (request.Status.HasValue)
            {
                query = query.Where(a => a.Status == request.Status.Value);
            }

            if (request.StartDate.HasValue)
            {
                query = query.Where(a => a.Shift.StartTime.Date >= request.StartDate.Value.Date);
            }

            if (request.EndDate.HasValue)
            {
                query = query.Where(a => a.Shift.EndTime.Date <= request.EndDate.Value.Date);
            }

            if (!string.IsNullOrEmpty(request.Department))
            {
                query = query.Where(a => a.Employee.Department == request.Department);
            }

            if (!string.IsNullOrEmpty(request.Position))
            {
                query = query.Where(a => a.Employee.Position == request.Position);
            }

            if (request.IsConfirmed.HasValue)
            {
                if (request.IsConfirmed.Value)
                {
                    query = query.Where(a => a.Status == AssignmentStatus.Confirmed || 
                                           a.Status == AssignmentStatus.InProgress || 
                                           a.Status == AssignmentStatus.Completed);
                }
                else
                {
                    query = query.Where(a => a.Status == AssignmentStatus.Assigned || 
                                           a.Status == AssignmentStatus.Declined);
                }
            }

            // Apply sorting
            query = request.SortBy.ToLower() switch
            {
                "employeename" => request.SortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(a => a.Employee.FirstName).ThenByDescending(a => a.Employee.LastName)
                    : query.OrderBy(a => a.Employee.FirstName).ThenBy(a => a.Employee.LastName),
                "shifttitle" => request.SortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(a => a.Shift.Title)
                    : query.OrderBy(a => a.Shift.Title),
                "status" => request.SortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(a => a.Status)
                    : query.OrderBy(a => a.Status),
                "assignedat" => request.SortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(a => a.AssignedAt)
                    : query.OrderBy(a => a.AssignedAt),
                _ => request.SortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(a => a.Shift.StartTime)
                    : query.OrderBy(a => a.Shift.StartTime)
            };

            var totalCount = await query.CountAsync();
            var assignments = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var assignmentResponses = assignments.Select(MapToAssignmentResponse).ToList();

            return new PagedResult<AssignmentResponse>
            {
                Data = assignmentResponses,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize),
                HasNextPage = request.Page * request.PageSize < totalCount,
                HasPreviousPage = request.Page > 1
            };
        }

        public async Task<AssignmentResponse?> GetAssignmentByIdAsync(int assignmentId)
        {
            var assignment = await _context.Assignments
                .Include(a => a.Shift)
                    .ThenInclude(s => s.Schedule)
                .Include(a => a.Shift)
                    .ThenInclude(s => s.Location)
                .Include(a => a.Employee)
                .Include(a => a.AssignedByUser)
                .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);

            return assignment == null ? null : MapToAssignmentResponse(assignment);
        }

        // Interface method: CreateAssignmentAsync(CreateAssignmentRequest)
        public async Task<AssignmentResponse> CreateAssignmentAsync(CreateAssignmentRequest request)
        {
            // Default to admin user (ID = 1) since interface doesn't include assignedByUserId
            return await CreateAssignmentWithUserAsync(request, 1);
        }

        // Helper method for internal use with user ID
        public async Task<AssignmentResponse> CreateAssignmentWithUserAsync(CreateAssignmentRequest request, int assignedByUserId)
        {
            // Check if shift exists
            var shift = await _context.Shifts.FindAsync(request.ShiftId);
            if (shift == null)
                throw new ArgumentException("Shift not found");

            // Check if employee exists
            var employee = await _context.Employees.FindAsync(request.EmployeeId);
            if (employee == null)
                throw new ArgumentException("Employee not found");

            // Check for conflicts
            var hasConflict = await _context.Assignments
                .Include(a => a.Shift)
                .AnyAsync(a => a.EmployeeId == request.EmployeeId &&
                             a.Shift.StartTime < shift.EndTime &&
                             a.Shift.EndTime > shift.StartTime &&
                             (a.Status == AssignmentStatus.Assigned || 
                              a.Status == AssignmentStatus.Confirmed || 
                              a.Status == AssignmentStatus.InProgress));

            if (hasConflict)
                throw new InvalidOperationException("Employee has a conflicting assignment");

            var assignment = new Assignment
            {
                ShiftId = request.ShiftId,
                EmployeeId = request.EmployeeId,
                Status = AssignmentStatus.Assigned,
                AssignedAt = DateTime.UtcNow,
                AssignedByUserId = assignedByUserId,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow
            };

            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();

            return await GetAssignmentByIdAsync(assignment.AssignmentId) ?? 
                   throw new InvalidOperationException("Failed to retrieve created assignment");
        }

        public async Task<AssignmentResponse?> UpdateAssignmentAsync(int assignmentId, UpdateAssignmentRequest request)
        {
            var assignment = await _context.Assignments.FindAsync(assignmentId);
            if (assignment == null) return null;

            if (request.Status.HasValue)
                assignment.Status = request.Status.Value;

            if (!string.IsNullOrEmpty(request.Notes))
                assignment.Notes = request.Notes;

            assignment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await GetAssignmentByIdAsync(assignmentId);
        }

        public async Task<bool> DeleteAssignmentAsync(int assignmentId)
        {
            var assignment = await _context.Assignments.FindAsync(assignmentId);
            if (assignment == null) return false;

            _context.Assignments.Remove(assignment);
            await _context.SaveChangesAsync();
            return true;
        }

        // Interface method: CreateBulkAssignmentsAsync(BulkAssignmentRequest)
        public async Task<BulkAssignmentResponse> CreateBulkAssignmentsAsync(BulkAssignmentRequest request)
        {
            // Default to admin user (ID = 1) since interface doesn't include assignedByUserId
            return await CreateBulkAssignmentsWithUserAsync(request, 1);
        }

        // Helper method for internal use with user ID
        public async Task<BulkAssignmentResponse> CreateBulkAssignmentsWithUserAsync(BulkAssignmentRequest request, int assignedByUserId)
        {
            var response = new BulkAssignmentResponse
            {
                TotalRequested = request.EmployeeIds.Count,
                SuccessfulAssignments = 0,
                SuccessfulCount = 0,
                FailedAssignments = 0,
                ErrorCount = 0,
                Errors = new List<string>(),
                CreatedAssignments = new List<AssignmentResponse>()
            };

            foreach (var employeeId in request.EmployeeIds)
            {
                try
                {
                    var assignmentRequest = new CreateAssignmentRequest
                    {
                        ShiftId = request.ShiftId,
                        EmployeeId = employeeId,
                        Notes = request.Notes
                    };

                    var createdAssignment = await CreateAssignmentWithUserAsync(assignmentRequest, assignedByUserId);
                    response.CreatedAssignments.Add(createdAssignment);
                    response.SuccessfulAssignments++;
                    response.SuccessfulCount++;
                }
                catch (Exception ex)
                {
                    response.FailedAssignments++;
                    response.ErrorCount++;
                    response.Errors.Add($"Employee {employeeId}: {ex.Message}");
                }
            }

            return response;
        }

        // Interface method: UpdateAssignmentStatusAsync(int, AssignmentStatus, string?)
        public async Task<AssignmentResponse?> UpdateAssignmentStatusAsync(int assignmentId, AssignmentStatus status, string? notes = null)
        {
            var assignment = await _context.Assignments.FindAsync(assignmentId);
            if (assignment == null) return null;

            assignment.Status = status;
            assignment.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(notes))
            {
                assignment.Notes = string.IsNullOrEmpty(assignment.Notes)
                    ? notes
                    : $"{assignment.Notes}\n{notes}";
            }

            await _context.SaveChangesAsync();
            return await GetAssignmentByIdAsync(assignmentId);
        }

        public async Task<AssignmentResponse?> ConfirmAssignmentAsync(int assignmentId)
        {
            return await UpdateAssignmentStatusAsync(assignmentId, AssignmentStatus.Confirmed, "Assignment confirmed");
        }

        public async Task<AssignmentResponse?> DeclineAssignmentAsync(int assignmentId, string? reason = null)
        {
            var notes = string.IsNullOrEmpty(reason) ? "Assignment declined" : $"Assignment declined: {reason}";
            return await UpdateAssignmentStatusAsync(assignmentId, AssignmentStatus.Declined, notes);
        }

        // Interface method: CheckInAsync(int, DateTime?)
        public async Task<AssignmentResponse?> CheckInAsync(int assignmentId, DateTime? checkInTime = null)
        {
            var assignment = await _context.Assignments.FindAsync(assignmentId);
            if (assignment == null) return null;

            assignment.CheckInTime = checkInTime ?? DateTime.UtcNow;
            assignment.Status = AssignmentStatus.InProgress;
            assignment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await GetAssignmentByIdAsync(assignmentId);
        }

        // Interface method: CheckOutAsync(int, DateTime?)
        public async Task<AssignmentResponse?> CheckOutAsync(int assignmentId, DateTime? checkOutTime = null)
        {
            var assignment = await _context.Assignments
                .Include(a => a.Shift)
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);

            if (assignment == null) return null;

            assignment.CheckOutTime = checkOutTime ?? DateTime.UtcNow;
            assignment.Status = AssignmentStatus.Completed;
            assignment.UpdatedAt = DateTime.UtcNow;

            // Calculate hours worked
            if (assignment.CheckInTime.HasValue && assignment.CheckOutTime.HasValue)
            {
                var totalHours = (decimal)(assignment.CheckOutTime.Value - assignment.CheckInTime.Value).TotalHours;
                assignment.HoursWorked = Math.Round(totalHours, 2);

                // Calculate regular and overtime hours (assuming 8 hours is regular)
                if (totalHours <= 8)
                {
                    assignment.RegularHours = assignment.HoursWorked;
                    assignment.OvertimeHours = 0;
                }
                else
                {
                    assignment.RegularHours = 8;
                    assignment.OvertimeHours = assignment.HoursWorked - 8;
                }

                // Calculate total pay
                var regularPay = assignment.RegularHours.Value * assignment.Employee.HourlyRate;
                var overtimePay = assignment.OvertimeHours.Value * assignment.Employee.HourlyRate * 1.5m;
                assignment.TotalPay = regularPay + overtimePay;
            }

            await _context.SaveChangesAsync();
            return await GetAssignmentByIdAsync(assignmentId);
        }

        // Overloaded methods for backward compatibility
        public async Task<AssignmentResponse?> CheckInAsync(int assignmentId, CheckInRequest request)
        {
            var assignment = await CheckInAsync(assignmentId, request.CheckInTime);
            
            // Add notes if provided
            if (!string.IsNullOrEmpty(request.Notes) && assignment != null)
            {
                var assignmentEntity = await _context.Assignments.FindAsync(assignmentId);
                if (assignmentEntity != null)
                {
                    assignmentEntity.Notes = string.IsNullOrEmpty(assignmentEntity.Notes)
                        ? $"Check-in: {request.Notes}"
                        : $"{assignmentEntity.Notes}\nCheck-in: {request.Notes}";
                    
                    await _context.SaveChangesAsync();
                    assignment = await GetAssignmentByIdAsync(assignmentId);
                }
            }

            return assignment;
        }

        public async Task<AssignmentResponse?> CheckOutAsync(int assignmentId, CheckOutRequest request)
        {
            var assignment = await CheckOutAsync(assignmentId, request.CheckOutTime);
            
            // Add notes if provided
            if (!string.IsNullOrEmpty(request.Notes) && assignment != null)
            {
                var assignmentEntity = await _context.Assignments.FindAsync(assignmentId);
                if (assignmentEntity != null)
                {
                    assignmentEntity.Notes = string.IsNullOrEmpty(assignmentEntity.Notes)
                        ? $"Check-out: {request.Notes}"
                        : $"{assignmentEntity.Notes}\nCheck-out: {request.Notes}";
                    
                    await _context.SaveChangesAsync();
                    assignment = await GetAssignmentByIdAsync(assignmentId);
                }
            }

            return assignment;
        }

        public async Task<List<AssignmentResponse>> GetAssignmentsByEmployeeAsync(int employeeId)
        {
            var assignments = await _context.Assignments
                .Include(a => a.Shift)
                    .ThenInclude(s => s.Schedule)
                .Include(a => a.Shift)
                    .ThenInclude(s => s.Location)
                .Include(a => a.Employee)
                .Include(a => a.AssignedByUser)
                .Where(a => a.EmployeeId == employeeId)
                .OrderByDescending(a => a.Shift.StartTime)
                .ToListAsync();

            return assignments.Select(MapToAssignmentResponse).ToList();
        }

        public async Task<List<AssignmentResponse>> GetAssignmentsByShiftAsync(int shiftId)
        {
            var assignments = await _context.Assignments
                .Include(a => a.Shift)
                    .ThenInclude(s => s.Schedule)
                .Include(a => a.Shift)
                    .ThenInclude(s => s.Location)
                .Include(a => a.Employee)
                .Include(a => a.AssignedByUser)
                .Where(a => a.ShiftId == shiftId)
                .OrderBy(a => a.Employee.FirstName)
                .ThenBy(a => a.Employee.LastName)
                .ToListAsync();

            return assignments.Select(MapToAssignmentResponse).ToList();
        }

        // Interface method: GetEmployeeAssignmentsAsync(int, DateTime?, DateTime?)
        public async Task<List<AssignmentResponse>> GetEmployeeAssignmentsAsync(int employeeId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Assignments
                .Include(a => a.Shift)
                    .ThenInclude(s => s.Schedule)
                .Include(a => a.Shift)
                    .ThenInclude(s => s.Location)
                .Include(a => a.Employee)
                .Include(a => a.AssignedByUser)
                .Where(a => a.EmployeeId == employeeId);

            if (startDate.HasValue)
            {
                query = query.Where(a => a.Shift.StartTime.Date >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                query = query.Where(a => a.Shift.EndTime.Date <= endDate.Value.Date);
            }

            var assignments = await query
                .OrderByDescending(a => a.Shift.StartTime)
                .ToListAsync();

            return assignments.Select(MapToAssignmentResponse).ToList();
        }

        public async Task<List<AssignmentResponse>> GetMyAssignmentsAsync(int employeeId)
        {
            return await GetEmployeeAssignmentsAsync(employeeId);
        }

        public async Task<List<AssignmentResponse>> GetTodaysAssignmentsAsync(int employeeId)
        {
            var today = DateTime.Today;
            return await GetEmployeeAssignmentsAsync(employeeId, today, today);
        }

        public async Task<List<AssignmentResponse>> GetUpcomingAssignmentsAsync(int employeeId, int days = 7)
        {
            var startDate = DateTime.Today;
            var endDate = startDate.AddDays(days);
            return await GetEmployeeAssignmentsAsync(employeeId, startDate, endDate);
        }

        public async Task<bool> AssignmentExistsAsync(int assignmentId)
        {
            return await _context.Assignments.AnyAsync(a => a.AssignmentId == assignmentId);
        }

        private static AssignmentResponse MapToAssignmentResponse(Assignment assignment)
        {
            return new AssignmentResponse
            {
                AssignmentId = assignment.AssignmentId,
                ShiftId = assignment.ShiftId,
                ScheduleId = assignment.Shift?.ScheduleId ?? 0,
                ScheduleName = assignment.Shift?.Schedule?.Name ?? "",
                ShiftTitle = assignment.Shift?.Title ?? "",
                ShiftStartTime = assignment.Shift?.StartTime ?? DateTime.MinValue,
                ShiftEndTime = assignment.Shift?.EndTime ?? DateTime.MinValue,
                LocationName = assignment.Shift?.Location?.Name,
                EmployeeId = assignment.EmployeeId,
                EmployeeName = $"{assignment.Employee?.FirstName} {assignment.Employee?.LastName}".Trim(),
                EmployeeNumber = assignment.Employee?.EmployeeNumber ?? "",
                Status = assignment.Status,
                AssignedAt = assignment.AssignedAt,
                AssignedByUserId = assignment.AssignedByUserId,
                AssignedByUserName = $"{assignment.AssignedByUser?.FirstName} {assignment.AssignedByUser?.LastName}".Trim(),
                CheckInTime = assignment.CheckInTime,
                CheckOutTime = assignment.CheckOutTime,
                HoursWorked = assignment.HoursWorked,
                RegularHours = assignment.RegularHours,
                OvertimeHours = assignment.OvertimeHours,
                TotalPay = assignment.TotalPay,
                Notes = assignment.Notes,
                CreatedAt = assignment.CreatedAt,
                UpdatedAt = assignment.UpdatedAt
            };
        }
    }
}

