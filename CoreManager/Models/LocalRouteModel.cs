using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreManager.Models.TrafficAddress;

namespace CoreManager.Models
{
    public class LocalRouteModel
    {
        public LocalRouteModel()
        {
            SrcPoint = new Point();
            DstPoint = new Point();
            PathRoute=new PathPoint();
        }

        public Guid RouteUId { set; get; }
        public LocalRouteTypes LocalRouteType { set; get; }
        public Point SrcPoint { set; get; }
        public Point DstPoint { set; get; }
        public string RouteStartTime { set; get; }
        public PathPoint PathRoute { set; get; }

    }
}
