using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreDA;
using CoreManager.Models;
using CoreManager.Models.RouteModels;

namespace CoreManager.RouteGroupManager
{
    public interface IRouteGroupManager
    {
        bool AddRouteGroup(int userId, RouteGroupModel model, int suggestAccompanyCount);
        List<RouteGroupModel> GetRouteGroup(int routeRequestId);
        List<long> GetExcludeRouteRequestIds(int routeRequestId);
        List<GroupModel> GetSuggestedGroups(int routeRequestId);
    }
}
