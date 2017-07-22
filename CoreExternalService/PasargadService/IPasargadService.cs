using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreExternalService.Models;

namespace CoreExternalService
{
    public interface IPasargadService
    {
        PasargadPayModel RequestPayment(double chargeValue, long tripId, DateTime tripTime);
        int VerifyTransaction(string authority, int amount, out long refId);
        //bool VerifyPayment(PayReq payreq, PasargadPaymentModel model);
        PasargadPayResponse CheckPayment(long payreqPayReqId, string payreqPayReqRefId, string payreqPayReqCreateTime);
        PasargadPayResponse VerifyPasargadPayment(long payreqPayReqId, double payreqPayReqValue, string invoicetime);
    }
}
