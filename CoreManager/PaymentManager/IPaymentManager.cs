using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreManager.Models;

namespace CoreManager.PaymentManager
{
    public interface IPaymentManager
    {
        PaymentDetailModel ChargeAccount(int userId, int chargeValue, string userNameFamilyString);
        PaymentDetailModel VerifyPayment(PaymentDetailModel model);
        PasargadResponseModel VerifyPasargadPayment(PasargadPaymentModel model);
        PasargadPayModel ChargePasargad(int userId, long chargeAmount, string userNameFamilyString,long tripId,DateTime tripTime);
        PasargadPayModel Getpayment(long modelReqId);
    }
}
