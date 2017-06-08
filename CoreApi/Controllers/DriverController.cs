﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using System.Web.Http.Results;
using System.Web.Script.Serialization;
using CoreManager.LogProvider;
using CoreManager.Models;
using CoreManager.Models.RouteModels;
using CoreManager.Resources;
using CoreManager.ResponseProvider;
using CoreManager.RouteGroupManager;
using CoreManager.RouteManager;
using CoreManager.UserManager;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CoreApi.Controllers
{
    [Authorize]
    public class DriverController : ApiController
    {
        private static string Tag = "DriverController";
        private IRouteManager _routemanager;
        private IUserManager _userManager;
        private ILogProvider _logmanager;
        private IResponseProvider _responseProvider;


        public DriverController()
        {
        }

        public DriverController(IRouteManager routeManager,
            ILogProvider logManager,
            IUserManager userManager,
            IResponseProvider responseProvider)
        {
            _routemanager = routeManager;
            _logmanager = logManager;
            _userManager = userManager;
            _responseProvider = responseProvider;
        }


        [HttpPost]
        [Route("SetUserRoute")]
        [Authorize]
        public IHttpActionResult SetUserRoute(SubStationModel model)
        {
            try
            {
                int ff;
                if (User != null && int.TryParse(User.Identity.GetUserId(), out ff))
                {
                    var res = _routemanager.SetUserRoute(ff, model.StRouteId,model.StationId);
                    return Json(_responseProvider.GenerateRouteResponse(new {IsSubmited = res}, "SetUserRoute"));
                }
                else
                {
                    return
                        ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.Unauthorized,
                            "You are unauthorized to see Info!"));
                }
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "SetUserRoute", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("SetTripLocation")]
        [Authorize]
        public IHttpActionResult SetTripLocation(DriverTripModel model)
        {
            try
            {
                int ff;
                int.TryParse(User.Identity.GetUserId(), out ff);
                    var res = _routemanager.SetTripLocation(ff, model);
                    return Json(_responseProvider.GenerateRouteResponse(res, "SetTripLocation"));
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "SetTripLocation", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("GetDriverInitialInfo")]
        [Authorize]
        public IHttpActionResult GetDriverInitialInfo()
        {
            try
            {
                int ff;
                if (User != null && int.TryParse(User.Identity.GetUserId(), out ff))
                {
                    var res = _userManager.GetDriverInitialInfo(ff);
                    return Json(_responseProvider.GenerateRouteResponse(res, "GetDriverInitialInfo"));
                }
                else
                {
                    return
                        ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.Unauthorized,
                            "You are unauthorized to see Info!"));
                }
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "GetDriverInitialInfo", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }
        //[HttpPost]
        //[Route("GetDriveRouteInfo")]
        //[Authorize]
        //public IHttpActionResult GetDriveRouteInfo(DriverRouteModel model)
        //{
        //    try
        //    {
        //        int ff;
        //        if (User != null && int.TryParse(User.Identity.GetUserId(), out ff))
        //        {
        //            var res = _routemanager.GetRouteInfo(ff, model.DriverRouteId);
        //            return Json(_responseProvider.GenerateRouteResponse(res, "GetDriveRouteInfo"));
        //        }
        //        else
        //        {
        //            return
        //                ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.Unauthorized,
        //                    "You are unauthorized to see Info!"));
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        _logmanager.Log(Tag, "GetDriveRouteInfo", e.Message);
        //    }
        //    return Json(_responseProvider.GenerateUnknownErrorResponse());
        //}

        [HttpPost]
        [Route("GetUserTrips")]
        [Authorize]
        public IHttpActionResult GetUserTrips()
        {
            try
            {
                int ff;
                int.TryParse(User.Identity.GetUserId(), out ff);
                    var res = _routemanager.GetUserTrips(ff);
                    return Json(_responseProvider.GenerateRouteResponse(res, "GetUserTrips"));
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "GetUserTrips", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("DeleteDriverRoute")]
        [Authorize]
        public IHttpActionResult DeleteDriverRoute(DriverRouteModel model)
        {
            try
            {
                int ff;
                if (User != null && int.TryParse(User.Identity.GetUserId(), out ff))
                {
                    var res = _routemanager.DeleteDriverRoute(ff, model.DriverRouteId);
                    return Json(_responseProvider.GenerateRouteResponse(new {IsDeleted = res}, "DeleteDriverRoute"));
                }
                else
                {
                    return
                        ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.Unauthorized,
                            "You are unauthorized to see Info!"));
                }
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "DeleteDriverRoute", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("GetStationRoutes")]
        [AllowAnonymous]
        public IHttpActionResult GetStationRoutes()
        {
            try
            {
                var res = _routemanager.GetStationRoutes();
                return Json(_responseProvider.GenerateRouteResponse(res));
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "GetStationRoutes", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }
        [HttpPost]
        [Route("GetStations")]
        [AllowAnonymous]
        public IHttpActionResult GetStations(StationRouteModel model)
        {
            try
            {
                var res = _routemanager.GetStations(model.StRouteId);
                return Json(_responseProvider.GenerateRouteResponse(res));
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "GetStations", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("GetDriverRoutes")]
        [Authorize]
        public IHttpActionResult GetDriverRoutes()
        {
            try
            {
                int ff;
                if (User != null && int.TryParse(User.Identity.GetUserId(), out ff))
                {
                    var res = _routemanager.GetDriverRoutes(ff);
                    return Json(_responseProvider.GenerateRouteResponse(res));
                }
                else
                {
                    return
                        ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.Unauthorized,
                            "You are unauthorized to see Info!"));
                }
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "GetDriverRoutes", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("SetTrip")]
        [Authorize]
        public IHttpActionResult SetTrip(DriverRouteModel model)
        {
            try
            {
                int ff;
                if (User != null && int.TryParse(User.Identity.GetUserId(), out ff))
                {
                    var res = _routemanager.SetDriverTrip(ff, model);
                    return Json(_responseProvider.GenerateRouteResponse(res, "SetTrip"));
                }
                else
                {
                    return
                        ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.Unauthorized,
                            "You are unauthorized to see Info!"));
                }
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "SetTrip", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("DisableTrip")]
        [Authorize]
        public IHttpActionResult DisableTrip(DriverRouteModel model)
        {
            try
            {
                int ff;
                if (User != null && int.TryParse(User.Identity.GetUserId(), out ff))
                {
                    var res = _routemanager.DisableDriverTrip(ff, model);
                    return Json(_responseProvider.GenerateRouteResponse(new {IsDisabled = res}, "DisableTrip"));
                }
                else
                {
                    return
                        ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.Unauthorized,
                            "You are unauthorized to see Info!"));
                }
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "DisableTrip", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpGet]
        [Route("InvokeTrips")]
        [AllowAnonymous]
        public IHttpActionResult InvokeTrips()
        {
            try
            {
                var res = _routemanager.InvokeTrips();
                return Json(_responseProvider.GenerateResponse(new List<string> {res}, "InvokeTrips"));
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "InvokeTrips", e.Message + "-" + e.InnerException.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("GetDriverAppVersion")]
        [AllowAnonymous]
        public IHttpActionResult GetDriverAppVersion()
        {
            var _appVersion = ConfigurationManager.AppSettings["MobileDriverAppVersion"];
            return Json(new ResponseModel() {Messages = new List<string>() {_appVersion}});
        }
    }
}