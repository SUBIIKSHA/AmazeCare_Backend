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
    public class PrescriptionController : ControllerBase
    {
        private readonly IPrescriptionService _prescriptionService;

        public PrescriptionController(IPrescriptionService prescriptionService)
        {
            _prescriptionService = prescriptionService;
        }
        // GET: api/Prescription
        [HttpGet]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> GetAllPrescriptions()
        {
            var result = await _prescriptionService.GetAllPrescriptionsAsync();
            if (!result.Any())
                return NotFound("No prescriptions available.");

            return Ok(result);
        }



        [HttpPost]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> AddPrescription([FromBody] PrescriptionCreateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _prescriptionService.AddPrescriptionAsync(dto);
            return CreatedAtAction(nameof(GetByRecordId), new { recordId = result.RecordID }, result);
        }

        [HttpGet("record/{recordId}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> GetByRecordId(int recordId)
        {
            var result = await _prescriptionService.GetPrescriptionsByRecordIdAsync(recordId);
            if (!result.Any())
                return NotFound($"No prescriptions found for record ID: {recordId}");

            return Ok(result);
        }
    }
}
