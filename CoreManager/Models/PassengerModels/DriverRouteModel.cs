using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreManager.Models.TrafficAddress;

namespace CoreManager.Models
{
    public class DriverRouteModel
    {
        public long TripId { set; get; }
        public long DriverRouteId { set; get; }
        public string TimingString { set; get; }
        public int TimingHour { set; get; }
        public int TimingMin { set; get; }
        public string PricingString { set; get; }
        public string CarString { set; get; }
        public string SrcMainAddress { set; get; }
        public string SrcAddress { set; get; }
        public string SrcLink { set; get; }
        public string SrcLat { set; get; }
        public string SrcLng { set; get; }
        public string DstAddress { set; get; }
        public string DstLink { set; get; }
        public string DstLat { set; get; }
        public string DstLng { set; get; }
        public short FilledSeats { set; get; }
        public short CarSeats { set; get; }
        public bool HasTrip { set; get; }
        public int TripState { set; get; }
    }
}