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
    
    public partial class TripLocation
    {
        public long TripLocationId { get; set; }
        public System.DateTime TlCreateTime { get; set; }
        public int TlUserId { get; set; }
        public long TripId { get; set; }
        public short TripState { get; set; }
        public decimal TlLat { get; set; }
        public decimal TlLng { get; set; }
        public System.Data.Entity.Spatial.DbGeography TlGeo { get; set; }
    }
}
