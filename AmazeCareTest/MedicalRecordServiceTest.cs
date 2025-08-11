using AmazeCareAPI.Contexts;
using AmazeCareAPI.Exceptions;
using AmazeCareAPI.Interfaces;
using AmazeCareAPI.Mappers;
using AmazeCareAPI.Models;
using AmazeCareAPI.Models.DTOs;
using AmazeCareAPI.Repositories;
using AmazeCareAPI.Services;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace AmazeCareAPI.Tests
{
    [TestFixture]
    public class MedicalRecordServiceTest
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;
        private MedicalRecordService _service;
    [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            _context.Patients.AddRange(
                new Patient { PatientID = 201, UserName = "Johnkk", FullName = "John" },
                new Patient { PatientID = 202, UserName = "Janeshan", FullName = "Jane" }
            );

            _context.Doctors.AddRange(
                new Doctor { DoctorID = 101, Name = "Dr. Smith", UserName = "drsmith101" },
                new Doctor { DoctorID = 102, Name = "Dr. Doe", UserName = "drdoe102" }
            );


            _context.AppointmentStatuses.AddRange(
                new AppointmentStatusMaster { StatusID = 1, StatusName = "Confirmed" } 
            );

            _context.Appointments.AddRange(
            new Appointment { AppointmentID = 1, DoctorID = 101, PatientID = 201, StatusID = 1 },
            new Appointment { AppointmentID = 2, DoctorID = 102, PatientID = 202, StatusID = 1 }
        );

            _context.MedicalRecords.AddRange(
                new MedicalRecord { RecordID = 1, AppointmentID = 1, Status = "active", Diagnosis = "Flu", TreatmentPlan = "Rest" },
                new MedicalRecord { RecordID = 2, AppointmentID = 2, Status = "inactive", Diagnosis = "Cold", TreatmentPlan = "Tablets" }
            );
            _context.SaveChanges();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<CreateMedicalRecordDTO, MedicalRecord>();
                cfg.CreateMap<MedicalRecord, MedicalRecordDTO>();
                cfg.AddProfile<AmazeCareMapperProfile>();
            });
            _mapper = config.CreateMapper();

            var medicalRecordRepo = new MedicalRecordRepositoryDB(_context);
            var appointmentRepo = new AppointmentRepositoryDB(_context);

            _service = new MedicalRecordService(medicalRecordRepo, appointmentRepo, _mapper);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task CreateMedicalRecord_ValidDto_ShouldAddToDatabase()
        {
            var dto = new CreateMedicalRecordDTO
            {
                AppointmentID = 2,
                Diagnosis = "Cough",
                TreatmentPlan = "Syrup"
            };

            var result = await _service.CreateAsync(dto);

            Assert.NotNull(result);
            Assert.AreEqual(dto.Diagnosis, result.Diagnosis);
        }
        [Test]
        public void CreateMedicalRecord_InvalidAppointmentId()
        { 
            var invalidDto = new CreateMedicalRecordDTO
            {
                AppointmentID = 999, 
                Diagnosis = "Headache",
                TreatmentPlan = "Painkillers"
            };

            var ex = Assert.ThrowsAsync<NoSuchEntityException>(async () => await _service.CreateAsync(invalidDto));
            Assert.That(ex.Message, Is.EqualTo("Appointment with ID 999 not found"));
        }
        [Test]
        public async Task GetAllAsync_ShouldReturnAllMedicalRecords()
        {
            var result = await _service.GetAllAsync();

            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.AreEqual(2, resultList.Count);
            Assert.IsTrue(resultList.Any(r => r.Diagnosis == "Flu"));
            Assert.IsTrue(resultList.Any(r => r.Diagnosis == "Cold"));
        }

        [Test]
        public async Task UpdateAsync_ValidIdAndDto_ShouldUpdateRecord()
        {
            var updateDto = new UpdateMedicalRecordDTO
            {
                Diagnosis = "Updated Diagnosis",
                TreatmentPlan = "Updated Plan"
            };

            var result = await _service.UpdateAsync(1, updateDto);

            Assert.NotNull(result);
            Assert.AreEqual(updateDto.Diagnosis, result.Diagnosis);
            Assert.AreEqual(updateDto.TreatmentPlan, result.TreatmentPlan);
        }
        [Test]
        public async Task DeleteAsync_ValidId_ShouldDeleteRecord()
        {
            var deleted = await _service.DeleteAsync(1);

            Assert.IsTrue(deleted);
            var remaining = await _service.GetAllAsync();
            Assert.AreEqual(1, remaining.Count());
        }

        [Test]
        public async Task DeleteAsync_InvalidId_ShouldReturnFalse()
        {
            var mockRepo = new Mock<IRepository<int, MedicalRecord>>();
            var mockAppointmentRepo = new Mock<IRepository<int, Appointment>>();

            mockRepo.Setup(r => r.Delete(999)).ReturnsAsync((MedicalRecord)null); 

            var service = new MedicalRecordService(mockRepo.Object, mockAppointmentRepo.Object, _mapper);

            var deleted = await service.DeleteAsync(999);

            Assert.IsFalse(deleted);
        }

        [Test]
        public async Task GetMedicalRecordById_ExistingId_ReturnsRecord()
        {
            var result = await _service.GetByIdAsync(1);
            Assert.NotNull(result);
            Assert.AreEqual("Flu", result.Diagnosis);
        }

        [Test]
        public async Task GetMedicalRecordById_NonExistingId_ReturnsNull()
        {
            var result = await _service.GetByIdAsync(999);
            Assert.IsNull(result);
        }

        [Test]
        public async Task GetMedicalRecordsByStatus_ExistingStatus_ReturnsRecords()
        {
            var result = (await _service.GetMedicalRecordsByStatusAsync("active")).ToList();
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Flu", result[0].Diagnosis);
        }

        [Test]
        public async Task GetMedicalRecordsByStatus_NonExistingStatus_ReturnsEmpty()
        {
            var result = await _service.GetMedicalRecordsByStatusAsync("deleted");
            Assert.IsEmpty(result);
        }

        [Test]
        public async Task GetMedicalRecordsByAppointmentId_ValidId_ReturnsRecords()
        {
            var result = (await _service.GetByAppointmentIdAsync(1)).ToList();
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Flu", result[0].Diagnosis);
        }

        [Test]
        public async Task GetMedicalRecordsByAppointmentId_InvalidId_ReturnsEmpty()
        {
            var result = await _service.GetByAppointmentIdAsync(999);
            Assert.IsEmpty(result);
        }

        [Test]
        public async Task DeleteMedicalRecord_ValidId_SoftDeletesRecord()
        {
            var result = await _service.DeleteMedicalRecord(1);
            Assert.IsTrue(result);

            var record = await _context.MedicalRecords.FindAsync(1);
            Assert.AreEqual("Deleted", record.Status);
        }

        [Test]
        public async Task DeleteMedicalRecord_InvalidId_ReturnsFalse()
        {
            var result = await _service.DeleteMedicalRecord(999);
            Assert.IsFalse(result);
        }
        [Test]
        public async Task GetByAppointmentIdAsync_UsesFallbackFiltering_WhenRepositoryIsNotDb()
        {
            var mockRepo = new Mock<IRepository<int, MedicalRecord>>();

            var allRecords = new List<MedicalRecord>
    {
        new MedicalRecord { RecordID = 1, AppointmentID = 10, Diagnosis = "Flu", Status = "active", TreatmentPlan = "Rest" },
        new MedicalRecord { RecordID = 2, AppointmentID = 20, Diagnosis = "Cold", Status = "inactive", TreatmentPlan = "Tablets" }
    };

            mockRepo.Setup(r => r.GetAll()).ReturnsAsync(allRecords);
            var mockAppointmentRepo = new Mock<IRepository<int, Appointment>>();

            var service = new MedicalRecordService(mockRepo.Object, mockAppointmentRepo.Object, _mapper);
            var result = await service.GetByAppointmentIdAsync(10);

            Assert.NotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(10, result.First().AppointmentID);
        }


    }
}