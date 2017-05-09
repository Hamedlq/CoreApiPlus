using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreManager.Models;

namespace CoreManager.PricingManager
{
    public interface IPricingManager
    {
        string GetPriceString(RouteRequestModel routeRequestModel);
    }
}
