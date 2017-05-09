using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreManager.Models
{
    public class RouteEventModel
    {
        public long EventId { set; get; }
        public EventTypes EventType { set; get; }
        public int RouteRequestId { set; get; }
        public string SrcGAddress { set; get; }
        public string SrcDetailAddress { set; get; }
        public string SrcLatitude { set; get; }
        public string SrcLongitude { set; get; }
        public string DstGAddress { set; get; }
        public string DstDetailAddress { set; get; }
        public string DstLatitude { set; get; }
        public string DstLongitude { set; get; }
        public bool IsDrive { set; get; }
        public PricingOptions PriceOption { set; get; }
        public decimal CostMinMax { set; get; }
        public long RecommendPathId { set; get; }
        public int RouteRequestState { set; get; }

    }
}
