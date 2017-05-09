using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreDA;

namespace CoreManager.TimingService
{
    public class TimingStrategy:ITimingStrategy
    {
        private readonly List<ITimingOption> _timings;
        public TimingStrategy()
        {
            _timings=new List<ITimingOption>();
            _timings.Add(new NowTiming());
            _timings.Add(new TodayTiming());
            _timings.Add(new InDateTimeTiming());
            _timings.Add(new WeeklyTiming());
        }

        public string GetTimingString(List<vwRRTiming> timingModel)
        {
            return _timings.FirstOrDefault(x => x.IsOption(timingModel.FirstOrDefault())).GetTimingString(timingModel);
        }
        public bool IsSimilarTiming(RouteRequest route, GenerateSimilarRoutes_Result similarRoute, vwRRTiming routeTiming, vwRRTiming similarRouteTiming)
        {
           return _timings.FirstOrDefault(x => x.IsOption(routeTiming)).IsSimilarTiming( route,  similarRoute,  routeTiming, similarRouteTiming);
        }
        public bool IsSimilarTiming(RouteRequest route, RouteRequest similarRoute, vwRRTiming routeTiming, vwRRTiming similarRouteTiming)
        {
            return _timings.FirstOrDefault(x => x.IsOption(routeTiming)).IsSimilarTiming(route, similarRoute, routeTiming, similarRouteTiming);
        }

        public bool IsCurrentTiming(vwRRTiming rrTiming, int diff)
        {
            return _timings.FirstOrDefault(x => x.IsOption(rrTiming)).IsCurrentTiming(rrTiming,diff);
        }
    }
}
