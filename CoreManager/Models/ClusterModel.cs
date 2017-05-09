using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreManager.Models.TrafficAddress;

namespace CoreManager.Models
{
    public class ClusterModel
    {
        public ClusterModel()
        {
            SrcPoint = new Point();
            DstPoint = new Point();
            PathRoute=new PathPoint();
        }

        public string NameFamily { set; get; }
        public LocalRouteTypes LocalRouteType { set; get; }
        public Point SrcPoint { set; get; }
        public Point DstPoint { set; get; }
        public string RouteStartTime { set; get; }
        public PathPoint PathRoute { set; get; }

    }
}
