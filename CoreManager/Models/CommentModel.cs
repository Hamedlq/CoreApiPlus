using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreManager.Models
{
    public class CommentModel
    {
        public int GroupId { set; get; }
        public string Mobile { set; get; }
        public long CommentId { set; get; }
        public string NameFamily { set; get; }
        public string TimingString { set; get; }
        public string Comment { set; get; }
        public bool IsDeletable { set; get; }
        public string UserImageId { set; get; }
        //public byte[] UserPic { set; get; }

    }
}
