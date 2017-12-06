using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreManager.Models
{
    public class NotifReqModel
    {
        public string Mobile { set; get; }
        public int NotifType { set; get; }
        public bool IsNewRouteSuggest { set; get; }
        public bool IsNewMessage { set; get; }
    }
}
