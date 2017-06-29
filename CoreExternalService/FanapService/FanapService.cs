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
        private string client_secret;
        private string platform_address;
        private string token;
        public FanapService()
        {
            sso_address = "http://sandbox.fanapium.com";
            client_Id = "85504dcc9e6272b2f8ee45ae";
            redirect_Uri = "http://mibarimapp.com/coreapi/loginreturn";
            //redirect_Uri = "http://mibarimiiii.ir";
            client_secret = "134a5602";
            platform_address="http://sandbox.fanapium.com:8080";
            token = "3e78bf162cf84c3c95c250e30a1695be";
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
    }
}
