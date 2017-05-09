using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreManager.Models
{
    public class NotificationModel
    {
        public long SuggestRouteRequestId { set; get; }
        public long MessageRouteRequestId { set; get; }
        public bool IsNewRouteSuggest { set; get; }
        public bool IsNewMessage { set; get; }
    }
}
