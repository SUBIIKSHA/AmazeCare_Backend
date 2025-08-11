using AmazeCareAPI.Contexts;
using AmazeCareAPI.Exceptions;
using AmazeCareAPI.Models;
using AmazeCareAPI.Models.DTOs;
using AmazeCareAPI.Repositories;
using AmazeCareAPI.Services;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AmazeCareAPI.Tests
{
    public class BillingServiceTest
    {
        private BillingService _service;
        private BillingRepositoryDB _repository;
        private ApplicationDbContext _context;
        private IMapper _mapper;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Billing, BillingResponseDTO>();
            });
            _mapper = config.CreateMapper();

            _repository = new BillingRepositoryDB(_context);
            _service = new BillingService(_repository, _mapper);
        }

        [Test]
        public async Task GetAllAsync_ReturnsAllBillings()
        {
            var appointment = new Appointment { AppointmentID = 1, PatientID = 101 };
            var status = new BillingStatusMaster { StatusID = 1, StatusName = "Paid" };

            _context.Appointments.Add(appointment);
            _context.BillingStatuses.Add(status);
            await _context.SaveChangesAsync();

            _context.Billings.Add(new Billing
            {
                BillingID = 1,
                TotalAmount = 100,
                AppointmentID = appointment.AppointmentID,
                StatusID = status.StatusID,
                BillingDate = DateTime.Now,
                PatientID = 101
            });

            await _context.SaveChangesAsync();

            var result = await _service.GetAllAsync();
            Assert.AreEqual(1, result.Count());
        }

        [Test]
        public async Task GetByIdAsync_ReturnsCorrectBilling()
        {
            var appointment = new Appointment { AppointmentID = 1, PatientID = 1001, AppointmentDateTime = DateTime.Now };
            _context.Appointments.Add(appointment);

            var billingStatus = new BillingStatusMaster { StatusID = 1, StatusName = "Paid" };
            _context.BillingStatuses.Add(billingStatus);
            await _context.SaveChangesAsync();

            var billing = new Billing
            {
                BillingID = 1,
                AppointmentID = appointment.AppointmentID,
                StatusID = billingStatus.StatusID,
                BillingDate = DateTime.Now,
                TotalAmount = 150,
                PatientID = 1001
            };
            _context.Billings.Add(billing);
            await _context.SaveChangesAsync();

            var result = await _service.GetByIdAsync(1);
            Assert.AreEqual(150, result.TotalAmount);
        }

        [Test]
        public void GetByIdAsync_ThrowsWhenBillingNotFound()
        {
            var ex = Assert.ThrowsAsync<NoSuchEntityException>(async () =>
                await _service.GetByIdAsync(999));

            Assert.That(ex.Message, Is.EqualTo("The requested entity was not found."));
        }

        [Test]
        public async Task GetByAppointmentIdAsync_ReturnsCorrectBillings()
        {
            var appointment = new Appointment { AppointmentID = 1, PatientID = 1001, AppointmentDateTime = DateTime.Now };
            _context.Appointments.Add(appointment);

            var status = new BillingStatusMaster { StatusID = 1, StatusName = "Paid" };
            _context.BillingStatuses.Add(status);
            await _context.SaveChangesAsync();

            _context.Billings.AddRange(
                new Billing { BillingID = 1, AppointmentID = 1, StatusID = 1, BillingDate = DateTime.Today, TotalAmount = 100 },
                new Billing { BillingID = 2, AppointmentID = 2, StatusID = 1, BillingDate = DateTime.Today, TotalAmount = 200 }
            );
            await _context.SaveChangesAsync();

            var result = await _service.GetByAppointmentIdAsync(1);

            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(100, result.First().TotalAmount);
        }

        [Test]
        public async Task GetByAppointmentIdAsync_ReturnsEmptyList_WhenNoBillings()
        {
            var result = await _service.GetByAppointmentIdAsync(1);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }

  

        [Test]
        public async Task GetByStatusIdAsync_ReturnsEmptyList_WhenNoBillings()
        {
            var result = await _service.GetByStatusIdAsync(1);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }

        [Test]
        public async Task GetPatientIdByAppointment_NoAppointment_Throws()
        {
            var method = typeof(BillingService).GetMethod("GetPatientIdByAppointment", BindingFlags.NonPublic | BindingFlags.Instance);
            var ex = Assert.ThrowsAsync<NoSuchEntityException>(async () => await (Task<int>)method.Invoke(_service, new object[] { 999 }));
            Assert.AreEqual("Appointment not found.", ex.Message);
        }


        [Test]
        public async Task GetByDateRangeAsync_ReturnsEmptyList_WhenNoMatches()
        {
            var appointment = new Appointment { AppointmentID = 1, PatientID = 1001 };
            _context.Appointments.Add(appointment);
            var status = new BillingStatusMaster { StatusID = 1, StatusName = "Paid" };
            _context.BillingStatuses.Add(status);
            await _context.SaveChangesAsync();

            _context.Billings.Add(new Billing
            {
                BillingID = 1,
                AppointmentID = 1,
                StatusID = 1,
                BillingDate = new DateTime(2025, 8, 1),
                TotalAmount = 100
            });
            await _context.SaveChangesAsync();

            var result = await _service.GetByDateRangeAsync(
                new DateTime(2025, 8, 5),
                new DateTime(2025, 8, 10));

            Assert.IsEmpty(result);
        }

        [Test]
        public void CreateAsync_ThrowsWhenAppointmentNotFound()
        {
            var billingDTO = new BillingCreateDTO
            {
                AppointmentID = 999,
                StatusID = 1,
                BillingDate = DateTime.Now
            };

            var ex = Assert.ThrowsAsync<NoSuchEntityException>(async () =>
                await _service.CreateAsync(billingDTO));

            StringAssert.Contains("Medical record not found for this appointment.", ex.Message);
        }

        [Test]
        public void CreateAsync_ThrowsWhenMedicalRecordNotFound()
        {
            var appointment = new Appointment { AppointmentID = 1, PatientID = 5 };
            _context.Appointments.Add(appointment);
            _context.SaveChanges();

            var billingDTO = new BillingCreateDTO
            {
                AppointmentID = 1,
                StatusID = 1,
                BillingDate = DateTime.Now
            };

            var ex = Assert.ThrowsAsync<NoSuchEntityException>(async () =>
                await _service.CreateAsync(billingDTO));

            Assert.That(ex.Message, Is.EqualTo("Medical record not found for this appointment."));
        }

        [Test]
        public async Task CreateAsync_CalculatesTotalCorrectly()
        {
            var appointment = new Appointment { AppointmentID = 1, PatientID = 10 };
            var med = new MedicineMaster { MedicineID = 1, MedicineName = "Paracetamol", Price = 10 };
            var test = new TestMaster { TestID = 1, TestName = "Blood Test", Price = 50 };
            var prescription = new Prescription
            {
                PrescriptionID = 1,
                RecordID = 1,
                Medicine = med,
                Quantity = 2,
                RecommendedTests = new List<RecommendedTest>
                {
                    new RecommendedTest { Test = test }
                }
            };
            var medicalRecord = new MedicalRecord
            {
                RecordID = 1,
                AppointmentID = 1,
                Prescriptions = new List<Prescription> { prescription }
            };

            _context.Appointments.Add(appointment);
            _context.Medicines.Add(med);
            _context.Tests.Add(test);
            _context.MedicalRecords.Add(medicalRecord);
            await _context.SaveChangesAsync();

            var billingDTO = new BillingCreateDTO
            {
                AppointmentID = 1,
                StatusID = 1,
                BillingDate = DateTime.Now
            };

            var result = await _service.CreateAsync(billingDTO);

            Assert.AreEqual(70, result.TotalAmount);
        }

        [Test]
        public void CreateAsync_Throws_WhenNoMedicinesAndTests()
        {
            var appointment = new Appointment { AppointmentID = 1, PatientID = 10 };
            var medicalRecord = new MedicalRecord
            {
                RecordID = 1,
                AppointmentID = 1,
                Prescriptions = new List<Prescription>() 
            };
            _context.Appointments.Add(appointment);
            _context.MedicalRecords.Add(medicalRecord);
            _context.SaveChanges();

            var billingDTO = new BillingCreateDTO
            {
                AppointmentID = 1,
                StatusID = 1,
                BillingDate = DateTime.Now
            };

            var ex = Assert.ThrowsAsync<Exception>(async () =>
                await _service.CreateAsync(billingDTO));

            Assert.That(ex.Message, Is.EqualTo("No prescriptions found to generate billing."));
        }

        [Test]
        public async Task CreateAsync_HandlesZeroPriceMedicinesAndTests()
        {
            var appointment = new Appointment { AppointmentID = 1, PatientID = 10 };
            var med = new MedicineMaster { MedicineID = 1, MedicineName = "FreeMed", Price = 0 };
            var test = new TestMaster { TestID = 1, TestName = "FreeTest", Price = 0 };
            var prescription = new Prescription
            {
                PrescriptionID = 1,
                RecordID = 1,
                Medicine = med,
                Quantity = 2,
                RecommendedTests = new List<RecommendedTest>
                {
                    new RecommendedTest { Test = test }
                }
            };
            var medicalRecord = new MedicalRecord
            {
                RecordID = 1,
                AppointmentID = 1,
                Prescriptions = new List<Prescription> { prescription }
            };

            _context.Appointments.Add(appointment);
            _context.Medicines.Add(med);
            _context.Tests.Add(test);
            _context.MedicalRecords.Add(medicalRecord);
            await _context.SaveChangesAsync();

            var billingDTO = new BillingCreateDTO
            {
                AppointmentID = 1,
                StatusID = 1,
                BillingDate = DateTime.Now
            };

            var result = await _service.CreateAsync(billingDTO);

            Assert.AreEqual(0, result.TotalAmount);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }
    }
}
