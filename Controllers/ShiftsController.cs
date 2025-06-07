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
    public class ShiftsController : ControllerBase
    {
        private readonly IShiftService _shiftService;

        public ShiftsController(IShiftService shiftService)
        {
            _shiftService = shiftService;
        }

        /// <summary>
        /// Get all shifts with pagination and filtering
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedResult<ShiftResponse>>> GetShifts([FromQuery] ShiftSearchRequest request)
        {
            try
            {
                var result = await _shiftService.GetShiftsAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving shifts", error = ex.Message });
            }
        }

        /// <summary>
        /// Get shift by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ShiftResponse>> GetShift(int id)
        {
            try
            {
                var shift = await _shiftService.GetShiftByIdAsync(id);
                if (shift == null)
                    return NotFound(new { message = "Shift not found" });

                return Ok(shift);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the shift", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new shift (Admin/Manager only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ShiftResponse>> CreateShift([FromBody] CreateShiftRequest request)
        {
            try
            {
                var shift = await _shiftService.CreateShiftAsync(request);
                return CreatedAtAction(nameof(GetShift), new { id = shift.ShiftId }, shift);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the shift", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing shift (Admin/Manager only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ShiftResponse>> UpdateShift(int id, [FromBody] UpdateShiftRequest request)
        {
            try
            {
                var shift = await _shiftService.UpdateShiftAsync(id, request);
                if (shift == null)
                    return NotFound(new { message = "Shift not found" });

                return Ok(shift);
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
                return StatusCode(500, new { message = "An error occurred while updating the shift", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a shift (Admin/Manager only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult> DeleteShift(int id)
        {
            try
            {
                var result = await _shiftService.DeleteShiftAsync(id);
                if (!result)
                    return NotFound(new { message = "Shift not found" });

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the shift", error = ex.Message });
            }
        }

        /// <summary>
        /// Get shifts for a specific schedule
        /// </summary>
        [HttpGet("schedule/{scheduleId}")]
        public async Task<ActionResult<List<ShiftResponse>>> GetShiftsBySchedule(int scheduleId)
        {
            try
            {
                var shifts = await _shiftService.GetShiftsByScheduleAsync(scheduleId);
                return Ok(shifts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving shifts", error = ex.Message });
            }
        }

        /// <summary>
        /// Get shifts for a specific date
        /// </summary>
        [HttpGet("date/{date}")]
        public async Task<ActionResult<List<ShiftResponse>>> GetShiftsByDate(DateTime date)
        {
            try
            {
                var shifts = await _shiftService.GetShiftsByDateAsync(date);
                return Ok(shifts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving shifts", error = ex.Message });
            }
        }

        /// <summary>
        /// Get shifts for a date range
        /// </summary>
        [HttpGet("date-range")]
        public async Task<ActionResult<List<ShiftResponse>>> GetShiftsByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                if (startDate >= endDate)
                    return BadRequest(new { message = "Start date must be before end date" });

                var shifts = await _shiftService.GetShiftsByDateRangeAsync(startDate, endDate);
                return Ok(shifts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving shifts", error = ex.Message });
            }
        }

        /// <summary>
        /// Get today's shifts
        /// </summary>
        [HttpGet("today")]
        public async Task<ActionResult<List<ShiftResponse>>> GetTodaysShifts()
        {
            try
            {
                var shifts = await _shiftService.GetTodaysShiftsAsync();
                return Ok(shifts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving today's shifts", error = ex.Message });
            }
        }

        /// <summary>
        /// Get tomorrow's shifts
        /// </summary>
        [HttpGet("tomorrow")]
        public async Task<ActionResult<List<ShiftResponse>>> GetTomorrowsShifts()
        {
            try
            {
                var shifts = await _shiftService.GetTomorrowsShiftsAsync();
                return Ok(shifts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving tomorrow's shifts", error = ex.Message });
            }
        }

        /// <summary>
        /// Get this week's shifts
        /// </summary>
        [HttpGet("this-week")]
        public async Task<ActionResult<List<ShiftResponse>>> GetThisWeeksShifts()
        {
            try
            {
                var shifts = await _shiftService.GetThisWeeksShiftsAsync();
                return Ok(shifts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving this week's shifts", error = ex.Message });
            }
        }

        /// <summary>
        /// Get shifts by department
        /// </summary>
        [HttpGet("department/{department}")]
        public async Task<ActionResult<List<ShiftResponse>>> GetShiftsByDepartment(string department)
        {
            try
            {
                var shifts = await _shiftService.GetShiftsByDepartmentAsync(department);
                return Ok(shifts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving shifts", error = ex.Message });
            }
        }

        /// <summary>
        /// Get shifts by location
        /// </summary>
        [HttpGet("location/{locationId}")]
        public async Task<ActionResult<List<ShiftResponse>>> GetShiftsByLocation(int locationId)
        {
            try
            {
                var shifts = await _shiftService.GetShiftsByLocationAsync(locationId);
                return Ok(shifts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving shifts", error = ex.Message });
            }
        }

        /// <summary>
        /// Get available employees for a shift
        /// </summary>
        [HttpGet("{id}/available-employees")]
        public async Task<ActionResult<List<EmployeeInfo>>> GetAvailableEmployeesForShift(int id)
        {
            try
            {
                var employees = await _shiftService.GetAvailableEmployeesForShiftAsync(id);
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
        /// Get preferred employees for a shift
        /// </summary>
        [HttpGet("{id}/preferred-employees")]
        public async Task<ActionResult<List<EmployeeInfo>>> GetPreferredEmployeesForShift(int id)
        {
            try
            {
                var employees = await _shiftService.GetPreferredEmployeesForShiftAsync(id);
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
        /// Check for shift conflicts for an employee
        /// </summary>
        [HttpGet("conflicts")]
        public async Task<ActionResult<List<ShiftResponse>>> CheckShiftConflicts(
            [FromQuery] int employeeId,
            [FromQuery] DateTime startTime,
            [FromQuery] DateTime endTime)
        {
            try
            {
                var conflicts = await _shiftService.GetShiftConflictsAsync(employeeId, startTime, endTime);
                return Ok(conflicts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while checking for conflicts", error = ex.Message });
            }
        }

        /// <summary>
        /// Get understaffed shifts
        /// </summary>
        [HttpGet("understaffed")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<List<ShiftResponse>>> GetUnderstaffedShifts([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var shifts = await _shiftService.GetUnderstaffedShiftsAsync(startDate, endDate);
                return Ok(shifts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving understaffed shifts", error = ex.Message });
            }
        }
    }
}

