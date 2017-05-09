using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CoreManager.Models
{
    public class NotifModel
    {
        private String _title;
        private String _body;
        public String Title {
            set { _title = value; }
            get { return _title; } 
        }
        public String EncodedTitle
        {
            get { return HttpUtility.UrlEncode(_title); }
        }
        public String Body
        {
            set { _body = value; }
            get { return _body; }
        }
        public String EncodedBody
        {
            get { return HttpUtility.UrlEncode(_body); }
        }
        public String Action{ set; get; }
        public int RequestCode { set; get; }
        public int NotificationId { set; get; }
        public int Tab { set; get; }
    }
}
