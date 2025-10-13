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
    }
}
