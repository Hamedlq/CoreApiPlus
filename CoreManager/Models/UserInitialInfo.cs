using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreManager.Resources;

namespace CoreManager.Models
{
    public class UserInitialInfo
    {
        public bool IsUserRegistered { set; get; }
        public int ChatCount { set; get; }
        public int RouteSuggestCount { set; get; }
    }
}
