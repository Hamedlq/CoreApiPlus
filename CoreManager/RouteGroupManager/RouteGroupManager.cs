using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Spatial;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using CoreManager.Models;
using CoreDA;
using CoreManager.Helper;
using CoreManager.Models.RouteModels;
using CoreManager.PricingManager;
using CoreManager.Resources;
using CoreManager.ResponseProvider;
using CoreManager.RouteManager;
using CoreManager.TimingService;
using DayOfWeek = System.DayOfWeek;

namespace CoreManager.RouteGroupManager
{
    public class RouteGroupManager : IRouteGroupManager
    {
        private readonly ITimingService _timingService;
        private readonly IPricingManager _pricingManager;
        private readonly IResponseProvider _responseProvider;
        #region Constructor
        public RouteGroupManager()
        {
        }

        public RouteGroupManager(ITimingService timingService, IPricingManager pricingManager,IResponseProvider responseProvider)
        {
            _timingService = timingService;
            _pricingManager = pricingManager;
            _responseProvider = responseProvider;
        }
        #endregion

        public int GroupRouteCount(int groupId)
        {
            var groupCount = 0;
            using (var dataModel = new MibarimEntities())
            {
                groupCount = dataModel.vwRouteGroups.Where(x => x.GroupId == groupId).Sum(x=>x.AccompanyCount+1);
            }
            return groupCount;
        }

        public bool AddRouteGroup(int userId, RouteGroupModel model, int suggestAccompanyCount)
        {
            var routegroup = new RouteGroup();
            using (var dataModel = new MibarimEntities())
            {
                //I have a group and add this route to it
                var userGroup = dataModel.vwRouteGroups.FirstOrDefault(x => x.RGHolderRRId == model.RgHolderRrId && x.RouteRequestId==model.RgHolderRrId && x.RGIsConfirmed);
                //I'm part of a group and add this to that group
                if (userGroup == null)
                {
                    userGroup = dataModel.vwRouteGroups.FirstOrDefault(x =>x.UserId==userId && x.RouteRequestId==model.RgHolderRrId &&  x.RGIsConfirmed);
                }
                if (userGroup == null)
                {
                    if (IsAddQuataExceed(userId, model.RgHolderRrId))
                    {
                        _responseProvider.SetBusinessMessage(new MessageResponse() { Type = ResponseTypes.Error, Message = getResource.getMessage("OnlythreeGroupAllowed") });
                        return false;
                    }
                    using (var dbContextTransaction = dataModel.Database.BeginTransaction())
                    {
                        try
                        {
                            var group = new Group()
                            {
                                GCreateTime = DateTime.Now,
                                GIsDeleted = false,
                                GCreatorUser = userId
                            };
                            dataModel.Groups.Add(group);
                            dataModel.SaveChanges();
                            routegroup.RGCreateTime = DateTime.Now;
                            routegroup.GroupId = group.GroupId;
                            routegroup.RGRouteRequestId = model.RouteId;
                            routegroup.RGHolderRRId = model.RgHolderRrId;
                            routegroup.RGIsDeleted = false;
                            routegroup.RGIsConfirmed = false;
                            dataModel.RouteGroups.Add(routegroup);
                            var holderRoutegroup = new RouteGroup();
                            holderRoutegroup.RGCreateTime = DateTime.Now;
                            holderRoutegroup.GroupId = group.GroupId;
                            holderRoutegroup.RGRouteRequestId = model.RgHolderRrId;
                            holderRoutegroup.RGHolderRRId = model.RgHolderRrId;
                            holderRoutegroup.RGIsDeleted = false;
                            holderRoutegroup.RGIsConfirmed = true;
                            dataModel.RouteGroups.Add(holderRoutegroup);
                            dbContextTransaction.Commit();
                        }
                        catch (Exception)
                        {
                            dbContextTransaction.Rollback();
                        }
                    }
                }
                else
                {
                    if (GroupRouteCount(userGroup.GroupId)+ suggestAccompanyCount > 5)
                    {
                        _responseProvider.SetBusinessMessage(new MessageResponse() { Type = ResponseTypes.Error, Message = getResource.getMessage("GroupCapacityExceed") });
                        return false;
                    }
                    if (IsRepeated(userId, model.RgHolderRrId, model.RouteId, userGroup.GroupId))
                    {
                        _responseProvider.SetBusinessMessage(new MessageResponse() { Type = ResponseTypes.Error, Message = getResource.getMessage("RequestRepeated") });
                        return false;
                    }
                    routegroup.RGCreateTime = DateTime.Now;
                    routegroup.GroupId = userGroup.GroupId;
                    routegroup.RGRouteRequestId = model.RouteId;
                    routegroup.RGHolderRRId = model.RgHolderRrId;
                    routegroup.RGIsDeleted = false;
                    routegroup.RGIsConfirmed = false;
                    dataModel.RouteGroups.Add(routegroup);
                }
                dataModel.SaveChanges();
            }
            return true;
        }

        public List<RouteGroupModel> GetRouteGroup(int routeRequestId)
        {
            var groupRoutes = new List<RouteGroupModel>();
            using (var dataModel = new MibarimEntities())
            {
                //var userGroupRoutes = dataModel.vwRouteGroups.Where(x => x.RGHolderRRId == routeRequestId && x.RGIsConfirmed).ToList();
                var userGroupRoutes = new List<vwRouteGroup>();
                var groupRouteNo = dataModel.vwRouteGroups.FirstOrDefault(x => x.RouteRequestId == routeRequestId && x.RGIsConfirmed);
                if (groupRouteNo != null)
                {
                    userGroupRoutes = dataModel.vwRouteGroups.Where(x => x.GroupId == groupRouteNo.GroupId && x.RouteRequestId != routeRequestId).ToList();
                }
                if (userGroupRoutes.Count > 0)
                {
                    var timings = _timingService.GetRequestTimings(userGroupRoutes.Select(x => x.RouteRequestId).ToList());
                    foreach (var vwgroupRoute in userGroupRoutes)
                    {
                        var routeGroupModel = new RouteGroupModel();
                        routeGroupModel.RouteId = vwgroupRoute.RouteRequestId;
                        routeGroupModel.SrcLatitude = vwgroupRoute.SrcLatitude.ToString();
                        routeGroupModel.SrcLongitude = vwgroupRoute.SrcLongitude.ToString();
                        routeGroupModel.DstLatitude = vwgroupRoute.DstLatitude.ToString();
                        routeGroupModel.DstLongitude = vwgroupRoute.DstLongitude.ToString();
                        routeGroupModel.RgHolderRrId = vwgroupRoute.RGHolderRRId;
                        routeGroupModel.GroupId = vwgroupRoute.GroupId;
                        routeGroupModel.Name = vwgroupRoute.Name;
                        routeGroupModel.Family = vwgroupRoute.Family;
                        routeGroupModel.RgIsConfimed = vwgroupRoute.RGIsConfirmed;
                        routeGroupModel.AccompanyCount = vwgroupRoute.AccompanyCount;
                        routeGroupModel.IsDrive = vwgroupRoute.IsDrive;
                        routeGroupModel.CarString = GetCarString(vwgroupRoute);
                        routeGroupModel.PricingString = _pricingManager.GetPriceString(new RouteRequestModel()
                        {
                            PriceOption = (PricingOptions)vwgroupRoute.RRPricingOption,
                            CostMinMax = (decimal)vwgroupRoute.RRPricingMinMax,
                            IsDrive = (bool)vwgroupRoute.IsDrive
                        });
                        routeGroupModel.TimingString = _timingService.GetTimingString(timings.Where(y => y.RouteRequestId == vwgroupRoute.RouteRequestId).ToList());
                        groupRoutes.Add(routeGroupModel);
                    }
                }
            }
            return groupRoutes;
        }

        private string GetCarString(vwRouteGroup vwgroupRoute)
        {
            var carString = "";
            if (vwgroupRoute.CarPlateNo == null)
            {
                carString = vwgroupRoute.CarType + vwgroupRoute.CarColor;
            }
            else
            {
                carString = string.Format(getResource.getMessage("CarInfoStr"), vwgroupRoute.CarType,
                    vwgroupRoute.CarColor,
                    vwgroupRoute.CarPlateNo);
            }
            return carString;
        }

        public List<long> GetExcludeRouteRequestIds(int routeRequestId)
        {
            var groupRouteRequestIds = new List<long>();
            using (var dataModel = new MibarimEntities())
            {

                var groupRouteNo = dataModel.RouteGroups.Where(x => x.RGRouteRequestId== routeRequestId && !x.RGIsDeleted).Select(x=>x.GroupId).ToList();
                if (groupRouteNo.Count>0)
                {
                    groupRouteRequestIds = dataModel.vwRouteGroups.Where(y => groupRouteNo.Contains(y.GroupId)).Select(u => u.RouteRequestId).ToList();
                }
            }
            return groupRouteRequestIds;
        }

        public List<GroupModel> GetSuggestedGroups(int routeRequestId)
        {
            var suggestedGroups = new List<GroupModel>();
            using (var dataModel = new MibarimEntities())
            {
                var groupRoutes = new List<vwRouteGroup>();
                var groupRouteNo = dataModel.vwRouteGroups.Where(x => x.RouteRequestId == routeRequestId && !x.RGIsConfirmed).Select(y => y.GroupId).ToList();
                if (groupRouteNo.Count > 0)
                {
                    groupRoutes = dataModel.vwRouteGroups.Where(x => groupRouteNo.Contains(x.GroupId) && x.RouteRequestId != routeRequestId).ToList();
                }
                if (groupRoutes.Count > 0)
                {
                    var timings = _timingService.GetRequestTimings(groupRoutes.Select(x => x.RouteRequestId).ToList());
                    var groups = groupRoutes.GroupBy(p => p.GroupId,
                               (key, g) => new
                               {
                                   RouteGroupId = key,
                                   requests = g.ToList()
                               });
                    foreach (var group in groups)
                    {
                        var groupModel = new GroupModel();
                        groupModel.GroupId = group.RouteGroupId;
                        var routeGrouplist = new List<RouteGroupModel>();
                        foreach (var request in group.requests)
                        {
                            var routeGroupModel = new RouteGroupModel();
                            routeGroupModel.RouteId = request.RouteRequestId;
                            routeGroupModel.RgHolderRrId = request.RGHolderRRId;
                            routeGroupModel.SrcLatitude = request.SrcLatitude.ToString();
                            routeGroupModel.SrcLongitude = request.SrcLongitude.ToString();
                            routeGroupModel.DstLatitude = request.DstLatitude.ToString();
                            routeGroupModel.DstLongitude = request.DstLongitude.ToString();
                            routeGroupModel.DstLongitude = request.DstLongitude.ToString();
                            routeGroupModel.RgIsConfimed = request.RGIsConfirmed;
                            routeGroupModel.IsDrive= request.IsDrive;
                            routeGroupModel.AccompanyCount = request.AccompanyCount;
                            routeGroupModel.Name = request.Name;
                            routeGroupModel.Family = request.Family;
                            routeGroupModel.PricingString = _pricingManager.GetPriceString(new RouteRequestModel()
                            {
                                PriceOption = (PricingOptions)request.RRPricingOption,
                                CostMinMax = (decimal)request.RRPricingMinMax,
                                IsDrive = (bool)request.IsDrive
                            });
                            routeGroupModel.CarString = GetCarString(request);
                            routeGroupModel.TimingString = _timingService.GetTimingString(timings.Where(y => y.RouteRequestId == request.RouteRequestId).ToList());
                            routeGrouplist.Add(routeGroupModel);
                        }
                        groupModel.GroupMembers = routeGrouplist;
                        suggestedGroups.Add(groupModel);
                    }
                }
            }
            return suggestedGroups;
        }
        private bool IsAddQuataExceed(int userId,long rGHolderRRId)
        {
            using (var dataModel = new MibarimEntities())
            {
                var groupMade = dataModel.vwRouteGroups.Where(x => x.RGHolderRRId == rGHolderRRId).GroupBy(x => x.GroupId).ToList();
                if (groupMade.Count > 3)
                {
                    return true;
                }
                return false;
            }
        }

        private bool IsRepeated(int userId, long rgHolderRrId, long routeId, int groupId)
        {
            using (var dataModel = new MibarimEntities())
            {
                if (dataModel.vwRouteGroups.Any(x => x.RGHolderRRId == rgHolderRrId && x.RouteRequestId == routeId && x.GroupId==groupId && !x.RRIsDeleted))
                {
                    return true;
                }
                return false;
            }
        }


        public bool IsGroupCapacityExceed(int groupId, int accompanyCount)
        {
            using (var dataModel = new MibarimEntities())
            {
                var groupCount = dataModel.vwRouteGroups.Where(x => x.GroupId == groupId).Sum(x => x.AccompanyCount + 1);
                if (groupCount + accompanyCount > 5)
                {
                    return true;
                }
                return false;
            }

        }

    }
}
