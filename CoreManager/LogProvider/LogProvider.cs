using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreDA;

namespace CoreManager.LogProvider
{
    public class LogProvider : ILogProvider
    {
        public void Log(string controller, string action, string message)
        {
            using (var dataModel = new MibarimEntities())
            {
                dataModel.EventLogs.Add(new EventLog()
                {
                    LogCreateTime = DateTime.Now,
                    LogController = controller,
                    LogAction = action,
                    LogMessage = message
                });
                dataModel.SaveChanges();
            }

        }
    }
}
