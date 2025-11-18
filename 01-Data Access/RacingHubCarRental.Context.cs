using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FinanceML.Core.Models; // Change to your real namespace
using RacingHubCarRental.Models.Database; // Recommended new folder

namespace RacingHubCarRental.Data
{
    /// <summary>
    /// Modern EF Core DbContext replacing the old EF6 Database-First auto-generated context.
    /// Clean, async, DI-friendly, and stored-procedure supported.
    /// </summary>
    public class RacingHubCarRentalDbContext : DbContext
    {
        public RacingHubCarRentalDbContext(DbContextOptions<RacingHubCarRentalDbContext> options)
            : base(options)
        {
        }

        // ============================
        // DbSets (Tables)
        // ============================
        public DbSet<CarModel> CarModels { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<FleetCar> FleetCars { get; set; }
        public DbSet<ManufacturerModel> ManufacturerModels { get; set; }
        public DbSet<Manufacturer> Manufacturers { get; set; }
        public DbSet<Rental> Rentals { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }

        // ============================
        // Stored Procedure Result Models
        // ============================
        public DbSet<CarSearchResult> CarSearchResults { get; set; }
        public DbSet<AvailabilityResult> AvailabilityResults { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Prevent EF from trying to create key for SP result models
            modelBuilder.Entity<CarSearchResult>().HasNoKey();
            modelBuilder.Entity<AvailabilityResult>().HasNoKey();

            base.OnModelCreating(modelBuilder);
        }

        // ================================================
        // Modern Stored Procedure Calls (Async + Clean)
        // ================================================
        public async Task<int> CarCheckAvailabilityAsync(string licensePlate, DateTime? start, DateTime? end)
        {
            var sql = "EXEC CarCheckAvailability @p0, @p1, @p2";

            return await Database.ExecuteSqlRawAsync(sql, licensePlate, start, end);
        }

        public async Task<List<CarSearchResult>> CarSearchAsync(
            int? manufacturerId,
            int? modelId,
            int? year,
            bool? manualGear,
            DateTime? startDate,
            DateTime? returnDate,
            string freeText)
        {
            string sql =
                "EXEC CarsSearch @p0, @p1, @p2, @p3, @p4, @p5, @p6";

            return await CarSearchResults
                .FromSqlRaw(sql,
                    manufacturerId,
                    modelId,
                    year,
                    manualGear,
                    startDate,
                    returnDate,
                    freeText
                )
                .ToListAsync();
        }

        public async Task<List<AvailabilityResult>> UpdateAvailabilityAsync()
        {
            return await AvailabilityResults
                .FromSqlRaw("EXEC UpdateAvailabilityOfCars")
                .ToListAsync();
        }
    }

    // ===========================================================
    // Strongly Typed Result Models for Stored Procedures
    // ===========================================================

    public class CarSearchResult
    {
        public string LicensePlate { get; set; }
        public string Manufacturer { get; set; }
        public string ModelName { get; set; }
        public int ProductionYear { get; set; }
        public bool ManualGear { get; set; }
        public decimal DailyPrice { get; set; }
    }

    public class AvailabilityResult
    {
        public string LicensePlate { get; set; }
        public bool IsAvailable { get; set; }
    }
}

