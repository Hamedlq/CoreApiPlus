using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreManager.Models
{
    public class EventAttendeeModel
    {
        public string Name { set; get; }
        public string Family { set; get; }
        public string Mobile { set; get; }
        public string EventName { set; get; }
        public int EventAttendeeNo { set; get; }
        public string Latitude { set; get; }
        public string Longitude { set; get; }
    }
}
