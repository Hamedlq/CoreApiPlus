using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreDA;
using CoreManager.Models;
using CoreManager.Models.TrafficAddress;

namespace CoreManager.AdminManager
{
    public interface IAdminManager
    {
        List<ClusterListModel> GetAllClusters();
        List<ClusterModel> GetCluster(long clusterId);
    }
}
