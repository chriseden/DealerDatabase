using Microsoft.EntityFrameworkCore;
using DealerDatabase.Data.Entities;

namespace DealerDatabase.Data;

public class DealerDbContext(DbContextOptions<DealerDbContext> options) : DbContext(options)
{
    public DbSet<Dealer> Dealers => Set<Dealer>();
    public DbSet<TradingName> TradingNames => Set<TradingName>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Director> Directors => Set<Director>();
    public DbSet<Source> Sources => Set<Source>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Dealer>(entity =>
        {
            entity.HasKey(d => d.Id);

            entity.Property(d => d.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(d => d.LineageTag)
                .IsRequired()
                .HasColumnName("LineageTag")
                .HasMaxLength(1000);
            
            entity.Property(d => d.CompanyNumber).HasMaxLength(20);
            entity.Property(d => d.CompanyStatus).HasMaxLength(50);

            entity.Property(d => d.FcaReferenceNumber).HasMaxLength(20);
            entity.Property(d => d.FcaStatus).HasMaxLength(50);
            entity.Property(d => d.IcoRegistrationNumber).HasMaxLength(20);
            entity.Property(d => d.SafStatus).HasMaxLength(50);
            entity.Property(d => d.VatNumber).HasMaxLength(30);
            entity.Property(d => d.Domain).HasMaxLength(1024);

            entity.HasIndex(d => d.CompanyNumber);
            entity.HasIndex(d => d.FcaReferenceNumber);
            entity.HasIndex(d => d.VatNumber);

            entity.HasMany(d => d.TradingNames)
                .WithOne()
                .HasForeignKey(t => t.DealerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(d => d.Addresses)
                .WithOne()
                .HasForeignKey(a => a.DealerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(d => d.Contacts)
                .WithOne()
                .HasForeignKey(c => c.DealerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(d => d.Directors)
                .WithOne()
                .HasForeignKey(dr => dr.DealerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(d => d.Sources)
                .WithOne()
                .HasForeignKey(s => s.DealerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TradingName>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Name).IsRequired().HasMaxLength(200);
            entity.Property(t => t.Source).HasMaxLength(100);
        });

        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.AddressType).HasMaxLength(50);
            entity.Property(a => a.AddressLine1).HasMaxLength(150);
            entity.Property(a => a.City).HasMaxLength(100);
            entity.Property(a => a.Postcode).HasMaxLength(20);
            entity.Property(a => a.Source).HasMaxLength(100);

            entity.HasIndex(a => a.Postcode);
        });

        modelBuilder.Entity<Contact>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.ContactType).HasMaxLength(50); 
            entity.Property(c => c.Value).HasMaxLength(250);
            entity.Property(c => c.Source).HasMaxLength(100);
        });

        modelBuilder.Entity<Director>(entity =>
        {
            entity.HasKey(dr => dr.Id);
            entity.Property(dr => dr.FullName).HasMaxLength(150);
            entity.Property(dr => dr.Role).HasMaxLength(100);
            entity.Property(dr => dr.Source).HasMaxLength(100);
        });

        modelBuilder.Entity<Source>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.SourceName).HasMaxLength(100);
            entity.Property(s => s.RawPayloadJson).HasColumnType("TEXT"); 
        });
    }
}