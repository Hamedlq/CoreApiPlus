using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreManager.Models
{
    public class DiscountModel
    {
        public string PaymentCharge { set; get; }
        public string DiscountCode { set; get; }
        public string DiscountTitle { set; get; }
        public string DiscountStateString { set; get; }
        public long ChargeAmount { set; get; }
        public long SeatPrice { set; get; }
        public DiscountStates DiscountState { set; get; }
    }
}
