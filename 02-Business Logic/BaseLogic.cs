using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RacingHubCarRental
{
    /// <summary>
    /// Represents the basic needs for a logic class!
    /// </summary>
    public abstract class BaseLogic : IDisposable
    {

        /// <summary>
        /// Holds the database!
        /// </summary>
        protected RacingHubCarRentalEntities DB = new RacingHubCarRentalEntities();

        /// <summary>
        /// Clears database resources!
        /// </summary>
        public void Dispose()
        {
            DB.Dispose();
        }

    }
}
