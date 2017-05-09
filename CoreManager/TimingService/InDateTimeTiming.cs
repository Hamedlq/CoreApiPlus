using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreDA;
using CoreManager.Helper;
using CoreManager.Models;
using CoreManager.Resources;

namespace CoreManager.TimingService
{
    public class InDateTimeTiming:ITimingOption
    {
        public bool IsOption(vwRRTiming timingModel)
        {
            return (timingModel.RRTimingOption == (int)TimingOptions.InDateAndTime);
        }

        public string GetTimingString(List<vwRRTiming> timingModel)
        {
            string timing = "";
            string displayTime = "";
            DateTime time;
            var date = ((DateTime)timingModel.FirstOrDefault().RRTheDate);
            time = DateTime.Today.Add((TimeSpan)timingModel.FirstOrDefault().RRTheTime);
            displayTime = time.ToString("HH:mm");
            timing = string.Format(getResource.getMessage("AtDateTime"), date.ToShamsiDayOfWeek(), date.ToShamsiDateYMD(), displayTime);
            return timing;
        }
        public bool IsSimilarTiming(RouteRequest route, GenerateSimilarRoutes_Result similarRoute, vwRRTiming routeTiming, vwRRTiming similarRouteTiming)
        {
            var diff = (similarRouteTiming.RRTheTime - routeTiming.RRTheTime);
            var diffDay = (similarRouteTiming.RRTheDate - routeTiming.RRTheDate);
            if (similarRouteTiming.RRTimingOption != (int)TimingOptions.Weekly)
            {
                
                if (Math.Abs(diff.Value.TotalHours) < 2 && diffDay.Value.TotalDays == 0)
                    return true;
            }
            if (similarRouteTiming.RRTimingOption == (int)TimingOptions.Weekly)
                if (similarRouteTiming.RRDayofWeek == routeTiming.RRDayofWeek && diffDay.Value.TotalDays == 0 && Math.Abs(diff.Value.TotalHours) < 2)
                    return true;
            //if (similarRouteTiming.RRTimingOption == (int)TimingOptions.Today)
            //    if (routeTiming.RRTheDate.Value.Date == DateTime.Now.Date && Math.Abs(diff.Value.TotalHours) < 1)
            //        return true;
            return false;
        }
        public bool IsSimilarTiming(RouteRequest route, RouteRequest similarRoute, vwRRTiming routeTiming, vwRRTiming similarRouteTiming)
        {
            var diff = (similarRouteTiming.RRTheTime - routeTiming.RRTheTime);
            var diffDay = (similarRouteTiming.RRTheDate - routeTiming.RRTheDate);
            if (similarRouteTiming.RRTimingOption != (int)TimingOptions.Weekly)
            {
                
                if (Math.Abs(diff.Value.TotalHours) <= 2 && diffDay.Value.TotalDays == 0)
                    return true;
            }
            if (similarRouteTiming.RRTimingOption == (int)TimingOptions.Weekly)
                if (similarRouteTiming.RRDayofWeek == routeTiming.RRDayofWeek && diffDay.Value.TotalDays == 0 && Math.Abs(diff.Value.TotalHours) <= 2)
                    return true;
            //if (similarRouteTiming.RRTimingOption == (int)TimingOptions.Today)
            //    if (routeTiming.RRTheDate.Value.Date == DateTime.Now.Date && Math.Abs(diff.Value.TotalHours) < 1)
            //        return true;
            return false;
        }

        public bool IsCurrentTiming(vwRRTiming rrTiming, int diff)
        {
            var nowDate = DateTime.Now.Date;
            var minTime = DateTime.Now.AddMinutes((-1) * diff).TimeOfDay;
            var maxTime = DateTime.Now.AddMinutes(diff).TimeOfDay;
            if (rrTiming.RRTheDate == nowDate && rrTiming.RRTheTime < maxTime && rrTiming.RRTheTime > minTime)
            {
                return true;
            }
            return false;
        }
    }
}
