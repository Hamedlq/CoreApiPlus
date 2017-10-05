using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreManager.Models
{
    public class RatingModel
    {
        public long RateId { set; get; }
        public Guid UserUId { set; get; }
        public string Name { set; get; }
        public string Family { set; get; }
        public string RateDescription { set; get; }
        public int Rate { set; get; }
        public int Presence { set; get; } // 0 not present and 1 present obviously!
        public string ImageId { set; get; }
        public string RatingsList { set; get; }
    }
}
