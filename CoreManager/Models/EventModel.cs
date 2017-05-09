using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreManager.Models
{
    public class EventModel
    {
        public long EventId { set; get; }
        public EventTypes EventType { set; get; }
        public DateTime EventStartTime { set; get; }
        public DateTime EventEndTime { set; get; }
        public string StartTimeString { set; get; }
        public string EndTimeString { set; get; }
        public string Conductor { set; get; }
        public string Name { set; get; }
        public string Address { set; get; }
        public string Latitude { set; get; }
        public string Longitude { set; get; }
        public string Description { set; get; }
        public string ExternalLink { set; get; }
    }
}
