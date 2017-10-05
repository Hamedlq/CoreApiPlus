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
    public class StationController : ApiController
    {
        private static string Tag = "StationController";
        private IRouteManager _routemanager;
        private IRouteGroupManager _routeGroupManager;
        private ILogProvider _logmanager;
        private IResponseProvider _responseProvider;


        public StationController()
        {
        }
        public StationController(IRouteManager routeManager, 
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
        [Authorize]
        [Route("GetMainStations")]
        public IHttpActionResult GetMainStations()
        {
            try
            {
                var res = _routemanager.GetMainStations();
                return Json(_responseProvider.GenerateRouteResponse(res));
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "GetMainStations", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Authorize]
        [Route("GetStationRoute")]
        public IHttpActionResult GetStationRoute(StationRouteModel model)
        {
            try
            {
                var res = _routemanager.GetStationRoute(model.SrcStId, model.DstStId);
                return Json(_responseProvider.GenerateRouteResponse(res, "StationRoute"));
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "GetStationRoute", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }


        [HttpPost]
        [Authorize]
        [Route("GetSubStations")]
        public IHttpActionResult GetSubStations(StationModel model)
        {
            try
            {
                var res = _routemanager.GetSubStations(model.MainStationId);
                return Json(_responseProvider.GenerateRouteResponse(res));
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "GetSubStations", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }


        [HttpPost]
        [Authorize(Roles = "AdminUser")]
        [Route("SubmitMainStation")]
        public IHttpActionResult SubmitMainStation(StationModel model)
        {
            try
            {
                var res = _routemanager.SubmitMainStation(int.Parse(User.Identity.GetUserId()), model.Name,model.StLat, model.StLng);
                return Json(_responseProvider.GenerateRouteResponse(res, "SubmitMainStation"));
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "SubmitMainStation", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Authorize(Roles = "AdminUser")]
        [Route("SubmitStation")]
        public IHttpActionResult SubmitStation(StationModel model)
        {
            try
            {
                var res = _routemanager.SubmitStation(int.Parse(User.Identity.GetUserId()), model.Name, model.StLat, model.StLng,model.MainStationId);
                return Json(_responseProvider.GenerateRouteResponse(res, "SubmitStation"));
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "SubmitStation", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }
        
    }
}
