using AmazeCareAPI.Interfaces;
using AmazeCareAPI.Models;
using AmazeCareAPI.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;

namespace AmazeCareAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("DefaultCORS")]
    public class PatientController : ControllerBase
    {
        private readonly IPatientService _patientService;

        public PatientController(IPatientService patientService)
        {
            _patientService = patientService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<ActionResult<ICollection<Patient>>> GetAllPatients()
        {
            var patients = await _patientService.GetAllPatients();
            return Ok(patients);
        }

        [HttpGet("byDoctor/{doctorId}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> GetPatientsByDoctor(int doctorId)
        {
            var patients = await _patientService.GetPatientsByDoctorIdAsync(doctorId);
            if (patients == null || !patients.Any())
            {
                return NotFound($"No patients found for doctor with ID {doctorId}");
            }
            return Ok(patients);
        }


        [HttpGet("status/{statusId}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> GetPatientsByStatus(int statusId)
        {
            var result = await _patientService.GetPatientsByStatusAsync(statusId);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<ActionResult<Patient>> GetPatientById(int id)
        {
            var patient = await _patientService.GetPatientById(id);
            if (patient == null)
                return NotFound($"Patient with ID {id} not found.");
            return Ok(patient);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Patient>> AddPatient([FromBody] AddPatientRequestDTO request)
        {
            var addedPatient = await _patientService.AddPatient(request);
            return CreatedAtAction(nameof(GetPatientById), new { id = addedPatient.PatientID }, addedPatient);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Patient")]
        public async Task<ActionResult<Patient>> UpdatePatient(int id, [FromBody] UpdatePatientRequestDTO request)
        {
            var updatedPatient = await _patientService.UpdatePatient(id, request);
            if (updatedPatient == null)
                return NotFound($"Patient with ID {id} not found.");
            return Ok(updatedPatient);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePatient(int id)
        {
            var result = await _patientService.DeactivatePatientAsync(id);
            if (!result)
                return NotFound();

            return Ok("Patient marked as deactivated.");
        }

        [HttpGet("masters")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PatientMasterResponseDTO>> GetMasterDataForAddingPatients()
        {
            var data = await _patientService.GetDataForAddingPatients();
            return Ok(data);
        }

        [HttpPost("search")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<ActionResult<PaginatedPatientResponseDTO>> SearchPatients([FromBody] PatientSearchRequestDTO request)
        {
            try
            {
                var result = await _patientService.SearchPatients(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new PaginatedPatientResponseDTO
                {
                    Error = new ErrorObjectDTO
                    {
                        ErrorMessage = ex.Message
                    }
                });
            }
        }
    }
}
