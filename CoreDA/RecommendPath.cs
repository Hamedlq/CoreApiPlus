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
    
    public partial class RecommendPath
    {
        public long RecommendPathId { get; set; }
        public int RecommendPathIndex { get; set; }
        public decimal RecommendSrcLat { get; set; }
        public decimal RecommendSrcLng { get; set; }
        public System.Data.Entity.Spatial.DbGeography RecommendSrcGeo { get; set; }
        public decimal RecommendDstLat { get; set; }
        public decimal RecommendDstLng { get; set; }
        public System.Data.Entity.Spatial.DbGeography RecommendDstGeo { get; set; }
    }
}
