using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreExternalService.Models;

namespace CoreExternalService
{
    public interface IKavenegarService
    {
        string SendSmsMessages(string mobileBrief, string rand);
        string SendVoiceMessages(string mobileBrief, string rand);
        string SendAdminSms(string mobileBrief, string rand);
        string GetLastMessage();
    }
}
