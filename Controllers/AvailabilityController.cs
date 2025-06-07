using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EmployeeScheduling.API.Services;
using EmployeeScheduling.API.DTOs;
using EmployeeScheduling.API.Models;
using System.Security.Claims;

namespace EmployeeScheduling.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AvailabilityController : ControllerBase
    {
        private readonly IAvailabilityService _availabilityService;

        public AvailabilityController(IAvailabilityService availabilityService)
        {
            _availabilityService = availabilityService;
        }

        /// <summary>
        /// Get all availability records (Admin/Manager only)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<List<AvailabilityResponse>>> GetAllAvailability(
            [FromQuery] DayOfWeek? dayOfWeek = null,
            [FromQuery] AvailabilityType? availabilityType = null)
        {
            try
            {
                var availability = await _availabilityService.GetAllAvailabilityAsync(dayOfWeek, availabilityType);
                return Ok(availability);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving availability", error = ex.Message });
            }
        }

        /// <summary>
        /// Get availability by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<AvailabilityResponse>> GetAvailability(int id)
        {
            try
            {
                var availability = await _availabilityService.GetAvailabilityByIdAsync(id);
                if (availability == null)
                    return NotFound(new { message = "Availability not found" });

                // Check authorization - employees can only view their own availability
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                if (userRole == "Employee")
                {
                    var userId = User.FindFirst("userId")?.Value;
                    // You would need to verify that this availability belongs to the current user
                }

                return Ok(availability);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the availability", error = ex.Message });
            }
        }

        /// <summary>
        /// Get availability for a specific employee
        /// </summary>
        [HttpGet("employee/{employeeId}")]
        public async Task<ActionResult<List<AvailabilityResponse>>> GetEmployeeAvailability(int employeeId)
        {
            try
            {
                // Check authorization - employees can only view their own availability
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                if (userRole == "Employee")
                {
                    var userId = User.FindFirst("userId")?.Value;
                    // You would need to verify that the employeeId belongs to the current user
                }

                var availability = await _availabilityService.GetEmployeeAvailabilityAsync(employeeId);
                return Ok(availability);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving employee availability", error = ex.Message });
            }
        }

        /// <summary>
        /// Get current user's availability
        /// </summary>
        [HttpGet("my-availability")]
        public async Task<ActionResult<List<AvailabilityResponse>>> GetMyAvailability()
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User ID not found in token" });

                // You would need to implement logic to get employee ID from user ID
                var employeeId = int.Parse(userId); // This needs proper implementation

                var availability = await _availabilityService.GetEmployeeAvailabilityAsync(employeeId);
                return Ok(availability);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving your availability", error = ex.Message });
            }
        }

        /// <summary>
        /// Create new availability
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<AvailabilityResponse>> CreateAvailability([FromBody] CreateAvailabilityRequest request)
        {
            try
            {
                // Check authorization - employees can only create their own availability
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                if (userRole == "Employee")
                {
                    var userId = User.FindFirst("userId")?.Value;
                    // You would need to verify that the request.EmployeeId belongs to the current user
                    // For now, override the EmployeeId with the current user's employee ID
                    var currentEmployeeId = int.Parse(userId ?? "0"); // This needs proper implementation
                    request.EmployeeId = currentEmployeeId;
                }

                var availability = await _availabilityService.CreateAvailabilityAsync(request);
                return CreatedAtAction(nameof(GetAvailability), new { id = availability.AvailabilityId }, availability);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the availability", error = ex.Message });
            }
        }

        /// <summary>
        /// Create multiple availability records at once
        /// </summary>
        [HttpPost("bulk")]
        public async Task<ActionResult<List<AvailabilityResponse>>> CreateBulkAvailability([FromBody] BulkAvailabilityRequest request)
        {
            try
            {
                // Check authorization - employees can only create their own availability
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                if (userRole == "Employee")
                {
                    var userId = User.FindFirst("userId")?.Value;
                    var currentEmployeeId = int.Parse(userId ?? "0"); // This needs proper implementation
                    request.EmployeeId = currentEmployeeId;
                }

                var availability = await _availabilityService.BulkCreateAvailabilityAsync(request.EmployeeId, request.AvailabilityRequests);
                return Ok(availability);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating bulk availability", error = ex.Message });
            }
        }

        /// <summary>
        /// Update availability
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<AvailabilityResponse>> UpdateAvailability(int id, [FromBody] UpdateAvailabilityRequest request)
        {
            try
            {
                // Check authorization - employees can only update their own availability
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                if (userRole == "Employee")
                {
                    var userId = User.FindFirst("userId")?.Value;
                    // You would need to verify that this availability belongs to the current user
                }

                var availability = await _availabilityService.UpdateAvailabilityAsync(id, request);
                if (availability == null)
                    return NotFound(new { message = "Availability not found" });

                return Ok(availability);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the availability", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete availability
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAvailability(int id)
        {
            try
            {
                // Check authorization - employees can only delete their own availability
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                if (userRole == "Employee")
                {
                    var userId = User.FindFirst("userId")?.Value;
                    // You would need to verify that this availability belongs to the current user
                }

                var result = await _availabilityService.DeleteAvailabilityAsync(id);
                if (!result)
                    return NotFound(new { message = "Availability not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the availability", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete all availability for an employee (or specific day)
        /// </summary>
        [HttpDelete("employee/{employeeId}")]
        public async Task<ActionResult> BulkDeleteAvailability(int employeeId, [FromQuery] DayOfWeek? dayOfWeek = null)
        {
            try
            {
                // Check authorization - employees can only delete their own availability
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                if (userRole == "Employee")
                {
                    var userId = User.FindFirst("userId")?.Value;
                    var currentEmployeeId = int.Parse(userId ?? "0"); // This needs proper implementation
                    if (employeeId != currentEmployeeId)
                        return Forbid("You can only delete your own availability");
                }

                var result = await _availabilityService.BulkDeleteAvailabilityAsync(employeeId, dayOfWeek);
                if (!result)
                    return NotFound(new { message = "No availability found to delete" });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting availability", error = ex.Message });
            }
        }

        /// <summary>
        /// Get available employees for a specific shift
        /// </summary>
        [HttpGet("shift/{shiftId}/available-employees")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<List<AvailabilityResponse>>> GetAvailableEmployeesForShift(int shiftId)
        {
            try
            {
                var employees = await _availabilityService.GetAvailableEmployeesForShiftAsync(shiftId);
                return Ok(employees);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving available employees", error = ex.Message });
            }
        }

        /// <summary>
        /// Get preferred employees for a specific shift
        /// </summary>
        [HttpGet("shift/{shiftId}/preferred-employees")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<List<AvailabilityResponse>>> GetPreferredEmployeesForShift(int shiftId)
        {
            try
            {
                var employees = await _availabilityService.GetPreferredEmployeesForShiftAsync(shiftId);
                return Ok(employees);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving preferred employees", error = ex.Message });
            }
        }

        /// <summary>
        /// Check if an employee is available for a specific shift
        /// </summary>
        [HttpGet("employee/{employeeId}/shift/{shiftId}/check")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<bool>> CheckEmployeeAvailabilityForShift(int employeeId, int shiftId)
        {
            try
            {
                var isAvailable = await _availabilityService.IsEmployeeAvailableForShiftAsync(employeeId, shiftId);
                return Ok(new { isAvailable });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while checking availability", error = ex.Message });
            }
        }
    }

    // Helper DTOs for request bodies
    public class BulkAvailabilityRequest
    {
        public int EmployeeId { get; set; }
        public List<CreateAvailabilityRequest> AvailabilityRequests { get; set; } = new();
    }
}

