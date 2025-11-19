using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RacingHubCarRental
{
    /// <summary>
    /// Core business logic for handling Rentals (Orders).
    /// Modernized to async, SOLID, and granular commit approach.
    /// </summary>
    public class OrdersLogic : BaseLogic
    {
        // =====================================================================
        // VALIDATION HELPERS (each can be a separate commit)
        // =====================================================================

        private void ValidateId(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid rental ID.", nameof(id));
        }

        private void ValidateString(string value, string param)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"{param} cannot be empty.", param);
        }

        private void ValidateUser(User user)
        {
            if (user == null)
                throw new InvalidOperationException("User does not exist.");
        }

        // =====================================================================
        // QUERY HELPERS
        // =====================================================================

        private IQueryable<Rental> BaseQuery()
        {
            return DB.Rentals.Include(r => r.FleetCar);
        }

        private IQueryable<Rental> OrderedHistory(IQueryable<Rental> q)
        {
            return q.OrderByDescending(r => r.RentalID);
        }

        private IQueryable<Rental> PendingReturns(IQueryable<Rental> q)
        {
            return q.Where(r => r.ActualReturnDate == null)
                    .OrderBy(r => r.ReturnDate);
        }


        // =====================================================================
        // SYNC API (for backward compatibility)
        // =====================================================================

        public List<Rental> GetAllRentals() =>
            BaseQuery().ToList();

        public List<Rental> GetRentalsToCarReturn() =>
            PendingReturns(BaseQuery()).ToList();

        public List<Rental> GetRentalsForUser(string username)
        {
            ValidateString(username, nameof(username));

            return BaseQuery()
                .Where(r => r.User.Username.ToLower() == username.ToLower())
                .OrderByDescending(r => r.RentalID)
                .ToList();
        }

        public Rental GetRentalByID(int id)
        {
            ValidateId(id);
            return DB.Rentals.Find(id);
        }


        // =====================================================================
        // ASYNC API (modern recommended implementation)
        // =====================================================================

        public async Task<List<Rental>> GetAllRentalsAsync(CancellationToken token = default)
        {
            return await SafeExecuteAsync(async () =>
            {
                return await BaseQuery().ToListAsync(token);
            }, token);
        }

        public async Task<List<Rental>> GetRentalsToCarReturnAsync(CancellationToken token = default)
        {
            return await SafeExecuteAsync(async () =>
            {
                return await PendingReturns(BaseQuery()).ToListAsync(token);
            }, token);
        }

        public async Task<List<Rental>> GetRentalsForUserAsync(string username, CancellationToken token = default)
        {
            ValidateString(username, nameof(username));

            return await SafeExecuteAsync(async () =>
            {
                return await OrderedHistory(BaseQuery()
                    .Where(r => r.User.Username.ToLower() == username.ToLower()))
                    .ToListAsync(token);
            }, token);
        }

        public async Task<Rental?> GetRentalByIdAsync(int id, CancellationToken token = default)
        {
            ValidateId(id);

            return await SafeExecuteAsync(async () =>
            {
                return await DB.Rentals.FindAsync(token, id);
            }, token);
        }


        // =====================================================================
        // INSERT RENTAL
        // =====================================================================

        public int InsertRental(string licenseNumber, DateTime startDate, DateTime returnDate, string username)
        {
            ValidateString(licenseNumber, nameof(licenseNumber));
            ValidateString(username, nameof(username));

            var user = DB.Users.FirstOrDefault(u => u.Username.ToLower() == username.ToLower());
            ValidateUser(user);

            var rental = new Rental
            {
                LicensePlate = licenseNumber,
                PickUpDate = startDate,
                ReturnDate = returnDate,
                UserID = user.UserID
            };

            DB.Rentals.Add(rental);
            DB.SaveChanges();

            DB.Entry(rental).GetDatabaseValues();
            return rental.RentalID;
        }

        public async Task<int> InsertRentalAsync(
            string licenseNumber,
            DateTime startDate,
            DateTime returnDate,
            string username,
            CancellationToken token = default)
        {
            ValidateString(licenseNumber, nameof(licenseNumber));
            ValidateString(username, nameof(username));

            return await SafeExecuteAsync(async () =>
            {
                var user = await DB.Users
                    .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower(), token);

                ValidateUser(user);

                Rental rental = new Rental
                {
                    LicensePlate = licenseNumber,
                    PickUpDate = startDate,
                    ReturnDate = returnDate,
                    UserID = user.UserID
                };

                DB.Rentals.Add(rental);
                await DB.SaveChangesAsync(token);

                await DB.Entry(rental).ReloadAsync(token);
                return rental.RentalID;

            }, token);
        }



        // =====================================================================
        // UPDATE RETURN DATE
        // =====================================================================

        public void UpdateCarReturn(int rentalID)
        {
            ValidateId(rentalID);

            var rental = DB.Rentals.Find(rentalID);
            if (rental == null)
                throw new InvalidOperationException("Rental not found.");

            rental.ActualReturnDate = DateTime.Today;

            DB.Entry(rental).State = EntityState.Modified;
            DB.SaveChanges();
        }

        public async Task UpdateCarReturnAsync(int rentalID, CancellationToken token = default)
        {
            ValidateId(rentalID);

            await SafeExecuteAsync(async () =>
            {
                var rental = await DB.Rentals.FindAsync(token, rentalID);
                if (rental == null)
                    throw new InvalidOperationException("Rental not found.");

                rental.ActualReturnDate = DateTime.Today;

                DB.Entry(rental).State = EntityState.Modified;
                await DB.SaveChangesAsync(token);

            }, token);
        }
    }
}

