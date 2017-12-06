using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreManager.Models
{
    public class FilterModel
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
        public string TimeString { set; get; }
        public bool IsActive { set; get; }
        public bool IsAlert { set; get; }
        public bool IsMatched { set; get; }
        public short CarSeats { set; get; }
    }
}
