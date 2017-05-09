using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreManager.Models.RouteModels
{
    public class GroupModel
    {
        public int GroupId { set; get; }
        public List<RouteGroupModel> GroupMembers { set; get; }
    }
}
