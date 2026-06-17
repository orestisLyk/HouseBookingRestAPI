using HouseBookingRestApi.Models;
using Microsoft.EntityFrameworkCore;

namespace HouseBookingRestApi.Data
{
    public class HouseBookingRestApiContext: DbContext
    {
        public HouseBookingRestApiContext(DbContextOptions<HouseBookingRestApiContext> options)
            : base(options)
        {
        }

        public DbSet<Capability> Capabilities { get; set; }

        public DbSet<Role> Roles { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<Owner> Owners { get; set; }

        public DbSet<Renter> Renters { get; set; }

        public DbSet<House> Houses { get; set; }

        public DbSet<Booking> Bookings { get; set; }

        public DbSet<HouseImage> HouseImages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Capability>((entity) =>
            {
                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.HasIndex(e => e.Name, "UQ_Capabilities_Name").IsUnique();
                entity.HasIndex(e => e.Name, "IX_Capabilities_Name");
            });

            modelBuilder.Entity<Role>((entity) =>
            {
                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);

                entity.HasMany(d => d.Capabilities).WithMany(p => p.Roles)
                    .UsingEntity("RolesCapabilities", j =>
                    {
                        j.HasIndex("CapabilitiesId")
                        .HasDatabaseName("IX_RolesCapabilities_CapabilitiesId");
                    });
                
                entity.HasIndex(e => e.Name, "UQ_Roles_Name").IsUnique();
                entity.HasIndex(e => e.Name, "IX_Roles_Name");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Firstname).HasMaxLength(100);
                entity.Property(e => e.Lastname).HasMaxLength(100);
                entity.Property(e => e.Username).HasMaxLength(100);
                entity.Property(e => e.Password).HasMaxLength(100);

                entity.HasOne(d => d.Role).WithMany(p => p.Users)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Users_Roles");

                entity.HasIndex(e => e.Email, "IX_Users_Email");
                entity.HasIndex(e => e.RoleId, "IX_Users_RoleId");
                entity.HasIndex(e => e.Username, "IX_Users_Username");
                entity.HasIndex(e => e.Email, "UQ_Users_Email").IsUnique();
                entity.HasIndex(e => e.Username, "UQ_Users_Username").IsUnique();
            });

            modelBuilder.Entity<Owner>(entity =>
            {
                entity.HasOne(d => d.User).WithOne(p => p.Owner)
                    .HasForeignKey<Owner>(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Owners_Users");

                entity.HasIndex(e => e.UserId, "IX_Owners_UserId").IsUnique();
            });

            modelBuilder.Entity<Renter>(entity =>
            {
                entity.HasOne(d => d.User).WithOne(p => p.Renter)
                    .HasForeignKey<Renter>(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Renters_Users");
                entity.HasIndex(e => e.UserId, "IX_Renters_UserId").IsUnique();
            });

            modelBuilder.Entity<House>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Address).HasMaxLength(200);
                entity.Property(e => e.Region).HasMaxLength(100);
                entity.Property(e => e.PricePerNight).HasColumnType("decimal(18,2)");
                entity.HasOne(d => d.Owner).WithMany(p => p.Houses)
                    .HasForeignKey(d => d.OwnerId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Houses_Owners");
                entity.HasIndex(e => e.OwnerId, "IX_Houses_OwnerId");
            });

            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasOne(d => d.House).WithMany(p => p.Bookings)
                    .HasForeignKey(d => d.HouseId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Bookings_Houses");
                entity.HasOne(d => d.Renter).WithMany(p => p.Bookings)
                    .HasForeignKey(d => d.RenterId)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_Bookings_Renters");
                entity.HasIndex(e => e.HouseId, "IX_Bookings_HouseId");
                entity.HasIndex(e => e.RenterId, "IX_Bookings_RenterId");
            });

            modelBuilder.Entity<HouseImage>(entity =>
            {
                entity.Property(e => e.Url).HasMaxLength(500);
                entity.HasOne(d => d.House).WithMany(p => p.HouseImages)
                    .HasForeignKey(d => d.HouseId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_HouseImages_House");
                entity.HasIndex(e => e.HouseId, "IX_HouseImages_HouseId");
            });
        }
    }
}
