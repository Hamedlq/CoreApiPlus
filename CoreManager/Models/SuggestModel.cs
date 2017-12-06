using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreManager.Models
{
    public class SuggestModel
    {
        public long FilterId { set; get; }
        public long SrcStationId { set; get; }
        public string SrcStation { set; get; }
        public string SrcStLat { set; get; }
        public string SrcStLng { set; get; }
        public long DstStationId { set; get; }
        public string DstStation { set; get; }
        public string DstStLat { set; get; }
        public string DstStLng { set; get; }
        public DateTime? Time { set; get; }
        public int TimeHour { set; get; }
        public int TimeMinute { set; get; }
        public long Price { set; get; }
        public string PriceString { set; get; }
        public int PairPassengers { set; get; }

    }
}
