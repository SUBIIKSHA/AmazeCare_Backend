using AmazeCareAPI.Interfaces;
using AmazeCareAPI.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;

namespace AmazeCareAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("DefaultCORS")]
    public class DoctorController : ControllerBase
    {
        private readonly IDoctorService _doctorService;

        public DoctorController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> GetAllDoctors()
        {
            var doctors = await _doctorService.GetAllDoctors();
            return Ok(doctors);
        }

        [HttpGet("status/{statusId}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> GetDoctorsByStatus(int statusId)
        {
            var result = await _doctorService.GetDoctorsByStatusAsync(statusId);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> GetDoctorById(int id)
        {
            var doctor = await _doctorService.GetDoctorById(id);
            if (doctor == null) return NotFound();
            return Ok(doctor);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddDoctor([FromBody] AddDoctorRequestDTO request)
        {
            try
            {
                var added = await _doctorService.AddDoctor(request);
                return CreatedAtAction(nameof(GetDoctorById), new { id = added.DoctorID }, added);
            }
            catch (Exception ex)
            {
                var innerMost = ex;
                while (innerMost.InnerException != null)
                    innerMost = innerMost.InnerException;

                return BadRequest(new ErrorObjectDTO
                {
                    ErrorNumber = 430,
                    ErrorMessage = $"Doctor registration failed: {innerMost.Message}"
                });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> UpdateDoctor(int id, [FromBody] UpdateDoctorRequestDTO request)
        {
            var updated = await _doctorService.UpdateDoctor(id, request);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteDoctor(int id)
        {
            var result = await _doctorService.DeactivateDoctorAsync(id);
            if (!result)
                return NotFound();

            return Ok("Doctor marked as inactive.");
        }

        [HttpGet("form-data")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> GetDataForAddingDoctors()
        {
            var data = await _doctorService.GetDataForAddingDoctors();
            return Ok(data);
        }

        [HttpPost("search")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> SearchDoctors([FromBody] DoctorSearchRequestDTO request)
        {
            var result = await _doctorService.SearchDoctors(request);
            return Ok(result);
        }
    }
}
