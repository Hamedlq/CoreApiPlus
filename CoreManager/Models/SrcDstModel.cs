using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreManager.Models
{
    public class SrcDstModel
    {
        public string SrcLat { set; get; }
        public string SrcLng { set; get; }
        public string DstLat { set; get; }
        public string DstLng { set; get; }
        public string Uuid { set; get; }
        public string WayPoints { set; get; }
        public string TaxiPriceModel { set; get; }

    }
}
