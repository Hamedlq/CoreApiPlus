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
    [AllowAnonymous]
    public class TaxiMeterController : ApiController
    {
        private static string Tag = "TaxiMeterController";
        private IRouteManager _routemanager;
        private IRouteGroupManager _routeGroupManager;
        private ILogProvider _logmanager;
        private IResponseProvider _responseProvider;

        
        public TaxiMeterController()
        {
        }
        public TaxiMeterController(IRouteManager routeManager, 
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
        [Route("GetPathPrice")]
        [AllowAnonymous]
        public IHttpActionResult GetPathPrice(SrcDstModel model)
        {
            try
            {
                var routePrice = _routemanager.GetPathPrice(model);
                ResponseModel responseModel = _responseProvider.GenerateResponse( routePrice , "pathprice");
                return Json(responseModel);
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "GetPathPrice", e.Message);
            }
            return null;
        }

    }
}
