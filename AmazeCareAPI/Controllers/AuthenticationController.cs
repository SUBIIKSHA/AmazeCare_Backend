using AmazeCareAPI.Interfaces;
using AmazeCareAPI.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;


namespace AmazeCareAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("DefaultCORS")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticate _authenticateService;

        public AuthenticationController(IAuthenticate authenticateService)
        {
            _authenticateService = authenticateService;
        }

        [HttpPost("Register/Doctor")]
        public async Task<ActionResult<AddDoctorResponseDTO>> RegisterDoctor(AddDoctorRequestDTO requestDTO)
        {
            try
            {
                var result = await _authenticateService.RegisterDoctor(requestDTO);
                return Created("", result);
            }
            catch (Exception ex)
            {
                var message = ex.InnerException?.Message ?? ex.Message;
                return BadRequest(new ErrorObjectDTO
                {
                    ErrorNumber = 430,
                    ErrorMessage = message
                });
            }
        }

        [HttpPost("Register/Patient")]
        public async Task<ActionResult<AddPatientResponseDTO>> RegisterPatient(AddPatientRequestDTO requestDTO)
        {
            try
            {
                var result = await _authenticateService.RegisterPatient(requestDTO);
                return Created("", result);
            }
            catch (Exception ex)
            {
                var message = ex.InnerException?.Message ?? ex.Message;
                return BadRequest(new ErrorObjectDTO
                {
                    ErrorNumber = 430,
                    ErrorMessage = message
                });
            }
        }

        [HttpPost("Login")]
        public async Task<ActionResult<LoginResponseDTO>> Login(LoginRequestDTO requestDTO)
        {
            try
            {
                var result = await _authenticateService.Login(requestDTO);
                return Ok(result);
            }
            catch (Exception)
            {
                return Unauthorized(new ErrorObjectDTO { ErrorNumber = 401, ErrorMessage = "Invalid username or password" });
            }
        }
    }
}
