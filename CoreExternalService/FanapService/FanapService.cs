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
    public class FanapService : IFanapService
    {
        private string sso_address ;
        private string client_Id ;
        private string redirect_Uri;
        private string pay_redirect_Uri;
        private string client_secret;
        private string platform_address;
        private string credit_address;
        //private string pay_invoice;
        private string token;
        public FanapService()
        {
            sso_address = "http://keylead.fanapium.com";
            client_Id = "b05741339a41cf30f58ac0e9";
            redirect_Uri = "http://ifanap.mibarim.ir/fanap/loginreturn";
            //redirect_Uri = "http://mibarimiiii.ir";
            //redirect_Uri = "http://fanap.mibarimapp.com/testapp/fanap/loginreturn";
            pay_redirect_Uri = "http://ifanap.mibarim.ir/fanap/pay/?payreqId=";
            client_secret = "2cdcc4a9";
            platform_address="http://sandbox.fanapium.com:8080";
            credit_address= "http://sandbox.fanapium.com:1031";
            //pay_invoice= "http://sandbox.fanapium.com:1031/v1/pbc/payinvoice/?invoiceId=";
            token = "f5b0c0049cfd49e78216a2230c63eeb1";
        }

        public FanapTokenResponse GetAuthenticationToken(string code)
        {
            //try
            //{
            //    WebRequest tRequest;
            //    tRequest = WebRequest.Create(sso_address+ "/oauth2/token/");
            //    tRequest.Method = "post";
            //    //tRequest.ContentType = " application/x-www-form-urlencoded;charset=UTF-8";
            //    string postData = "grant_type=authorization_code&code=" + code + "&client_id=" + client_Id +
            //                      "&client_secret=" + client_secret;//+"&redirect_uri="+redirect_Uri;
            //    Console.WriteLine(postData);
            //    Byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            //    tRequest.ContentLength = byteArray.Length;

            //    Stream dataStream = tRequest.GetRequestStream();
            //    dataStream.Write(byteArray, 0, byteArray.Length);
            //    dataStream.Close();

            //    WebResponse tResponse = tRequest.GetResponse();

            //    dataStream = tResponse.GetResponseStream();

            //    StreamReader tReader = new StreamReader(dataStream);

            //    String sResponseFromServer = tReader.ReadToEnd();


            //    tReader.Close();
            //    dataStream.Close();
            //    tResponse.Close();
            //    JavaScriptSerializer js = new JavaScriptSerializer();
            //    var tokenResponse = js.Deserialize<FanapTokenResponse>(sResponseFromServer);
            //    return tokenResponse;
            //}
            //catch (Exception e)
            //{
            //    var tokenResponse =new FanapTokenResponse();
            //    tokenResponse.error = e.ToString();
            //    return tokenResponse;
            //}


            var getTokenUrl = sso_address + "/oauth2/";
            var client = new RestClient(getTokenUrl);
            var restRequest = new RestRequest("token/", Method.POST);
            restRequest.AddParameter("grant_type", "authorization_code");
            restRequest.AddParameter("code", code);
            restRequest.AddParameter("redirect_uri", redirect_Uri);
            restRequest.AddParameter("client_id", client_Id);
            restRequest.AddParameter("client_secret", client_secret);
            IRestResponse response = client.Execute(restRequest);
            JavaScriptSerializer js = new JavaScriptSerializer();
            var tokenResponse = js.Deserialize<FanapTokenResponse>(response.Content);
            return tokenResponse;
        }

        public FanapPlatformRegisterResponse RegisterUserToFanapPlatform(string nickname)
        {
            var getTokenUrl = platform_address + "/nzh/";
            var client = new RestClient(getTokenUrl);
            var restRequest = new RestRequest("getUserBusiness/", Method.GET);
            restRequest.AddHeader("_token_", token);
            restRequest.AddHeader("_token_issuer_", "1");
            //restRequest.AddParameter("nickname", nickname);
            IRestResponse response = client.Execute(restRequest);
            JavaScriptSerializer js = new JavaScriptSerializer();
            var tokenResponse = js.Deserialize<FanapPlatformRegisterResponse>(response.Content);
            return tokenResponse;
        }

        public FanapUserInfo GetFanapUser(string usertoken)
        {
            var getTokenUrl = platform_address + "/nzh/";
            var client = new RestClient(getTokenUrl);
            var restRequest = new RestRequest("getUserProfile/", Method.GET);
            restRequest.AddHeader("_token_", usertoken);
            restRequest.AddHeader("_token_issuer_", "1");
            //restRequest.AddParameter("nickname", nickname);
            IRestResponse response = client.Execute(restRequest);
            JavaScriptSerializer js = new JavaScriptSerializer();
            var tokenResponse = js.Deserialize<FanapResult>(response.Content);

            return tokenResponse.result;
        }

        public FanapUserInfo RegisterWithSso(string userToken,string nickname)
        {
            var registerUrl = platform_address + "/aut/";
            var client = new RestClient(registerUrl);
            var restRequest = new RestRequest("registerWithSSO/", Method.GET);
            restRequest.AddHeader("_token_", userToken);
            restRequest.AddHeader("_token_issuer_", "1");
            restRequest.AddParameter("nickname", nickname);
            IRestResponse response = client.Execute(restRequest);
            JavaScriptSerializer js = new JavaScriptSerializer();
            var userInfoResponse = js.Deserialize<FanapPlatformRegisterResponse>(response.Content);
            return userInfoResponse.result;
        }

        public FanapBusiness getBusiness(string fanapUserAccessToken)
        {
            var registerUrl = platform_address + "/nzh/biz/";
            var client = new RestClient(registerUrl);
            var restRequest = new RestRequest("getBusiness/", Method.GET);
            restRequest.AddHeader("_token_", fanapUserAccessToken);
            restRequest.AddHeader("_token_issuer_", "1");
            IRestResponse response = client.Execute(restRequest);
            JavaScriptSerializer js = new JavaScriptSerializer();
            var userInfoResponse = js.Deserialize<FanapBusiness>(response.Content);
            return userInfoResponse;
        }

        public string GetOneTimeToken()
        {
            var registerUrl = platform_address + "/nzh/";
            var client = new RestClient(registerUrl);
            var restRequest = new RestRequest("ott/", Method.POST);
            restRequest.AddHeader("_token_", token);
            restRequest.AddHeader("_token_issuer_", "1");
            IRestResponse response = client.Execute(restRequest);
            JavaScriptSerializer js = new JavaScriptSerializer();
            var userInfoResponse = js.Deserialize<FanapOttResult>(response.Content);
            return userInfoResponse.ott;
        }

        public string IssueInvoice(long? tripPassPrice, long? fuserId, string ott,string description)
        {
            var registerUrl = platform_address + "/nzh/biz/";
            var client = new RestClient(registerUrl);
            var restRequest = new RestRequest("issueInvoice/", Method.POST);
            restRequest.AddHeader("_token_", token);
            restRequest.AddHeader("_token_issuer_", "1");
            restRequest.AddHeader("_ott_",ott);
            restRequest.AddParameter("redirectURL", pay_redirect_Uri);
            restRequest.AddParameter("userId", fuserId);
            restRequest.AddParameter("productId[]", 0);
            restRequest.AddParameter("price[]", tripPassPrice*10);
            restRequest.AddParameter("productDescription[]", description);
            restRequest.AddParameter("quantity[]", 1);
            restRequest.AddParameter("guildCode", "TRANSPORTATION_GUILD");
            restRequest.AddParameter("addressId", 0);
            restRequest.AddParameter("preferredTaxRate", 0);
            IRestResponse response = client.Execute(restRequest);
            JavaScriptSerializer js = new JavaScriptSerializer();
            var res= js.Deserialize<FanapInvoiceResult>(response.Content);
            return res.result.id.ToString();
        }

        public FanapPayment GetPaymentResult(string invoiceId)
        {
            var registerUrl = platform_address + "/nzh/biz/";
            var client = new RestClient(registerUrl);
            var restRequest = new RestRequest("getInvoiceList/", Method.POST);
            restRequest.AddHeader("_token_", token);
            restRequest.AddHeader("_token_issuer_", "1");
            restRequest.AddParameter("size",1);
            restRequest.AddParameter("id", invoiceId);
            restRequest.AddParameter("firstId", 0);
            IRestResponse response = client.Execute(restRequest);
            JavaScriptSerializer js = new JavaScriptSerializer();
            var res = js.Deserialize<FanapPaymentResult>(response.Content);
            return res.result[0];
        }

        public bool CloseInvoice(string payreqPayReqRefId)
        {
            var registerUrl = platform_address + "/nzh/biz/";
            var client = new RestClient(registerUrl);
            var restRequest = new RestRequest("closeInvoice/?id="+ payreqPayReqRefId, Method.POST);
            restRequest.AddHeader("_token_", token);
            restRequest.AddHeader("_token_issuer_", "1");
            IRestResponse response = client.Execute(restRequest);
            return true;
        }
    }
}
