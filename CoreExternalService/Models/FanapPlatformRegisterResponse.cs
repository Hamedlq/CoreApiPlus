using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreExternalService.Models
{
    public class FanapPlatformRegisterResponse
    {
        public bool hasError { get; set; }
        public int errorCode { get; set; }
        public int count { get; set; }
        public string cellphoneNumber { get; set; }
        public FanapUserInfo result { get; set; }
    }

}
