
namespace RacingHubCarRental
{
    using System;
    using System.Collections.Generic;
    
    public partial class ManufacturerModel
    {
        public ManufacturerModel()
        {
            this.CarModels = new HashSet<CarModel>();
        }
    
        public int ManufacturerModelID { get; set; }
        public string ManufacturerModelName { get; set; }
        public int ManufacturerID { get; set; }
    
        public static ICollection<CarModel> CarModels { get; set; }
        public virtual Manufacturer Manufacturer { get; set; }
    }
}
