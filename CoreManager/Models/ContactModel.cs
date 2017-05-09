using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreManager.Models
{
    public class ContactModel
    {
        public long ContactId { set; get; }
        public string Name { set; get; }
        public string Family { set; get; }
        public Gender Gender { set; get; }
        public string LastMsgTime { set; get; }
        public string LastMsg { set; get; }
        public int IsSupport { set; get; }
        public int IsDriver { set; get; }
        public int IsRideAccepted { set; get; }
        public int IsPassengerAccepted { set; get; }
        public Guid? UserImageId { set; get; }
        public byte[] UserPic { set; get; }
        public string AboutUser { set; get; }
        public int NewChats { set; get; }
    }
}
