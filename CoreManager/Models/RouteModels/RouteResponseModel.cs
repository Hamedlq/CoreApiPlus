using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreManager.Models.RouteModels;

namespace CoreManager.Models
{
    public class RouteResponseModel 
    {
        public int RouteId { set; get; }
        public Guid? RouteUId { set; get; }
        public string SrcAddress { set; get; }
        public string SrcLatitude { set; get; }
        public string SrcLongitude { set; get; }
        public string DstAddress { set; get; }
        public string DstLatitude { set; get; }
        public string DstLongitude { set; get; }
        public int AccompanyCount { set; get; }
        public bool IsDrive { set; get; }
        public string TimingString { set; get; }
        public string DateString { set; get; }
        public string PricingString { set; get; }
        public string CarString { set; get; }
        public int SuggestCount { set; get; }
        public int NewSuggestCount { set; get; }
        public string RouteRequestState { set; get; }
        public bool? Sat { set; get; }
        public bool? Sun { set; get; }
        public bool? Mon { set; get; }
        public bool? Tue { set; get; }
        public bool? Wed { set; get; }
        public bool? Thu { set; get; }
        public bool? Fri { set; get; }
        /*public List<RouteGroupModel> GroupRoutes { set; get; }
        public List<GroupModel> SuggestGroups { set; get; }*/
        public List<BriefRouteModel> SuggestRoutes { set; get; }
    }
}
