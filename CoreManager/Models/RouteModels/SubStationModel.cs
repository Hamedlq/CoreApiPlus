using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreManager.Models.RouteModels;

namespace CoreManager.Models
{
    public class SubStationModel
    {
        public long StRouteId { set; get; }
        public long StationId { set; get; }
        public string StAdd { set; get; }
        public string StLat { set; get; }
        public string StLng { set; get; }
    }
}
