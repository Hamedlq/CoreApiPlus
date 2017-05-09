using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreManager.Models.RouteModels
{
    public class RouteGroupModel:BriefRouteModel
    {
        public long RgHolderRrId { set; get; }
        public int GroupId { set; get; }
        public bool RgIsConfimed { set; get; }
    }
}
