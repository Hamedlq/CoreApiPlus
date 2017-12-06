using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreManager.Models;

namespace CoreManager.TaxiMeterManager
{
    public interface ITaxiMeterManager
    {
        TmTokensModel GetTokens(TmTokensModel model);
        string GetTap30Token(string code);
        Gtoken GetGoogleApi(string googleApi);
    }
}
