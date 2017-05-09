using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Spatial;
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
using CoreManager.PricingManager;
using CoreManager.Resources;
using CoreManager.ResponseProvider;
using DayOfWeek = System.DayOfWeek;
using CoreManager.RouteGroupManager;
using CoreManager.TimingService;
using CoreManager.TransactionManager;
using Point = CoreManager.Models.Point;

namespace CoreManager.AdminManager
{
    public class AdminManager : IAdminManager
    {
        private static string Tag = "AdminManager";
        private readonly ITimingService _timingService;
        private readonly IRouteGroupManager _routeGroupManager;
        private readonly IPricingManager _pricingManager;
        private readonly IResponseProvider _responseProvider;
        private readonly ILogProvider _logmanager;
        private readonly IGoogleService _gService;
        private readonly INotificationManager _notifManager;
        private readonly ITransactionManager _transactionManager;

        #region Constructor
        public AdminManager()
        {
        }
        public AdminManager(IRouteGroupManager routeGroupManager,
                            ITimingService timingService,
                            IPricingManager pricingManager,
                            IResponseProvider responseProvider,
                            INotificationManager notifManager,
                            ITransactionManager transactionManager,
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
            _gService = new GoogleService();
        }
        #endregion

        #region Public Methods
        public List<ClusterListModel> GetAllClusters()
        {
            var modellist = new List<ClusterListModel>();
            using (var dataModel = new MibarimEntities())
            {
                /*var clusters =
                    dataModel.Path_Cluster.ToList();
                foreach (var pathCluster in clusters)
                {
                    if (!modellist.Any(x => x.ClusterId == pathCluster.cluster))
                    {
                        var cc = new ClusterListModel();
                        cc.ClusterId = pathCluster.cluster;
                        modellist.Add(cc);
                    }
                }*/
            }
            return modellist;
        }

        public List<ClusterModel> GetCluster(long clusterId)
        {
            var localRouteModel = new ClusterModel();
            var res = new List<ClusterModel>();
            using (var dataModel = new MibarimEntities())
            {
                /*var paths = dataModel.Path_Cluster.Where(x => x.cluster == clusterId).ToList();
                foreach (var pathCluster in paths)
                {
                    var thePath = dataModel.RouteRequestGPaths.FirstOrDefault(x => x.RoutePathId == pathCluster.pathId);
                    var routes =
                        dataModel.RouteRequests.Where(x => x.RouteRequestUId == thePath.RouteRequestUId).ToList();
                    var theRoute = routes.FirstOrDefault();
                    var user = dataModel.vwUserInfoes.FirstOrDefault(x => x.UserId == theRoute.RouteRequestUserId);
                    var timings = _timingService.GetRequestTimings(routes.Select(x => x.RouteRequestId).ToList()).ToList();
                    string timing = _timingService.GetTimingString(timings);
                    localRouteModel = new ClusterModel();
                    localRouteModel.NameFamily = user.Name + " " + user.Family;
                    localRouteModel.RouteStartTime = timing;
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

                    var routePaths = dataModel.RouteRequestGRoutes.Where(x => x.RoutePathId == thePath.RoutePathId).ToList();
                    localRouteModel.PathRoute.path = AdminMapper.CastRouteToPathRoute(routePaths);
                    res.Add(localRouteModel);

                }*/


            }
            return res;
        }

        #endregion

        #region Private Methods

        #endregion

    }
}
