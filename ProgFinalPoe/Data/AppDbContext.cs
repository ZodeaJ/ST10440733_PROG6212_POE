using System.Collections.Generic;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;
using ProgFinalPoe.Models;

namespace ProgFinalPoe.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Lecturer> Lecturers { get; set; }
        public DbSet<Claim> Claims { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Coordinator> Coordinators { get; set; }
        public DbSet<Manager> Managers { get; set; }
        public DbSet<HR> HRs { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Claim>()
                .Property(c => c.HourlyRate)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Invoice>()
                .Property(i => i.Amount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<User>()
                .Property(u => u.HourlyRate)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Claim)
                .WithMany()
                .HasForeignKey(i => i.ClaimId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Lecturer)
                .WithMany()
                .HasForeignKey(i => i.LecturerId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Lecturer)
                .WithMany()
                .HasForeignKey(u => u.LecturerId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);


            //Set Data to be able to log in
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = 1,
                    Username = "SK",
                    Password = "HR123",
                    Role = "HR",
                    Name = "Shinobu",
                    Surname = "Kocho",
                    Email = "sk@gmail.com",
                    HourlyRate = 0,
                    Department = "Administration",
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1)
                },
               new User
               {
                   UserId = 2,
                   Username = "GT",
                   Password = "lecturer123",
                   Role = "Lecturer",
                   Name = "Giyu",
                   Surname = "Tomioka",
                   Email = "gt@gmail.com",
                   HourlyRate = 250,
                   Department = "Computer Science",
                   IsActive = true,
                   CreatedAt = new DateTime(2024, 1, 1)
               },
               new User
               {
                   UserId = 3,
                   Username = "TK",
                   Password = "coordinator123",
                   Role = "Coordinator",
                   Name = "Tanjiro",
                   Surname = "Kamado",
                   Email = "tk@gamil.com",
                   HourlyRate = 0,
                   Department = "Administration",
                   IsActive = true,
                   CreatedAt = new DateTime(2024, 1, 1)
               },
               new User
               {
                   UserId = 4,
                   Username = "IH",
                   Password = "manager123",
                   Role = "Manager",
                   Name = "Inosuke",
                   Surname = "Hashibira",
                   Email = "ih@gmail.com",
                   HourlyRate = 0,
                   Department = "Administration",
                   IsActive = true,
                   CreatedAt = new DateTime(2024, 1, 1)
               }
           );
        }
    }
}