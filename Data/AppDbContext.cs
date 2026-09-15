using Microsoft.EntityFrameworkCore;
using LostAndFoundAgency.Models;
using System.Linq;

namespace LostAndFoundAgency.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Person> Persons { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<FoundItem> FoundItems { get; set; }
        public DbSet<LostRequest> LostRequests { get; set; }
        public DbSet<Return> Returns { get; set; }
        public DbSet<Match> Matches { get; set; }

        public AppDbContext()
        {
        }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=LostAndFoundDB;Trusted_Connection=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigurePeople(modelBuilder);
            ConfigureReferenceData(modelBuilder);
            ConfigureRequests(modelBuilder);
            ConfigureMatchesAndReturns(modelBuilder);

            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }

        private static void ConfigurePeople(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Person>(entity =>
            {
                entity.Property(p => p.FullName).IsRequired().HasMaxLength(120);
                entity.Property(p => p.Login).IsRequired().HasMaxLength(64);
                entity.Property(p => p.PasswordHash).IsRequired().HasMaxLength(256);
                entity.Property(p => p.Phone).HasMaxLength(32);
                entity.Property(p => p.Email).HasMaxLength(255);
                entity.Property(p => p.DocumentNumber).HasMaxLength(64);
                entity.Property(p => p.Address).HasMaxLength(250);
                entity.HasIndex(p => p.Login).IsUnique();
            });

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(120);
                entity.Property(e => e.Login).IsRequired().HasMaxLength(64);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(256);
                entity.Property(e => e.Position).IsRequired().HasMaxLength(80);
                entity.Property(e => e.Phone).HasMaxLength(32);
                entity.HasIndex(e => e.Login).IsUnique();
            });
        }

        private static void ConfigureReferenceData(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>(entity =>
            {
                entity.Property(c => c.CategoryName).IsRequired().HasMaxLength(80);
                entity.Property(c => c.Description).HasMaxLength(500);
                entity.HasIndex(c => c.CategoryName).IsUnique();
            });

            modelBuilder.Entity<Location>(entity =>
            {
                entity.Property(l => l.LocationName).IsRequired().HasMaxLength(120);
                entity.Property(l => l.Address).HasMaxLength(250);
                entity.Property(l => l.Description).HasMaxLength(500);
                entity.HasIndex(l => l.LocationName).IsUnique();
            });
        }

        private static void ConfigureRequests(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LostRequest>(entity =>
            {
                entity.ToTable(table => table.HasCheckConstraint(
                    "CK_LostRequests_Status",
                    $"[Status] IN (N'{ItemStatuses.Pending}', N'{ItemStatuses.LostClosedFound}', N'{ItemStatuses.Rejected}')"));

                entity.Property(r => r.ItemName).IsRequired().HasMaxLength(120);
                entity.Property(r => r.Description).HasMaxLength(1000);
                entity.Property(r => r.Color).HasMaxLength(50);
                entity.Property(r => r.Brand).HasMaxLength(80);
                entity.Property(r => r.Status).IsRequired().HasMaxLength(40).HasDefaultValue(ItemStatuses.Pending);
                entity.Property(r => r.RequestDate).HasDefaultValueSql("SYSUTCDATETIME()");

                entity.HasIndex(r => r.Status);
                entity.HasIndex(r => r.DateLost);
                entity.HasIndex(r => new { r.CategoryId, r.Status });

                entity.HasOne(r => r.Person)
                    .WithMany(p => p.LostRequests)
                    .HasForeignKey(r => r.PersonId);

                entity.HasOne(r => r.Category)
                    .WithMany(c => c.LostRequests)
                    .HasForeignKey(r => r.CategoryId);

                entity.HasOne(r => r.Location)
                    .WithMany(l => l.LostRequests)
                    .HasForeignKey(r => r.LocationId);

                entity.HasOne(r => r.Employee)
                    .WithMany(e => e.AssignedLostRequests)
                    .HasForeignKey(r => r.EmployeeId);
            });

            modelBuilder.Entity<FoundItem>(entity =>
            {
                entity.ToTable(table => table.HasCheckConstraint(
                    "CK_FoundItems_Status",
                    $"[Status] IN (N'{ItemStatuses.Pending}', N'{ItemStatuses.FoundClosedReturned}', N'{ItemStatuses.Rejected}')"));

                entity.Property(i => i.ItemName).IsRequired().HasMaxLength(120);
                entity.Property(i => i.Description).HasMaxLength(1000);
                entity.Property(i => i.Color).HasMaxLength(50);
                entity.Property(i => i.Brand).HasMaxLength(80);
                entity.Property(i => i.StorageLocation).HasMaxLength(160);
                entity.Property(i => i.Status).IsRequired().HasMaxLength(40).HasDefaultValue(ItemStatuses.Pending);
                entity.Property(i => i.PhotoPath).HasMaxLength(500);

                entity.HasIndex(i => i.Status);
                entity.HasIndex(i => i.DateFound);
                entity.HasIndex(i => new { i.CategoryId, i.Status });

                entity.HasOne(i => i.Finder)
                    .WithMany(p => p.FoundItems)
                    .HasForeignKey(i => i.FinderId);

                entity.HasOne(i => i.Category)
                    .WithMany(c => c.FoundItems)
                    .HasForeignKey(i => i.CategoryId);

                entity.HasOne(i => i.Location)
                    .WithMany(l => l.FoundItems)
                    .HasForeignKey(i => i.LocationId);

                entity.HasOne(i => i.Employee)
                    .WithMany(e => e.AssignedFoundItems)
                    .HasForeignKey(i => i.EmployeeId);
            });
        }

        private static void ConfigureMatchesAndReturns(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Match>(entity =>
            {
                entity.ToTable(table =>
                {
                    table.HasCheckConstraint("CK_Matches_MatchPercent", "[MatchPercent] >= 0 AND [MatchPercent] <= 100");
                    table.HasCheckConstraint(
                        "CK_Matches_Status",
                        $"[MatchStatus] IN (N'{MatchStatuses.Pending}', N'{MatchStatuses.Confirmed}', N'{MatchStatuses.Rejected}')");
                });

                entity.Property(m => m.MatchStatus).IsRequired().HasMaxLength(40).HasDefaultValue(MatchStatuses.Pending);
                entity.Property(m => m.Comment).HasMaxLength(500);
                entity.Property(m => m.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
                entity.HasIndex(m => new { m.FoundItemId, m.LostRequestId }).IsUnique();
                entity.HasIndex(m => m.MatchStatus);

                entity.HasOne(m => m.FoundItem)
                    .WithMany(i => i.Matches)
                    .HasForeignKey(m => m.FoundItemId);

                entity.HasOne(m => m.LostRequest)
                    .WithMany(r => r.Matches)
                    .HasForeignKey(m => m.LostRequestId);
            });

            modelBuilder.Entity<Return>(entity =>
            {
                entity.Property(r => r.DocumentNumber).HasMaxLength(64);
                entity.Property(r => r.Comment).HasMaxLength(500);
                entity.Property(r => r.ReturnDate).HasDefaultValueSql("SYSUTCDATETIME()");
                entity.HasIndex(r => r.FoundItemId).IsUnique();

                entity.HasOne(r => r.FoundItem)
                    .WithOne(i => i.Return)
                    .HasForeignKey<Return>(r => r.FoundItemId);

                entity.HasOne(r => r.Person)
                    .WithMany(p => p.Returns)
                    .HasForeignKey(r => r.PersonId);

                entity.HasOne(r => r.Employee)
                    .WithMany(e => e.ProcessedReturns)
                    .HasForeignKey(r => r.EmployeeId);
            });
        }
    }
}
