using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreExternalService.Models;

namespace CoreExternalService
{
    public interface IZarinPalService
    {
        int RequestAuthoruty(decimal chargeValue, string desc, out string authority);
        int VerifyTransaction(string authority, int amount, out long refId);
    }
}
