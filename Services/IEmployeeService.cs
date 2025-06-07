using EmployeeScheduling.API.DTOs;
using EmployeeScheduling.API.Models;

namespace EmployeeScheduling.API.Services
{
    public interface IEmployeeService
    {
        Task<PagedResult<EmployeeResponse>> GetEmployeesAsync(EmployeeSearchRequest request);
        Task<EmployeeResponse?> GetEmployeeByIdAsync(int employeeId);
        Task<EmployeeResponse?> GetEmployeeByUserIdAsync(int userId);
        Task<EmployeeResponse> CreateEmployeeAsync(CreateEmployeeRequest request);
        Task<EmployeeResponse?> UpdateEmployeeAsync(int employeeId, UpdateEmployeeRequest request);
        Task<bool> DeleteEmployeeAsync(int employeeId);
        Task<bool> EmployeeExistsAsync(int employeeId);
        Task<bool> EmployeeNumberExistsAsync(string employeeNumber, int? excludeEmployeeId = null);
        Task<List<string>> GetDepartmentsAsync();
        Task<List<string>> GetPositionsAsync();
        Task<List<EmployeeResponse>> GetEmployeesByManagerAsync(int managerId);
    }
}
