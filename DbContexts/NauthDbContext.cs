using Microsoft.EntityFrameworkCore;
using nauth_asp.Models;
using nauth_asp.Models.TwoFactorAuth;
using nauth_asp.Models.EmailAction;
using nauth_asp.Models.Permissions;
using nauth_asp.Models.Service;
using nauth_asp.Models.Session;

namespace nauth_asp.DbContexts
{
    public class NauthDbContext : DbContext
    {
        public NauthDbContext(DbContextOptions<NauthDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<DB_User> Users { get; set; }
        public DbSet<DB_Session> Sessions { get; set; }
        public DbSet<DB_Service> Services { get; set; }
        public DbSet<DB_Permission> Permissions { get; set; }
        public DbSet<DB_UserPermission> UserPermissions { get; set; }
        public DbSet<DB_UserService> UserServices { get; set; }
        public DbSet<DB_2FAEntry> TwoFactorAuthEntries { get; set; }
        public DbSet<DB_EmailAction> EmailActions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<DB_User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.email).IsRequired().HasMaxLength(255);
                entity.HasIndex(e => e.email).IsUnique();
                entity.Property(e => e.passwordHash).IsRequired();
                entity.Property(e => e.passwordSalt).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
            });

            // Service configuration
            modelBuilder.Entity<DB_Service>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.userId).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();

                // Foreign key relationship
                entity.HasOne(s => s.user)
                    .WithMany(u => u.ownedServices)
                    .HasForeignKey(s => s.userId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Permission configuration
            modelBuilder.Entity<DB_Permission>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.key).IsRequired().HasMaxLength(255);
                entity.Property(e => e.ServiceId).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();

                // Unique constraint on key per service
                entity.HasIndex(e => new { e.key, e.ServiceId }).IsUnique();

                // Foreign key relationship
                entity.HasOne(p => p.Service)
                    .WithMany(s => s.permissions)
                    .HasForeignKey(p => p.ServiceId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Session configuration
            modelBuilder.Entity<DB_Session>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.jwtHash).IsRequired();
                entity.Property(e => e.userId).IsRequired();
                entity.Property(e => e.serviceId).IsRequired(false);
                entity.Property(e => e.is2FAConfirmed).IsRequired();
                entity.Property(e => e.ExpiresAt).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();

                // Foreign key relationships with explicit property mapping
                entity.HasOne(s => s.user)
                    .WithMany(u => u.sessions)
                    .HasForeignKey(s => s.userId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(s => s.service)
                    .WithMany(s => s.sessions)
                    .HasForeignKey(s => s.serviceId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // UserPermission configuration
            modelBuilder.Entity<DB_UserPermission>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.permissionId).IsRequired();
                entity.Property(e => e.userId).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();

                // Unique constraint on user-permission combination
                entity.HasIndex(e => new { e.userId, e.permissionId }).IsUnique();

                // Foreign key relationships
                entity.HasOne(up => up.permission)
                    .WithMany()
                    .HasForeignKey(up => up.permissionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(up => up.user)
                    .WithMany(u => u.permissions)
                    .HasForeignKey(up => up.userId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // UserService configuration
            modelBuilder.Entity<DB_UserService>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.userId).IsRequired();
                entity.Property(e => e.serviceId).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();

                // Unique constraint on user-service combination
                entity.HasIndex(e => new { e.userId, e.serviceId }).IsUnique();

                // Foreign key relationships
                entity.HasOne(us => us.user)
                    .WithMany(u => u.services)
                    .HasForeignKey(us => us.userId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(us => us.service)
                    .WithMany()
                    .HasForeignKey(us => us.serviceId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // TwoFactorAuth configuration
            modelBuilder.Entity<DB_2FAEntry>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.recoveryCode).IsRequired();
                entity.Property(e => e._2faSecret).IsRequired();
                entity.Property(e => e.userId).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();

                // Foreign key relationships
                entity.HasOne(tfa => tfa.user)
                    .WithMany(u => u._2FAEntries)
                    .HasForeignKey(tfa => tfa.userId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // EmailAction configuration
            modelBuilder.Entity<DB_EmailAction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.token).IsRequired();
                entity.Property(e => e.ExpiresAt).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();

                // Index on token for quick lookups
                entity.HasIndex(e => e.token).IsUnique();
            });
        }
    }
}
