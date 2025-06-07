using Microsoft.EntityFrameworkCore;
using EmployeeScheduling.API.Models;

namespace EmployeeScheduling.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSets for each model
        public DbSet<User> Users { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Qualification> Qualifications { get; set; }
        public DbSet<EmployeeQualification> EmployeeQualifications { get; set; }
        public DbSet<ShiftQualification> ShiftQualifications { get; set; }
        public DbSet<Availability> Availabilities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships and constraints

            // User relationships
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Employee relationships
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.User)
                .WithMany(u => u.Employees)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Employee>()
                .HasIndex(e => e.EmployeeNumber)
                .IsUnique();

            // Schedule relationships
            modelBuilder.Entity<Schedule>()
                .HasOne(s => s.CreatedByUser)
                .WithMany()
                .HasForeignKey(s => s.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Shift relationships
            modelBuilder.Entity<Shift>()
                .HasOne(s => s.Schedule)
                .WithMany(sc => sc.Shifts)
                .HasForeignKey(s => s.ScheduleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Shift>()
                .HasOne(s => s.Location)
                .WithMany(l => l.Shifts)
                .HasForeignKey(s => s.LocationId)
                .OnDelete(DeleteBehavior.SetNull);

            // Assignment relationships
            modelBuilder.Entity<Assignment>()
                .HasOne(a => a.Shift)
                .WithMany(s => s.Assignments)
                .HasForeignKey(a => a.ShiftId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Assignment>()
                .HasOne(a => a.Employee)
                .WithMany(e => e.Assignments)
                .HasForeignKey(a => a.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Assignment>()
                .HasOne(a => a.AssignedByUser)
                .WithMany()
                .HasForeignKey(a => a.AssignedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // EmployeeQualification relationships
            modelBuilder.Entity<EmployeeQualification>()
                .HasOne(eq => eq.Employee)
                .WithMany(e => e.EmployeeQualifications)
                .HasForeignKey(eq => eq.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EmployeeQualification>()
                .HasOne(eq => eq.Qualification)
                .WithMany(q => q.EmployeeQualifications)
                .HasForeignKey(eq => eq.QualificationId)
                .OnDelete(DeleteBehavior.Cascade);

            // ShiftQualification relationships
            modelBuilder.Entity<ShiftQualification>()
                .HasOne(sq => sq.Shift)
                .WithMany(s => s.ShiftQualifications)
                .HasForeignKey(sq => sq.ShiftId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ShiftQualification>()
                .HasOne(sq => sq.Qualification)
                .WithMany(q => q.ShiftQualifications)
                .HasForeignKey(sq => sq.QualificationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Availability relationships
            modelBuilder.Entity<Availability>()
                .HasOne(a => a.Employee)
                .WithMany(e => e.Availabilities)
                .HasForeignKey(a => a.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed some initial data
            SeedData(modelBuilder);
            // Add this line in the OnModelCreating method, after the existing configurations
            modelBuilder.Entity<Employee>()
                .Property(e => e.HourlyRate)
                .HasPrecision(18, 2); // 18 total digits, 2 decimal places

        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed default admin user
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = 1,
                    FirstName = "Admin",
                    LastName = "User",
                    Email = "admin@example.com",
                    PasswordHash = "$2a$11$8ZpGGXeQcab7EXhYmBZZOeJ8QJ8VmMZqQQJ8QJ8VmMZqQQJ8QJ8Vm", // "Admin123!"
                    Role = "Admin",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            );

            // Seed default locations
            modelBuilder.Entity<Location>().HasData(
                new Location { LocationId = 1, Name = "Main Office", Address = "123 Main St", City = "Anytown", State = "ST", ZipCode = "12345", IsActive = true },
                new Location { LocationId = 2, Name = "Warehouse", Address = "456 Industrial Blvd", City = "Anytown", State = "ST", ZipCode = "12346", IsActive = true }
            );

            // Seed default qualifications
            modelBuilder.Entity<Qualification>().HasData(
                new Qualification { QualificationId = 1, Name = "Basic Training", Description = "Basic employee training", IsActive = true },
                new Qualification { QualificationId = 2, Name = "Forklift Operator", Description = "Certified forklift operator", IsActive = true },
                new Qualification { QualificationId = 3, Name = "Safety Certified", Description = "Safety training certification", IsActive = true }
            );
        }
    }
}
