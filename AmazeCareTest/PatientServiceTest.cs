using AmazeCareAPI.Contexts;
using AmazeCareAPI.Exceptions;
using AmazeCareAPI.Interfaces;
using AmazeCareAPI.Models;
using AmazeCareAPI.Models.DTOs;
using AmazeCareAPI.Repositories;
using AmazeCareAPI.Services;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmazeCareAPI.Tests
{
    [TestFixture]
    public class PatientServiceTest
    {
        private Mock<IRepository<int, Patient>> _mockPatientRepo;
        private Mock<IRepository<int, GenderMaster>> _mockGenderRepo;
        private Mock<IRepository<string, User>> _mockUserRepo;
        private Mock<IMapper> _mockMapper;
        private Mock<ApplicationDbContext> _mockContext;
        private PatientService _service;

        [SetUp]
        public void Setup()
        {
            _mockPatientRepo = new Mock<IRepository<int, Patient>>();
            _mockGenderRepo = new Mock<IRepository<int, GenderMaster>>();
            _mockUserRepo = new Mock<IRepository<string, User>>();
            _mockMapper = new Mock<IMapper>();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDB")
                .Options;
            _mockContext = new Mock<ApplicationDbContext>(options);

            _service = new PatientService(
                _mockPatientRepo.Object,
                _mockGenderRepo.Object,
                _mockUserRepo.Object,
                _mockMapper.Object,
                _mockContext.Object
            );
        }

        [Test]
        public async Task GetPatientById_ShouldReturnPatient_WhenPatientExists()
        {
            var patientId = 1;
            var patient = new Patient { PatientID = patientId, FullName = "John Doe" };
            _mockPatientRepo.Setup(r => r.GetById(patientId)).ReturnsAsync(patient);

            var result = await _service.GetPatientById(patientId);

            Assert.IsNotNull(result);
            Assert.AreEqual("John Doe", result.FullName);
        }

        [Test]
        public void AddPatient_ShouldThrowException_WhenUsernameExists()
        {
            var addDto = new AddPatientRequestDTO { Username = "existingUser", Email = "test@example.com", Password = "password", RoleID = 1, FullName = "John Doe", DOB = DateTime.Parse("1990-01-01"), GenderID = 1, BloodGroup = "A+", ContactNumber = "1234567890", Address = "123 Street", statusID = 1 };


            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var dbContext = new ApplicationDbContext(options);

            var mockDatabaseFacade = new Mock<DatabaseFacade>(dbContext);
            mockDatabaseFacade.Setup(d => d.BeginTransactionAsync(default))
                              .ReturnsAsync(Mock.Of<IDbContextTransaction>());

            var mockContext = new Mock<ApplicationDbContext>(options);
            mockContext.Setup(c => c.Database).Returns(mockDatabaseFacade.Object);

            var mockUserRepoConcrete = new Mock<UserRepository>(dbContext);
            mockUserRepoConcrete.Setup(u => u.GetByUsernameAsync("existingUser"))
                .ReturnsAsync(new User { UserName = "existingUser" });

            var serviceWithConcreteUserRepo = new PatientService(
                _mockPatientRepo.Object,
                _mockGenderRepo.Object,
                mockUserRepoConcrete.Object,
                _mockMapper.Object,
                mockContext.Object
            );

            Assert.ThrowsAsync<Exception>(async () => await serviceWithConcreteUserRepo.AddPatient(addDto));
        }


        [Test]
        public async Task AddPatient_ShouldAddPatient_WhenUsernameNotExists()
        {
            var addDto = new AddPatientRequestDTO
            {
                Username = "newUser",
                Email = "test@example.com",
                Password = "password",
                RoleID = 1,
                FullName = "John Doe",
                DOB = DateTime.Parse("1990-01-01"),
                GenderID = 1,
                BloodGroup = "A+",
                ContactNumber = "1234567890",
                Address = "123 Street",
                statusID = 1
            };

            var newUser = new User { UserName = "newUser", Email = addDto.Email };
            var newPatient = new Patient { PatientID = 1, FullName = addDto.FullName, UserName = addDto.Username };

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var dbContext = new ApplicationDbContext(options);

            var mockDatabaseFacade = new Mock<DatabaseFacade>(dbContext);
            mockDatabaseFacade.Setup(d => d.BeginTransactionAsync(default))
                              .ReturnsAsync(Mock.Of<IDbContextTransaction>());

            var mockContext = new Mock<ApplicationDbContext>(options);
            mockContext.Setup(c => c.Database).Returns(mockDatabaseFacade.Object);

            var mockUserRepoConcrete = new Mock<UserRepository>(dbContext);
            mockUserRepoConcrete.Setup(u => u.GetByUsernameAsync("newUser")).ReturnsAsync((User?)null);
            mockUserRepoConcrete.Setup(u => u.Add(It.IsAny<User>())).ReturnsAsync(newUser);
            mockUserRepoConcrete.Setup(u => u.Update(It.IsAny<string>(), It.IsAny<User>())).ReturnsAsync(newUser);

            _mockPatientRepo.Setup(p => p.Add(It.IsAny<Patient>())).ReturnsAsync(newPatient);

            _mockMapper.Setup(m => m.Map<Patient>(addDto)).Returns(newPatient);

            var service = new PatientService(
                _mockPatientRepo.Object,
                _mockGenderRepo.Object,
                mockUserRepoConcrete.Object,
                _mockMapper.Object,
                mockContext.Object
            );

            var result = await service.AddPatient(addDto);

            Assert.IsNotNull(result);
            Assert.AreEqual(addDto.FullName, result.FullName);
            Assert.AreEqual(addDto.Username, result.UserName);
        }


        [Test]
        public async Task UpdatePatient_ShouldUpdateFields_WhenPatientExists()
        {
            var existingPatient = new Patient
            {
                PatientID = 1,
                FullName = "Old Name",
                DOB = DateTime.Parse("1980-01-01"),
                GenderID = 1,
                ContactNumber = "1112223333",
                Address = "Old Address"
            };

            var updateDto = new UpdatePatientRequestDTO
            {
                FullName = "New Name",
                DOB = DateTime.Parse("1990-01-01"),
                GenderID = 2,
                ContactNumber = "9998887777",
                Address = "New Address"
            };

            _mockPatientRepo.Setup(r => r.GetById(existingPatient.PatientID)).ReturnsAsync(existingPatient);
            _mockPatientRepo.Setup(r => r.Update(existingPatient.PatientID, It.IsAny<Patient>()))
                .ReturnsAsync((int id, Patient p) => p);

            var result = await _service.UpdatePatient(existingPatient.PatientID, updateDto);

            Assert.AreEqual(updateDto.FullName, result.FullName);
            Assert.AreEqual(updateDto.DOB, result.DOB);
            Assert.AreEqual(updateDto.GenderID, result.GenderID);
            Assert.AreEqual(updateDto.ContactNumber, result.ContactNumber);
            Assert.AreEqual(updateDto.Address, result.Address);
        }

        [Test]
        public void UpdatePatient_ShouldThrowException_WhenPatientNotFound()
        {
            _mockPatientRepo.Setup(r => r.GetById(It.IsAny<int>())).ReturnsAsync((Patient?)null);

            var updateDto = new UpdatePatientRequestDTO { FullName = "Name" };

            Assert.ThrowsAsync<Exception>(async () => await _service.UpdatePatient(999, updateDto));
        }

        [Test]
        public async Task DeletePatient_ShouldReturnTrue_WhenPatientDeleted()
        {
            var patient = new Patient { PatientID = 1 };
            _mockPatientRepo.Setup(r => r.GetById(patient.PatientID)).ReturnsAsync(patient);
            _mockPatientRepo.Setup(r => r.Delete(patient.PatientID)).ReturnsAsync(patient); 

            var result = await _service.DeletePatient(patient.PatientID);

            Assert.IsTrue(result);
        }


        [Test]
        public async Task DeletePatient_ShouldReturnFalse_WhenPatientNotFound()
        {
            _mockPatientRepo.Setup(r => r.GetById(It.IsAny<int>())).ReturnsAsync((Patient?)null);

            var result = await _service.DeletePatient(999);

            Assert.IsFalse(result);
        }

        [Test]
        public async Task GetAllPatients_ShouldReturnAllPatients()
        {
            var patients = new List<Patient>
            {
                new Patient { PatientID = 1, FullName = "John Doe" },
                new Patient { PatientID = 2, FullName = "Jane Smith" }
            };

            _mockPatientRepo.Setup(r => r.GetAll()).ReturnsAsync(patients);

            var result = await _service.GetAllPatients();

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Any(p => p.FullName == "John Doe"));
        }
        [Test]
        public async Task GetPatientsByStatusAsync_ShouldReturnMappedDtos()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDB_GetPatientsByStatus")
                .Options;

            var dbContext = new ApplicationDbContext(options);

            var mockPatientRepoDB = new Mock<PatientRepositoryDB>(dbContext);

            var patients = new List<Patient>
            {
                new Patient { PatientID = 1, FullName = "John" },
                new Patient { PatientID = 2, FullName = "Jane" }
            };

            mockPatientRepoDB
                .Setup(r => r.GetPatientsByStatusAsync(1))
                .ReturnsAsync(patients);

            _mockMapper
                .Setup(m => m.Map<IEnumerable<PatientSearchResponseDTO>>(patients))
                .Returns(new List<PatientSearchResponseDTO> { new(), new() });

            var service = new PatientService(
                mockPatientRepoDB.Object,
                _mockGenderRepo.Object,
                _mockUserRepo.Object,
                _mockMapper.Object,
                _mockContext.Object
            );

            var result = await service.GetPatientsByStatusAsync(1);

            Assert.AreEqual(2, result.Count());
        }

        [Test]
        public async Task DeactivatePatientAsync_ShouldReturnTrue()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDB")
            .Options;

            var dbContext = new ApplicationDbContext(options);

            var mockPatientRepoDB = new Mock<PatientRepositoryDB>(dbContext);

            mockPatientRepoDB.Setup(r => r.DeactivatePatientAsync(1)).ReturnsAsync(true);

            var service = new PatientService(
                mockPatientRepoDB.Object,
                _mockGenderRepo.Object,
                _mockUserRepo.Object,
                _mockMapper.Object,
                _mockContext.Object
            );

            var result = await service.DeactivatePatientAsync(1);

            Assert.IsTrue(result);
        }
        [Test]
        public async Task GetDataForAddingPatients_ShouldReturnGenders()
        {
            var genders = new List<GenderMaster>
            {
                new GenderMaster { GenderID = 1, GenderName = "Male" },
                new GenderMaster { GenderID = 2, GenderName = "Female" }
            };
            _mockGenderRepo.Setup(r => r.GetAll()).ReturnsAsync(genders);

            var result = await _service.GetDataForAddingPatients();

            Assert.AreEqual(2, result.Genders.Count);
            Assert.AreEqual("Male", result.Genders.First().GenderName);
        }
        [Test]
        public async Task SearchPatients_ShouldThrowNotFoundException_WhenNoPatients()
        {
            _mockPatientRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<Patient>());

            var request = new PatientSearchRequestDTO { PageNumber = 1, PageSize = 10 };

           Assert.ThrowsAsync<NotFoundException>(async () => await _service.SearchPatients(request));
        }

        [Test]
        public async Task SearchPatients_ShouldReturnPaginatedResults()
        {
            var patients = new List<Patient>
            {
                new Patient { PatientID = 1, FullName = "John", GenderID = 1, DOB = DateTime.Parse("1990-01-01") },
                new Patient { PatientID = 2, FullName = "Jane", GenderID = 2, DOB = DateTime.Parse("1995-01-01") }
            };

            _mockPatientRepo.Setup(r => r.GetAll()).ReturnsAsync(patients);
            _mockGenderRepo.Setup(r => r.GetById(It.IsAny<int>())).ReturnsAsync(new GenderMaster { GenderName = "Male" });
            _mockMapper.Setup(m => m.Map<PatientSearchResponseDTO>(It.IsAny<Patient>()))
                       .Returns((Patient p) => new PatientSearchResponseDTO { FullName = p.FullName });

            var request = new PatientSearchRequestDTO
            {
                FullName = "John",
                GenderIds = new List<int> { 1 },
                PageNumber = 1,
                PageSize = 10,
                Sort = 1
            };

            var result = await _service.SearchPatients(request);

            Assert.AreEqual(1, result.Patients.Count());
            Assert.AreEqual("John", result.Patients.First().FullName);
        }
        [Test]
        public async Task SearchPatients_ShouldFilterByDOBRange()
        {
            var patients = new List<Patient>
            {
                new Patient { PatientID = 1, FullName = "Alice", DOB = new DateTime(1990, 1, 1), GenderID = 1 },
                new Patient { PatientID = 2, FullName = "Bob", DOB = new DateTime(1980, 1, 1), GenderID = 1 },
                new Patient { PatientID = 3, FullName = "Charlie", DOB = new DateTime(2000, 1, 1), GenderID = 1 }
            };

            _mockPatientRepo.Setup(r => r.GetAll()).ReturnsAsync(patients);

            var dobRange = new SearchRange<DateTime>
            {
                MinValue = new DateTime(1985, 1, 1),
                MaxValue = new DateTime(1995, 12, 31)
            };

            var request = new PatientSearchRequestDTO
            {
                DOBRange = dobRange,
                PageNumber = 1,
                PageSize = 10
            };

            _mockMapper.Setup(m => m.Map<IEnumerable<PatientSearchResponseDTO>>(It.IsAny<IEnumerable<Patient>>()))
                       .Returns(new List<PatientSearchResponseDTO>());

            _mockMapper
                .Setup(m => m.Map<PatientSearchResponseDTO>(It.IsAny<Patient>()))
                .Returns((Patient p) => new PatientSearchResponseDTO { FullName = p.FullName });

            _mockGenderRepo
                .Setup(r => r.GetById(It.IsAny<int>()))
                .ReturnsAsync(new GenderMaster { GenderName = "Male" });

            var result = await _service.SearchPatients(request);

            Assert.AreEqual(1, result.Patients.Count());
            Assert.IsTrue(result.Patients.All(p => p.FullName == "Alice"));
        }
        [Test]
        public async Task SearchPatients_ShouldPaginateResults_WhenMoreThanPageSize()
        {
            var patients = new List<Patient>();
            for (int i = 1; i <= 20; i++)
            {
                patients.Add(new Patient
                {
                    PatientID = i,
                    FullName = "Patient " + i,
                    DOB = DateTime.Parse("1990-01-01"),
                    GenderID = 1
                });
            }

            _mockPatientRepo.Setup(r => r.GetAll()).ReturnsAsync(patients);

            _mockGenderRepo.Setup(r => r.GetById(It.IsAny<int>()))
                           .ReturnsAsync(new GenderMaster { GenderName = "Male" });

            _mockMapper.Setup(m => m.Map<PatientSearchResponseDTO>(It.IsAny<Patient>()))
                       .Returns((Patient p) => new PatientSearchResponseDTO { FullName = p.FullName });

            var request = new PatientSearchRequestDTO
            {
                PageNumber = 2,
                PageSize = 5,
                Sort = 1  
            };

            var result = await _service.SearchPatients(request);

            Assert.AreEqual(5, result.Patients.Count()); 
            Assert.AreEqual(20, result.TotalNumberOfRecords); 
            Assert.AreEqual(2, result.PageNumber);

            Assert.AreEqual("Patient 14", result.Patients.First().FullName);
        }
        [TestCase(1)]   
        [TestCase(-1)]  
        [TestCase(2)]   
        [TestCase(-2)]
        public async Task SearchPatients_ShouldSortPatients_BySortParameter(int sort)
        {
            var patients = new List<Patient>
            {
                new Patient { PatientID = 1, FullName = "Charlie", DOB = new DateTime(1992,1,1), GenderID = 1 },
                new Patient { PatientID = 2, FullName = "Alice", DOB = new DateTime(1990,1,1), GenderID = 1 },
                new Patient { PatientID = 3, FullName = "Bob", DOB = new DateTime(1991,1,1), GenderID = 1 }
            };

            _mockPatientRepo.Setup(r => r.GetAll()).ReturnsAsync(patients);

            _mockGenderRepo.Setup(r => r.GetById(It.IsAny<int>())).ReturnsAsync(new GenderMaster { GenderName = "Male" });

            _mockMapper.Setup(m => m.Map<PatientSearchResponseDTO>(It.IsAny<Patient>()))
                       .Returns((Patient p) => new PatientSearchResponseDTO { FullName = p.FullName });

            var request = new PatientSearchRequestDTO
            {
                PageNumber = 1,
                PageSize = 10,
                Sort = sort
            };

            var result = await _service.SearchPatients(request);

            var sortedNames = result.Patients.Select(p => p.FullName).ToList();

            switch (sort)
            {
                case 1:
                    CollectionAssert.AreEqual(new[] { "Alice", "Bob", "Charlie" }, sortedNames);
                    break;
                case -1:
                    CollectionAssert.AreEqual(new[] { "Charlie", "Bob", "Alice" }, sortedNames);
                    break;
                case 2:
                    CollectionAssert.AreEqual(new[] { "Alice", "Bob", "Charlie" }, sortedNames);
                    break;
                case -2: 
                    CollectionAssert.AreEqual(new[] { "Charlie", "Bob", "Alice" }, sortedNames);
                    break;
                default:
                    Assert.Fail("Unexpected sort value");
                    break;
            }
        }
        [Test]
        public async Task AddPatient_ShouldRollbackAndThrow_WhenExceptionOccurs()
        {
            var addDto = new AddPatientRequestDTO { Username = "newUser", Email = "test@example.com", Password = "password", RoleID = 1, FullName = "John Doe", DOB = DateTime.Parse("1990-01-01"), GenderID = 1, BloodGroup = "A+", ContactNumber = "1234567890", Address = "123 Street", statusID = 1 };

            var dbContext = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

            var mockDatabaseFacade = new Mock<DatabaseFacade>(dbContext);
            mockDatabaseFacade.Setup(d => d.BeginTransactionAsync(default))
                              .ReturnsAsync(Mock.Of<IDbContextTransaction>());

            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.Database).Returns(mockDatabaseFacade.Object);

            var mockUserRepoConcrete = new Mock<UserRepository>(dbContext);
            mockUserRepoConcrete.Setup(u => u.GetByUsernameAsync("newUser")).ReturnsAsync((User?)null);
            mockUserRepoConcrete.Setup(u => u.Add(It.IsAny<User>())).ThrowsAsync(new Exception("DB error"));

            _mockPatientRepo.Setup(p => p.Add(It.IsAny<Patient>())).ReturnsAsync(new Patient());

            var service = new PatientService(
                _mockPatientRepo.Object,
                _mockGenderRepo.Object,
                mockUserRepoConcrete.Object,
                _mockMapper.Object,
                mockContext.Object
            );

            var ex = Assert.ThrowsAsync<Exception>(async () => await service.AddPatient(addDto));
            Assert.That(ex.Message, Is.EqualTo("Patient registration failed"));
        }
        [Test]
        public void AddPatient_ShouldThrowException_WhenUserRepositoryIsNotUserRepositoryType()
        {
            var addDto = new AddPatientRequestDTO { Username = "testUser", Email = "test@example.com", Password = "password", RoleID = 1, FullName = "John Doe", DOB = DateTime.Parse("1990-01-01"), GenderID = 1, BloodGroup = "A+", ContactNumber = "1234567890", Address = "123 Street", statusID = 1 };
            var mockUserRepo = new Mock<IRepository<string, User>>();

            var service = new PatientService(
                _mockPatientRepo.Object,
                _mockGenderRepo.Object,
                mockUserRepo.Object, 
                _mockMapper.Object,
                _mockContext.Object
            );

            var ex = Assert.ThrowsAsync<Exception>(async () => await service.AddPatient(addDto));
            Assert.That(ex.Message, Is.EqualTo("Invalid user repository"));
        }
        [Test]
        public async Task SearchPatients_ShouldReturnUnsorted_WhenSortIsDefault()
        {
            var patients = new List<Patient>
            {
                new Patient { PatientID = 1, FullName = "Bob", DOB = DateTime.Parse("1990-01-01") },
                new Patient { PatientID = 2, FullName = "Alice", DOB = DateTime.Parse("1985-01-01") }
            };
            _mockPatientRepo.Setup(r => r.GetAll()).ReturnsAsync(patients);
            _mockGenderRepo.Setup(r => r.GetById(It.IsAny<int>())).ReturnsAsync(new GenderMaster { GenderName = "Male" });
            _mockMapper.Setup(m => m.Map<PatientSearchResponseDTO>(It.IsAny<Patient>()))
                       .Returns((Patient p) => new PatientSearchResponseDTO { FullName = p.FullName });

            var request = new PatientSearchRequestDTO
            {
                PageNumber = 1,
                PageSize = 10,
                Sort = 0 
            };

            var result = await _service.SearchPatients(request);

            Assert.AreEqual(2, result.Patients.Count());
            Assert.AreEqual("Bob", result.Patients.First().FullName);
        }

    }
}
