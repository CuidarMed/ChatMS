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
        //public DbSet<User> Users { get; set; }
        public DbSet<ChatRoom> ChatRooms { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //additional model settings

            // ChatRoom Configuration
            modelBuilder.Entity<ChatRoom>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DoctorId).IsRequired();
                entity.Property(e => e.PatientId).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.IsActive).HasDefaultValue(true);

                entity.HasIndex(e => new { e.DoctorId, e.PatientId }).IsUnique();
                entity.HasIndex(e => e.DoctorId);
                entity.HasIndex(e => e.PatientId);

                //entity.HasOne(e => e.Doctor)
                //    .WithMany()
                //    .HasForeignKey(e => e.DoctorId)
                //    .OnDelete(DeleteBehavior.Restrict);

                //entity.HasOne(e => e.Patient)
                //    .WithMany()
                //    .HasForeignKey(e => e.PatientId)
                //    .OnDelete(DeleteBehavior.Restrict);

                //entity.HasIndex(e => new { e.DoctorId, e.PatientId });

                //entity.HasMany(e => e.Messages)
                //      .WithOne(m => m.ChatRoom)
                //      .HasForeignKey(m => m.ChatRoomId)
                //      .OnDelete(DeleteBehavior.Cascade);
            });

            // ChatMessage Configuration
            modelBuilder.Entity<ChatMessage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SenderId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Message).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.SentAt).IsRequired();
                entity.Property(e => e.IsRead).HasDefaultValue(false);

                entity.HasOne(e => e.ChatRoom)
                    .WithMany(c => c.Messages)
                    .HasForeignKey(e => e.ChatRoomId)
                    .OnDelete(DeleteBehavior.Cascade);

                //entity.HasOne(e => e.Sender)
                //    .WithMany()
                //    .HasForeignKey(e => e.SenderId)
                //    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.ChatRoomId);
                entity.HasIndex(e => e.SentAt);
                entity.HasIndex(e => e.SenderId);
            });
        }
    }
}
