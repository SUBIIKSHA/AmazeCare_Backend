using AmazeCareAPI.Contexts;
using AmazeCareAPI.Exceptions;
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
    public class RecommendedTestServiceTest
    {
        private RecommendedTestService _service;
        private Mock<RecommendedTestRepositoryDB> _mockRepo;
        private ApplicationDbContext _context;
        private IMapper _mapper;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            _context.Prescriptions.Add(new Prescription { PrescriptionID = 1 });
            _context.Tests.Add(new TestMaster { TestID = 1, TestName = "Blood Test" });
            _context.SaveChanges();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<AmazeCareAPI.Mappers.AmazeCareMapperProfile>();
            });
            _mapper = config.CreateMapper();

            _mockRepo = new Mock<RecommendedTestRepositoryDB>(_context);

            _service = new RecommendedTestService(_mockRepo.Object, _context, _mapper);
        }

        [Test]
        public async Task GetAllAsync_ReturnsAllRecommendedTests()
        {
            var data = new List<RecommendedTest>
            {
                new RecommendedTest
                {
                    RecommendedTestID = 1,
                    TestID = 1,
                    PrescriptionID = 1,
                    Test = new TestMaster { TestID = 1, TestName = "Blood Test" }
                }
            };
            _mockRepo.Setup(r => r.GetAll()).ReturnsAsync(data);
            var result = await _service.GetAllAsync();

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("Blood Test", result.First().TestName);
        }

        [Test]
        public async Task GetByIdAsync_ExistingId_ReturnsRecommendedTest()
        {
            var testEntity = new RecommendedTest
            {
                RecommendedTestID = 1,
                TestID = 1,
                PrescriptionID = 1,
                Test = new TestMaster { TestID = 1, TestName = "Blood Test" }
            };
            _mockRepo.Setup(r => r.GetById(1)).ReturnsAsync(testEntity);

            var result = await _service.GetByIdAsync(1);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.RecommendedTestID);
            Assert.AreEqual("Blood Test", result.TestName);
        }

        [Test]
        public async Task GetByPrescriptionIdAsync_ReturnsFilteredRecommendedTests()
        {
            var prescription = new Prescription { RecordID = 1, Notes = "Sample Prescription" };
            var test1 = new TestMaster { TestID = 5, TestName = "Blood Test", Price = 200 };

            _context.Prescriptions.Add(prescription);
            _context.Tests.Add(test1);

            _context.RecommendedTests.AddRange(
                new RecommendedTest
                {
                    RecommendedTestID = 1,
                    PrescriptionID = 1,
                    TestID = 5
                }
            );

            _context.SaveChanges();

            var results = await _service.GetByPrescriptionIdAsync(1);

            Assert.IsNotNull(results);
            Assert.AreEqual(1, results.Count());
            Assert.True(results.Any(r => r.TestName == "Blood Test"));
        }



        [Test]
        public void AddAsync_InvalidPrescription_ThrowsNoSuchEntityException()
        {
            var dto = new RecommendedTestCreateDTO { PrescriptionID = 999, TestID = 1 };
            var ex = Assert.ThrowsAsync<NoSuchEntityException>(() => _service.AddAsync(dto));
            Assert.That(ex.Message, Does.Contain("MedicalRecord with ID 999 not found"));
        }

        [Test]
        public void AddAsync_InvalidTest_ThrowsNoSuchEntityException()
        {
            var dto = new RecommendedTestCreateDTO { PrescriptionID = 1, TestID = 999 };
            var ex = Assert.ThrowsAsync<NoSuchEntityException>(() => _service.AddAsync(dto));
            Assert.That(ex.Message, Does.Contain("Test with ID 999 not found"));
        }

        [Test]
        public async Task AddAsync_ValidDTO_ReturnsSavedRecommendedTestResponseDTO()
        {
            var dto = new RecommendedTestCreateDTO { PrescriptionID = 1, TestID = 1 };
            var entityToSave = _mapper.Map<RecommendedTest>(dto);
            entityToSave.RecommendedTestID = 10;

            _mockRepo.Setup(r => r.Add(It.IsAny<RecommendedTest>()))
                .ReturnsAsync(entityToSave);
            _context.RecommendedTests.Add(entityToSave);
            _context.SaveChanges();

            var result = await _service.AddAsync(dto);

            Assert.IsNotNull(result);
            Assert.AreEqual(10, result.RecommendedTestID);
            Assert.AreEqual(dto.TestID, result.TestID);
        }

        [Test]
        public async Task DeleteAsync_ExistingId_ReturnsDeletedEntity()
        {
            var entity = new RecommendedTest
            {
                RecommendedTestID = 1,
                TestID = 1,
                PrescriptionID = 1,
                Test = new TestMaster { TestID = 1, TestName = "Blood Test" }
            };
            _mockRepo.Setup(r => r.Delete(1)).ReturnsAsync(entity);

            var result = await _service.DeleteAsync(1);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.RecommendedTestID);
        }

        [Test]
        public void DeleteAsync_InvalidId_ThrowsNoSuchEntityException()
        {
            _mockRepo.Setup(r => r.Delete(999)).ReturnsAsync((RecommendedTest)null);
            var ex = Assert.ThrowsAsync<NoSuchEntityException>(() => _service.DeleteAsync(999));
            Assert.That(ex.Message, Does.Contain("RecommendedTest with ID 999 not found"));
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }
    }
}
