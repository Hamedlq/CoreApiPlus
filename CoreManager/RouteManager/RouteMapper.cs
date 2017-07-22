using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;
using CoreDA;
using CoreManager.Models;
using AutoMapper;
using CoreManager.Models.RouteModels;
using System.Device.Location;
using CoreExternalService.Models;
using CoreManager.Helper;
using CoreManager.Models.TrafficAddress;
using CoreManager.Resources;
using Point = CoreManager.Models.Point;

namespace CoreManager.RouteManager
{
    public static class RouteMapper
    {
        public static int GetDayOfWeek(DayOfWeek dayOfWeek)
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

        public static IList<RRTiming> CastModelToRrTiming(RouteRequestModel model)
        {
            IList<RRTiming> rrt = new List<RRTiming>();
            int dayOfWeek = 0;
            dayOfWeek = GetDayOfWeek(model.TheDate.DayOfWeek);
            switch (model.TimingOption)
            {
                case TimingOptions.Now:
                    rrt.Add(new RRTiming()
                    {
                        RRTheTime = DateTime.Now.TimeOfDay,
                        RRDayofWeek = dayOfWeek,
                        RRTheDate = DateTime.Now.Date,
                        //RouteRequestId = routeRequestId,
                        RRTimingCreateTime = DateTime.Now,
                        RRTimingOption = (int) TimingOptions.Now,
                        RRTimingIsDeleted = false
                    });
                    break;
                case TimingOptions.Today:
                    rrt.Add(new RRTiming()
                    {
                        RRTheTime = model.TheTime.TimeOfDay,
                        RRDayofWeek = dayOfWeek,
                        RRTheDate = DateTime.Now.Date,
                        //RouteRequestId = routeRequestId,
                        RRTimingCreateTime = DateTime.Now,
                        RRTimingOption = (int) TimingOptions.Today,
                        RRTimingIsDeleted = false
                    });
                    break;
                case TimingOptions.InDateAndTime:
                    rrt.Add(new RRTiming()
                    {
                        RRTheTime = model.TheTime.TimeOfDay,
                        RRDayofWeek = dayOfWeek,
                        RRTheDate = model.TheDate,
                        //RouteRequestId = routeRequestId,
                        RRTimingCreateTime = DateTime.Now,
                        RRTimingOption = (int) TimingOptions.InDateAndTime,
                        RRTimingIsDeleted = false
                    });
                    break;
                case TimingOptions.Weekly:
                    if (model.SatDatetime > DateTime.MinValue)
                    {
                        rrt.Add(new RRTiming()
                        {
                            RRDayofWeek = (int) WeekDay.Sat,
                            RRTheTime = model.SatDatetime.TimeOfDay,
                            //RouteRequestId = routeRequestId,
                            RRTimingCreateTime = DateTime.Now,
                            RRTimingOption = (int) TimingOptions.Weekly,
                            RRTimingIsDeleted = false
                        });
                    }
                    if (model.SunDatetime > DateTime.MinValue)
                    {
                        rrt.Add(new RRTiming()
                        {
                            RRDayofWeek = (int) WeekDay.Sun,
                            RRTheTime = model.SunDatetime.TimeOfDay,
                            //RouteRequestId = routeRequestId,
                            RRTimingCreateTime = DateTime.Now,
                            RRTimingOption = (int) TimingOptions.Weekly,
                            RRTimingIsDeleted = false
                        });
                    }
                    if (model.MonDatetime > DateTime.MinValue)
                    {
                        rrt.Add(new RRTiming()
                        {
                            RRDayofWeek = (int) WeekDay.Mon,
                            RRTheTime = model.MonDatetime.TimeOfDay,
                            //RouteRequestId = routeRequestId,
                            RRTimingCreateTime = DateTime.Now,
                            RRTimingOption = (int) TimingOptions.Weekly,
                            RRTimingIsDeleted = false
                        });
                    }
                    if (model.TueDatetime > DateTime.MinValue)
                    {
                        rrt.Add(new RRTiming()
                        {
                            RRDayofWeek = (int) WeekDay.Tue,
                            RRTheTime = model.TueDatetime.TimeOfDay,
                            //RouteRequestId = routeRequestId,
                            RRTimingCreateTime = DateTime.Now,
                            RRTimingOption = (int) TimingOptions.Weekly,
                            RRTimingIsDeleted = false
                        });
                    }
                    if (model.WedDatetime > DateTime.MinValue)
                    {
                        rrt.Add(new RRTiming()
                        {
                            RRDayofWeek = (int) WeekDay.Wed,
                            RRTheTime = model.WedDatetime.TimeOfDay,
                            //RouteRequestId = routeRequestId,
                            RRTimingCreateTime = DateTime.Now,
                            RRTimingOption = (int) TimingOptions.Weekly,
                            RRTimingIsDeleted = false
                        });
                    }
                    if (model.ThuDatetime > DateTime.MinValue)
                    {
                        rrt.Add(new RRTiming()
                        {
                            RRDayofWeek = (int) WeekDay.Thu,
                            RRTheTime = model.ThuDatetime.TimeOfDay,
                            //RouteRequestId = routeRequestId,
                            RRTimingCreateTime = DateTime.Now,
                            RRTimingOption = (int) TimingOptions.Weekly,
                            RRTimingIsDeleted = false
                        });
                    }
                    if (model.FriDatetime > DateTime.MinValue)
                    {
                        rrt.Add(new RRTiming()
                        {
                            RRDayofWeek = (int) WeekDay.Fri,
                            RRTheTime = model.FriDatetime.TimeOfDay,
                            //RouteRequestId = routeRequestId,
                            RRTimingCreateTime = DateTime.Now,
                            RRTimingOption = (int) TimingOptions.Weekly,
                            RRTimingIsDeleted = false
                        });
                    }
                    break;
            }
            return rrt;
        }

        public static RouteRequest CastModelToRouteRequest(RouteRequestModel model, int userId)
        {
            RouteRequest rr = new RouteRequest();
            rr.SrcGAddress = model.SrcGAddress;
            rr.SrcDetailAddress = model.SrcDetailAddress;
            rr.SrcLatitude = decimal.Parse(model.SrcLatitude);
            rr.SrcLongitude = decimal.Parse(model.SrcLongitude);
            rr.SrcGeo = CreatePoint(model.SrcLatitude, model.SrcLongitude);
            rr.DstGAddress = model.DstGAddress;
            rr.DstDetailAddress = model.DstDetailAddress;
            rr.DstLatitude = decimal.Parse(model.DstLatitude);
            rr.DstLongitude = decimal.Parse(model.DstLongitude);
            rr.DstGeo = CreatePoint(model.DstLatitude, model.DstLongitude);
            rr.AccompanyCount = model.AccompanyCount;
            rr.RequestCreateTime = DateTime.Now;
            rr.RequestLastModifyTime = DateTime.Now;
            rr.RouteRequestType = (int) RouteRequestType.ByWebUser;
            rr.RouteRequestUserId = userId;
            rr.RRIsDeleted = false;
            rr.RRIsConfirmed = (int)BooleanValue.True;
            rr.IsDrive = model.IsDrive;
            rr.RecommendPathId = model.RecommendPathId;
            if (model.IsDrive)
            {
                rr.RouteRequestState = (int)RouteRequestState.WaitForPassenger;
            }
            else
            {
                rr.RouteRequestState = (int)RouteRequestState.WaitForDriver;
            }
            rr.SrcDstDistance = getDistance(double.Parse(model.SrcLatitude), double.Parse(model.SrcLongitude), double.Parse(model.DstLatitude), double.Parse(model.DstLongitude));
            return rr;
        }

        public static DbGeography CreatePoint(string latitude, string longitude)
        {
			//too important -first argument must be longitude and then latitude
            var text = string.Format("POINT({1} {0})", latitude, longitude);
            // 4326 is most common coordinate system used by GPS/Maps
            return DbGeography.FromText(text, 4326);
        }

        public static RRPricing CastModelToRrPricing(RouteRequestModel model)
        {
            var rrp = new RRPricing();
            switch (model.PriceOption)
            {
                case PricingOptions.Free:
                    rrp = new RRPricing()
                    {
                        //RouteRequestId = routeRequestId,
                        RRPricingIsDeleted = false,
                        RRPricingOption = (int) PricingOptions.Free,
                        RRPricingCreateTime = DateTime.Now,
                        RRPricingMinMax = 0
                    };
                    break;
                case PricingOptions.MinMax:
                    rrp=new RRPricing()
                    {
                      //  RouteRequestId = routeRequestId,
                        RRPricingIsDeleted = false,
                        RRPricingOption = (int) PricingOptions.MinMax,
                        RRPricingCreateTime = DateTime.Now,
                        RRPricingMinMax = model.CostMinMax
                    };
                    break;
                case PricingOptions.NoMatter:
                    rrp=new RRPricing()
                    {
                        //RouteRequestId = routeRequestId,
                        RRPricingIsDeleted = false,
                        RRPricingOption = (int) PricingOptions.NoMatter,
                        RRPricingCreateTime = DateTime.Now,
                        RRPricingMinMax = 0
                    };
                    break;
            }

            return rrp;

        }

        public static List<RouteRequestModel> CastToRouteRequestModelList(List<vwRouteRequest> vwList)
        {
            var list = new List<RouteRequestModel>();

            foreach (var request in vwList)
            {
                //populate RouteRequestModel Object
                Mapper.CreateMap<vwRouteRequest, RouteRequestModel>();
                RouteRequestModel routeRequestModel = Mapper.Map<vwRouteRequest, RouteRequestModel>(request);
                //routeRequestModel.RouteUId = request.RouteRequestUId;
                routeRequestModel.PriceOption = (PricingOptions) request.RRPricingOption;

                routeRequestModel.CostMinMax = request.RRPricingMinMax != null
                    ? decimal.Parse(request.RRPricingMinMax.ToString())
                    : 0;
                list.Add(routeRequestModel);
            }
            return list;
        }

        public static List<RouteRequestModel> AddRouteRequestTimingModelList(List<RouteRequestModel> list,
            List<vwRRTiming> timingList)
        {
            //var rlist = list
            //    .GroupBy(u => u.RouteRequestId)
            //    .Select(grp => grp.ToList())
            //    .ToList();
            var reslist = new List<RouteRequestModel>();
            foreach (var request in list)
            {
                reslist.Add(AddRouteRequestTimingModel(request, timingList));
            }
            return reslist;
        }

        public static RouteRequestModel AddRouteRequestTimingModel(RouteRequestModel request,
            List<vwRRTiming> timingList)
        {
            var timings = timingList.Where(y => y.RouteRequestId == request.RouteRequestId);
            foreach (var timing in timings)
            {
                switch (timing.RRTimingOption)
                {
                    case (int) TimingOptions.Today:
                        request.TimingOption = TimingOptions.Today;
                        request.TheTime = DateTime.Today.Add((TimeSpan) timing.RRTheTime);
                        break;
                    case (int) TimingOptions.InDateAndTime:
                        request.TimingOption = TimingOptions.InDateAndTime;
                        request.TheTime = DateTime.Today.Add((TimeSpan) timing.RRTheTime);
                        request.TheDate = (DateTime) timing.RRTheDate;
                        break;
                    case (int) TimingOptions.Weekly:
                        request.TimingOption = TimingOptions.Weekly;
                        switch (timing.RRDayofWeek)
                        {
                            case (int) WeekDay.Sat:
                                request.SatDatetime = DateTime.Today.Add((TimeSpan) timing.RRTheTime);
                                break;
                            case (int) WeekDay.Sun:
                                request.SunDatetime = DateTime.Today.Add((TimeSpan) timing.RRTheTime);
                                break;
                            case (int) WeekDay.Mon:
                                request.MonDatetime = DateTime.Today.Add((TimeSpan) timing.RRTheTime);
                                break;
                            case (int) WeekDay.Tue:
                                request.TueDatetime = DateTime.Today.Add((TimeSpan) timing.RRTheTime);
                                break;
                            case (int) WeekDay.Wed:
                                request.WedDatetime = DateTime.Today.Add((TimeSpan) timing.RRTheTime);
                                break;
                            case (int) WeekDay.Thu:
                                request.ThuDatetime = DateTime.Today.Add((TimeSpan) timing.RRTheTime);
                                break;
                            case (int) WeekDay.Fri:
                                request.FriDatetime = DateTime.Today.Add((TimeSpan) timing.RRTheTime);
                                break;
                        }
                        break;
                }
            }
            return request;
        }

        public static List<RouteSuggestModel> CastToRouteSuggestModel(List<vwRouteSuggest> list,
            List<vwRRTiming> timingList)
        {

            List<RouteSuggestModel> modelList = new List<RouteSuggestModel>();
            foreach (var suggest in list)
            {
                RouteSuggestModel routeSuggestModel = new RouteSuggestModel();
                //populate SuggestrouteResponseModel Object
                RouteResponseModel routeResponsetModel = new RouteResponseModel();
                routeResponsetModel.RouteId = (int) suggest.RouteRequestId;
                routeResponsetModel.SrcAddress = (suggest.SrcGAddress ?? "") + " - " + (suggest.SrcDetailAddress ?? "");
                routeResponsetModel.DstAddress = (suggest.DstGAddress ?? "") + " - " + (suggest.DstDetailAddress ?? "");
                routeResponsetModel.SrcLatitude = suggest.SrcLatitude.ToString();
                routeResponsetModel.SrcLongitude = suggest.SrcLongitude.ToString();
                routeResponsetModel.DstLatitude = suggest.DstLatitude.ToString();
                routeResponsetModel.DstLongitude = suggest.DstLongitude.ToString();
                routeResponsetModel.AccompanyCount = suggest.AccompanyCount;
                routeResponsetModel.IsDrive = (bool) suggest.IsDrive;
                routeSuggestModel.SuggestRouteResponse = routeResponsetModel;
                //populate CarInfo Object
                Mapper.CreateMap<vwRouteSuggest, CarInfoModel>();
                CarInfoModel carInfoModel = Mapper.Map<vwRouteSuggest, CarInfoModel>(suggest);
                routeSuggestModel.CarInfo = carInfoModel;
                //populate SelfRoute Object
                RouteResponseModel selfRouteResponse = new RouteResponseModel()
                {
                    RouteId = (int) suggest.SelfRouteRequestId
                };
                routeSuggestModel.SelfRouteResponse = selfRouteResponse;
                routeSuggestModel.IsSuggestSeen = suggest.IsSuggestSeen;
                routeSuggestModel.IsSuggestAccepted = suggest.IsSuggestAccepted;
                routeSuggestModel.IsSuggestRejected = suggest.IsSuggestRejected;
                routeSuggestModel.NameFamily = suggest.Name + " " + suggest.Family;
                routeSuggestModel.Gender = ((Gender) suggest.Gender).ToString();
                routeSuggestModel.SrcDistance = Math.Round(suggest.SSrcDistance, 0).ToString();
                routeSuggestModel.DstDistance = Math.Round(suggest.SDstDistance, 0).ToString();
                modelList.Add(routeSuggestModel);
            }
            return modelList;
        }

        public static RouteResponseModel CastRouteRequestToRouteResponse(RouteRequestModel requestModel)
        {
            var routeResponse = new RouteResponseModel();
            routeResponse.RouteId = requestModel.RouteRequestId;
            routeResponse.SrcAddress = (requestModel.SrcGAddress ?? "") +  (requestModel.SrcDetailAddress!=null? " - " + requestModel.SrcDetailAddress : "");
            routeResponse.DstAddress = (requestModel.DstGAddress ?? "") + (requestModel.DstDetailAddress!=null ? " - " + requestModel.DstDetailAddress :"");
            routeResponse.SrcLatitude = requestModel.SrcLatitude;
            routeResponse.SrcLongitude = requestModel.SrcLongitude;
            routeResponse.DstLatitude = requestModel.DstLatitude;
            routeResponse.DstLongitude = requestModel.DstLongitude;
            routeResponse.AccompanyCount = requestModel.AccompanyCount;
            routeResponse.IsDrive = requestModel.IsDrive;
            routeResponse.RouteRequestState= getRouteState(requestModel.RouteRequestState);
            if (requestModel.SatDatetime == null)
            {
            }
            return routeResponse;
        }

        private static string getRouteState(int routeRequestState)
        {
            switch (routeRequestState)
            {
                case (int)RouteRequestState.WaitForDriver:
                    return getResource.getString("WaitForDriver");
                case (int)RouteRequestState.WaitForPassenger:
                    return getResource.getString("WaitForPassenger");
                case (int)RouteRequestState.Suggested:
                    return getResource.getString("Suggested");
                case (int)RouteRequestState.RideShareRequested:
                    return getResource.getString("RideShareRequested");
                case (int)RouteRequestState.RideShareAccepted:
                    return getResource.getString("RideShareAccepted");
                case (int)RouteRequestState.TripHappened:
                    return getResource.getString("TripHappened");
            }
            return "";
        }

        public static RouteGroupModel CastSuggestRouteToRouteGroup(vwRouteSuggest routeSuggest)
        {
            var routeGroup=new RouteGroupModel();
            routeGroup.RgHolderRrId = routeSuggest.SelfRouteRequestId;
            routeGroup.RouteId = routeSuggest.RouteRequestId;
            return routeGroup;
        }
        private static double getDistance(double srcLatitude, double srcLongitude, double dstLatitude, double dstLongitude)
        {
            var sCoord = new GeoCoordinate(srcLatitude, srcLongitude);
            var eCoord = new GeoCoordinate(dstLatitude, dstLongitude);

            return sCoord.GetDistanceTo(eCoord);
        }

        public static EventModel CastToEvent(Event evnt)
        {
            var startDate = evnt.EventStartTime.Date;
            var endDate = evnt.EventEndTime.Date;
            var displayStartTime = evnt.EventStartTime.ToString("HH:mm");
            var displayEndTime = evnt.EventEndTime.ToString("HH:mm");
            var ev = new EventModel();
            ev.EventId = evnt.EventId;
            ev.Name = evnt.EventName;
            ev.EventType = (EventTypes)evnt.EventType;
            ev.Address = evnt.EventAddress;
            ev.Conductor = evnt.EventCoductorName;
            ev.EventStartTime= evnt.EventStartTime;
            ev.EventEndTime= evnt.EventEndTime;
            ev.StartTimeString = string.Format(getResource.getMessage("EventDateTime"), startDate.ToShamsiDayOfWeek(), startDate.ToShamsiDateYMD(), displayStartTime);
            ev.EndTimeString = string.Format(getResource.getMessage("EventDateTime"), endDate.ToShamsiDayOfWeek(), endDate.ToShamsiDateYMD(), displayEndTime);
            ev.Latitude = evnt.EventLat.ToString();
            ev.Longitude = evnt.EventLng.ToString();
            ev.Description = evnt.EventDesc;
            ev.ExternalLink= evnt.EventLink;
            return ev;
        }

        public static CityLoc CastToCityLoc(CityLocation cityLocation)
        {
            var loc=new CityLoc();
            loc.ShortName = cityLocation.CityLocShortName;
            loc.FullName = cityLocation.CityLocFullName;
            loc.CityLocationType = (CityLocationTypes)cityLocation.CityLocType;
            loc.CityLocationPoint.Lat = cityLocation.CityLocLat.ToString();
            loc.CityLocationPoint.Lng = cityLocation.CityLocLng.ToString();
            return loc;
        }

        public static List<CoreExternalService.Models.Point> GetPathStepsFromGService(GDirectionResponse gRoute)
        {
            var steps = new List<CoreExternalService.Models.Point>();
            CoreExternalService.Models.Point point;
            var IsFirst = true;
            var firstOrDefault = gRoute.Routes.FirstOrDefault();
            if (firstOrDefault != null)
                foreach (var leg in firstOrDefault.Legs)
                {
                    foreach (var step in leg.Steps)
                    {
                        if (IsFirst)
                        {
                            point = new CoreExternalService.Models.Point();
                            point.Lat = step.Start_location.Lat;
                            point.Lng = step.Start_location.Lng;
                            point.Distance = step.Distance.Value.ToString();
                            point.Duration = step.Duration.Value.ToString();
                            IsFirst = false;
                            steps.Add(point);
                        }
                        point = new CoreExternalService.Models.Point();
                        point.Lat = step.End_location.Lat;
                        point.Lng = step.End_location.Lng;
                        point.Distance = step.Distance.Value.ToString();
                        point.Duration = step.Duration.Value.ToString();
                        steps.Add(point);
                    }
                }
            return steps;
        }

        /*public static List<PathPoint> GetPathsFromGService(GDirectionResponse gResult)
        {
            var res = new List<PathPoint>();
            Point point;
            foreach (var route in gResult.Routes)
            {
                var pathPoint = new PathPoint();
                var IsFirst = true;
                foreach (var leg in route.Legs)
                {
                    foreach (var step in leg.Steps)
                    {
                        if (IsFirst)
                        {
                            point = new Point();
                            point.Lat = step.Start_location.Lat;
                            point.Lng = step.Start_location.Lng;
                            point.Distance = step.Distance.Value.ToString();
                            point.Duration = step.Duration.Value.ToString();
                            IsFirst = false;
                            pathPoint.path.Add(point);
                        }
                        point = new Point();
                        point.Lat = step.End_location.Lat;
                        point.Lng = step.End_location.Lng;
                        point.Distance = step.Distance.Value.ToString();
                        point.Duration = step.Duration.Value.ToString();
                        pathPoint.path.Add(point);
                    }
                }
                res.Add(pathPoint);
            }
            return res;
        }*/

        public static List<Point> CastRouteToPathRoute(List<vw_PathRoute> routePaths)
        {
            var res = new List<Point>();
            var point = new Point();

            foreach (var vwPathRoute in routePaths.OrderBy(x=>x.RecommendPathSeq))
            {
                point = new Point();
                point.Lat = vwPathRoute.RecommendLat.ToString("G29");
                point.Lng = vwPathRoute.RecommendLng.ToString("G29");
                res.Add(point);
            }
            return res;
        }

        public static List<Point> CastRouteToPathRoute(List<vwPath> routePaths)
        {
            var res = new List<Point>();
            var point = new Point();

            foreach (var vwPathRoute in routePaths.OrderBy(x => x.RecommendPathSeq))
            {
                point = new Point();
                point.Lat = vwPathRoute.RecommendLat.ToString("G29");
                point.Lng = vwPathRoute.RecommendLng.ToString("G29");
                res.Add(point);
            }
            return res;
        }

        public static TripRouteModel CastTripRouteToModel(vwTripRoute vwTripRoute, TripLocation lastPoint, int userId)
        {
            var tr = new TripRouteModel();
            tr.UserName = vwTripRoute.Name;
            tr.UserFamily = vwTripRoute.Family;
            tr.UserMobile = vwTripRoute.UserName;
            tr.PayPrice = vwTripRoute.RRPricingMinMax != null
                    ? decimal.Parse(vwTripRoute.RRPricingMinMax.ToString())
                    : 0;
            tr.IsDriver = vwTripRoute.IsDrive;
            tr.IsMe = vwTripRoute.RouteRequestUserId == userId;
            tr.UserImageId = vwTripRoute.UserImageId;
            tr.Lat = lastPoint.TlLat.ToString();
            tr.Lng = lastPoint.TlLng.ToString();
            return tr;
        }
        public static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        public static RouteRequestModel CastGoEventToRouteRequest(EventModel eventModel, RouteEventModel model)
        {
            var routeRequest = new RouteRequestModel();
            routeRequest.SrcLatitude = model.SrcLatitude;
            routeRequest.SrcLongitude = model.SrcLongitude;
            routeRequest.SrcGAddress = model.SrcGAddress;
            routeRequest.DstLatitude= eventModel.Latitude;
            routeRequest.DstLongitude = eventModel.Longitude;
            routeRequest.DstGAddress = eventModel.Name;
            routeRequest.IsDrive = model.IsDrive;
            routeRequest.TimingOption=TimingOptions.InDateAndTime;
            routeRequest.TheDate = eventModel.EventStartTime;
            routeRequest.TheTime = eventModel.EventStartTime;
            routeRequest.PriceOption=PricingOptions.MinMax;
            //kesafat karie
            routeRequest.CostMinMax = model.RecommendPathId;
            if (model.IsDrive)
            {
                routeRequest.RouteRequestState = (int)RouteRequestState.WaitForPassenger;
            }
            else
            {
                routeRequest.RouteRequestState = (int)RouteRequestState.WaitForDriver;
            }
            return routeRequest;
        }
        public static RouteRequestModel CastReturnEventToRouteRequest(EventModel eventModel, RouteEventModel model)
        {
            var routeRequest = new RouteRequestModel();
            routeRequest.SrcLatitude = eventModel.Latitude;
            routeRequest.SrcLongitude = eventModel.Longitude;
            routeRequest.SrcGAddress = eventModel.Name;
            routeRequest.DstLatitude = model.DstLatitude;
            routeRequest.DstLongitude = model.DstLongitude;
            routeRequest.DstGAddress = model.DstGAddress;
            routeRequest.IsDrive = model.IsDrive;
            routeRequest.PriceOption = PricingOptions.MinMax;
            routeRequest.TimingOption = TimingOptions.InDateAndTime;
            routeRequest.TheDate = eventModel.EventEndTime;
            routeRequest.TheTime = eventModel.EventEndTime;
            //kesafat karie
            routeRequest.CostMinMax = model.CostMinMax;
            if (model.IsDrive)
            {
                routeRequest.RouteRequestState = (int)RouteRequestState.WaitForPassenger;
            }
            else
            {
                routeRequest.RouteRequestState = (int)RouteRequestState.WaitForDriver;
            }
            return routeRequest;
        }

        public static Event CastEventRequestToEvent(EventRequestModel model)
        {
            var evnt=new Event();
            evnt.EventCreateTime=DateTime.Now;
            evnt.EventType= (int)EventTypes.GoReturn;
            evnt.EventCoductorName = model.Name;
            evnt.EventCoductorFamily = model.Family;
            evnt.EventCoductorMobile = model.Mobile;
            evnt.EventName = model.EventName;
            evnt.EventLink = model.EventLink;
            var startTime = new DateTime(model.Edate.Year, model.Edate.Month, model.Edate.Day, model.TimeStart.Hour, model.TimeStart.Minute, 0);
            evnt.EventStartTime = startTime;
            var endTime = new DateTime(model.Edate.Year, model.Edate.Month, model.Edate.Day, model.TimeEnd.Hour, model.TimeEnd.Minute, 0);
            evnt.EventEndTime = endTime;
            evnt.EventDeadLine = endTime.AddHours(2);
            evnt.EventConfirmed = 0;//false
            evnt.EventDesc = "";
            evnt.EventAddress = "";
            evnt.EventLat = decimal.Parse(model.Latitude);
            evnt.EventLng = decimal.Parse(model.Longitude);
            evnt.IsConfirmed = false;

            return evnt;
        }

        public static string GetTimePart(string responseTimingString)
        {
            var time= responseTimingString.Split(new string[] { "ساعت" }, StringSplitOptions.None);
            return time[1];
        }

        public static string GetDatePart(string responseTimingString)
        {
            var time = responseTimingString.Split(new string[] { "ساعت" }, StringSplitOptions.None);
            return time[0];
        }

        public static LocalRouteUserModel CastRouteToRouteUser(RouteRequest theRoute, vwUserInfo user)
        {
            var res = new LocalRouteUserModel();
            res.Name = "";//user.Name;
            res.Family = "";//user.Family;
            res.SrcPoint.Lat = theRoute.SrcLatitude.ToString("G29");
            res.SrcPoint.Lng = theRoute.SrcLongitude.ToString("G29");
            res.DstPoint.Lat = theRoute.DstLatitude.ToString("G29");
            res.DstPoint.Lng = theRoute.DstLongitude.ToString("G29");
            if (theRoute.IsDrive)
            {
                res.LocalRouteType = LocalRouteTypes.Driver;
            }
            else
            {
                res.LocalRouteType = LocalRouteTypes.Passenger;
            }
            res.RouteUId = (Guid) theRoute.RouteRequestUId;
            return res;
        }

        public static RouteResponseModel AddResponseTiming(RouteResponseModel response, List<vwRRTiming> timingList, Guid? routeRequestUId)
        {
            foreach (var timing in timingList)
            {
                switch (timing.RRTimingOption)
                {
                    case (int)TimingOptions.Weekly:
                        switch (timing.RRDayofWeek)
                        {
                            case (int)WeekDay.Sat:
                                response.Sat = true;
                                break;
                            case (int)WeekDay.Sun:
                                response.Sun = true;
                                break;
                            case (int)WeekDay.Mon:
                                response.Mon = true;
                                break;
                            case (int)WeekDay.Tue:
                                response.Tue = true;
                                break;
                            case (int)WeekDay.Wed:
                                response.Wed = true;
                                break;
                            case (int)WeekDay.Thu:
                                response.Thu = true;
                                break;
                            case (int)WeekDay.Fri:
                                response.Fri = true;
                                break;
                        }
                        break;
                }
            }
            return response;

        }

        public static UserRouteModel AddResponseTiming(UserRouteModel response, List<vwRRTiming> timingList, Guid? routeRequestUId)
        {
            foreach (var timing in timingList)
            {
                switch (timing.RRTimingOption)
                {
                    case (int)TimingOptions.Weekly:
                        switch (timing.RRDayofWeek)
                        {
                            case (int)WeekDay.Sat:
                                response.Sat = true;
                                break;
                            case (int)WeekDay.Sun:
                                response.Sun = true;
                                break;
                            case (int)WeekDay.Mon:
                                response.Mon = true;
                                break;
                            case (int)WeekDay.Tue:
                                response.Tue = true;
                                break;
                            case (int)WeekDay.Wed:
                                response.Wed = true;
                                break;
                            case (int)WeekDay.Thu:
                                response.Thu = true;
                                break;
                            case (int)WeekDay.Fri:
                                response.Fri = true;
                                break;
                        }
                        break;
                }
            }
            return response;

        }

        public static string GetUserNameFamilyString(vwUserInfo user)
        {
            var res = "";
            if (user.Gender == (int)Gender.Man)
            {
                res = " آقای ";
            }
            else if (user.Gender == (int)Gender.Woman)
            {
                res = " خانم ";
            }
            else
            {
                res = "";
            }
            return res + user.Name + " " + user.Family;
        }
    }
}
