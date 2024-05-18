using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SuperAdmin.Service.Database.Entities;
using SuperAdmin.Service.Database.Enums;

namespace SuperAdmin.Service.Database
{
    public class AppDbContext : IdentityDbContext<User, Role, string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Role>()
                .HasMany(x => x.Permissions)
                .WithMany(x => x.Roles)
                .UsingEntity<RolePermission>(
                    x => x.HasOne<Permission>(y => y.Permission).WithMany(y => y.RolePermissions).HasForeignKey(y => y.PermissionId),
                    x => x.HasOne<Role>(y => y.Role).WithMany(y => y.RolePermissions).HasForeignKey(y => y.RoleId)
                );

            builder.Entity<Ticket>()
                .HasMany(t => t.TicketActionLogs)
                .WithOne(t => t.Ticket)
                .HasForeignKey(t => t.TicketId)
                .IsRequired();

            builder.Entity<Ticket>()
                .HasMany(t => t.TicketReplies)
                .WithOne(t => t.Ticket)
                .HasForeignKey(t => t.TicketId)
                .IsRequired();

            builder.Entity<Ticket>()
                .Property(t => t.Priority)
                .HasConversion(p => p.ToString(), p => (Priority)Enum.Parse(typeof(Priority), p))
                .HasDefaultValue(Priority.LOW);

            builder.Entity<Ticket>()
              .Property(t => t.Status)
              .HasConversion(s => s.ToString(), s => (TicketStatus)Enum.Parse(typeof(TicketStatus), s))
              .HasDefaultValue(TicketStatus.PENDING);

            builder.Entity<TicketActionLog>()
                .Property(t => t.ActionType)
                .HasConversion(a => a.ToString(), a => (ActionType)Enum.Parse(typeof(ActionType), a));

            builder.Entity<TicketActionLog>()
                .HasOne(t => t.PerformedByUser)
                .WithMany()
                .HasForeignKey(t => t.PerformedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<TicketReply>()
                .Property(t => t.UserType)
                .HasConversion(u => u.ToString(), u => (UserType)Enum.Parse(typeof(UserType), u));

            builder.Entity<AutoSavingSetting>()
                .HasIndex(x => x.DurationInMonths)
                .IsUnique();
        }

        public DbSet<OneTimePassword> OneTimePasswords { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<Country> Country { get; set; }
        public DbSet<SalaryDisbursementChargeSetting> SalaryDisbursementChargeSettings { get; set; }
        public DbSet<CompanySalaryDisbursementChargeSetting> CompanySalaryDisbursementChargeSettings { get; set; }
        public DbSet<PayrollVendorChargeSeting> PayrollVendorChargeSetings { get; set; }
        public DbSet<PayrollVendorChargeDeductionBasis> PayrollVendorChargeDeductionBasis { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketCategory> TicketCategories { get; set; }
        public DbSet<TicketActionLog> TicketActionLogs { get; set; }
        public DbSet<TicketReply> TicketReplies { get; set; }
        public DbSet<AutoSavingSetting> AutoSavingSettings { get; set; }
        public DbSet<WeavrCorporateUser> WeavrCorporateUsers { get; set; }
        public DbSet<WeavrLoginCredential> WeavrLoginCredential { get; set; }
        public DbSet<WeavrCorporateAccount> WeavrCorporateAccounts { get; set; }
        public DbSet<WeavrAccountBalance> WeavrAccountBalances { get; set; }
        public DbSet<ProductUpdateCategory> ProductUpdateCategories { get; set; }
        public DbSet<ProductUpdate> ProductUpdates { get; set; }
    }
}
