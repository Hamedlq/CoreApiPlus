using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreManager.Models
{
    public class StationModel
    {
        public string Name { set; get; }
        public string StLat { set; get; }
        public string StLng { set; get; }
        public long MainStationId { set; get; }

    }
}
