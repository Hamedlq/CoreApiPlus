using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreExternalService.Models
{
    public class SnappPriceResponse
    {
        public SnappDataPriceResponse data { set; get; }
    }
    public class SnappDataPriceResponse
    {
        public string final { set; get; }
    }

}