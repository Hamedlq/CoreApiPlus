using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreManager.Models
{
    public class ActiveTripModel
    {
        public string DriverName { set; get; }
        public string DriverFamily { set; get; }
        public string DriverMobile { set; get; }
        public string TripTime { set; get; }
        public string TripOrigin { set; get; }
        public string TripDest { set; get; }
        public string EmptySeats { set; get; }
        public string PassName { set; get; }
        public string PassFamily { set; get; }
        public string PassMobile { set; get; }
    }
}
