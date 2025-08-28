using AmazeCareAPI.Contexts;
using AmazeCareAPI.Exceptions;
using AmazeCareAPI.Interfaces;
using AmazeCareAPI.Models;
using AmazeCareAPI.Models.DTOs;
using AmazeCareAPI.Repositories;
using AmazeCareAPI.Services;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmazeCareAPI.Tests
{
    [TestFixture]
    public class DoctorServiceTest
    {
        private Mock<IRepository<int, Doctor>> _mockDoctorRepo;
        private Mock<IRepository<int, SpecializationMaster>> _mockSpecializationRepo;
        private Mock<IRepository<int, QualificationMaster>> _mockQualificationRepo;
        private Mock<IRepository<string, User>> _mockUserRepo;
        private Mock<IMapper> _mockMapper;
        private ApplicationDbContext _context;
        private DoctorService _service;

        [SetUp]
        public void Setup()
        {
            _mockDoctorRepo = new Mock<IRepository<int, Doctor>>();
            _mockSpecializationRepo = new Mock<IRepository<int, SpecializationMaster>>();
            _mockQualificationRepo = new Mock<IRepository<int, QualificationMaster>>();
            _mockUserRepo = new Mock<IRepository<string, User>>();
            _mockMapper = new Mock<IMapper>();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) 
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning)) 
                .Options;

            _context = new ApplicationDbContext(options);

            _service = new DoctorService(
                _mockDoctorRepo.Object,
                _mockSpecializationRepo.Object,
                _mockQualificationRepo.Object,
                _mockUserRepo.Object,
                _mockMapper.Object,
                _context 
            );
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task GetDoctorById_ShouldReturnDoctor_WhenExists()
        {
            var doctorId = 1;
            var doctor = new Doctor { DoctorID = doctorId, Name = "Dr. Smith" };
            _mockDoctorRepo.Setup(r => r.GetById(doctorId)).ReturnsAsync(doctor);
            _mockMapper.Setup(m => m.Map<Doctor>(doctor)).Returns(doctor);

            var result = await _service.GetDoctorById(doctorId);

            Assert.IsNotNull(result);
            Assert.AreEqual("Dr. Smith", result.Name);
        }

        [Test]
        public void GetDoctorById_ShouldThrowException_WhenNotFound()
        {
            _mockDoctorRepo.Setup(r => r.GetById(It.IsAny<int>())).ReturnsAsync((Doctor?)null);

            Assert.ThrowsAsync<NoSuchEntityException>(async () => await _service.GetDoctorById(999));
        }

        [Test]
        public async Task AddDoctor_ShouldThrowException_WhenUsernameExists()
        {
            var dto = new AddDoctorRequestDTO
            {
                Username = "existingUser",
                Email = "a@b.com",
                Password = "password",
                RoleID = 1,
                Name = "Doc",
                ContactNumber = "123",
                SpecializationID = 1,
                QualificationID = 1,
                Experience = 5,
                Designation = "MD",
                statusID = 1
            };

            _context.Users.Add(new User
            {
                UserName = "existingUser",
                Email = "a@b.com",
                Password = System.Text.Encoding.UTF8.GetBytes("dummyPassword"),
                HashKey = new byte[] { 1, 2, 3, 4 } 
            });
            await _context.SaveChangesAsync();


            var ex = Assert.ThrowsAsync<Exception>(async () => await _service.AddDoctor(dto));
            Assert.That(ex.Message, Does.Contain("already exists"));
        }

        [Test]
        public async Task AddDoctor_ShouldAddDoctor_WhenUsernameNotExists()
        {
            var dto = new AddDoctorRequestDTO
            {
                Username = "newUser",
                Email = "test@example.com",
                Password = "password",
                RoleID = 1,
                Name = "Dr New",
                ContactNumber = "1234567890",
                SpecializationID = 1,
                QualificationID = 1,
                Experience = 5,
                Designation = "MD",
                statusID = 1
            };

            var newUser = new User { UserName = dto.Username, Email = dto.Email };
            _mockUserRepo.Setup(r => r.Add(It.IsAny<User>())).ReturnsAsync(newUser);

            var newDoctor = new Doctor { DoctorID = 1, Name = dto.Name, UserName = dto.Username };
            _mockMapper.Setup(m => m.Map<Doctor>(dto)).Returns(newDoctor);
            _mockDoctorRepo.Setup(r => r.Add(It.IsAny<Doctor>())).ReturnsAsync(newDoctor);
            _mockUserRepo.Setup(r => r.Update(dto.Username, It.IsAny<User>())).ReturnsAsync(newUser);

            var result = await _service.AddDoctor(dto);

            Assert.IsNotNull(result);
            Assert.AreEqual(dto.Name, result.Name);
            Assert.AreEqual(dto.Username, result.UserName);
        }

        [Test]
        public async Task UpdateDoctor_ShouldUpdateFields_WhenDoctorExists()
        {
            var doctorId = 1;
            var existingDoctor = new Doctor
            {
                DoctorID = doctorId,
                Name = "Old Name",
                SpecializationID = 1,
                QualificationID = 1,
                Experience = 3,
                Designation = "Doc",
                ContactNumber = "111"
            };
            var specialization = new SpecializationMaster { SpecializationID = 2, SpecializationName = "Spec2" };
            var qualification = new QualificationMaster { QualificationID = 2, QualificationName = "Qual2" };

            _mockDoctorRepo.Setup(r => r.GetById(doctorId)).ReturnsAsync(existingDoctor);
            _mockSpecializationRepo.Setup(r => r.GetById(2)).ReturnsAsync(specialization);
            _mockQualificationRepo.Setup(r => r.GetById(2)).ReturnsAsync(qualification);
            _mockDoctorRepo.Setup(r => r.Update(doctorId, It.IsAny<Doctor>())).ReturnsAsync((int id, Doctor d) => d);

            var updateDto = new UpdateDoctorRequestDTO
            {
                Name = "New Name",
                SpecializationID = 2,
                QualificationID = 2,
                Experience = 5,
                Designation = "New Designation",
                ContactNumber = "999"
            };

            var result = await _service.UpdateDoctor(doctorId, updateDto);

            Assert.AreEqual(updateDto.Name, result.Name);
            Assert.AreEqual(updateDto.SpecializationID, result.SpecializationID);
            Assert.AreEqual(updateDto.QualificationID, result.QualificationID);
            Assert.AreEqual(updateDto.Experience, result.Experience);
            Assert.AreEqual(updateDto.Designation, result.Designation);
            Assert.AreEqual(updateDto.ContactNumber, result.ContactNumber);
        }

        [Test]
        public void UpdateDoctor_ShouldThrowException_WhenDoctorNotFound()
        {
            _mockDoctorRepo.Setup(r => r.GetById(It.IsAny<int>())).ReturnsAsync((Doctor?)null);
            var updateDto = new UpdateDoctorRequestDTO { Name = "New Name" };

            Assert.ThrowsAsync<NoSuchEntityException>(async () => await _service.UpdateDoctor(999, updateDto));
        }

        [Test]
        public async Task DeleteDoctor_ShouldReturnTrue_WhenDoctorExists()
        {
            var doctorId = 1;
            var doctor = new Doctor { DoctorID = doctorId };
            _mockDoctorRepo.Setup(r => r.GetById(doctorId)).ReturnsAsync(doctor);
            _mockDoctorRepo.Setup(r => r.Delete(doctorId)).ReturnsAsync(doctor);

            var result = await _service.DeleteDoctor(doctorId);

            Assert.IsTrue(result);
        }

        [Test]
        public void DeleteDoctor_ShouldThrowException_WhenDoctorNotFound()
        {
            _mockDoctorRepo.Setup(r => r.GetById(It.IsAny<int>())).ReturnsAsync((Doctor?)null);

            Assert.ThrowsAsync<NoSuchEntityException>(async () => await _service.DeleteDoctor(999));
        }
        [Test]
        public async Task GetAllDoctors_ShouldReturnAllDoctors()
        {
            var doctorsList = new List<Doctor>
            {
                new Doctor { DoctorID = 1, Name = "Dr. A" },
                new Doctor { DoctorID = 2, Name = "Dr. B" }
            };

            _mockDoctorRepo.Setup(r => r.GetAll()).ReturnsAsync(doctorsList);

            var result = await _service.GetAllDoctors();

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("Dr. A", result.ElementAt(0).Name);
            Assert.AreEqual("Dr. B", result.ElementAt(1).Name);
        }
        [Test]
        public async Task GetDataForAddingDoctors_ShouldReturnSpecializationsAndQualifications()
        {
            var specializations = new List<SpecializationMaster>
            {
                new() { SpecializationID = 1, SpecializationName = "Spec1" },
                new() { SpecializationID = 2, SpecializationName = "Spec2" }
            };
            var qualifications = new List<QualificationMaster>
            {
                new() { QualificationID = 1, QualificationName = "Qual1" },
                new() { QualificationID = 2, QualificationName = "Qual2" }
            };

            _mockSpecializationRepo.Setup(r => r.GetAll()).ReturnsAsync(specializations);
            _mockQualificationRepo.Setup(r => r.GetAll()).ReturnsAsync(qualifications);

            var result = await _service.GetDataForAddingDoctors();

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Specializations.Count);
            Assert.AreEqual(2, result.Qualifications.Count);
        }
        [Test]
        public async Task GetDoctorsBySpecialization_ShouldReturnDoctorsWithGivenSpecialization()
        {
            var doctors = new List<Doctor>
            {
                new() { DoctorID = 1, Name = "Doc1", SpecializationID = 1 },
                new() { DoctorID = 2, Name = "Doc2", SpecializationID = 2 },
                new() { DoctorID = 3, Name = "Doc3", SpecializationID = 1 }
            };

            _mockDoctorRepo.Setup(r => r.GetAll()).ReturnsAsync(doctors);

            var result = await _service.GetDoctorsBySpecialization(1);

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(d => d.SpecializationID == 1));
        }
       


        [Test]
        public async Task GetDoctorsByStatusAsync_ShouldReturnMappedDoctors()
        {
            var mockConcreteRepo = new Mock<DoctorRepositoryDB>(_context) { CallBase = true };

            var doctors = new List<Doctor>
            {
                new() { DoctorID = 1, Name = "Doc1", StatusID = 1 },
                new() { DoctorID = 2, Name = "Doc2", StatusID = 1 }
            };

            mockConcreteRepo.Setup(r => r.GetDoctorsByStatusAsync(1)).ReturnsAsync(doctors);

            _mockMapper.Setup(m => m.Map<IEnumerable<DoctorSearchResponseDTO>>(It.IsAny<IEnumerable<Doctor>>()))
                .Returns((IEnumerable<Doctor> docs) =>
                {
                    var list = new List<DoctorSearchResponseDTO>();
                    foreach (var d in docs)
                    {
                        list.Add(new DoctorSearchResponseDTO { Name = d.Name });
                    }
                    return list;
                });

            var serviceWithConcreteRepo = new DoctorService(
                mockConcreteRepo.Object,
                _mockSpecializationRepo.Object,
                _mockQualificationRepo.Object,
                _mockUserRepo.Object,
                _mockMapper.Object,
                _context);

            var result = await serviceWithConcreteRepo.GetDoctorsByStatusAsync(1);

            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(result.All(d => d.Name.StartsWith("Doc")));
        }

        [Test]
        public async Task SearchDoctors_ShouldFilterByAllCriteriaAndSort()
        {
            var doctors = new List<Doctor>
            {
                new Doctor { DoctorID = 1, Name = "Alice", SpecializationID = 1, QualificationID = 1, Experience = 5 },
                new Doctor { DoctorID = 2, Name = "Bob", SpecializationID = 2, QualificationID = 1, Experience = 3 },
                new Doctor { DoctorID = 3, Name = "Charlie", SpecializationID = 1, QualificationID = 2, Experience = 7 },
                new Doctor { DoctorID = 4, Name = "Alina", SpecializationID = 1, QualificationID = 1, Experience = 10 }
            };

            _mockDoctorRepo.Setup(r => r.GetAll()).ReturnsAsync(doctors);

            _mockSpecializationRepo.Setup(r => r.GetById(It.IsAny<int>()))
                .ReturnsAsync((int id) => new SpecializationMaster { SpecializationID = id, SpecializationName = $"Spec{id}" });

            _mockQualificationRepo.Setup(r => r.GetById(It.IsAny<int>()))
                .ReturnsAsync((int id) => new QualificationMaster { QualificationID = id, QualificationName = $"Qual{id}" });

            _mockMapper.Setup(m => m.Map<DoctorSearchResponseDTO>(It.IsAny<Doctor>()))
                .Returns<Doctor>(d => new DoctorSearchResponseDTO { Name = d.Name });

            var request = new DoctorSearchRequestDTO
            {
                Name = "a", 
                SpecializationIds = new List<int> { 1 },
                QualificationIds = new List<int> { 1 },
                ExperienceRange = new SearchRange<int> { MinValue = 4, MaxValue = 10 },
                Sort = 1, 
                PageNumber = 1,
                PageSize = 10
            };

            var result = await _service.SearchDoctors(request);

            Assert.IsTrue(result.Doctors.All(d => d.Name.ToLower().Contains("a")));
            Assert.IsTrue(result.Doctors.All(d => d.Name == "Alice" || d.Name == "Alina"));
            Assert.AreEqual(2, result.Doctors.Count);
            Assert.AreEqual("Alice", result.Doctors.First().Name);
        }
        [Test]
        public async Task SearchDoctors_ShouldSortCorrectly_BasedOnSortParameter()
        {
            var doctors = new List<Doctor>
            {
                new() { DoctorID = 1, Name = "Charlie", SpecializationID = 2, QualificationID = 1, Experience = 5 },
                new() { DoctorID = 2, Name = "Alice", SpecializationID = 1, QualificationID = 1, Experience = 10 },
                new() { DoctorID = 3, Name = "Bob", SpecializationID = 3, QualificationID = 2, Experience = 7 },
            };

            _mockDoctorRepo.Setup(r => r.GetAll()).ReturnsAsync(doctors);

            _mockSpecializationRepo.Setup(r => r.GetById(It.IsAny<int>())).ReturnsAsync((int id) =>
                new SpecializationMaster { SpecializationID = id, SpecializationName = $"Spec{id}" });

            _mockQualificationRepo.Setup(r => r.GetById(It.IsAny<int>())).ReturnsAsync((int id) =>
                new QualificationMaster { QualificationID = id, QualificationName = $"Qual{id}" });

            _mockMapper.Setup(m => m.Map<DoctorSearchResponseDTO>(It.IsAny<Doctor>()))
                .Returns<Doctor>(d => new DoctorSearchResponseDTO { Name = d.Name });
            var sortOptions = new[] { 1, -1, 2, -2, 3, -3, 0 };

            foreach (var sort in sortOptions)
            {
                var request = new DoctorSearchRequestDTO
                {
                    Sort = sort,
                    PageNumber = 1,
                    PageSize = 10
                };

                var result = await _service.SearchDoctors(request);

                var sortedNames = result.Doctors.Select(d => d.Name).ToList();

                switch (sort)
                {
                    case 1:
                        CollectionAssert.AreEqual(new[] { "Alice", "Bob", "Charlie" }, sortedNames);
                        break;
                    case -1:
                        CollectionAssert.AreEqual(new[] { "Charlie", "Bob", "Alice" }, sortedNames);
                        break;
                    case 2:
                        CollectionAssert.AreEqual(new[] { "Charlie", "Bob", "Alice" }, sortedNames); 
                        break;
                    case -2:
                        CollectionAssert.AreEqual(new[] { "Alice", "Bob", "Charlie" }, sortedNames); 
                        break;
                    case 3:
                        CollectionAssert.AreEqual(new[] { "Alice", "Charlie", "Bob" }, sortedNames); 
                        break;
                    case -3:
                        CollectionAssert.AreEqual(new[] { "Bob", "Charlie", "Alice" }, sortedNames); 
                        break;
                    case 0:
                        CollectionAssert.AreEquivalent(new[] { "Charlie", "Alice", "Bob" }, sortedNames); 
                        break;
                }
            }
        }
        [Test]
        public void SearchDoctors_ShouldThrowNotFoundException_WhenNoDoctorsFound()
        {
            _mockDoctorRepo.Setup(r => r.GetAll()).ReturnsAsync((List<Doctor>?)null);

            var request = new DoctorSearchRequestDTO
            {
                PageNumber = 1,
                PageSize = 10
            };

            Assert.ThrowsAsync<NotFoundException>(async () => await _service.SearchDoctors(request));
        }

      
    }
}
