using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreExternalService.Models;

namespace CoreExternalService
{
    public interface IFanapService
    {
        FanapTokenResponse GetAuthenticationToken(string code);
    }
}
