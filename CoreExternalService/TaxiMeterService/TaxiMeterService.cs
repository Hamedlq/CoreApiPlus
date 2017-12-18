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
using RestSharp.Authenticators;

namespace CoreExternalService
{
    public class TaxiMeterService : ITaxiMeterService
    {
        private string _snapp_base_url;
        private string _snapp_api_url;
        private string _snapp_username;
        private string _snapp_password;
        private string _tap30_base_url;
        private string _tap30_phone_number;
        private string _tap30_password;
        private string _carpino_base_url;
        private string _carpino_username;
        private string _carpino_password;
        private string _alopeyk_base_url;
        private string _alopeyk_phone_number;

        public TaxiMeterService()
        {
            _snapp_base_url = "https://oauth-passenger.snapp.site/";
            _snapp_api_url = "https://api.snapp.site/";
            _snapp_username = ConfigurationManager.AppSettings["SnappUserName"];
            _snapp_password = ConfigurationManager.AppSettings["SnappPassword"];
            _tap30_base_url = "https://tap33.me/api/";
            _tap30_phone_number = ConfigurationManager.AppSettings["Tap30UserName"];
            _carpino_base_url = "https://api.carpino.io/";
            _carpino_username = ConfigurationManager.AppSettings["CarpinoUserName"];
            _carpino_password = ConfigurationManager.AppSettings["CarpinoPassword"];
            _alopeyk_base_url = "https://api.alopeyk.com/";
            _alopeyk_phone_number= ConfigurationManager.AppSettings["AloPeykUserName"];
        }

        public string GetSnapToken()
        {
            var snappUrl = _snapp_base_url + "/v1/";
            var client = new RestClient(snappUrl);
            var restRequest = new RestRequest("auth/", Method.POST);
            restRequest.AddParameter("username", _snapp_username);
            restRequest.AddParameter("password", _snapp_password);
            restRequest.AddParameter("grant_type", "password");
            restRequest.AddParameter("client_id", "android_293ladfa12938176yfgsndf");
            restRequest.AddParameter("client_secret", "as;dfh98129-9111.*(U)jsflsdf");
            IRestResponse response = client.Execute(restRequest);
            JavaScriptSerializer js = new JavaScriptSerializer();
            var res = js.Deserialize<SnappAuthResponse>(response.Content);
            return res.access_token;
        }

        public string SendTap30TokenSms()
        {
            var tap30Url = _tap30_base_url + "/v2/";
            var client = new RestClient(tap30Url);
            var restRequest = new RestRequest("user/", Method.POST);
            restRequest.AddJsonBody(new
            {
                credential=new
                {
                    phoneNumber= _tap30_phone_number,
                    role= "PASSENGER"
                }
            });
            IRestResponse response = client.Execute(restRequest);
            return "";
        }

        public CarpinoAuthResponse GetCarpinoToken()
        {
            Uri carpinoUrl = new Uri(_carpino_base_url + "v1/auth/");
            var client = new RestClient()
            {
                BaseUrl = carpinoUrl,
                Authenticator = new HttpBasicAuthenticator(_carpino_username, _carpino_password)
            };
            //client.Authenticator = new SimpleAuthenticator("User ID", , "Password", );  
            var restRequest = new RestRequest("/token", Method.GET);
            restRequest.AddParameter("platform", "ANDROID");
            restRequest.AddParameter("role", "PASSENGER");
            restRequest.AddParameter("app_version","2.1.13");
            restRequest.AddParameter("auth_type", "PHONE");
            IRestResponse response = client.Execute(restRequest);
            JavaScriptSerializer js = new JavaScriptSerializer();
            var res = js.Deserialize<CarpinoAuthResponse>(response.Content);
            if (res.authToken == null)
            {
                throw new Exception(response.Content);
            }
            return res;
        }

        public string GetTap30Token(string code)
        {
            var tap30Url = _tap30_base_url + "/v2/user/";
            var client = new RestClient(tap30Url);
            var restRequest = new RestRequest("confirm/", Method.POST);
            restRequest.AddJsonBody(new
            {
                confirmation = new
                {
                    code = code
                },
                credential = new
                {
                    phoneNumber = _tap30_phone_number,
                    role= "PASSENGER"
                },
                deviceInfo=new
                {
                    appVersion= 100000192,
                    deviceModel= "SM-N900",
                    deviceToken= "fbbMOkSxrgo:APA91bE0ZOda80BIs4dQOTRgIoLpb8eeAu41Yd6-3-E3M3dZ8qaKK82PZ87ZoFQPrmAAnsup9BrRknIIC0-rEhzwzHgoCakEoGtEHx3qqou5y95m05QZu6SKJ7xoyVb9gp0ZErrd-vye",
                    deviceType= "ANDROID",
                    deviceVendor= "samsung",
                    osVersion="21",
                    platform= "ANDROID"
                }
            });
            IRestResponse response = client.Execute(restRequest);
            JavaScriptSerializer js = new JavaScriptSerializer();
            var res = js.Deserialize<Tap30AuthResponse>(response.Content);
            return res.data.token;
        }

        public string SendAloPeykSms()
        {
            var alopeykUrl = _alopeyk_base_url + "/api/v2/";
            var client = new RestClient(alopeykUrl);
            var restRequest = new RestRequest("login/", Method.POST);
            restRequest.AddJsonBody(new
            {
                phone = _alopeyk_phone_number,
                type= "CUSTOMER",
                model= "SM-N900",
                uuid= "264c1d5f9c0b10e7",
                platform= "android",
                version= "5.0",
                manufacturer= "samsung",
                isVirtual= "123456",
                serial= "264c1d5f9c0b10e7",
                app_version= "2.4.1",
                push_token= "ad084f19-591d-4e97-8447-b657a79ac91f",
                with= new[] {"customer"},
            });
            IRestResponse response = client.Execute(restRequest);
            JavaScriptSerializer js = new JavaScriptSerializer();
            var res = js.Deserialize<AlopeykResponse>(response.Content);
            return res.@object.token;
        }

        public long GetSnappPrice(string authToken,string srcLat, string srcLng,string dstLat,string dstLng)
        {
            Uri snappUri = new Uri(_snapp_api_url + "v2/passenger");
            var client = new RestClient(snappUri);
            //client.Authenticator = new SimpleAuthenticator("User ID", , "Password", );  
            var restRequest = new RestRequest("/price", Method.POST);
            restRequest.AddHeader("authorization", authToken);
            restRequest.AddJsonBody(new
            {
                origin_lat = srcLat,
                origin_lng = srcLng,
                destination_lat = dstLat,
                destination_lng = dstLng,
                service_type = 1,
                sub_service_type = 0,
                destination_place_id = 0,
                round_trip=false,
                services=false,
                tag=2
            });
            IRestResponse response = client.Execute(restRequest);
            JavaScriptSerializer js = new JavaScriptSerializer();
            var res = js.Deserialize<SnappPriceResponse>(response.Content);
            if (res.data == null)
            {
                throw new Exception(response.Content);
            }
            long ret;
            long.TryParse(res.data.final,out ret);
            return ret;
        }

        public string GetTap30Price(string modelSrcLat, string modelSrcLng, string modelDstLat, string modelDstLng, string tap30Token)
        {
            var srcLat = Double.Parse(modelSrcLat);
            var srcLng = Double.Parse(modelSrcLng);
            var dstLat = Double.Parse(modelDstLat);
            var dstLng = Double.Parse(modelDstLng);
            var tap30Url = _tap30_base_url + "/v2/ride/";
            var client = new RestClient(tap30Url);
            var restRequest = new RestRequest("preview", Method.POST);
            restRequest.AddHeader("X-Authorization", tap30Token);
            restRequest.AddJsonBody(new
            {
                destinations = new[] {new 
                {
                    latitude = srcLat,
                    longitude = srcLng
                }},
                origin = new
                {
                    latitude = dstLat,
                    longitude = dstLng
                },
                hasReturn = false
            });
            IRestResponse response = client.Execute(restRequest);
            JavaScriptSerializer js = new JavaScriptSerializer();
            var res = js.Deserialize<Tap30PriceResponse>(response.Content);
            return res.data.priceInfos[0].price;
        }


        public string GetAloPeykToken(string theCode, string token)
        {
            var baseUrl = _alopeyk_base_url + "api/v2/";
            var client = new RestClient(baseUrl);
            var restRequest = new RestRequest("verify/", Method.POST);
            restRequest.AddJsonBody(new
            {
                code= theCode,
                token= token
            });
            IRestResponse response = client.Execute(restRequest);
            JavaScriptSerializer js = new JavaScriptSerializer();
            var res = js.Deserialize<AlopeykTokenResponse>(response.Content);
            return res.@object.csrf_token;
        }
    }
}
