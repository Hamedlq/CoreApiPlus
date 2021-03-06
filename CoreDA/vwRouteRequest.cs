//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CoreDA
{
    using System;
    using System.Collections.Generic;
    
    public partial class vwRouteRequest
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int RouteRequestType { get; set; }
        public string SrcGAddress { get; set; }
        public string SrcDetailAddress { get; set; }
        public decimal SrcLatitude { get; set; }
        public decimal SrcLongitude { get; set; }
        public string DstGAddress { get; set; }
        public string DstDetailAddress { get; set; }
        public decimal DstLatitude { get; set; }
        public decimal DstLongitude { get; set; }
        public int AccompanyCount { get; set; }
        public long RouteRequestId { get; set; }
        public int RRPricingOption { get; set; }
        public Nullable<decimal> RRPricingMinMax { get; set; }
        public string ConfirmatedText { get; set; }
        public short RRIsConfirmed { get; set; }
        public bool IsDrive { get; set; }
        public int CarInfoId { get; set; }
        public string CarType { get; set; }
        public string CarColor { get; set; }
        public string CarPlateNo { get; set; }
        public Nullable<short> RouteRequestState { get; set; }
    }
}
