using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreManager.Models;
using CoreManager.Resources;

namespace CoreManager.PricingManager
{
    public class PricingManager:IPricingManager
    {
        public string GetPriceString(RouteRequestModel routeRequestModel)
        {
            string pricing = "";
            switch (routeRequestModel.PriceOption)
            {
                case PricingOptions.Free:
                    pricing = getResource.getString("Free");
                    break;
                case PricingOptions.NoMatter:
                    pricing = getResource.getString("NoMatterPrice");
                    break;
                case PricingOptions.MinMax:
                    if (routeRequestModel.IsDrive)
                    {
                        pricing = "-";
                    }
                    else
                    {
                        var pricingPattern = getResource.getMessage("Rial");
                        var price = (int)routeRequestModel.CostMinMax;
                        var pricingString = price.ToString("N0", new NumberFormatInfo()
                        {
                            NumberGroupSizes = new[] { 3 },
                            NumberGroupSeparator = ","
                        });
                        pricing = string.Format(pricingPattern, pricingString);
                    }
                    break;
            }
            return pricing;

        }
    }
}
