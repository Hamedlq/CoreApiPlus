using System;
using System.Collections.Generic;
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
        private IRouteGroupManager _routeGroupManager;
        private ILogProvider _logmanager;
        private IResponseProvider _responseProvider;


        public DriverController()
        {
        }
        public DriverController(IRouteManager routeManager,
            ILogProvider logManager,
            IRouteGroupManager routeGroupManager,
            IResponseProvider responseProvider)
        {
            _routemanager = routeManager;
            _logmanager = logManager;
            _routeGroupManager = routeGroupManager;
            _responseProvider = responseProvider;
        }

        [HttpPost]
        [Route("SetUserRoute")]
        public IHttpActionResult SetUserRoute(StationRouteModel model)
        {
            try
            {
                int ff;
                if (User != null && int.TryParse(User.Identity.GetUserId(), out ff))
                {
                    var res = _routemanager.SetUserRoute(ff, model.StRouteId);
                    return Json(_responseProvider.GenerateRouteResponse(new {IsSubmited=res}, "SetUserRoute"));
                }
                else
                {
                    return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "You are unauthorized to see Info!"));
                }
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "SetUserRoute", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }
        [HttpPost]
        [Route("DeleteDriverRoute")]

        public IHttpActionResult DeleteDriverRoute(DriverRouteModel model)
        {
            try
            {
                int ff;
                if (User != null && int.TryParse(User.Identity.GetUserId(), out ff))
                {
                    var res = _routemanager.DeleteDriverRoute(ff,model.DriverRouteId);
                    return Json(_responseProvider.GenerateRouteResponse(new { IsDeleted = res }, "DeleteDriverRoute"));
                }
                else
                {
                    return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "You are unauthorized to see Info!"));
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
        [Route("GetDriverRoutes")]
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
                    return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "You are unauthorized to see Info!"));
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
        public IHttpActionResult SetTrip(DriverRouteModel model)
        {
            try
            {
                int ff;
                if (User != null && int.TryParse(User.Identity.GetUserId(), out ff))
                {
                    var res = _routemanager.SetDriverTrip(ff, model);
                    return Json(_responseProvider.GenerateRouteResponse(new { IsSubmited = res }, "SetTrip"));
                }
                else
                {
                    return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "You are unauthorized to see Info!"));
                }
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "SetUserRoute", e.Message);
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
                return Json(_responseProvider.GenerateResponse(new List<string> { res }, "InvokeTrips"));
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "InvokeTrips", e.Message + "-" + e.InnerException.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }
    }
}
