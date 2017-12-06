using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreManager.Models
{
    public class GasRank
    {
        public long GasRankId { set; get; }
        public int Rank { set; get; }
        public string Name { set; get; }
        public string Family { set; get; }
        public string Payment { set; get; }
        public string Payed { set; get; }
    }
}
