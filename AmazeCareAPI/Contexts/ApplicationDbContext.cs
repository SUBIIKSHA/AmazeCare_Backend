using AmazeCareAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AmazeCareAPI.Contexts
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<RoleMaster> Roles { get; set; }
        public DbSet<SpecializationMaster> Specializations { get; set; }
        public DbSet<QualificationMaster> Qualifications { get; set; }
        public DbSet<AppointmentStatusMaster> AppointmentStatuses { get; set; }
        public DbSet<BillingStatusMaster> BillingStatuses { get; set; }
        public DbSet<TestMaster> Tests { get; set; }
        public DbSet<MedicineMaster> Medicines { get; set; }
        public DbSet<DosagePatternMaster> DosagePatterns { get; set; }
        public DbSet<GenderMaster> Genders { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Billing> Billings { get; set; }
        public DbSet<MedicalRecord> MedicalRecords { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<RecommendedTest> RecommendedTests { get; set; }
        public DbSet<DoctorStatusMaster> DoctorStatuses { get; set; }
        public DbSet<PatientStatusMaster> PatientStatuses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var hmac = new System.Security.Cryptography.HMACSHA256();
            var password = "#13admin@0803";
            var passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            var hashKey = hmac.Key;

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserName = "admin",
                    Email = "admin@amazecare.com",
                    Password = passwordHash,
                    HashKey = hashKey,
                    RoleID = 1,
                    CreatedAt = DateTime.Now
                }
            );

            modelBuilder.Entity<User>()
                .HasKey(u => u.UserName)
                .HasName("PK_User_Username");

            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleID)
                .HasConstraintName("FK_User_Role")
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Doctor>()
                .HasKey(d => d.DoctorID)
                .HasName("PK_DoctorID");

            modelBuilder.Entity<Doctor>()
                .HasOne(d => d.User)
                .WithOne(u => u.Doctor)
                .HasForeignKey<Doctor>(d => d.UserName)
                .HasConstraintName("FK_Doctor_User")
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Doctor>()
                .HasOne(d => d.Specialization)
                .WithMany(s => s.Doctors)
                .HasForeignKey(d => d.SpecializationID)
                .HasConstraintName("FK_Doctor_Specialization")
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Doctor>()
                .HasOne(d => d.Qualification)
                .WithMany(q => q.Doctors)
                .HasForeignKey(d => d.QualificationID)
                .HasConstraintName("FK_Doctor_Qualification")
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Doctor>()
                .HasOne(d => d.Status)
                .WithMany(s => s.Doctors)
                .HasForeignKey(d => d.StatusID)
                .HasConstraintName("FK_Doctor_Status")
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Patient>()
                .HasKey(p => p.PatientID)
                .HasName("PK_PatientID");

            modelBuilder.Entity<Patient>()
                .HasOne(p => p.User)
                .WithOne(u => u.Patient)
                .HasForeignKey<Patient>(p => p.UserName)
                .HasConstraintName("FK_Patient_User")
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Patient>()
                .HasOne(p => p.Status)
                .WithMany(s => s.Patients)
                .HasForeignKey(p => p.StatusID)
                .HasConstraintName("FK_Patient_Status")
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasKey(a => a.AppointmentID)
                .HasName("PK_AppointmentID");

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DoctorID)
                .HasConstraintName("FK_Appointment_Doctor")
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientID)
                .HasConstraintName("FK_Appointment_Patient")
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Billing>()
                 .HasKey(b => b.BillingID)
                 .HasName("PK_Billing");

            modelBuilder.Entity<Billing>()
                .HasOne(b => b.Appointment)
                .WithOne(a => a.Billing)
                .HasForeignKey<Billing>(b => b.AppointmentID)
                .HasConstraintName("FK_Billing_Appointment")
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Billing>()
                .HasOne(b => b.Patient)
                .WithMany(p => p.Billings)
                .HasForeignKey(b => b.PatientID)
                .HasConstraintName("FK_Billing_Patient")
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Billing>()
                .HasOne(b => b.BillingStatus)
                .WithMany()
                .HasForeignKey(b => b.StatusID)
                .HasPrincipalKey(bs => bs.StatusID)
                .HasConstraintName("FK_Billing_BillingStatus")
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<MedicalRecord>()
                .HasKey(m => m.RecordID)
                .HasName("PK_MedicalRecord");

            modelBuilder.Entity<MedicalRecord>()
                .HasOne(m => m.Appointment)
                .WithMany()
                .HasForeignKey(m => m.AppointmentID)
                .HasConstraintName("FK_MedicalRecord_Appointment")
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MedicalRecord>()
                .HasOne(m => m.Patient)
                .WithMany(p => p.MedicalRecords)
                .HasForeignKey(m => m.PatientID)
                .HasConstraintName("FK_MedicalRecord_Patient")
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MedicalRecord>()
                .HasOne(m => m.Doctor)
                .WithMany(d => d.MedicalRecords)
                .HasForeignKey(m => m.DoctorID)
                .HasConstraintName("FK_MedicalRecord_Doctor")
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Prescription>()
                .HasKey(p => p.PrescriptionID)
                .HasName("PK_Prescription");

            modelBuilder.Entity<Prescription>()
                .HasOne(p => p.Record)
                .WithMany(r => r.Prescriptions)
                .HasForeignKey(p => p.RecordID)
                .HasConstraintName("FK_Prescription_Record")
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Prescription>()
                .HasOne(p => p.Medicine)
                .WithMany(m => m.Prescriptions)
                .HasForeignKey(p => p.MedicineID)
                .HasConstraintName("FK_Prescription_Medicine")
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Prescription>()
                .HasOne(p => p.DosagePattern)
                .WithMany(d => d.Prescriptions)
                .HasForeignKey(p => p.PatternID)
                .HasConstraintName("FK_Prescription_DosagePattern")
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<RecommendedTest>()
                 .HasKey(rt => rt.RecommendedTestID)
                 .HasName("PK_RecommendedTest");

            modelBuilder.Entity<RecommendedTest>()
                 .HasOne(rt => rt.Test)
                 .WithMany()
                 .HasForeignKey(rt => rt.TestID)
                 .HasPrincipalKey(tm => tm.TestID) 
                 .HasConstraintName("FK_RecommendedTest_Test")
                 .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<RecommendedTest>()
                .HasOne(rt => rt.Prescription)
                .WithMany(p => p.RecommendedTests)
                .HasForeignKey(rt => rt.PrescriptionID)
                .HasConstraintName("FK_RecommendedTest_Prescription")
                .OnDelete(DeleteBehavior.Cascade);



            modelBuilder.Entity<AppointmentStatusMaster>().HasKey(a => a.StatusID).HasName("PK_AppointmentStatus_StatusID");
            modelBuilder.Entity<BillingStatusMaster>().HasKey(b => b.StatusID).HasName("PK_BillingStatusMaster_StatusID");
            modelBuilder.Entity<QualificationMaster>().HasKey(q => q.QualificationID).HasName("PK_QualificationMaster");
            modelBuilder.Entity<SpecializationMaster>().HasKey(s => s.SpecializationID).HasName("PK_SpecializationMaster");
            modelBuilder.Entity<TestMaster>().HasKey(t => t.TestID).HasName("PK_TestMaster");
            modelBuilder.Entity<MedicineMaster>().HasKey(m => m.MedicineID).HasName("PK_MedicineMaster");
            modelBuilder.Entity<DosagePatternMaster>().HasKey(d => d.PatternID).HasName("PK_DosagePatternMaster");
            modelBuilder.Entity<GenderMaster>().HasKey(g => g.GenderID).HasName("PK_GenderMaster");
            modelBuilder.Entity<RoleMaster>().HasKey(r => r.RoleID).HasName("PK_RoleMaster");
            modelBuilder.Entity<DoctorStatusMaster>().HasKey(d => d.StatusID).HasName("PK_DoctorStatus");
            modelBuilder.Entity<PatientStatusMaster>().HasKey(p => p.StatusID).HasName("PK_PatientStatus");

            modelBuilder.Entity<RoleMaster>().HasData(
                new RoleMaster { RoleID = 1, RoleName = "Admin" },
                new RoleMaster { RoleID = 2, RoleName = "Doctor" },
                new RoleMaster { RoleID = 3, RoleName = "Patient" }
            );

            modelBuilder.Entity<AppointmentStatusMaster>().HasData(
                new AppointmentStatusMaster { StatusID = 1, StatusName = "Pending" },
                new AppointmentStatusMaster { StatusID = 2, StatusName = "Scheduled" },
                new AppointmentStatusMaster { StatusID = 3, StatusName = "Completed" },
                new AppointmentStatusMaster { StatusID = 4, StatusName = "Cancelled" },
                new AppointmentStatusMaster { StatusID = 5, StatusName = "Rejected" }
            );

            modelBuilder.Entity<GenderMaster>().HasData(
                new GenderMaster { GenderID = 1, GenderName = "Male" },
                new GenderMaster { GenderID = 2, GenderName = "Female" },
                new GenderMaster { GenderID = 3, GenderName = "Other" }
            );

            modelBuilder.Entity<DosagePatternMaster>().HasData(
                new DosagePatternMaster { PatternID = 1, PatternCode = "1-0-0", Timing = "BF" },
                new DosagePatternMaster { PatternID = 2, PatternCode = "0-1-0", Timing = "AF" }, 
                new DosagePatternMaster { PatternID = 3, PatternCode = "0-0-1", Timing = "BF" }, 
                new DosagePatternMaster { PatternID = 4, PatternCode = "1-1-0", Timing = "AF" },
                new DosagePatternMaster { PatternID = 5, PatternCode = "1-1-1", Timing = "AF" }, 
                new DosagePatternMaster { PatternID = 6, PatternCode = "0-1-1", Timing = "BF" }  
            );

            modelBuilder.Entity<BillingStatusMaster>().HasData(
                new BillingStatusMaster { StatusID = 1, StatusName = "Pending" },
                new BillingStatusMaster { StatusID = 2, StatusName = "Paid" },
                new BillingStatusMaster { StatusID = 3, StatusName = "Cancelled" },
                new BillingStatusMaster { StatusID = 4, StatusName = "Refunded" }
            );

            modelBuilder.Entity<DoctorStatusMaster>().HasData(
                new DoctorStatusMaster { StatusID = 1, StatusName = "Active" },
                new DoctorStatusMaster { StatusID = 2, StatusName = "Inactive" }
            );

            modelBuilder.Entity<PatientStatusMaster>().HasData(
                new PatientStatusMaster { StatusID = 1, StatusName = "Active" },
                new PatientStatusMaster { StatusID = 2, StatusName = "Deactivated" }
            );


        }
    }
}
