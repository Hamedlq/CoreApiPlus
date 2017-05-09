using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreManager.Models
{
    public class PaymentDetailModel
    {
        public int State { set; get; }
        public string BankLink { set; get; }
        public string Authority { set; get; }
        public long RefId { set; get; }
        public long ReqId { set; get; }
    }
}
