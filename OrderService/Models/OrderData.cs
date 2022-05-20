using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderService.Models
{
    public class OrderData
    {
        public int? Id { get; set; }
        public string? Code { get; set; }
        public int UserId { get; set; }
        public int CourierId { get; set; }
        public double? Longitude { get; set; }
        public double? Latitude { get; set; }

        public List<OrderDetailData> Details { get; set; }

    }
}
