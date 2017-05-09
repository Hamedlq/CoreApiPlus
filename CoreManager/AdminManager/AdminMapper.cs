using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;
using CoreDA;
using CoreManager.Models;
using AutoMapper;
using CoreManager.Models.RouteModels;
using System.Device.Location;
using CoreExternalService.Models;
using CoreManager.Helper;
using CoreManager.Models.TrafficAddress;
using CoreManager.Resources;
using Point = CoreManager.Models.Point;

namespace CoreManager.AdminManager
{
    public static class AdminMapper
    {
        public static List<Point> CastRouteToPathRoute(List<RouteRequestGRoute> routePaths)
        {
            var res = new List<Point>();
            var point = new Point();

            foreach (var vwPathRoute in routePaths.OrderBy(x => x.RRRSeq))
            {
                point = new Point();
                point.Lat = vwPathRoute.RRRLatitude.ToString("G29");
                point.Lng = vwPathRoute.RRRLongitude.ToString("G29");
                res.Add(point);
            }
            return res;
        }
        public static LocalRouteUserModel CastRouteToRouteUser(RouteRequest theRoute, vwUserInfo user)
        {
            var res = new LocalRouteUserModel();
            res.Name = user.Name;
            res.Family = user.Family;
            res.SrcPoint.Lat = theRoute.SrcLatitude.ToString("G29");
            res.SrcPoint.Lng = theRoute.SrcLongitude.ToString("G29");
            res.DstPoint.Lat = theRoute.DstLatitude.ToString("G29");
            res.DstPoint.Lng = theRoute.DstLongitude.ToString("G29");
            if (theRoute.IsDrive)
            {
                res.LocalRouteType = LocalRouteTypes.Driver;
            }
            else
            {
                res.LocalRouteType = LocalRouteTypes.Passenger;
            }
            res.RouteUId = (Guid)theRoute.RouteRequestUId;
            return res;
        }
    }
}
