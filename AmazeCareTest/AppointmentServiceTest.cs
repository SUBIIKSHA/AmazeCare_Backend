using AmazeCareAPI.Exceptions;
using AmazeCareAPI.Interfaces;
using AmazeCareAPI.Models;
using AmazeCareAPI.Models.DTOs;
using AmazeCareAPI.Services;
using AutoMapper;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmazeCareAPI.Tests
{
    [TestFixture]
    public class AppointmentServiceTest
    {
        private Mock<IRepository<int, Appointment>> _appointmentRepoMock;
        private Mock<IRepository<int, Patient>> _patientRepoMock;
        private Mock<IRepository<int, Doctor>> _doctorRepoMock;
        private IMapper _mapper;
        private AppointmentService _service;

        [SetUp]
        public void Setup()
        {
            _appointmentRepoMock = new Mock<IRepository<int, Appointment>>();
            _patientRepoMock = new Mock<IRepository<int, Patient>>();
            _doctorRepoMock = new Mock<IRepository<int, Doctor>>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Appointment, AppointmentSearchResponseDTO>();
                cfg.CreateMap<Appointment, AppointmentResponseDTO>()
                    .ForMember(dest => dest.PatientName, opt => opt.MapFrom(src => src.Patient.FullName))
                    .ForMember(dest => dest.DoctorName, opt => opt.MapFrom(src => src.Doctor.Name));
                cfg.CreateMap<AppointmentRequestDTO, Appointment>();
            });
            _mapper = config.CreateMapper();

            _service = new AppointmentService(_appointmentRepoMock.Object,
                                              _patientRepoMock.Object,
                                              _doctorRepoMock.Object,
                                              _mapper);
        }

        [Test]
        public async Task BookAppointment_ValidInput_ReturnsResponseDTO()
        {
            var request = new AppointmentRequestDTO
            {
                PatientID = 1,
                DoctorID = 2,
                AppointmentDateTime = DateTime.Now,
                VisitReason = "Check-up",
                Symptoms = "Headache"
            };

            var patient = new Patient { PatientID = 1, FullName = "John Doe" };
            var doctor = new Doctor { DoctorID = 2, Name = "Dr. Smith" };

            _patientRepoMock.Setup(r => r.GetById(1)).ReturnsAsync(patient);
            _doctorRepoMock.Setup(r => r.GetById(2)).ReturnsAsync(doctor);
            _appointmentRepoMock.Setup(r => r.Add(It.IsAny<Appointment>()))
                .ReturnsAsync((Appointment appt) =>
                {
                    appt.AppointmentID = 100;
                    appt.Patient = patient;
                    appt.Doctor = doctor;
                    return appt;
                });
            var result = await _service.BookAppointment(request);

            Assert.IsNotNull(result);
            Assert.AreEqual("John Doe", result.PatientName);
            Assert.AreEqual("Dr. Smith", result.DoctorName);
            Assert.AreEqual("Pending", result.Status);
        }

        [Test]
        public void BookAppointment_InvalidPatient_ThrowsNotFoundException()
        {
            var request = new AppointmentRequestDTO
            {
                PatientID = 1,
                DoctorID = 2,
                AppointmentDateTime = DateTime.Now
            };

            _patientRepoMock.Setup(r => r.GetById(1)).ReturnsAsync((Patient)null);

            Assert.ThrowsAsync<NotFoundException>(() => _service.BookAppointment(request));
        }

        [Test]
        public void BookAppointment_InvalidDoctor_ThrowsNotFoundException()
        {
            var request = new AppointmentRequestDTO
            {
                PatientID = 1,
                DoctorID = 2,
                AppointmentDateTime = DateTime.Now
            };

            _patientRepoMock.Setup(r => r.GetById(1)).ReturnsAsync(new Patient());
            _doctorRepoMock.Setup(r => r.GetById(2)).ReturnsAsync((Doctor)null);

            Assert.ThrowsAsync<NotFoundException>(() => _service.BookAppointment(request));
        }

        [Test]
        public async Task GetAllAppointments_ShouldReturnList()
        {
            var appointments = new List<Appointment>
            {
                new Appointment { AppointmentID = 1, VisitReason = "Checkup", Patient = new Patient(), Doctor = new Doctor() },
                new Appointment { AppointmentID = 2, VisitReason = "Consultation", Patient = new Patient(), Doctor = new Doctor() }
            };

            _appointmentRepoMock.Setup(r => r.GetAll()).ReturnsAsync(appointments);
            var result = await _service.GetAllAppointments();

            Assert.AreEqual(2, result.Count());
        }

        [Test]
        public void GetAllAppointments_NoAppointments_ThrowsNotFoundException()
        {
            _appointmentRepoMock.Setup(r => r.GetAll()).ReturnsAsync(new List<Appointment>());

            Assert.ThrowsAsync<NotFoundException>(() => _service.GetAllAppointments());
        }

        [Test]
        public async Task ApproveAppointment_ValidId_ShouldUpdateStatusAndReturnAppointment()
        {
            var appointment = new Appointment
            {
                AppointmentID = 1,
                StatusID = 1,
                AppointmentDateTime = new DateTime(2025, 08, 10)
            };

            var newDate = new DateTime(2025, 08, 15);

            _appointmentRepoMock.Setup(r => r.GetById(1)).ReturnsAsync(appointment);
            _appointmentRepoMock.Setup(r => r.Update(1, It.IsAny<Appointment>())).ReturnsAsync((int id, Appointment a) => a);

            var result = await _service.ApproveAppointment(1, newDate);

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.StatusID); 
            Assert.AreEqual(newDate, result.AppointmentDateTime);
        }
        [Test]
        public void ApproveAppointment_InvalidId_ShouldThrowNotFoundException()
        {
            _appointmentRepoMock.Setup(r => r.GetById(99)).ReturnsAsync((Appointment)null);

            var ex = Assert.ThrowsAsync<NotFoundException>(() => _service.ApproveAppointment(99, DateTime.Now));
            Assert.AreEqual("No appointments were found.", ex.Message);
        }

        [Test]
        public async Task CancelAppointment_ValidId_ShouldSetStatusToCancelled()
        {
            var appointment = new Appointment
            {
                AppointmentID = 1,
                StatusID = 1
            };

            _appointmentRepoMock.Setup(r => r.GetById(1)).ReturnsAsync(appointment);
            _appointmentRepoMock.Setup(r => r.Update(1, It.IsAny<Appointment>())).ReturnsAsync((int id, Appointment a) => a);

            var result = await _service.CancelAppointment(1);

            Assert.IsNotNull(result);
            Assert.AreEqual(4, result.StatusID);
        }

        [Test]
        public void CancelAppointment_InvalidId_ShouldThrowNotFoundException()
        {
            _appointmentRepoMock.Setup(r => r.GetById(99)).ReturnsAsync((Appointment)null);

            var ex = Assert.ThrowsAsync<NotFoundException>(() => _service.CancelAppointment(99));
            Assert.AreEqual("No appointments were found.", ex.Message);
        }

        [Test]
        public async Task GetAppointmentsByPatientId_ShouldReturnList()
        {
            var appointments = new List<Appointment>
            {
                new Appointment { AppointmentID = 1, PatientID = 1, Doctor = new Doctor(), Patient = new Patient() },
                new Appointment { AppointmentID = 2, PatientID = 1, Doctor = new Doctor(), Patient = new Patient() }
            };

            _appointmentRepoMock.Setup(r => r.GetAll()).ReturnsAsync(appointments);

            var result = await _service.GetAppointmentsByPatientId(1);

            Assert.AreEqual(2, result.Count());
        }

        [Test]
        public void GetAppointmentsByPatientId_NoMatches_ThrowsNotFoundException()
        {
            _appointmentRepoMock.Setup(r => r.GetAll()).ReturnsAsync(new List<Appointment>());

            Assert.ThrowsAsync<NotFoundException>(() => _service.GetAppointmentsByPatientId(1));
        }

        [Test]
        public async Task CompleteAppointment_ValidId_ShouldSetStatusToCompleted()
        {
            var appointment = new Appointment
            {
                AppointmentID = 1,
                StatusID = 2 
            };

            _appointmentRepoMock.Setup(r => r.GetById(1)).ReturnsAsync(appointment);
            _appointmentRepoMock
                .Setup(r => r.Update(1, It.IsAny<Appointment>()))
                .ReturnsAsync((int id, Appointment a) => a);

            var result = await _service.CompleteAppointment(1);

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.StatusID); 
        }
        [Test]
        public void CompleteAppointment_InvalidId_ShouldThrowNotFoundException()
        {
            _appointmentRepoMock.Setup(r => r.GetById(99)).ReturnsAsync((Appointment)null);

            var ex = Assert.ThrowsAsync<NotFoundException>(() => _service.CompleteAppointment(99));
            Assert.AreEqual("No appointments were found.", ex.Message);
        }


        [Test]
        public async Task RescheduleAppointment_ValidId_ShouldUpdateDate()
        {
            var oldDate = DateTime.Now;
            var newDate = oldDate.AddDays(3);
            var appointment = new Appointment { AppointmentID = 1, AppointmentDateTime = oldDate };

            _appointmentRepoMock.Setup(r => r.GetById(1)).ReturnsAsync(appointment);
            _appointmentRepoMock
                .Setup(r => r.Update(1, It.IsAny<Appointment>()))
                .ReturnsAsync((int id, Appointment a) => a);

            var result = await _service.RescheduleAppointment(1, newDate);

            Assert.IsNotNull(result);
            Assert.AreEqual(newDate, result.AppointmentDateTime);
        }
        [Test]
        public void RescheduleAppointment_InvalidId_ShouldThrowNotFoundException()
        {
            _appointmentRepoMock.Setup(r => r.GetById(99)).ReturnsAsync((Appointment)null);

            var ex = Assert.ThrowsAsync<NotFoundException>(() => _service.RescheduleAppointment(99, DateTime.Now));
            Assert.AreEqual("No appointments were found.", ex.Message);
        }
        [Test]
        public async Task GetAppointmentsByDate_ValidDate_ReturnsMatchingAppointments()
        {
            var date = DateTime.Today;
            var appointments = new List<Appointment>
            {
                new Appointment { AppointmentID = 1, AppointmentDateTime = date },
                new Appointment { AppointmentID = 2, AppointmentDateTime = date.AddDays(-1) }
            };
            _appointmentRepoMock.Setup(r => r.GetAll()).ReturnsAsync(appointments);

            var result = await _service.GetAppointmentsByDate(date);

            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().AppointmentID, Is.EqualTo(1));
        }
        [Test]
        public async Task GetAppointmentsByStatus_ValidStatus_ReturnsMatchingAppointments()
        {
            var appointments = new List<Appointment>
            {
                new Appointment { AppointmentID = 1, StatusID = 1 },
                new Appointment { AppointmentID = 2, StatusID = 2 }
            };
            _appointmentRepoMock.Setup(r => r.GetAll()).ReturnsAsync(appointments);

            var result = await _service.GetAppointmentsByStatus(1);

            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().AppointmentID, Is.EqualTo(1));
        }
        [Test]
        public async Task SearchAppointments_WithDoctorAndStatus_ReturnsFilteredResults()
        {
            var appointments = new List<Appointment>
            {
                new Appointment { AppointmentID = 1, DoctorID = 101, StatusID = 2, AppointmentDateTime = DateTime.Today, Doctor = new Doctor { Name = "Dr A" }, Patient = new Patient { FullName = "Patient A" }, Status = new AppointmentStatusMaster { StatusName = "Confirmed" } },
                new Appointment { AppointmentID = 2, DoctorID = 102, StatusID = 3, AppointmentDateTime = DateTime.Today }
            };
            _appointmentRepoMock.Setup(r => r.GetAll()).ReturnsAsync(appointments);

            var request = new AppointmentSearchRequestDTO
            {
                DoctorID = 101,
                StatusID = 2,
                PageNumber = 1,
                PageSize = 10
            };
            var result = await _service.SearchAppointments(request);

            Assert.That(result.Appointments.Count, Is.EqualTo(1));
            Assert.That(result.Appointments.First().DoctorName, Is.EqualTo("Dr A"));
            Assert.That(result.TotalNumberOfRecords, Is.EqualTo(1));
        }
        [Test]
        public async Task SearchAppointments_WithDateRange_ReturnsAppointmentsWithinRange()
        {
            var appointments = new List<Appointment>
            {
                new Appointment { AppointmentID = 1, AppointmentDateTime = new DateTime(2024, 1, 10), Doctor = new Doctor(), Patient = new Patient(), Status = new AppointmentStatusMaster() },
                new Appointment { AppointmentID = 2, AppointmentDateTime = new DateTime(2024, 1, 20), Doctor = new Doctor(), Patient = new Patient(), Status = new AppointmentStatusMaster() },
                new Appointment { AppointmentID = 3, AppointmentDateTime = new DateTime(2024, 2, 1), Doctor = new Doctor(), Patient = new Patient(), Status = new AppointmentStatusMaster() }
            };

            _appointmentRepoMock.Setup(r => r.GetAll()).ReturnsAsync(appointments);

            var request = new AppointmentSearchRequestDTO
            {
                DateRange = new SearchRange<DateTime>
                {
                    MinValue = new DateTime(2024, 1, 15),
                    MaxValue = new DateTime(2024, 1, 31)
                },
                PageNumber = 1,
                PageSize = 10
            };

            var result = await _service.SearchAppointments(request);

            Assert.That(result.Appointments.Count, Is.EqualTo(1));
            Assert.That(result.Appointments.First().AppointmentID, Is.EqualTo(2));
        }

        [Test]
        public async Task SearchAppointments_SortByDateAscending_ReturnsSortedAppointments()
        {
            var appointments = new List<Appointment>
            {
                new Appointment { AppointmentID = 1, AppointmentDateTime = new DateTime(2024, 5, 5), Doctor = new Doctor(), Patient = new Patient(), Status = new AppointmentStatusMaster() },
                new Appointment { AppointmentID = 2, AppointmentDateTime = new DateTime(2024, 3, 3), Doctor = new Doctor(), Patient = new Patient(), Status = new AppointmentStatusMaster() }
            };

            _appointmentRepoMock.Setup(r => r.GetAll()).ReturnsAsync(appointments);

            var request = new AppointmentSearchRequestDTO
            {
                Sort = 1,
                PageNumber = 1,
                PageSize = 10
            };

            var result = await _service.SearchAppointments(request);

            var sorted = result.Appointments.ToList();
            Assert.That(sorted[0].AppointmentID, Is.EqualTo(2));
            Assert.That(sorted[1].AppointmentID, Is.EqualTo(1));
        }
        [Test]
        public async Task SearchAppointments_SortByStatusDescending_ReturnsSortedAppointments()
        {
            var appointments = new List<Appointment>
            {
                new Appointment { AppointmentID = 1, StatusID = 1, AppointmentDateTime = DateTime.Today, Doctor = new Doctor(), Patient = new Patient(), Status = new AppointmentStatusMaster() },
                new Appointment { AppointmentID = 2, StatusID = 3, AppointmentDateTime = DateTime.Today, Doctor = new Doctor(), Patient = new Patient(), Status = new AppointmentStatusMaster() }
            };

            _appointmentRepoMock.Setup(r => r.GetAll()).ReturnsAsync(appointments);

            var request = new AppointmentSearchRequestDTO
            {
                Sort = -2,
                PageNumber = 1,
                PageSize = 10
            };

            var result = await _service.SearchAppointments(request);
            var sorted = result.Appointments.ToList();
            Assert.That(sorted[0].AppointmentID, Is.EqualTo(2));
            Assert.That(sorted[1].AppointmentID, Is.EqualTo(1));
        }
        [Test]
        public async Task SearchAppointments_SortByDoctorAscending_ReturnsSortedAppointments()
        {
            var appointments = new List<Appointment>
            {
                new Appointment { AppointmentID = 1, DoctorID = 200, AppointmentDateTime = DateTime.Today, Doctor = new Doctor(), Patient = new Patient(), Status = new AppointmentStatusMaster() },
                new Appointment { AppointmentID = 2, DoctorID = 100, AppointmentDateTime = DateTime.Today, Doctor = new Doctor(), Patient = new Patient(), Status = new AppointmentStatusMaster() }
            };

            _appointmentRepoMock.Setup(r => r.GetAll()).ReturnsAsync(appointments);

            var request = new AppointmentSearchRequestDTO
            {
                Sort = 3,
                PageNumber = 1,
                PageSize = 10
            };

            var result = await _service.SearchAppointments(request);

            var sorted = result.Appointments.ToList();
            Assert.That(sorted[0].AppointmentID, Is.EqualTo(2));
            Assert.That(sorted[1].AppointmentID, Is.EqualTo(1));
        }
        [Test]
        public async Task SearchAppointments_SortByDateDescending_ReturnsSortedAppointments()
        {
            var appointments = new List<Appointment>
            {
                new Appointment { AppointmentID = 1, AppointmentDateTime = new DateTime(2025, 8, 5), DoctorID = 1, StatusID = 1, Doctor = new Doctor(), Patient = new Patient(), Status = new AppointmentStatusMaster() },
                new Appointment { AppointmentID = 2, AppointmentDateTime = new DateTime(2025, 8, 10), DoctorID = 2, StatusID = 2, Doctor = new Doctor(), Patient = new Patient(), Status = new AppointmentStatusMaster() }
            };

            _appointmentRepoMock.Setup(r => r.GetAll()).ReturnsAsync(appointments);

            var request = new AppointmentSearchRequestDTO
            {
                Sort = -1, 
                PageNumber = 1,
                PageSize = 10
            };

            var result = await _service.SearchAppointments(request);

            var sorted = result.Appointments.ToList();
            Assert.That(sorted[0].AppointmentID, Is.EqualTo(2));
            Assert.That(sorted[1].AppointmentID, Is.EqualTo(1));
        }
        [Test]
        public async Task SearchAppointments_SortByStatusAscending_ReturnsSortedAppointments()
        {
            var appointments = new List<Appointment>
            {
                new Appointment { AppointmentID = 1, StatusID = 5, AppointmentDateTime = DateTime.Today, DoctorID = 1, Doctor = new Doctor(), Patient = new Patient(), Status = new AppointmentStatusMaster() },
                new Appointment { AppointmentID = 2, StatusID = 2, AppointmentDateTime = DateTime.Today, DoctorID = 2, Doctor = new Doctor(), Patient = new Patient(), Status = new AppointmentStatusMaster() }
            };

            _appointmentRepoMock.Setup(r => r.GetAll()).ReturnsAsync(appointments);

            var request = new AppointmentSearchRequestDTO
            {
                Sort = 2, 
                PageNumber = 1,
                PageSize = 10
            };

            var result = await _service.SearchAppointments(request);

            var sorted = result.Appointments.ToList();
            Assert.That(sorted[0].AppointmentID, Is.EqualTo(2));
            Assert.That(sorted[1].AppointmentID, Is.EqualTo(1));
        }
        [Test]
        public async Task SearchAppointments_SortByDoctorDescending_ReturnsSortedAppointments()
        {
            var appointments = new List<Appointment>
            {
                new Appointment { AppointmentID = 1, DoctorID = 100, AppointmentDateTime = DateTime.Today, Doctor = new Doctor(), Patient = new Patient(), Status = new AppointmentStatusMaster() },
                new Appointment { AppointmentID = 2, DoctorID = 200, AppointmentDateTime = DateTime.Today, Doctor = new Doctor(), Patient = new Patient(), Status = new AppointmentStatusMaster() }
            };

            _appointmentRepoMock.Setup(r => r.GetAll()).ReturnsAsync(appointments);

            var request = new AppointmentSearchRequestDTO
            {
                Sort = -3,
                PageNumber = 1,
                PageSize = 10
            };

            var result = await _service.SearchAppointments(request);

            var sorted = result.Appointments.ToList();
            Assert.That(sorted[0].AppointmentID, Is.EqualTo(2));
            Assert.That(sorted[1].AppointmentID, Is.EqualTo(1));
        }
        [Test]
        public async Task SearchAppointments_SortByUnknownValue_HitsDefaultCase_ReturnsUnsortedAppointments()
        {
            var appointments = new List<Appointment>
            {
                new Appointment { AppointmentID = 1, DoctorID = 200, AppointmentDateTime = DateTime.Today, Doctor = new Doctor(), Patient = new Patient(), Status = new AppointmentStatusMaster() },
                new Appointment { AppointmentID = 2, DoctorID = 100, AppointmentDateTime = DateTime.Today, Doctor = new Doctor(), Patient = new Patient(), Status = new AppointmentStatusMaster() }
            };

            _appointmentRepoMock.Setup(r => r.GetAll()).ReturnsAsync(appointments);

            var request = new AppointmentSearchRequestDTO
            {
                Sort = 99, 
                PageNumber = 1,
                PageSize = 10
            };

            var result = await _service.SearchAppointments(request);

            var list = result.Appointments.ToList();
            Assert.That(list[0].AppointmentID, Is.EqualTo(1));
            Assert.That(list[1].AppointmentID, Is.EqualTo(2));
        }

        [Test]
        public async Task SearchAppointments_FilterByPatientId_ReturnsFilteredAppointments()
        {
            var appointments = new List<Appointment>
            {
                new Appointment { AppointmentID = 1, PatientID = 100, Doctor = new Doctor(), Patient = new Patient(), Status = new AppointmentStatusMaster() },
                new Appointment { AppointmentID = 2, PatientID = 200, Doctor = new Doctor(), Patient = new Patient(), Status = new AppointmentStatusMaster() },
                new Appointment { AppointmentID = 3, PatientID = 100, Doctor = new Doctor(), Patient = new Patient(), Status = new AppointmentStatusMaster() }
            };

            _appointmentRepoMock.Setup(r => r.GetAll()).ReturnsAsync(appointments);

            var request = new AppointmentSearchRequestDTO
            {
                PatientID = 100,
                PageNumber = 1,
                PageSize = 10
            };

            var result = await _service.SearchAppointments(request);

            var filtered = result.Appointments.ToList();
            Assert.That(filtered.Count, Is.EqualTo(2));
        }
        [Test]
        public async Task SearchAppointments_Pagination_WorksCorrectly()
        {
            var appointments = Enumerable.Range(1, 25).Select(i =>
                new Appointment
                {
                    AppointmentID = i,
                    PatientID = 1,
                    Doctor = new Doctor(),
                    Patient = new Patient(),
                    Status = new AppointmentStatusMaster()
                }).ToList();

            _appointmentRepoMock.Setup(r => r.GetAll()).ReturnsAsync(appointments);

            var request = new AppointmentSearchRequestDTO
            {
                PageNumber = 2,
                PageSize = 10
            };

            var result = await _service.SearchAppointments(request);

            var paged = result.Appointments.ToList();
            Assert.That(paged.Count, Is.EqualTo(10));
            Assert.That(paged[0].AppointmentID, Is.EqualTo(11));
            Assert.That(paged[9].AppointmentID, Is.EqualTo(20));
            Assert.That(result.TotalNumberOfRecords, Is.EqualTo(25));
            Assert.That(result.PageNumber, Is.EqualTo(2));
        }
        [Test]
        public async Task RejectAppointment_ValidId_UpdatesStatusToRejected()
        {
            var appointment = new Appointment { AppointmentID = 1, StatusID = 1 };
            _appointmentRepoMock.Setup(r => r.GetById(1)).ReturnsAsync(appointment);
            _appointmentRepoMock.Setup(r => r.Update(1, It.IsAny<Appointment>())).ReturnsAsync((int id, Appointment a) => a);

            var result = await _service.RejectAppointment(1);

            Assert.That(result.StatusID, Is.EqualTo(5));
            _appointmentRepoMock.Verify(r => r.Update(1, It.Is<Appointment>(a => a.StatusID == 5)), Times.Once);
        }
        [Test]
        public void RejectAppointment_InvalidId_ThrowsNotFoundException()
        {
            _appointmentRepoMock.Setup(r => r.GetById(It.IsAny<int>())).ReturnsAsync((Appointment)null);

            var ex = Assert.ThrowsAsync<NotFoundException>(() => _service.RejectAppointment(999));
            Assert.That(ex.Message, Does.Contain("appointments"));
        }
        [Test]
        public async Task GetAppointmentsByDoctorId_ReturnsAppointmentsForDoctor()
        {
            var appointments = new List<Appointment>
            {
                new Appointment { AppointmentID = 1, DoctorID = 100 },
                new Appointment { AppointmentID = 2, DoctorID = 200 },
                new Appointment { AppointmentID = 3, DoctorID = 100 }
            };

            _appointmentRepoMock.Setup(r => r.GetAll()).ReturnsAsync(appointments);

            var result = await _service.GetAppointmentsByDoctorId(100);

            var list = result.ToList();
            Assert.That(list.Count, Is.EqualTo(2));
            Assert.That(list.All(a => a.DoctorID == 100));
        }
        [Test]
        public void SearchAppointments_NoAppointments_ThrowsNotFoundException()
        {
            _appointmentRepoMock.Setup(r => r.GetAll()).ReturnsAsync(new List<Appointment>()); 

            var request = new AppointmentSearchRequestDTO
            {
                PageNumber = 1,
                PageSize = 10
            };

            var ex = Assert.ThrowsAsync<NotFoundException>(() => _service.SearchAppointments(request));
            Assert.That(ex.Message, Is.EqualTo("No appointments were found."));
        }


    }
}
