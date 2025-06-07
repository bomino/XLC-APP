using Microsoft.EntityFrameworkCore;
using EmployeeScheduling.API.Data;
using EmployeeScheduling.API.Models;
using EmployeeScheduling.API.DTOs;

namespace EmployeeScheduling.API.Services
{
    public class AvailabilityService : IAvailabilityService
    {
        private readonly ApplicationDbContext _context;

        public AvailabilityService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<AvailabilityResponse>> GetEmployeeAvailabilityAsync(int employeeId)
        {
            var availabilities = await _context.Availabilities
                .Include(a => a.Employee)
                .Where(a => a.EmployeeId == employeeId)
                .OrderBy(a => a.DayOfWeek)
                .ThenBy(a => a.StartTime)
                .ToListAsync();

            return availabilities.Select(a => new AvailabilityResponse
            {
                AvailabilityId = a.AvailabilityId,
                EmployeeId = a.EmployeeId,
                EmployeeName = $"{a.Employee.FirstName} {a.Employee.LastName}",
                DayOfWeek = a.DayOfWeek,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                AvailabilityType = a.AvailabilityType,
                Notes = a.Notes,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            }).ToList();
        }

        public async Task<AvailabilityResponse?> GetAvailabilityByIdAsync(int availabilityId)
        {
            var availability = await _context.Availabilities
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(a => a.AvailabilityId == availabilityId);

            if (availability == null)
                return null;

            return new AvailabilityResponse
            {
                AvailabilityId = availability.AvailabilityId,
                EmployeeId = availability.EmployeeId,
                EmployeeName = $"{availability.Employee.FirstName} {availability.Employee.LastName}",
                DayOfWeek = availability.DayOfWeek,
                StartTime = availability.StartTime,
                EndTime = availability.EndTime,
                AvailabilityType = availability.AvailabilityType,
                Notes = availability.Notes,
                CreatedAt = availability.CreatedAt,
                UpdatedAt = availability.UpdatedAt
            };
        }

        public async Task<AvailabilityResponse> CreateAvailabilityAsync(CreateAvailabilityRequest request)
        {
            // Validate employee exists
            var employee = await _context.Employees.FindAsync(request.EmployeeId);
            if (employee == null)
                throw new ArgumentException("Employee not found");

            // Validate time range
            if (request.StartTime >= request.EndTime)
                throw new ArgumentException("Start time must be before end time");

            // Check for overlapping availability for the same day
            var hasOverlap = await _context.Availabilities
                .AnyAsync(a => a.EmployeeId == request.EmployeeId &&
                              a.DayOfWeek == request.DayOfWeek &&
                              a.StartTime < request.EndTime &&
                              a.EndTime > request.StartTime);

            if (hasOverlap)
                throw new InvalidOperationException("Employee already has availability that overlaps with this time period");

            var availability = new Availability
            {
                EmployeeId = request.EmployeeId,
                DayOfWeek = request.DayOfWeek,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                AvailabilityType = request.AvailabilityType,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Availabilities.Add(availability);
            await _context.SaveChangesAsync();

            // Reload with includes
            availability = await _context.Availabilities
                .Include(a => a.Employee)
                .FirstAsync(a => a.AvailabilityId == availability.AvailabilityId);

            return new AvailabilityResponse
            {
                AvailabilityId = availability.AvailabilityId,
                EmployeeId = availability.EmployeeId,
                EmployeeName = $"{availability.Employee.FirstName} {availability.Employee.LastName}",
                DayOfWeek = availability.DayOfWeek,
                StartTime = availability.StartTime,
                EndTime = availability.EndTime,
                AvailabilityType = availability.AvailabilityType,
                Notes = availability.Notes,
                CreatedAt = availability.CreatedAt,
                UpdatedAt = availability.UpdatedAt
            };
        }

        public async Task<AvailabilityResponse> UpdateAvailabilityAsync(int availabilityId, UpdateAvailabilityRequest request)
        {
            var availability = await _context.Availabilities
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(a => a.AvailabilityId == availabilityId);

            if (availability == null)
                throw new ArgumentException("Availability not found");

            // Validate time range if provided
            var startTime = request.StartTime ?? availability.StartTime;
            var endTime = request.EndTime ?? availability.EndTime;
            var dayOfWeek = request.DayOfWeek ?? availability.DayOfWeek;

            if (startTime >= endTime)
                throw new ArgumentException("Start time must be before end time");

            // Check for overlapping availability for the same day (excluding current record)
            var hasOverlap = await _context.Availabilities
                .AnyAsync(a => a.EmployeeId == availability.EmployeeId &&
                              a.AvailabilityId != availabilityId &&
                              a.DayOfWeek == dayOfWeek &&
                              a.StartTime < endTime &&
                              a.EndTime > startTime);

            if (hasOverlap)
                throw new InvalidOperationException("Employee already has availability that overlaps with this time period");

            // Update fields
            if (request.DayOfWeek.HasValue)
                availability.DayOfWeek = request.DayOfWeek.Value;

            if (request.StartTime.HasValue)
                availability.StartTime = request.StartTime.Value;

            if (request.EndTime.HasValue)
                availability.EndTime = request.EndTime.Value;

            if (request.AvailabilityType.HasValue)
                availability.AvailabilityType = request.AvailabilityType.Value;

            if (request.Notes != null)
                availability.Notes = request.Notes;

            availability.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new AvailabilityResponse
            {
                AvailabilityId = availability.AvailabilityId,
                EmployeeId = availability.EmployeeId,
                EmployeeName = $"{availability.Employee.FirstName} {availability.Employee.LastName}",
                DayOfWeek = availability.DayOfWeek,
                StartTime = availability.StartTime,
                EndTime = availability.EndTime,
                AvailabilityType = availability.AvailabilityType,
                Notes = availability.Notes,
                CreatedAt = availability.CreatedAt,
                UpdatedAt = availability.UpdatedAt
            };
        }

        public async Task<bool> DeleteAvailabilityAsync(int availabilityId)
        {
            var availability = await _context.Availabilities.FindAsync(availabilityId);
            if (availability == null)
                return false;

            _context.Availabilities.Remove(availability);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<AvailabilityResponse>> GetAvailableEmployeesForShiftAsync(int shiftId)
        {
            var shift = await _context.Shifts.FindAsync(shiftId);
            if (shift == null)
                throw new ArgumentException("Shift not found");

            var shiftDayOfWeek = shift.StartTime.DayOfWeek;
            var shiftStartTime = shift.StartTime.TimeOfDay;
            var shiftEndTime = shift.EndTime.TimeOfDay;

            var availableEmployees = await _context.Availabilities
                .Include(a => a.Employee)
                .Where(a => a.DayOfWeek == shiftDayOfWeek &&
                           a.AvailabilityType == AvailabilityType.Available &&
                           a.StartTime <= shiftStartTime &&
                           a.EndTime >= shiftEndTime)
                .Where(a => !_context.Assignments
                    .Any(assignment => assignment.EmployeeId == a.EmployeeId &&
                                     assignment.Status != AssignmentStatus.Declined &&
                                     assignment.Status != AssignmentStatus.NoShow &&
                                     assignment.Shift.StartTime < shift.EndTime &&
                                     assignment.Shift.EndTime > shift.StartTime))
                .ToListAsync();

            return availableEmployees.Select(a => new AvailabilityResponse
            {
                AvailabilityId = a.AvailabilityId,
                EmployeeId = a.EmployeeId,
                EmployeeName = $"{a.Employee.FirstName} {a.Employee.LastName}",
                DayOfWeek = a.DayOfWeek,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                AvailabilityType = a.AvailabilityType,
                Notes = a.Notes,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            }).ToList();
        }

        public async Task<List<AvailabilityResponse>> GetPreferredEmployeesForShiftAsync(int shiftId)
        {
            var shift = await _context.Shifts.FindAsync(shiftId);
            if (shift == null)
                throw new ArgumentException("Shift not found");

            var shiftDayOfWeek = shift.StartTime.DayOfWeek;
            var shiftStartTime = shift.StartTime.TimeOfDay;
            var shiftEndTime = shift.EndTime.TimeOfDay;

            var preferredEmployees = await _context.Availabilities
                .Include(a => a.Employee)
                .Where(a => a.DayOfWeek == shiftDayOfWeek &&
                           a.AvailabilityType == AvailabilityType.Preferred &&
                           a.StartTime <= shiftStartTime &&
                           a.EndTime >= shiftEndTime)
                .Where(a => !_context.Assignments
                    .Any(assignment => assignment.EmployeeId == a.EmployeeId &&
                                     assignment.Status != AssignmentStatus.Declined &&
                                     assignment.Status != AssignmentStatus.NoShow &&
                                     assignment.Shift.StartTime < shift.EndTime &&
                                     assignment.Shift.EndTime > shift.StartTime))
                .ToListAsync();

            return preferredEmployees.Select(a => new AvailabilityResponse
            {
                AvailabilityId = a.AvailabilityId,
                EmployeeId = a.EmployeeId,
                EmployeeName = $"{a.Employee.FirstName} {a.Employee.LastName}",
                DayOfWeek = a.DayOfWeek,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                AvailabilityType = a.AvailabilityType,
                Notes = a.Notes,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            }).ToList();
        }

        public async Task<bool> IsEmployeeAvailableForShiftAsync(int employeeId, int shiftId)
        {
            var shift = await _context.Shifts.FindAsync(shiftId);
            if (shift == null)
                return false;

            var shiftDayOfWeek = shift.StartTime.DayOfWeek;
            var shiftStartTime = shift.StartTime.TimeOfDay;
            var shiftEndTime = shift.EndTime.TimeOfDay;

            // Check if employee has availability that covers the shift time
            var hasAvailability = await _context.Availabilities
                .AnyAsync(a => a.EmployeeId == employeeId &&
                              a.DayOfWeek == shiftDayOfWeek &&
                              (a.AvailabilityType == AvailabilityType.Available || a.AvailabilityType == AvailabilityType.Preferred) &&
                              a.StartTime <= shiftStartTime &&
                              a.EndTime >= shiftEndTime);

            if (!hasAvailability)
                return false;

            // Check if employee has conflicting assignments
            var hasConflict = await _context.Assignments
                .AnyAsync(a => a.EmployeeId == employeeId &&
                              a.Status != AssignmentStatus.Declined &&
                              a.Status != AssignmentStatus.NoShow &&
                              a.Shift.StartTime < shift.EndTime &&
                              a.Shift.EndTime > shift.StartTime);

            return !hasConflict;
        }

        public async Task<List<AvailabilityResponse>> GetAllAvailabilityAsync(DayOfWeek? dayOfWeek = null, AvailabilityType? availabilityType = null)
        {
            var query = _context.Availabilities
                .Include(a => a.Employee)
                .AsQueryable();

            if (dayOfWeek.HasValue)
                query = query.Where(a => a.DayOfWeek == dayOfWeek.Value);

            if (availabilityType.HasValue)
                query = query.Where(a => a.AvailabilityType == availabilityType.Value);

            var availabilities = await query
                .OrderBy(a => a.Employee.FirstName)
                .ThenBy(a => a.Employee.LastName)
                .ThenBy(a => a.DayOfWeek)
                .ThenBy(a => a.StartTime)
                .ToListAsync();

            return availabilities.Select(a => new AvailabilityResponse
            {
                AvailabilityId = a.AvailabilityId,
                EmployeeId = a.EmployeeId,
                EmployeeName = $"{a.Employee.FirstName} {a.Employee.LastName}",
                DayOfWeek = a.DayOfWeek,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                AvailabilityType = a.AvailabilityType,
                Notes = a.Notes,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            }).ToList();
        }

        public async Task<bool> BulkDeleteAvailabilityAsync(int employeeId, DayOfWeek? dayOfWeek = null)
        {
            var query = _context.Availabilities
                .Where(a => a.EmployeeId == employeeId);

            if (dayOfWeek.HasValue)
                query = query.Where(a => a.DayOfWeek == dayOfWeek.Value);

            var availabilities = await query.ToListAsync();
            
            if (!availabilities.Any())
                return false;

            _context.Availabilities.RemoveRange(availabilities);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<AvailabilityResponse>> BulkCreateAvailabilityAsync(int employeeId, List<CreateAvailabilityRequest> requests)
        {
            // Validate employee exists
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null)
                throw new ArgumentException("Employee not found");

            var results = new List<AvailabilityResponse>();

            foreach (var request in requests)
            {
                try
                {
                    // Set the employee ID from the parameter
                    request.EmployeeId = employeeId;
                    var availability = await CreateAvailabilityAsync(request);
                    results.Add(availability);
                }
                catch (Exception)
                {
                    // Skip invalid entries but continue processing others
                    continue;
                }
            }

            return results;
        }
    }
}

