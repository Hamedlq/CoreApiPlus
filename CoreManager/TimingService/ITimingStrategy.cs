using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreDA;

namespace CoreManager.TimingService
{
    public interface ITimingStrategy
    {
        string GetTimingString(List<vwRRTiming> timingModel);

        bool IsSimilarTiming(RouteRequest route, GenerateSimilarRoutes_Result similarRoute, vwRRTiming routeTiming, vwRRTiming similarRouteTiming);
        bool IsSimilarTiming(RouteRequest route, RouteRequest similarRoute, vwRRTiming routeTiming, vwRRTiming similarRouteTiming);

        bool IsCurrentTiming(vwRRTiming rrTiming, int diff);
    }
}