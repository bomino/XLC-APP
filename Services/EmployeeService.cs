using Microsoft.EntityFrameworkCore;
using EmployeeScheduling.API.Data;
using EmployeeScheduling.API.DTOs;
using EmployeeScheduling.API.Models;

namespace EmployeeScheduling.API.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly ApplicationDbContext _context;

        public EmployeeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<EmployeeResponse>> GetEmployeesAsync(EmployeeSearchRequest request)
        {
            var query = _context.Employees
                .Include(e => e.Manager)
                .Where(e => e.IsActive);

            // Apply filters
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(e =>
                    e.FirstName.ToLower().Contains(searchTerm) ||
                    e.LastName.ToLower().Contains(searchTerm) ||
                    e.Email.ToLower().Contains(searchTerm) ||
                    e.EmployeeNumber.ToLower().Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(request.Department))
            {
                query = query.Where(e => e.Department == request.Department);
            }

            if (!string.IsNullOrEmpty(request.Position))
            {
                query = query.Where(e => e.Position == request.Position);
            }

            if (!string.IsNullOrEmpty(request.Status))
            {
                query = query.Where(e => e.Status == request.Status);
            }

            if (!string.IsNullOrEmpty(request.EmploymentType))
            {
                query = query.Where(e => e.EmploymentType == request.EmploymentType);
            }

            if (request.ManagerId.HasValue)
            {
                query = query.Where(e => e.ManagerId == request.ManagerId);
            }

            // Apply sorting
            query = request.SortBy.ToLower() switch
            {
                "firstname" => request.SortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(e => e.FirstName)
                    : query.OrderBy(e => e.FirstName),
                "lastname" => request.SortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(e => e.LastName)
                    : query.OrderBy(e => e.LastName),
                "department" => request.SortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(e => e.Department)
                    : query.OrderBy(e => e.Department),
                "position" => request.SortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(e => e.Position)
                    : query.OrderBy(e => e.Position),
                "hiredate" => request.SortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(e => e.HireDate)
                    : query.OrderBy(e => e.HireDate),
                _ => query.OrderBy(e => e.LastName)
            };

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination
            var employees = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(e => new EmployeeResponse
                {
                    EmployeeId = e.EmployeeId,
                    UserId = e.UserId,
                    EmployeeNumber = e.EmployeeNumber,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    FullName = e.FullName,
                    Email = e.Email,
                    PhoneNumber = e.PhoneNumber,
                    Address = e.Address,
                    DateOfBirth = e.DateOfBirth,
                    Age = e.Age,
                    HireDate = e.HireDate,
                    TerminationDate = e.TerminationDate,
                    Department = e.Department,
                    Position = e.Position,
                    HourlyRate = e.HourlyRate,
                    EmploymentType = e.EmploymentType,
                    Status = e.Status,
                    ManagerId = e.ManagerId,
                    ManagerName = e.Manager != null ? e.Manager.FullName : null,
                    Notes = e.Notes,
                    MaxHoursPerWeek = e.MaxHoursPerWeek,
                    MinHoursPerWeek = e.MinHoursPerWeek,
                    YearsOfService = e.YearsOfService,
                    IsActive = e.IsActive,
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt
                })
                .ToListAsync();

            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

            return new PagedResult<EmployeeResponse>
            {
                Data = employees,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = totalPages,
                HasNextPage = request.Page < totalPages,
                HasPreviousPage = request.Page > 1
            };
        }

        public async Task<EmployeeResponse?> GetEmployeeByIdAsync(int employeeId)
        {
            var employee = await _context.Employees
                .Include(e => e.Manager)
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.IsActive);

            if (employee == null)
                return null;

            return new EmployeeResponse
            {
                EmployeeId = employee.EmployeeId,
                UserId = employee.UserId,
                EmployeeNumber = employee.EmployeeNumber,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                FullName = employee.FullName,
                Email = employee.Email,
                PhoneNumber = employee.PhoneNumber,
                Address = employee.Address,
                DateOfBirth = employee.DateOfBirth,
                Age = employee.Age,
                HireDate = employee.HireDate,
                TerminationDate = employee.TerminationDate,
                Department = employee.Department,
                Position = employee.Position,
                HourlyRate = employee.HourlyRate,
                EmploymentType = employee.EmploymentType,
                Status = employee.Status,
                ManagerId = employee.ManagerId,
                ManagerName = employee.Manager?.FullName,
                Notes = employee.Notes,
                MaxHoursPerWeek = employee.MaxHoursPerWeek,
                MinHoursPerWeek = employee.MinHoursPerWeek,
                YearsOfService = employee.YearsOfService,
                IsActive = employee.IsActive,
                CreatedAt = employee.CreatedAt,
                UpdatedAt = employee.UpdatedAt
            };
        }

        public async Task<EmployeeResponse?> GetEmployeeByUserIdAsync(int userId)
        {
            var employee = await _context.Employees
                .Include(e => e.Manager)
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.UserId == userId && e.IsActive);

            if (employee == null)
                return null;

            return new EmployeeResponse
            {
                EmployeeId = employee.EmployeeId,
                UserId = employee.UserId,
                EmployeeNumber = employee.EmployeeNumber,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                FullName = employee.FullName,
                Email = employee.Email,
                PhoneNumber = employee.PhoneNumber,
                Address = employee.Address,
                DateOfBirth = employee.DateOfBirth,
                Age = employee.Age,
                HireDate = employee.HireDate,
                TerminationDate = employee.TerminationDate,
                Department = employee.Department,
                Position = employee.Position,
                HourlyRate = employee.HourlyRate,
                EmploymentType = employee.EmploymentType,
                Status = employee.Status,
                ManagerId = employee.ManagerId,
                ManagerName = employee.Manager?.FullName,
                Notes = employee.Notes,
                MaxHoursPerWeek = employee.MaxHoursPerWeek,
                MinHoursPerWeek = employee.MinHoursPerWeek,
                YearsOfService = employee.YearsOfService,
                IsActive = employee.IsActive,
                CreatedAt = employee.CreatedAt,
                UpdatedAt = employee.UpdatedAt
            };
        }

        public async Task<EmployeeResponse> CreateEmployeeAsync(CreateEmployeeRequest request)
        {
            var employee = new Employee
            {
                UserId = request.UserId ?? 0,
                EmployeeNumber = request.EmployeeNumber,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Address = request.Address,
                DateOfBirth = request.DateOfBirth,
                HireDate = request.HireDate,
                Department = request.Department,
                Position = request.Position,
                HourlyRate = request.HourlyRate,
                EmploymentType = request.EmploymentType,
                ManagerId = request.ManagerId,
                Notes = request.Notes,
                MaxHoursPerWeek = request.MaxHoursPerWeek,
                MinHoursPerWeek = request.MinHoursPerWeek,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            return await GetEmployeeByIdAsync(employee.EmployeeId) ?? throw new InvalidOperationException("Failed to retrieve created employee");
        }

        public async Task<EmployeeResponse?> UpdateEmployeeAsync(int employeeId, UpdateEmployeeRequest request)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null || !employee.IsActive)
                return null;

            // Update only provided fields
            if (!string.IsNullOrEmpty(request.FirstName))
                employee.FirstName = request.FirstName;

            if (!string.IsNullOrEmpty(request.LastName))
                employee.LastName = request.LastName;

            if (!string.IsNullOrEmpty(request.Email))
                employee.Email = request.Email;

            if (request.PhoneNumber != null)
                employee.PhoneNumber = request.PhoneNumber;

            if (request.Address != null)
                employee.Address = request.Address;

            if (request.DateOfBirth.HasValue)
                employee.DateOfBirth = request.DateOfBirth;

            if (!string.IsNullOrEmpty(request.Department))
                employee.Department = request.Department;

            if (!string.IsNullOrEmpty(request.Position))
                employee.Position = request.Position;

            if (request.HourlyRate.HasValue)
                employee.HourlyRate = request.HourlyRate.Value;

            if (!string.IsNullOrEmpty(request.EmploymentType))
                employee.EmploymentType = request.EmploymentType;

            if (!string.IsNullOrEmpty(request.Status))
                employee.Status = request.Status;

            if (request.ManagerId.HasValue)
                employee.ManagerId = request.ManagerId;

            if (request.Notes != null)
                employee.Notes = request.Notes;

            if (request.MaxHoursPerWeek.HasValue)
                employee.MaxHoursPerWeek = request.MaxHoursPerWeek.Value;

            if (request.MinHoursPerWeek.HasValue)
                employee.MinHoursPerWeek = request.MinHoursPerWeek.Value;

            if (request.TerminationDate.HasValue)
                employee.TerminationDate = request.TerminationDate;

            employee.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetEmployeeByIdAsync(employeeId);
        }

        public async Task<bool> DeleteEmployeeAsync(int employeeId)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null)
                return false;

            // Soft delete
            employee.IsActive = false;
            employee.Status = "Terminated";
            employee.TerminationDate = DateTime.UtcNow;
            employee.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EmployeeExistsAsync(int employeeId)
        {
            return await _context.Employees.AnyAsync(e => e.EmployeeId == employeeId && e.IsActive);
        }

        public async Task<bool> EmployeeNumberExistsAsync(string employeeNumber, int? excludeEmployeeId = null)
        {
            var query = _context.Employees.Where(e => e.EmployeeNumber == employeeNumber && e.IsActive);

            if (excludeEmployeeId.HasValue)
            {
                query = query.Where(e => e.EmployeeId != excludeEmployeeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<List<string>> GetDepartmentsAsync()
        {
            return await _context.Employees
                .Where(e => e.IsActive)
                .Select(e => e.Department)
                .Distinct()
                .OrderBy(d => d)
                .ToListAsync();
        }

        public async Task<List<string>> GetPositionsAsync()
        {
            return await _context.Employees
                .Where(e => e.IsActive)
                .Select(e => e.Position)
                .Distinct()
                .OrderBy(p => p)
                .ToListAsync();
        }

        public async Task<List<EmployeeResponse>> GetEmployeesByManagerAsync(int managerId)
        {
            return await _context.Employees
                .Where(e => e.ManagerId == managerId && e.IsActive)
                .Select(e => new EmployeeResponse
                {
                    EmployeeId = e.EmployeeId,
                    UserId = e.UserId,
                    EmployeeNumber = e.EmployeeNumber,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    FullName = e.FullName,
                    Email = e.Email,
                    Department = e.Department,
                    Position = e.Position,
                    Status = e.Status,
                    HireDate = e.HireDate
                })
                .OrderBy(e => e.LastName)
                .ToListAsync();
        }
    }
}

