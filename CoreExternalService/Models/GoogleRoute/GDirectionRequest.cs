using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreExternalService.Models
{
    public class GDirectionRequest
    {
        public GDirectionRequest()
        {
            Src=new Point();
            Dst=new Point();
            WayPoints=new List<Point>();
        }

        public Point Src { set; get; }
        public Point Dst { set; get; }
        public List<Point> WayPoints { set; get; }

    }
}
