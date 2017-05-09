using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using CoreExternalService.Models;
using RestSharp;

namespace CoreExternalService
{
    public class ZarinPalService : IZarinPalService
    {
        private string ZARINPAL_MERCHANT_CODE;// = "99df7a98-6797-11e6-ba96-000c295eb8fc";
        public ZarinPalService()
        {
            ZARINPAL_MERCHANT_CODE = ConfigurationManager.AppSettings["ZarinPalMerchantKey"];
        }

        public int RequestAuthoruty(decimal chargeValue, string desc, out string authority)
        {
            System.Net.ServicePointManager.Expect100Continue = false;
            ZarinPalServiceReference.PaymentGatewayImplementationServicePortTypeClient zp = new ZarinPalServiceReference.PaymentGatewayImplementationServicePortTypeClient();
            string Authority;
            int status = zp.PaymentRequest(ZARINPAL_MERCHANT_CODE, int.Parse(chargeValue.ToString()), desc, "someEmail@mibarim.ir", "09111111111", "http://mibarimapp.com/verify", out Authority);
            authority = Authority;
            return status;
        }

        public int VerifyTransaction(string authority, int amount, out long refId)
        {
            int Status;
            System.Net.ServicePointManager.Expect100Continue = false;
            ZarinPalServiceReference.PaymentGatewayImplementationServicePortTypeClient zp = new ZarinPalServiceReference.PaymentGatewayImplementationServicePortTypeClient();

            Status = zp.PaymentVerification(ZARINPAL_MERCHANT_CODE, authority, amount, out refId);
            return Status;
        }
    }
}
