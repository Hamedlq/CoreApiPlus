using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreManager.Models
{
    public class TmTokensModel
    {
        private string _snappToken;
        private string _tap30Token;
        private string _carpinoToken;
        public string SnappToken { set; get; }
        public int SnappTokenStatus { set; get; }
        public string Tap30Token { set; get; }
        public int Tap30TokenStatus { set; get; }
        public string CarpinoToken { set; get; }
        public string CarpinoRefreshToken { set; get; }
        public int CarpinoTokenStatus { set; get; }
    }
}