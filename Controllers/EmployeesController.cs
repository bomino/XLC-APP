using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EmployeeScheduling.API.Services;
using EmployeeScheduling.API.DTOs;
using System.Security.Claims;

namespace EmployeeScheduling.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeesController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        /// <summary>
        /// Get all employees with search, filter, and pagination
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<PagedResult<EmployeeResponse>>> GetEmployees([FromQuery] EmployeeSearchRequest request)
        {
            try
            {
                var result = await _employeeService.GetEmployeesAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving employees", error = ex.Message });
            }
        }

        /// <summary>
        /// Get employee by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeeResponse>> GetEmployee(int id)
        {
            try
            {
                // Get current user info
                var currentUserIdClaim = User.FindFirst("userId")?.Value;
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (currentUserIdClaim == null || !int.TryParse(currentUserIdClaim, out int currentUserId))
                {
                    return Unauthorized();
                }

                var employee = await _employeeService.GetEmployeeByIdAsync(id);
                if (employee == null)
                {
                    return NotFound(new { message = "Employee not found" });
                }

                // Authorization: Admin/Manager can view any employee, Employee can only view their own record
                if (currentUserRole != "Admin" && currentUserRole != "Manager")
                {
                    if (employee.UserId != currentUserId)
                    {
                        return Forbid("You can only view your own employee record"); ;
                    }
                }

                return Ok(employee);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the employee", error = ex.Message });
            }
        }

        /// <summary>
        /// Get current user's employee profile
        /// </summary>
        [HttpGet("me")]
        public async Task<ActionResult<EmployeeResponse>> GetMyProfile()
        {
            try
            {
                var currentUserIdClaim = User.FindFirst("userId")?.Value;
                if (currentUserIdClaim == null || !int.TryParse(currentUserIdClaim, out int currentUserId))
                {
                    return Unauthorized();
                }

                var employee = await _employeeService.GetEmployeeByUserIdAsync(currentUserId);
                if (employee == null)
                {
                    return NotFound(new { message = "Employee profile not found for current user" });
                }

                return Ok(employee);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving your profile", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new employee
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<EmployeeResponse>> CreateEmployee(CreateEmployeeRequest request)
        {
            try
            {
                // Validate employee number uniqueness
                if (await _employeeService.EmployeeNumberExistsAsync(request.EmployeeNumber))
                {
                    return BadRequest(new { message = "Employee number already exists" });
                }

                var employee = await _employeeService.CreateEmployeeAsync(request);
                return CreatedAtAction(nameof(GetEmployee), new { id = employee.EmployeeId }, employee);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the employee", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing employee
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<EmployeeResponse>> UpdateEmployee(int id, UpdateEmployeeRequest request)
        {
            try
            {
                if (!await _employeeService.EmployeeExistsAsync(id))
                {
                    return NotFound(new { message = "Employee not found" });
                }

                var updatedEmployee = await _employeeService.UpdateEmployeeAsync(id, request);
                if (updatedEmployee == null)
                {
                    return NotFound(new { message = "Employee not found" });
                }

                return Ok(updatedEmployee);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the employee", error = ex.Message });
            }
        }

        /// <summary>
        /// Update current user's own profile (limited fields)
        /// </summary>
        [HttpPut("me")]
        public async Task<ActionResult<EmployeeResponse>> UpdateMyProfile(UpdateEmployeeRequest request)
        {
            try
            {
                var currentUserIdClaim = User.FindFirst("userId")?.Value;
                if (currentUserIdClaim == null || !int.TryParse(currentUserIdClaim, out int currentUserId))
                {
                    return Unauthorized();
                }

                var employee = await _employeeService.GetEmployeeByUserIdAsync(currentUserId);
                if (employee == null)
                {
                    return NotFound(new { message = "Employee profile not found" });
                }

                // Employees can only update limited fields
                var limitedRequest = new UpdateEmployeeRequest
                {
                    PhoneNumber = request.PhoneNumber,
                    Address = request.Address,
                    // Add other fields employees should be allowed to update
                };

                var updatedEmployee = await _employeeService.UpdateEmployeeAsync(employee.EmployeeId, limitedRequest);
                return Ok(updatedEmployee);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating your profile", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete an employee (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            try
            {
                if (!await _employeeService.EmployeeExistsAsync(id))
                {
                    return NotFound(new { message = "Employee not found" });
                }

                var result = await _employeeService.DeleteEmployeeAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Employee not found" });
                }

                return Ok(new { message = "Employee deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the employee", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all departments
        /// </summary>
        [HttpGet("departments")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<List<string>>> GetDepartments()
        {
            try
            {
                var departments = await _employeeService.GetDepartmentsAsync();
                return Ok(departments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving departments", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all positions
        /// </summary>
        [HttpGet("positions")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<List<string>>> GetPositions()
        {
            try
            {
                var positions = await _employeeService.GetPositionsAsync();
                return Ok(positions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving positions", error = ex.Message });
            }
        }

        /// <summary>
        /// Get employees by manager
        /// </summary>
        [HttpGet("manager/{managerId}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<List<EmployeeResponse>>> GetEmployeesByManager(int managerId)
        {
            try
            {
                // Additional authorization: Managers can only see their own subordinates
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var currentUserIdClaim = User.FindFirst("userId")?.Value;

                if (currentUserRole == "Manager" && currentUserIdClaim != null)
                {
                    if (int.TryParse(currentUserIdClaim, out int currentUserId))
                    {
                        var currentManager = await _employeeService.GetEmployeeByUserIdAsync(currentUserId);
                        if (currentManager != null && currentManager.EmployeeId != managerId)
                        {
                            return Forbid("You can only view your own subordinates");
                        }
                    }
                }

                var employees = await _employeeService.GetEmployeesByManagerAsync(managerId);
                return Ok(employees);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving employees", error = ex.Message });
            }
        }

        /// <summary>
        /// Search employees (simplified endpoint for quick searches)
        /// </summary>
        [HttpGet("search")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<List<EmployeeResponse>>> SearchEmployees([FromQuery] string searchTerm, [FromQuery] int limit = 10)
        {
            try
            {
                var request = new EmployeeSearchRequest
                {
                    SearchTerm = searchTerm,
                    PageSize = limit,
                    Page = 1
                };

                var result = await _employeeService.GetEmployeesAsync(request);
                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while searching employees", error = ex.Message });
            }
        }
    }
}
