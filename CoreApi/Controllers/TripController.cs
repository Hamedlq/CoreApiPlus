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
    public class TripController : ApiController
    {
        private static string Tag = "TripController";
        private IRouteManager _routemanager;
        private IRouteGroupManager _routeGroupManager;
        private ILogProvider _logmanager;
        private IResponseProvider _responseProvider;


        public TripController()
        {
        }
        public TripController(IRouteManager routeManager, 
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
        [Route("GetTripInfo")]
        public IHttpActionResult GetTripInfo(TripRequest model)
        {
            try
            {
                var res = _routemanager.GetTripInfo(int.Parse(User.Identity.GetUserId()), model.TripId);
                return Json(_responseProvider.GenerateRouteResponse(res, "TripInfo"));
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "SendUserTripLocation", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }
        [HttpPost]
        [Route("EndTripRequest")]
        public IHttpActionResult EndTripRequest(TripRequest model)
        {
            try
            {
                //var res = _routemanager.EndTrip(int.Parse(User.Identity.GetUserId()), model.TripId);
                //return Json(_responseProvider.GenerateRouteResponse(res, "TripInfo"));
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "SendUserTripLocation", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("ToggleContactState")]
        public IHttpActionResult ToggleContactState(ContactStateModel model)
        {
            try
            {
                var res = _routemanager.ToggleContactState(int.Parse(User.Identity.GetUserId()), model.ContactId);

                return Json(_responseProvider.GenerateRouteResponse(res, "ContactState"));
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "ToggleContactState", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());

        }
        
    }
}
