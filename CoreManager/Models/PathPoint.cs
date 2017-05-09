using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreManager.Models.TrafficAddress
{
    public class PathPoint
    {
        public PathPoint()
        {
            metadata=new MetaData();
            path=new List<Point>();
        }

        public MetaData metadata { set; get; }
        public List<Point> path { set; get; }
    }
}
