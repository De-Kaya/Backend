using Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Data.Context;

public class DataContext(DbContextOptions<DataContext> options) : IdentityDbContext(options)
{
    public DbSet<RoomEntity> Rooms { get; set; }
    public DbSet<ReservationEntity> Reservations { get; set; }
    public DbSet<CustomerEntity> Customers { get; set; }
    public DbSet<PaymentEntity> Payments { get; set; }
    public DbSet<MaintenanceLogEntity> MaintenanceLogs { get; set; }
    public DbSet<CustomerBalanceEntity> CustomerBalances { get; set; }
    public DbSet<RoomStatusEntity> RoomStatuses { get; set; }
    public DbSet<PaymentMethodEntity> PaymentMethods { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<RoomStatusEntity>().HasData(
            new RoomStatusEntity { Id = 1, StatusName = "Müsait", Description = "Oda kiralamaya hazır" },
            new RoomStatusEntity { Id = 2, StatusName = "Rezerve", Description = "Oda rezerve edilmiş" },
            new RoomStatusEntity { Id = 3, StatusName = "Bakımda", Description = "Oda bakımda" },
            new RoomStatusEntity { Id = 4, StatusName = "Arızalı", Description = "Oda arızalı" }
        );

        modelBuilder.Entity<PaymentMethodEntity>().HasData(
            new PaymentMethodEntity { Id = 1, MethodName = "Nakit" },
            new PaymentMethodEntity { Id = 2, MethodName = "Kredi Kartı" }
        );

        modelBuilder.Entity<RoomEntity>()
            .HasIndex(r => r.SerialNumber)
            .IsUnique();

        modelBuilder.Entity<RoomStatusEntity>()
            .HasIndex(rs => rs.StatusName)
            .IsUnique();

        modelBuilder.Entity<PaymentMethodEntity>()
            .HasIndex(pm => pm.MethodName)
            .IsUnique();

        modelBuilder.Entity<RoomEntity>()
            .HasOne(r => r.Status)
            .WithMany()
            .HasForeignKey(r => r.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ReservationEntity>()
            .HasOne(r => r.Room)
            .WithMany(r => r.Reservations)
            .HasForeignKey(r => r.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ReservationEntity>()
            .HasOne(r => r.Customer)
            .WithMany(c => c.Reservations)
            .HasForeignKey(r => r.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PaymentEntity>()
            .HasOne(p => p.Reservation)
            .WithMany(r => r.Payments)
            .HasForeignKey(p => p.ReservationId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PaymentEntity>()
            .HasOne(p => p.PaymentMethod)
            .WithMany()
            .HasForeignKey(p => p.PaymentMethodId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MaintenanceLogEntity>()
            .HasOne(m => m.Room)
            .WithMany(r => r.MaintenanceLogs)
            .HasForeignKey(m => m.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CustomerBalanceEntity>()
            .HasOne(b => b.Customer)
            .WithMany(c => c.CustomerBalances)
            .HasForeignKey(b => b.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CustomerBalanceEntity>()
            .HasOne(b => b.Reservation)
            .WithMany()
            .HasForeignKey(b => b.ReservationId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CustomerBalanceEntity>()
            .HasOne(b => b.Payment)
            .WithMany()
            .HasForeignKey(b => b.PaymentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}