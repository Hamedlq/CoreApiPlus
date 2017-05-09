using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreDA;
using CoreManager.Models;
using CoreManager.Resources;

namespace CoreManager.TimingService
{
    public class TodayTiming : ITimingOption
    {
        public bool IsOption(vwRRTiming timingModel)
        {
            return (timingModel.RRTimingOption == (int)TimingOptions.Today);
        }

        public string GetTimingString(List<vwRRTiming> timingModel)
        {
            string timing = "";
            string displayTime = "";
            DateTime time;
            time = DateTime.Today.Add((TimeSpan)timingModel.FirstOrDefault().RRTheTime);
            displayTime = time.ToString("HH:mm");
            timing = string.Format(getResource.getMessage("AtTime"), displayTime);
            return timing;
        }
        public bool IsSimilarTiming(RouteRequest route, GenerateSimilarRoutes_Result similarRoute, vwRRTiming routeTiming, vwRRTiming similarRouteTiming)
        {
            var diff = (similarRouteTiming.RRTheTime - routeTiming.RRTheTime);
            if (routeTiming.RRTheDate == DateTime.Now.Date && similarRouteTiming.RRTheDate == DateTime.Now.Date)
            {
                if (Math.Abs(diff.Value.TotalHours) < 2)
                    return true;
            }
            if (similarRouteTiming.RRTimingOption == (int)TimingOptions.Weekly)
                if (similarRouteTiming.RRDayofWeek == routeTiming.RRDayofWeek && Math.Abs(diff.Value.TotalHours) < 2)
                    return true;
            //if (similarRouteTiming.RRTimingOption == (int)TimingOptions.InDateAndTime)
            //    if (similarRouteTiming.RRTheDate.Value.Date == DateTime.Now.Date && diff.Value.TotalHours < 1)
            //        return true;
            return false;
        }

        public bool IsSimilarTiming(RouteRequest route, RouteRequest similarRoute, vwRRTiming routeTiming, vwRRTiming similarRouteTiming)
        {
            var diff = (similarRouteTiming.RRTheTime - routeTiming.RRTheTime);
            if (routeTiming.RRTheDate == DateTime.Now.Date && similarRouteTiming.RRTheDate == DateTime.Now.Date)
            {
                if (Math.Abs(diff.Value.TotalHours) < 2)
                    return true;
            }
            if (similarRouteTiming.RRTimingOption == (int)TimingOptions.Weekly)
                if (similarRouteTiming.RRDayofWeek == routeTiming.RRDayofWeek && Math.Abs(diff.Value.TotalHours) < 2)
                    return true;
            //if (similarRouteTiming.RRTimingOption == (int)TimingOptions.InDateAndTime)
            //    if (similarRouteTiming.RRTheDate.Value.Date == DateTime.Now.Date && diff.Value.TotalHours < 1)
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
