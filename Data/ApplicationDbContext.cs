using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Medicare.Models;
using Microsoft.AspNetCore.Identity;
using System;

namespace Medicare.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, Role, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Specialization> Specializations { get; set; }
        public DbSet<WorkingHours> WorkingHours { get; set; }
        public DbSet<MedicalRecord> MedicalRecords { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<MedicalDoc> MedicalDocs { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Role>().HasData(
                new Role { Id = "1", Name = "Admin", NormalizedName = "ADMIN", Description = "Administrator" },
                new Role { Id = "2", Name = "Doctor", NormalizedName = "DOCTOR" , Description = "Doctor user" },
                new Role { Id = "3", Name = "Patient", NormalizedName = "PATIENT", Description = "Patient user" }
            );

            // Restrict cascade paths for SQL Server compatibility
            builder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Review>()
                .HasOne(r => r.Patient)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Review>()
                .HasOne(r => r.Doctor)
                .WithMany(d => d.Reviews)
                .HasForeignKey(r => r.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed default Specializations
            builder.Entity<Specialization>().HasData(
                new Specialization { Id = 1, Name = "General Practice", Description = "General health care and family medicine" },
                new Specialization { Id = 2, Name = "Cardiology", Description = "Heart and blood vessels care" },
                new Specialization { Id = 3, Name = "Pediatrics", Description = "Infants, children, and adolescents care" },
                new Specialization { Id = 4, Name = "Dermatology", Description = "Skin, hair, and nails care" },
                new Specialization { Id = 5, Name = "Orthopedics", Description = "Bones, joints, ligaments, tendons, and muscles care" }
            );

            // Seed default Admin User
            var adminUser = new User
            {
                Id = "admin-user-id",
                UserName = "admin@medicare.com",
                NormalizedUserName = "ADMIN@MEDICARE.COM",
                Email = "admin@medicare.com",
                NormalizedEmail = "ADMIN@MEDICARE.COM",
                EmailConfirmed = true,
                FirstName = "Admin",
                LastName = "User",
                CreatedDate = new DateTime(2026, 6, 18),
                SecurityStamp = "d7d1b3db-8378-43fc-8377-a87f7bd99aa8",
                ConcurrencyStamp = "a72cb3db-8378-43fc-8377-a87f7bd99ab1"
            };

            var passwordHasher = new PasswordHasher<User>();
            adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "Admin@123456");

            builder.Entity<User>().HasData(adminUser);

            // Assign Admin role to Admin user
            builder.Entity<IdentityUserRole<string>>().HasData(new IdentityUserRole<string>
            {
                RoleId = "1",
                UserId = "admin-user-id"
            });
        }
    }
}
