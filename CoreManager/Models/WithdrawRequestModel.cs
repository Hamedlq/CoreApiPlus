using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreManager.Models
{
    public class WithdrawRequestModel
    {
        public string WithdrawAmount { set; get; }
        public string WithdrawDate { set; get; }
        public string WithdrawStateString { set; get; }
        public WithdrawStates WithdrawState { set; get; }
    }
}
