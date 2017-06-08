using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreManager.Models
{
    public class FanapModel
    {
        public string Code { set; get; }
        public string State { set; get; }
        public Guid StateGUid {
            get
            {
                Guid uguid;
                Guid.TryParse(this.State, out uguid);
                return uguid;
            }
        }

    }
}
