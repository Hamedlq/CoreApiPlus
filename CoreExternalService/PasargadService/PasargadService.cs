using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Xml;
using CoreExternalService.Models;
using RestSharp;

namespace CoreExternalService
{
    public class PasargadService : IPasargadService
    {
        private string MerchantCode;
        private string TerminalCode;
        private string RedirectAddress;
        private string PrivateKey;

        public PasargadService()
        {
            AppSettingsReader appRead = new AppSettingsReader();

            //MerchantCode = ConfigurationManager.AppSettings["ZarinPalMerchantKey"];
            MerchantCode = appRead.GetValue("MerchantCode", typeof(string)).ToString();
            TerminalCode = appRead.GetValue("TerminalCode", typeof(string)).ToString();
            RedirectAddress = appRead.GetValue("RedirectAddress", typeof(string)).ToString() + "VerifyPasargad";
            PrivateKey =
                "<RSAKeyValue><Modulus>xuFudk2RThxg+/9rYX4bkImQ/rk/33Pdno8GJlmgGti4/ar+Lep7nYn6E2SxTK4hDm+SHe0XsvlK5LMbf2B+feOX+9zHILs/hoekk2IJ/fakGcECRFHbkt8RzQWrzNfX98Jmtu1iz1X2pLYKDlYZPwNd6aOd8O6HizGd9MSzfIU=</Modulus><Exponent>AQAB</Exponent><P>5h7DVGcO8vQBobFypI7eYQg/fHDmMdWA+UW+NTkb/7wCj4Yd9HLelq4Mxly7Gmva8iXyJon67VRFa3rtfPaMcw==</P><Q>3T9FRQ9zhEa77kXXqIyWyV04OFA8zIbImScnoh+feunuioHecCHx4xE31L/1O7gwnXdXvtNuXF0yBlcVoaTNJw==</Q><DP>tDexZ59SYMjhojzy+Jb+52TrO0y7qpl3aUDKZqo0GEKoirhRK0jus3jZflvPGDERhgRbsPzsbANMXpEl/nCjqw==</DP><DQ>x9cqfmKieMxbW7GCRiAW4vNsoJD5GdR0xMF1Lx9ZMfCzIjCD9szya6NVxrlMjRCl+NWfUCIyAQO897UZONRe6Q==</DQ><InverseQ>PhufemBs25+y1HTFwZt+6Aap/iE7XfxlMWRxksjTJG6gOhTLIzihuUrhxx9pMHlqEkGQ6VhMGorWN4K+7HVf1A==</InverseQ><D>t/WVg4BEQ4gkfXPJE6jePlfA8pzP5BT8jcml2ptUaQDGPH2KF1apeRNDaeTdyxvWH9A7y8qe/UFycRDrAmtzE8WLVOSTEZHBC1Ntyge+jHLADIxhQj5FortuXRiQZaNavqXG2avpPzDbV94Qab50RF9xcVE9xzpFeocSUjdcZBE=</D></RSAKeyValue>";
        }

        public PasargadPayModel RequestPayment(double chargeValue, long tripId, DateTime tripTime)
        {
            var Amount = chargeValue*10; //convert to rial
            var pasargadPayModel = new PasargadPayModel();
            pasargadPayModel.MerchantCode = MerchantCode;
            pasargadPayModel.TerminalCode = TerminalCode;
            pasargadPayModel.Amount = Amount.ToString();
            pasargadPayModel.RedirectAddress = RedirectAddress;

            string timeStamp = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            string invoiceDate = tripTime.ToString("yyyy/MM/dd HH:mm");
            string invoiceNumber = tripId.ToString();
            string action = "1003";
            pasargadPayModel.InvoiceNumber = invoiceNumber;
            pasargadPayModel.InvoiceDate = invoiceDate;
            pasargadPayModel.TimeStamp = timeStamp;
            pasargadPayModel.Action = action;

            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(PrivateKey);

            string data = "#" + MerchantCode + "#" + TerminalCode + "#" + invoiceNumber +
                          "#" + invoiceDate + "#" + Amount + "#" + RedirectAddress +
                          "#" + action + "#" + timeStamp + "#";

            byte[] signedData = rsa.SignData(Encoding.UTF8.GetBytes(data), new
                SHA1CryptoServiceProvider());

            string signedString = Convert.ToBase64String(signedData);
            pasargadPayModel.Sign = signedString;
            pasargadPayModel.BankLink = "https://pep.shaparak.ir/gateway.aspx";

            return pasargadPayModel;
        }

        public int VerifyTransaction(string authority, int amount, out long refId)
        {
            /*int Status;
            System.Net.ServicePointManager.Expect100Continue = false;
            ZarinPalServiceReference.PaymentGatewayImplementationServicePortTypeClient zp = new ZarinPalServiceReference.PaymentGatewayImplementationServicePortTypeClient();

            Status = zp.PaymentVerification(ZARINPAL_MERCHANT_CODE, authority, amount, out refId);*/
            refId = 1;
            return 1;
        }

        public PasargadPayResponse CheckPayment(long invoiceNumber, string transactionReferenceID, string invoiceDate)
        {
            string strXML = ReadPaymentResult(transactionReferenceID);
            var res= new PasargadPayResponse();
            //در صورتی که تراکنشی انجام نشده باشد فایلی از بانک برگشت داده نمی شود  
            //دستور شزطی زیر جهت اعلام نتیجه به کاربر است
            if (strXML == "")
            {
                res.Result = false;
                res.TraceNumber="تراکنش  انجام نشد ";
            }
            else
            {
                XmlDocument oXml = new XmlDocument();
                oXml.LoadXml(strXML);
                //xmlResult.Document = oXml;
                XmlElement oElResult = (XmlElement) oXml.SelectSingleNode("//result"); //نتیجه تراکنش
                XmlElement _oElTraceNumber = (XmlElement) oXml.SelectSingleNode("//traceNumber"); //شماره پیگیری
                XmlElement _TXNreferenceNumber = (XmlElement) oXml.SelectSingleNode("//referenceNumber"); //شماره ارجاع
                res.Result = Boolean.Parse(oElResult.InnerText);
                if (res.Result)
                {
                    res.TraceNumber = _oElTraceNumber.InnerText;
                    res.ReferenceNumber = _TXNreferenceNumber.InnerText;
                }
            }
            return res;
        }

        public PasargadPayResponse VerifyPasargadPayment(long payreqPayReqId, double payreqPayReqValue, string invoicetime)
        {
            var res = new PasargadPayResponse();
            var amount = payreqPayReqValue * 10; //convert to rial
            string timeStamp = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            string invoiceDate = invoicetime;
            string invoiceNumber = payreqPayReqId.ToString();

            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(PrivateKey);

            string data = "#" + MerchantCode + "#" + TerminalCode + "#" + invoiceNumber +
          "#" + invoiceDate + "#" + amount + "#" + timeStamp + "#";

            byte[] signedData = rsa.SignData(Encoding.UTF8.GetBytes(data), new
            SHA1CryptoServiceProvider());

            string signedString = Convert.ToBase64String(signedData);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://pep.shaparak.ir/VerifyPayment.aspx");
            string text = "InvoiceNumber=" + invoiceNumber + "&InvoiceDate=" +
                        invoiceDate + "&MerchantCode=" + MerchantCode + "&TerminalCode=" +
                        TerminalCode + "&Amount=" + amount + "&TimeStamp=" + timeStamp + "&Sign=" + signedString;
            byte[] textArray = Encoding.UTF8.GetBytes(text);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = textArray.Length;
            request.GetRequestStream().Write(textArray, 0, textArray.Length);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string result = reader.ReadToEnd();

            if (result == "")
            {
                res.Result = false;
                res.ResultMessage = "تراکنش  انجام نشد ";
            }
            else
            {
                XmlDocument oXml = new XmlDocument();
                oXml.LoadXml(result);
                //xmlResult.DocumentContent = result;
                XmlElement oElResult = (XmlElement)oXml.SelectSingleNode("//result"); //نتیجه تراکنش
                XmlElement msg = (XmlElement)oXml.SelectSingleNode("//resultMessage"); //شماره پیگیری
                res.ResultMessage=msg.InnerText;
                res.Result = Boolean.Parse(oElResult.InnerText);
            }
            return res;
        }

        private string ReadPaymentResult(string tref)
        {
            HttpWebRequest request =
                (HttpWebRequest) WebRequest.Create("https://pep.shaparak.ir/CheckTransactionResult.aspx");
            string text = "invoiceUID=" + tref;
            byte[] textArray = Encoding.UTF8.GetBytes(text);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = textArray.Length;
            request.GetRequestStream().Write(textArray, 0, textArray.Length);
            HttpWebResponse response = (HttpWebResponse) request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string result = reader.ReadToEnd();
            return result;
        }
    }
}