using ELibraryManagement.Api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ELibraryManagement.Api.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<Book> Books { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<BookCategory> BookCategories { get; set; }
        public DbSet<BorrowRecord> BorrowRecords { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Fine> Fines { get; set; }
        public DbSet<UserStatus> UserStatuses { get; set; }
        public DbSet<FineActionHistory> FineActionHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure BookCategory many-to-many relationship
            modelBuilder.Entity<BookCategory>()
                .HasKey(bc => new { bc.BookId, bc.CategoryId });

            modelBuilder.Entity<BookCategory>()
                .HasOne(bc => bc.Book)
                .WithMany(b => b.BookCategories)
                .HasForeignKey(bc => bc.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BookCategory>()
                .HasOne(bc => bc.Category)
                .WithMany(c => c.BookCategories)
                .HasForeignKey(bc => bc.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure BorrowRecord relationships
            modelBuilder.Entity<BorrowRecord>()
                .HasOne(br => br.User)
                .WithMany(u => u.BorrowRecords)
                .HasForeignKey(br => br.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BorrowRecord>()
                .HasOne(br => br.Book)
                .WithMany(b => b.BorrowRecords)
                .HasForeignKey(br => br.BookId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Review relationships
            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Book)
                .WithMany(b => b.Reviews)
                .HasForeignKey(r => r.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Fine relationships
            modelBuilder.Entity<Fine>()
                .HasOne(f => f.User)
                .WithMany(u => u.Fines)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Fine>()
                .HasOne(f => f.BorrowRecord)
                .WithMany(br => br.Fines)
                .HasForeignKey(f => f.BorrowRecordId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure UserStatus relationships
            modelBuilder.Entity<UserStatus>()
                .HasKey(us => us.UserId);

            modelBuilder.Entity<UserStatus>()
                .HasOne(us => us.User)
                .WithOne(u => u.UserStatus)
                .HasForeignKey<UserStatus>(us => us.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure decimal properties
            modelBuilder.Entity<Fine>()
                .Property(f => f.Amount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<UserStatus>()
                .Property(us => us.TotalOutstandingFines)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<FineActionHistory>()
                .Property(fah => fah.Amount)
                .HasColumnType("decimal(18,2)");

            // Configure indexes for performance
            modelBuilder.Entity<Book>()
                .HasIndex(b => b.ISBN)
                .IsUnique()
                .HasFilter("[ISBN] IS NOT NULL");

            modelBuilder.Entity<Book>()
                .HasIndex(b => new { b.Title, b.Author });

            modelBuilder.Entity<BorrowRecord>()
                .HasIndex(br => new { br.UserId, br.BookId, br.BorrowDate });

            modelBuilder.Entity<Review>()
                .HasIndex(r => new { r.BookId, r.UserId })
                .IsUnique();

            // Configure global query filters for soft delete (chỉ áp dụng cho Book và Category)
            modelBuilder.Entity<Book>()
                .HasQueryFilter(b => !b.IsDeleted);

            modelBuilder.Entity<Category>()
                .HasQueryFilter(c => !c.IsDeleted);

            // Configure query filter for junction table to match Book's filter
            modelBuilder.Entity<BookCategory>()
                .HasQueryFilter(bc => !bc.Book.IsDeleted && !bc.Category.IsDeleted);

            // Seed data
            SeedData.Initialize(modelBuilder);
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                // Xử lý timestamp cho tất cả entities
                if (entry.Entity.GetType().GetProperty("CreatedAt") != null)
                {
                    if (entry.State == EntityState.Added)
                    {
                        entry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
                    }
                }
            }
        }
    }
}
