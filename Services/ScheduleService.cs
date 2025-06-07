using Microsoft.EntityFrameworkCore;
using EmployeeScheduling.API.Data;
using EmployeeScheduling.API.DTOs;
using EmployeeScheduling.API.Models;

namespace EmployeeScheduling.API.Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly ApplicationDbContext _context;

        public ScheduleService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<ScheduleResponse>> GetSchedulesAsync(ScheduleSearchRequest request)
        {
            var query = _context.Schedules
                .Include(s => s.CreatedByUser)
                .Include(s => s.Shifts)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(s =>
                    s.Name.ToLower().Contains(searchTerm) ||
                    (s.Description != null && s.Description.ToLower().Contains(searchTerm)));
            }

            if (request.IsPublished.HasValue)
            {
                query = query.Where(s => s.IsPublished == request.IsPublished.Value);
            }

            if (request.StartDateFrom.HasValue)
            {
                query = query.Where(s => s.StartDate >= request.StartDateFrom.Value);
            }

            if (request.StartDateTo.HasValue)
            {
                query = query.Where(s => s.StartDate <= request.StartDateTo.Value);
            }

            if (request.CreatedByUserId.HasValue)
            {
                query = query.Where(s => s.CreatedByUserId == request.CreatedByUserId.Value);
            }

            // Apply sorting
            query = request.SortBy.ToLower() switch
            {
                "name" => request.SortDirection.ToLower() == "desc" 
                    ? query.OrderByDescending(s => s.Name)
                    : query.OrderBy(s => s.Name),
                "enddate" => request.SortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(s => s.EndDate)
                    : query.OrderBy(s => s.EndDate),
                "createdat" => request.SortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(s => s.CreatedAt)
                    : query.OrderBy(s => s.CreatedAt),
                _ => request.SortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(s => s.StartDate)
                    : query.OrderBy(s => s.StartDate)
            };

            var totalCount = await query.CountAsync();
            var schedules = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var scheduleResponses = schedules.Select(MapToScheduleResponse).ToList();

            return new PagedResult<ScheduleResponse>
            {
                Data = scheduleResponses,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize),
                HasNextPage = request.Page * request.PageSize < totalCount,
                HasPreviousPage = request.Page > 1
            };
        }

        public async Task<ScheduleResponse?> GetScheduleByIdAsync(int scheduleId)
        {
            var schedule = await _context.Schedules
                .Include(s => s.CreatedByUser)
                .Include(s => s.Shifts)
                    .ThenInclude(sh => sh.Assignments)
                .FirstOrDefaultAsync(s => s.ScheduleId == scheduleId);

            return schedule == null ? null : MapToScheduleResponse(schedule);
        }

        // UPDATED METHOD: Removed int createdByUserId parameter
        public async Task<ScheduleResponse> CreateScheduleAsync(CreateScheduleRequest request)
        {
            var schedule = new Schedule
            {
                Name = request.Name,
                Description = request.Description,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                IsPublished = false,
                CreatedByUserId = 1, // Default to admin user
                CreatedAt = DateTime.UtcNow
            };

            _context.Schedules.Add(schedule);
            await _context.SaveChangesAsync();

            return await GetScheduleByIdAsync(schedule.ScheduleId) ?? throw new InvalidOperationException("Failed to retrieve created schedule");
        }

        public async Task<ScheduleResponse?> UpdateScheduleAsync(int scheduleId, UpdateScheduleRequest request)
        {
            var schedule = await _context.Schedules.FindAsync(scheduleId);
            if (schedule == null) return null;

            if (!string.IsNullOrEmpty(request.Name))
                schedule.Name = request.Name;

            if (request.Description != null)
                schedule.Description = request.Description;

            if (request.StartDate.HasValue)
                schedule.StartDate = request.StartDate.Value;

            if (request.EndDate.HasValue)
                schedule.EndDate = request.EndDate.Value;

            schedule.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await GetScheduleByIdAsync(scheduleId);
        }

        public async Task<bool> DeleteScheduleAsync(int scheduleId)
        {
            var schedule = await _context.Schedules.FindAsync(scheduleId);
            if (schedule == null) return false;

            _context.Schedules.Remove(schedule);
            await _context.SaveChangesAsync();
            return true;
        }

        // UPDATED METHOD: Changed return type from bool to ScheduleResponse?
        public async Task<ScheduleResponse?> PublishScheduleAsync(int scheduleId)
        {
            var schedule = await _context.Schedules.FindAsync(scheduleId);
            if (schedule == null) return null;

            schedule.IsPublished = true;
            schedule.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            
            return await GetScheduleByIdAsync(scheduleId);
        }

        // UPDATED METHOD: Changed return type from bool to ScheduleResponse?
        public async Task<ScheduleResponse?> UnpublishScheduleAsync(int scheduleId)
        {
            var schedule = await _context.Schedules.FindAsync(scheduleId);
            if (schedule == null) return null;

            schedule.IsPublished = false;
            schedule.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            
            return await GetScheduleByIdAsync(scheduleId);
        }

        // NEW METHOD: Get current active schedule
        public async Task<ScheduleResponse?> GetCurrentScheduleAsync()
        {
            var today = DateTime.Today;
            var schedule = await _context.Schedules
                .Include(s => s.CreatedByUser)
                .Include(s => s.Shifts)
                    .ThenInclude(sh => sh.Assignments)
                .FirstOrDefaultAsync(s => s.IsPublished && s.StartDate <= today && s.EndDate >= today);

            return schedule == null ? null : MapToScheduleResponse(schedule);
        }

        // NEW METHOD: Get upcoming schedules
        public async Task<List<ScheduleResponse>> GetUpcomingSchedulesAsync(int days = 30)
        {
            var startDate = DateTime.Today;
            var endDate = startDate.AddDays(days);

            var schedules = await _context.Schedules
                .Include(s => s.CreatedByUser)
                .Include(s => s.Shifts)
                    .ThenInclude(sh => sh.Assignments)
                .Where(s => s.StartDate >= startDate && s.StartDate <= endDate)
                .OrderBy(s => s.StartDate)
                .ToListAsync();

            return schedules.Select(MapToScheduleResponse).ToList();
        }

        // NEW METHOD: Get schedules by date range
        public async Task<List<ScheduleResponse>> GetSchedulesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var schedules = await _context.Schedules
                .Include(s => s.CreatedByUser)
                .Include(s => s.Shifts)
                    .ThenInclude(sh => sh.Assignments)
                .Where(s => s.StartDate <= endDate && s.EndDate >= startDate)
                .OrderBy(s => s.StartDate)
                .ToListAsync();

            return schedules.Select(MapToScheduleResponse).ToList();
        }

        // EXISTING METHODS (keeping for backward compatibility)
        public async Task<bool> ScheduleExistsAsync(int scheduleId)
        {
            return await _context.Schedules.AnyAsync(s => s.ScheduleId == scheduleId);
        }

        public async Task<List<ScheduleResponse>> GetCurrentSchedulesAsync()
        {
            var today = DateTime.Today;
            var schedules = await _context.Schedules
                .Include(s => s.CreatedByUser)
                .Include(s => s.Shifts)
                    .ThenInclude(sh => sh.Assignments)
                .Where(s => s.IsPublished && s.StartDate <= today && s.EndDate >= today)
                .OrderBy(s => s.StartDate)
                .ToListAsync();

            return schedules.Select(MapToScheduleResponse).ToList();
        }

        public async Task<List<ScheduleResponse>> GetUpcomingSchedulesAsync()
        {
            var today = DateTime.Today;
            var schedules = await _context.Schedules
                .Include(s => s.CreatedByUser)
                .Include(s => s.Shifts)
                    .ThenInclude(sh => sh.Assignments)
                .Where(s => s.IsPublished && s.StartDate > today)
                .OrderBy(s => s.StartDate)
                .ToListAsync();

            return schedules.Select(MapToScheduleResponse).ToList();
        }

        public async Task<ScheduleResponse?> GetScheduleByDateAsync(DateTime date)
        {
            var schedule = await _context.Schedules
                .Include(s => s.CreatedByUser)
                .Include(s => s.Shifts)
                    .ThenInclude(sh => sh.Assignments)
                .FirstOrDefaultAsync(s => s.IsPublished && s.StartDate <= date && s.EndDate >= date);

            return schedule == null ? null : MapToScheduleResponse(schedule);
        }

        private static ScheduleResponse MapToScheduleResponse(Schedule schedule)
        {
            return new ScheduleResponse
            {
                ScheduleId = schedule.ScheduleId,
                Name = schedule.Name,
                Description = schedule.Description,
                StartDate = schedule.StartDate,
                EndDate = schedule.EndDate,
                IsPublished = schedule.IsPublished,
                CreatedByUserId = schedule.CreatedByUserId,
                CreatedByUserName = $"{schedule.CreatedByUser?.FirstName} {schedule.CreatedByUser?.LastName}".Trim(),
                CreatedAt = schedule.CreatedAt,
                UpdatedAt = schedule.UpdatedAt,
                TotalShifts = schedule.Shifts?.Count ?? 0,
                TotalAssignments = schedule.Shifts?.SelectMany(s => s.Assignments ?? new List<Assignment>()).Count() ?? 0
            };
        }
    }
}

