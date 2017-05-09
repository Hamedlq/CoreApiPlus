using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreManager.Models.TrafficAddress;

namespace CoreManager.Models
{
    public class TripResponse
    {
        public TripResponse()
        {
            PathRoute=new PathPoint();
            SrcPoint=new Point();
            DstPoint=new Point();
            TripRoutes = new List<TripRouteModel>();
        }

        public long TripId{ set; get; }
        public string CarInfo { set; get; }
        public Point SrcPoint { set; get; }
        public Point DstPoint { set; get; }
        public PathPoint PathRoute { set; get; }
        public List<TripRouteModel> TripRoutes { set; get; }

    }
}
