using System;
using System.Collections.Generic;

namespace UserService.Models
{
    public partial class Courier
    {
        public Courier()
        {
            Orders = new HashSet<Order>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public bool Status { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
    }
}
