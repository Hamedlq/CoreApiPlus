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
    public class KavenegarService : IKavenegarService
    {
        private string API_CODE = "6A7743547175714A30476B576E34696A41586C694C513D3D";
        private string sendTemplateUrl;
        private string sendUrl;
        private string smsTemplate = "smsVerify";
        private string voiceTemplate = "voiceVerify";
        public KavenegarService()
        {
            API_CODE = ConfigurationManager.AppSettings["KaveNegarApiKey"];
            sendUrl = "https://api.kavenegar.com/v1/" + API_CODE + "/sms/";
            sendTemplateUrl = "https://api.kavenegar.com/v1/" + API_CODE + "/verify/";
        }

        public string SendSmsMessages(string mobileBrief, string rand)
        {
            var client = new RestClient(sendTemplateUrl);
            var restRequest = new RestRequest("lookup.json", Method.POST);
            restRequest.AddParameter("receptor", mobileBrief);
            restRequest.AddParameter("token", rand);
            restRequest.AddParameter("template", smsTemplate);
            IRestResponse response = client.Execute(restRequest);
            /*
                        JavaScriptSerializer js = new JavaScriptSerializer();

                        if (!string.IsNullOrWhiteSpace(response.Content))
                        {
                            directionResponse = js.Deserialize<GDirectionResponse>(response.Content);
                        }
            */
            return response.Content;
        }
        public string SendVoiceMessages(string mobileBrief, string rand)
        {
            var client = new RestClient(sendTemplateUrl);
            var restRequest = new RestRequest("lookup.json", Method.POST);
            restRequest.AddParameter("receptor", mobileBrief);
            restRequest.AddParameter("token", rand);
            restRequest.AddParameter("template", voiceTemplate);
            IRestResponse response = client.Execute(restRequest);
            return response.Content;
        }


        public string SendAdminSms(string mobileBrief, string rand)
        {
            var client = new RestClient(sendUrl);
            var restRequest = new RestRequest("send.json", Method.POST);
            restRequest.AddParameter("receptor", "09358695785");
            restRequest.AddParameter("message", "شماره" + mobileBrief + "با کد" + rand);
            IRestResponse response = client.Execute(restRequest);
            return response.Content;
        }

        public string GetLastMessage()
        {
            var client = new RestClient(sendUrl);
            var restRequest = new RestRequest("latestoutbox.json", Method.POST);
            restRequest.AddParameter("pagesize", 1);
            IRestResponse response = client.Execute(restRequest);
            return response.Content;
        }
    }
}
