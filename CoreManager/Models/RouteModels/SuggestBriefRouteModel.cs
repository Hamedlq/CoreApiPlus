using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreManager.Models
{
    public class SuggestBriefRouteModel
    {
        public SuggestBriefRouteModel()
        {
            SelfRouteModel=new BriefRouteModel();
            SuggestRouteModel=new BriefRouteModel();
        }

        public BriefRouteModel SelfRouteModel { set; get; }
        public BriefRouteModel SuggestRouteModel { set; get; }
    }
}
