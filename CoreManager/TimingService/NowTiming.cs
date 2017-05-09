using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using CoreDA;
using CoreManager.Models;
using CoreManager.Resources;

namespace CoreManager.TimingService
{
    public class NowTiming:ITimingOption
    {
        public bool IsOption(vwRRTiming timingModel)
        {
            return (timingModel.RRTimingOption == (int)TimingOptions.Now);
        }

        public string GetTimingString(List<vwRRTiming> timingModel)
        {
            return getResource.getString("Now");
        }

        public bool IsSimilarTiming(RouteRequest route, GenerateSimilarRoutes_Result similarRoute, vwRRTiming routeTiming, vwRRTiming similarRouteTiming)
        {
            var diff = (similarRouteTiming.RRTheTime - routeTiming.RRTheTime);
            if (routeTiming.RRTheDate == DateTime.Now.Date && similarRouteTiming.RRTheDate == DateTime.Now.Date)// && routeTiming.RRTimingOption == similarRouteTiming.RRTimingOption)
            {
                    if (Math.Abs(diff.Value.TotalHours) < 2)
                        return true;
            }
            return false;
        }
        public bool IsSimilarTiming(RouteRequest route, RouteRequest similarRoute, vwRRTiming routeTiming, vwRRTiming similarRouteTiming)
        {
            var diff = (similarRouteTiming.RRTheTime - routeTiming.RRTheTime);
            if (routeTiming.RRTheDate == DateTime.Now.Date && similarRouteTiming.RRTheDate == DateTime.Now.Date)// && routeTiming.RRTimingOption == similarRouteTiming.RRTimingOption)
            {
                if (Math.Abs(diff.Value.TotalHours) < 2)
                    return true;
            }
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