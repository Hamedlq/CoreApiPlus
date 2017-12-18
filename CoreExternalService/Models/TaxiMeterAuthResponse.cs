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

    public class Tap30PriceResponse
    {
        public string result { set; get; }
        public Tap30PriceDataResponse data { set; get; }

    }
    public class Tap30PriceDataResponse
    {
        public List<Tap30PriceInfos> priceInfos { set; get; }
    }

    public class Tap30PriceInfos
    {
        public string price { set; get; }
    }

    public class AlopeykResponse
    {
        public string status { set; get; }
        public string message { set; get; }
        public AlopeykObject @object { set; get; }
    }

    public class AlopeykObject
    {
        public string id { set; get; }
        public string token { set; get; }
    }

    public class AlopeykTokenResponse
    {
        public string status { set; get; }
        public AlopeykObjectResponse @object { set; get; }

    }

public class AlopeykObjectResponse
{
    public string csrf_token { set; get; }
    }
}