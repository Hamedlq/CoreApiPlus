using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using CoreExternalService.Models;
using CoreExternalService.SMSirSentAndReceivedMessages;

namespace CoreExternalService
{
    public class SmsService : ISmsService
    {
        private string UserName; // = "09358695785";
        private string UserPassword; // = "ITexpert";

        public SmsService()
        {
            UserName = ConfigurationManager.AppSettings["UserName"];
            UserPassword = ConfigurationManager.AppSettings["UserPassword"];
        }

        public List<SmsMessage> GetReceivedSmsMessages()
        {
            List<SmsMessage> result = new List<SmsMessage>();
            SMSirSentAndReceivedMessages.SendReceiveSoapClient ws =
                new SMSirSentAndReceivedMessages.SendReceiveSoapClient();
            string message = string.Empty;
            SMSirSentAndReceivedMessages.ReceivedMessage[] messages = ws.GetReceivedMessages(UserName,
                UserPassword, DateTime.Now.AddDays(-1), DateTime.Now, ref message);
            foreach (var receivedMessage in messages)
            {
                var msg = new SmsMessage();
                msg.Id = receivedMessage.ID;
                msg.LineNumber = receivedMessage.LineNumber;
                msg.SMSMessageBody = receivedMessage.SMSMessageBody;
                msg.ReceiveDateTime = receivedMessage.LatinReceiveDateTime;
                msg.MobileNo = receivedMessage.MobileNo;
                result.Add(msg);
            }
            return result;
        }

        public string SendSmsMessages(string mobileBrief, string smsBody)
        {
            
                List<SMSirSentAndReceivedMessages.WebServiceSmsSend> sendDetails = new List<SMSirSentAndReceivedMessages.WebServiceSmsSend>();
                sendDetails.Add(new SMSirSentAndReceivedMessages.WebServiceSmsSend()
                {
                    IsFlash = false,
                    MessageBody = smsBody,
                    MobileNo = long.Parse(mobileBrief)
                });
                SMSirSentAndReceivedMessages.SendReceiveSoapClient ws = new SMSirSentAndReceivedMessages.SendReceiveSoapClient();
                DateTime sendSince = DateTime.Now;
                string message = string.Empty;
                ArrayOfLong result = ws.SendMessage(UserName, UserPassword, sendDetails.ToArray(), 47370, sendSince, ref message);
                if (!string.IsNullOrWhiteSpace(message))
                    throw new Exception(message);
                StringBuilder sbResult = new StringBuilder();
                foreach (var current in result)
                    sbResult.Append(current + ",");
                var res = sbResult.ToString();
                return res;
            
        }
    }
}
