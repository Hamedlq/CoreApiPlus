using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreManager.Models.TrafficAddress;

namespace CoreManager.Models
{
    public class PathPriceResponse
    {
        public PathPriceResponse()
        {
            PathRoute=new PathPoint();
        }

        public PathPoint PathRoute { set; get; }
        public string SharedServicePrice { set; get; }
        public string PrivateServicePrice { set; get; }
        public string Tap30PathPrice { set; get; }

    }
}
