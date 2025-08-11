using AmazeCareAPI.Contexts;
using AmazeCareAPI.Interfaces;
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
using System.Linq;
using System.Threading.Tasks;

namespace AmazeCareAPI.Tests
{
    public class PrescriptionServiceTest
    {
        private PrescriptionService _service;
        private Mock<IRepository<int, Prescription>> _mockRepo;
        private ApplicationDbContext _context;
        private IMapper _mapper;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);

            _context.Medicines.Add(new MedicineMaster { MedicineID = 1, MedicineName = "Paracetamol" });
            _context.DosagePatterns.Add(new DosagePatternMaster { PatternID = 1, PatternCode = "101", Timing = "BF" });
            _context.SaveChanges();

            _mockRepo = new Mock<IRepository<int, Prescription>>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<AmazeCareAPI.Mappers.AmazeCareMapperProfile>();
            });
            _mapper = config.CreateMapper();

            _service = new PrescriptionService(_mockRepo.Object, _mapper, _context);
        }

        [Test]
        public async Task AddPrescriptionAsync_ShouldReturnMappedResponse()
        {
            var dto = new PrescriptionCreateDTO { RecordID = 1, MedicineID = 1, PatternID = 1, Quantity = 10, Days = 5, Notes = "Take with food" };

            var fakePrescription = _mapper.Map<Prescription>(dto);
            fakePrescription.PrescriptionID = 100;

            _mockRepo.Setup(r => r.Add(It.IsAny<Prescription>()))
                     .ReturnsAsync((Prescription p) =>
                     {
                         p.PrescriptionID = fakePrescription.PrescriptionID;
                         _context.Prescriptions.Add(p);
                         _context.SaveChanges();
                         return p;
                     });

            var result = await _service.AddPrescriptionAsync(dto);

            Assert.NotNull(result);
            Assert.AreEqual(dto.RecordID, result.RecordID);
            Assert.AreEqual("Paracetamol", result.MedicineName);
            Assert.AreEqual("101", result.PatternCode);
        }

        [Test]
        public async Task GetPrescriptionsByRecordIdAsync_RepositoryDB()
        {
            var prescription = new Prescription
            {
                RecordID = 20,
                MedicineID = 1,
                PatternID = 1,
                Quantity = 2,
                Days = 7,
                Notes = "After lunch",
                PrescribedDate = DateTime.UtcNow,
            };

            _context.Prescriptions.Add(prescription);
            _context.SaveChanges();

            var realRepo = new PrescriptionRepositoryDB(_context);
            _service = new PrescriptionService(realRepo, _mapper, _context);

            var result = await _service.GetPrescriptionsByRecordIdAsync(20);

            Assert.NotNull(result);
            var list = result.ToList();
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("Paracetamol", list[0].MedicineName);
            Assert.AreEqual("101", list[0].PatternCode);
        }

        [Test]
        public async Task GetPrescriptionsByRecordIdAsync_Generic()
        {
            var prescriptions = new List<Prescription>
            {
                new Prescription { RecordID = 30, MedicineID = 1, PatternID = 1, Quantity = 5, Days = 3, Notes = "Twice a day", PrescribedDate = DateTime.UtcNow, Medicine = new MedicineMaster { MedicineID = 1, MedicineName = "Paracetamol" }, DosagePattern = new DosagePatternMaster { PatternID = 1, PatternCode = "101" } },
                new Prescription { RecordID = 999, MedicineID = 1, PatternID = 1, Quantity = 1, Days = 1, Notes = "Once", PrescribedDate = DateTime.UtcNow, Medicine = new MedicineMaster { MedicineID = 1, MedicineName = "xndsnd" }, DosagePattern = new DosagePatternMaster { PatternID = 1, PatternCode = "102" } }

            };

            _mockRepo.Setup(r => r.GetAll()).ReturnsAsync(prescriptions);

            var result = await _service.GetPrescriptionsByRecordIdAsync(30);

            Assert.NotNull(result);
            var list = result.ToList();
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("Paracetamol", list[0].MedicineName);
            Assert.AreEqual("101", list[0].PatternCode);
            Assert.AreEqual(30, list[0].RecordID);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }
    }
}
