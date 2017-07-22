using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreManager.Models
{
    public class PasargadPayModel
    {
        public string BankLink { set; get; }
        public string MerchantCode { set; get; }
        public string TerminalCode { set; get; }
        public string InvoiceNumber { set; get; }
        public string InvoiceDate { set; get; }
        public string Amount { set; get; }
        public string RedirectAddress { set; get; }
        public string Action { set; get; }
        public string TimeStamp { set; get; }
        public string Sign { set; get; }
        public long ReqId { set; get; }
    }
}
