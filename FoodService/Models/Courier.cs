using System;
using System.Collections.Generic;

namespace FoodService.Models
{
    public partial class Courier
    {
        public Courier()
        {
            Orders = new HashSet<Order>();
        }

        public int Id { get; set; }
        public int UserId { get; set; }
        public string Status { get; set; } = null!;
        public double? Longitude { get; set; }
        public double? Latitude { get; set; }

        public virtual User User { get; set; } = null!;
        public virtual ICollection<Order> Orders { get; set; }
    }
}
