using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreManager.Models
{
    public class PayModel
    {
        public long TripId { set; get; }
        public string DiscountCode { set; get; }
        public long SeatPrice { set; get; }
        public long Credit { set; get; }
        public long ChargeAmount { set; get; }
    }
}
