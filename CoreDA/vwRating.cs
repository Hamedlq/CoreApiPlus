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
    
    public partial class vwRating
    {
        public string Family { get; set; }
        public long FellowUserId { get; set; }
        public long RaterUserId { get; set; }
        public Nullable<short> Rate { get; set; }
        public long TripId { get; set; }
        public System.DateTime RateCreateTime { get; set; }
        public long RateId { get; set; }
        public Nullable<System.Guid> UserImageId { get; set; }
        public Nullable<int> Gender { get; set; }
        public Nullable<System.Guid> UserUId { get; set; }
        public string Name { get; set; }
        public string RateDescription { get; set; }
    }
}
