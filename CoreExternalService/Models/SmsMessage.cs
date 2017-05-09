using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreExternalService.Models
{
    public class SmsMessage
    {
        public long Id { set; get; }
        public string LineNumber { set; get; }
        public string SMSMessageBody { set; get; }
        public string MobileNo { set; get; }
        public DateTime ReceiveDateTime { set; get; }
        public string TypeOfMessage { set; get; }
    }
}
