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
    
    public partial class RouteSuggest
    {
        public long RouteSuggestId { get; set; }
        public System.DateTime RSuggestCreateTime { get; set; }
        public long SuggestRouteRequestId { get; set; }
        public long SelfRouteRequestId { get; set; }
        public double SSrcDistance { get; set; }
        public double SDstDistance { get; set; }
        public bool IsSuggestDeleted { get; set; }
        public bool IsSuggestSent { get; set; }
        public bool IsSuggestSeen { get; set; }
        public bool IsSuggestAccepted { get; set; }
        public bool IsSuggestRejected { get; set; }
    }
}