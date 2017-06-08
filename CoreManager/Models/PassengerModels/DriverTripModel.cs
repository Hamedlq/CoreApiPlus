using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreManager.Models.TrafficAddress;

namespace CoreManager.Models
{
    public class DriverTripModel
    {
        public long DriverRouteId { set; get; }
        public long TripId { set; get; }
        public string StAddress { set; get; }
        public string StLink { set; get; }
        public string StLat { set; get; }
        public string StLng { set; get; }
        public string DriverLat { set; get; }
        public string DriverLng { set; get; }
        public int FilledSeats { set; get; }
        public int TripState { set; get; }

    }
}