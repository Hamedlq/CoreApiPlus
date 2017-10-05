using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreExternalService.Models
{
    public class FanapResult
    {
        public FanapUserInfo result { get; set; }
    }

    public class FanapOttResult
    {
        public bool hasError { get; set; }
        public int errorCode { get; set; }
        public int count { get; set; }
        public string ott { get; set; }
    }

    public class FanapInvoiceResult
    {
        public FanapInvoice result { get; set; }
    }

    public class FanapInvoice
    {
        public long id { get; set; }
        public string paymentBillNumber { get; set; }
        public string uniqueNumber { get; set; }
    }


    public class FanapPaymentResult
    {
        public List<FanapPayment> result { get; set; }
    }

    public class FanapPayment
    {
        public bool payed { get; set; }
        public bool canceled { get; set; }
    }
}