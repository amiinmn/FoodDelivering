using System;
using System.Collections.Generic;

namespace OrderService.Models
{
    public partial class Order
    {
        public Order()
        {
            OrderDetails = new HashSet<OrderDetail>();
        }

        public int Id { get; set; }
        public int UserId { get; set; }
        public string Code { get; set; } = null!;
        public int CourierId { get; set; }
        public double? Longitude { get; set; }
        public double? Latitude { get; set; }

        public virtual Courier Courier { get; set; } = null!;
        public virtual User User { get; set; } = null!;
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
