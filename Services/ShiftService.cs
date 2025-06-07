using Microsoft.EntityFrameworkCore;
using EmployeeScheduling.API.Data;
using EmployeeScheduling.API.DTOs;
using EmployeeScheduling.API.Models;

namespace EmployeeScheduling.API.Services
{
    public class ShiftService : IShiftService
    {
        private readonly ApplicationDbContext _context;

        public ShiftService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<ShiftResponse>> GetShiftsAsync(ShiftSearchRequest request)
        {
            var query = _context.Shifts
                .Include(s => s.Schedule)
                .Include(s => s.Location)
                .Include(s => s.Assignments)
                    .ThenInclude(a => a.Employee)
                .Include(s => s.ShiftQualifications)
                    .ThenInclude(sq => sq.Qualification)
                .Where(s => s.IsActive);

            // Apply filters
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(s =>
                    s.Title.ToLower().Contains(searchTerm) ||
                    (s.Description != null && s.Description.ToLower().Contains(searchTerm)));
            }

            if (request.ScheduleId.HasValue)
            {
                query = query.Where(s => s.ScheduleId == request.ScheduleId.Value);
            }

            if (request.StartTimeFrom.HasValue)
            {
                query = query.Where(s => s.StartTime >= request.StartTimeFrom.Value);
            }

            if (request.StartTimeTo.HasValue)
            {
                query = query.Where(s => s.StartTime <= request.StartTimeTo.Value);
            }

            if (request.LocationId.HasValue)
            {
                query = query.Where(s => s.LocationId == request.LocationId.Value);
            }

            if (request.IsFullyStaffed.HasValue)
            {
                if (request.IsFullyStaffed.Value)
                {
                    query = query.Where(s => s.Assignments.Count >= s.RequiredEmployees);
                }
                else
                {
                    query = query.Where(s => s.Assignments.Count < s.RequiredEmployees);
                }
            }

            if (request.QualificationIds != null && request.QualificationIds.Any())
            {
                query = query.Where(s => s.ShiftQualifications.Any(sq => request.QualificationIds.Contains(sq.QualificationId)));
            }

            // Apply sorting
            query = request.SortBy.ToLower() switch
            {
                "title" => request.SortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(s => s.Title)
                    : query.OrderBy(s => s.Title),
                "endtime" => request.SortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(s => s.EndTime)
                    : query.OrderBy(s => s.EndTime),
                "location" => request.SortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(s => s.Location!.Name)
                    : query.OrderBy(s => s.Location!.Name),
                _ => request.SortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(s => s.StartTime)
                    : query.OrderBy(s => s.StartTime)
            };

            var totalCount = await query.CountAsync();
            var shifts = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var shiftResponses = shifts.Select(MapToShiftResponse).ToList();

            return new PagedResult<ShiftResponse>
            {
                Data = shiftResponses,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize),
                HasNextPage = request.Page * request.PageSize < totalCount,
                HasPreviousPage = request.Page > 1
            };
        }

        public async Task<ShiftResponse?> GetShiftByIdAsync(int shiftId)
        {
            var shift = await _context.Shifts
                .Include(s => s.Schedule)
                .Include(s => s.Location)
                .Include(s => s.Assignments)
                    .ThenInclude(a => a.Employee)
                .Include(s => s.ShiftQualifications)
                    .ThenInclude(sq => sq.Qualification)
                .FirstOrDefaultAsync(s => s.ShiftId == shiftId && s.IsActive);

            return shift == null ? null : MapToShiftResponse(shift);
        }

        public async Task<ShiftResponse> CreateShiftAsync(CreateShiftRequest request)
        {
            var shift = new Shift
            {
                ScheduleId = request.ScheduleId,
                Title = request.Title,
                Description = request.Description,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                RequiredEmployees = request.RequiredEmployees,
                LocationId = request.LocationId,
                Department = "", // Default empty, can be enhanced
                Position = "", // Default empty, can be enhanced
                IsActive = true
            };

            _context.Shifts.Add(shift);
            await _context.SaveChangesAsync();

            // Add required qualifications
            if (request.RequiredQualificationIds.Any())
            {
                var shiftQualifications = request.RequiredQualificationIds.Select(qId => new ShiftQualification
                {
                    ShiftId = shift.ShiftId,
                    QualificationId = qId,
                    IsRequired = true
                }).ToList();

                _context.ShiftQualifications.AddRange(shiftQualifications);
                await _context.SaveChangesAsync();
            }

            return await GetShiftByIdAsync(shift.ShiftId) ?? throw new InvalidOperationException("Failed to retrieve created shift");
        }

        public async Task<ShiftResponse?> UpdateShiftAsync(int shiftId, UpdateShiftRequest request)
        {
            var shift = await _context.Shifts
                .Include(s => s.ShiftQualifications)
                .FirstOrDefaultAsync(s => s.ShiftId == shiftId && s.IsActive);

            if (shift == null) return null;

            if (!string.IsNullOrEmpty(request.Title))
                shift.Title = request.Title;

            if (request.Description != null)
                shift.Description = request.Description;

            if (request.StartTime.HasValue)
                shift.StartTime = request.StartTime.Value;

            if (request.EndTime.HasValue)
                shift.EndTime = request.EndTime.Value;

            if (request.RequiredEmployees.HasValue)
                shift.RequiredEmployees = request.RequiredEmployees.Value;

            if (request.LocationId.HasValue)
                shift.LocationId = request.LocationId.Value;

            // Update qualifications if provided
            if (request.RequiredQualificationIds != null)
            {
                // Remove existing qualifications
                _context.ShiftQualifications.RemoveRange(shift.ShiftQualifications);

                // Add new qualifications
                var newQualifications = request.RequiredQualificationIds.Select(qId => new ShiftQualification
                {
                    ShiftId = shiftId,
                    QualificationId = qId,
                    IsRequired = true
                }).ToList();

                _context.ShiftQualifications.AddRange(newQualifications);
            }

            await _context.SaveChangesAsync();
            return await GetShiftByIdAsync(shiftId);
        }

        public async Task<bool> DeleteShiftAsync(int shiftId)
        {
            var shift = await _context.Shifts.FindAsync(shiftId);
            if (shift == null) return false;

            shift.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ShiftResponse>> GetShiftsByScheduleAsync(int scheduleId)
        {
            var shifts = await _context.Shifts
                .Include(s => s.Schedule)
                .Include(s => s.Location)
                .Include(s => s.Assignments)
                    .ThenInclude(a => a.Employee)
                .Include(s => s.ShiftQualifications)
                    .ThenInclude(sq => sq.Qualification)
                .Where(s => s.ScheduleId == scheduleId && s.IsActive)
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            return shifts.Select(MapToShiftResponse).ToList();
        }

        public async Task<List<ShiftResponse>> GetShiftsByDateAsync(DateTime date)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1);

            var shifts = await _context.Shifts
                .Include(s => s.Schedule)
                .Include(s => s.Location)
                .Include(s => s.Assignments)
                    .ThenInclude(a => a.Employee)
                .Include(s => s.ShiftQualifications)
                    .ThenInclude(sq => sq.Qualification)
                .Where(s => s.IsActive && s.StartTime >= startOfDay && s.StartTime < endOfDay)
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            return shifts.Select(MapToShiftResponse).ToList();
        }

        public async Task<List<ShiftResponse>> GetShiftsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var shifts = await _context.Shifts
                .Include(s => s.Schedule)
                .Include(s => s.Location)
                .Include(s => s.Assignments)
                    .ThenInclude(a => a.Employee)
                .Include(s => s.ShiftQualifications)
                    .ThenInclude(sq => sq.Qualification)
                .Where(s => s.IsActive && s.StartTime >= startDate && s.StartTime <= endDate)
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            return shifts.Select(MapToShiftResponse).ToList();
        }

        public async Task<List<ShiftResponse>> GetTodaysShiftsAsync()
        {
            return await GetShiftsByDateAsync(DateTime.Today);
        }

        public async Task<List<ShiftResponse>> GetTomorrowsShiftsAsync()
        {
            return await GetShiftsByDateAsync(DateTime.Today.AddDays(1));
        }

        public async Task<List<ShiftResponse>> GetThisWeeksShiftsAsync()
        {
            var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(7);

            return await GetShiftsByDateRangeAsync(startOfWeek, endOfWeek);
        }

        // NEW METHOD: Get shifts by department
        public async Task<List<ShiftResponse>> GetShiftsByDepartmentAsync(string department)
        {
            var shifts = await _context.Shifts
                .Include(s => s.Schedule)
                .Include(s => s.Location)
                .Include(s => s.Assignments)
                    .ThenInclude(a => a.Employee)
                .Include(s => s.ShiftQualifications)
                    .ThenInclude(sq => sq.Qualification)
                .Where(s => s.Department == department && s.IsActive)
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            return shifts.Select(MapToShiftResponse).ToList();
        }

        // NEW METHOD: Get shifts by location
        public async Task<List<ShiftResponse>> GetShiftsByLocationAsync(int locationId)
        {
            var shifts = await _context.Shifts
                .Include(s => s.Schedule)
                .Include(s => s.Location)
                .Include(s => s.Assignments)
                    .ThenInclude(a => a.Employee)
                .Include(s => s.ShiftQualifications)
                    .ThenInclude(sq => sq.Qualification)
                .Where(s => s.LocationId == locationId && s.IsActive)
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            return shifts.Select(MapToShiftResponse).ToList();
        }

        // UPDATED METHOD: Changed return type from EmployeeResponse to EmployeeInfo
        public async Task<List<EmployeeInfo>> GetAvailableEmployeesForShiftAsync(int shiftId)
        {
            var shift = await _context.Shifts.FindAsync(shiftId);
            if (shift == null) return new List<EmployeeInfo>();

            var dayOfWeek = shift.StartTime.DayOfWeek;
            var shiftStartTime = shift.StartTime.TimeOfDay;
            var shiftEndTime = shift.EndTime.TimeOfDay;

            var availableEmployees = await _context.Employees
                .Include(e => e.Availabilities)
                .Include(e => e.Assignments)
                .Where(e => e.IsActive &&
                    e.Availabilities.Any(a => a.DayOfWeek == dayOfWeek &&
                        a.IsAvailable &&
                        a.StartTime <= shiftStartTime &&
                        a.EndTime >= shiftEndTime) &&
                    !e.Assignments.Any(a => a.Shift.StartTime < shift.EndTime && a.Shift.EndTime > shift.StartTime))
                .ToListAsync();

            return availableEmployees.Select(e => new EmployeeInfo
            {
                EmployeeId = e.EmployeeId,
                EmployeeNumber = e.EmployeeNumber,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Email = e.Email,
                PhoneNumber = e.PhoneNumber,
                Department = e.Department,
                Position = e.Position,
                HourlyRate = e.HourlyRate,
                IsActive = e.IsActive
            }).ToList();
        }

        // UPDATED METHOD: Changed return type from EmployeeResponse to EmployeeInfo
        public async Task<List<EmployeeInfo>> GetPreferredEmployeesForShiftAsync(int shiftId)
        {
            // For now, return available employees. This could be enhanced with preference logic
            return await GetAvailableEmployeesForShiftAsync(shiftId);
        }

        // NEW METHOD: Get shift conflicts for an employee
        public async Task<List<ShiftResponse>> GetShiftConflictsAsync(int employeeId, DateTime startTime, DateTime endTime)
        {
            var conflicts = await _context.Shifts
                .Include(s => s.Schedule)
                .Include(s => s.Location)
                .Include(s => s.Assignments)
                    .ThenInclude(a => a.Employee)
                .Include(s => s.ShiftQualifications)
                    .ThenInclude(sq => sq.Qualification)
                .Where(s => s.Assignments.Any(a => a.EmployeeId == employeeId) &&
                           s.StartTime < endTime &&
                           s.EndTime > startTime &&
                           s.IsActive)
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            return conflicts.Select(MapToShiftResponse).ToList();
        }

        // NEW METHOD: Get understaffed shifts
        public async Task<List<ShiftResponse>> GetUnderstaffedShiftsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Shifts
                .Include(s => s.Schedule)
                .Include(s => s.Location)
                .Include(s => s.Assignments)
                    .ThenInclude(a => a.Employee)
                .Include(s => s.ShiftQualifications)
                    .ThenInclude(sq => sq.Qualification)
                .Where(s => s.IsActive);

            if (startDate.HasValue)
                query = query.Where(s => s.StartTime.Date >= startDate.Value.Date);

            if (endDate.HasValue)
                query = query.Where(s => s.EndTime.Date <= endDate.Value.Date);

            var understaffedShifts = await query
                .Where(s => s.Assignments.Count < s.RequiredEmployees)
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            return understaffedShifts.Select(MapToShiftResponse).ToList();
        }

        // EXISTING METHODS (keeping for backward compatibility)
        public async Task<bool> ShiftExistsAsync(int shiftId)
        {
            return await _context.Shifts.AnyAsync(s => s.ShiftId == shiftId && s.IsActive);
        }

        public async Task<bool> HasSchedulingConflictAsync(int employeeId, DateTime startTime, DateTime endTime, int? excludeShiftId = null)
        {
            var query = _context.Assignments
                .Include(a => a.Shift)
                .Where(a => a.EmployeeId == employeeId &&
                    a.Shift.StartTime < endTime &&
                    a.Shift.EndTime > startTime);

            if (excludeShiftId.HasValue)
            {
                query = query.Where(a => a.ShiftId != excludeShiftId.Value);
            }

            return await query.AnyAsync();
        }

        private static ShiftResponse MapToShiftResponse(Shift shift)
        {
            return new ShiftResponse
            {
                ShiftId = shift.ShiftId,
                ScheduleId = shift.ScheduleId,
                ScheduleName = shift.Schedule?.Name ?? "",
                Title = shift.Title,
                Description = shift.Description,
                StartTime = shift.StartTime,
                EndTime = shift.EndTime,
                Duration = shift.EndTime - shift.StartTime,
                RequiredEmployees = shift.RequiredEmployees,
                AssignedEmployees = shift.Assignments?.Count ?? 0,
                IsFullyStaffed = (shift.Assignments?.Count ?? 0) >= shift.RequiredEmployees,
                LocationId = shift.LocationId,
                LocationName = shift.Location?.Name,
                IsActive = shift.IsActive,
                RequiredQualifications = shift.ShiftQualifications?.Select(sq => new QualificationInfo
                {
                    QualificationId = sq.QualificationId,
                    Name = sq.Qualification?.Name ?? "",
                    Description = sq.Qualification?.Description,
                    IsRequired = sq.IsRequired
                }).ToList() ?? new List<QualificationInfo>(),
                Assignments = shift.Assignments?.Select(a => new AssignmentInfo
                {
                    AssignmentId = a.AssignmentId,
                    EmployeeId = a.EmployeeId,
                    EmployeeName = $"{a.Employee?.FirstName} {a.Employee?.LastName}".Trim(),
                    EmployeeNumber = a.Employee?.EmployeeNumber ?? "",
                    IsConfirmed = a.IsConfirmed,
                    AssignedAt = a.AssignedAt
                }).ToList() ?? new List<AssignmentInfo>()
            };
        }
    }
}

