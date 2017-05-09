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
    public class WeeklyTiming : ITimingOption
    {
        public bool IsOption(vwRRTiming timingModel)
        {
            return (timingModel.RRTimingOption == (int)TimingOptions.Weekly);
        }

        public string GetTimingString(List<vwRRTiming> timingModel)
        {
            string timing = "";
            string displayTime = "";
            DateTime time;
            vwRRTiming vwRrTiming = new vwRRTiming();
            var timingModelCount = timingModel.Count - 1;
            for (int i = 0; i <= timingModelCount; i++)
            {
                vwRrTiming = timingModel.ElementAt(i);
                switch (vwRrTiming.RRDayofWeek)
                {
                    case (int)WeekDay.Sat:
                        time = DateTime.Today.Add((TimeSpan)vwRrTiming.RRTheTime);
                        displayTime = time.ToString("HH:mm");
                        if (i != timingModelCount && displayTime == time.ToString("HH:mm"))
                        {
                            timing += string.Format(getResource.getMessage("AtDayOfWeekNoTime") + "، ",
                                getResource.getString("Saturday"));
                        }
                        else
                        {
                            timing += string.Format(getResource.getMessage("AtDayOfWeek") + " - ",
                                getResource.getString("Saturday"), displayTime);
                        }
                        break;
                    case (int)WeekDay.Sun:
                        time = DateTime.Today.Add((TimeSpan)vwRrTiming.RRTheTime);
                        displayTime = time.ToString("HH:mm");
                        if (i != timingModelCount && displayTime == time.ToString("HH:mm"))
                        {
                            timing += string.Format(getResource.getMessage("AtDayOfWeekNoTime") + "، ",
                                getResource.getString("Sunday"));
                        }
                        else
                        {
                            timing += string.Format(getResource.getMessage("AtDayOfWeek") + " - ",
                                getResource.getString("Sunday"), displayTime);
                        }

                        break;
                    case (int)WeekDay.Mon:
                        time = DateTime.Today.Add((TimeSpan)vwRrTiming.RRTheTime);
                        displayTime = time.ToString("HH:mm");
                        if (i != timingModelCount && displayTime == time.ToString("HH:mm"))
                        {
                            timing += string.Format(getResource.getMessage("AtDayOfWeekNoTime") + "، ",
                                getResource.getString("Monday"));
                        }
                        else
                        {
                            timing += string.Format(getResource.getMessage("AtDayOfWeek") + " - ",
                                getResource.getString("Monday"), displayTime);
                        }
                        break;
                    case (int)WeekDay.Tue:
                        time = DateTime.Today.Add((TimeSpan)vwRrTiming.RRTheTime);
                        displayTime = time.ToString("HH:mm");
                        if (i != timingModelCount && displayTime == time.ToString("HH:mm"))
                        {
                            timing += string.Format(getResource.getMessage("AtDayOfWeekNoTime") + "، ",
                                getResource.getString("Tuesday"));
                        }
                        else
                        {
                            timing += string.Format(getResource.getMessage("AtDayOfWeek") + " - ",
                                getResource.getString("Tuesday"), displayTime);
                        }
                        break;
                    case (int)WeekDay.Wed:
                        time = DateTime.Today.Add((TimeSpan)vwRrTiming.RRTheTime);
                        displayTime = time.ToString("HH:mm");
                        if (i != timingModelCount && displayTime == time.ToString("HH:mm"))
                        {
                            timing += string.Format(getResource.getMessage("AtDayOfWeekNoTime") + "، ",
                                getResource.getString("Wednesday"));
                        }
                        else
                        {
                            timing += string.Format(getResource.getMessage("AtDayOfWeek") + " - ",
                                getResource.getString("Wednesday"), displayTime);
                        }
                        break;
                    case (int)WeekDay.Thu:
                        time = DateTime.Today.Add((TimeSpan)vwRrTiming.RRTheTime);
                        displayTime = time.ToString("HH:mm");
                        if (i != timingModelCount && displayTime == time.ToString("HH:mm"))
                        {
                            timing += string.Format(getResource.getMessage("AtDayOfWeekNoTime") + "، ",
                                getResource.getString("Thursday"));
                        }
                        else
                        {
                            timing += string.Format(getResource.getMessage("AtDayOfWeek") + " - ",
                                getResource.getString("Thursday"), displayTime);
                        }
                        break;
                    case (int)WeekDay.Fri:
                        time = DateTime.Today.Add((TimeSpan)vwRrTiming.RRTheTime);
                        displayTime = time.ToString("HH:mm");
                        if (i != timingModelCount && displayTime == time.ToString("HH:mm"))
                        {
                            timing += string.Format(getResource.getMessage("AtDayOfWeekNoTime") + "، ",
                                getResource.getString("Friday"), displayTime);
                        }
                        else
                        {
                            timing += string.Format(getResource.getMessage("AtDayOfWeek") + " - ",
                                getResource.getString("Friday"), displayTime);
                        }
                        break;
                }

            }
            return timing.Substring(0, timing.Length - 3);
        }

        public bool IsSimilarTiming(RouteRequest route, GenerateSimilarRoutes_Result similarRoute,
            vwRRTiming routeTiming, vwRRTiming similarRouteTiming)
        {
            var diff = (similarRouteTiming.RRTheTime - routeTiming.RRTheTime);
            if (routeTiming.RRTimingOption == similarRouteTiming.RRTimingOption)
            {
                if (similarRouteTiming.RRDayofWeek == routeTiming.RRDayofWeek)
                {
                    //var diffTime = (similarRouteTiming.RRTheTime - routeTiming.RRTheTime);
                    if (Math.Abs(diff.Value.TotalHours) < 2)
                        return true;
                }
            }
            if (similarRouteTiming.RRDayofWeek == routeTiming.RRDayofWeek && Math.Abs(diff.Value.TotalHours) < 2)
            {
                return true;
            }
            return false;
        }

        public bool IsSimilarTiming(RouteRequest route, RouteRequest similarRoute,
            vwRRTiming routeTiming, vwRRTiming similarRouteTiming)
        {
            var diff = (similarRouteTiming.RRTheTime - routeTiming.RRTheTime);
            if (routeTiming.RRTimingOption == similarRouteTiming.RRTimingOption)
            {
                if (similarRouteTiming.RRDayofWeek == routeTiming.RRDayofWeek)
                {
                    //var diffTime = (similarRouteTiming.RRTheTime - routeTiming.RRTheTime);
                    if (Math.Abs(diff.Value.TotalHours) <= 2)
                        return true;
                }
            }
            if (similarRouteTiming.RRTimingOption == (int) TimingOptions.InDateAndTime)
            {
                if (similarRouteTiming.RRTheDate > DateTime.Now && similarRouteTiming.RRDayofWeek == routeTiming.RRDayofWeek && Math.Abs(diff.Value.TotalHours) <= 2)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsCurrentTiming(vwRRTiming rrTiming, int diff)
        {
            var minTime = DateTime.Now.AddMinutes((-1) * diff).TimeOfDay;
            var maxTime = DateTime.Now.AddMinutes(diff).TimeOfDay;
            var dayofweek = GetDayOfWeek(DateTime.Now.DayOfWeek);
            if (rrTiming.RRDayofWeek == dayofweek && rrTiming.RRTheTime < maxTime && rrTiming.RRTheTime > minTime)
            {
                return true;
            }
            return false;
        }

        public int GetDayOfWeek(DayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case DayOfWeek.Saturday:
                    return (int)WeekDay.Sat;
                    break;
                case DayOfWeek.Sunday:
                    return (int)WeekDay.Sun;
                    break;
                case DayOfWeek.Monday:
                    return (int)WeekDay.Mon;
                    break;
                case DayOfWeek.Tuesday:
                    return (int)WeekDay.Tue;
                    break;
                case DayOfWeek.Wednesday:
                    return (int)WeekDay.Wed;
                    break;
                case DayOfWeek.Thursday:
                    return (int)WeekDay.Thu;
                    break;
                case DayOfWeek.Friday:
                    return (int)WeekDay.Fri;
                    break;
            }
            return 0;
        }
    }
}
