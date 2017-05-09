using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreDA;

namespace CoreManager.TimingService
{
    public interface ITimingService
    {
        string GetTimingString(List<vwRRTiming> timingModel);
        string GetTimingString(DateTime time);
        bool IsSimilarTiming(RouteRequest route, GenerateSimilarRoutes_Result similarRoute, vwRRTiming routeTiming,
            vwRRTiming similarRouteTiming);
        bool IsSimilarTiming(RouteRequest route, RouteRequest similarRoute, vwRRTiming routeTiming,
            vwRRTiming similarRouteTiming);

        List<vwRRTiming> GetRequestTimings(List<long> routeRequestIds);
        DateTime GetNextOccurance(RRTiming time);
        bool IsCurrentTiming(vwRRTiming rrTiming, int diff);
    }
}
