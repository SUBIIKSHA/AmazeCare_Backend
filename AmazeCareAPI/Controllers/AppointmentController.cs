using AmazeCareAPI.Contexts;
using AmazeCareAPI.Interfaces;
using AmazeCareAPI.Models;
using AmazeCareAPI.Models.DTOs;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AmazeCareAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("DefaultCORS")]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public AppointmentController(IAppointmentService appointmentService, ApplicationDbContext context, IMapper mapper)
        {
            _appointmentService = appointmentService;
            _context = context;
            _mapper = mapper;
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
        public async Task<ActionResult<List<AppointmentResponseDTO>>> GetAllAppointments()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.Status)
                .ToListAsync();

            var appointmentDTOs = _mapper.Map<List<AppointmentResponseDTO>>(appointments);

            return Ok(appointmentDTOs);
        }

        [HttpGet("byPatient/{patientId}")]
        [Authorize(Roles = "Admin,Patient")]
        public async Task<IActionResult> GetByPatient(int patientId)
        {
            var result = await _appointmentService.GetAppointmentsByPatientId(patientId);

            var options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles,  // Avoid circular refs
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = true  // Optional, formats JSON nicely
            };

            var json = JsonSerializer.Serialize(result, options);

            return Content(json, "application/json");
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
        public async Task<ActionResult<Appointment>> ApproveAppointment(int appointmentId)
        {
            var result = await _appointmentService.ApproveAppointment(appointmentId);
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
        [Authorize(Roles = "Doctor,Patient,Admin")]
        public async Task<ActionResult<Appointment>> CancelAppointment(int appointmentId)
        {
            var result = await _appointmentService.CancelAppointment(appointmentId);
            return Ok(result);
        }

        [HttpPut("reschedule/{appointmentId}")]
        [Authorize(Roles = "Doctor,Patient,Admin")]
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
        public async Task<ActionResult<IEnumerable<AppointmentResponseDTO>>> GetAppointmentsByStatus(int statusId)
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.Status)
                .Where(a => a.StatusID == statusId)
                .ToListAsync();

            var result = appointments.Select(a => new AppointmentResponseDTO
            {
                AppointmentID = a.AppointmentID,
                PatientName = a.Patient?.FullName ?? "Unknown",
                DoctorName = a.Doctor?.Name ?? "Unknown",
                AppointmentDateTime = a.AppointmentDateTime,
                Symptoms = a.Symptoms,
                VisitReason = a.VisitReason,
                Status = a.Status?.StatusName ?? string.Empty
            }).ToList();

            return Ok(result);
        }
    }
}
