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
    public class AssignmentsController : ControllerBase
    {
        private readonly IAssignmentService _assignmentService;

        public AssignmentsController(IAssignmentService assignmentService)
        {
            _assignmentService = assignmentService;
        }

        /// <summary>
        /// Get all assignments with pagination and filtering
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<PagedResult<AssignmentResponse>>> GetAssignments([FromQuery] AssignmentSearchRequest request)
        {
            try
            {
                var result = await _assignmentService.GetAssignmentsAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving assignments", error = ex.Message });
            }
        }

        /// <summary>
        /// Get assignment by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<AssignmentResponse>> GetAssignment(int id)
        {
            try
            {
                var assignment = await _assignmentService.GetAssignmentByIdAsync(id);
                if (assignment == null)
                    return NotFound(new { message = "Assignment not found" });

                // Check authorization - employees can only view their own assignments
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var userId = User.FindFirst("userId")?.Value;

                if (userRole == "Employee")
                {
                    // Get employee ID from user ID (you might need to implement this logic)
                    // For now, assuming the assignment belongs to the current user
                    var currentUserId = int.Parse(userId ?? "0");
                    // You would need to check if this assignment belongs to the current user's employee record
                }

                return Ok(assignment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the assignment", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new assignment (Admin/Manager only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<AssignmentResponse>> CreateAssignment([FromBody] CreateAssignmentRequest request)
        {
            try
            {
                var assignment = await _assignmentService.CreateAssignmentAsync(request);
                return CreatedAtAction(nameof(GetAssignment), new { id = assignment.AssignmentId }, assignment);
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
                return StatusCode(500, new { message = "An error occurred while creating the assignment", error = ex.Message });
            }
        }

        /// <summary>
        /// Create multiple assignments at once (Admin/Manager only)
        /// </summary>
        [HttpPost("bulk")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<BulkAssignmentResponse>> CreateBulkAssignments([FromBody] BulkAssignmentRequest request)
        {
            try
            {
                var result = await _assignmentService.CreateBulkAssignmentsAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating bulk assignments", error = ex.Message });
            }
        }

        /// <summary>
        /// Update assignment status
        /// </summary>
        [HttpPut("{id}/status")]
        public async Task<ActionResult<AssignmentResponse>> UpdateAssignmentStatus(
            int id, 
            [FromBody] UpdateAssignmentStatusRequest request)
        {
            try
            {
                var assignment = await _assignmentService.UpdateAssignmentStatusAsync(id, request.Status, request.Notes);
                if (assignment == null)
                    return NotFound(new { message = "Assignment not found" });

                return Ok(assignment);
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
                return StatusCode(500, new { message = "An error occurred while updating the assignment", error = ex.Message });
            }
        }

        /// <summary>
        /// Employee confirms assignment
        /// </summary>
        [HttpPost("{id}/confirm")]
        public async Task<ActionResult<AssignmentResponse>> ConfirmAssignment(int id, [FromBody] ConfirmAssignmentRequest? request = null)
        {
            try
            {
                var assignment = await _assignmentService.UpdateAssignmentStatusAsync(
                    id, 
                    Models.AssignmentStatus.Confirmed, 
                    request?.Notes);

                if (assignment == null)
                    return NotFound(new { message = "Assignment not found" });

                return Ok(assignment);
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
                return StatusCode(500, new { message = "An error occurred while confirming the assignment", error = ex.Message });
            }
        }

        /// <summary>
        /// Employee declines assignment
        /// </summary>
        [HttpPost("{id}/decline")]
        public async Task<ActionResult<AssignmentResponse>> DeclineAssignment(int id, [FromBody] DeclineAssignmentRequest request)
        {
            try
            {
                var assignment = await _assignmentService.UpdateAssignmentStatusAsync(
                    id, 
                    Models.AssignmentStatus.Declined, 
                    request.Reason);

                if (assignment == null)
                    return NotFound(new { message = "Assignment not found" });

                return Ok(assignment);
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
                return StatusCode(500, new { message = "An error occurred while declining the assignment", error = ex.Message });
            }
        }

        /// <summary>
        /// Employee checks in to assignment
        /// </summary>
        [HttpPost("{id}/checkin")]
        public async Task<ActionResult<AssignmentResponse>> CheckIn(int id, [FromBody] CheckInRequest? request = null)
        {
            try
            {
                var assignment = await _assignmentService.CheckInAsync(id, request?.CheckInTime);
                if (assignment == null)
                    return NotFound(new { message = "Assignment not found" });

                return Ok(assignment);
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
                return StatusCode(500, new { message = "An error occurred while checking in", error = ex.Message });
            }
        }

        /// <summary>
        /// Employee checks out of assignment
        /// </summary>
        [HttpPost("{id}/checkout")]
        public async Task<ActionResult<AssignmentResponse>> CheckOut(int id, [FromBody] CheckOutRequest? request = null)
        {
            try
            {
                var assignment = await _assignmentService.CheckOutAsync(id, request?.CheckOutTime);
                if (assignment == null)
                    return NotFound(new { message = "Assignment not found" });

                return Ok(assignment);
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
                return StatusCode(500, new { message = "An error occurred while checking out", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete an assignment (Admin/Manager only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult> DeleteAssignment(int id)
        {
            try
            {
                var result = await _assignmentService.DeleteAssignmentAsync(id);
                if (!result)
                    return NotFound(new { message = "Assignment not found" });

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the assignment", error = ex.Message });
            }
        }

        /// <summary>
        /// Get assignments for a specific employee
        /// </summary>
        [HttpGet("employee/{employeeId}")]
        public async Task<ActionResult<List<AssignmentResponse>>> GetEmployeeAssignments(
            int employeeId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                // Check authorization - employees can only view their own assignments
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                if (userRole == "Employee")
                {
                    // You would need to verify that the employeeId belongs to the current user
                    // This requires additional logic to map user to employee
                }

                var assignments = await _assignmentService.GetEmployeeAssignmentsAsync(employeeId, startDate, endDate);
                return Ok(assignments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving employee assignments", error = ex.Message });
            }
        }

        /// <summary>
        /// Get current user's assignments
        /// </summary>
        [HttpGet("my-assignments")]
        public async Task<ActionResult<List<AssignmentResponse>>> GetMyAssignments(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User ID not found in token" });

                // You would need to implement logic to get employee ID from user ID
                // For now, this is a placeholder
                var employeeId = int.Parse(userId); // This needs proper implementation

                var assignments = await _assignmentService.GetEmployeeAssignmentsAsync(employeeId, startDate, endDate);
                return Ok(assignments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving your assignments", error = ex.Message });
            }
        }

        /// <summary>
        /// Get today's assignments for current user
        /// </summary>
        [HttpGet("my-assignments/today")]
        public async Task<ActionResult<List<AssignmentResponse>>> GetMyTodaysAssignments()
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User ID not found in token" });

                // You would need to implement logic to get employee ID from user ID
                var employeeId = int.Parse(userId); // This needs proper implementation

                var assignments = await _assignmentService.GetTodaysAssignmentsAsync(employeeId);
                return Ok(assignments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving today's assignments", error = ex.Message });
            }
        }

        /// <summary>
        /// Get upcoming assignments for current user
        /// </summary>
        [HttpGet("my-assignments/upcoming")]
        public async Task<ActionResult<List<AssignmentResponse>>> GetMyUpcomingAssignments([FromQuery] int days = 7)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User ID not found in token" });

                // You would need to implement logic to get employee ID from user ID
                var employeeId = int.Parse(userId); // This needs proper implementation

                var assignments = await _assignmentService.GetUpcomingAssignmentsAsync(employeeId, days);
                return Ok(assignments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving upcoming assignments", error = ex.Message });
            }
        }
    }

    // Helper DTOs for request bodies
    public class UpdateAssignmentStatusRequest
    {
        public Models.AssignmentStatus Status { get; set; }
        public string? Notes { get; set; }
    }

    public class ConfirmAssignmentRequest
    {
        public string? Notes { get; set; }
    }

    public class DeclineAssignmentRequest
    {
        public string Reason { get; set; } = string.Empty;
    }

    public class CheckInRequest
    {
        public DateTime? CheckInTime { get; set; }
    }

    public class CheckOutRequest
    {
        public DateTime? CheckOutTime { get; set; }
    }
}

