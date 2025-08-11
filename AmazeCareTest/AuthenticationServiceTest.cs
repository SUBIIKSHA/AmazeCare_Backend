using AmazeCareAPI.Exceptions;
using AmazeCareAPI.Interfaces;
using AmazeCareAPI.Models;
using AmazeCareAPI.Models.DTOs;
using AmazeCareAPI.Services;
using AutoMapper;
using Moq;
using NUnit.Framework;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AmazeCareAPI.Tests
{
    [TestFixture]
    public class AuthenticationServiceTest
    {
        private Mock<IRepository<int, Doctor>> _doctorRepository;
        private Mock<IRepository<int, Patient>> _patientRepository;
        private Mock<IRepository<string, User>> _userRepository;
        private Mock<ITokenService> _tokenService;
        private Mock<IMapper> _mapper;
        private AuthenticationService _authenticationService;

        [SetUp]
        public void Setup()
        {
            _doctorRepository = new Mock<IRepository<int, Doctor>>();
            _patientRepository = new Mock<IRepository<int, Patient>>();
            _userRepository = new Mock<IRepository<string, User>>();
            _tokenService = new Mock<ITokenService>();
            _mapper = new Mock<IMapper>();

            _authenticationService = new AuthenticationService(
                _doctorRepository.Object,
                _patientRepository.Object,
                _userRepository.Object,
                _tokenService.Object,
                _mapper.Object
            );
        }

        [TestCase("doctor1", "doc@example.com", "pass123", 1)]
        public async Task RegisterDoctor_ShouldReturnSuccess(string username, string email, string password, int roleId)
        {
            var doctorDto = new AddDoctorRequestDTO { Username = username, Email = email, Password = password, RoleID = roleId };
            var doctor = new Doctor { DoctorID = 1, UserName = username };

            _mapper.Setup(m => m.Map<Doctor>(It.IsAny<AddDoctorRequestDTO>())).Returns(doctor);
            _userRepository.Setup(r => r.Add(It.IsAny<User>())).ReturnsAsync(new User { UserName = username });
            _doctorRepository.Setup(r => r.Add(It.IsAny<Doctor>())).ReturnsAsync(doctor);
            _userRepository.Setup(r => r.Update(It.IsAny<string>(), It.IsAny<User>())).ReturnsAsync(new User());

            var result = await _authenticationService.RegisterDoctor(doctorDto);

            Assert.That(result.DoctorID, Is.EqualTo(1));
            Assert.That(result.Username, Is.EqualTo(username));
            Assert.That(result.Message, Is.EqualTo("Doctor registered successfully"));
        }

        [TestCase("patient1", "pat@example.com", "pass123", 2)]
        public async Task RegisterPatient_ShouldReturnSuccess(string username, string email, string password, int roleId)
        {
            var patientDto = new AddPatientRequestDTO { Username = username, Email = email, Password = password, RoleID = roleId, DOB = DateTime.UtcNow };
            var patient = new Patient { PatientID = 1, UserName = username, DOB = patientDto.DOB };

            _mapper.Setup(m => m.Map<Patient>(It.IsAny<AddPatientRequestDTO>())).Returns(patient);
            _userRepository.Setup(r => r.Add(It.IsAny<User>())).ReturnsAsync(new User { UserName = username });
            _patientRepository.Setup(r => r.Add(It.IsAny<Patient>())).ReturnsAsync(patient);
            _userRepository.Setup(r => r.Update(It.IsAny<string>(), It.IsAny<User>())).ReturnsAsync(new User());

            var result = await _authenticationService.RegisterPatient(patientDto);

            Assert.That(result.PatientID, Is.EqualTo(1));
            Assert.That(result.Username, Is.EqualTo(username));
            Assert.That(result.Message, Is.EqualTo("Patient registered successfully"));
        }

        [TestCase("testuser", "password")]
        public async Task Login_ShouldReturnToken_WhenCredentialsAreValid(string username, string password)
        {
            var hashKey = Encoding.UTF8.GetBytes("key");
            using var hmac = new HMACSHA256(hashKey);
            var passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

            var user = new User
            {
                UserName = username,
                HashKey = hashKey,
                Password = passwordHash,
                Role = new RoleMaster { RoleName = "Doctor" }
            };

            _userRepository.Setup(r => r.GetById(username)).ReturnsAsync(user);
            _tokenService.Setup(t => t.GenerateToken(It.IsAny<TokenUser>())).ReturnsAsync("token123");

            var result = await _authenticationService.Login(new LoginRequestDTO { Username = username, Password = password });

            Assert.That(result.Username, Is.EqualTo(username));
            Assert.That(result.Role, Is.EqualTo("Doctor"));
            Assert.That(result.Token, Is.EqualTo("token123"));
        }

        [TestCase("wronguser", "password")]
        public void Login_ShouldThrowException_WhenUserNotFound(string username, string password)
        {
            _userRepository.Setup(r => r.GetById(username)).ReturnsAsync((User)null);

            Assert.ThrowsAsync<NoSuchEntityException>(async () =>
                await _authenticationService.Login(new LoginRequestDTO { Username = username, Password = password }));
        }
    }
}
