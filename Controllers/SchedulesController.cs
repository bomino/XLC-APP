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
    public class SchedulesController : ControllerBase
    {
        private readonly IScheduleService _scheduleService;

        public SchedulesController(IScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        /// <summary>
        /// Get all schedules with pagination and filtering
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedResult<ScheduleResponse>>> GetSchedules([FromQuery] ScheduleSearchRequest request)
        {
            try
            {
                var result = await _scheduleService.GetSchedulesAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving schedules", error = ex.Message });
            }
        }

        /// <summary>
        /// Get schedule by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ScheduleResponse>> GetSchedule(int id)
        {
            try
            {
                var schedule = await _scheduleService.GetScheduleByIdAsync(id);
                if (schedule == null)
                    return NotFound(new { message = "Schedule not found" });

                return Ok(schedule);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the schedule", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new schedule (Admin/Manager only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ScheduleResponse>> CreateSchedule([FromBody] CreateScheduleRequest request)
        {
            try
            {
                var schedule = await _scheduleService.CreateScheduleAsync(request);
                return CreatedAtAction(nameof(GetSchedule), new { id = schedule.ScheduleId }, schedule);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the schedule", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing schedule (Admin/Manager only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ScheduleResponse>> UpdateSchedule(int id, [FromBody] UpdateScheduleRequest request)
        {
            try
            {
                var schedule = await _scheduleService.UpdateScheduleAsync(id, request);
                if (schedule == null)
                    return NotFound(new { message = "Schedule not found" });

                return Ok(schedule);
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
                return StatusCode(500, new { message = "An error occurred while updating the schedule", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a schedule (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteSchedule(int id)
        {
            try
            {
                var result = await _scheduleService.DeleteScheduleAsync(id);
                if (!result)
                    return NotFound(new { message = "Schedule not found" });

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the schedule", error = ex.Message });
            }
        }

        /// <summary>
        /// Publish a schedule (Admin/Manager only)
        /// </summary>
        [HttpPost("{id}/publish")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ScheduleResponse>> PublishSchedule(int id)
        {
            try
            {
                var schedule = await _scheduleService.PublishScheduleAsync(id);
                if (schedule == null)
                    return NotFound(new { message = "Schedule not found" });

                return Ok(schedule);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while publishing the schedule", error = ex.Message });
            }
        }

        /// <summary>
        /// Unpublish a schedule (Admin/Manager only)
        /// </summary>
        [HttpPost("{id}/unpublish")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ScheduleResponse>> UnpublishSchedule(int id)
        {
            try
            {
                var schedule = await _scheduleService.UnpublishScheduleAsync(id);
                if (schedule == null)
                    return NotFound(new { message = "Schedule not found" });

                return Ok(schedule);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while unpublishing the schedule", error = ex.Message });
            }
        }

        /// <summary>
        /// Get current published schedule
        /// </summary>
        [HttpGet("current")]
        public async Task<ActionResult<ScheduleResponse>> GetCurrentSchedule()
        {
            try
            {
                var schedule = await _scheduleService.GetCurrentScheduleAsync();
                if (schedule == null)
                    return NotFound(new { message = "No current schedule found" });

                return Ok(schedule);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the current schedule", error = ex.Message });
            }
        }

        /// <summary>
        /// Get upcoming schedules
        /// </summary>
        [HttpGet("upcoming")]
        public async Task<ActionResult<List<ScheduleResponse>>> GetUpcomingSchedules([FromQuery] int days = 30)
        {
            try
            {
                var schedules = await _scheduleService.GetUpcomingSchedulesAsync(days);
                return Ok(schedules);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving upcoming schedules", error = ex.Message });
            }
        }

        /// <summary>
        /// Get schedules for a specific date range
        /// </summary>
        [HttpGet("date-range")]
        public async Task<ActionResult<List<ScheduleResponse>>> GetSchedulesByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                if (startDate >= endDate)
                    return BadRequest(new { message = "Start date must be before end date" });

                var schedules = await _scheduleService.GetSchedulesByDateRangeAsync(startDate, endDate);
                return Ok(schedules);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving schedules", error = ex.Message });
            }
        }
    }
}

