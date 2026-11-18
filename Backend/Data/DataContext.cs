using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace Backend.Data
{
    public class DataContext : DbContext, IDataProtectionKeyContext
    {
        public DataContext(DbContextOptions<DataContext> options)
            : base(options) { }

        public DbSet<SiteText> SiteTexts { get; set; }
        public DbSet<GalleryImage> GalleryImages { get; set; }

        // Noeglerne der krypterer login-cookies. Gemmes i databasen, saa man ikke logges ud
        // (og et login i gang ikke fejler), hver gang serveren genstarter.
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;

        // Indbygget, cookiefri besoegsstatistik (se Controllers/Analytics/AnalyticsController.cs)
        public DbSet<AnalyticsEvent> AnalyticsEvents { get; set; } = null!;
        public DbSet<AnalyticsSalt> AnalyticsSalts { get; set; } = null!;

        // Ydelser (kort paa forsiden + egen side /ydelser/<slug>) og deres billeder/videoer
        public DbSet<Service> Services { get; set; } = null!;
        public DbSet<ServiceMedia> ServiceMedia { get; set; } = null!;

        // Henvendelser fra kontaktformularen (ryddes op efter 12 måneder)
        public DbSet<ContactRequest> ContactRequests { get; set; } = null!;
        public DbSet<ContactAttachment> ContactAttachments { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SiteText>().ToTable("SiteTexts");
            modelBuilder.Entity<SiteText>().HasData(SiteTextSeed.Texts);

            modelBuilder.Entity<GalleryImage>().ToTable("GalleryImages");
            modelBuilder.Entity<GalleryImage>().HasIndex(i => i.FileName).IsUnique();
            modelBuilder.Entity<GalleryImage>().HasIndex(i => i.ContentHash);
            // Eksisterende rækker (fra før video) er billeder
            modelBuilder.Entity<GalleryImage>().Property(i => i.Kind).HasDefaultValue("image");

            modelBuilder.Entity<AnalyticsEvent>(e =>
            {
                e.ToTable("AnalyticsEvents");
                e.Property(x => x.Type).HasMaxLength(16);
                e.Property(x => x.Path).HasMaxLength(200);
                e.Property(x => x.Target).HasMaxLength(64);
                e.Property(x => x.VisitorHash).HasMaxLength(64);
                e.Property(x => x.Referrer).HasMaxLength(100);
                e.Property(x => x.Device).HasMaxLength(8);
                e.Property(x => x.Country).HasMaxLength(2);
                e.Property(x => x.City).HasMaxLength(100);
                e.HasIndex(x => x.CreatedAt);
                e.HasIndex(x => new { x.Type, x.CreatedAt });
            });

            modelBuilder.Entity<AnalyticsSalt>(e =>
            {
                e.ToTable("AnalyticsSalts");
                e.HasKey(x => x.Day);
                e.Property(x => x.Salt).HasMaxLength(64);
            });

            modelBuilder.Entity<Service>(e =>
            {
                e.ToTable("Services");
                e.HasIndex(x => x.Slug).IsUnique();
                e.HasIndex(x => x.SortOrder);
                e.HasMany(x => x.Media)
                    .WithOne(m => m.Service)
                    .HasForeignKey(m => m.ServiceId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ServiceMedia>(e =>
            {
                e.ToTable("ServiceMedia");
                e.HasIndex(x => new { x.ServiceId, x.SortOrder });
            });

            modelBuilder.Entity<ContactRequest>(e =>
            {
                e.ToTable("ContactRequests");
                e.HasIndex(x => x.CreatedAt);
                e.HasMany(x => x.Attachments)
                    .WithOne(a => a.Request)
                    .HasForeignKey(a => a.ContactRequestId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ContactAttachment>(e =>
            {
                e.ToTable("ContactAttachments");
                e.HasIndex(x => x.ContactRequestId);
            });
        }
    }
}
