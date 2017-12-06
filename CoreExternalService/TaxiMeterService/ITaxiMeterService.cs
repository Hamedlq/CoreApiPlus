using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreExternalService.Models;

namespace CoreExternalService
{
    public interface ITaxiMeterService
    {
        string GetSnapToken();
        string SendTap30TokenSms();
        CarpinoAuthResponse GetCarpinoToken();
    }
}
