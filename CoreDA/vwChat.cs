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
    
    public partial class vwChat
    {
        public System.DateTime ChatCreateTime { get; set; }
        public bool ChatIsDeleted { get; set; }
        public int ChatUserId { get; set; }
        public string ChatTxt { get; set; }
        public string Family { get; set; }
        public string Name { get; set; }
        public Nullable<int> Gender { get; set; }
        public long ContactId { get; set; }
        public long ChatId { get; set; }
        public Nullable<System.Guid> UserImageId { get; set; }
    }
}
