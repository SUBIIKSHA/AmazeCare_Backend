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
    public class MedicalRecordController : ControllerBase
    {
        private readonly IMedicalRecordService _medicalRecordService;

        public MedicalRecordController(IMedicalRecordService medicalRecordService)
        {
            _medicalRecordService = medicalRecordService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MedicalRecordDTO>>> GetAll()
        {
            var records = await _medicalRecordService.GetAllAsync();
            return Ok(records);
        }

        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetByStatus(string status)
        {
            var records = await _medicalRecordService.GetMedicalRecordsByStatusAsync(status);
            if (records == null || !records.Any())
                return NotFound("No medical records found with the given status.");
            return Ok(records);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MedicalRecordDTO>> GetById(int id)
        {
            var record = await _medicalRecordService.GetByIdAsync(id);
            return Ok(record);
        }

        [HttpGet("ByAppointment/{appointmentId}")]
        public async Task<ActionResult<IEnumerable<MedicalRecordDTO>>> GetByAppointmentId(int appointmentId)
        {
            var records = await _medicalRecordService.GetByAppointmentIdAsync(appointmentId);
            return Ok(records);
        }

        [HttpPost]
        public async Task<ActionResult<MedicalRecordDTO>> Create([FromBody] CreateMedicalRecordDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _medicalRecordService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.RecordID }, created);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<MedicalRecordDTO>> Update(int id, [FromBody] UpdateMedicalRecordDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _medicalRecordService.UpdateAsync(id, dto);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _medicalRecordService.DeleteMedicalRecord(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}
