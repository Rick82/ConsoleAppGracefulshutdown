using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBusMessages
{
    public sealed class PlaceOrderRequestMessage
    {
        public string Name { get; set; }
        public DateTime DateTimeCreated { get; set; }
        public List<OrderItemDTO> OrderItems { get; set; }
    }

    public sealed class OrderItemDTO
    {
        public int Id { get; set; }
        public int ItemQty { get; set; }
        public double Total { get; set; }
    }
}
