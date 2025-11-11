using Microsoft.EntityFrameworkCore;
using DocMan.Model.Entities;
using System.Security.Cryptography;
using System.Text;

namespace DocMan.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<DocumentChunk> DocumentChunks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.FullName).HasMaxLength(256);
            entity.Property(e => e.Theme).HasMaxLength(50).HasDefaultValue("light");

            // Unique constraints
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();

            // Soft delete filter
            entity.HasQueryFilter(e => e.DeletedAt == null);
        });

        // Category configuration
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Category");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Color).HasMaxLength(7).HasDefaultValue("#000000");

            // Soft delete filter
            entity.HasQueryFilter(e => e.DeletedAt == null);
        });

        // Document configuration
        modelBuilder.Entity<Document>(entity =>
        {
            entity.ToTable("Document");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(512);
            entity.Property(e => e.ContentType).HasMaxLength(100);
            entity.Property(e => e.Extension).HasMaxLength(10);
            entity.Property(e => e.FilePath).HasMaxLength(1000);

            entity.HasOne(e => e.User)
                .WithMany(u => u.Documents)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Category)
                .WithMany(c => c.Documents)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // Soft delete filter
            entity.HasQueryFilter(e => e.DeletedAt == null);
        });

        // DocumentChunk configuration with Azure SQL vector support
        modelBuilder.Entity<DocumentChunk>(entity =>
        {
            entity.ToTable("DocumentChunk");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired();

            // Azure SQL vector type for dense embeddings (1536 dimensions for text-embedding-3-small)
            entity.Property(e => e.DenseEmbedding)
                .HasColumnType("vector(1536)");

            entity.HasOne(e => e.Document)
                .WithMany(d => d.Chunks)
                .HasForeignKey(e => e.DocumentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_DocumentChunks_Documents");

            // Soft delete filter
            //entity.HasQueryFilter(e => e.DeletedAt == null);
        });

        // Seed data
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Use static GUIDs for seeding
        var user1Id = new Guid("11111111-1111-1111-1111-111111111111");
        var user2Id = new Guid("22222222-2222-2222-2222-222222222222");
        var seedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var user1 = new User
        {
            Id = user1Id,
            Username = "john_doe",
            Email = "john@example.com",
            PasswordHash = HashPassword("Password123!"),
            FullName = "John Doe",
            Theme = "light",
            IsActive = true,
            CreatedAt = seedDate
        };

        var user2 = new User
        {
            Id = user2Id,
            Username = "jane_smith",
            Email = "jane@example.com",
            PasswordHash = HashPassword("Password123!"),
            FullName = "Jane Smith",
            Theme = "dark",
            IsActive = true,
            CreatedAt = seedDate
        };

        modelBuilder.Entity<User>().HasData(user1, user2);

        // Create seed categories
        var category1 = new Category
        {
            Id = new Guid("33333333-3333-3333-3333-333333333333"),
            Name = "Technical Documentation",
            Description = "Technical manuals and API documentation",
            Color = "#3498db",
            CreatedAt = seedDate
        };

        var category2 = new Category
        {
            Id = new Guid("44444444-4444-4444-4444-444444444444"),
            Name = "Business Reports",
            Description = "Financial and business reports",
            Color = "#e74c3c",
            CreatedAt = seedDate
        };

        var category3 = new Category
        {
            Id = new Guid("55555555-5555-5555-5555-555555555555"),
            Name = "Research Papers",
            Description = "Academic and research papers",
            Color = "#2ecc71",
            CreatedAt = seedDate
        };

        var category4 = new Category
        {
            Id = new Guid("66666666-6666-6666-6666-666666666666"),
            Name = "Project Files",
            Description = "Project documentation and specifications",
            Color = "#f39c12",
            CreatedAt = seedDate
        };

        modelBuilder.Entity<Category>().HasData(category1, category2, category3, category4);
    }

    private static string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}

