using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreManager.Models
{
    public class GasScore
    {
        public long DistanceRouted { set; get; }
        public long Payment { set; get; }
        public int RouteCount { set; get; }
    }
}
