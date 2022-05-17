using System;
using System.Collections.Generic;

namespace UserService.Models
{
    public partial class OrderDetail
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int FoodId { get; set; }
        public int CourierId { get; set; }

        public virtual Courier Courier { get; set; } = null!;
        public virtual Food Food { get; set; } = null!;
        public virtual Order Order { get; set; } = null!;
    }
}
