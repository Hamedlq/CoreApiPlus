using System;
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
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CoreApi.Controllers
{
    [Authorize]
    public class PassengerController : ApiController
    {
        private static string Tag = "PassengerController";
        private IRouteManager _routemanager;
        private IRouteGroupManager _routeGroupManager;
        private ILogProvider _logmanager;
        private IResponseProvider _responseProvider;


        public PassengerController()
        {
        }
        public PassengerController(IRouteManager routeManager,
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
        [Route("GetPassengerRoutes")]
        public IHttpActionResult GetPassengerRoutes(PassFilterModel model)
        {
            try
            {
                int ff;
                if (User != null && int.TryParse(User.Identity.GetUserId(), out ff))
                {
                    var res = _routemanager.GetPassengerRoutes(ff,model);
                    return Json(_responseProvider.GenerateRouteResponse(res));
                }
                else
                {
                    return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "You are unauthorized to see Info!"));
                }
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "GetPassengerRoutes", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("GetPassengerStRoute")]
        [AllowAnonymous]
        public IHttpActionResult GetPassengerStRoute()
        {
            try
            {
                var res = _routemanager.GetPassengerStationRoutes();
                return Json(_responseProvider.GenerateRouteResponse(res));
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "GetStationRoutes", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("RequestBooking")]
        public IHttpActionResult RequestBooking(PassRouteModel model)
        {
            try
            {
                int ff;
                if (User != null && int.TryParse(User.Identity.GetUserId(), out ff))
                {
                    var res = _routemanager.RequestBooking(ff, model.TripId);
                    return Json(res);
                }
                else
                {
                    return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "You are unauthorized to see Info!"));
                }
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "RequestBooking", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("SetPassLocation")]
        [Authorize]
        public IHttpActionResult SetPassLocation(PassRouteModel model)
        {
            try
            {
                int ff;
                int.TryParse(User.Identity.GetUserId(), out ff);
                var res = _routemanager.SetPassLocation(ff, model);
                return Json(_responseProvider.GenerateRouteResponse(res, "SetPassLocation"));
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "SetPassLocation", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }
    }
}
