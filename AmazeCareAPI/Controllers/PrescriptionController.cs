using AmazeCareAPI.Models.DTOs;
using AmazeCareAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace AmazeCareAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Doctor")]
    [EnableCors("DefaultCORS")]
    public class PrescriptionController : ControllerBase
    {
        private readonly IPrescriptionService _prescriptionService;

        public PrescriptionController(IPrescriptionService prescriptionService)
        {
            _prescriptionService = prescriptionService;
        }
        // GET: api/Prescription
        [HttpGet]
        public async Task<IActionResult> GetAllPrescriptions()
        {
            var result = await _prescriptionService.GetAllPrescriptionsAsync();
            if (!result.Any())
                return NotFound("No prescriptions available.");

            return Ok(result);
        }


        [HttpPost]
        public async Task<IActionResult> AddPrescription([FromBody] PrescriptionCreateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _prescriptionService.AddPrescriptionAsync(dto);
            return CreatedAtAction(nameof(GetByRecordId), new { recordId = result.RecordID }, result);
        }

        [HttpGet("record/{recordId}")]
        public async Task<IActionResult> GetByRecordId(int recordId)
        {
            var result = await _prescriptionService.GetPrescriptionsByRecordIdAsync(recordId);
            if (!result.Any())
                return NotFound($"No prescriptions found for record ID: {recordId}");

            return Ok(result);
        }
    }
}
