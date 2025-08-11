using AmazeCareAPI.Models.DTOs;
using AmazeCareAPI.Exceptions;
using AmazeCareAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmazeCareAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecommendedTestController : ControllerBase
    {
        private readonly IRecommendedTestService _service;

        public RecommendedTestController(IRecommendedTestService service)
        {
            _service = service;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<ActionResult<IEnumerable<RecommendedTestResponseDTO>>> GetAll()
        {
            var tests = await _service.GetAllAsync();
            return Ok(tests);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<ActionResult<RecommendedTestResponseDTO>> GetById(int id)
        {
            try
            {
                var test = await _service.GetByIdAsync(id);
                return Ok(test);
            }
            catch (NoSuchEntityException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("record/{recordId}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<ActionResult<IEnumerable<RecommendedTestResponseDTO>>> GetByPrescriptionIdAsync(int prescriptionId)
        {
            var tests = await _service.GetByPrescriptionIdAsync(prescriptionId);
            return Ok(tests);
        }

        [HttpPost]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult<RecommendedTestResponseDTO>> Create([FromBody] RecommendedTestCreateDTO dto)
        {
            try
            {
                var created = await _service.AddAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.RecommendedTestID }, created);
            }
            catch (NoSuchEntityException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult<RecommendedTestResponseDTO>> Delete(int id)
        {
            try
            {
                var deleted = await _service.DeleteAsync(id);
                return Ok(deleted);
            }
            catch (NoSuchEntityException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
