using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Spatial;
using System.Device.Location;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CoreManager.Models;
using CoreDA;
using CoreExternalService;
using CoreExternalService.Models;
using CoreManager.Helper;
using CoreManager.LogProvider;
using CoreManager.Models.RouteModels;
using CoreManager.Models.TrafficAddress;
using CoreManager.NotificationManager;
using CoreManager.PaymentManager;
using CoreManager.PricingManager;
using CoreManager.Resources;
using CoreManager.ResponseProvider;
using DayOfWeek = System.DayOfWeek;
using CoreManager.RouteGroupManager;
using CoreManager.TimingService;
using CoreManager.TransactionManager;
using Point = CoreManager.Models.Point;

namespace CoreManager.RouteManager
{
    public class RouteManager : IRouteManager
    {
        private static string Tag = "RouteManager";
        private readonly ITimingService _timingService;
        private readonly IRouteGroupManager _routeGroupManager;
        private readonly IPricingManager _pricingManager;
        private readonly IResponseProvider _responseProvider;
        private readonly ILogProvider _logmanager;
        private readonly IGoogleService _gService;
        private readonly INotificationManager _notifManager;
        private readonly ITransactionManager _transactionManager;
        private readonly IPaymentManager _paymentManager;

        #region Constructor

        public RouteManager()
        {
        }

        public RouteManager(IRouteGroupManager routeGroupManager,
            ITimingService timingService,
            IPricingManager pricingManager,
            IResponseProvider responseProvider,
            INotificationManager notifManager,
            ITransactionManager transactionManager,
            IPaymentManager paymentManager,
            ILogProvider logmanager)
        {
            //_timingService = new TimingStrategy.TimingStrategy();
            _routeGroupManager = routeGroupManager;
            _timingService = timingService;
            _pricingManager = pricingManager;
            _responseProvider = responseProvider;
            _logmanager = logmanager;
            _notifManager = notifManager;
            _transactionManager = transactionManager;
            _paymentManager = paymentManager;
            _gService = new GoogleService();
        }

        #endregion

        #region Public Methods

        public List<long> InsertUserRoute(RouteRequestModel model, int userId)
        {
            var routeRequestIds = new List<long>();
            using (var dataModel = new MibarimEntities())
            {
                using (var dbContextTransaction = dataModel.Database.BeginTransaction())
                {
                    try
                    {
                        var carInfo = dataModel.CarInfoes.FirstOrDefault(x => x.UserId == userId);
                        var timings = RouteMapper.CastModelToRrTiming(model);
                        //var routeModel = RouteMapper.CastModelToRouteRequest(model, userId);
                        var pricingModel = RouteMapper.CastModelToRrPricing(model);
                        var uid = Guid.NewGuid();
                        foreach (var rrTiming in timings)
                        {
                            var rr = RouteMapper.CastModelToRouteRequest(model, userId);
                            rr.CarInfoId = carInfo != null ? carInfo.CarInfoId : 0;
                            rr.RouteRequestUId = uid;
                            dataModel.RouteRequests.Add(rr);
                            dataModel.SaveChanges();
                            rrTiming.RouteRequestId = rr.RouteRequestId;
                            dataModel.RRTimings.Add(rrTiming);
                            pricingModel.RouteRequestId = rr.RouteRequestId;
                            dataModel.RRPricings.Add(pricingModel);
                            dataModel.SaveChanges();
                            routeRequestIds.Add(rr.RouteRequestId);
                        }
                        dbContextTransaction.Commit();
                    }
                    catch (Exception)
                    {
                        dbContextTransaction.Rollback();
                        return null;
                    }
                }
                DoStuff(routeRequestIds, userId);
                return routeRequestIds;
            }
        }

        private List<long> InsertUserEventRoute(RouteRequestModel model, int userId, DateTime eventStartTime,
            DateTime eventEndTime)
        {
            var routeRequestIds = new List<long>();
            using (var dataModel = new MibarimEntities())
            {
                using (var dbContextTransaction = dataModel.Database.BeginTransaction())
                {
                    try
                    {
                        var carInfo = dataModel.CarInfoes.FirstOrDefault(x => x.UserId == userId);
                        var timings = RouteMapper.CastModelToRrTiming(model);
                        //var routeModel = RouteMapper.CastModelToRouteRequest(model, userId);
                        var pricingModel = RouteMapper.CastModelToRrPricing(model);
                        var uid = Guid.NewGuid();
                        foreach (var rrTiming in timings)
                        {
                            var rr = RouteMapper.CastModelToRouteRequest(model, userId);
                            rr.CarInfoId = carInfo != null ? carInfo.CarInfoId : 0;
                            rr.RouteRequestUId = uid;
                            dataModel.RouteRequests.Add(rr);
                            dataModel.SaveChanges();
                            rrTiming.RouteRequestId = rr.RouteRequestId;
                            dataModel.RRTimings.Add(rrTiming);
                            pricingModel.RouteRequestId = rr.RouteRequestId;
                            dataModel.RRPricings.Add(pricingModel);
                            dataModel.SaveChanges();
                            routeRequestIds.Add(rr.RouteRequestId);
                        }
                        dbContextTransaction.Commit();
                    }
                    catch (Exception)
                    {
                        dbContextTransaction.Rollback();
                        return null;
                    }
                }
                DoEventStuff(routeRequestIds, userId, eventStartTime, eventEndTime);
                return routeRequestIds;
            }
        }

        public void InsertUserEventRoute(RouteEventModel model, int userId)
        {
            var evnt = GetAllEvents().Where(x => x.EventId == model.EventId).ToList();
            if (evnt.Count > 0)
            {
                var theEvent = evnt.FirstOrDefault();
                var routeRequest = new RouteRequestModel();

                if (model.SrcLatitude != null && !string.IsNullOrEmpty(model.SrcLatitude))
                {
                    routeRequest = RouteMapper.CastGoEventToRouteRequest(evnt.FirstOrDefault(), model);
                    InsertUserEventRoute(routeRequest, userId, theEvent.EventStartTime, theEvent.EventEndTime);
                }
                if (model.DstLatitude != null && !string.IsNullOrEmpty(model.DstLatitude))
                {
                    routeRequest = RouteMapper.CastReturnEventToRouteRequest(evnt.FirstOrDefault(), model);
                    InsertUserEventRoute(routeRequest, userId, theEvent.EventStartTime, theEvent.EventEndTime);
                }
            }
        }

        public List<RouteResponseModel> GetUserRoutes(int userId)
        {
            var responseList = new List<RouteResponseModel>();
            var requestList = new List<RouteRequestModel>();
            using (var dataModel = new MibarimEntities())
            {
                var routeRequests =
                    dataModel.vwRouteRequests.Where(x => x.UserId == userId && x.RRIsConfirmed == 1).ToList();
                var timings = _timingService.GetRequestTimings(routeRequests.Select(x => x.RouteRequestId).ToList());
                requestList = RouteMapper.CastToRouteRequestModelList(routeRequests);
                requestList = RouteMapper.AddRouteRequestTimingModelList(requestList, timings);
                foreach (var requestModel in requestList)
                {
                    var response = RouteMapper.CastRouteRequestToRouteResponse(requestModel);
                    response.PricingString = _pricingManager.GetPriceString(requestModel);
                    var timingString =
                        _timingService.GetTimingString(
                            timings.Where(y => y.RouteRequestId == requestModel.RouteRequestId).ToList());
                    response.TimingString = RouteMapper.GetTimePart(timingString);
                    response.DateString = RouteMapper.GetDatePart(timingString);
                    response.CarString =
                        GetCarInfoString(
                            routeRequests.FirstOrDefault(x => x.RouteRequestId == requestModel.RouteRequestId));
                    //response.SuggestGroups = _routeGroupManager.GetSuggestedGroups(requestModel.RouteRequestId);
                    //response.GroupRoutes = _routeGroupManager.GetRouteGroup(requestModel.RouteRequestId);
                    response.SuggestCount = GetSuggestRoutesCount(requestModel.RouteRequestId);
                    //response.SuggestRoutes = GetBriefSuggestRoutes(requestModel.RouteRequestId);
                    responseList.Add(response);
                }
            }
            return responseList;
        }

        public List<RouteResponseModel> GetUserWeekRoutes(int userId)
        {
            var responseList = new List<RouteResponseModel>();
            var requestList = new List<RouteRequestModel>();
            using (var dataModel = new MibarimEntities())
            {
                var routeRequests =
                    dataModel.vwRouteRequests.Where(x => x.UserId == userId && x.RRIsConfirmed == 1).ToList();
                //var timings = _timingService.GetRequestTimings(routeRequests.Select(x => x.RouteRequestId).ToList());
                requestList = RouteMapper.CastToRouteRequestModelList(routeRequests);
                //requestList = RouteMapper.AddRouteRequestTimingModelList(requestList, timings);
                var weekRequestGroups = requestList.GroupBy(x => x.RouteUId);
                foreach (var group in weekRequestGroups)
                {
                    var firstModel = requestList.FirstOrDefault(x => x.RouteUId == group.Key);
                    var response = RouteMapper.CastRouteRequestToRouteResponse(firstModel);
                    var timings =
                        _timingService.GetRequestTimings(
                            requestList.Where(x => x.RouteUId == group.Key)
                                .Select(x => (long) x.RouteRequestId)
                                .ToList());
                    response = RouteMapper.AddResponseTiming(response, timings, group.Key);
                    response.PricingString = _pricingManager.GetPriceString(firstModel);
                    var timingString =
                        _timingService.GetTimingString(
                            timings.Where(y => y.RouteRequestId == firstModel.RouteRequestId).ToList());
                    response.TimingString = RouteMapper.GetTimePart(timingString);
                    response.DateString = RouteMapper.GetDatePart(timingString);
                    response = GetSuggestRoutesCount(response, group.Key);
                    responseList.Add(response);
                }
            }
            return responseList;
        }

        private RouteResponseModel GetSuggestRoutesCount(RouteResponseModel response, Guid? routeRequestUId)
        {
            var newCounter = 0;
            using (var dataModel = new MibarimEntities())
            {
                var routes =
                    dataModel.RouteRequests.Where(
                            x => x.RouteRequestUId == routeRequestUId && !x.RRIsDeleted && x.RRIsConfirmed == 1)
                        .Select(x => x.RouteRequestId)
                        .ToList();
                var vwRouteSuggests =
                    dataModel.vwRouteSuggests.Where(
                        x => routes.Contains(x.SelfRouteRequestId) && !x.IsSuggestAccepted && !x.IsSuggestRejected);
                var grouproutes = vwRouteSuggests.GroupBy(x => x.SuggestRouteRequestUId).ToList();
                response.SuggestCount = grouproutes.Count;
                foreach (var grouproute in grouproutes)
                {
                    var route = vwRouteSuggests.FirstOrDefault(x => x.SuggestRouteRequestUId == grouproute.Key);
                    if (!route.IsSuggestSeen)
                    {
                        newCounter++;
                    }
                }
                response.NewSuggestCount = newCounter;
            }
            return response;
        }

        public List<RouteResponseModel> GetAllRoutes()
        {
            var responseList = new List<RouteResponseModel>();
            var requestList = new List<RouteRequestModel>();
            using (var dataModel = new MibarimEntities())
            {
                var routeRequests = dataModel.vwRouteRequests.Where(x => x.RRIsConfirmed == 1).ToList();
                var timings = _timingService.GetRequestTimings(routeRequests.Select(x => x.RouteRequestId).ToList());
                requestList = RouteMapper.CastToRouteRequestModelList(routeRequests);
                requestList = RouteMapper.AddRouteRequestTimingModelList(requestList, timings);

                foreach (var requestModel in requestList)
                {
                    var response = RouteMapper.CastRouteRequestToRouteResponse(requestModel);
                    response.PricingString = _pricingManager.GetPriceString(requestModel);
                    response.TimingString =
                        _timingService.GetTimingString(
                            timings.Where(y => y.RouteRequestId == requestModel.RouteRequestId).ToList());
                    response.CarString =
                        GetCarInfoString(
                            routeRequests.FirstOrDefault(x => x.RouteRequestId == requestModel.RouteRequestId));
                    //response.SuggestGroups = _routeGroupManager.GetSuggestedGroups(requestModel.RouteRequestId);
                    //response.GroupRoutes = _routeGroupManager.GetRouteGroup(requestModel.RouteRequestId);
                    response.SuggestRoutes = GetBriefSuggestRoutes(requestModel.RouteRequestId);
                    responseList.Add(response);
                }
            }
            return responseList.OrderByDescending(x => x.RouteId).Take(100).ToList();
        }

        public string GetRouteConfirmationMessage(int userId, List<long> routeId)
        {
            var confirmText = GetRouteMessage(userId, routeId);
            using (var dataModel = new MibarimEntities())
            {
                var routes = dataModel.RouteRequests.Where(x => routeId.Contains((int) x.RouteRequestId)).ToList();
                foreach (var route in routes)
                {
                    route.ConfirmatedText = confirmText;
                }
                dataModel.SaveChanges();
            }
            return confirmText;
        }

        public async Task DoStuff(List<long> routeIds, int userId)
        {
            await Task.Run(() => { LongRunningOperation(routeIds, userId); });
        }

        private async Task DoEventStuff(List<long> routeIds, int userId, DateTime eventStartTime, DateTime eventEndTime)
        {
            await Task.Run(() => { LongRunningEventOperation(routeIds, userId, eventStartTime, eventEndTime); });
        }

        private async Task LongRunningOperation(List<long> routeIds, int userId)
        {
            var notifSendingSuggests = new List<RouteSuggest>();
            SaveRouteGroutes(routeIds.FirstOrDefault());
            using (var dataModel = new MibarimEntities())
            {
                var spResult = dataModel.GenerateSimilarRoutes((int) routeIds.FirstOrDefault(), 80, false);
                var similarRoutes = spResult.ToList();
                var routeRequestIds = similarRoutes.Select(x => x.RouteRequestId).ToList();
                routeRequestIds.AddRange(routeIds);
                foreach (var routeId in routeIds)
                {
                    var route =
                        dataModel.RouteRequests.FirstOrDefault(
                            x => x.RouteRequestId == routeId && x.RouteRequestUserId == userId);
                    var timings = _timingService.GetRequestTimings(routeRequestIds);
                    //var pricings = GetRequestPricing(routeRequestIds);
                    foreach (var result in similarRoutes)
                    {
                        if (SimilarTiming(result, route, timings))
                        {
                            var selfRouteRequest =
                                dataModel.RouteRequests.FirstOrDefault(
                                    x => x.RouteRequestId == (int) route.RouteRequestId);
                            selfRouteRequest.RouteRequestState = (int) RouteRequestState.Suggested;
                            var otherRouteRequest =
                                dataModel.RouteRequests.FirstOrDefault(
                                    x => x.RouteRequestId == (int) result.RouteRequestId);
                            otherRouteRequest.RouteRequestState = (int) RouteRequestState.Suggested;
                            var routeSuggest = new RouteSuggest();
                            routeSuggest.SSrcDistance = (double) result.AltStartSec;
                            routeSuggest.SDstDistance = (double) result.AltEndSec;
                            routeSuggest.IsSuggestAccepted = false;
                            routeSuggest.IsSuggestDeleted = false;
                            routeSuggest.IsSuggestRejected = false;
                            routeSuggest.IsSuggestSeen = true;
                            routeSuggest.IsSuggestSent = true;
                            routeSuggest.RSuggestCreateTime = DateTime.Now;
                            routeSuggest.SelfRouteRequestId = (int) route.RouteRequestId;
                            routeSuggest.SuggestRouteRequestId = (int) result.RouteRequestId;
                            dataModel.RouteSuggests.Add(routeSuggest);
                            notifSendingSuggests.Add(routeSuggest);
                            //add self suggestion too
                            var routeSelfSuggest = new RouteSuggest();
                            routeSelfSuggest.SSrcDistance = (double) result.AltStartSec;
                            routeSelfSuggest.SDstDistance = (double) result.AltEndSec;
                            routeSelfSuggest.IsSuggestAccepted = false;
                            routeSelfSuggest.IsSuggestDeleted = false;
                            routeSelfSuggest.IsSuggestRejected = false;
                            routeSelfSuggest.IsSuggestSeen = false;
                            routeSelfSuggest.IsSuggestSent = false;
                            routeSelfSuggest.RSuggestCreateTime = DateTime.Now;
                            routeSelfSuggest.SelfRouteRequestId = (int) result.RouteRequestId;
                            routeSelfSuggest.SuggestRouteRequestId = (int) route.RouteRequestId;
                            dataModel.RouteSuggests.Add(routeSelfSuggest);
                            notifSendingSuggests.Add(routeSelfSuggest);
                        }
                    }
                }
                dataModel.SaveChanges();
                var notifRoute =
                    dataModel.RouteRequests.FirstOrDefault(
                        x => x.RouteRequestId == routeIds.FirstOrDefault() && x.RouteRequestUserId == userId);
                if (notifRoute != null && notifRoute.IsDrive)
                {
                    _notifManager.SendDriverRouteNotif();
                }
                if (notifSendingSuggests.Count > 0)
                {
                    try
                    {
                        _notifManager.SendSuggestionNotif(notifSendingSuggests);
                    }
                    catch (Exception e)
                    {
                        _logmanager.Log(Tag, "SendSuggestionNotif", e.Message);
                    }
                }
            }
        }

        private void SaveRouteGroutes(long routeId)
        {
            Point point;
            RouteRequestGRoute rrr;
            int index = 0;
            int seq = 0;
            bool IsFirst = true;
            using (var dataModel = new MibarimEntities())
            {
                var theRoute = dataModel.RouteRequests.FirstOrDefault(x => x.RouteRequestId == routeId);
                var model = new SrcDstModel();
                model.SrcLat = theRoute.SrcLatitude.ToString();
                model.SrcLng = theRoute.SrcLongitude.ToString();
                model.DstLat = theRoute.DstLatitude.ToString();
                model.DstLng = theRoute.DstLongitude.ToString();
                var gResult = GetGoogleRoute(model, null, false);
                foreach (var route in gResult.Routes)
                {
                    var routeGPath = new RouteRequestGPath();
                    routeGPath.RoutePathIndex = index;
                    routeGPath.RouteRequestUId = (Guid) theRoute.RouteRequestUId;
                    dataModel.RouteRequestGPaths.Add(routeGPath);
                    dataModel.SaveChanges();
                    index++;
                    IsFirst = true;
                    seq = 0;
                    foreach (var leg in route.Legs)
                    {
                        foreach (var step in leg.Steps)
                        {
                            if (IsFirst)
                            {
                                IsFirst = false;
                                rrr = new RouteRequestGRoute();
                                rrr.RRRLatitude = decimal.Parse(step.Start_location.Lat);
                                rrr.RRRLongitude = decimal.Parse(step.Start_location.Lng);
                                rrr.RRRDuration = step.Duration.Value;
                                rrr.RRRGeo = RouteMapper.CreatePoint(step.Start_location.Lat, step.Start_location.Lng);
                                rrr.RoutePathId = routeGPath.RoutePathId;
                                rrr.RRRSeq = seq;
                                seq++;
                                dataModel.RouteRequestGRoutes.Add(rrr);
                            }
                            rrr = new RouteRequestGRoute();
                            rrr.RRRLatitude = decimal.Parse(step.Start_location.Lat);
                            rrr.RRRLongitude = decimal.Parse(step.Start_location.Lng);
                            rrr.RRRDuration = step.Duration.Value;
                            rrr.RRRGeo = RouteMapper.CreatePoint(step.Start_location.Lat, step.Start_location.Lng);
                            rrr.RoutePathId = routeGPath.RoutePathId;
                            rrr.RRRSeq = seq;
                            seq++;
                            dataModel.RouteRequestGRoutes.Add(rrr);
                        }
                    }
                    routeGPath.RoutePathSteps = seq;
                }
                dataModel.SaveChanges();
            }
        }

        private async Task LongRunningEventOperation(List<long> routeIds, int userId, DateTime eventStartTime,
            DateTime eventEndTime)
        {
            var notifSendingSuggests = new List<RouteSuggest>();
            using (var dataModel = new MibarimEntities())
            {
                foreach (var routeId in routeIds)
                {
                    var route =
                        dataModel.RouteRequests.FirstOrDefault(
                            x => x.RouteRequestId == routeId && x.RouteRequestUserId == userId);
                    var spResult = dataModel.GenerateSimilarRoutes((int) route.RouteRequestId, 80, false);
                    var similarRoutes = spResult.ToList();
                    var routeRequestIds = similarRoutes.Select(x => x.RouteRequestId).ToList();
                    routeRequestIds.Add(route.RouteRequestId);
                    var timings = _timingService.GetRequestTimings(routeRequestIds);
                    //var pricings = GetRequestPricing(routeRequestIds);
                    foreach (var result in similarRoutes)
                    {
                        if (GetMatchTiming(result, route, timings, eventStartTime, eventEndTime))
                        {
                            var selfRouteRequest =
                                dataModel.RouteRequests.FirstOrDefault(
                                    x => x.RouteRequestId == (int) route.RouteRequestId);
                            selfRouteRequest.RouteRequestState = (int) RouteRequestState.Suggested;
                            var otherRouteRequest =
                                dataModel.RouteRequests.FirstOrDefault(
                                    x => x.RouteRequestId == (int) result.RouteRequestId);
                            otherRouteRequest.RouteRequestState = (int) RouteRequestState.Suggested;
                            var routeSuggest = new RouteSuggest();
                            routeSuggest.SSrcDistance = (double) result.AltStartSec;
                            routeSuggest.SDstDistance = (double) result.AltEndSec;
                            routeSuggest.IsSuggestAccepted = false;
                            routeSuggest.IsSuggestDeleted = false;
                            routeSuggest.IsSuggestRejected = false;
                            routeSuggest.IsSuggestSeen = true;
                            routeSuggest.IsSuggestSent = true;
                            routeSuggest.RSuggestCreateTime = DateTime.Now;
                            routeSuggest.SelfRouteRequestId = (int) route.RouteRequestId;
                            routeSuggest.SuggestRouteRequestId = (int) result.RouteRequestId;
                            dataModel.RouteSuggests.Add(routeSuggest);
                            notifSendingSuggests.Add(routeSuggest);
                            //add self suggestion too
                            var routeSelfSuggest = new RouteSuggest();
                            routeSelfSuggest.SSrcDistance = (double) result.AltStartSec;
                            routeSelfSuggest.SDstDistance = (double) result.AltEndSec;
                            routeSelfSuggest.IsSuggestAccepted = false;
                            routeSelfSuggest.IsSuggestDeleted = false;
                            routeSelfSuggest.IsSuggestRejected = false;
                            routeSelfSuggest.IsSuggestSeen = false;
                            routeSelfSuggest.IsSuggestSent = false;
                            routeSelfSuggest.RSuggestCreateTime = DateTime.Now;
                            routeSelfSuggest.SelfRouteRequestId = (int) result.RouteRequestId;
                            routeSelfSuggest.SuggestRouteRequestId = (int) route.RouteRequestId;
                            dataModel.RouteSuggests.Add(routeSelfSuggest);
                            notifSendingSuggests.Add(routeSelfSuggest);
                        }
                    }
                }
                dataModel.SaveChanges();
                var notifRoute =
                    dataModel.RouteRequests.FirstOrDefault(
                        x => x.RouteRequestId == routeIds.FirstOrDefault() && x.RouteRequestUserId == userId);
                if (notifRoute != null && notifRoute.IsDrive)
                {
                    _notifManager.SendDriverRouteNotif();
                }
                if (notifSendingSuggests.Count > 0)
                {
                    try
                    {
                        _notifManager.SendSuggestionNotif(notifSendingSuggests);
                    }
                    catch (Exception e)
                    {
                        _logmanager.Log(Tag, "SendSuggestionNotif", e.Message);
                    }
                }
            }
        }

        private bool GetMatchTiming(GenerateSimilarRoutes_Result similarRoute, RouteRequest route,
            List<vwRRTiming> timings, DateTime eventStartTime, DateTime eventEndTime)
        {
            var routeTimings = timings.Where(x => x.RouteRequestId == route.RouteRequestId);
            var similarRouteTimings = timings.Where(x => x.RouteRequestId == similarRoute.RouteRequestId);
            foreach (var routeTiming in routeTimings)
            {
                foreach (var similarRouteTiming in similarRouteTimings)
                {
                    var start = eventStartTime.AddHours(-2);
                    var startDayOfWeek = RouteMapper.GetDayOfWeek(eventStartTime.DayOfWeek);
                    var end = eventEndTime.AddHours(+1);

                    if (similarRouteTiming.RRTimingOption == (int) TimingOptions.Weekly)
                    {
                        if (similarRouteTiming.RRDayofWeek == startDayOfWeek &&
                            start.TimeOfDay < similarRouteTiming.RRTheTime &&
                            end.TimeOfDay > similarRouteTiming.RRTheTime)
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }
            return false;
        }


        public string ConfirmRoute(ConfirmationModel model, int userId)
        {
            int similarrouteCount = 0;
            using (var dataModel = new MibarimEntities())
            {
                foreach (var routeId in model.RouteIds)
                {
                    var route =
                        dataModel.RouteRequests.FirstOrDefault(
                            x => x.RouteRequestId == routeId && x.RouteRequestUserId == userId);
                    route.RRIsConfirmed = (int) BooleanValue.True;
                    route.RequestLastModifyTime = DateTime.Now;
                    dataModel.SaveChanges();
                    var spResult = dataModel.GenerateSimilarRoutes((int) route.RouteRequestId, 35, false);
                    var similarRoutes = spResult.ToList();
                    var routeRequestIds = similarRoutes.Select(x => x.RouteRequestId).ToList();
                    routeRequestIds.Add(route.RouteRequestId);
                    var timings = _timingService.GetRequestTimings(routeRequestIds);
                    //var pricings = GetRequestPricing(routeRequestIds);
                    foreach (var result in similarRoutes)
                    {
                        if (SimilarTiming(result, route, timings))
                        {
                            similarrouteCount++;
                            var routeSuggest = new RouteSuggest();
                            /*routeSuggest.SSrcDistance = (double)result.AltStartSec;
                            routeSuggest.SDstDistance = (double)result.AltEndSec;*/
                            routeSuggest.IsSuggestAccepted = false;
                            routeSuggest.IsSuggestDeleted = false;
                            routeSuggest.IsSuggestRejected = false;
                            routeSuggest.IsSuggestSeen = true;
                            routeSuggest.IsSuggestSent = true;
                            routeSuggest.RSuggestCreateTime = DateTime.Now;
                            routeSuggest.SelfRouteRequestId = (int) route.RouteRequestId;
                            routeSuggest.SuggestRouteRequestId = (int) result.RouteRequestId;
                            dataModel.RouteSuggests.Add(routeSuggest);
                            //add self suggestion too
                            var routeSelfSuggest = new RouteSuggest();
                            /*routeSelfSuggest.SSrcDistance = (double)result.AltStartSec;
                            routeSelfSuggest.SDstDistance = (double)result.AltEndSec;*/
                            routeSelfSuggest.IsSuggestAccepted = false;
                            routeSelfSuggest.IsSuggestDeleted = false;
                            routeSelfSuggest.IsSuggestRejected = false;
                            routeSelfSuggest.IsSuggestSeen = false;
                            routeSelfSuggest.IsSuggestSent = false;
                            routeSelfSuggest.RSuggestCreateTime = DateTime.Now;
                            routeSelfSuggest.SelfRouteRequestId = (int) result.RouteRequestId;
                            routeSelfSuggest.SuggestRouteRequestId = (int) route.RouteRequestId;
                            dataModel.RouteSuggests.Add(routeSelfSuggest);
                        }
                    }
                }
                dataModel.SaveChanges();
                if (similarrouteCount == 0)
                {
                    return getResource.getMessage("suggesttoFriends");
                }
                else
                {
                    return String.Format(getResource.getMessage("similarFound"), similarrouteCount);
                }
            }
        }

        public List<RouteSuggestModel> GetRouteSuggests(int userId)
        {
            var list = new List<RouteSuggestModel>();
            using (var dataModel = new MibarimEntities())
            {
                var suggestRoutes = dataModel.vwRouteSuggests.Where(x => x.UserId == userId).ToList();
                var routeRequestIds = suggestRoutes.Select(x => x.RouteRequestId).ToList();
                var timings = _timingService.GetRequestTimings(routeRequestIds);
                list = RouteMapper.CastToRouteSuggestModel(suggestRoutes, timings);
                //fill time string
                list.ForEach(
                    x =>
                        x.TimingString =
                            _timingService.GetTimingString(
                                timings.Where(y => y.RouteRequestId == x.SuggestRouteResponse.RouteId).ToList()));
            }
            return list;
        }

        public List<BriefRouteModel> GetSuggestRouteByRouteId(int userId, int routeId)
        {
            var suggestRoutes = new List<BriefRouteModel>();
            using (var dataModel = new MibarimEntities())
            {
                var validateSuggestRoute =
                    dataModel.RouteRequests.Where(x => x.RouteRequestUserId == userId && x.RouteRequestId == routeId)
                        .ToList();
                if (validateSuggestRoute.Count > 0)
                {
                    suggestRoutes = GetBriefSuggestRoutes(routeId);
                }
            }
            return suggestRoutes;
        }

        public List<BriefRouteModel> GetSuggestWeekRouteByRouteId(int userId, int routeId)
        {
            var suggestRoutes = new List<BriefRouteModel>();
            using (var dataModel = new MibarimEntities())
            {
                var validateSuggestRoute =
                    dataModel.RouteRequests.Where(x => x.RouteRequestUserId == userId && x.RouteRequestId == routeId)
                        .ToList();
                if (validateSuggestRoute.Count > 0)
                {
                    suggestRoutes = GetBriefSuggestWeekRoutes(routeId);
                }
            }
            return suggestRoutes;
        }

        public List<BriefRouteModel> GetUserSuggestRouteByRouteId(int routeId)
        {
            var suggestRoutes = new List<BriefRouteModel>();
            using (var dataModel = new MibarimEntities())
            {
                var validateSuggestRoute =
                    dataModel.RouteRequests.Where(x => x.RouteRequestId == routeId).ToList();
                if (validateSuggestRoute.Count > 0)
                {
                    suggestRoutes = GetBriefSuggestRoutes(routeId, true);
                }
            }
            return suggestRoutes;
        }

        public string AcceptSuggestedRoute(int userId, int routeId, int selfRouteId)
        {
            string res = string.Empty;
            using (var dataModel = new MibarimEntities())
            {
                var validateSuggestRoute =
                    dataModel.vwTwoRouteSuggests.FirstOrDefault(
                        x =>
                            x.SelfRRUserId == userId && x.SuggestRouteRequestId == routeId &&
                            x.SelfRouteRequestId == selfRouteId);
                if (validateSuggestRoute != null)
                {
                    var suggestRoute =
                        dataModel.RouteSuggests.FirstOrDefault(
                            x => x.SuggestRouteRequestId == routeId && !x.IsSuggestDeleted);
                    if (suggestRoute != null)
                    {
                        var routeGroup = new RouteGroupModel()
                        {
                            RgHolderRrId = validateSuggestRoute.SelfRouteRequestId,
                            RouteId = validateSuggestRoute.SuggestRouteRequestId
                        };
                        var result = _routeGroupManager.AddRouteGroup(userId, routeGroup,
                            validateSuggestRoute.SAccompanyCount);
                        if (result)
                        {
                            suggestRoute.IsSuggestAccepted = true;
                            res = getResource.getMessage("SuggestAccepted");
                            if (!validateSuggestRoute.IsSuggestAccepted)
                            {
                                var r1 = new MessageResponse()
                                {
                                    Type = ResponseTypes.Info,
                                    Message = getResource.getMessage("WaitForOtherUserToAccept")
                                };
                                _responseProvider.SetBusinessMessage(r1);
                            }

                            dataModel.SaveChanges();
                        }
                        //}
                        //else
                        //{
                        //    var r1 = new MessageResponse()
                        //    {
                        //        Type = ResponseTypes.Info,
                        //        Message = getResource.getMessage("OnlythreeGroupAllowed")
                        //    };
                        //    _responseProvider.SetBusinessMessage(r1);
                        //}
                    }
                    //else
                    //{
                    //    var r1 = new MessageResponse()
                    //    {
                    //        Type = ResponseTypes.Info,
                    //        Message = getResource.getMessage("GroupCapacityExceed")
                    //    };
                    //    _responseProvider.SetBusinessMessage(r1);
                    //}
                }
                else
                {
                    var r = new MessageResponse()
                    {
                        Type = ResponseTypes.Warning,
                        Message = getResource.getMessage("NotFound")
                    };
                    _responseProvider.SetBusinessMessage(r);
                }
            }
            return res;
        }

        public string JoinGroup(int userId, long routeId, int groupId)
        {
            using (var dataModel = new MibarimEntities())
            {
                var validation =
                    dataModel.RouteGroups.Where(x => x.RGRouteRequestId == routeId && x.RGIsConfirmed && !x.RGIsDeleted)
                        .ToList();
                if (validation.Count >= 1)
                {
                    _responseProvider.SetBusinessMessage(new MessageResponse()
                    {
                        Type = ResponseTypes.Error,
                        Message = getResource.getMessage("LeaveTheGroup")
                    });
                    return string.Empty;
                }

                var group =
                    dataModel.vwRouteGroups.FirstOrDefault(
                        x => x.UserId == userId && x.RouteRequestId == routeId && x.GroupId == groupId);
                if (group != null)
                {
                    var routeGroup =
                        dataModel.RouteGroups.FirstOrDefault(
                            x => x.RGRouteRequestId == routeId && x.GroupId == groupId && !x.RGIsDeleted);
                    routeGroup.RGIsConfirmed = true;
                    dataModel.SaveChanges();
                }
                else
                {
                    _responseProvider.SetBusinessMessage(new MessageResponse()
                    {
                        Type = ResponseTypes.Warning,
                        Message = getResource.getMessage("UnknownGroup")
                    });
                    return string.Empty;
                }
            }
            return getResource.getMessage("JoinedGroup");
        }

        public string LeaveGroup(int userId, long routeId, int groupId)
        {
            using (var dataModel = new MibarimEntities())
            {
                var group =
                    dataModel.vwRouteGroups.FirstOrDefault(
                        x => x.UserId == userId && x.RouteRequestId == routeId && x.GroupId == groupId);
                if (group != null)
                {
                    var usrGroup = dataModel.vwGroups.FirstOrDefault(x => x.GroupId == group.GroupId);
                    if (usrGroup.GIsDriverConfirmed != null && usrGroup.GIsDriverConfirmed.Value &&
                        usrGroup.GRouteRequestId == routeId)
                    {
                        _responseProvider.SetBusinessMessage(new MessageResponse()
                        {
                            Type = ResponseTypes.Error,
                            Message = getResource.getMessage("CantLeave")
                        });
                        return string.Empty;
                    }
                    var routeGroup =
                        dataModel.RouteGroups.FirstOrDefault(
                            x => x.GroupId == groupId && x.RGRouteRequestId == routeId && !x.RGIsDeleted);
                    routeGroup.RGIsConfirmed = false;
                    dataModel.SaveChanges();
                    return getResource.getMessage("LeavedGroup");
                }
                else
                {
                    _responseProvider.SetBusinessMessage(new MessageResponse()
                    {
                        Type = ResponseTypes.Warning,
                        Message = getResource.getMessage("UnknownGroup")
                    });
                    return string.Empty;
                }
            }
        }

        public string DeleteRoute(int userId, int routeRequestId)
        {
            var res = "";
            using (var dataModel = new MibarimEntities())
            {
                var route =
                    dataModel.RouteRequests.FirstOrDefault(
                        x => x.RouteRequestUserId == userId && x.RouteRequestId == routeRequestId);
                if (route != null)
                {
                    //var usrGroup = dataModel.vwRouteGroups.FirstOrDefault(x => x.RouteRequestId == routeRequestId && x.RGIsConfirmed);
                    /*if (usrGroup != null)
                    {
                        _responseProvider.SetBusinessMessage(new MessageResponse() { Type = ResponseTypes.Error, Message = getResource.getMessage("CantDelete") });
                        return string.Empty;
                    }*/
                    using (var dbContextTransaction = dataModel.Database.BeginTransaction())
                    {
                        try
                        {
                            //var routeGroups = dataModel.RouteGroups.Where(x => x.RGRouteRequestId == routeRequestId || x.RGHolderRRId == routeRequestId);
                            var routeSuggests =
                                dataModel.RouteSuggests.Where(
                                    x =>
                                        x.SuggestRouteRequestId == routeRequestId ||
                                        x.SelfRouteRequestId == routeRequestId);
                            //routeGroups.Each(x => x.RGIsDeleted = true);
                            routeSuggests.Each(x => x.IsSuggestDeleted = true);
                            route.RRIsDeleted = true;
                            dataModel.SaveChanges();
                            dbContextTransaction.Commit();
                            res = getResource.getMessage("RouteDeleted");
                        }
                        catch (Exception e)
                        {
                            dbContextTransaction.Rollback();
                            _responseProvider.SetBusinessMessage(new MessageResponse()
                            {
                                Type = ResponseTypes.Error,
                                Message = getResource.getMessage("ErrorInDelete")
                            });
                            _logmanager.Log(Tag, "DeleteRoute", e.Message);
                        }
                    }
                }
                else
                {
                    _responseProvider.SetBusinessMessage(new MessageResponse()
                    {
                        Type = ResponseTypes.Warning,
                        Message = getResource.getMessage("UnknownRoute")
                    });
                }
            }
            return res;
        }

        public string DeleteGroupSuggest(int userId, long routeId, int routeGroupId)
        {
            using (var dataModel = new MibarimEntities())
            {
                var group =
                    dataModel.vwRouteGroups.FirstOrDefault(
                        x => x.UserId == userId && x.RouteRequestId == routeId && x.GroupId == routeGroupId);
                if (group != null)
                {
                    var routeGroup =
                        dataModel.RouteGroups.FirstOrDefault(
                            x => x.RGRouteRequestId == routeId && x.GroupId == routeGroupId && !x.RGIsDeleted);
                    routeGroup.RGIsDeleted = true;
                    var routesugget =
                        dataModel.RouteSuggests.FirstOrDefault(
                            x => x.SelfRouteRequestId == routeId && !x.IsSuggestDeleted);
                    routesugget.IsSuggestAccepted = false;
                    dataModel.SaveChanges();
                }
                else
                {
                    _responseProvider.SetBusinessMessage(new MessageResponse()
                    {
                        Type = ResponseTypes.Warning,
                        Message = getResource.getMessage("UnknownGroup")
                    });
                    return string.Empty;
                }
            }
            return getResource.getMessage("GroupSuggestDeleted");
        }

        public string DeleteRouteSuggest(int userId, int selfRouteId, int routeRequestId)
        {
            using (var dataModel = new MibarimEntities())
            {
                var validation =
                    dataModel.RouteRequests.FirstOrDefault(
                        x => x.RouteRequestId == selfRouteId && x.RouteRequestUserId == userId);
                if (validation != null)
                {
                    var suggest =
                        dataModel.RouteSuggests.FirstOrDefault(
                            x =>
                                x.SuggestRouteRequestId == routeRequestId && x.SelfRouteRequestId == selfRouteId &&
                                !x.IsSuggestDeleted);
                    if (suggest != null)
                    {
                        suggest.IsSuggestRejected = true;
                        //suggest.IsSuggestDeleted = true;
                        dataModel.SaveChanges();
                        return getResource.getMessage("RouteSuggestDeleted");
                    }
                }
                else
                {
                    _responseProvider.SetBusinessMessage(new MessageResponse()
                    {
                        Type = ResponseTypes.Warning,
                        Message = getResource.getMessage("UnknownRoute")
                    });
                    return string.Empty;
                }
            }
            return string.Empty;
        }

        public List<EventModel> GetAllEvents()
        {
            var res = new List<EventModel>();
            using (var dataModel = new MibarimEntities())
            {
                var Events = dataModel.Events.Where(x => x.EventDeadLine > DateTime.Now && (bool) x.IsConfirmed);
                foreach (var evnt in Events)
                {
                    res.Add(RouteMapper.CastToEvent(evnt));
                }
            }
            return res;
        }

        public List<CityLoc> GetCityLocations(Point point)
        {
            var res = new List<CityLoc>();
            using (var dataModel = new MibarimEntities())
            {
                var cityLocations = dataModel.CityLocations;
                foreach (var loc in cityLocations)
                {
                    res.Add(RouteMapper.CastToCityLoc(loc));
                }
            }
            return res;
        }

        public List<PathPoint> GetRouteRecommends(SrcDstModel model, List<Point> wayPoints)
        {
            var gResult = GetGoogleRoute(model, wayPoints);
            //var gPathSteps = RouteMapper.GetPathsFromGService(gResult);
            var res = new List<PathPoint>();
            Point point;
            RecommendRoute recommendRoute;
            int index = 0;
            int seq = 0;
            using (var dataModel = new MibarimEntities())
            {
                foreach (var route in gResult.Routes)
                {
                    var recommendPath = new RecommendPath();
                    recommendPath.RecommendSrcLat = decimal.Parse(model.SrcLat);
                    recommendPath.RecommendSrcLng = decimal.Parse(model.SrcLng);
                    recommendPath.RecommendSrcGeo = RouteMapper.CreatePoint(model.SrcLat, model.SrcLng);
                    recommendPath.RecommendDstLat = decimal.Parse(model.DstLat);
                    recommendPath.RecommendDstLng = decimal.Parse(model.DstLng);
                    recommendPath.RecommendDstGeo = RouteMapper.CreatePoint(model.DstLat, model.DstLng);
                    recommendPath.RecommendPathIndex = index;
                    dataModel.RecommendPaths.Add(recommendPath);
                    dataModel.SaveChanges();
                    index++;
                    var pathPoint = new PathPoint();
                    pathPoint.metadata.name = recommendPath.RecommendPathId.ToString();
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
                                recommendRoute = new RecommendRoute();
                                recommendRoute.RecommendPathId = recommendPath.RecommendPathId;
                                recommendRoute.RecommendLat = decimal.Parse(step.Start_location.Lat);
                                recommendRoute.RecommendLng = decimal.Parse(step.Start_location.Lng);
                                recommendRoute.RecommendGeo = RouteMapper.CreatePoint(step.Start_location.Lat,
                                    step.Start_location.Lng);
                                recommendRoute.RecommendPathSeq = seq;
                                seq++;
                                dataModel.RecommendRoutes.Add(recommendRoute);
                            }
                            point = new Point();
                            point.Lat = step.End_location.Lat;
                            point.Lng = step.End_location.Lng;
                            point.Distance = step.Distance.Value.ToString();
                            point.Duration = step.Duration.Value.ToString();
                            pathPoint.path.Add(point);
                            recommendRoute = new RecommendRoute();
                            recommendRoute.RecommendPathId = recommendPath.RecommendPathId;
                            recommendRoute.RecommendLat = decimal.Parse(step.Start_location.Lat);
                            recommendRoute.RecommendLng = decimal.Parse(step.Start_location.Lng);
                            recommendRoute.RecommendGeo = RouteMapper.CreatePoint(step.Start_location.Lat,
                                step.Start_location.Lng);
                            recommendRoute.RecommendPathSeq = seq;
                            seq++;
                            dataModel.RecommendRoutes.Add(recommendRoute);
                        }
                    }
                    res.Add(pathPoint);
                }
                dataModel.SaveChanges();
            }
            return res;
        }

        public List<LocalRouteModel> GetLocaRoutes(Point point)
        {
            var localRouteModel = new LocalRouteModel();
            var res = new List<LocalRouteModel>();
            using (var dataModel = new MibarimEntities())
            {
                var spResult = dataModel.GetLocalRoutes(decimal.Parse(point.Lat), decimal.Parse(point.Lng), 10000, 10000);
                var nearRoutes = spResult.ToList();
                var routeUIds = nearRoutes.GroupBy(x => x.RouteRequestUId);
                foreach (var routeUId in routeUIds)
                {
                    var routes = nearRoutes.Where(x => x.RouteRequestUId == routeUId.Key);
                    var theRoute = routes.FirstOrDefault();
                    localRouteModel = new LocalRouteModel();
                    localRouteModel.RouteUId = (Guid) theRoute.RouteRequestUId;
                    localRouteModel.SrcPoint.Lat = theRoute.SrcLatitude.ToString("G29");
                    localRouteModel.SrcPoint.Lng = theRoute.SrcLongitude.ToString("G29");
                    localRouteModel.DstPoint.Lat = theRoute.DstLatitude.ToString("G29");
                    localRouteModel.DstPoint.Lng = theRoute.DstLongitude.ToString("G29");
                    if (routes.FirstOrDefault().IsDrive)
                    {
                        localRouteModel.LocalRouteType = LocalRouteTypes.Driver;
                    }
                    else
                    {
                        localRouteModel.LocalRouteType = LocalRouteTypes.Passenger;
                    }
                    if (theRoute.RecommendPathId != null && theRoute.RecommendPathId != 0)
                    {
                        var routePaths =
                            dataModel.vwPaths.Where(x => x.RecommendPathId == theRoute.RecommendPathId).ToList();
                        localRouteModel.PathRoute.path = RouteMapper.CastRouteToPathRoute(routePaths);
                    }
                    res.Add(localRouteModel);
                }

                /*var groupIds = nearRoutes.Where(x => x.RecommendPathId != 0 && x.RecommendPathId != null).GroupBy(x => x.RecommendPathId);
                foreach (var groupId in groupIds)
                {
                    var routes = nearRoutes.Where(x => x.RecommendPathId == groupId.Key);
                    var timings = _timingService.GetRequestTimings(nearRoutes.Where(x => x.RecommendPathId == groupId.Key).Select(x => (int)x.RouteRequestId).ToList());
                    localRouteModel = new LocalRouteModel();
                    localRouteModel.SrcPoint.Lat = routes.FirstOrDefault().SrcLatitude.ToString();
                    localRouteModel.SrcPoint.Lng = routes.FirstOrDefault().SrcLongitude.ToString();
                    localRouteModel.DstPoint.Lat = routes.FirstOrDefault().DstLatitude.ToString();
                    localRouteModel.DstPoint.Lng = routes.FirstOrDefault().DstLongitude.ToString();
                    string timing = _timingService.GetTimingString(timings);
                    localRouteModel.RouteStartTime = timing;
                    if (routes.FirstOrDefault().IsDrive)
                    {
                        localRouteModel.LocalRouteType = LocalRouteTypes.Driver;
                    }
                    else
                    {
                        localRouteModel.LocalRouteType = LocalRouteTypes.Passenger;
                    }
                    var routePaths = dataModel.vw_PathRoute.Where(x => x.RecommendPathId == groupId.Key).ToList();
                    localRouteModel.PathRoute.path = RouteMapper.CastRouteToPathRoute(routePaths);
                    res.Add(localRouteModel);
                }
                var routeIds = nearRoutes.Where(x => x.RecommendPathId == 0 || x.RecommendPathId == null).GroupBy(x => new { x.SrcLatitude, x.SrcLongitude, x.DstLatitude, x.DstLongitude });
                foreach (var routeId in routeIds)
                {
                    var routes = nearRoutes.Where(x => x.SrcLatitude == routeId.Key.SrcLatitude && x.SrcLongitude == routeId.Key.SrcLongitude && x.DstLatitude == routeId.Key.DstLatitude && x.DstLongitude == routeId.Key.DstLongitude);
                    var timings = _timingService.GetRequestTimings(routes.Select(x => (int)x.RouteRequestId).ToList()).Where(x => x.RRTheDate > DateTime.Now || x.RRTheDate == null).ToList();
                    if (timings.Count > 0)
                    {
                        localRouteModel = new LocalRouteModel();
                        localRouteModel.SrcPoint.Lat = routes.FirstOrDefault().SrcLatitude.ToString();
                        localRouteModel.SrcPoint.Lng = routes.FirstOrDefault().SrcLongitude.ToString();
                        localRouteModel.DstPoint.Lat = routes.FirstOrDefault().DstLatitude.ToString();
                        localRouteModel.DstPoint.Lng = routes.FirstOrDefault().DstLongitude.ToString();
                        string timing = _timingService.GetTimingString(timings);
                        localRouteModel.RouteStartTime = timing;
                        if (routes.FirstOrDefault().IsDrive)
                        {
                            localRouteModel.LocalRouteType = LocalRouteTypes.Driver;
                        }
                        else
                        {
                            localRouteModel.LocalRouteType = LocalRouteTypes.Passenger;
                        }
                        res.Add(localRouteModel);
                    }
                }*/
            }
            return res;
        }


        public string GetPrice(SrcDstModel model)
        {
            var res = "";
            var nightRes = "";

            var privateGRoute = GetGoogleRoute(model, null);
            var distance = privateGRoute.Routes.FirstOrDefault().Legs.FirstOrDefault().Distance.Value;
            var duration = privateGRoute.Routes.FirstOrDefault().Legs.FirstOrDefault().Duration.Value;
            var averageTime = (distance*3600)/15000;
            long extraPrice = 0;
            if (averageTime < duration)
            {
                extraPrice = ((duration - averageTime)/60)*813;
            }
            if (distance < 100)
            {
                res = RemoveDecimal((37260 + extraPrice)/8).ToString();
            }
            else if (distance < 2000)
            {
                res = RemoveDecimal(((((distance/100)*2781) + 37260) + extraPrice)/8).ToString();
                //res = RemoveDecimal(((decimal)((distance) * 2) + extraPrice) * 4).ToString();
            }
            else if (distance > 2000)
            {
                var first2000 = RemoveDecimal((20*2781) + 37260);
                res = RemoveDecimal((((((distance - 2000)/100)*925) + 37260 + first2000) + extraPrice)/8).ToString();
            }
            return res;
        }

        public PathPriceResponse GetPathPrice(SrcDstModel model)
        {
            var res = new PathPriceResponse();
            var nightRes = "";
            var point = new Point();
            var gResult = GetGoogleRoute(model, null, false);
            foreach (var route in gResult.Routes)
            {
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
                            res.PathRoute.path.Add(point);
                        }
                        point = new Point();
                        point.Lat = step.End_location.Lat;
                        point.Lng = step.End_location.Lng;
                        point.Distance = step.Distance.Value.ToString();
                        point.Duration = step.Duration.Value.ToString();
                        res.PathRoute.path.Add(point);
                    }
                }
            }

            var distance = gResult.Routes.FirstOrDefault().Legs.FirstOrDefault().Distance.Value;
            var duration = gResult.Routes.FirstOrDefault().Legs.FirstOrDefault().Duration.Value;
            var averageTime = (distance*3600)/15000;
            long extraPrice = 0;
            if (averageTime < duration)
            {
                extraPrice = ((duration - averageTime)/60)*132;
            }
            if (distance <= 500)
            {
                res.SharedServicePrice = RemoveDecimalToman(5060 + extraPrice).ToString();
            }
            else if (distance <= 7000)
            {
                res.SharedServicePrice = RemoveDecimalToman((((distance - 500)/100)*230) + 5060 + extraPrice).ToString();
            }
            else if (distance > 7000)
            {
                var first7000 = (65*230) + 5060;
                res.SharedServicePrice =
                    RemoveDecimalToman((((distance - 7000)/100)*184) + first7000 + extraPrice).ToString();
            }
            /* if (distance > 50000)
            {
                var first50000 = (430 * 184) + (65 * 230) + 5060;
                res.SharedServicePrice = RemoveDecimalToman(((((distance - 50000) / 100) * 184) + first50000 + extraPrice) * 1.3).ToString();
            }*/
            //private Service
            extraPrice = 0;
            if (averageTime < duration)
            {
                extraPrice = ((duration - averageTime)/60)*813;
            }
            if (distance <= 1330)
            {
                res.PrivateServicePrice = RemoveDecimalToman(37260 + extraPrice).ToString();
            }
            else if (distance <= 2000)
            {
                res.PrivateServicePrice = RemoveDecimalToman((((distance/100)*2781) + extraPrice)).ToString();
            }
            else if (distance > 2000)
            {
                var first2000 = (20*2781);
                res.PrivateServicePrice =
                    RemoveDecimalToman((((distance - 2000)/100)*925) + first2000 + extraPrice).ToString();
            }
            /*if (distance > 50000)
            {
                var first2000 = (20 * 2781);
                res.PrivateServicePrice = RemoveDecimalToman(((((distance - 2000) / 100) * 925) + first2000 + extraPrice) * 1.3).ToString();
            }*/
            return res;
        }

        public string RequestRideShare(int userId, int routeId, int selfRouteId)
        {
            string res = string.Empty;
            string smsBody = string.Empty;
            using (var dataModel = new MibarimEntities())
            {
                var validateSuggestRoute =
                    dataModel.vwTwoRouteSuggests.FirstOrDefault(
                        x =>
                            x.SelfRRUserId == userId && x.SuggestRouteRequestId == routeId &&
                            x.SelfRouteRequestId == selfRouteId);
                if (validateSuggestRoute != null)
                {
                    var suggestRoute =
                        dataModel.RouteSuggests.FirstOrDefault(
                            x =>
                                x.SuggestRouteRequestId == routeId && x.SelfRouteRequestId == selfRouteId &&
                                !x.IsSuggestDeleted);
                    if (suggestRoute != null)
                    {
                        //accept all same suggests
                        var allSameSuggests =
                            dataModel.vwTwoRouteSuggests.Where(
                                x =>
                                    x.SelfRRUserId == userId &&
                                    x.SuggestRouteRequestUId == validateSuggestRoute.SuggestRouteRequestUId &&
                                    x.SelfRouteRequestUId == validateSuggestRoute.SelfRouteRequestUId);
                        RouteSuggest thesuggest;
                        RouteRequest theRouteRequest;
                        foreach (var vwTwoRouteSuggest in allSameSuggests)
                        {
                            thesuggest =
                                dataModel.RouteSuggests.FirstOrDefault(
                                    x =>
                                        x.SuggestRouteRequestId == vwTwoRouteSuggest.SuggestRouteRequestId &&
                                        x.SelfRouteRequestId == vwTwoRouteSuggest.SelfRouteRequestId);
                            thesuggest.IsSuggestAccepted = true;
                            theRouteRequest = dataModel.RouteRequests.FirstOrDefault(
                                x => x.RouteRequestId == vwTwoRouteSuggest.SelfRouteRequestId && !x.RRIsDeleted);
                            theRouteRequest.RouteRequestState = (int) RouteRequestState.RideShareRequested;
                        }
                        /*var theRoute =
                                dataModel.RouteRequests.FirstOrDefault(
                                    x => x.RouteRequestId == validateSuggestRoute.SelfRouteRequestId && !x.RRIsDeleted);*/
                        suggestRoute.IsSuggestAccepted = true;
                        var selfRouteRequest =
                            dataModel.RouteRequests.FirstOrDefault(
                                x => x.RouteRequestId == validateSuggestRoute.SelfRouteRequestId && !x.RRIsDeleted);
                        selfRouteRequest.RouteRequestState = (int) RouteRequestState.RideShareRequested;

                        /*if (selfRouteRequest != null && selfRouteRequest.IsDrive)
                        {
                            contact = dataModel.Contacts.FirstOrDefault(x => x.ContactDriverUserId == validateSuggestRoute.SelfRRUserId && x.ContactPassengerUserId == validateSuggestRoute.SuggestRRUserId);
                        }
                        else
                        {
                            contact = dataModel.Contacts.FirstOrDefault(x => x.ContactPassengerUserId == validateSuggestRoute.SelfRRUserId && x.ContactDriverUserId == validateSuggestRoute.SuggestRRUserId);
                        }*/
                        var contact = new Contact();
                        contact =
                            dataModel.Contacts.FirstOrDefault(
                                x =>
                                    (x.ContactDriverUserId == validateSuggestRoute.SelfRRUserId ||
                                     x.ContactDriverUserId == validateSuggestRoute.SuggestRRUserId)
                                    &&
                                    (x.ContactPassengerUserId == validateSuggestRoute.SuggestRRUserId ||
                                     x.ContactPassengerUserId == validateSuggestRoute.SelfRRUserId));

                        if (contact == null)
                        {
                            contact = new Contact();

                            if (selfRouteRequest != null && selfRouteRequest.IsDrive)
                            {
                                contact.ContactDriverUserId = validateSuggestRoute.SelfRRUserId;
                                contact.ContactPassengerUserId = validateSuggestRoute.SuggestRRUserId;
                            }
                            else
                            {
                                contact.ContactDriverUserId = validateSuggestRoute.SuggestRRUserId;
                                contact.ContactPassengerUserId = validateSuggestRoute.SelfRRUserId;
                            }

                            contact.ContactCreateTime = DateTime.Now;
                            contact.ContactIsDeleted = false;
                            dataModel.Contacts.Add(contact);
                            dataModel.SaveChanges();
                        }
                        /*if (contact.SmsSent == null || !(bool) contact.SmsSent)
                        {
                            if (selfRouteRequest != null && selfRouteRequest.IsDrive)
                            {
                                smsBody = "سلام" + "\r\n" + "ههم مسیری با شما در اپلیکیشن 'می بریم' ثبت شده است.برای قرار ملاقات و مشاهده حساب از اپلیکیشن استفاده نمایید" + "\r\n Mibarim.com";
                            }
                            else
                            {
                                smsBody = "سلام" + "\r\n" + "هم مسیری با شما در اپلیکیشن 'می بریم' ثبت شده است.برای قرار ملاقات و شارژ حساب از اپلیکیشن استفاده نمایید" + "\r\n Mibarim.com";
                            }
                            var user = dataModel.vwUserInfoes.FirstOrDefault(x => x.UserId == validateSuggestRoute.SuggestRRUserId);
                            if (user != null)
                            {
                                var smsService = new SmsService();
                                smsService.SendSmsMessages(MobileBrief(user.UserName), smsBody);
                                contact.SmsSent = true;
                            }
                        }HHamedIT
                         */
                        contact.ContactLastMsgTime = DateTime.Now;
                        contact.ContactIsRideAccepted = false;
                        contact.ContactLastMsg = RouteMapper.Truncate(getResource.getMessage("RideShareRequestSent"), 29);
                        Chat chat = new Chat();
                        chat.ChatCreateTime = DateTime.Now;
                        chat.ContactId = contact.ContactId;
                        chat.ChatIsDeleted = false;
                        chat.ChatUserId = validateSuggestRoute.SelfRRUserId;
                        chat.ChatTxt = getResource.getMessage("RideShareRequestSent");
                        dataModel.Chats.Add(chat);
                        dataModel.SaveChanges();
                        res = getResource.getMessage("RideShareRequestSent");
                        _notifManager.SendRideShareRequestNotif(validateSuggestRoute.SuggestRRUserId);
                        //_notifManager.SendRideShareRequestNotif(validateSuggestRoute.SelfRRUserId);
                        return res;
                    }
                }
                var r = new MessageResponse()
                {
                    Type = ResponseTypes.Warning,
                    Message = getResource.getMessage("NotFound")
                };
                _responseProvider.SetBusinessMessage(r);
            }
            return res;
        }

        private string MobileBrief(string Mobile)
        {
            if (!string.IsNullOrEmpty(Mobile))
                return Mobile.Substring(1);
            return string.Empty;
        }

        /*public string AcceptRideShare(int userId, int routeId, int selfRouteId)
        {
            string res = string.Empty;
            using (var dataModel = new MibarimEntities())
            {
                var validateSuggestRoute = dataModel.vwTwoRouteSuggests.FirstOrDefault(x => x.SelfRRUserId == userId && x.SuggestRouteRequestId == routeId && x.SelfRouteRequestId == selfRouteId);
                if (validateSuggestRoute != null)
                {
                    var suggestRoute = dataModel.RouteSuggests.FirstOrDefault(x => x.SuggestRouteRequestId == routeId && x.SelfRouteRequestId == selfRouteId && !x.IsSuggestDeleted);
                    if (suggestRoute != null)
                    {
                        suggestRoute.IsSuggestAccepted = true;
                        var contact =
                            dataModel.Contacts.FirstOrDefault(
                                x =>
                                    (x.ContactProUserId == validateSuggestRoute.SelfRRUserId ||
                                     x.ContactProUserId == validateSuggestRoute.SuggestRRUserId)
                                    &&
                                    (x.ContactUserId == validateSuggestRoute.SuggestRRUserId ||
                                     x.ContactUserId == validateSuggestRoute.SelfRRUserId));
                        if (contact != null)
                        {
                            contact.ContactLastMsgTime = DateTime.Now;
                            contact.ContactLastMsg = getResource.getMessage("RideShareAccepted");
                            Chat chat = new Chat();
                            chat.ChatCreateTime = DateTime.Now;
                            chat.ContactId = contact.ContactId;
                            chat.ChatIsDeleted = false;
                            chat.ChatUserId = validateSuggestRoute.SelfRRUserId;
                            chat.ChatTxt = getResource.getMessage("RideShareAccepted");
                            dataModel.Chats.Add(chat);
                            dataModel.SaveChanges();
                            res = getResource.getMessage("RideShareAccepted");
                            _notifManager.SendRideShareAcceptionNotif(validateSuggestRoute.SuggestRRUserId);

                            /*                            var theRoute = dataModel.vwRouteRequests.FirstOrDefault(x => x.RouteRequestId == selfRouteId);
                                                        if (theRoute.IsDrive)
                                                        {
                                                            MakePairFactor(userId,contact.ContactId, selfRouteId,routeId, theRoute.RRPricingMinMax.Value);
                                                        }
                                                        else
                                                        {
                                                            theRoute = dataModel.vwRouteRequests.FirstOrDefault(x => x.RouteRequestId == routeId);
                                                            MakePairFactor(userId, contact.ContactId, routeId, selfRouteId, theRoute.RRPricingMinMax.Value);
                                                        }#1#
                        }
                        else
                        {
                            res = getResource.getMessage("NotFound");
                        }
                        return res;
                    }
                }
                var r = new MessageResponse() { Type = ResponseTypes.Warning, Message = getResource.getMessage("NotFound") };
                _responseProvider.SetBusinessMessage(r);
            }
            return res;
        }*/

        //private void MakePairFactor(int userId, long contactId, int passRouteId, int driverRouteId, decimal price)
        //{
        //    using (var dataModel = new MibarimEntities())
        //    {
        //        Chat moneyChat = new Chat();
        //        moneyChat.ChatCreateTime = DateTime.Now;
        //        moneyChat.ContactId = contactId;
        //        moneyChat.ChatIsDeleted = false;
        //        moneyChat.ChatUserId = userId;
        //        moneyChat.ChatTxt = getResource.getMessage("FactorCreated");
        //        dataModel.Chats.Add(moneyChat);

        //        var passfactor = new Factor();
        //        passfactor.FactorUserId = userId;
        //        passfactor.FactorType = (int)FactorType.PassengerPay;
        //        passfactor.FactorCreateTime = DateTime.Now;
        //        passfactor.FactorRequestId = passRouteId;
        //        passfactor.FactorPrice = price + 5000;
        //        passfactor.IsDeleted = false;
        //        passfactor.IsConfirmed = true;
        //        passfactor.IsPayed = false;
        //        dataModel.Factors.Add(passfactor);
        //        dataModel.SaveChanges();

        //        var driverfactor = new Factor();
        //        driverfactor.FactorUserId = userId;
        //        driverfactor.FactorType = (int)FactorType.DriverReceipt;
        //        driverfactor.PairFactorId = passfactor.FactorId;
        //        driverfactor.FactorCreateTime = DateTime.Now;
        //        driverfactor.FactorRequestId = driverRouteId;
        //        driverfactor.FactorPrice = price;
        //        driverfactor.IsDeleted = false;
        //        driverfactor.IsConfirmed = true;
        //        driverfactor.IsPayed = false;
        //        dataModel.Factors.Add(passfactor);
        //        dataModel.SaveChanges();
        //    }
        //}


        public List<SuggestBriefRouteModel> GetAcceptedSuggestRouteByContactId(int userId, long contactId)
        {
            var suggestRoutes = new List<SuggestBriefRouteModel>();
            using (var dataModel = new MibarimEntities())
            {
                var validate = dataModel.Contacts.FirstOrDefault(x => x.ContactId == contactId);
                if (validate != null)
                {
                    var theuserId = validate.ContactDriverUserId == userId
                        ? validate.ContactPassengerUserId
                        : validate.ContactDriverUserId;
                    var suggests =
                        dataModel.vwTwoRouteSuggests.Where(
                            x =>
                                x.SelfRRUserId == theuserId && x.SuggestRRUserId == userId && x.IsSuggestAccepted &&
                                !x.IsSuggestRejected).ToList();
                    var suggestRoutesId = suggests.Select(x => x.SelfRouteRequestId);
                    var selfrouteIds = suggests.Select(x => x.SuggestRouteRequestId);
                    var vwRouteSuggests =
                        dataModel.vwRouteSuggests.Where(
                            x => selfrouteIds.Contains(x.SelfRouteRequestId) && !x.IsSuggestRejected).ToList();
                    var routes = dataModel.vwRouteRequests.Where(x => selfrouteIds.Contains(x.RouteRequestId));
                    if (vwRouteSuggests.Count > 0)
                    {
                        var timings =
                            _timingService.GetRequestTimings(vwRouteSuggests.Select(x => x.RouteRequestId).ToList());
                        foreach (var vwRouteSuggest in vwRouteSuggests)
                        {
                            var self = routes.FirstOrDefault(x => x.RouteRequestId == vwRouteSuggest.SelfRouteRequestId);
                            var suggestBriefRouteModel = new SuggestBriefRouteModel();
                            var selfRouteModel = new BriefRouteModel();
                            selfRouteModel.RouteId = self.RouteRequestId;
                            selfRouteModel.SrcLatitude = self.SrcLatitude.ToString();
                            selfRouteModel.SrcLongitude = self.SrcLongitude.ToString();
                            selfRouteModel.DstLatitude = self.DstLatitude.ToString();
                            selfRouteModel.DstLongitude = self.DstLongitude.ToString();
                            suggestBriefRouteModel.SelfRouteModel = selfRouteModel;
                            var suggestRouteModel = new BriefRouteModel();
                            suggestRouteModel.RouteId = (int) vwRouteSuggest.RouteRequestId;
                            suggestRouteModel.Name = vwRouteSuggest.Name;
                            suggestRouteModel.Family = vwRouteSuggest.Family;
                            suggestRouteModel.SrcLatitude = vwRouteSuggest.SrcLatitude.ToString();
                            suggestRouteModel.SrcLongitude = vwRouteSuggest.SrcLongitude.ToString();
                            suggestRouteModel.DstLatitude = vwRouteSuggest.DstLatitude.ToString();
                            suggestRouteModel.DstLongitude = vwRouteSuggest.DstLongitude.ToString();
                            suggestRouteModel.IsDrive = vwRouteSuggest.IsDrive;
                            suggestRouteModel.AccompanyCount = vwRouteSuggest.AccompanyCount;
                            var srcDistance = vwRouteSuggest.SSrcDistance.ToString("N0", new NumberFormatInfo()
                            {
                                NumberGroupSizes = new[] {3},
                                NumberGroupSeparator = ","
                            });
                            suggestRouteModel.SrcDistance = string.Format(getResource.getMessage("Meter"), srcDistance);
                            var dstDistance = vwRouteSuggest.SDstDistance.ToString("N0", new NumberFormatInfo()
                            {
                                NumberGroupSizes = new[] {3},
                                NumberGroupSeparator = ","
                            });
                            suggestRouteModel.DstDistance = string.Format(getResource.getMessage("Meter"), dstDistance);
                            suggestRouteModel.PricingString = _pricingManager.GetPriceString(new RouteRequestModel()
                            {
                                PriceOption = (PricingOptions) vwRouteSuggest.RRPricingOption,
                                CostMinMax = (decimal) vwRouteSuggest.RRPricingMinMax,
                                IsDrive = (bool) vwRouteSuggest.IsDrive
                            });
                            suggestRouteModel.TimingString =
                                _timingService.GetTimingString(
                                    timings.Where(y => y.RouteRequestId == vwRouteSuggest.RouteRequestId).ToList());
                            suggestRouteModel.CarString = GetCarInfoString(vwRouteSuggest);
                            suggestBriefRouteModel.SuggestRouteModel = suggestRouteModel;
                            suggestRoutes.Add(suggestBriefRouteModel);
                        }
                        // suggestion seen
                        var suggs = dataModel.RouteSuggests.Where(y => suggestRoutesId.Contains(y.SelfRouteRequestId));
                        foreach (var routeSuggest in suggs)
                        {
                            routeSuggest.IsSuggestSent = true;
                            routeSuggest.IsSuggestSeen = true;
                        }
                        dataModel.SaveChanges();
                    }
                }
            }
            return suggestRoutes;
        }

        public List<SuggestBriefRouteModel> GetSimilarSuggestRouteByContactId(int userId, long contactId)
        {
            var suggestRoutes = new List<SuggestBriefRouteModel>();
            using (var dataModel = new MibarimEntities())
            {
                var validate = dataModel.Contacts.FirstOrDefault(x => x.ContactId == contactId);
                if (validate != null)
                {
                    var otherUserId = validate.ContactDriverUserId == userId
                        ? validate.ContactPassengerUserId
                        : validate.ContactDriverUserId;
                    var suggests =
                        dataModel.vwTwoRouteSuggests.Where(
                            x => x.SelfRRUserId == otherUserId && x.SuggestRRUserId == userId).ToList();
                    var suggestRoutesIds = suggests.Select(x => x.SelfRouteRequestId);
                    var selfrouteIds = suggests.Select(x => x.SuggestRouteRequestId);
                    var vwRouteSuggests =
                        dataModel.vwRouteSuggests.Where(
                            x =>
                                selfrouteIds.Contains(x.SelfRouteRequestId) &&
                                suggestRoutesIds.Contains(x.RouteRequestId) && !x.IsSuggestRejected).ToList();
                    var routes = dataModel.vwRouteRequests.Where(x => selfrouteIds.Contains(x.RouteRequestId));
                    if (vwRouteSuggests.Count > 0)
                    {
                        var timings =
                            _timingService.GetRequestTimings(vwRouteSuggests.Select(x => x.RouteRequestId).ToList());
                        foreach (var vwRouteSuggest in vwRouteSuggests)
                        {
                            var self = routes.FirstOrDefault(x => x.RouteRequestId == vwRouteSuggest.SelfRouteRequestId);
                            var suggestBriefRouteModel = new SuggestBriefRouteModel();
                            var selfRouteModel = new BriefRouteModel();
                            selfRouteModel.RouteId = self.RouteRequestId;
                            selfRouteModel.SrcLatitude = self.SrcLatitude.ToString();
                            selfRouteModel.SrcLongitude = self.SrcLongitude.ToString();
                            selfRouteModel.DstLatitude = self.DstLatitude.ToString();
                            selfRouteModel.DstLongitude = self.DstLongitude.ToString();
                            suggestBriefRouteModel.SelfRouteModel = selfRouteModel;
                            var suggestRouteModel = new BriefRouteModel();
                            suggestRouteModel.RouteId = (int) vwRouteSuggest.RouteRequestId;
                            suggestRouteModel.Name = vwRouteSuggest.Name;
                            suggestRouteModel.Family = vwRouteSuggest.Family;
                            suggestRouteModel.SrcLatitude = vwRouteSuggest.SrcLatitude.ToString();
                            suggestRouteModel.SrcLongitude = vwRouteSuggest.SrcLongitude.ToString();
                            suggestRouteModel.DstLatitude = vwRouteSuggest.DstLatitude.ToString();
                            suggestRouteModel.DstLongitude = vwRouteSuggest.DstLongitude.ToString();
                            suggestRouteModel.IsDrive = vwRouteSuggest.IsDrive;
                            suggestRouteModel.AccompanyCount = vwRouteSuggest.AccompanyCount;
                            var srcDistance = vwRouteSuggest.SSrcDistance.ToString("N0", new NumberFormatInfo()
                            {
                                NumberGroupSizes = new[] {3},
                                NumberGroupSeparator = ","
                            });
                            suggestRouteModel.SrcDistance = string.Format(getResource.getMessage("Meter"), srcDistance);
                            var dstDistance = vwRouteSuggest.SDstDistance.ToString("N0", new NumberFormatInfo()
                            {
                                NumberGroupSizes = new[] {3},
                                NumberGroupSeparator = ","
                            });
                            suggestRouteModel.DstDistance = string.Format(getResource.getMessage("Meter"), dstDistance);
                            //based on our business model we get 1000 toman from driver
                            var thePrice = (decimal) vwRouteSuggest.RRPricingMinMax;
                            if (!vwRouteSuggest.IsDrive)
                            {
                                //ServiceWage.Fee = (double)thePrice;
                                thePrice = thePrice; // - ServiceWage.WageDecimal;
                            }
                            suggestRouteModel.PricingString = _pricingManager.GetPriceString(new RouteRequestModel()
                            {
                                PriceOption = (PricingOptions) vwRouteSuggest.RRPricingOption,
                                CostMinMax = thePrice,
                                IsDrive = (bool) vwRouteSuggest.IsDrive
                            });
                            suggestRouteModel.TimingString =
                                _timingService.GetTimingString(
                                    timings.Where(y => y.RouteRequestId == vwRouteSuggest.RouteRequestId).ToList());
                            suggestRouteModel.CarString = GetCarInfoString(vwRouteSuggest);
                            suggestBriefRouteModel.SuggestRouteModel = suggestRouteModel;
                            suggestRoutes.Add(suggestBriefRouteModel);
                        }
                        // suggestion seen
                        var suggs =
                            dataModel.RouteSuggests.Where(
                                y =>
                                    selfrouteIds.Contains(y.SelfRouteRequestId) &&
                                    suggestRoutesIds.Contains(y.SuggestRouteRequestId));
                        foreach (var routeSuggest in suggs)
                        {
                            routeSuggest.IsSuggestSent = true;
                            routeSuggest.IsSuggestSeen = true;
                        }
                        dataModel.SaveChanges();
                    }
                }
            }
            return suggestRoutes;
        }

        public TripResponse GetTripInfo(int userId, long tripId)
        {
            var tripRes = new TripResponse();
            var pastHour = DateTime.Now.AddHours(-1);
            using (var dataModel = new MibarimEntities())
            {
                var path = new List<vw_PathRoute>();
                var carInfo = new CarInfo();
                var tripRoute =
                    dataModel.vwTripRoutes.FirstOrDefault(x => x.TrTripId == tripId && x.RouteRequestUserId == userId);
                if (tripRoute != null && tripRoute.TrState == (int) TripRouteState.TripRouteAlerted)
                {
                    var tr = dataModel.TripRoutes.FirstOrDefault(x => x.TripRouteId == tripRoute.TripRouteId);
                    tr.TrState = (int) TripRouteState.TripRouteJoined;
                    tr.TrModifyTime = DateTime.Now;
                    dataModel.SaveChanges();
                }
                var routes = dataModel.vwTripRoutes.Where(x => x.TrTripId == tripId);
                foreach (var vwTripRoute in routes)
                {
                    var lastPoint =
                        dataModel.TripLocations.OrderByDescending(x => x.TlCreateTime)
                            .FirstOrDefault(
                                x => x.TlUserId == vwTripRoute.RouteRequestUserId && x.TlCreateTime > pastHour);
                    if (vwTripRoute.IsDrive)
                    {
                        path =
                            dataModel.vw_PathRoute.Where(x => x.RecommendPathId == vwTripRoute.RecommendPathId).ToList();
                        carInfo =
                            dataModel.CarInfoes.FirstOrDefault(
                                x => x.UserId == vwTripRoute.RouteRequestUserId && !x.CarInfoIsDeleted);
                        tripRes.SrcPoint.Lat = vwTripRoute.SrcLatitude.ToString();
                        tripRes.SrcPoint.Lng = vwTripRoute.SrcLongitude.ToString();
                        tripRes.DstPoint.Lat = vwTripRoute.DstLatitude.ToString();
                        tripRes.DstPoint.Lng = vwTripRoute.DstLongitude.ToString();
                    }
                    if (vwTripRoute.TrState == (int) TripRouteState.TripRouteJoined && lastPoint != null)
                    {
                        var tr = RouteMapper.CastTripRouteToModel(vwTripRoute, lastPoint, userId);
                        tripRes.TripRoutes.Add(tr);
                    }
                }
                tripRes.PathRoute.path = RouteMapper.CastRouteToPathRoute(path);
                if (carInfo != null)
                {
                    tripRes.CarInfo = GetCarInfoString(carInfo);
                }
                tripRes.TripId = tripId;
            }
            return tripRes;
        }

        //public int EndTrip(int userId, long tripId)
        //{
        //    using (var dataModel = new MibarimEntities())
        //    {
        //        var tripRoute =
        //            dataModel.vwTripRoutes.FirstOrDefault(x => x.TrTripId == tripId && x.RouteRequestUserId == userId);
        //        if (tripRoute != null && tripRoute.IsDrive)
        //        {
        //            var otherTripRoutes =
        //                dataModel.vwTripRoutes.Where(
        //                    x => x.TrTripId == tripId && x.TrState == (int)TripRouteState.TripRouteJoined && x.RouteRequestUserId != userId).ToList();
        //            if (otherTripRoutes.Count > 0)
        //            {
        //                _responseProvider.SetBusinessMessage(new MessageResponse()
        //                {
        //                    Type = ResponseTypes.Error,
        //                    Message = getResource.getMessage("PassengersFirst")
        //                });
        //                return 0;
        //            }
        //            else
        //            {
        //                var routesWithOthers = dataModel.vwTripRoutes.Where(
        //                    x => x.TrTripId == tripId).ToList();
        //                foreach (var routesWithOther in routesWithOthers)
        //                {
        //                    var route = dataModel.TripRoutes.FirstOrDefault(x => x.TripRouteId == tripRoute.TripRouteId);
        //                    if (routesWithOther.TrState == (int)TripRouteState.TripRouteJoined)
        //                    {
        //                        route.TrState = (int)TripRouteState.TripRouteFinished;
        //                        route.TrModifyTime = DateTime.Now;
        //                    }
        //                    if (routesWithOther.TrState == (int)TripRouteState.TripRouteAlerted)
        //                    {
        //                        route.TrState = (int)TripRouteState.TripRouteNotJoined;
        //                        route.TrModifyTime = DateTime.Now;
        //                    }
        //                    var userinfo = dataModel.UserInfoes.FirstOrDefault(x => x.UserId == routesWithOther.RouteRequestUserId);
        //                    userinfo.TripId = 0;
        //                }
        //                var trip = dataModel.Trips.FirstOrDefault(x => x.TripId == tripId);
        //                trip.TripEndTime = DateTime.Now;
        //                trip.TripState = (int)TripState.Finished;
        //                var user = dataModel.UserInfoes.FirstOrDefault(x => x.UserId == userId);
        //                user.TripId = 0;
        //                dataModel.SaveChanges();
        //                return 1;
        //            }
        //        }
        //        else
        //        {
        //            var route = dataModel.TripRoutes.FirstOrDefault(x => x.TripRouteId == tripRoute.TripRouteId);
        //            if (route.TrState == (int)TripRouteState.TripRouteJoined)
        //            {
        //                route.TrState = (int)TripRouteState.TripRouteFinished;
        //                route.TrModifyTime = DateTime.Now;
        //                var user = dataModel.UserInfoes.FirstOrDefault(x => x.UserId == userId);
        //                user.TripId = 0;
        //                dataModel.SaveChanges();
        //                var tripDriver =
        //                    dataModel.vwTripRoutes.FirstOrDefault(x => x.TrTripId == tripId && x.IsDrive);
        //                var pay = dataModel.RRPricings.FirstOrDefault(x => x.RouteRequestId == tripRoute.RouteRequestId);
        //                _transactionManager.PayMoney(userId, tripDriver.RouteRequestUserId, (int)pay.RRPricingMinMax);
        //            }
        //            return 1;
        //        }
        //    }
        //}

        public List<RouteResponseModel> GetUserRoutesByMobile(string mobile)
        {
            var responseList = new List<RouteResponseModel>();
            var requestList = new List<RouteRequestModel>();
            using (var dataModel = new MibarimEntities())
            {
                var user = dataModel.vwUserInfoes.FirstOrDefault(x => x.UserName == mobile);
                var routeRequests =
                    dataModel.vwRouteRequests.Where(x => x.UserId == user.UserId && x.RRIsConfirmed == 1).ToList();
                var timings = _timingService.GetRequestTimings(routeRequests.Select(x => x.RouteRequestId).ToList());
                requestList = RouteMapper.CastToRouteRequestModelList(routeRequests);
                requestList = RouteMapper.AddRouteRequestTimingModelList(requestList, timings);
                foreach (var requestModel in requestList)
                {
                    var response = RouteMapper.CastRouteRequestToRouteResponse(requestModel);
                    response.PricingString = _pricingManager.GetPriceString(requestModel);
                    response.TimingString =
                        _timingService.GetTimingString(
                            timings.Where(y => y.RouteRequestId == requestModel.RouteRequestId).ToList());
                    response.CarString =
                        GetCarInfoString(
                            routeRequests.FirstOrDefault(x => x.RouteRequestId == requestModel.RouteRequestId));
                    //response.SuggestGroups = _routeGroupManager.GetSuggestedGroups(requestModel.RouteRequestId);
                    //response.GroupRoutes = _routeGroupManager.GetRouteGroup(requestModel.RouteRequestId);
                    //response.SuggestCount = GetSuggestRoutesCount(requestModel.RouteRequestId);
                    response.SuggestRoutes = GetBriefSuggestRoutes(requestModel.RouteRequestId);
                    responseList.Add(response);
                }
            }
            return responseList;
        }

        public void InsertEvent(EventRequestModel model)
        {
            using (var dataModel = new MibarimEntities())
            {
                var evnt = RouteMapper.CastEventRequestToEvent(model);
                dataModel.Events.Add(evnt);
                dataModel.SaveChanges();
                var smsService = new SmsService();
                smsService.SendSmsMessages("9358695785", "رویداد جدید");
                _notifManager.SendNewEvent();
            }
        }

        public ShareResponse ShareRoute(int userId, int modelRouteRequestId)
        {
            var res = new ShareResponse();
            using (var dataModel = new MibarimEntities())
            {
                var rr =
                    dataModel.RouteRequests.Where(
                            x =>
                                x.RouteRequestUserId == userId && x.RouteRequestId == modelRouteRequestId &&
                                !x.RRIsDeleted)
                        .ToList();
                if (rr.Count > 0)
                {
                    var shared = dataModel.MapImages.Where(x => x.RouteRequestId == modelRouteRequestId).ToList();
                    if (shared.Count > 0)
                    {
                        res.ImageId = shared.FirstOrDefault().MapShareGuid.ToString();
                        res.ImageCaption = shared.FirstOrDefault().ShareCaption;
                        res.ImagePath = shared.FirstOrDefault().SharePath;
                        return res;
                    }
                    var mapImage = new MapImage();
                    var imageId = Guid.NewGuid();
                    mapImage.MapShareGuid = imageId;
                    mapImage.SeenCount = 0;
                    mapImage.RouteRequestId = modelRouteRequestId;
                    GDirectionRequest request = new GDirectionRequest();
                    request.Src.Lat = rr.FirstOrDefault().SrcLatitude.ToString();
                    request.Src.Lng = rr.FirstOrDefault().SrcLongitude.ToString();
                    request.Dst.Lat = rr.FirstOrDefault().DstLatitude.ToString();
                    request.Dst.Lng = rr.FirstOrDefault().DstLongitude.ToString();
                    var recommendPathId = rr.FirstOrDefault().RecommendPathId;
                    if (recommendPathId > 0)
                    {
                        var point = new CoreExternalService.Models.Point();
                        var path =
                            dataModel.RecommendRoutes.Where(x => x.RecommendPathId == recommendPathId)
                                .OrderBy(x => x.RecommendPathSeq)
                                .ToList();
                        foreach (var vwPathRoute in path)
                        {
                            point = new CoreExternalService.Models.Point();
                            point.Lat = vwPathRoute.RecommendLat.ToString("G29");
                            point.Lng = vwPathRoute.RecommendLng.ToString("G29");
                            request.WayPoints.Add(point);
                        }
                    }
                    var img = _gService.GetMapImage(request);
                    var hasCar = (rr.FirstOrDefault().IsDrive) ? getResource.getString("WithCar") : "";
                    var timingModel =
                        _timingService.GetRequestTimings(new List<long>() {rr.FirstOrDefault().RouteRequestId});
                    string timing = _timingService.GetTimingString(timingModel);
                    string from = "";
                    from += (!string.IsNullOrWhiteSpace(rr.FirstOrDefault().SrcGAddress))
                        ? rr.FirstOrDefault().SrcGAddress + "، "
                        : "";
                    string to = "";
                    to += (!string.IsNullOrWhiteSpace(rr.FirstOrDefault().DstGAddress))
                        ? rr.FirstOrDefault().DstGAddress + "، "
                        : "";
                    var confirmMessage = string.Format(getResource.getMessage("ShareBody"), timing, hasCar, from, to);

                    mapImage.ShareImage = img;
                    res.ImageId = (string) imageId.ToString();
                    res.ImageCaption = confirmMessage;
                    mapImage.ShareCaption = confirmMessage;
                    res.ImagePath = "mibarim.ir/image?id=" + imageId;
                    mapImage.SharePath = "mibarim.ir/image?id=" + imageId;
                    dataModel.MapImages.Add(mapImage);
                    dataModel.SaveChanges();
                }
            }
            return res;
        }

        public ImageResponse GetMapImageById(ImageRequest model)
        {
            var res = new ImageResponse();
            using (var dataModel = new MibarimEntities())
            {
                var image = dataModel.MapImages.Where(x => x.MapShareGuid == model.ImageId).ToList();
                if (image.Count > 0)
                {
                    var img = image.FirstOrDefault();
                    res.ImageFile = img.ShareImage;
                    res.ImageId = img.MapShareGuid.ToString();
                    res.ImageType = ImageType.MapImage.ToString();
                    img.SeenCount++;
                    dataModel.SaveChanges();
                }
            }
            return res;
        }

        public LocalRouteUserModel GetRouteInfo(int userId, Guid routeUId)
        {
            var res = new LocalRouteUserModel();
            using (var dataModel = new MibarimEntities())
            {
                var route = dataModel.RouteRequests.Where(x => x.RouteRequestUId == routeUId);
                var theRoute = route.FirstOrDefault();
                var user = dataModel.vwUserInfoes.FirstOrDefault(x => x.UserId == theRoute.RouteRequestUserId);
                res = RouteMapper.CastRouteToRouteUser(theRoute, user);
                if (theRoute.RecommendPathId != null && theRoute.RecommendPathId != 0)
                {
                    var routePaths =
                        dataModel.vwPaths.Where(x => x.RecommendPathId == theRoute.RecommendPathId).ToList();
                    res.PathRoute.path = RouteMapper.CastRouteToPathRoute(routePaths);
                }
                var timings = _timingService.GetRequestTimings(route.Select(x => x.RouteRequestId).ToList());
                string timing = _timingService.GetTimingString(timings);
                res.RouteStartTime = timing;
            }
            return res;
        }

        public void InsertRideRequest(RouteRequestModel model, int userId)
        {
            var routeRequestIds = new List<long>();
            using (var dataModel = new MibarimEntities())
            {
                using (var dbContextTransaction = dataModel.Database.BeginTransaction())
                {
                    try
                    {
                        var carInfo = dataModel.CarInfoes.FirstOrDefault(x => x.UserId == userId);
                        var timings = RouteMapper.CastModelToRrTiming(model);
                        //var routeModel = RouteMapper.CastModelToRouteRequest(model, userId);
                        var pricingModel = RouteMapper.CastModelToRrPricing(model);
                        var uid = Guid.NewGuid();
                        foreach (var rrTiming in timings)
                        {
                            var rr = RouteMapper.CastModelToRouteRequest(model, userId);
                            rr.CarInfoId = carInfo != null ? carInfo.CarInfoId : 0;
                            rr.RouteRequestUId = uid;
                            dataModel.RouteRequests.Add(rr);
                            dataModel.SaveChanges();
                            rrTiming.RouteRequestId = rr.RouteRequestId;
                            dataModel.RRTimings.Add(rrTiming);
                            pricingModel.RouteRequestId = rr.RouteRequestId;
                            dataModel.RRPricings.Add(pricingModel);
                            dataModel.SaveChanges();
                            routeRequestIds.Add(rr.RouteRequestId);
                        }
                        dbContextTransaction.Commit();
                    }
                    catch (Exception)
                    {
                        dbContextTransaction.Rollback();
                    }
                }

                var riderRoutes = dataModel.RouteRequests.Where(x => x.RouteRequestUId == model.RouteUId).ToList();
                var routes = dataModel.RouteRequests.Where(x => routeRequestIds.Contains(x.RouteRequestId)).ToList();
                var theTimings = _timingService.GetRequestTimings(routeRequestIds);
                theTimings.AddRange(_timingService.GetRequestTimings(riderRoutes.Select(x => x.RouteRequestId).ToList()));
                foreach (var route in routes)
                {
                    //var pricings = GetRequestPricing(routeRequestIds);
                    foreach (var result in riderRoutes)
                    {
                        if (SimilarTiming(result, route, theTimings))
                        {
                            var selfRouteRequest =
                                dataModel.RouteRequests.FirstOrDefault(
                                    x => x.RouteRequestId == route.RouteRequestId);
                            selfRouteRequest.RouteRequestState = (int) RouteRequestState.Suggested;
                            var otherRouteRequest =
                                dataModel.RouteRequests.FirstOrDefault(
                                    x => x.RouteRequestId == result.RouteRequestId);
                            otherRouteRequest.RouteRequestState = (int) RouteRequestState.Suggested;
                            var routeSuggest = new RouteSuggest();
                            /*routeSuggest.SSrcDistance = (double)result.AltStartSec;
                            routeSuggest.SDstDistance = (double)result.AltEndSec;*/
                            routeSuggest.IsSuggestAccepted = false;
                            routeSuggest.IsSuggestDeleted = false;
                            routeSuggest.IsSuggestRejected = false;
                            routeSuggest.IsSuggestSeen = true;
                            routeSuggest.IsSuggestSent = true;
                            routeSuggest.RSuggestCreateTime = DateTime.Now;
                            routeSuggest.SelfRouteRequestId = (int) route.RouteRequestId;
                            routeSuggest.SuggestRouteRequestId = (int) result.RouteRequestId;
                            dataModel.RouteSuggests.Add(routeSuggest);
                            //add self suggestion too
                            var routeSelfSuggest = new RouteSuggest();
                            /*routeSelfSuggest.SSrcDistance = (double)result.AltStartSec;
                            routeSelfSuggest.SDstDistance = (double)result.AltEndSec;*/
                            routeSelfSuggest.IsSuggestAccepted = false;
                            routeSelfSuggest.IsSuggestDeleted = false;
                            routeSelfSuggest.IsSuggestRejected = false;
                            routeSelfSuggest.IsSuggestSeen = false;
                            routeSelfSuggest.IsSuggestSent = false;
                            routeSelfSuggest.RSuggestCreateTime = DateTime.Now;
                            routeSelfSuggest.SelfRouteRequestId = (int) result.RouteRequestId;
                            routeSelfSuggest.SuggestRouteRequestId = (int) route.RouteRequestId;
                            dataModel.RouteSuggests.Add(routeSelfSuggest);
                            dataModel.SaveChanges();
                            RequestRideShare(userId, (int) otherRouteRequest.RouteRequestId,
                                (int) selfRouteRequest.RouteRequestId);
                        }
                    }
                }
                dataModel.SaveChanges();
            }
            DoStuff(routeRequestIds, userId);
        }

        public ContactModel GetContactByRoutes(int routeId, int selfRouteId)
        {
            var res = new ContactModel();
            using (var dataModel = new MibarimEntities())
            {
                var route = dataModel.RouteRequests.FirstOrDefault(x => x.RouteRequestId == routeId);
                var selfRoute = dataModel.RouteRequests.FirstOrDefault(x => x.RouteRequestId == selfRouteId);
                var contact =
                    dataModel.Contacts.FirstOrDefault(
                        x =>
                            (x.ContactDriverUserId == route.RouteRequestUserId &&
                             x.ContactPassengerUserId == selfRoute.RouteRequestUserId)
                            ||
                            (x.ContactPassengerUserId == route.RouteRequestUserId &&
                             x.ContactDriverUserId == selfRoute.RouteRequestUserId)
                    );
                if (contact != null)
                {
                    res.ContactId = contact.ContactId;
                }
            }
            return res;
        }

        public DriverRouteModel GetRouteInfo(int userId, long driverRouteId)
        {
            var res = new DriverRouteModel();
            using (var dataModel = new MibarimEntities())
            {
                var r = dataModel.vwDriverRoutes.FirstOrDefault(x => x.DriverRouteId == driverRouteId);
                res.DriverRouteId = r.DriverRouteId;
                res.SrcAddress = r.SrcStAdd;
                res.SrcLat = r.SrcStLat.ToString();
                res.SrcLng = r.SrcStlng.ToString();
            }
            return res;
        }

        //public UserRouteModel GetTripProfile(int routeRequestId, int userId)
        //{
        //    var u = new UserRouteModel();
        //    using (var dataModel = new MibarimEntities())
        //    {
        //        var validateSuggestRoute =
        //            dataModel.vwTwoRouteSuggests.Where(x => x.SelfRRUserId == userId && x.SuggestRouteRequestId == routeRequestId).ToList();
        //        if (validateSuggestRoute.Count > 0)
        //        {
        //            var routeRequest = dataModel.vwRouteProfiles.FirstOrDefault(x => x.RouteRequestId == routeRequestId);
        //            var about = dataModel.AboutUsers.Where(x => x.UserId == routeRequest.UserId).ToList();

        //            var allSimilarrequests= dataModel.vwRouteRequests.Where(x => x.RouteRequestUId == routeRequest.RouteRequestUId);
        //            var theTimings = _timingService.GetRequestTimings(allSimilarrequests.Select(x=>x.RouteRequestId).ToList());
        //            u.RouteId = routeRequestId;
        //            u.Name = routeRequest.Name;
        //            u.Family= routeRequest.Family;
        //            u.Family = routeRequest.Family;
        //            if (about.Count >0)
        //            {
        //                var firstOrDefault = about.OrderByDescending(y => y.AboutCreateTime).FirstOrDefault();
        //                if (firstOrDefault != null)
        //                    u.UserAboutme = firstOrDefault.AboutDesc;
        //            }
        //            u.SrcAddress = routeRequest.SrcGAddress;
        //            u.DstAddress = routeRequest.DstGAddress;
        //            DateTime time = DateTime.Today.Add((TimeSpan)theTimings.FirstOrDefault().RRTheTime);
        //            u.TimingString = time.ToString("HH:mm");
        //            u=RouteMapper.AddResponseTiming(u, theTimings, routeRequest.RouteRequestUId);


        //            //suggestRoutes = GetBriefSuggestWeekRoutes(routeId);

        //        }
        //    }
        //    return u;
        //}

        void IRouteManager.DoCalc()
        {
            DoCalc();
        }

        public ContactStateModel ToggleContactState(int userId, long contactId)
        {
            var res = new ContactStateModel();
            using (var dataModel = new MibarimEntities())
            {
                var contact =
                    dataModel.Contacts.FirstOrDefault(
                        x =>
                            x.ContactId == contactId &&
                            (x.ContactPassengerUserId == userId || x.ContactDriverUserId == userId));

                if (contact != null)
                {
                    var isChat =
                        dataModel.Chats.Where(x => x.ContactId == contactId)
                            .Select(x => x.ChatUserId)
                            .Distinct()
                            .Count();
                    if (isChat == 1)
                    {
                        res.State = false;
                        res.Msg = getResource.getMessage("AppointmentNotSet");
                        return res;
                    }
                    var user = dataModel.vwUserInfoes.FirstOrDefault(x => x.UserId == userId);
                    NotifModel notifadmin = new NotifModel();
                    notifadmin.Title = user.Name + " " + user.Family + ":";
                    notifadmin.Body = getResource.getMessage("ActivateRideState");
                    notifadmin.Tab = (int) MainTabs.Message;
                    notifadmin.RequestCode = (int) NotificationType.TripStateActivated;
                    notifadmin.NotificationId = (int) NotificationType.TripStateActivated;
                    _notifManager.SendNotifToUser(notifadmin, 1);

                    if (contact.ContactDriverUserId == userId)
                    {
                        if (contact.ContactIsRideAccepted == null || (bool) !contact.ContactIsRideAccepted)
                        {
                            contact.ContactIsRideAccepted = true;
                            res.State = true;
                            var chat = new Chat();
                            chat.ChatCreateTime = DateTime.Now;
                            chat.ChatIsDeleted = false;
                            chat.ContactId = contactId;
                            chat.ChatUserId = userId;
                            chat.ChatTxt = getResource.getMessage("ActivateRideState");
                            dataModel.Chats.Add(chat);
                            contact.ContactLastMsgTime = DateTime.Now;
                            contact.ContactLastMsg = RouteMapper.Truncate(getResource.getMessage("ActivateRideState"),
                                29);
                            ;
                            dataModel.SaveChanges();
                            NotifModel notifModel = new NotifModel();
                            notifModel.Title = user.Name + " " + user.Family + ":";
                            notifModel.Body = getResource.getMessage("ActivateRideState");
                            notifModel.Tab = (int) MainTabs.Message;
                            notifModel.RequestCode = (int) NotificationType.TripStateActivated;
                            notifModel.NotificationId = (int) NotificationType.TripStateActivated;
                            _notifManager.SendNotifToUser(notifModel, contact.ContactPassengerUserId);
                        }
                        else
                        {
                            contact.ContactIsRideAccepted = false;
                            res.State = false;
                            var chat = new Chat();
                            chat.ChatCreateTime = DateTime.Now;
                            chat.ChatIsDeleted = false;
                            chat.ContactId = contactId;
                            chat.ChatUserId = userId;
                            chat.ChatTxt = getResource.getMessage("deactivateRideState");
                            dataModel.Chats.Add(chat);
                            contact.ContactLastMsgTime = DateTime.Now;
                            contact.ContactLastMsg = RouteMapper.Truncate(
                                getResource.getMessage("deactivateRideState"), 29);
                            dataModel.SaveChanges();
                            NotifModel notifModel = new NotifModel();
                            notifModel.Title = user.Name + " " + user.Family + ":";
                            notifModel.Body = getResource.getMessage("deactivateRideState");
                            notifModel.Tab = (int) MainTabs.Message;
                            notifModel.RequestCode = (int) NotificationType.TripStateActivated;
                            notifModel.NotificationId = (int) NotificationType.TripStateActivated;
                            _notifManager.SendNotifToUser(notifModel, contact.ContactPassengerUserId);
                        }
                    }
                    else if (contact.ContactPassengerUserId == userId)
                    {
                        if (contact.IsPassengerAccepted == null || (bool) !contact.IsPassengerAccepted)
                        {
                            contact.IsPassengerAccepted = true;
                            res.Msg = getResource.getMessage("AutomaticTransaction");
                            res.State = true;
                            var chat = new Chat();
                            chat.ChatCreateTime = DateTime.Now;
                            chat.ChatIsDeleted = false;
                            chat.ContactId = contactId;
                            chat.ChatUserId = userId;
                            chat.ChatTxt = getResource.getMessage("ActivateTripState");
                            dataModel.Chats.Add(chat);
                            contact.ContactLastMsgTime = DateTime.Now;
                            contact.ContactLastMsg = RouteMapper.Truncate(getResource.getMessage("ActivateTripState"),
                                29);
                            dataModel.SaveChanges();
                            NotifModel notifModel = new NotifModel();
                            notifModel.Title = user.Name + " " + user.Family + ":";
                            notifModel.Body = getResource.getMessage("ActivateTripState");
                            notifModel.Tab = (int) MainTabs.Message;
                            notifModel.RequestCode = (int) NotificationType.TripStateActivated;
                            notifModel.NotificationId = (int) NotificationType.TripStateActivated;
                            _notifManager.SendNotifToUser(notifModel, contact.ContactDriverUserId);
                        }
                        else
                        {
                            var chat = new Chat();
                            chat.ChatCreateTime = DateTime.Now;
                            chat.ChatIsDeleted = false;
                            chat.ContactId = contactId;
                            chat.ChatUserId = userId;
                            chat.ChatTxt = getResource.getMessage("DeactiveTripState");
                            dataModel.Chats.Add(chat);
                            contact.ContactLastMsgTime = DateTime.Now;
                            contact.ContactLastMsg = RouteMapper.Truncate(getResource.getMessage("DeactiveTripState"),
                                29);
                            dataModel.SaveChanges();
                            NotifModel notifModel = new NotifModel();
                            notifModel.Title = user.Name + " " + user.Family + ":";
                            notifModel.Body = getResource.getMessage("DeactiveTripState");
                            notifModel.Tab = (int) MainTabs.Message;
                            notifModel.RequestCode = (int) NotificationType.TripStateActivated;
                            notifModel.NotificationId = (int) NotificationType.TripStateActivated;
                            _notifManager.SendNotifToUser(notifModel, contact.ContactDriverUserId);
                            contact.IsPassengerAccepted = false;
                            res.State = false;
                        }
                    }
                    dataModel.SaveChanges();
                }
            }
            return res;
        }

        public ScoreModel GetUserScoresByRouteId(int userId, int routeRequestId)
        {
            var s = new ScoreModel();
            using (var dataModel = new MibarimEntities())
            {
                var route = dataModel.RouteRequests.FirstOrDefault(x => x.RouteRequestId == routeRequestId);
                s.Score =
                    dataModel.vwTripRoutes.Where(
                        x =>
                            x.RouteRequestUserId == route.RouteRequestUserId &&
                            x.TrState == (int) TripRouteState.TripRouteFinished).ToList().Count;
                if (route != null)
                {
                    var average =
                        dataModel.vwContactScores.Where(
                            x =>
                                (x.ContactDriverUserId == route.RouteRequestUserId) &&
                                x.CsUserId != x.ContactDriverUserId).Average(x => x.CsScore);
                    if (average !=
                        null)
                        s.ContactScore = (int) average;
                    var aboutMe = dataModel.AboutUsers.FirstOrDefault(x => x.UserId == route.RouteRequestUserId);
                    if (aboutMe != null) s.AboutMe = aboutMe.AboutDesc;

                    average +=
                        dataModel.vwContactScores.Where(
                            x =>
                                x.ContactDriverUserId == route.RouteRequestUserId &&
                                x.CsUserId != x.ContactPassengerUserId).Average(x => x.CsScore);
                    if (average !=
                        null)
                        s.ContactScore = (int) average/2;
                }
            }
            s.MoneySave = 0;
            return s;
        }

        public List<PassRouteModel> GetPassengerRoutes(int userId, PassFilterModel model)
        {
            var res = new List<PassRouteModel>();
            using (var dataModel = new MibarimEntities())
            {
                DateTime triptime = DateTime.Now.AddMinutes(-15);
                var dataList =
                    dataModel.vwDriverTrips.Where(
                        x =>
                            x.TStartTime > triptime &&
                            (x.TState == (int) TripState.Scheduled || x.TState == (int) TripState.InTripTime ||
                             x.TState == (int) TripState.InPreTripTime || x.TState == (int) TripState.InRiding));
                foreach (var trip in dataList)
                {
                    var filledSeats = 0;
                    var passRouteModel = new PassRouteModel();
                    var isbooked =
                        dataModel.vwBookPays.FirstOrDefault(
                            x => x.TripId == trip.TripId && x.PayReqRefID != null && x.PayReqUserId == userId);
                    passRouteModel.IsBooked = isbooked != null;
                    if (isbooked != null)
                    {
                        passRouteModel.MobileNo = trip.UserName;
                        passRouteModel.CarPlate = trip.CarPlateNo;
                    }
                    passRouteModel.TripId = trip.TripId;
                    passRouteModel.TripState = trip.TState;
                    passRouteModel.Name = trip.Name;
                    passRouteModel.Family = trip.Family;
                    passRouteModel.TimingString = trip.TStartTime.ToString("HH:mm");
                    passRouteModel.PricingString = trip.PassPrice.ToString();
                    passRouteModel.SrcAddress = trip.SrcStAdd;
                    passRouteModel.SrcLink = "https://www.google.com/maps/place/" + trip.SrcStLat + "," +
                                             trip.SrcStlng;
                    passRouteModel.DstAddress = trip.DstMainStName;
                    passRouteModel.DstLink = "https://www.google.com/maps/place/" + trip.DstMainStLat + "," +
                                             trip.DstMainStLng;
                    passRouteModel.UserImageId = trip.UserImageId;
                    passRouteModel.IsVerified = true;
                    passRouteModel.CarSeats = trip.TEmptySeat;
                    var tripUsers = dataModel.vwBookPays.Where(x => x.TripId == trip.TripId);
                    foreach (var tripUser in tripUsers)
                    {
                        if (tripUser.PayReqRefID != null)
                        {
                            filledSeats++;
                        }
                        else if (tripUser.BrCreateTime.AddMinutes(10) > DateTime.Now)
                        {
                            filledSeats++;
                        }
                    }
                    passRouteModel.EmptySeats = trip.TEmptySeat - filledSeats;
                    passRouteModel.CarString = trip.CarType + " " + trip.CarColor;
                    res.Add(passRouteModel);
                }
            }
            return res;
        }

        public PaymentDetailModel RequestBooking(int userId, long modelTripId)
        {
            var payreq = new PaymentDetailModel();
            using (var dataModel = new MibarimEntities())
            {
                var vwuser = dataModel.Fanaps.FirstOrDefault(x => x.userId == userId);
                if (vwuser != null)
                {
                    payreq.BankLink = "http://sandbox.fanapium.com:8080/pbc/buy-creditPack/?_token_=" +
                                      vwuser.access_token + "&_token_issuer_=1";
                    payreq.State = 100;
                }
                else
                {
                    var trip = dataModel.vwDriverTrips.FirstOrDefault(x => x.TripId == modelTripId);
                    if (trip != null)
                    {
                        var user = dataModel.vwUserInfoes.FirstOrDefault(x => x.UserId == userId);
                        payreq = _paymentManager.ChargeAccount(userId, (int) trip.PassPrice,
                            user.Name + " " + user.Family);
                        var bookreq = new BookRequest();
                        bookreq.TripId = modelTripId;
                        bookreq.BrCreateTime = DateTime.Now;
                        bookreq.PayReqId = payreq.ReqId;
                        dataModel.BookRequests.Add(bookreq);
                        dataModel.SaveChanges();
                    }
                }
            }
            return payreq;
        }

        public List<StationRouteModel> GetStationRoutes()
        {
            var res = new List<StationRouteModel>();
            using (var dataModel = new MibarimEntities())
            {
                var dataList = dataModel.vwStationRoutes.ToList();
                foreach (var station in dataList)
                {
                    var stationRouteModel = new StationRouteModel();
                    stationRouteModel.StRouteId = station.StationRouteId;
                    stationRouteModel.SrcStAdd = station.SrcMainStName;
                    stationRouteModel.SrcStLat = station.SrcMainStLat.ToString();
                    stationRouteModel.SrcStLng = station.SrcMainStLng.ToString();
                    stationRouteModel.SrcStId = station.SrcMStationId;
                    stationRouteModel.DstStAdd = station.DstMainStName;
                    stationRouteModel.DstStLat = station.DstMainStLat.ToString();
                    stationRouteModel.DstStLng = station.DstMainStLng.ToString();
                    stationRouteModel.DstStId = station.DstMStationId;
                    stationRouteModel.StRoutePrice = station.DriverPrice.ToString();
                    res.Add(stationRouteModel);
                }
            }
            return res;
        }

        public List<StationRouteModel> GetPassengerStationRoutes()
        {
            var res = new List<StationRouteModel>();
            using (var dataModel = new MibarimEntities())
            {
                var dataList = dataModel.vwStationRoutes.ToList();
                foreach (var station in dataList)
                {
                    var stationRouteModel = new StationRouteModel();
                    stationRouteModel.StRouteId = station.StationRouteId;
                    stationRouteModel.SrcStAdd = station.SrcMainStName;
                    stationRouteModel.SrcStLat = station.SrcMainStLat.ToString();
                    stationRouteModel.SrcStLng = station.SrcMainStLng.ToString();
                    stationRouteModel.SrcStId = station.SrcMStationId;
                    stationRouteModel.DstStAdd = station.DstMainStName;
                    stationRouteModel.DstStLat = station.DstMainStLat.ToString();
                    stationRouteModel.DstStLng = station.DstMainStLng.ToString();
                    stationRouteModel.DstStId = station.DstMStationId;
                    stationRouteModel.StRoutePrice = station.PassPrice.ToString();
                    res.Add(stationRouteModel);
                }
            }
            return res;
        }

        public bool SetUserRoute(int userId, long stRouteId, long stationId)
        {
            using (var dataModel = new MibarimEntities())
            {
                if (
                    dataModel.DriverRoutes.Any(
                        x => x.UserId == userId && x.StationRouteId == stRouteId && !x.DrIsDeleted))
                {
                    _responseProvider.SetBusinessMessage(new MessageResponse()
                    {
                        Type = ResponseTypes.Error,
                        Message = getResource.getMessage("RouteExist")
                    });
                    return false;
                }
                //var stationRoute = dataModel.StationRoutes.FirstOrDefault(x => x.StationRouteId == stRouteId);
                //var subStation =
                //    dataModel.Stations.OrderByDescending(x => x.StationId)
                //        .FirstOrDefault(x => x.MainStationId == stationRoute.SrcMStationId);
                var driverRoute = new DriverRoute();
                driverRoute.UserId = userId;
                driverRoute.DrIsDeleted = false;
                driverRoute.DrCreateTime = DateTime.Now;
                var car = dataModel.vwCarInfoes.FirstOrDefault(x => x.UserId == userId);
                if (car != null)
                {
                    driverRoute.CarinfoId = car.CarInfoId;
                }
                driverRoute.DrSrcStationId = stationId; //subStation.StationId;
                driverRoute.StationRouteId = stRouteId;
                dataModel.DriverRoutes.Add(driverRoute);
                dataModel.SaveChanges();
            }
            return true;
        }

        public List<DriverRouteModel> GetDriverRoutes(int userId)
        {
            var res = new List<DriverRouteModel>();
            using (var dataModel = new MibarimEntities())
            {
                var dataList = dataModel.vwDriverRoutes.Where(x => x.UserId == userId);
                foreach (var dr in dataList)
                {
                    var driverRouteModel = new DriverRouteModel();
                    var filledSeats = 0;
                    var lastTrip =
                        dataModel.Trips.Where(x => x.DriverRouteId == dr.DriverRouteId)
                            .OrderByDescending(x => x.TCreateTime)
                            .FirstOrDefault();
                    if (lastTrip != null)
                    {
                        driverRouteModel.TimingString = lastTrip.TStartTime.ToString("HH:mm");
                        driverRouteModel.TimingHour = lastTrip.TStartTime.Hour;
                        driverRouteModel.TimingMin = lastTrip.TStartTime.Minute;
                        driverRouteModel.HasTrip = false;
                        if (lastTrip.TState == (int) TripState.InTripTime || lastTrip.TState == (int) TripState.InRiding ||
                            lastTrip.TState == (int) TripState.InPreTripTime ||
                            lastTrip.TState == (int) TripState.Scheduled)
                        {
                            driverRouteModel.HasTrip = true;
                            driverRouteModel.TripState = lastTrip.TState;
                            driverRouteModel.TripId = lastTrip.TripId;
                        }
                        var tripUsers = dataModel.vwBookPays.Where(x => x.TripId == lastTrip.TripId);
                        foreach (var tripUser in tripUsers)
                        {
                            if (tripUser.PayReqRefID != null)
                            {
                                filledSeats++;
                            }
                            else if (tripUser.BrCreateTime.AddMinutes(10) > DateTime.Now)
                            {
                                filledSeats++;
                            }
                        }
                        driverRouteModel.FilledSeats = (short) filledSeats;
                        driverRouteModel.CarSeats = lastTrip.TEmptySeat;
                    }
                    else
                    {
                        driverRouteModel.TimingString = "-:-";
                        driverRouteModel.HasTrip = false;
                        driverRouteModel.FilledSeats = 0;
                        driverRouteModel.CarSeats = 0;
                    }
                    driverRouteModel.DriverRouteId = dr.DriverRouteId;
                    driverRouteModel.SrcAddress = dr.SrcStAdd;
                    driverRouteModel.SrcLink = "https://www.google.com/maps/place/" + dr.SrcStLat + "," + dr.SrcStlng;
                    driverRouteModel.SrcLat = dr.SrcStLat.ToString();
                    driverRouteModel.SrcLng = dr.SrcStlng.ToString();
                    driverRouteModel.DstAddress = dr.DstMainStName;
                    driverRouteModel.DstLink = "https://www.google.com/maps/place/" + dr.DstMainStLat + "," +
                                               dr.DstMainStLng;
                    driverRouteModel.DstLat = dr.DstMainStLat.ToString();
                    driverRouteModel.DstLng = dr.DstMainStLng.ToString();
                    driverRouteModel.DriverRouteId = dr.DriverRouteId;
                    driverRouteModel.PricingString = dr.DriverPrice.ToString();
                    driverRouteModel.CarString = dr.CarType + " " + dr.CarColor + " " + dr.CarPlateNo;
                    res.Add(driverRouteModel);
                }
            }
            return res;
        }

        public TripTimeModel SetDriverTrip(int userId, DriverRouteModel model)
        {
            var res = new TripTimeModel();
            using (var dataModel = new MibarimEntities())
            {
                var driveTrip =
                    dataModel.vwDriverTrips.FirstOrDefault(
                        x => x.UserId == userId && x.DriverRouteId == model.DriverRouteId &&
                             x.TState == (int) TripState.Scheduled);
                if (driveTrip != null)
                {
                    _responseProvider.SetBusinessMessage(new MessageResponse()
                    {
                        Type = ResponseTypes.Error,
                        Message = getResource.getMessage("TripAlreadyEnabled")
                    });
                    res.IsSubmited = false;
                }
                else
                {
                    var trip = new Trip();
                    trip.TStartTime = GetNextDateTime(model.TimingHour, model.TimingMin);
                    res.RemainHour = (int) (trip.TStartTime - DateTime.Now).TotalHours;
                    var remainMin = (int) (trip.TStartTime - DateTime.Now).TotalMinutes;
                    res.RemainMin = (remainMin % 60);
                    trip.DriverRouteId = model.DriverRouteId;
                    trip.TCreateTime = DateTime.Now;
                    trip.TEmptySeat = model.CarSeats;
                    trip.TState = (int) TripState.Scheduled;
                    dataModel.Trips.Add(trip);
                    dataModel.SaveChanges();
                }
            }
            res.IsSubmited = true;
            return res;
        }

        public string InvokeTrips()
        {
            using (var dataModel = new MibarimEntities())
            {
                var activeTrips =
                    dataModel.Trips.Where(
                        x =>
                            x.TState == (int) TripState.Scheduled || x.TState == (int) TripState.InTripTime ||
                            x.TState == (int) TripState.InPreTripTime);
                foreach (var preTrip in activeTrips.Where(x => x.TState == (int) TripState.Scheduled))
                {
                    if (preTrip.TStartTime.AddMinutes(-15) < DateTime.Now)
                    {
                        preTrip.TState = (int) TripState.InPreTripTime;
                    }
                }
                foreach (var activeTrip in activeTrips.Where(x => x.TState == (int) TripState.InPreTripTime))
                {
                    if (activeTrip.TStartTime < DateTime.Now)
                    {
                        activeTrip.TState = (int) TripState.InTripTime;
                        var driveRoute =
                            dataModel.DriverRoutes.FirstOrDefault(x => x.DriverRouteId == activeTrip.DriverRouteId);
                        var usr = dataModel.vwUserInfoes.FirstOrDefault(x => x.UserId == driveRoute.UserId);
                        var msg = string.Format(getResource.getMessage("PayForSetTrip"),
                            RouteMapper.GetUserNameFamilyString(usr), 1000);
                        _transactionManager.ChargeAccount((int) driveRoute.UserId, 1000, msg,TransactionType.CreditChargeAccount);
                    }
                }
                //var doingTrips = dataModel.Trips.Where(x => x.TState == (int)TripState.InTripTime);
                foreach (var doingTrip in activeTrips.Where(x => x.TState == (int) TripState.InTripTime))
                {
                    if (doingTrip.TStartTime.AddMinutes(15) < DateTime.Now)
                    {
                        doingTrip.TState = (int) TripState.DriverNotCome;
                    }
                }
                foreach (
                    var doingTrip in
                    activeTrips.Where(x => x.TState == (int) TripState.InRiding || x.TState == (int) TripState.InDriving)
                )
                {
                    if (doingTrip.TStartTime.AddMinutes(30) < DateTime.Now)
                    {
                        doingTrip.TState = (int) TripState.FinishedByTime;
                    }
                }
                dataModel.SaveChanges();
            }
            return "Done";
        }

        public bool DeleteDriverRoute(int userId, long modelDriverRouteId)
        {
            using (var dataModel = new MibarimEntities())
            {
                var rm =
                    dataModel.DriverRoutes.FirstOrDefault(
                        x => x.UserId == userId && x.DriverRouteId == modelDriverRouteId);
                if (rm == null)
                {
                    _responseProvider.SetBusinessMessage(new MessageResponse()
                    {
                        Type = ResponseTypes.Error,
                        Message = getResource.getMessage("RouteNotExist")
                    });
                    return false;
                }
                var rt =
                    dataModel.Trips.FirstOrDefault(
                        x => x.TState == (int) TripState.Scheduled && x.DriverRouteId == modelDriverRouteId);
                if (rt != null)
                {
                    _responseProvider.SetBusinessMessage(new MessageResponse()
                    {
                        Type = ResponseTypes.Error,
                        Message = getResource.getMessage("TripExists")
                    });
                    return false;
                }
                rm.DrIsDeleted = true;
                dataModel.SaveChanges();
            }
            return true;
        }

        public bool DisableDriverTrip(int userId, DriverRouteModel model)
        {
            using (var dataModel = new MibarimEntities())
            {
                var driveTrip =
                    dataModel.vwDriverTrips.FirstOrDefault(
                        x => x.UserId == userId && x.DriverRouteId == model.DriverRouteId &&
                             x.TState == (int) TripState.Scheduled);
                if (driveTrip != null)
                {
                    var tripUsers = dataModel.vwBookPays.Where(x => x.TripId == driveTrip.TripId);
                    foreach (var tripUser in tripUsers)
                    {
                        if (tripUser.PayReqRefID != null)
                        {
                            _responseProvider.SetBusinessMessage(new MessageResponse()
                            {
                                Type = ResponseTypes.Error,
                                Message = getResource.getMessage("TripAlreadySet")
                            });
                            return false;
                        }
                        else if (tripUser.BrCreateTime.AddMinutes(10) > DateTime.Now)
                        {
                            _responseProvider.SetBusinessMessage(new MessageResponse()
                            {
                                Type = ResponseTypes.Error,
                                Message = getResource.getMessage("TripAlreadySet")
                            });
                            return false;
                        }
                    }
                    var trip = dataModel.Trips.FirstOrDefault(x => x.TripId == driveTrip.TripId);
                    trip.TState = (int) TripState.CanceledByUser;
                    dataModel.SaveChanges();
                }
                else
                {
                    _responseProvider.SetBusinessMessage(new MessageResponse()
                    {
                        Type = ResponseTypes.Error,
                        Message = getResource.getMessage("TripAlreadyDisabled")
                    });
                    return false;
                }
            }
            return true;
        }

        public DriverTripModel GetUserTrips(int userId)
        {
            var res = new DriverTripModel();
            using (var dataModel = new MibarimEntities())
            {
                var trip =
                    dataModel.vwDriverTrips.FirstOrDefault(
                        x =>
                            x.UserId == userId &&
                            (x.TState == (int) TripState.InRiding || x.TState == (int) TripState.InTripTime ||
                             x.TState == (int) TripState.InRanking));
                if (trip != null)
                {
                    res.FilledSeats = 0;
                    var tripUsers = dataModel.vwBookPays.Where(x => x.TripId == trip.TripId);
                    foreach (var tripUser in tripUsers)
                    {
                        if (tripUser.PayReqRefID != null)
                        {
                            res.FilledSeats++;
                        }
                        else if (tripUser.BrCreateTime.AddMinutes(10) > DateTime.Now)
                        {
                            res.FilledSeats++;
                        }
                    }
                    res.DriverRouteId = trip.DriverRouteId;
                    res.StAddress = trip.SrcStAdd;
                    res.StLat = trip.SrcStLat.ToString();
                    res.StLng = trip.SrcStlng.ToString();
                    res.StLink = "https://www.google.com/maps/place/" + trip.SrcStLat + "," +
                                 trip.SrcStlng;
                    res.TripId = trip.TripId;
                    res.TripState = trip.TState;
                }
            }
            return res;
        }

        public DriverTripModel SetTripLocation(int userId, DriverTripModel model)
        {
            using (var dataModel = new MibarimEntities())
            {
                var triplocation = new TripLocation();
                triplocation.TlCreateTime = DateTime.Now;
                triplocation.TripState = (short) model.TripState;
                triplocation.TlUserId = userId;
                triplocation.TlLat = decimal.Parse(model.DriverLat);
                triplocation.TlLng = decimal.Parse(model.DriverLng);
                triplocation.TlGeo = RouteMapper.CreatePoint(model.DriverLat, model.DriverLng);
                triplocation.TripId = model.TripId;
                dataModel.TripLocations.Add(triplocation);
                dataModel.SaveChanges();
                var ct = dataModel.Trips.FirstOrDefault(x => x.TripId == model.TripId);
                if (ct.TState < model.TripState)
                {
                    ct.TState = (short) model.TripState;
                    dataModel.SaveChanges();
                }
            }
            return model;
        }

        public PassRouteModel SetPassLocation(int userId, PassRouteModel model)
        {
            var res = new PassRouteModel();
            using (var dataModel = new MibarimEntities())
            {
                var triplocation = new TripLocation();
                triplocation.TlCreateTime = DateTime.Now;
                triplocation.TripState = (short) model.TripState;
                triplocation.TlUserId = userId;
                triplocation.TlLat = decimal.Parse(model.SrcLatitude);
                triplocation.TlLng = decimal.Parse(model.SrcLongitude);
                triplocation.TlGeo = RouteMapper.CreatePoint(model.SrcLatitude, model.SrcLongitude);
                triplocation.TripId = model.TripId;
                dataModel.TripLocations.Add(triplocation);
                dataModel.SaveChanges();
                var ct = dataModel.Trips.FirstOrDefault(x => x.TripId == model.TripId);
                var driveModel = dataModel.DriverRoutes.FirstOrDefault(x => x.DriverRouteId == ct.DriverRouteId);
                var tl =
                    dataModel.TripLocations.Where(x => x.TripId == model.TripId && x.TlUserId == driveModel.UserId)
                        .OrderByDescending(x => x.TlCreateTime)
                        .ToList();
                var geoSum = new List<GeoCoordinate>();
                int max = tl.Count > 5 ? 5 : tl.Count;
                for (int i = 0; i < max; i++)
                {
                    var geo = new GeoCoordinate((double) tl[i].TlLat, (double) tl[i].TlLng);
                    geoSum.Add(geo);
                }
                if (geoSum.Count > 0)
                {
                    var geores = GetCentralGeoCoordinate(geoSum);
                    res.SrcLatitude = geores.Latitude.ToString();
                    res.SrcLongitude = geores.Longitude.ToString();
                }
            }
            return res;
        }

        public List<SubStationModel> GetStations(long stRouteId)
        {
            var res = new List<SubStationModel>();
            using (var dataModel = new MibarimEntities())
            {
                var stationRoute = dataModel.StationRoutes.FirstOrDefault(x => x.StationRouteId == stRouteId);
                var subStation =
                    dataModel.Stations.OrderByDescending(x => x.StationId)
                        .Where(x => x.MainStationId == stationRoute.SrcMStationId);
                foreach (var station in subStation)
                {
                    var st = new SubStationModel();
                    st.StAdd = station.StAdd;
                    st.StLat = station.StLat.ToString();
                    st.StLng = station.StLng.ToString();
                    st.StationId = station.StationId;
                    res.Add(st);
                }
            }
            return res;
        }

        public GeoCoordinate GetCentralGeoCoordinate(
            IList<GeoCoordinate> geoCoordinates)
        {
            if (geoCoordinates.Count == 1)
            {
                return geoCoordinates.Single();
            }

            double x = 0;
            double y = 0;
            double z = 0;

            foreach (var geoCoordinate in geoCoordinates)
            {
                var latitude = geoCoordinate.Latitude*Math.PI/180;
                var longitude = geoCoordinate.Longitude*Math.PI/180;

                x += Math.Cos(latitude)*Math.Cos(longitude);
                y += Math.Cos(latitude)*Math.Sin(longitude);
                z += Math.Sin(latitude);
            }

            var total = geoCoordinates.Count;

            x = x/total;
            y = y/total;
            z = z/total;

            var centralLongitude = Math.Atan2(y, x);
            var centralSquareRoot = Math.Sqrt(x*x + y*y);
            var centralLatitude = Math.Atan2(z, centralSquareRoot);

            return new GeoCoordinate(centralLatitude*180/Math.PI, centralLongitude*180/Math.PI);
        }

        private DateTime GetNextDateTime(int hour, int min)
        {
            DateTime current = DateTime.Now;
            DateTime nextDatetime = DateTime.Now;
            nextDatetime = new DateTime(current.Year, current.Month, current.Day, hour, min, 0);
            if (nextDatetime < current)
            {
                nextDatetime = new DateTime(current.Year, current.Month, current.AddDays(1).Day, hour, min, 0);
            }
            /*int minuteUntilNext = (min - current.Minute + 60)%60;
            int hoursUntilNext = (hour - current.Hour + 24)%24;
            if (min + current.Minute > 60)
            {
                hoursUntilNext--;
            }
            DateTime nextDatetime = current.AddHours(hoursUntilNext).AddMinutes(minuteUntilNext);*/
            return nextDatetime;
        }

        private async Task DoCalc()
        {
            await Task.Run(() => { calculate(); });
        }

        private async Task calculate()
        {
            var rnd = new Random();
            using (var dataModel = new MibarimEntities())
            {
                var prePathes = dataModel.RouteRequestGPaths.Select(x => x.RouteRequestUId).ToList();
                var routes =
                    dataModel.RouteRequests.Where(
                        x =>
                            !prePathes.Contains((Guid) x.RouteRequestUId) && x.RRIsDeleted == false &&
                            x.RRIsConfirmed == 1).GroupBy(x => x.RouteRequestUId);
                foreach (var routeRequest in routes)
                {
                    var route = dataModel.RouteRequests.FirstOrDefault(x => x.RouteRequestUId == routeRequest.Key);
                    Thread.Sleep(rnd.Next(1000, 5000));
                    SaveRouteGroutes(route.RouteRequestId);
                }
            }
        }


        private double RemoveDecimal(double? priceValue)
        {
            if (priceValue != null)
            {
                return Math.Round((priceValue.Value)/1000, 0)*1000;
            }
            return 0;
        }

        private double RemoveDecimalToman(double? priceValue)
        {
            if (priceValue != null)
            {
                return Math.Round((priceValue.Value)/1000, 0)*100;
            }
            return 0;
        }

        private double Toman(double? priceValue)
        {
            if (priceValue != null)
            {
                return Math.Round((priceValue.Value)/10, 0);
            }
            return 0;
        }

        //        private bool IsTehran(string srcLat, string srcLng)
        //        {
        //            var srcLatitude = double.Parse(srcLat);
        //            var srcLongitude = double.Parse(srcLng);
        //            if (srcLatitude < 35.799715 && srcLatitude > 35.588923 && srcLongitude < 51.634026 && srcLongitude > 51.180840)
        //            {
        //                return true;
        //            }
        //            return false;
        //        }
        public bool CheckConfirmationText(int routeId, string confirmText)
        {
            vwRouteRequest route;
            using (var dataModel = new MibarimEntities())
            {
                route = dataModel.vwRouteRequests.FirstOrDefault(x => x.RouteRequestId == routeId);
            }
            return (route.ConfirmatedText.GetHashCode() == confirmText.GetHashCode());
        }

        #endregion

        #region Private Methods

        private bool SimilarTiming(GenerateSimilarRoutes_Result similarRoute, RouteRequest route,
            List<vwRRTiming> timings)
        {
            bool similar = false;
            var routeTimings = timings.Where(x => x.RouteRequestId == route.RouteRequestId);
            var similarRouteTimings = timings.Where(x => x.RouteRequestId == similarRoute.RouteRequestId);
            foreach (var routeTiming in routeTimings)
            {
                foreach (var similarRouteTiming in similarRouteTimings)
                {
                    similar = _timingService.IsSimilarTiming(route, similarRoute, routeTiming, similarRouteTiming);
                }
            }
            return similar;
        }

        private bool SimilarTiming(RouteRequest similarRoute, RouteRequest route, List<vwRRTiming> timings)
        {
            bool similar = false;
            var routeTimings = timings.Where(x => x.RouteRequestId == route.RouteRequestId);
            var similarRouteTimings = timings.Where(x => x.RouteRequestId == similarRoute.RouteRequestId);
            foreach (var routeTiming in routeTimings)
            {
                foreach (var similarRouteTiming in similarRouteTimings)
                {
                    similar = _timingService.IsSimilarTiming(route, similarRoute, routeTiming, similarRouteTiming);
                }
            }
            return similar;
        }

        private string GetRouteMessage(int userId, List<long> routeIds)
        {
            string confirmMessage;
            string hasCar;
            vwRouteRequest routeModel;
            vwRRPricing pricingModel;
            List<vwRRTiming> timingModel;
            using (var dataModel = new MibarimEntities())
            {
                routeModel =
                    dataModel.vwRouteRequests.FirstOrDefault(
                        x => x.RouteRequestId == routeIds.FirstOrDefault() && x.UserId == userId);
                timingModel = _timingService.GetRequestTimings(routeIds);
                pricingModel =
                    dataModel.vwRRPricings.FirstOrDefault(
                        x => x.RouteRequestId == routeIds.FirstOrDefault() && x.UserId == userId);
            }
            hasCar = (routeModel.IsDrive) ? getResource.getString("WithCar") : getResource.getString("WithoutCar");
            string timing = _timingService.GetTimingString(timingModel);
            string from = "";
            from += (!string.IsNullOrWhiteSpace(routeModel.SrcGAddress)) ? routeModel.SrcGAddress + "، " : "";
            from += (!string.IsNullOrWhiteSpace(routeModel.SrcDetailAddress)) ? routeModel.SrcDetailAddress + " " : "";
            string to = "";
            to += (!string.IsNullOrWhiteSpace(routeModel.DstGAddress)) ? routeModel.DstGAddress + "، " : "";
            to += (!string.IsNullOrWhiteSpace(routeModel.DstDetailAddress)) ? routeModel.DstDetailAddress + " " : "";
            var pricing = GetPriceMessage(routeModel, pricingModel);
            confirmMessage = string.Format(getResource.getMessage("RouteBody"), hasCar, timing, from, to, pricing);
            return confirmMessage;
        }

        private string GetPriceMessage(vwRouteRequest routeRequest, vwRRPricing routeRequestPricing)
        {
            string pricing = "";
            switch (routeRequestPricing.RRPricingOption)
            {
                case (int) PricingOptions.Free:
                    pricing = getResource.getMessage(routeRequest.IsDrive ? "FreeDrive" : "FreeRide");
                    break;
                case (int) PricingOptions.NoMatter:
                    pricing = getResource.getMessage("EveryPrice");
                    break;
                case (int) PricingOptions.MinMax:
                    var pricingPattern = ((bool) routeRequest.IsDrive)
                        ? getResource.getMessage("MinPrice")
                        : getResource.getMessage("MaxPrice");
                    pricing = string.Format(pricingPattern, (int) routeRequest.RRPricingMinMax);
                    break;
            }
            return pricing;
        }

        private int GetSuggestRoutesCount(int routeRequestId)
        {
            var suggestRoutes = new List<BriefRouteModel>();
            using (var dataModel = new MibarimEntities())
            {
//                var vwRouteSuggests = dataModel.vwDoubleSuggests.Where(x => x.SelfRouteRequestId == routeRequestId && !x.SelfIsSuggestAccepted && !x.SelfIsSuggestRejected ).ToList();
                var vwRouteSuggests =
                    dataModel.vwRouteSuggests.Where(
                            x => x.SelfRouteRequestId == routeRequestId && !x.IsSuggestAccepted && !x.IsSuggestRejected)
                        .ToList();
                //                var excludedSuggestions = _routeGroupManager.GetExcludeRouteRequestIds(routeRequestId).ToList();
                //                vwRouteSuggests = vwRouteSuggests.Where(y => !excludedSuggestions.Contains(y.RouteRequestId)).ToList();
                return vwRouteSuggests.Count;
            }
        }

        private List<BriefRouteModel> GetBriefSuggestRoutes(int routeRequestId, bool isAdmin = false)
        {
            var suggestRoutes = new List<BriefRouteModel>();
            using (var dataModel = new MibarimEntities())
            {
                var vwRouteSuggests = new List<vwRouteSuggest>();
                if (isAdmin)
                {
                    vwRouteSuggests =
                        dataModel.vwRouteSuggests.Where(
                            x => x.SelfRouteRequestId == routeRequestId && !x.IsSuggestRejected).ToList();
                }
                else
                {
                    vwRouteSuggests =
                        dataModel.vwRouteSuggests.Where(
                                x =>
                                    x.SelfRouteRequestId == routeRequestId && !x.IsSuggestAccepted &&
                                    !x.IsSuggestRejected)
                            .ToList();
                }

                /*var excludedSuggestions = _routeGroupManager.GetExcludeRouteRequestIds(routeRequestId).ToList();
                vwRouteSuggests = vwRouteSuggests.Where(y => !excludedSuggestions.Contains(y.RouteRequestId)).ToList();*/
                if (vwRouteSuggests.Count > 0)
                {
                    var timings =
                        _timingService.GetRequestTimings(vwRouteSuggests.Select(x => x.RouteRequestId).ToList());
                    foreach (var vwRouteSuggest in vwRouteSuggests)
                    {
                        var briefRouteModel = new BriefRouteModel();
                        briefRouteModel.RouteId = (int) vwRouteSuggest.RouteRequestId;
                        briefRouteModel.Name = vwRouteSuggest.Name;
                        briefRouteModel.Family = vwRouteSuggest.Family;
                        briefRouteModel.UserImageId = vwRouteSuggest.UserImageId;
                        briefRouteModel.SrcAddress = vwRouteSuggest.SrcGAddress;
                        briefRouteModel.SrcLatitude = vwRouteSuggest.SrcLatitude.ToString();
                        briefRouteModel.SrcLongitude = vwRouteSuggest.SrcLongitude.ToString();
                        briefRouteModel.DstAddress = vwRouteSuggest.DstGAddress;
                        briefRouteModel.DstLatitude = vwRouteSuggest.DstLatitude.ToString();
                        briefRouteModel.DstLongitude = vwRouteSuggest.DstLongitude.ToString();
                        briefRouteModel.IsDrive = vwRouteSuggest.IsDrive;
                        briefRouteModel.IsSuggestSeen = vwRouteSuggest.IsSuggestSeen;
                        briefRouteModel.AccompanyCount = 0;
                        var srcDistance = vwRouteSuggest.SSrcDistance.ToString("N0", new NumberFormatInfo()
                        {
                            NumberGroupSizes = new[] {3},
                            NumberGroupSeparator = ","
                        });
                        briefRouteModel.SrcDistance = string.Format(getResource.getMessage("Meter"), srcDistance);
                        var dstDistance = vwRouteSuggest.SDstDistance.ToString("N0", new NumberFormatInfo()
                        {
                            NumberGroupSizes = new[] {3},
                            NumberGroupSeparator = ","
                        });
                        briefRouteModel.DstDistance = string.Format(getResource.getMessage("Meter"), dstDistance);
                        //based on our business model we get 1000 toman from driver
                        //ServiceWage.Fee = (double)vwRouteSuggest.RRPricingMinMax;
                        var thePrice = (decimal) vwRouteSuggest.RRPricingMinMax; // - ServiceWage.WageDecimal;
                        briefRouteModel.PricingString = _pricingManager.GetPriceString(new RouteRequestModel()
                        {
                            PriceOption = (PricingOptions) vwRouteSuggest.RRPricingOption,
                            //based on our business model we get 1000 toman from driver
                            CostMinMax = thePrice,
                            IsDrive = (bool) vwRouteSuggest.IsDrive
                        });
                        briefRouteModel.TimingString =
                            _timingService.GetTimingString(
                                timings.Where(y => y.RouteRequestId == vwRouteSuggest.RouteRequestId).ToList());
                        briefRouteModel.CarString = GetCarInfoString(vwRouteSuggest);
                        if (vwRouteSuggest.RecommendPathId != null && vwRouteSuggest.RecommendPathId != 0)
                        {
                            var routePaths =
                                dataModel.vwPaths.Where(x => x.RecommendPathId == vwRouteSuggest.RecommendPathId)
                                    .ToList();
                            briefRouteModel.PathRoute.path = RouteMapper.CastRouteToPathRoute(routePaths);
                        }
                        suggestRoutes.Add(briefRouteModel);
                    }
                    // suggestion seen
                    var suggs = dataModel.RouteSuggests.Where(y => routeRequestId == y.SelfRouteRequestId);
                    foreach (var routeSuggest in suggs)
                    {
                        routeSuggest.IsSuggestSent = true;
                        routeSuggest.IsSuggestSeen = true;
                    }
                    dataModel.SaveChanges();
                }
            }
            return suggestRoutes;
        }

        private List<BriefRouteModel> GetBriefSuggestWeekRoutes(int routeRequestId, bool isAdmin = false)
        {
            var suggestRoutes = new List<BriefRouteModel>();
            using (var dataModel = new MibarimEntities())
            {
                var route = dataModel.RouteRequests.FirstOrDefault(x => x.RouteRequestId == routeRequestId);
                var siblings =
                    dataModel.RouteRequests.Where(x => x.RouteRequestUId == route.RouteRequestUId)
                        .Select(y => y.RouteRequestId);
                var vwRouteSuggests = new List<vwRouteSuggest>();
                if (isAdmin)
                {
                    vwRouteSuggests =
                        dataModel.vwRouteSuggests.Where(
                            x => siblings.Contains(x.SelfRouteRequestId) && !x.IsSuggestRejected).ToList();
                }
                else
                {
                    vwRouteSuggests =
                        dataModel.vwRouteSuggests.Where(
                                x =>
                                    siblings.Contains(x.SelfRouteRequestId) && !x.IsSuggestAccepted &&
                                    !x.IsSuggestRejected)
                            .ToList();
                }
                var vwRouteSuggestsGroup = vwRouteSuggests.GroupBy(x => x.UserId);
                /*var excludedSuggestions = _routeGroupManager.GetExcludeRouteRequestIds(routeRequestId).ToList();
                vwRouteSuggests = vwRouteSuggests.Where(y => !excludedSuggestions.Contains(y.RouteRequestId)).ToList();*/
                if (vwRouteSuggests.Count > 0)
                {
                    foreach (var everyvwRouteSuggest in vwRouteSuggestsGroup)
                    {
                        var vwRouteSuggest =
                            vwRouteSuggests.FirstOrDefault(x => x.UserId == everyvwRouteSuggest.Key);
                        var timings = _timingService.GetRequestTimings(new List<long>() {vwRouteSuggest.RouteRequestId});
                        var briefRouteModel = new BriefRouteModel();
                        briefRouteModel.RouteId = (int) vwRouteSuggest.RouteRequestId;
                        briefRouteModel.Name = vwRouteSuggest.Name;
                        briefRouteModel.Family = vwRouteSuggest.Family;
                        briefRouteModel.UserImageId = vwRouteSuggest.UserImageId;
                        briefRouteModel.SrcAddress = vwRouteSuggest.SrcGAddress;
                        briefRouteModel.SrcLatitude = vwRouteSuggest.SrcLatitude.ToString();
                        briefRouteModel.SrcLongitude = vwRouteSuggest.SrcLongitude.ToString();
                        briefRouteModel.DstAddress = vwRouteSuggest.DstGAddress;
                        briefRouteModel.DstLatitude = vwRouteSuggest.DstLatitude.ToString();
                        briefRouteModel.DstLongitude = vwRouteSuggest.DstLongitude.ToString();
                        briefRouteModel.IsDrive = vwRouteSuggest.IsDrive;
                        briefRouteModel.IsSuggestSeen = vwRouteSuggest.IsSuggestSeen;
                        briefRouteModel.AccompanyCount = 0;
                        var srcDistance = vwRouteSuggest.SSrcDistance.ToString("N0", new NumberFormatInfo()
                        {
                            NumberGroupSizes = new[] {3},
                            NumberGroupSeparator = ","
                        });
                        briefRouteModel.SrcDistance = string.Format(getResource.getMessage("Meter"), srcDistance);
                        var dstDistance = vwRouteSuggest.SDstDistance.ToString("N0", new NumberFormatInfo()
                        {
                            NumberGroupSizes = new[] {3},
                            NumberGroupSeparator = ","
                        });
                        briefRouteModel.DstDistance = string.Format(getResource.getMessage("Meter"), dstDistance);
                        //based on our business model we get 1000 toman from driver
                        //ServiceWage.Fee = (double)vwRouteSuggest.RRPricingMinMax;
                        var thePrice = (decimal) vwRouteSuggest.RRPricingMinMax; // - ServiceWage.WageDecimal;
                        briefRouteModel.PricingString = _pricingManager.GetPriceString(new RouteRequestModel()
                        {
                            PriceOption = (PricingOptions) vwRouteSuggest.RRPricingOption,
                            //based on our business model we get 1000 toman from driver
                            CostMinMax = thePrice,
                            IsDrive = (bool) vwRouteSuggest.IsDrive
                        });
                        DateTime time = DateTime.Today.Add((TimeSpan) timings.FirstOrDefault().RRTheTime);
                        briefRouteModel.TimingString = time.ToString("HH:mm");
                        briefRouteModel.CarString = GetCarInfoString(vwRouteSuggest);
                        if (vwRouteSuggest.RecommendPathId != null && vwRouteSuggest.RecommendPathId != 0)
                        {
                            var routePaths =
                                dataModel.vwPaths.Where(x => x.RecommendPathId == vwRouteSuggest.RecommendPathId)
                                    .ToList();
                            briefRouteModel.PathRoute.path = RouteMapper.CastRouteToPathRoute(routePaths);
                        }
                        suggestRoutes.Add(briefRouteModel);
                    }
                    // suggestion seen
                    var suggs = dataModel.RouteSuggests.Where(y => routeRequestId == y.SelfRouteRequestId);
                    foreach (var routeSuggest in suggs)
                    {
                        routeSuggest.IsSuggestSent = true;
                        routeSuggest.IsSuggestSeen = true;
                    }
                    dataModel.SaveChanges();
                }
            }
            return suggestRoutes;
        }


        private string GetCarInfoString(vwRouteRequest routeRequest)
        {
            var carString = "";
            if (routeRequest.CarPlateNo == null)
            {
                carString = routeRequest.CarType + routeRequest.CarColor;
            }
            else
            {
                carString = string.Format(getResource.getMessage("CarInfoStr"), routeRequest.CarType,
                    routeRequest.CarColor,
                    routeRequest.CarPlateNo);
            }
            return carString;
        }

        private string GetCarInfoString(CarInfo carInfo)
        {
            var carString = "";
            if (carInfo.CarPlateNo == null)
            {
                carString = carInfo.CarType + carInfo.CarColor;
            }
            else
            {
                carString = string.Format(getResource.getMessage("CarInfoStr"), carInfo.CarType,
                    carInfo.CarColor,
                    carInfo.CarPlateNo);
            }
            return carString;
        }

        private string GetCarInfoString(vwRouteSuggest routeRequest)
        {
            var carString = "";
            if (routeRequest.CarPlateNo == null)
            {
                carString = routeRequest.CarType + routeRequest.CarColor;
            }
            else
            {
                carString = string.Format(getResource.getMessage("CarInfoStr"), routeRequest.CarType,
                    routeRequest.CarColor,
                    routeRequest.CarPlateNo);
            }
            return carString;
        }

        private GDirectionResponse GetGoogleRoute(SrcDstModel model, List<Point> wayPoints, bool alternatives = true)
        {
            var gDirectionRequest = new GDirectionRequest();
            var reqwaypoints = new List<CoreExternalService.Models.Point>();
            gDirectionRequest.Src.Lat = model.SrcLat;
            gDirectionRequest.Src.Lng = model.SrcLng;
            gDirectionRequest.Dst.Lat = model.DstLat;
            gDirectionRequest.Dst.Lng = model.DstLng;
            if (wayPoints != null)
            {
                var counter = wayPoints.Count < 5 ? 1 : wayPoints.Count/5;
                int i = 0;
                foreach (var wayPoint in wayPoints)
                {
                    if (i%counter == 0)
                    {
                        var thisPoint = new CoreExternalService.Models.Point();
                        thisPoint.Lat = wayPoint.Lat;
                        thisPoint.Lng = wayPoint.Lng;
                        reqwaypoints.Add(thisPoint);
                    }
                }
                gDirectionRequest.WayPoints = reqwaypoints;
            }
            var gResult = _gService.GetGRoute(gDirectionRequest, alternatives);
            return gResult;
        }

        #endregion
    }
}