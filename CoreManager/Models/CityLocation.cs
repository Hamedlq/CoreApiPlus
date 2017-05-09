using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreManager.Models
{
    public class CityLoc
    {
        public CityLoc()
        {
            CityLocationPoint=new Point();
        }

        public CityLocationTypes CityLocationType { set; get; }
        public string ShortName { set; get; }
        public string FullName { set; get; }
        public Point CityLocationPoint { set; get; }

    }
}
