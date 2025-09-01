using AutoMapper;
using AmazeCareAPI.Models;
using AmazeCareAPI.Models.DTOs;

namespace AmazeCareAPI.Mappers
{
    public class AmazeCareMapperProfile : Profile
    {
        public AmazeCareMapperProfile()
        {

            CreateMap<AddDoctorRequestDTO, Doctor>();
            CreateMap<Doctor, AddDoctorResponseDTO>();
            CreateMap<Doctor, DoctorSearchResponseDTO>();
            CreateMap<UpdateDoctorRequestDTO, Doctor>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));


            CreateMap<AddPatientRequestDTO, Patient>()
                 .ForMember(dest => dest.GenderID, opt => opt.MapFrom(src => src.GenderID));  

            CreateMap<UpdatePatientRequestDTO, Patient>()
                .ForMember(dest => dest.GenderID, opt => opt.MapFrom(src => src.GenderID));  


            CreateMap<Patient, AddPatientResponseDTO>();

            CreateMap<Patient, PatientSearchResponseDTO>()
                .ForMember(dest => dest.GenderName, opt => opt.Ignore()); 

            CreateMap<LoginResponseDTO, User>();
            CreateMap<User, LoginResponseDTO>();

            CreateMap<AppointmentRequestDTO, Appointment>();
            CreateMap<Appointment, AppointmentResponseDTO>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.StatusName))
            .ForMember(dest => dest.DoctorName, opt => opt.MapFrom(src => src.Doctor.Name))
            .ForMember(dest => dest.PatientName, opt => opt.MapFrom(src => src.Patient.FullName));

            CreateMap<Appointment, AppointmentSearchResponseDTO>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.StatusName))
            .ForMember(dest => dest.DoctorName, opt => opt.MapFrom(src => src.Doctor.Name))
            .ForMember(dest => dest.PatientName, opt => opt.MapFrom(src => src.Patient.FullName));


            CreateMap<PrescriptionCreateDTO, Prescription>();

            CreateMap<RecommendedTestCreateDTO, RecommendedTest>();
            CreateMap<RecommendedTest, RecommendedTestResponseDTO>()
                .ForMember(dest => dest.TestName, opt => opt.MapFrom(src => src.Test.TestName));

            CreateMap<CreateMedicalRecordDTO, MedicalRecord>();
            CreateMap<UpdateMedicalRecordDTO, MedicalRecord>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<MedicalRecord, MedicalRecordDTO>();

            CreateMap<Prescription, PrescriptionResponseDTO>()
                .ForMember(dest => dest.MedicineName, opt => opt.MapFrom(src => src.Medicine.MedicineName))
                .ForMember(dest => dest.PatternCode, opt => opt.MapFrom(src => src.DosagePattern.PatternCode))
                .ForMember(dest => dest.DosageTiming, opt => opt.MapFrom(src => src.DosagePattern.Timing));

            CreateMap<Billing, BillingResponseDTO>()
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.BillingStatus.StatusName));

            CreateMap<BillingCreateDTO, Billing>()
            .ForMember(dest => dest.TotalAmount, opt => opt.Ignore());
            CreateMap<UpdateMedicalRecordDTO, MedicalRecord>();

        }
    }
}
