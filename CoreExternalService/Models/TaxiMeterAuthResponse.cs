using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreExternalService.Models
{
    public class SnappAuthResponse
    {
        public string access_token { set; get; }
        public string token_type { set; get; }
        public string expires_in { set; get; }
        public string refresh_token { set; get; }
        public string email { set; get; }
        public string fullname { set; get; }
    }

    public class CarpinoAuthResponse
    {
        public string userId { set; get; }
        public string tokenType { set; get; }
        public string authToken { set; get; }
        public string refreshToken { set; get; }

    }

    public class Tap30AuthResponse
    {
        public string result { set; get; }
        public Tap30AuthDataResponse data { set; get; }

    }
    public class Tap30AuthDataResponse
    {
        public string token { set; get; }
    }
}