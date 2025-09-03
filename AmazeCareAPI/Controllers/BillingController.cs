using AmazeCareAPI.Models.DTOs;
using AmazeCareAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace AmazeCareAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableCors("DefaultCORS")]
    public class BillingController : ControllerBase
    {
        private readonly IBillingService _billingService;

        public BillingController(IBillingService billingService)
        {
            _billingService = billingService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Patient")]
        public async Task<ActionResult<IEnumerable<BillingResponseDTO>>> GetAll()
        {
            var billings = await _billingService.GetAllAsync();
            return Ok(billings);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BillingResponseDTO>> GetById(int id)
        {
            try
            {
                var billing = await _billingService.GetByIdAsync(id);
                return Ok(billing);
            }
            catch
            {
                return NotFound();
            }
        }

        [HttpGet("appointment/{appointmentId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<BillingResponseDTO>>> GetByAppointmentId(int appointmentId)
        {
            var billings = await _billingService.GetByAppointmentIdAsync(appointmentId);
            return Ok(billings);
        }

        [HttpGet("status/{statusId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<BillingResponseDTO>>> GetByStatusId(int statusId)
        {
            var billings = await _billingService.GetByStatusIdAsync(statusId);
            return Ok(billings);
        }

        [HttpGet("date-range")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<BillingResponseDTO>>> GetByDateRange(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to)
        {
            var billings = await _billingService.GetByDateRangeAsync(from, to);
            return Ok(billings);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BillingResponseDTO>> Create([FromBody] BillingCreateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var created = await _billingService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.BillingID }, created);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
