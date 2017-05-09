using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CoreManager.Models
{
    public class ResponseModel
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Status { get; set; }
        public int Count { get; set; }
        public string Type { get; set; }
        public List<MessageResponse> Errors { get; set; }
        public List<MessageResponse> Warnings { get; set; }
        public List<MessageResponse> Infos { get; set; }
        public List<string> Messages { get; set; }
    }
}
