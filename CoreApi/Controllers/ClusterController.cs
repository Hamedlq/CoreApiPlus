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
using CoreManager.AdminManager;
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
    public class ClusterController : ApiController
    {
        private static string Tag = "ClusterController";
        private IRouteManager _routemanager;
        private IAdminManager _adminmanager;
        private IRouteGroupManager _routeGroupManager;
        private ILogProvider _logmanager;
        private IResponseProvider _responseProvider;


        public ClusterController()
        {
        }

        public ClusterController(IRouteManager routeManager,
            ILogProvider logManager,
            IRouteGroupManager routeGroupManager,
            IAdminManager adminManager,
            IResponseProvider responseProvider)
        {
            _routemanager = routeManager;
            _logmanager = logManager;
            _adminmanager = adminManager;
            _routeGroupManager = routeGroupManager;
            _responseProvider = responseProvider;
        }

        [HttpPost]
        [Route("GetAllClusters")]
        [Authorize(Roles = "AdminUser")]
        public IHttpActionResult GetAllClusters()
        {
            try
            {
                var list = _adminmanager.GetAllClusters();
                return Json(_responseProvider.GenerateRouteResponse(list,"clusters"));
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "GetAllRoutes", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("GetCluster")]
        [Authorize(Roles = "AdminUser")]
        public IHttpActionResult GetCluster(ClusterListModel model)
        {
            try
            {
                var list = _adminmanager.GetCluster(model.ClusterId);
                return Json(_responseProvider.GenerateRouteResponse(list, "clusters"));
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "GetCluster", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }
    }
}
