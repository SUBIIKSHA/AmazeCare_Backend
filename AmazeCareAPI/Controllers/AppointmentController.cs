using AmazeCareAPI.Interfaces;
using AmazeCareAPI.Models;
using AmazeCareAPI.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmazeCareAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [HttpPost("book")]
        [Authorize(Roles = "Patient")]
        public async Task<ActionResult<AppointmentResponseDTO>> BookAppointment([FromBody] AppointmentRequestDTO request)
        {
            var result = await _appointmentService.BookAppointment(request);
            return Ok(result);
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<AppointmentSearchResponseDTO>>> GetAllAppointments()
        {
            var result = await _appointmentService.GetAllAppointments();
            return Ok(result);
        }

        [HttpGet("byPatient/{patientId}")]
        [Authorize(Roles = "Admin,Patient")]
        public async Task<ActionResult<IEnumerable<AppointmentSearchResponseDTO>>> GetByPatient(int patientId)
        {
            var result = await _appointmentService.GetAppointmentsByPatientId(patientId);
            return Ok(result);
        }

        [HttpGet("byDoctor/{doctorId}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<ActionResult<IEnumerable<AppointmentSearchResponseDTO>>> GetByDoctor(int doctorId)
        {
            var result = await _appointmentService.GetAppointmentsByDoctorId(doctorId);
            return Ok(result);
        }

        [HttpPut("approve/{appointmentId}")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult<Appointment>> ApproveAppointment(int appointmentId, [FromBody] DateTime? scheduledDateTime)
        {
            var result = await _appointmentService.ApproveAppointment(appointmentId, scheduledDateTime);
            return Ok(result);
        }

        [HttpPut("reject/{appointmentId}")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult<Appointment>> RejectAppointment(int appointmentId)
        {
            var result = await _appointmentService.RejectAppointment(appointmentId);
            return Ok(result);
        }

        [HttpPut("complete/{appointmentId}")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult<Appointment>> CompleteAppointment(int appointmentId)
        {
            var result = await _appointmentService.CompleteAppointment(appointmentId);
            return Ok(result);
        }

        [HttpPut("cancel/{appointmentId}")]
        [Authorize(Roles = "Doctor,Patient")]
        public async Task<ActionResult<Appointment>> CancelAppointment(int appointmentId)
        {
            var result = await _appointmentService.CancelAppointment(appointmentId);
            return Ok(result);
        }

        [HttpPut("reschedule/{appointmentId}")]
        [Authorize(Roles = "Doctor,Patient")]
        public async Task<ActionResult<Appointment>> RescheduleAppointment(int appointmentId, [FromBody] DateTime newDateTime)
        {
            var result = await _appointmentService.RescheduleAppointment(appointmentId, newDateTime);
            return Ok(result);
        }

        [HttpPost("search")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<ActionResult<PaginatedAppointmentResponseDTO>> SearchAppointments([FromBody] AppointmentSearchRequestDTO request)
        {
            var result = await _appointmentService.SearchAppointments(request);
            return Ok(result);
        }

        [HttpGet("byDate")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<ActionResult<IEnumerable<Appointment>>> GetAppointmentsByDate([FromQuery] DateTime date)
        {
            var result = await _appointmentService.GetAppointmentsByDate(date);
            return Ok(result);
        }

        [HttpGet("byStatus/{statusId}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<ActionResult<IEnumerable<Appointment>>> GetAppointmentsByStatus(int statusId)
        {
            var result = await _appointmentService.GetAppointmentsByStatus(statusId);
            return Ok(result);
        }
    }
}
