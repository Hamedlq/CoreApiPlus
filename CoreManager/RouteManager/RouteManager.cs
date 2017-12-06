using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
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
                    //response = GetSuggestRoutesCount(response, group.Key);
                    responseList.Add(response);
                }
            }
            return responseList;
        }

        /*private RouteResponseModel GetSuggestRoutesCount(RouteResponseModel response, Guid? routeRequestUId)
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
        }*/

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
                res.SharedServicePrice = RemoveDecimalToman(5566 + extraPrice).ToString();
            }
            else if (distance <= 7000)
            {
                res.SharedServicePrice = RemoveDecimalToman((((distance - 500)/100)*253) + 5566 + extraPrice).ToString();
            }
            else if (distance <= 20000)
            {
                var first7000 = (65 * 253) + 5566;
                res.SharedServicePrice =
                    RemoveDecimalToman((((distance - 7000) / 100) * 202) + first7000 + extraPrice).ToString();
            }
            else if (distance > 20000)
            {
                var first20000 = (125 * 202) + (65 * 253) + 5566;
                res.SharedServicePrice  =
                    RemoveDecimalToman((((distance - 20000) / 100) * 150) + first20000 + extraPrice).ToString();
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
                res.PrivateServicePrice = RemoveDecimalToman(40986 + extraPrice).ToString();
            }
            else if (distance <= 2000)
            {
                res.PrivateServicePrice = RemoveDecimalToman((((distance/100)*3059) + extraPrice)).ToString();
            }
            else if (distance > 2000)
            {
                var first2000 = (20*3059);
                res.PrivateServicePrice =
                    RemoveDecimalToman((((distance - 2000)/100)*1017) + first2000 + extraPrice).ToString();
            }
            /*if (distance > 50000)
            {
                var first2000 = (20 * 2781);
                res.PrivateServicePrice = RemoveDecimalToman(((((distance - 2000) / 100) * 925) + first2000 + extraPrice) * 1.3).ToString();
            }*/
            using (var dataModel = new MibarimEntities())
            {
                var tm=new TmLocation();
                tm.CreateTime=DateTime.Now;
                tm.TmSrcLat = decimal.Parse(model.SrcLat);
                tm.TmSrcLng = decimal.Parse(model.SrcLng);
                tm.TmDstLat = decimal.Parse(model.DstLat);
                tm.TmDstLng = decimal.Parse(model.DstLng);
                dataModel.TmLocations.Add(tm);
                dataModel.SaveChanges();
            }
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
                List<vwDriverTrip> dataList;
                if (model == null || model.FilteringId == 0)
                {
                    dataList= dataModel.vwDriverTrips.Where(
                            x =>
                                x.TStartTime > triptime &&
                                (x.TState == (int)TripState.Scheduled || x.TState == (int)TripState.InTripTime ||
                                 x.TState == (int)TripState.InPreTripTime || x.TState == (int)TripState.InRiding))
                        .OrderBy(x => x.TStartTime).ToList();
                }
                else
                {
                    var filter = dataModel.Filters.FirstOrDefault(x=>x.FilterId==model.FilteringId);
                    dataList = dataModel.vwDriverTrips.Where(
                            x =>
                                x.TStartTime > triptime &&
                                (x.TState == (int)TripState.Scheduled || x.TState == (int)TripState.InTripTime ||
                                 x.TState == (int)TripState.InPreTripTime || x.TState == (int)TripState.InRiding)
                                 && x.SrcMStationId== filter.SrcMStationId
                                 && x.DstMStationId==filter.DstMStationId)
                        .OrderBy(x => x.TStartTime).ToList();
                }
                
                var tripIds = dataList.Select(y => y.TripId);
                var bookedTrip =
                    dataModel.BookRequests.Where(x => tripIds.Contains(x.TripId) && x.UserId == userId);
                var tripUserlists = dataModel.BookRequests.Where(x => tripIds.Contains(x.TripId));
                var bookeds = bookedTrip.Where(x => x.IsBooked == true).Select(y => y.TripId);

                var dataList2 = dataList.Where(x => bookeds.Contains(x.TripId)).ToList();
                dataList2.AddRange(dataList.Where(x => !bookeds.Contains(x.TripId)).ToList());
                foreach (var trip in dataList2)
                {
                    var filledSeats = 0;
                    var passRouteModel = new PassRouteModel();
                    /*var isbooked =
                        dataModel.vwBookPays.FirstOrDefault(
                            x => x.TripId == trip.TripId && x.PayReqRefID != null && x.PayReqUserId == userId);*/
                    var isbooked = bookedTrip.FirstOrDefault(
                        x => x.TripId == trip.TripId && x.UserId == userId && (bool) x.IsBooked);
                    passRouteModel.IsBooked = false;
                    if (isbooked != null)
                    {
                        passRouteModel.IsBooked = (bool) isbooked.IsBooked;
                        if ((bool) isbooked.IsBooked)
                        {
                            passRouteModel.MobileNo = trip.UserName;
                            passRouteModel.CarPlate = trip.CarPlateNo;
                        }
                    }
                    passRouteModel.TripId = trip.TripId;
                    passRouteModel.TripState = trip.TState;
                    passRouteModel.Name = trip.Name;
                    passRouteModel.SrcLatitude = trip.SrcStLat.ToString();
                    passRouteModel.SrcLongitude = trip.SrcStlng.ToString();
                    passRouteModel.DstLatitude = trip.DstMainStLat.ToString();
                    passRouteModel.DstLongitude = trip.DstMainStLng.ToString();
                    passRouteModel.Family = trip.Family;
                    passRouteModel.TimingString = trip.TStartTime.ToString("HH:mm");
                    passRouteModel.Price = (long) trip.PassPrice;
                    passRouteModel.PricingString =
                        RouteMapper.PersianNumber(passRouteModel.Price.ToString("N0", new NumberFormatInfo()
                        {
                            NumberGroupSizes = new[] {3},
                            NumberGroupSeparator = ","
                        }));
                    passRouteModel.SrcAddress = trip.SrcMainStName + "، " + trip.SrcStAdd;
                    passRouteModel.SrcMainAddress = trip.SrcMainStName;
                    passRouteModel.SrcLink = "https://www.google.com/maps/place/" + trip.SrcStLat + "," +
                                             trip.SrcStlng;
                    passRouteModel.DstAddress = trip.DstMainStName;
                    passRouteModel.DstLink = "https://www.google.com/maps/place/" + trip.DstMainStLat + "," +
                                             trip.DstMainStLng;
                    passRouteModel.UserImageId = trip.UserImageId;
                    passRouteModel.IsVerified = trip.VerifiedLevel != null &&
                                                trip.VerifiedLevel == (int) VerifiedLevel.Verified;
                    passRouteModel.CarSeats = trip.TEmptySeat;
                    var tripUsers = tripUserlists.Where(x => x.TripId == trip.TripId);
                    foreach (var tripUser in tripUsers)
                    {
                        if ((bool) tripUser.IsBooked)
                        {
                            filledSeats++;
                        }
                        else if (tripUser.BrCreateTime.AddMinutes(10) > DateTime.Now)
                        {
                            filledSeats++;
                        }
                    }
                    passRouteModel.EmptySeats = trip.TEmptySeat - filledSeats;
                    passRouteModel.EmptySeats = passRouteModel.EmptySeats >= 0 ? trip.TEmptySeat - filledSeats : 0;
                    passRouteModel.CarString = trip.CarType + " " + trip.CarColor;
                    res.Add(passRouteModel);
                }
            }
            return res;
        }

        public PassRouteModel GetPassengerTrip(int userId, long filterId)
        {
            var passRouteModel = new PassRouteModel();
            
            using (var dataModel = new MibarimEntities())
            {
                //DateTime triptime = DateTime.Now.AddMinutes(-15);
                
                var filter = dataModel.vwFilterPlus.FirstOrDefault(x => x.FilterId == filterId);
                if (filter ==null)
                {
                    //error
                    return passRouteModel;
                }
                /*var time = ((DateTime)filter.LastTimeSet);
                var matchTrips = dataModel.vwDriverTrips.Where(
                            x =>
                               DbFunctions.TruncateTime(x.TStartTime) == time.Date &&
                                x.TStartTime.Hour == time.Hour &&
                                x.TStartTime.Minute == time.Minute 
                                 && x.StationRouteId == filter.StationRouteId).ToList();*/
                var price = GetTimePrice((DateTime)filter.LastTimeSet, filter);
                if (filter.TripId!=null && filter.TripId > 0)
                {
                    var trips = dataModel.Filters.Where(x=>x.TripId==filter.TripId).ToList();
                    var theTrip = dataModel.vwDriverTrips.FirstOrDefault(x=>x.TripId==filter.TripId);
                    passRouteModel.IsBooked = true;
                    passRouteModel.EmptySeats = 0;
                    passRouteModel.MobileNo = theTrip.UserName;
                    passRouteModel.CarPlate = theTrip.CarPlateNo;
                    passRouteModel.TripId = theTrip.TripId;
                    passRouteModel.TripState = theTrip.TState;
                    passRouteModel.Name = theTrip.Name;
                    passRouteModel.SrcLatitude = theTrip.SrcStLat.ToString();
                    passRouteModel.SrcLongitude = theTrip.SrcStlng.ToString();
                    passRouteModel.DstLatitude = theTrip.DstMainStLat.ToString();
                    passRouteModel.DstLongitude = theTrip.DstMainStLng.ToString();
                    passRouteModel.Family = theTrip.Family;
                    passRouteModel.TimingString = theTrip.TStartTime.ToString("HH:mm");
                    passRouteModel.Price = RemoveCeilingDecimal(price / (trips.Count)); 
                    passRouteModel.PricingString =
                        RouteMapper.PersianNumber(RemoveCeilingDecimal(price / (trips.Count)).ToString("N0", new NumberFormatInfo()
                        {
                            NumberGroupSizes = new[] { 3 },
                            NumberGroupSeparator = ","
                        }));
                    passRouteModel.SrcAddress = theTrip.SrcMainStName + "، " + theTrip.SrcStAdd;
                    passRouteModel.SrcMainAddress = theTrip.SrcMainStName;
                    passRouteModel.SrcLink = "https://www.google.com/maps/place/" + theTrip.SrcStLat + "," +
                                             theTrip.SrcStlng;
                    passRouteModel.DstAddress = theTrip.DstMainStName;
                    passRouteModel.DstLink = "https://www.google.com/maps/place/" + theTrip.DstMainStLat + "," +
                                             theTrip.DstMainStLng;
                    passRouteModel.UserImageId = theTrip.UserImageId;
                    passRouteModel.IsVerified = theTrip.VerifiedLevel != null &&
                                                theTrip.VerifiedLevel == (int)VerifiedLevel.Verified;
                    passRouteModel.CarSeats = trips.Count;
                    passRouteModel.CarString = theTrip.CarType + " " + theTrip.CarColor;
                }
                else
                {
                    passRouteModel.IsBooked = false;
                    passRouteModel.SrcLatitude = filter.SrcStLat.ToString();
                    passRouteModel.SrcLongitude = filter.SrcStLng.ToString();
                    passRouteModel.DstLatitude = filter.DstStLat.ToString();
                    passRouteModel.DstLongitude = filter.DstStLng.ToString();
                    passRouteModel.TimingString = ((DateTime)filter.LastTimeSet).ToString("HH:mm");
                    passRouteModel.Price = RemoveCeilingDecimal(price);
                    passRouteModel.PricingString =
                        RouteMapper.PersianNumber(RemoveCeilingDecimal(price).ToString("N0", new NumberFormatInfo()
                        {
                            NumberGroupSizes = new[] { 3 },
                            NumberGroupSeparator = ","
                        }));
                    passRouteModel.SrcAddress = filter.SrcStName;
                    passRouteModel.SrcLink = "https://www.google.com/maps/place/" + filter.SrcStLat + "," +
                                             filter.SrcStLng;
                    passRouteModel.DstAddress = filter.DstStName;
                    passRouteModel.DstLink = "https://www.google.com/maps/place/" + filter.DstStLat + "," +
                                             filter.DstStLng;

                }
            }
            return passRouteModel;
        }

        public PaymentDetailModel RequestBooking(int userId, long modelTripId, long modelChargeAmount)
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
                        payreq = _paymentManager.ChargeAccount(userId, (int) modelChargeAmount,
                            user.Name + " " + user.Family);
                        var bookreq = new BookRequest();
                        bookreq.TripId = modelTripId;
                        bookreq.UserId = userId;
                        bookreq.BookingType = (int) BookingTypes.ByZarinPal;
                        bookreq.IsBooked = false;
                        bookreq.BrCreateTime = DateTime.Now;
                        bookreq.PayReqId = payreq.ReqId;
                        dataModel.BookRequests.Add(bookreq);
                        dataModel.SaveChanges();
                    }
                }
            }
            return payreq;
        }

        public PaymentDetailModel RequestPayBooking(int userId, long modelTripId, long modelChargeAmount)
        {
            var payreq = new PaymentDetailModel();
            using (var dataModel = new MibarimEntities())
            {
                var trip = dataModel.vwDriverTrips.FirstOrDefault(x => x.TripId == modelTripId);
                if (trip != null)
                {
                    var pr = new PayReq();
                    pr.PayReqCreateTime = DateTime.Now;
                    pr.PayReqUserId = userId;
                    pr.PayReqValue = modelChargeAmount;
                    dataModel.PayReqs.Add(pr);
                    dataModel.SaveChanges();
                    var bookreq = new BookRequest();
                    bookreq.TripId = modelTripId;
                    bookreq.BrCreateTime = DateTime.Now;
                    bookreq.BookingType = (int) BookingTypes.ByPasargad;
                    bookreq.UserId = userId;
                    bookreq.IsBooked = false;
                    bookreq.PayReqId = pr.PayReqId;
                    dataModel.BookRequests.Add(bookreq);
                    dataModel.SaveChanges();
                    payreq.BankLink = "http://mibarimapp.com/coreapi/PasargadPay?reqid=" + pr.PayReqId;
                    payreq.State = 100;
                }
            }
            return payreq;
        }

        public List<StationRouteModel> GetStationRoutes()
        {
            var res = new List<StationRouteModel>();
            using (var dataModel = new MibarimEntities())
            {
                var dataList = dataModel.vwStationRoutes.OrderByDescending(x => x.StationRouteId).ToList();
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

        public long SetUserRoute(int userId, long stRouteId, long stationId)
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
                    return 0;
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
                return driverRoute.DriverRouteId;
            }
        }

        public long SetRoute(int userId, long srcSubStId, long dstStId)
        {
            using (var dataModel = new MibarimEntities())
            {
                var srcStation = dataModel.Stations.FirstOrDefault(x => x.StationId == srcSubStId);
                var route =
                    dataModel.StationRoutes.FirstOrDefault(
                        x => x.SrcMStationId == srcStation.MainStationId && x.DstMStationId == dstStId);
                if (route != null)
                {
                    /*if (
                        dataModel.DriverRoutes.Any(
                            x => x.UserId == userId && x.StationRouteId == route.StationRouteId && !x.DrIsDeleted))
                    {
                        _responseProvider.SetBusinessMessage(new MessageResponse()
                        {
                            Type = ResponseTypes.Error,
                            Message = getResource.getMessage("RouteExist")
                        });
                        return 0;
                    }*/
                    var driverRoute = new DriverRoute();
                    driverRoute.UserId = userId;
                    driverRoute.DrIsDeleted = false;
                    driverRoute.DrCreateTime = DateTime.Now;
                    var car = dataModel.vwCarInfoes.FirstOrDefault(x => x.UserId == userId);
                    if (car != null)
                    {
                        driverRoute.CarinfoId = car.CarInfoId;
                    }
                    driverRoute.DrSrcStationId = srcSubStId; //subStation.StationId;
                    driverRoute.StationRouteId = route.StationRouteId;
                    dataModel.DriverRoutes.Add(driverRoute);
                    dataModel.SaveChanges();
                    return driverRoute.DriverRouteId;
                }
                else
                {
                    //TODO:GetFromgoogle;
                    return 0;
                }
            }
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
                        var tripUsers = dataModel.BookRequests.Where(x => x.TripId == lastTrip.TripId);
                        foreach (var tripUser in tripUsers)
                        {
                            if ((bool) tripUser.IsBooked)
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
                    driverRouteModel.SrcMainAddress = dr.SrcMainStName;
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
            return res.OrderByDescending(x => x.TripId).ThenByDescending(y => y.DriverRouteId).ToList();
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
                    var usr = dataModel.vwUserInfoes.FirstOrDefault(x => x.UserId == userId);
                    if (usr.VerifiedLevel != null && usr.VerifiedLevel >= (int) VerifiedLevel.Blocked)
                    {
                        _responseProvider.SetBusinessMessage(new MessageResponse()
                        {
                            Type = ResponseTypes.Error,
                            Message = getResource.getMessage("BlockedUser")
                        });
                        res.IsSubmited = false;
                        return res;
                    }
                    var trip = new Trip();
                    trip.TStartTime = GetNextDateTime(model.TimingHour, model.TimingMin);
                    res.RemainHour = (int) (trip.TStartTime - DateTime.Now).TotalHours;
                    var remainMin = (int) (trip.TStartTime - DateTime.Now).TotalMinutes;
                    res.RemainMin = (remainMin%60);
                    trip.DriverRouteId = model.DriverRouteId;
                    trip.TCreateTime = DateTime.Now;
                    trip.TEmptySeat = model.CarSeats;
                    trip.TState = (int) TripState.Scheduled;
                    dataModel.Trips.Add(trip);
                    dataModel.SaveChanges();
                    var stId = dataModel.DriverRoutes.FirstOrDefault(x => x.DriverRouteId == model.DriverRouteId);
                    FindMatchFilters(trip.TStartTime, stId.StationRouteId, trip.TripId);
                    /*var time = trip.TStartTime;
                    var matchTrips = dataModel.vwDriverTrips.Where(
                                x =>
                                   DbFunctions.TruncateTime(x.TStartTime) == time.Date &&
                                    x.TStartTime.Hour == time.Hour &&
                                    x.TStartTime.Minute == time.Minute
                                     && x.StationRouteId == stId.StationRouteId).ToList();
                    if (matchTrips.Count > 0)
                    {
                        foreach (var matchTrip in matchTrips)
                        {
                            matchTrip.TripId = trip.TripId;
                        }
                        dataModel.SaveChanges();
                    }*/
                }
            }
            res.IsSubmited = true;
            return res;
        }

        public string InvokeTrips()
        {
            bool sched = false;
            bool pretrip = false;
            bool Intrip = false;
            bool posttrip = false;

            using (var dataModel = new MibarimEntities())
            {
                var activeTrips =
                    dataModel.Trips.Where(
                        x =>
                            x.TState == (int) TripState.Scheduled || x.TState == (int) TripState.InTripTime ||
                            x.TState == (int) TripState.InRiding || x.TState == (int) TripState.InDriving ||
                            x.TState == (int) TripState.DriverRiding ||
                            x.TState == (int) TripState.InPreTripTime).OrderByDescending(x => x.TStartTime);
/*                foreach (var preTrip in activeTrips.Where(x => x.TState == (int) TripState.Scheduled))
                                                    {
                                                        if (preTrip.TStartTime.AddMinutes(-30) == DateTime.Now)
                                                        {
                                                            var driveRoute =
                                                                dataModel.DriverRoutes.FirstOrDefault(x => x.DriverRouteId == preTrip.DriverRouteId);
                                                            var route =
                                                                dataModel.vwStationRoutes.FirstOrDefault(x => x.StationRouteId == driveRoute.StationRouteId);
                                                                                                                                                                                                                                                                                                                                                
                                                            var user = dataModel.vwUserInfoes.FirstOrDefault(x => x.UserId == driveRoute.UserId);
                                                            var mobileBrief = user.UserName.Substring(1);
                                                            NotifModel notifModel = new NotifModel();
                                                            notifModel.Title = getResource.getMessage("SeatReserved");
                                                            notifModel.Body = string.Format(getResource.getMessage("SeatReservedFor"),
                                                                route.SrcMainStName,
                                                                route.DstMainStName, preTrip.TStartTime.ToString("HH:mm"));
                                                            notifModel.RequestCode = (int)preTrip.TripId;
                                                            notifModel.NotificationId = (int)preTrip.TripId;
                                                            //send driver notif
                                                            _notifManager.SendNotifToUser(notifModel, (int)driveRoute.UserId);
                                                            _notifManager.SendNotifToAdmins(notifModel);
                                                            //send driver sms
                                                            string smsBody = string.Format(getResource.getMessage("SeatReservedFor"), route.SrcMainStName,
                                                                route.DstMainStName, preTrip.TStartTime.ToString("HH:mm"));
                                                            var smsService = new SmsService();
                                                            smsService.SendSmsMessages(mobileBrief, smsBody);
                                                        }
                                                    }*/
                foreach (var preTrip in activeTrips.Where(x => x.TState == (int) TripState.Scheduled))
                {
                    if (preTrip.TStartTime.AddMinutes(-30) < DateTime.Now)
                    {
                        sched = true;
                        preTrip.TState = (int) TripState.InPreTripTime;
                        SendBookedMessages(preTrip);
                    }
                }
                if (sched)
                {
                    dataModel.SaveChanges();
                }
                foreach (var activeTrip in activeTrips.Where(x => x.TState == (int) TripState.InPreTripTime))
                {
                    if (activeTrip.TStartTime.AddMinutes(-1) < DateTime.Now)
                    {
                        pretrip = true;
                        activeTrip.TState = (int) TripState.InTripTime;
                        /*var driveRoute =
                            dataModel.DriverRoutes.FirstOrDefault(x => x.DriverRouteId == activeTrip.DriverRouteId);
                        var todate = DateTime.Today;
                        var drivertrips =
                            dataModel.DriverRoutes.Where(x => x.UserId == driveRoute.UserId)
                                .Select(x => x.DriverRouteId);
                        var todaytrips =
                            dataModel.vwDriverTrips.Count(
                                x =>
                                    drivertrips.Contains(x.DriverRouteId) &&
                                    (x.TState == (int) TripState.DriverNotCome ||
                                     x.TState == (int) TripState.FinishedByTime) && x.TStartTime >= todate);
                        if (todaytrips < 1)
                        {
                            var usr = dataModel.vwUserInfoes.FirstOrDefault(x => x.UserId == driveRoute.UserId);
                            var msg = string.Format(getResource.getMessage("PayForSetTrip"), 3000,
                                RouteMapper.GetUserNameFamilyString(usr));
                            _transactionManager.ChargeAccount((int) driveRoute.UserId, 3000, msg,
                                TransactionType.CreditChargeAccount);
                        }*/
                    }
                }
                if (pretrip)
                {
                    dataModel.SaveChanges();
                }
                //var doingTrips = dataModel.Trips.Where(x => x.TState == (int)TripState.InTripTime);
                foreach (var doingTrip in activeTrips.Where(x => x.TState == (int) TripState.InTripTime))
                {
                    if (doingTrip.TStartTime.AddMinutes(15) < DateTime.Now)
                    {
                        Intrip = true;
                        doingTrip.TState = (int) TripState.DriverNotCome;
                        var tripfilters = dataModel.Filters.Where(x => x.TripId == doingTrip.TripId);
                        foreach (var tripfilter in tripfilters)
                        {
                            tripfilter.IsActive = false;
                        }
                    }
                }
                if (Intrip)
                {
                    dataModel.SaveChanges();
                }
                foreach (
                    var doingTrip in
                    activeTrips.Where(x => x.TState == (int) TripState.InRiding || x.TState == (int) TripState.InDriving
                                           || x.TState == (int) TripState.DriverRiding)
                )
                {
                    if (doingTrip.TStartTime.AddMinutes(30) < DateTime.Now)
                    {
                        posttrip = true;
                        doingTrip.TState = (int) TripState.FinishedByTime;
                        TransferMoney(doingTrip);
                        MakeRating(doingTrip);
                    }
                }
                if (posttrip)
                {
                    dataModel.SaveChanges();
                }

                /*var kavehSmsService = new KavenegarService();
                kavehSmsService.GetLastMessage();
                var smsService = new SmsService();
                smsService.GetReceivedSmsMessages();*/
                //yesterday = DateTime.Now.AddDays(-1);

                //var googleToken =
                //        dataModel.vwDriverGtokens.Where(
                //            x =>
                //                x.TState == (int)TripState.Scheduled || x.TState == (int)TripState.InTripTime ||
                //                x.TState == (int)TripState.InRiding || x.TState == (int)TripState.InDriving ||
                //                x.TState == (int)TripState.DriverRiding ||
                //                x.TState == (int)TripState.InPreTripTime).OrderByDescending(x => x.TStartTime);
            }

            return "Done";
        }

        private void MakeRating(Trip preTrip)
        {
            using (var dataModel = new MibarimEntities())
            {
                var tripDrive = dataModel.vwDriverTrips.FirstOrDefault(x => x.TripId == preTrip.TripId);
                var booked = dataModel.BookRequests.Where(x => x.TripId == preTrip.TripId && (bool) x.IsBooked);
                var userIds = new List<long>();
                userIds.AddRange(booked.Select(x => (long) x.UserId));
                userIds.Add(tripDrive.UserId);
                var cou = userIds.Count;
                for (int i = 1; i <= cou; i++)
                {
                    for (int j = 0; j <= i - 1; j++)
                    {
                        if (i-1 != j) { 
                        var rating = new Rating();
                        rating.TripId = preTrip.TripId;
                        rating.RateCreateTime = DateTime.Now;
                        rating.RaterUserId = userIds[i-1];
                        rating.FellowUserId = userIds[j];
                        dataModel.Ratings.Add(rating);
                        var backrating = new Rating();
                        backrating.TripId = preTrip.TripId;
                        backrating.RateCreateTime = DateTime.Now;
                        backrating.RaterUserId = userIds[j];
                        backrating.FellowUserId = userIds[i-1];
                        dataModel.Ratings.Add(backrating);
                        }
                    }
                }
                dataModel.SaveChanges();
            }
        }

        /* private async Task DoTransferMoney(Trip preTrip)
         {
             await Task.Run(() => { TransferMoney(preTrip); });
         }*/

        private void TransferMoney(Trip preTrip)
        {
            using (var dataModel = new MibarimEntities())
            {
                var tripDrive = dataModel.vwDriverTrips.FirstOrDefault(x => x.TripId == preTrip.TripId);
                var booked = dataModel.BookRequests.Where(x => x.TripId == preTrip.TripId && (bool) x.IsBooked);
                foreach (var bookRequest in booked)
                {
                    try
                    {
                        var remain = _transactionManager.GetRemain((int) bookRequest.UserId);
                        var discount =
                            dataModel.vwDiscountUsers.FirstOrDefault(
                                x =>
                                    x.UserId == (int) bookRequest.UserId && x.DiscountEndTime > DateTime.Now &&
                                    (x.DuEndTime > DateTime.Now || x.DuEndTime == null) &&
                                    x.DuState == (int) DiscountStates.Submitted);
                        if (discount != null)
                        {
                            var du =
                                dataModel.DiscountUsers.FirstOrDefault(
                                    x => x.DuId == discount.DuId && x.UserId == (int) bookRequest.UserId);

                            switch (discount.DiscountType)
                            {
                                case (int) DiscountTypes.EndlessFirstFreeTrip:
                                case (int) DiscountTypes.EndlessFreeSeat:
                                case (int) DiscountTypes.FreeSeat:
                                case (int) DiscountTypes.AlwaysFreeSeat:
                                    du.DuState = (int) DiscountStates.Used;
                                    dataModel.SaveChanges();
                                    _transactionManager.GiftChargeAccount((int) bookRequest.UserId,
                                        (int) tripDrive.DriverPrice);
                                    _transactionManager.PayMoney((int) bookRequest.UserId, (int) tripDrive.UserId,
                                        (int) tripDrive.DriverPrice);
                                    break;
                                case (int) DiscountTypes.FirstFreeTrip:
                                    du.DuState = (int) DiscountStates.Used;
                                    dataModel.SaveChanges();
                                    _transactionManager.GiftChargeAccount((int) bookRequest.UserId,
                                        (int) tripDrive.DriverPrice);
                                    _transactionManager.PayMoney((int) bookRequest.UserId, (int) tripDrive.UserId,
                                        (int) tripDrive.DriverPrice);
                                    var isInvite =
                                        dataModel.Invites.Where(
                                            x =>
                                                x.InviterUserId == bookRequest.UserId &&
                                                x.CreateTime > DateTime.Now.AddMonths(-1));
                                    if (isInvite != null)
                                    {
                                        //DoReferal(isInvite);
                                    }
                                    break;
                                case (int) DiscountTypes.PercentDiscount:
                                    du.DuState = (int) DiscountStates.Used;
                                    dataModel.SaveChanges();
                                    var tripprice =
                                        Convert.ToInt32(tripDrive.DriverPrice*(discount.DiscountPercent*0.01));
                                    _transactionManager.GiftChargeAccount((int) bookRequest.UserId,
                                        tripprice);
                                    _transactionManager.PayMoney((int) bookRequest.UserId, (int) tripDrive.UserId,
                                        (int) tripDrive.DriverPrice);

                                    break;
                            }
                        }
                        else
                        {
                            if (tripDrive.DriverPrice > tripDrive.PassPrice)
                            {
                                _transactionManager.GiftChargeAccount(bookRequest.UserId.Value,
                                    (int) (tripDrive.DriverPrice - tripDrive.PassPrice));
                            }
                            _transactionManager.PayMoney(bookRequest.UserId.Value, (int) tripDrive.UserId,
                                (int) tripDrive.DriverPrice);
                        }
                    }
                    catch (Exception e)
                    {
                        _logmanager.Log(Tag, "TransferMoney", e.Message + " user: " + bookRequest.UserId);
                    }
                }
            }
        }

        private void SendBookedMessages(Trip preTrip)
        {
            using (var dataModel = new MibarimEntities())
            {
                try
                {
                    var booked = dataModel.BookRequests.Where(x => x.TripId == preTrip.TripId && (bool) x.IsBooked);
                    foreach (var bookRequest in booked)
                    {
                        var tripDrive = dataModel.vwDriverTrips.FirstOrDefault(x => x.TripId == preTrip.TripId);
                        NotifModel notifModel = new NotifModel();
                        notifModel.Title = getResource.getMessage("SeatReserved");
                        notifModel.Body = string.Format(getResource.getMessage("SeatReservedFor"),
                            tripDrive.SrcMainStName,
                            tripDrive.DstMainStName, tripDrive.TStartTime.ToString("HH:mm"));
                        notifModel.RequestCode = (int) tripDrive.TripId;
                        notifModel.NotificationId = (int) tripDrive.TripId;
                        //send passenger notif
                        _notifManager.SendNotifToUser(notifModel, (int) bookRequest.UserId);
                        //send driver notif
                        _notifManager.SendNotifToUser(notifModel, (int) tripDrive.UserId);
                        _notifManager.SendNotifToAdmins(notifModel);
                        //send passenger sms
                        var user = dataModel.vwUserInfoes.FirstOrDefault(x => x.UserId == (int) bookRequest.UserId);
                        var mobileBrief = user.UserName.Substring(1);
                        string smsBody = string.Format(getResource.getMessage("SeatReservedFor"),
                            tripDrive.SrcMainStName,
                            tripDrive.DstMainStName, tripDrive.TStartTime.ToString("HH:mm"));
                        var smsService = new SmsService();
                        smsService.SendSmsMessages(mobileBrief, smsBody);
                        //send driver sms
                        var driver = dataModel.vwUserInfoes.FirstOrDefault(x => x.UserId == tripDrive.UserId);
                        var drivermobileBrief = driver.UserName.Substring(1);
                        string smsBodydriver = string.Format(getResource.getMessage("SeatReservedFor"),
                            tripDrive.SrcMainStName, tripDrive.DstMainStName, tripDrive.TStartTime.ToString("HH:mm"));
                        smsService.SendSmsMessages(drivermobileBrief, smsBodydriver);
                        smsService.SendSmsMessages("9358695785", smsBody);
                        smsService.SendSmsMessages("9354205407", smsBody);
                    }
                }
                catch (Exception e)
                {
                    _logmanager.Log(Tag, "sendBookedMessages", e.Message);
                }
            }
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
                        x => (x.TState == (int) TripState.Scheduled ||
                              x.TState == (int) TripState.InTripTime ||
                              x.TState == (int) TripState.InPreTripTime ||
                              x.TState == (int) TripState.InRiding)
                             && x.DriverRouteId == modelDriverRouteId);
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
                    dataModel.vwDriverTrips.Where(
                            x => x.UserId == userId && x.DriverRouteId == model.DriverRouteId)
                        .OrderByDescending(x => x.TCreateTime)
                        .FirstOrDefault();
                if (driveTrip != null)
                {
                    if (driveTrip.TState == (int) TripState.Scheduled)
                    {
                        var tripUsers = dataModel.BookRequests.Where(x => x.TripId == driveTrip.TripId);
                        foreach (var tripUser in tripUsers)
                        {
                            if ((bool) tripUser.IsBooked)
                            {
                                _responseProvider.SetBusinessMessage(new MessageResponse()
                                {
                                    Type = ResponseTypes.Error,
                                    Message = getResource.getMessage("TripAlreadyReserved")
                                });
                                return false;
                            }
                            else if (tripUser.BrCreateTime.AddMinutes(10) > DateTime.Now)
                            {
                                _responseProvider.SetBusinessMessage(new MessageResponse()
                                {
                                    Type = ResponseTypes.Error,
                                    Message = getResource.getMessage("TripAlreadyReserved")
                                });
                                return false;
                            }
                        }
                        var trip = dataModel.Trips.FirstOrDefault(x => x.TripId == driveTrip.TripId);
                        trip.TState = (int) TripState.CanceledByUser;
                        dataModel.SaveChanges();
                        //cancel all filterTrips
                        var filters=dataModel.Filters.Where(x => x.TripId == trip.TripId).ToList();
                        if (filters.Count > 0)
                        {
                            if (trip.TStartTime < DateTime.Now.AddMinutes(-30))
                            {
                                foreach (var filter in filters)
                                {
                                    filter.TripId = null;
                                }
                                dataModel.SaveChanges();
                            }
                            else
                            {
                                _responseProvider.SetBusinessMessage(new MessageResponse()
                                {
                                    Type = ResponseTypes.Error,
                                    Message = getResource.getMessage("TripAlreadyReserved")
                                });
                                return false;
                            }
                        }
                    }
                    else
                    {
                        _responseProvider.SetBusinessMessage(new MessageResponse()
                        {
                            Type = ResponseTypes.Error,
                            Message = getResource.getMessage("TripAlreadySet")
                        });
                        return false;
                    }
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
                    var tripUsers = dataModel.BookRequests.Where(x => x.TripId == trip.TripId);
                    foreach (var tripUser in tripUsers)
                    {
                        if ((bool) tripUser.IsBooked)
                        {
                            res.FilledSeats++;
                        }
                        /*else if (tripUser.BrCreateTime.AddMinutes(10) > DateTime.Now)
                        {
                            res.FilledSeats++;
                        }*/
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

        public bool IsPayValid(int userId, PayModel model)
        {
            using (var dataModel = new MibarimEntities())
            {
                bool isPriceTrue = false;
                bool isCreditTrue = false;
                var trip = dataModel.vwDriverTrips.FirstOrDefault(x => x.TripId == model.TripId);
                var remain = _transactionManager.GetRemain(userId);
                var discount =
                    dataModel.vwDiscountUsers.FirstOrDefault(
                        x =>
                            x.UserId == userId && x.DiscountEndTime > DateTime.Now &&
                            (x.DuEndTime > DateTime.Now || x.DuEndTime == null) &&
                            x.DuState == (int) DiscountStates.Submitted);
                if (discount != null)
                {
                    /*//elecomp 50 discount
                    var trips = dataModel.BookRequests.Count(x => x.IsBooked.Value && x.UserId == userId);
                    if (trips > 0)
                    {
                        if (trips >= 2)
                        {
                            remain = remain + Convert.ToSingle((model.SeatPrice) * 0.7);
                        }
                        else
                        {
                            remain = remain + Convert.ToSingle((model.SeatPrice) * 0.5);
                        }
                        isPriceTrue = model.ChargeAmount >= 0;
                        isCreditTrue = model.Credit >= (remain + model.SeatPrice);
                    }
                    else
                    {*/
                    switch (discount.DiscountType)
                    {
                        case (int) DiscountTypes.EndlessFirstFreeTrip:
                        case (int) DiscountTypes.FirstFreeTrip:
                        case (int) DiscountTypes.EndlessFreeSeat:
                        case (int) DiscountTypes.FreeSeat:
                        case (int) DiscountTypes.AlwaysFreeSeat:
                            isPriceTrue = model.ChargeAmount >= 0;
                            isCreditTrue = model.Credit >= (remain + model.SeatPrice);
                            break;
                        case (int) DiscountTypes.PercentDiscount:
                            isPriceTrue = model.ChargeAmount + (model.SeatPrice*(discount.DiscountPercent*0.01)) >= 0;
                            isCreditTrue = model.Credit >= (remain + (model.SeatPrice*(discount.DiscountPercent*0.01)));
                            break;
                    }
                    //}
                }
                isPriceTrue = model.ChargeAmount + model.Credit >= model.SeatPrice || isPriceTrue;
                isCreditTrue = model.Credit >= remain || isCreditTrue;
                if (trip.PassPrice == model.SeatPrice && isPriceTrue && isCreditTrue)
                {
                    return true;
                }
            }
            return false;
        }

        public PaymentDetailModel BookSeat(int userId, PayModel model)
        {
            var res = new PaymentDetailModel();
            using (var dataModel = new MibarimEntities())
            {
                var tripDrive = dataModel.vwDriverTrips.FirstOrDefault(x => x.TripId == model.TripId);
                var remain = _transactionManager.GetRemain(userId);
                var discount =
                    dataModel.vwDiscountUsers.FirstOrDefault(
                        x =>
                            x.UserId == userId && x.DiscountEndTime > DateTime.Now &&
                            (x.DuEndTime > DateTime.Now || x.DuEndTime == null) &&
                            x.DuState == (int) DiscountStates.Submitted);
                if (discount != null)
                {
                    switch (discount.DiscountType)
                    {
                        case (int) DiscountTypes.EndlessFirstFreeTrip:
                        case (int) DiscountTypes.FirstFreeTrip:
                        case (int) DiscountTypes.EndlessFreeSeat:
                        case (int) DiscountTypes.FreeSeat:
                        case (int) DiscountTypes.AlwaysFreeSeat:
                            var bookreq = new BookRequest();
                            bookreq.TripId = model.TripId;
                            bookreq.BrCreateTime = DateTime.Now;
                            bookreq.UserId = userId;
                            bookreq.BookingType = (int) BookingTypes.ByDiscount;
                            bookreq.DuId = discount.DuId;
                            bookreq.IsBooked = true;
                            dataModel.BookRequests.Add(bookreq);
                            dataModel.SaveChanges();
                            var du =
                                dataModel.DiscountUsers.FirstOrDefault(
                                    x =>
                                        x.DuId == discount.DuId && x.UserId == userId &&
                                        x.DuState == (int) DiscountStates.Submitted);
                            du.DuState = (int) DiscountStates.Used;
                            dataModel.SaveChanges();
                            /*_transactionManager.GiftChargeAccount(userId, (int) tripDrive.DriverPrice);
                            _transactionManager.PayMoney(userId, (int) tripDrive.UserId, (int) tripDrive.DriverPrice);*/
                            break;
                        case (int) DiscountTypes.PercentDiscount:
                            var dbookreq = new BookRequest();
                            dbookreq.TripId = model.TripId;
                            dbookreq.BrCreateTime = DateTime.Now;
                            dbookreq.UserId = userId;
                            dbookreq.BookingType = (int) BookingTypes.ByDiscountAndCredit;
                            dbookreq.DuId = discount.DuId;
                            dbookreq.IsBooked = true;
                            dataModel.BookRequests.Add(dbookreq);
                            dataModel.SaveChanges();
                            var dud =
                                dataModel.DiscountUsers.FirstOrDefault(
                                    x =>
                                        x.DuId == discount.DuId && x.UserId == userId &&
                                        x.DuState == (int) DiscountStates.Submitted);
                            dud.DuState = (int) DiscountStates.Used;
                            dataModel.SaveChanges();
                            break;
                    }
                }
                else if (remain > model.SeatPrice)
                {
                    var bookreq = new BookRequest();
                    bookreq.TripId = model.TripId;
                    bookreq.BrCreateTime = DateTime.Now;
                    bookreq.UserId = userId;
                    bookreq.BookingType = (int) BookingTypes.ByCredit;
                    bookreq.Credit = remain;
                    bookreq.IsBooked = true;
                    dataModel.BookRequests.Add(bookreq);
                    dataModel.SaveChanges();
                    /*if (tripDrive.DriverPrice > tripDrive.PassPrice)
                    {
                        _transactionManager.GiftChargeAccount(userId,
                            (int) (tripDrive.DriverPrice - tripDrive.PassPrice));
                    }
                    _transactionManager.PayMoney(userId, (int) tripDrive.UserId, (int) tripDrive.DriverPrice);*/
                }
                else
                {
                    throw new Exception("booking has a problem- check it" + tripDrive.TripId + "-" + userId);
                }
                /*NotifModel notifModel = new NotifModel();
                notifModel.Title = getResource.getMessage("SeatReserved");
                notifModel.Body = string.Format(getResource.getMessage("SeatReservedFor"), tripDrive.SrcMainStName,
                    tripDrive.DstMainStName, tripDrive.TStartTime.ToString("HH:mm"));
                notifModel.RequestCode = (int) tripDrive.TripId;
                notifModel.NotificationId = (int) tripDrive.TripId;
                //send passenger notif
                _notifManager.SendNotifToUser(notifModel, userId);
                //send driver notif
                _notifManager.SendNotifToUser(notifModel, (int) tripDrive.UserId);
                _notifManager.SendNotifToAdmins(notifModel);
                //send passenger sms
                var user = dataModel.vwUserInfoes.FirstOrDefault(x => x.UserId == userId);
                var mobileBrief = user.UserName.Substring(1);
                string smsBody = string.Format(getResource.getMessage("SeatReservedFor"), tripDrive.SrcMainStName,
                    tripDrive.DstMainStName, tripDrive.TStartTime.ToString("HH:mm"));
                var smsService = new SmsService();
                smsService.SendSmsMessages(mobileBrief, smsBody);
                //send driver sms
                var driver = dataModel.vwUserInfoes.FirstOrDefault(x => x.UserId == tripDrive.UserId);
                var drivermobileBrief = driver.UserName.Substring(1);
                string smsBodydriver = string.Format(getResource.getMessage("SeatReservedFor"),
                    tripDrive.SrcMainStName, tripDrive.DstMainStName, tripDrive.TStartTime.ToString("HH:mm"));
                smsService.SendSmsMessages(drivermobileBrief, smsBodydriver);
                smsService.SendSmsMessages("9358695785", smsBody);
                smsService.SendSmsMessages("9354205407", smsBody);*/
            }
            res.State = 200;
            return
                res;
        }

        public bool ReserveSeat(long payReqId)
        {
            using (var dataModel = new MibarimEntities())
            {
                var bookreq = dataModel.BookRequests.FirstOrDefault(x => x.PayReqId == payReqId);
                if (bookreq != null)
                {
                    var trip = dataModel.vwDriverTrips.FirstOrDefault(x => x.TripId == bookreq.TripId);
                    if ((bool) bookreq.IsBooked)
                    {
                        return false;
                    }
                    var payreq = dataModel.PayReqs.FirstOrDefault(x => x.PayReqId == payReqId);
                    var payValue = Convert.ToInt32(payreq.PayReqValue);
                    var discount =
                        dataModel.vwDiscountUsers.FirstOrDefault(
                            x =>
                                x.UserId == payreq.PayReqUserId && x.DiscountEndTime > DateTime.Now &&
                                (x.DuEndTime > DateTime.Now || x.DuEndTime == null) &&
                                x.DuState == (int) DiscountStates.Submitted);
                    if (discount != null && discount.DiscountType == (int) DiscountTypes.PercentDiscount)
                    {
                        payValue = Convert.ToInt32(payValue + (trip.PassPrice*(discount.DiscountPercent*0.01)));
                        var du =
                            dataModel.DiscountUsers.FirstOrDefault(
                                x => x.UserId == payreq.PayReqUserId && x.DuState == (int) DiscountStates.Submitted);
                        du.DuState = (int) DiscountStates.Used;
                        bookreq.DuId = discount.DuId;
                        dataModel.SaveChanges();
                    }
                    if (payValue >= trip.PassPrice)
                    {
                        bookreq.IsBooked = true;
                        dataModel.SaveChanges();
                    }
                    _transactionManager.ChargeAccount(payreq.PayReqUserId, (int) payreq.PayReqValue);
                    /*var payRoute =
                        dataModel.vwPayRoutes.FirstOrDefault(
                            x => x.PayReqId == payReqId && x.DrIsDeleted == false);
                    var route =
                        dataModel.vwStationRoutes.FirstOrDefault(x => x.StationRouteId == payRoute.StationRouteId);
                    if (route.DriverPrice > route.PassPrice)
                    {
                        _transactionManager.GiftChargeAccount(payreq.PayReqUserId,
                            (int) (route.DriverPrice - route.PassPrice));
                    }
                    _transactionManager.PayMoney((int) bookreq.UserId, (int) payRoute.UserId, (int) route.DriverPrice);
                    NotifModel notifModel = new NotifModel();
                    notifModel.Title = getResource.getMessage("SeatReserved");
                    notifModel.Body = string.Format(getResource.getMessage("SeatReservedFor"), route.SrcMainStName,
                        route.DstMainStName, payRoute.TStartTime.ToString("HH:mm"));
                    notifModel.RequestCode = (int) payRoute.PayReqId;
                    notifModel.NotificationId = (int) payRoute.PayReqId;
                    //send passenger notif
                    _notifManager.SendNotifToUser(notifModel, payRoute.PayReqUserId);
                    //send driver notif
                    _notifManager.SendNotifToUser(notifModel, (int) payRoute.UserId);
                    _notifManager.SendNotifToAdmins(notifModel);
                    //send passenger sms
                    var user = dataModel.vwUserInfoes.FirstOrDefault(x => x.UserId == payRoute.PayReqUserId);
                    var mobileBrief = user.UserName.Substring(1);
                    string smsBody = string.Format(getResource.getMessage("SeatReservedFor"), route.SrcMainStName,
                        route.DstMainStName, payRoute.TStartTime.ToString("HH:mm"));
                    var smsService = new SmsService();
                    smsService.SendSmsMessages(mobileBrief, smsBody);
                    //send driver sms
                    var driver = dataModel.vwUserInfoes.FirstOrDefault(x => x.UserId == payRoute.UserId);
                    var drivermobileBrief = driver.UserName.Substring(1);
                    string smsBodydriver = string.Format(getResource.getMessage("SeatReservedFor"),
                        route.SrcMainStName, route.DstMainStName, payRoute.TStartTime.ToString("HH:mm"));
                    smsService.SendSmsMessages(drivermobileBrief, smsBodydriver);
                    smsService.SendSmsMessages("9358695785", smsBody);
                    smsService.SendSmsMessages("9354205407", smsBody);
                    dataModel.SaveChanges();*/
                    return true;
                }
                return false;
            }
        }

        public bool HasCapacity(PayModel model)
        {
            using (var dataModel = new MibarimEntities())
            {
                var trip = dataModel.Trips.FirstOrDefault(x => x.TripId == model.TripId);
                var books = dataModel.BookRequests.Count(x => (bool) x.IsBooked && x.TripId == trip.TripId);
                if (books <= trip.TEmptySeat)
                {
                    return true;
                }
            }
            return false;
        }

        public bool HasReserved(PayModel model, int userId)
        {
            using (var dataModel = new MibarimEntities())
            {
                //var trip = dataModel.Trips.FirstOrDefault(x => x.TripId == model.TripId);
                var books =
                    dataModel.BookRequests.Count(
                        x => (bool) x.IsBooked && x.TripId == model.TripId && x.UserId == userId);
                if (books > 0)
                {
                    return true;
                }
            }
            return false;
        }

        public TripTimeModel CancelBooking(int userId, long tripId)
        {
            var res = new TripTimeModel();
            using (var dataModel = new MibarimEntities())
            {
                var booked =
                    dataModel.BookRequests.FirstOrDefault(
                        x => (bool) x.IsBooked && x.TripId == tripId && x.UserId == userId && (bool) x.IsBooked);
                if (booked != null)
                {
                    booked.IsBooked = false;
                    dataModel.SaveChanges();
                    switch (booked.BookingType)
                    {
                        case (int) BookingTypes.ByDiscount:
                        case (int) BookingTypes.ByDiscountAndCredit:
                            var disuser = dataModel.DiscountUsers.FirstOrDefault(x => x.DuId == booked.DuId);
                            disuser.DuState = (short) DiscountStates.Submitted;
                            break;
                        /*case (int)BookingTypes.ByCredit:
                            //var remain = _transactionManager.GetRemain(userId);
                            var tripDrive = dataModel.vwDriverTrips.FirstOrDefault(x => x.TripId == tripId);
                            var tripPrice = 0;
                            if (tripDrive.DriverPrice > tripDrive.PassPrice)
                            {
                                tripPrice=(int)(tripDrive.DriverPrice - tripDrive.PassPrice);
                            }
                            tripPrice += (int) tripDrive.DriverPrice;
                            _transactionManager.PayMoney((int)tripDrive.UserId, userId, tripPrice);
                            break;*/
                        //case (int)BookingTypes.:
                    }
                    dataModel.SaveChanges();
                    res.IsSubmited = true;
                }
                else
                {
                    _responseProvider.SetBusinessMessage(new MessageResponse()
                    {
                        Type = ResponseTypes.Error,
                        Message = getResource.getMessage("TripAlreadyDisabled")
                    });
                    res.IsSubmited = false;
                }
            }
            return res;
        }

        public long SubmitMainStation(int userId, string modelName, string modelStLat, string modelStLng)
        {
            using (var dataModel = new MibarimEntities())
            {
                var ms = dataModel.MainStations.OrderByDescending(x => x.MainStationId).FirstOrDefault();
                var st = new MainStation();
                st.MainStationId = ms.MainStationId + 1;
                st.MainStName = modelName;
                st.MainStLat = decimal.Parse(modelStLat);
                st.MainStLng = decimal.Parse(modelStLng);
                st.MainStGeo = RouteMapper.CreatePoint(modelStLat, modelStLng);
                st.MainStRadius = 100;
                dataModel.MainStations.Add(st);
                dataModel.SaveChanges();
                return st.MainStationId;
            }
        }

        public long SubmitStation(int userId, string modelName, string modelStLat, string modelStLng,
            long modelMainStationId)
        {
            using (var dataModel = new MibarimEntities())
            {
                var ms = dataModel.MainStations.FirstOrDefault(x => x.MainStationId == modelMainStationId);
                var st = new Station();
                st.MainStationId = ms.MainStationId;
                st.StAdd = modelName;
                st.MainStationId = ms.MainStationId;
                st.StLat = decimal.Parse(modelStLat);
                st.StLng = decimal.Parse(modelStLng);
                st.StGeo = RouteMapper.CreatePoint(modelStLat, modelStLng);
                dataModel.Stations.Add(st);
                dataModel.SaveChanges();
                return st.StationId;
            }
        }

        public List<StationModel> GetMainStations()
        {
            var stlist = new List<StationModel>();
            using (var dataModel = new MibarimEntities())
            {
                var ms = dataModel.vwMainStations;
                foreach (var mainStation in ms)
                {
                    var st = new StationModel();
                    st.Name = mainStation.MainStName;
                    st.StLat = mainStation.MainStLat.ToString();
                    st.StLng = mainStation.MainStLng.ToString();
                    st.MainStationId = mainStation.MainStationId;
                    stlist.Add(st);
                }
            }
            return stlist;
        }

        public List<StationModel> GetAdminMainStations()
        {
            var stlist = new List<StationModel>();
            using (var dataModel = new MibarimEntities())
            {
                var ms = dataModel.MainStations;
                foreach (var mainStation in ms)
                {
                    var st = new StationModel();
                    st.Name = mainStation.MainStName;
                    st.StLat = mainStation.MainStLat.ToString();
                    st.StLng = mainStation.MainStLng.ToString();
                    st.MainStationId = mainStation.MainStationId;
                    stlist.Add(st);
                }
            }
            return stlist;
        }

        public StationRouteModel GetStationRoute(long srcStId, long dstStId)
        {
            using (var dataModel = new MibarimEntities())
            {
                var station =
                    dataModel.vwStationRoutes.FirstOrDefault(
                        x => x.SrcMStationId == srcStId && x.DstMStationId == dstStId);
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
                stationRouteModel.StRoutePrice =
                    RouteMapper.PersianNumber(((int) station.DriverPrice).ToString("N0", new NumberFormatInfo()
                    {
                        NumberGroupSizes = new[] {3},
                        NumberGroupSeparator = ","
                    }));
                return stationRouteModel;
            }
        }

        public List<SubStationModel> GetSubStations(long mainStationId)
        {
            var stlist = new List<SubStationModel>();
            using (var dataModel = new MibarimEntities())
            {
                var ms = dataModel.Stations.Where(x => x.MainStationId == mainStationId);
                foreach (var station in ms)
                {
                    var st = new SubStationModel();
                    st.StAdd = station.StAdd;
                    st.StLat = station.StLat.ToString();
                    st.StLng = station.StLng.ToString();
                    st.StationId = station.StationId;
                    stlist.Add(st);
                }
            }
            return stlist;
        }

        public bool MakeStationRoutes()
        {
            using (var dataModel = new MibarimEntities())
            {
                double res = 0;
                var nightRes = "";
                var model = new SrcDstModel();
                long min = 1000000;
                long mid = 0;
                long max = 0;
                long mintime = 1000000;
                long midtime = 0;
                long maxtime = 0;
                var srcmainStations = dataModel.vwMainStations.ToList();
                var dstmainStations = dataModel.vwMainStations.ToList();
                foreach (var srcmainStation in srcmainStations)
                {
                    foreach (var dstMainStation in dstmainStations)
                    {
                        if (srcmainStation.MainStationId != dstMainStation.MainStationId)
                        {
                            var routeStation =
                                dataModel.StationRoutes.FirstOrDefault(
                                    x =>
                                        x.SrcMStationId == srcmainStation.MainStationId &&
                                        x.DstMStationId == dstMainStation.MainStationId);
                            if (routeStation == null || routeStation.DriverPrice == 0)
                            {
                                model = new SrcDstModel();
                                model.SrcLat = srcmainStation.MainStLat.ToString();
                                model.SrcLng = srcmainStation.MainStLng.ToString();
                                model.DstLat = dstMainStation.MainStLat.ToString();
                                model.DstLng = dstMainStation.MainStLng.ToString();
                                var privateGRoute = GetGoogleRoute(model, null);
                                if (privateGRoute.Routes.Count > 0)
                                {
                                    var distance =
                                        privateGRoute.Routes.FirstOrDefault().Legs.FirstOrDefault().Distance.Value;
                                    var duration =
                                        privateGRoute.Routes.FirstOrDefault().Legs.FirstOrDefault().Duration.Value;
                                    min = mid = max = distance;
                                    foreach (var route in privateGRoute.Routes)
                                    {
                                        if (route.Legs.Count > 0 && route.Legs.FirstOrDefault().Distance.Value > min &&
                                            route.Legs.FirstOrDefault().Distance.Value < max)
                                        {
                                            mid = route.Legs.FirstOrDefault().Distance.Value;
                                            midtime = route.Legs.FirstOrDefault().Duration.Value;
                                        }
                                        if (route.Legs.Count > 0 && route.Legs.FirstOrDefault().Distance.Value <= min)
                                        {
                                            min = route.Legs.FirstOrDefault().Distance.Value;
                                            mintime = route.Legs.FirstOrDefault().Duration.Value;
                                        }
                                        if (route.Legs.Count > 0 && route.Legs.FirstOrDefault().Distance.Value >= max)
                                        {
                                            max = route.Legs.FirstOrDefault().Distance.Value;
                                            maxtime = route.Legs.FirstOrDefault().Duration.Value;
                                        }
                                    }

                                    var averageTime = (distance*3600)/15000;
                                    //check it out
                                    if (min < distance)
                                    {
                                        distance = min;
                                    }
                                    long extraPrice = 0;
                                    if (averageTime < duration)
                                    {
                                        //extraPrice = ((duration - averageTime)/60)*132;
                                        extraPrice = 0;
                                    }
                                    if (distance <= 500)
                                    {
                                        res = RemoveDecimalToman(5566 + extraPrice);
                                    }
                                    else if (distance <= 7000)
                                    {
                                        res = RemoveDecimalToman((((distance - 500)/100)*253) + 5566 + extraPrice);
                                    }
                                    else if (distance <= 20000)
                                    {
                                        var first7000 = (65*253) + 5566;
                                        res =
                                            RemoveDecimalToman((((distance - 7000)/100)*202) + first7000 + extraPrice);
                                    }
                                    else if (distance > 20000)
                                    {
                                        var first20000 = (125*202) + (65*253) + 5566;
                                        res =
                                            RemoveDecimalToman((((distance - 20000)/100)*150) + first20000 + extraPrice);
                                    }

                                    var stationRoute = new StationRoute();
                                    if (routeStation == null)
                                    {
                                        var laststr =
                                            dataModel.StationRoutes.OrderByDescending(x => x.StationRouteId)
                                                .FirstOrDefault();


                                        stationRoute.StationRouteId = laststr == null ? 1 : laststr.StationRouteId + 1;
                                    }
                                    else
                                    {
                                        stationRoute = routeStation;
                                    }
                                    stationRoute.SrcMStationId = srcmainStation.MainStationId;
                                    stationRoute.DstMStationId = dstMainStation.MainStationId;
                                    stationRoute.IsDeleted = false;
                                    stationRoute.DistanceMin = min;
                                    stationRoute.DurationMin = mintime;
                                    stationRoute.DistanceMid = mid;
                                    stationRoute.DurationMid = midtime;
                                    stationRoute.DistanceMax = max;
                                    stationRoute.DurationMax = maxtime;
                                    stationRoute.PassPrice = (long) RemoveDecimalToman(res*10);
                                    stationRoute.DriverPrice = (long) RemoveDecimalToman(res*9);
                                    if (routeStation == null)
                                    {
                                        dataModel.StationRoutes.Add(stationRoute);
                                    }
                                    dataModel.SaveChanges();
                                    Thread.Sleep(10000);
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }

        public PaymentDetailModel RequestInvoice(int userId, long chargeAmount)
        {
            var payreq = new PaymentDetailModel();
            using (var dataModel = new MibarimEntities())
            {
                var pr = new PayReq();
                pr.PayReqCreateTime = DateTime.Now;
                pr.PayReqUserId = userId;
                pr.PayReqValue = chargeAmount;
                dataModel.PayReqs.Add(pr);
                dataModel.SaveChanges();
                payreq.BankLink = "http://mibarimapp.com/coreapi/PasargadPay?reqid=" + pr.PayReqId;
                payreq.State = 100;
            }
            return payreq;
        }

        

        public bool InsertEmployeeModel(EmployeeRequestModels model)
        {
            var em = new Employee();
            using (var dataModel = new MibarimEntities())
            {
                em.Name = model.Name;
                em.Family = model.Family;
                em.Email = model.Email;
                em.Mobile = model.Mobile;
                em.TimeStart = model.TimeStart;
                em.TimeEnd = model.TimeEnd;
                em.HasReturn = model.Hasreturn;
                em.Routeselect = model.Routeselect;
                em.Entry = model.Entry;
                em.Enterprise = model.Enterprise;
                em.Introduce = model.Introduce;
                if (model.Latitude != null)
                {
                    em.Geo = RouteMapper.CreatePoint(model.Latitude, model.Longitude);
                    em.Latitude = decimal.Parse(model.Latitude);
                    em.Longitude = decimal.Parse(model.Longitude);
                }
                dataModel.Employees.Add(em);
                dataModel.SaveChanges();
            }
            return true;
        }

        public bool InsertEventAttendeeModel(EventAttendeeModel model)
        {
            var em = new EventAttendee();
            using (var dataModel = new MibarimEntities())
            {
                em.Name = model.Name;
                em.Family = model.Family;
                em.EventAttendeeNo = model.EventAttendeeNo;
                em.Mobile = model.Mobile;
                em.EventName = model.EventName;
                em.Latitude = decimal.Parse(model.Latitude);
                em.Longitude = decimal.Parse(model.Longitude);
                dataModel.EventAttendees.Add(em);
                dataModel.SaveChanges();
            }
            return true;
        }

        public string SendDriverNotifs()
        {
            string notifstring = "";
            using (var dataModel = new MibarimEntities())
            {
                var days16ago = DateTime.Now.AddDays(-16);
                var days8ago = DateTime.Now.AddDays(-8);
                var days4ago = DateTime.Now.AddDays(-4);
                var days2ago = DateTime.Now.AddDays(-2);
                var yesterday = DateTime.Now.AddDays(-1);
                var drivers = dataModel.GetLastEnter(days16ago).ToList();
                foreach (var lastResult in drivers)
                {
                    if ((lastResult.TCreateTime.Date == days16ago.Date && lastResult.TCreateTime.Hour == days16ago.Hour &&
                         lastResult.TCreateTime.Minute == days16ago.Minute) ||
                        (lastResult.TCreateTime.Date == days8ago.Date &&
                         lastResult.TCreateTime.Hour == days8ago.Hour &&
                         lastResult.TCreateTime.Minute == days8ago.Minute) ||
                        (lastResult.TCreateTime.Date == days4ago.Date &&
                         lastResult.TCreateTime.Hour == days4ago.Hour &&
                         lastResult.TCreateTime.Minute == days4ago.Minute) ||
                        (lastResult.TCreateTime.Date == days2ago.Date &&
                         lastResult.TCreateTime.Hour == days2ago.Hour &&
                         lastResult.TCreateTime.Minute == days2ago.Minute) ||
                        (lastResult.TCreateTime.Date == yesterday.Date &&
                         lastResult.TCreateTime.Hour == yesterday.Hour &&
                         lastResult.TCreateTime.Minute == yesterday.Minute))
                    {
                        var notif = new NotifModel();
                        notif.Title = getResource.getMessage("SetRoute");
                        notif.Body = getResource.getMessage("SetTime");
                        notif.Tab = (int) MainTabs.Message;
                        notif.RequestCode = (int) NotificationType.SetRouteReminder;
                        notif.NotificationId = (int) NotificationType.SetRouteReminder;
                        _notifManager.SendNotifToDriver(notif, (int) lastResult.UserId);
                        //_notifManager.SendNotifToAdmins(notif);
                        notifstring = getResource.getString("SetTime");
                    }
                }
            }
            return notifstring;
        }

        public List<SubStationModel> GetAllSubStations()
        {
            var stlist = new List<SubStationModel>();
            using (var dataModel = new MibarimEntities())
            {
                var ms = dataModel.vwStations;
                foreach (var station in ms)
                {
                    var st = new SubStationModel();
                    st.StAdd = station.MainStName + " " + station.StAdd;
                    st.StLat = station.StLat.ToString();
                    st.StLng = station.StLng.ToString();
                    st.StationId = station.StationId;
                    stlist.Add(st);
                }
            }
            return stlist;
        }

        public FilterModel SetFilter(int userId, FilterModel model)
        {
            var filterModel = new FilterModel();
            using (var dataModel = new MibarimEntities())
            {

                if (model.FilterId == 0)
                {
                var st =
                    dataModel.StationRoutes.FirstOrDefault(
                        x => x.SrcMStationId == model.SrcStationId && x.DstMStationId == model.DstStationId);
                if (st != null)
                {
                
                var filter= new Filter();
                filter.SrcMStationId = model.SrcStationId;
                filter.DstMStationId = model.DstStationId;
                filter.StationRouteId = st.StationRouteId;
                filter.IsDelete = false;
                filter.CreateTime=DateTime.Now;
                filter.LastTimeSet=DateTime.Now;
                filter.FilterUserId = userId;
                filter.IsActive = false;
                dataModel.Filters.Add(filter);
                dataModel.SaveChanges();
                filterModel.FilterId = filter.FilterId;
                    //SendPairNotif(userId, st.StationRouteId);
                }
                else
                {
                    var r = new MessageResponse()
                    {
                        Type = ResponseTypes.Error,
                        Message = getResource.getMessage("NotFound")
                    };
                    _responseProvider.SetBusinessMessage(r);
                }
                }
                else
                {
                    var filter = dataModel.Filters.FirstOrDefault(x => x.FilterId == model.FilterId && x.FilterUserId==userId);
                    if (filter != null)
                    {
                        filter.LastTimeSet = GetNextDateTime(model.TimeHour, model.TimeMinute);
                        filter.IsActive = true;
                        dataModel.SaveChanges();
                        FindMatchTrips((DateTime)filter.LastTimeSet,filter.StationRouteId,filter.FilterId);
                    }
                    else
                    {
                        var r = new MessageResponse()
                        {
                            Type = ResponseTypes.Error,
                            Message = getResource.getMessage("NotFound")
                        };
                        _responseProvider.SetBusinessMessage(r);
                    }
                    
                }
            }
            return filterModel;
        }

        private int FindMatchTrips(DateTime filterLastTimeSet, long filterStationRouteId, long filterFilterId)
        {
            using (var dataModel = new MibarimEntities())
            {
                var matchTrips = dataModel.vwDriverTrips.Where(
                                x =>
                                   DbFunctions.TruncateTime(x.TStartTime) == filterLastTimeSet.Date &&
                                    x.TStartTime.Hour == filterLastTimeSet.Hour &&
                                    x.TStartTime.Minute == filterLastTimeSet.Minute
                                     && x.StationRouteId == filterStationRouteId && x.TState==(int)TripState.Scheduled).ToList();
                if (matchTrips.Count > 0)
                {
                    var filter = dataModel.Filters.FirstOrDefault(x => x.FilterId == filterFilterId);
                    filter.TripId = matchTrips.FirstOrDefault().TripId;
                    dataModel.SaveChanges();
                    /*var matchFilters = dataModel.Filters.Where(
                                x =>
                                   DbFunctions.TruncateTime(x.LastTimeSet) == filterLastTimeSet.Date &&
                                    ((DateTime)x.LastTimeSet).Hour == filterLastTimeSet.Hour &&
                                    ((DateTime)x.LastTimeSet).Minute == filterLastTimeSet.Minute
                                     && x.StationRouteId == filterStationRouteId && x.IsActive && !x.IsDelete).ToList();
                    if (matchFilters.Count > 0)
                    {
                        foreach (var matchFilter in matchFilters)
                        {
                            matchFilter.TripId = tripTripId;
                        }
                        dataModel.SaveChanges();
                    }*/


                }
                return matchTrips.Count;
            }
        }

        public List<FilterModel> GetUserFilters(int userId)
        {
            var filterModelList = new List<FilterModel>();
            using (var dataModel = new MibarimEntities())
            {
                var filters = dataModel.vwFilters.Where(x => x.FilterUserId == userId && !x.IsDelete);
                foreach (var filter in filters)
                {
                    var filterModel=new FilterModel();
                    filterModel.FilterId = filter.FilterId;
                    filterModel.SrcStationId = filter.SrcMStationId;
                    filterModel.SrcStation = filter.SrcStName;
                    filterModel.SrcStLat = filter.SrcStLat.ToString();
                    filterModel.SrcStLng = filter.SrcStLng.ToString();
                    filterModel.DstStationId = filter.DstMStationId;
                    filterModel.DstStation = filter.DstStName;
                    filterModel.DstStLat = filter.DstStLat.ToString();
                    filterModel.DstStLng = filter.DstStLng.ToString();
                    if (filter.IsActive)
                    {
                        filterModel.Time = (DateTime) filter.LastTimeSet;
                        filterModel.TimeHour= ((DateTime)filter.LastTimeSet).Hour;
                        filterModel.TimeMinute= ((DateTime)filter.LastTimeSet).Minute;
                        filterModel.IsActive = true;
                    }
                    else
                    {
                        filterModel.Time = null;
                        filterModel.IsActive = false;
                    }
                    filterModel.IsMatched = filter.TripId != null;
                    filterModelList.Add(filterModel);
                }
            }
            return filterModelList;
        }

        public List<FilterTimeModel> GetFilterTimes(int userId, FilterModel model)
        {
            var res=new List<FilterTimeModel>();
            using (var dataModel = new MibarimEntities())
            {
                var filter = dataModel.vwFilterPlus.FirstOrDefault(x => x.FilterId == model.FilterId && !x.IsDelete);
                //var stationplus = dataModel.StationRoutePlus.FirstOrDefault(x => x.StationRouteId == filter.StationRouteId);
                var times = dataModel.GetAggregatedTimes(DateTime.Now, filter.StationRouteId).ToList();
                var trips = dataModel.vwDriverTrips.Where(x => x.StationRouteId == filter.StationRouteId && x.TState==(int)TripState.Scheduled).ToList();
                if (times.Count > 0 || trips.Count > 0)
                {
                    foreach (var ti in times)
                    {
                        var price = GetTimePrice((DateTime) ti.AggregatedTime, filter);
                        price = RemoveCeilingDecimal(price/(ti.co.Value + 1));
                        var aggregatedTimes = new FilterTimeModel()
                        {
                            PairPassengers = ti.co.Value,
                            TimeString = ((DateTime) ti.AggregatedTime).ToString("HH:mm"),
                            PriceString = RouteMapper.PersianNumber(((long)price).ToString("N0", new NumberFormatInfo()
                            {
                                NumberGroupSizes = new[] {3},
                                NumberGroupSeparator = ","
                            })),
                            Price = price,
                            Time = (DateTime) ti.AggregatedTime,
                            TimeHour = ((DateTime) ti.AggregatedTime).Hour,
                            TimeMinute = ((DateTime) ti.AggregatedTime).Minute,
                            IsManual = false
                        };
                        res.Add(aggregatedTimes);
                    }
                    foreach (var trip in trips)
                    {
                        var filterCount = dataModel.vwFilters.Count(x => x.TripId == trip.TripId);
                        var price = trip.PassPrice;
                        //price = RemoveCeilingDecimal(price / (filterCount + 1));
                        var aggregatedTimes = new FilterTimeModel()
                        {
                            PairPassengers = filterCount,
                            TimeString = trip.TStartTime.ToString("HH:mm"),
                            PriceString = RouteMapper.PersianNumber(((long)price).ToString("N0", new NumberFormatInfo()
                            {
                                NumberGroupSizes = new[] { 3 },
                                NumberGroupSeparator = ","
                            })),
                            Price = price,
                            Time = trip.TStartTime,
                            TimeHour = trip.TStartTime.Hour,
                            TimeMinute = trip.TStartTime.Minute,
                            IsManual = false
                        };
                        res.Add(aggregatedTimes);
                    }
                }
                else
                {
                    var price = GetTimePrice(GetNextDateTime(7,30), filter);
                    if (price != null && (price != 0)) { 
                    var rr = new FilterTimeModel()
                    {
                        PairPassengers = 1,
                        TimeString = "7:30",
                        PriceString = RouteMapper.PersianNumber(((long)price/2).ToString("N0", new NumberFormatInfo()
                        {
                            NumberGroupSizes = new[] { 3 },
                            NumberGroupSeparator = ","
                        })),
                        Price = price / 2,
                        Time = DateTime.Now,
                        TimeHour = 7,
                        TimeMinute = 30,
                        IsManual = false
                    };
                    res.Add(rr);
                    }
                    var price2 = GetTimePrice(GetNextDateTime(17, 30), filter);
                    if (price2 != null && (price2 != 0))
                    {
                        var rr2 = new FilterTimeModel()
                        {
                            PairPassengers = 1,
                            TimeString = "17:30",
                            PriceString =
                                RouteMapper.PersianNumber(((long) price2/2).ToString("N0", new NumberFormatInfo()
                                {
                                    NumberGroupSizes = new[] {3},
                                    NumberGroupSeparator = ","
                                })),
                            Price = price2/2,
                            Time = DateTime.Now,
                            TimeHour = 17,
                            TimeMinute = 30,
                            IsManual = false
                        };
                        res.Add(rr2);
                    }
                }
                var maxPrice = RouteMapper.MaxPrice(filter)/10;
                if ((maxPrice != 0)) { 
                    var rr3 = new FilterTimeModel()
                {
                    PairPassengers = 0,
                    PriceString = RouteMapper.PersianNumber((maxPrice).ToString("N0", new NumberFormatInfo()
                    {
                        NumberGroupSizes = new[] { 3 },
                        NumberGroupSeparator = ","
                    })),
                    Price = maxPrice,
                    IsManual = true
                };
                res.Add(rr3);
                }
                /*
                    var price=stationplus.Seven/SameFilters.Count;
                    var rr=new FilterTimeModel()
                    {
                        PairPassengers = SameFilters.Count,TimeString = "7:30",PriceString = RouteMapper.PersianNumber((stationplus.Seven).ToString("N0", new NumberFormatInfo()
                        {
                            NumberGroupSizes = new[] { 3 },
                            NumberGroupSeparator = ","
                        })),Price = 2000,Time = DateTime.Now,TimeHour = DateTime.Now.Hour, TimeMinute = DateTime.Now.Minute, IsManual=false
                    };
                    res.Add(rr);
                var rr2 = new FilterTimeModel()
                    {
                        PairPassengers = 2,
                        TimeString = DateTime.Now.AddMinutes(30).ToString("HH:mm"),
                        PriceString = RouteMapper.PersianNumber((3000).ToString("N0", new NumberFormatInfo()
                        {
                            NumberGroupSizes = new[] { 3 },
                            NumberGroupSeparator = ","
                        })),
                        Price = 3000,TimeHour = DateTime.Now.AddMinutes(30).Hour, TimeMinute = DateTime.Now.AddMinutes(30).Minute, Time = DateTime.Now.AddMinutes(30),
                        IsManual = false
                    };
                    res.Add(rr2);
                    var rr3 = new FilterTimeModel()
                    {
                        PairPassengers = 1,
                        PriceString = RouteMapper.PersianNumber((6000).ToString("N0", new NumberFormatInfo()
                        {
                            NumberGroupSizes = new[] { 3 },
                            NumberGroupSeparator = ","
                        })),
                        Price = 6000, IsManual = true
                    };
                    res.Add(rr3);*/
            }
            return res;
        }

        public bool DeleteFilter(int userId, FilterModel model)
        {
            using (var dataModel = new MibarimEntities())
            {
                var filter = dataModel.Filters.FirstOrDefault(x => x.FilterId == model.FilterId);
                if (filter != null)
                {
                    if (filter.TripId != null)
                    {
                        if (filter.LastTimeSet < DateTime.Now.AddMinutes(-30))
                        {
                            filter.IsDelete = true;
                            dataModel.SaveChanges();
                            return true;
                        }
                        else
                        {
                            _responseProvider.SetBusinessMessage(new MessageResponse()
                            {
                                Type = ResponseTypes.Error,
                                Message = getResource.getMessage("TripAlreadyReserved")
                            });
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public List<SuggestModel> GetSuggestedRoutes()
        {
            var res=new List<SuggestModel>();
            using (var dataModel = new MibarimEntities())
            {
                var f = dataModel.vwFilters.FirstOrDefault();
                var aggfilters = dataModel.GetAggregatedFilters(DateTime.Now).ToList();
                var filterIds = aggfilters.Select(y => y.FilterId);
                var stationIds=aggfilters.Select(y => y.StationRouteId);
                var vwfilter = dataModel.vwFilters.Where(x => filterIds.Contains(x.FilterId)).ToList();
                //var vwfilter = dataModel.vwFilterPlus.Where(x => filterIds.Contains(x.FilterId)).ToList();
                var stPrices = dataModel.StationRoutes.Where(x => stationIds.Contains(x.StationRouteId));
                foreach (var aggf in aggfilters)
                {
                    var ff = vwfilter.FirstOrDefault(x => x.FilterId == aggf.FilterId);
                    var stprice = stPrices.FirstOrDefault(x => x.StationRouteId == aggf.StationRouteId);
                    var sug = new SuggestModel();
                    sug.FilterId = aggf.FilterId;
                    sug.SrcStationId = aggf.SrcMStationId;
                    sug.SrcStLat = ff.SrcStLat.ToString();
                    sug.SrcStLng = ff.SrcStLng.ToString();
                    sug.SrcStation = ff.SrcStName;
                    sug.PairPassengers = aggf.co.Value;
                    var price = stprice.DriverPrice;
                    //var price = GetTimePrice(aggf.LastTimeSet, ff);
                    //price = (int) (price/aggf.co);
                    if (price != null)
                    {
                        sug.Price = (long) (price* aggf.co);
                        sug.PriceString = ((long)(price * aggf.co)).ToString("N0", new NumberFormatInfo(){NumberGroupSizes = new[] { 3 },NumberGroupSeparator = ","});
                    }
                    sug.DstStationId = ff.DstMStationId;
                    sug.DstStation = ff.DstStName;
                    sug.DstStLat = ff.DstStLat.ToString();
                    sug.DstStLng = ff.DstStLng.ToString();
                    sug.Time = aggf.LastTimeSet;
                    sug.TimeHour = ((DateTime)aggf.LastTimeSet).Hour;
                    sug.TimeMinute = ((DateTime)aggf.LastTimeSet).Minute;
                    sug.PairPassengers = (int)aggf.co;
                    res.Add(sug);
                }
            }
            return res;
        }

        private long? GetTimePrice(DateTime? aggfLastTimeSet, vwFilterPlu ff)
        {
            if (aggfLastTimeSet.HasValue)
            {
                switch (aggfLastTimeSet.Value.Hour)
                {
                    case 5:
                        return ff.Five/10;
                    case 6:
                        return ff.Six / 10;
                    case 7:
                        return ff.Seven / 10;
                    case 8:
                        return ff.Eight / 10;
                    case 9:
                        return ff.Nine / 10;
                    case 10:
                        return ff.Ten / 10;
                    case 11:
                        return ff.Eleven / 10;
                    case 12:
                        return ff.Twelve / 10;
                    case 13:
                        return ff.Thirteen / 10;
                    case 14:
                        return ff.Fourteen / 10;
                    case 15:
                        return ff.Fifteen / 10;
                    case 16:
                        return ff.Sixteen / 10;
                    case 17:
                        return ff.Seventeen / 10;
                    case 18:
                        return ff.Eighteen / 10;
                    case 19:
                        return ff.Nineteen / 10;
                    case 20:
                        return ff.Twenty / 10;
                    case 21:
                        return ff.TwentyOne / 10;
                    case 22:
                        return ff.TwentyTwo / 10;
                    case 23:
                        return ff.TwentyThree / 10;
                }
            }
            return 0;
        }

        public bool CancelFilter(int userId, FilterModel model)
        {
            using (var dataModel = new MibarimEntities())
            {
                var filter = dataModel.Filters.FirstOrDefault(x => x.FilterId == model.FilterId);
                if (filter != null)
                {
                    if (filter.TripId != null)
                    {
                        if (filter.LastTimeSet < DateTime.Now.AddMinutes(-30))
                        {
                            filter.IsActive = false;
                            dataModel.SaveChanges();
                            return true;
                        }
                        else
                        {
                            _responseProvider.SetBusinessMessage(new MessageResponse()
                            {
                                Type = ResponseTypes.Error,
                                Message = getResource.getMessage("TripAlreadyReserved")
                            });
                            return false;
                        }
                    }
                    else
                    {
                        filter.IsActive = false;
                        dataModel.SaveChanges();
                        return true;
                    }
                    
                }
            }
            return true;
        }

        public TripTimeModel AcceptSuggestRoute(int userId, FilterModel model)
        {
            var res =new  TripTimeModel();
            using (var dataModel = new MibarimEntities())
            {
                var filter = dataModel.Filters.FirstOrDefault(x => x.FilterId == model.FilterId);
                var srcStation = dataModel.Stations.FirstOrDefault(x => x.MainStationId == filter.SrcMStationId);
                if (filter != null)
                {
                    var usr = dataModel.vwUserInfoes.FirstOrDefault(x => x.UserId == userId);
                    if (usr.VerifiedLevel != null && usr.VerifiedLevel >= (int)VerifiedLevel.Blocked)
                    {
                        _responseProvider.SetBusinessMessage(new MessageResponse()
                        {
                            Type = ResponseTypes.Error,
                            Message = getResource.getMessage("BlockedUser")
                        });
                        res.IsSubmited = false;
                        return res;
                    }
                    var route =
                        dataModel.StationRoutes.FirstOrDefault(
                            x => x.StationRouteId == filter.StationRouteId);
                        var driverRoute = new DriverRoute();
                        driverRoute.UserId = userId;
                        driverRoute.DrIsDeleted = false;
                        driverRoute.DrCreateTime = DateTime.Now;
                        var car = dataModel.vwCarInfoes.FirstOrDefault(x => x.UserId == userId);
                        if (car != null)
                        {
                            driverRoute.CarinfoId = car.CarInfoId;
                        }
                        driverRoute.DrSrcStationId = srcStation.StationId; //subStation.StationId;
                        driverRoute.StationRouteId = route.StationRouteId;
                        dataModel.DriverRoutes.Add(driverRoute);
                        dataModel.SaveChanges();

                    
                    var trip = new Trip();
                    trip.TStartTime = GetNextDateTime(((DateTime)filter.LastTimeSet).Hour, ((DateTime)filter.LastTimeSet).Minute);
                    res.RemainHour = (int)(trip.TStartTime - DateTime.Now).TotalHours;
                    var remainMin = (int)(trip.TStartTime - DateTime.Now).TotalMinutes;
                    res.RemainMin = (remainMin % 60);
                    trip.DriverRouteId = driverRoute.DriverRouteId;
                    trip.TCreateTime = DateTime.Now;
                    trip.TEmptySeat = model.CarSeats;
                    trip.TState = (int)TripState.Scheduled;
                    dataModel.Trips.Add(trip);
                    dataModel.SaveChanges();
                    //var stId = dataModel.DriverRoutes.FirstOrDefault(x => x.DriverRouteId == filter.StationRouteId);
                    FindMatchFilters(trip.TStartTime, filter.StationRouteId, trip.TripId);
                    //var time = trip.TStartTime;
                    //var time = ((DateTime)filter.LastTimeSet);
                    
                }
            }
            res.IsSubmited = true;
            return res;
        }

        private int FindMatchFilters(DateTime time, long filterStationRouteId, long tripTripId)
        {
            using (var dataModel = new MibarimEntities())
            {
                var matchFilters = dataModel.Filters.Where(
                                x =>
                                   DbFunctions.TruncateTime(x.LastTimeSet) == time.Date &&
                                    ((DateTime)x.LastTimeSet).Hour == time.Hour &&
                                    ((DateTime)x.LastTimeSet).Minute == time.Minute
                                     && x.StationRouteId == filterStationRouteId && x.IsActive && !x.IsDelete).ToList();
                if (matchFilters.Count > 0)
                {
                    foreach (var matchFilter in matchFilters)
                    {
                        matchFilter.TripId = tripTripId;
                    }
                    dataModel.SaveChanges();
                }
                return matchFilters.Count;
            }
            
        }

        public string SetUserNotifications()
        {
            using (var dataModel = new MibarimEntities())
            {
                var minbefore = DateTime.Now.AddHours(1);
                var activefilters =
                    dataModel.vwFilters.Where(x => x.IsActive && ((DateTime) x.LastTimeSet).Hour == minbefore.Hour
                    && ((DateTime)x.LastTimeSet).Minute == minbefore.Minute);
                foreach (var activefilter in activefilters)
                {
                    var usersWithFilter = dataModel.Filters.Where(y => y.StationRouteId == activefilter.StationRouteId);
                    foreach (var filter in usersWithFilter)
                    {
                        var notif=new Notification();
                        notif.FilterId = filter.FilterId;
                        notif.NotifCreateTime=DateTime.Now;
                        notif.NotifExpireTime = (DateTime) activefilter.LastTimeSet;
                        notif.IsNotificationSent = false;
                        notif.IsNotificationSeen = false;
                        notif.NotifBody = string.Format(getResource.getMessage("SimilarRoute"), activefilter.SrcStName, activefilter.DstStName,
                            ((DateTime)activefilter.LastTimeSet).Hour,((DateTime)activefilter.LastTimeSet).Minute);
                        notif.NotifTitle= string.Format(getResource.getMessage("GetAlong"));
                        notif.NotifUserId =(int) filter.FilterUserId;
                        notif.NotifType = (short) NotificationType.NotifForFilter;
                        dataModel.Notifications.Add(notif);
                    }
                    var driversWithFilter =
                        dataModel.DriverRoutes.Where(x => x.StationRouteId == activefilter.StationRouteId);
                    foreach (var driverFilter in driversWithFilter)
                    {
                        var notif = new Notification();
                        notif.NotifCreateTime = DateTime.Now;
                        notif.NotifExpireTime = (DateTime)activefilter.LastTimeSet;
                        notif.IsNotificationSent = false;
                        notif.IsNotificationSeen = false;
                        notif.NotifBody = string.Format(getResource.getMessage("SimilarDriverRoute"), activefilter.SrcStName, activefilter.DstStName,
                            ((DateTime)activefilter.LastTimeSet).Hour, ((DateTime)activefilter.LastTimeSet).Minute);
                        notif.NotifTitle = string.Format(getResource.getMessage("AcceptRide"));
                        notif.NotifUserId = (int)driverFilter.UserId;
                        notif.NotifType = (short)NotificationType.NotifForDriver;
                        dataModel.Notifications.Add(notif);
                    }
                    dataModel.SaveChanges();
                }
            }
            return "ok";
        }


        public bool MakeStationRoutePlus()
        {
            var tmService=new TaxiMeterService();

            using (var dataModel = new MibarimEntities())
            {
                var lastsnapToken =
                        dataModel.AppsTokens.OrderByDescending(x => x.TokenCreateTime)
                            .FirstOrDefault(x => x.TokenApp == (short)TokenApp.SnapApp);
                if (lastsnapToken.TokenState == (short)TokenStatus.Valid)
                {
                
                var stationRoutes = dataModel.vwStationRoutes.ToList();
                foreach (var vwStationRoute in stationRoutes)
                {
                        var sm=new StationRoutePlu();
                    var oldsm =
                        dataModel.StationRoutePlus.FirstOrDefault(x => x.StationRouteId == vwStationRoute.StationRouteId);
                    if (oldsm !=null)
                    {
                        if (!RouteMapper.IsHourNull(oldsm, DateTime.Now.Hour))
                        {
                                continue;
                        }
                        sm = oldsm;
                    }
                        var price = tmService.GetSnappPrice(lastsnapToken.Token, vwStationRoute.SrcMainStLat.ToString(), vwStationRoute.SrcMainStLng.ToString(), vwStationRoute.DstMainStLat.ToString(), vwStationRoute.DstMainStLng.ToString());
                        sm.StationRouteId = vwStationRoute.StationRouteId;
                    switch (DateTime.Now.Hour)
                    {
                            case 5:
                            sm.Five = price;
                                break;
                            case 6:
                            sm.Six = price;
                                break;
                            case 7:
                                sm.Seven = price;
                                break;
                            case 8:
                                sm.Eight = price;
                                break;
                            case 9:
                                sm.Nine = price;
                                break;
                            case 10:
                                sm.Ten = price;
                                break;
                            case 11:
                                sm.Eleven = price;
                                break;
                            case 12:
                                sm.Twelve = price;
                                break;
                            case 13:
                                sm.Thirteen = price;
                                break;
                            case 14:
                                sm.Fourteen = price;
                                break;
                            case 15:
                                sm.Fifteen = price;
                                break;
                            case 16:
                                sm.Sixteen = price;
                                break;
                            case 17:
                                sm.Seventeen = price;
                                break;
                            case 18:
                                sm.Eighteen = price;
                                break;
                            case 19:
                                sm.Nineteen = price;
                                break;
                            case 20:
                                sm.Twenty = price;
                                break;
                            case 21:
                                sm.TwentyOne = price;
                                break;
                            case 22:
                                sm.TwentyTwo = price;
                                break;
                            case 23:
                                sm.TwentyThree = price;
                                break;
                        }
                        if (oldsm ==null)
                    {
                            dataModel.StationRoutePlus.Add(sm);
                        }
                        //break;
                        dataModel.SaveChanges();
                        //Thread.Sleep(10000);
                    }
                    
                }
            }
            return true;
        }

        

        /*public PasargadPayModel PayPasargad(int userId, long tripId, long chargeAmount)
        {
            using (var dataModel = new MibarimEntities())
            {
                var trip = dataModel.vwDriverTrips.FirstOrDefault(x => x.TripId == tripId);
                if (trip != null)
                {
                    /*var user = dataModel.vwUserInfoes.FirstOrDefault(x => x.UserId == userId);
                    var payreq = _paymentManager.ChargePasargad(userId, chargeAmount,
                        user.Name + " " + user.Family,tripId,trip.TStartTime);#1#
                    var bookreq = new BookRequest();
                    bookreq.TripId = tripId;
                    bookreq.BrCreateTime = DateTime.Now;
                    dataModel.BookRequests.Add(bookreq);
                    dataModel.SaveChanges();
                    return payreq;
                }
            }
            return null;
        }*/

        public
            GeoCoordinate GetCentralGeoCoordinate(
                IList<GeoCoordinate> geoCoordinates)
        {
            if (
                geoCoordinates.Count == 1)
            {
                return
                    geoCoordinates.Single();
            }
            double x = 0;

            double y = 0;
            double z = 0;

            foreach (var geoCoordinate in geoCoordinates)
            {
                var latitude = geoCoordinate.Latitude*Math.PI/180;
                var longitude = geoCoordinate.Longitude*Math.PI/180;

                x
                    +=
                    Math.Cos
                    (
                        latitude
                    )*
                    Math.Cos
                    (
                        longitude
                    );
                y
                    +=
                    Math.Cos
                    (
                        latitude
                    )*
                    Math.Sin
                    (
                        longitude
                    );
                z
                    +=
                    Math.Sin
                    (
                        latitude
                    );
            }

            var total = geoCoordinates.Count;

            x = x/total;
            y = y/total;
            z = z/total;

            var centralLongitude = Math.Atan2(y, x);
            var centralSquareRoot = Math.Sqrt(x*x + y*y);
            var centralLatitude = Math.Atan2(z, centralSquareRoot);

            return
                new
                    GeoCoordinate(centralLatitude*180/Math.PI, centralLongitude*180/Math.PI);
        }

        /*private DateTime GetNextDateTime(int hour, int min)
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
DateTime nextDatetime = current.AddHours(hoursUntilNext).AddMinutes(minuteUntilNext);#1#
            return nextDatetime;
        }*/
        private DateTime GetNextDateTime(int hour, int min)
        {
            DateTime current = DateTime.Now;
            DateTime nextDatetime = DateTime.Now;
            nextDatetime = new DateTime(current.Year, current.Month, current.Day, hour, min, 0);
            if (nextDatetime < current)
            {
                nextDatetime = new DateTime(current.Year, current.Month, current.AddDays(1).Day, hour, min, 0);
            }
            if (nextDatetime < current)
            {
                nextDatetime = new DateTime(current.Year, current.AddMonths(1).Month, current.AddDays(1).Day, hour, min, 0);
            }
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

        private long RemoveCeilingDecimal(double? priceValue)
        {
            if (priceValue != null)
            {
                return (long) (Math.Ceiling((priceValue.Value) / 1000)* 1000);
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