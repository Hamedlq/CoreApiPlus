using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreManager.Models
{
    public class TripRouteModel
    {
        public string UserName { set; get; }
        public string UserFamily { set; get; }
        public string UserMobile { set; get; }
        public string Lat { set; get; }
        public string Lng { set; get; }
        public decimal PayPrice { set; get; }
        public bool IsDriver { set; get; }
        public bool IsMe { set; get; }
        public Guid? UserImageId { set; get; }
    }
}
