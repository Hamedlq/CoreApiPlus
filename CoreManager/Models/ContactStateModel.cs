using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreManager.Models
{
    public class ContactStateModel
    {
        public int ContactId { set; get; }
        public bool State { set; get; }
        public string Msg { set; get; }
    }
}
