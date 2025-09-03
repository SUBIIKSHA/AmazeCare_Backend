using AmazeCareAPI.Contexts;
using AmazeCareAPI.Interfaces;
using AmazeCareAPI.Mappers;
using AmazeCareAPI.Models;
using AmazeCareAPI.Repositories;
using AmazeCareAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace AmazeCareAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers()
                  .AddJsonOptions(options =>
                  {
                      options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                      options.JsonSerializerOptions.WriteIndented = true;
                  });



            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme."
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] { }
                    }
                });
            });

            builder.Services.AddCors(options => options.AddPolicy("DefaultCORS", policy =>
            {
                policy.WithOrigins("http://localhost:3000")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
        }));
        

            builder.Services.AddAutoMapper(typeof(AmazeCareMapperProfile));

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("defaultConnection"));
            });

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration["Tokens:JWT"])
                        )
                    };
                });

            builder.Services.AddScoped<IRepository<int, Doctor>, DoctorRepositoryDB>();
            builder.Services.AddScoped<IRepository<int, Patient>, PatientRepositoryDB>();
            builder.Services.AddScoped<IRepository<string, User>, UserRepository>();
            builder.Services.AddScoped<IRepository<int, Appointment>, AppointmentRepositoryDB>();
            builder.Services.AddScoped<IRepository<int,Prescription>, PrescriptionRepositoryDB>();
            builder.Services.AddScoped<IRepository<int, RecommendedTest>,RecommendedTestRepositoryDB>();
            builder.Services.AddScoped<IRepository<int, SpecializationMaster>, SpecializationMasterRepository>();
            builder.Services.AddScoped<IRepository<int, QualificationMaster>, QualificationMasterRepository>();
            builder.Services.AddScoped<IRepository<int, GenderMaster>, GenderMasterRepository>();
            builder.Services.AddScoped<IRepository<int, MedicalRecord>, MedicalRecordRepositoryDB>();
            builder.Services.AddScoped<IRepository<int,Billing>,BillingRepositoryDB>();
            builder.Services.AddScoped<RecommendedTestRepositoryDB>();
            builder.Services.AddScoped<BillingRepositoryDB>();


            builder.Services.AddScoped<IAuthenticate, AuthenticationService>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IDoctorService, DoctorService>();
            builder.Services.AddScoped<IPatientService, PatientService>();
            builder.Services.AddScoped<IAppointmentService, AppointmentService>();
            builder.Services.AddScoped<IPrescriptionService, PrescriptionService>();
            builder.Services.AddScoped<IRecommendedTestService, RecommendedTestService>();
            builder.Services.AddScoped<IMedicalRecordService, MedicalRecordService>();
            builder.Services.AddScoped<IBillingService, BillingService>();
           


            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    context.Response.ContentType = "application/json";

                    var error = context.Features.Get<IExceptionHandlerFeature>();
                    if (error != null)
                    {
                        var err = new
                        {
                            message = error.Error.Message
                        };
                        await context.Response.WriteAsJsonAsync(err);
                    }
                });
            });
            app.UseCors("DefaultCORS");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
