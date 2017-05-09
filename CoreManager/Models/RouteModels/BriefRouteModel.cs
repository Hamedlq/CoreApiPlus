using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreManager.Models.TrafficAddress;

namespace CoreManager.Models
{
    public class BriefRouteModel
    {
        public BriefRouteModel()
        {
            PathRoute=new PathPoint();
        }

        public long RouteId { set; get; }
        public string Name { set; get; }
        public string Family { set; get; }
        public Guid? UserImageId { set; get; }
        public string  TimingString{ set; get; }
        public string  PricingString{ set; get; }
        public string  CarString{ set; get; }
        public string SrcAddress { set; get; }
        public string SrcDistance { set; get; }
        public string SrcLatitude { set; get; }
        public string SrcLongitude { set; get; }
        public string DstAddress { set; get; }
        public string DstDistance { set; get; }
        public string DstLatitude { set; get; }
        public string DstLongitude { set; get; }
        public int AccompanyCount { set; get; }
        public bool IsDrive { set; get; }
        public bool IsSuggestSeen { set; get; }
        public PathPoint PathRoute { set; get; }

    }
}
