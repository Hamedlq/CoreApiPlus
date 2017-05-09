using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreManager.Models
{
    public class DiscountModel
    {
        public string DiscountCode { set; get; }
        public string DiscountTitle { set; get; }
        public string DiscountStateString { set; get; }
        public DiscountStates DiscountState { set; get; }
    }
}
