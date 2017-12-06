using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreManager.Models
{
    public class FilterTimeModel
    {
        public long FilterId { set; get; }
        public int PairPassengers { set; get; }
        public long? Price { set; get; }
        public string PriceString { set; get; }
        public DateTime? Time { set; get; }
        public int TimeHour { set; get; }
        public int TimeMinute { set; get; }
        public string TimeString { set; get; }
        public bool IsManual { set; get; }
    }
}
