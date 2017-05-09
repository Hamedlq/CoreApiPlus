using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreManager.Models.RouteModels;

namespace CoreManager.Models
{
    public class StationRouteModel
    {
        public long StRouteId { set; get; }
        public long SrcStId { set; get; }
        public string SrcStAdd { set; get; }
        public string SrcStLat { set; get; }
        public string SrcStLng { set; get; }
        public long DstStId { set; get; }
        public string DstStAdd { set; get; }
        public string DstStLat { set; get; }
        public string DstStLng { set; get; }
        public string StRoutePrice { set; get; }
        public string StRouteDuration { set; get; }
    }
}
