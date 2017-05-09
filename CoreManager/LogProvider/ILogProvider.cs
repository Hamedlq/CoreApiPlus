using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreManager.LogProvider
{
    public interface ILogProvider
    {
        void Log(string controller, string action, string message);
    }
}
