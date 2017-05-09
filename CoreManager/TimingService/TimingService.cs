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
    public class TimingService : ITimingService
    {
        private readonly ITimingStrategy _timingStrategy;

        public TimingService(ITimingStrategy timingStrategy)
        {
            _timingStrategy = timingStrategy;
        }

        public string GetTimingString(List<vwRRTiming> timingModel)
        {
            return _timingStrategy.GetTimingString(timingModel);
        }
        public string GetTimingString(DateTime time)
        {
            string timing = "";
            string displayTime = "";
            var date = time.Date;
            //var timeofDay = time.TimeOfDay;
            displayTime = time.ToString("HH:MM");
            timing = string.Format(getResource.getMessage("DateTime"), date.ToShamsiDayOfWeek(), date.ToShamsiDateYMD(), displayTime);
            return timing;
        }
        public bool IsSimilarTiming(RouteRequest route, GenerateSimilarRoutes_Result similarRoute, vwRRTiming routeTiming, vwRRTiming similarRouteTiming)
        {
            return _timingStrategy.IsSimilarTiming(route, similarRoute, routeTiming, similarRouteTiming);
        }
        public bool IsSimilarTiming(RouteRequest route, RouteRequest similarRoute, vwRRTiming routeTiming, vwRRTiming similarRouteTiming)
        {
            return _timingStrategy.IsSimilarTiming(route, similarRoute, routeTiming, similarRouteTiming);
        }
        public List<vwRRTiming> GetRequestTimings(List<long> routeRequestIds)
        {
            var list = new List<vwRRTiming>();
            using (var dataModel = new MibarimEntities())
            {
                list = dataModel.vwRRTimings.Where(x => routeRequestIds.Contains(x.RouteRequestId)).ToList();
            }
            return list;
        }

        public DateTime GetNextOccurance(RRTiming time)
        {
            int daysToAdd = 0;
            var date = DateTime.Today.Add((TimeSpan)time.RRTheTime);
            switch (time.RRDayofWeek)
            {
                case (int)WeekDay.Sat:
                    daysToAdd = (((int)DayOfWeek.Saturday - (int)date.DayOfWeek + 7) % 7);
                    return date.AddDays(daysToAdd);
                case (int)WeekDay.Sun:
                    daysToAdd = (((int)DayOfWeek.Sunday - (int)date.DayOfWeek + 7) % 7);
                    return date.AddDays(daysToAdd);
                case (int)WeekDay.Mon:
                    daysToAdd = (((int)DayOfWeek.Monday - (int)date.DayOfWeek + 7) % 7);
                    return date.AddDays(daysToAdd);
                case (int)WeekDay.Tue:
                    daysToAdd = (((int)DayOfWeek.Tuesday - (int)date.DayOfWeek + 7) % 7);
                    return date.AddDays(daysToAdd);
                case (int)WeekDay.Wed:
                    daysToAdd = (((int)DayOfWeek.Wednesday - (int)date.DayOfWeek + 7) % 7);
                    return date.AddDays(daysToAdd);
                case (int)WeekDay.Thu:
                    daysToAdd = (((int)DayOfWeek.Thursday - (int)date.DayOfWeek + 7) % 7);
                    return date.AddDays(daysToAdd);
                case (int)WeekDay.Fri:
                    daysToAdd = (((int)DayOfWeek.Friday - (int)date.DayOfWeek + 7) % 7);
                    return date.AddDays(daysToAdd);
            }
            return DateTime.Now;
        }

        public bool IsCurrentTiming(vwRRTiming rrTiming, int diff)
        {
            return _timingStrategy.IsCurrentTiming(rrTiming, diff); 
        }

    }
}
