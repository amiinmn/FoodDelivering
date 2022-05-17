using System;
using System.Collections.Generic;

namespace UserService.Models
{
    public partial class Courier
    {
        public Courier()
        {
            OrderDetails = new HashSet<OrderDetail>();
        }

        public int Id { get; set; }
        public string Location { get; set; } = null!;
        public bool OrderComplete { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
