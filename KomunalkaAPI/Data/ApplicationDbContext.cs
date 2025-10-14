using KomunalkaAPI.Extensions;
using KomunalkaAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace KomunalkaAPI.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Tariff> Tariffs { get; set; }
        public DbSet<ServiceCounterValue> ServiceCounterValues { get; set; }
        public DbSet<ServiceCounterMeasurement> ServiceCounterMeasurements { get; set; }
        public DbSet<ServiceCounter> ServiceCounters { get; set; }
        public DbSet<ServiceCategory> ServiceCategories { get; set; }
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<AddressesServiceCategory> AddressesServiceCategories { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Region> Regions { get; set; }
        public DbSet<AddressType> AddressTypes { get; set; }
        public DbSet<UtilityType> UtilityTypes { get; set; }
        public DbSet<Meter> Meters { get; set; }
        public DbSet<MeterReadingImage> MeterReadingImages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply Laravel naming conventions (snake_case)
            modelBuilder.UseLaravelNamingConventions();

            // Configure many-to-many relationship with Laravel naming
            // Laravel uses alphabetical order: address_service_category (not addresses_service_categories)
            modelBuilder.Entity<AddressesServiceCategory>()
                .ToTable("address_service_category");
        }
    }
}
