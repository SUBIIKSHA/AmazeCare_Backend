using AmazeCareAPI.Models;
using AmazeCareAPI.Models.DTOs;

namespace AmazeCareAPI.Interfaces
{
    public interface IAppointmentService
    {
        Task<AppointmentResponseDTO> BookAppointment(AppointmentRequestDTO request);
        Task<IEnumerable<Appointment>> GetAllAppointments();
        Task<IEnumerable<Appointment>> GetAppointmentsByPatientId(int patientId);
        Task<IEnumerable<Appointment>> GetAppointmentsByDoctorId(int doctorId);
        Task<Appointment> ApproveAppointment(int appointmentId, DateTime? scheduledDateTime);
        Task<Appointment> RejectAppointment(int appointmentId);
        Task<Appointment> CompleteAppointment(int appointmentId);
        Task<Appointment> CancelAppointment(int appointmentId);
        Task<Appointment> RescheduleAppointment(int appointmentId, DateTime newDateTime);
        Task<IEnumerable<Appointment>> GetAppointmentsByDate(DateTime date);
        Task<IEnumerable<Appointment>> GetAppointmentsByStatus(int statusId);
        Task<PaginatedAppointmentResponseDTO> SearchAppointments(AppointmentSearchRequestDTO request);
    }
}
