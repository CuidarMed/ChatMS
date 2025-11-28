using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Configuration
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<User> Users { get; set; }
        public DbSet<ChatRoom> ChatRooms { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //additional model settings

            // User Configuration - Mapear a la tabla Users compartida con UserId como PK
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Id).HasColumnName("UserId");
                // No crear la tabla, solo mapear a la existente
            });

            // ChatRoom Configuration
            modelBuilder.Entity<ChatRoom>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.AppointmentId).IsRequired(); // AppointmentId es requerido (no nullable)

                entity.HasOne(e => e.Doctor)
                    .WithMany()
                    .HasForeignKey(e => e.DoctorID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Patient)
                    .WithMany()
                    .HasForeignKey(e => e.PatientId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Índice único para DoctorID, PatientId y AppointmentId
                // Solo aplicar unicidad cuando AppointmentId no es NULL
                entity.HasIndex(e => new { e.DoctorID, e.PatientId, e.AppointmentId })
                    .IsUnique()
                    .HasFilter("[AppointmentId] IS NOT NULL");
            });

            // ChatMessage Configuration
            modelBuilder.Entity<ChatMessage>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.Property(e => e.Message).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.SendAt).IsRequired();
                entity.Property(e => e.IsRead).HasDefaultValue(false);

                entity.HasOne(e => e.ChatRoom)
                    .WithMany(c => c.Messages)
                    .HasForeignKey(e => e.ChatRoomId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Sender)
                    .WithMany()
                    .HasForeignKey(e => e.SenderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.ChatRoomId);
                entity.HasIndex(e => e.SendAt);
            });
        }
    }
}
