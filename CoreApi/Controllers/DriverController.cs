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
using CoreManager.DriverManager;
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
        private IDriverManager _drivermanager;
        private IUserManager _userManager;
        private ILogProvider _logmanager;
        private IResponseProvider _responseProvider;


        public DriverController()
        {
        }

        public DriverController(IRouteManager routeManager,
            ILogProvider logManager,
            IUserManager userManager,
            IDriverManager driverManager,
            IResponseProvider responseProvider)
        {
            _routemanager = routeManager;
            _logmanager = logManager;
            _userManager = userManager;
            _drivermanager = driverManager;
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
                    bool isSubmitted = res > 0;
                    return Json(_responseProvider.GenerateRouteResponse(new {IsSubmited = isSubmitted, DriverRouteId =res}, "SetUserRoute"));
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
        [Route("SetRoute")]
        [Authorize]
        public IHttpActionResult SetRoute(StationRouteModel model)
        {
            try
            {
                int ff;
                if (User != null && int.TryParse(User.Identity.GetUserId(), out ff))
                {
                    var res = _routemanager.SetRoute(ff, model.SrcStId, model.DstStId);
                    bool isSubmitted = res > 0;
                    return Json(_responseProvider.GenerateRouteResponse(new { IsSubmited = isSubmitted, DriverRouteId = res }, "SetUserRoute"));
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
        [Route("GetPassengersInfo")]
        public IHttpActionResult GetPassengersInfo(DriverRouteModel model)
        {
            try
            {
                int ff;
                if (User != null && int.TryParse(User.Identity.GetUserId(), out ff))
                {
                    var res = _routemanager.GetPassengersInfo(ff,model);
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
                _logmanager.Log(Tag, "GetPassengersInfo", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }
        [HttpPost]
        [Route("GetDriverNewRoutes")]
        [Authorize]
        public IHttpActionResult GetDriverNewRoutes()
        {
            try
            {
                int ff;
                if (User != null && int.TryParse(User.Identity.GetUserId(), out ff))
                {
                    var res = _routemanager.GetDriverNewRoutes(ff);
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
            //_responseProvider.SetBusinessMessage(new MessageResponse()
            //{
            //    Type = ResponseTypes.Error,
            //    Message = getResource.getMessage("CodeUsed")
            //});

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
                //return Json(_responseProvider.GenerateResponse(new List<string> {res}, "InvokeTrips"));
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    _logmanager.Log(Tag, "InvokeTrips", e.Message + "-" + e.InnerException.Message);
                }
                else
                {
                    _logmanager.Log(Tag, "InvokeTrips", e.Message);
                }
            }
            try
            {
                var res = _routemanager.InvokeFilters();
                //return Json(_responseProvider.GenerateResponse(new List<string> {res}, "InvokeTrips"));
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    _logmanager.Log(Tag, "InvokeFilters", e.Message + "-" + e.InnerException.Message);
                }
                else
                {
                    _logmanager.Log(Tag, "InvokeFilters", e.Message);
                }
            }
            try
            {
                var res=_routemanager.SendDriverNotifs();
                //return Json(_responseProvider.GenerateResponse(new List<string> { res }, "InvokeTrips"));
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    _logmanager.Log(Tag, "SendDriverNotifs", e.Message + "-" + e.InnerException.Message);
                }
                else
                {
                    _logmanager.Log(Tag, "SendDriverNotifs", e.Message);
                }
            }
            try
            {
                var res = _routemanager.SetUserNotifications();
                //return Json(_responseProvider.GenerateResponse(new List<string> { res }, "InvokeTrips"));
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    _logmanager.Log(Tag, "SetUserNotifications", e.Message + "-" + e.InnerException.Message);
                }
                else
                {
                    _logmanager.Log(Tag, "SetUserNotifications", e.Message);
                }
            }
            try
            {
                var res = _routemanager.SendNewBookedMessages(); 
                return Json(_responseProvider.GenerateResponse(new List<string> { res }, "InvokeTrips"));
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    _logmanager.Log(Tag, "SendNewBookedMessages", e.Message + "-" + e.InnerException.Message);
                }
                else
                {
                    _logmanager.Log(Tag, "SendNewBookedMessages", e.Message);
                }
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



        [HttpPost]
        [Route("GetDriverInvite")]
        public IHttpActionResult GetDriverInvite()
        {
            try
            {
                var res = _userManager.GetUserInvite(int.Parse(User.Identity.GetUserId()),InviteTypes.DriverInvite);
                //var jsonRes = Json(_responseProvider.GenerateRouteResponse(res,"AboutMe"));
                return Json(res);
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "GetInvite", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("SaveDriverGcmToken")]
        [Authorize]
        public IHttpActionResult SaveDriverGcmToken(Gtoken model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(_responseProvider.GenerateBadRequestResponse(ModelState));
                }
                _userManager.InsertGoogleToken(int.Parse(User.Identity.GetUserId()), model,UserRoles.MobileDriver);
                return Json(_responseProvider.GenerateOKResponse());
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "SaveDriverGcmToken", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpGet]
        [Route("MakeStationRoutes")]
        [AllowAnonymous]
        public IHttpActionResult MakeStationRoutes([FromUri]int key)
        {
            try
            {
                if (key == 123) { 
                var res = _routemanager.MakeStationRoutes();
                return Json(_responseProvider.GenerateOKResponse());
                }
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    _logmanager.Log(Tag, "MakeStationRoutes", e.Message + "-" + e.InnerException.Message);
                }
                else
                {
                    _logmanager.Log(Tag, "MakeStationRoutes", e.Message);
                }
                
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("GetSuggestedRoutes")]
        [AllowAnonymous]
        public IHttpActionResult GetSuggestedRoutes()
        {
            try
            {
                var res = _routemanager.GetSuggestedRoutes();
                return Json(_responseProvider.GenerateRouteResponse(res));
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    _logmanager.Log(Tag, "GetSuggestedRoutes", e.Message + "-" + e.InnerException.Message);
                }
                else
                {
                    _logmanager.Log(Tag, "GetSuggestedRoutes", e.Message);
                }
                
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("AcceptSuggestRoute")]
        public IHttpActionResult AcceptSuggestRoute(FilterModel model)
        {
            try
            {
                int ff;
                int.TryParse(User.Identity.GetUserId(), out ff);
                var res = _routemanager.AcceptSuggestRoute(ff,model,false);
                return Json(_responseProvider.GenerateRouteResponse(res, "AcceptSuggestRoute"));
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    _logmanager.Log(Tag, "AcceptSuggestRoute", e.Message + "-" + e.InnerException.Message);
                }
                else
                {
                    _logmanager.Log(Tag, "AcceptSuggestRoute", e.Message);
                }
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("AcceptSuggestRouteByAdmin")]
        [Authorize(Roles = "AdminUser")]
        public IHttpActionResult AcceptSuggestRouteByAdmin(FilterModel model)
        {
            try
            {
                int ff;
                int.TryParse(User.Identity.GetUserId(), out ff);
                var res = _routemanager.AcceptSuggestRoute(ff, model,true);
                return Json(_responseProvider.GenerateRouteResponse(res, "AcceptSuggestRouteByAdmin"));
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    _logmanager.Log(Tag, "AcceptSuggestRouteByAdmin", e.Message + "-" + e.InnerException.Message);
                }
                else
                {
                    _logmanager.Log(Tag, "AcceptSuggestRouteByAdmin", e.Message);
                }
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpGet]
        [Route("DriverIosVersion")]
        [AllowAnonymous]
        public IHttpActionResult DriverIosVersion()
        {
            try
            {
                IosVersionModel v = new IosVersionModel();
                v.UrlCode = 1;
                v.VersionCode = 1;
                v.NewUrl = "https://new.sibapp.com/applications/mibarimapp";
                return Json(v);
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    _logmanager.Log(Tag, "DriverIosVersion", e.Message + "-" + e.InnerException.Message);
                }
                else
                {
                    _logmanager.Log(Tag, "DriverIosVersion", e.Message);
                }
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("PresentView")]
        public IHttpActionResult PresentView()
        {
            try
            {
                int ff;
                if (User != null && int.TryParse(User.Identity.GetUserId(), out ff))
                {
                    PresentViewModel v = new PresentViewModel();
                    var ui=_userManager.GetUser(ff);
                    /*v.PresentUrl = "http://mibarim.ir/Driver/DriverRate?useruid="+ui.UserUId;
                    v.WebViewPageUrl = "http://mibarim.ir/Driver/DriverRatePage?useruid=" + ui.UserUId;*/
                    v.PresentUrl = null;
                    v.WebViewPageUrl = null;
                    return Json(_responseProvider.GenerateResponse(v, "PresentView"));
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
                if (e.InnerException != null)
                {
                    _logmanager.Log(Tag, "PresentView", e.Message + "-" + e.InnerException.Message);
                }
                else
                {
                    _logmanager.Log(Tag, "PresentView", e.Message);
                }
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpGet]
        [Route("MakeStationRoutePlus")]
        [AllowAnonymous]
        public IHttpActionResult MakeStationRoutePlus(string ccd)
        {
            try
            {
                if (ccd=="123")
                {
                    var res = _routemanager.MakeStationRoutePlus();
                    return Json(_responseProvider.GenerateOKResponse());
                }
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    _logmanager.Log(Tag, "MakeStationRoutePlus", e.Message + "-" + e.InnerException.Message);
                }
                else
                {
                    _logmanager.Log(Tag, "MakeStationRoutePlus", e.Message);
                }

            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpGet]
        [Route("GasScores")]
        [AllowAnonymous]
        public IHttpActionResult GasScores([FromUri]string userUId)
        {
            try
            {
                var uid=new Guid(userUId);
                    var res = _drivermanager.GetGasScores(uid);
                    return Json(_responseProvider.GenerateRouteResponse(res, "GasScores"));
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "GasScores", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpGet]
        [Route("GasRanks")]
        [AllowAnonymous]
        public IHttpActionResult GasRanks([FromUri]string userUId)
        {
            try
            {
                var uid = new Guid(userUId);
                var res = _drivermanager.GasRanks(uid);
                return Json(_responseProvider.GenerateRouteResponse(res));
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "GasRanks", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }
    }
}