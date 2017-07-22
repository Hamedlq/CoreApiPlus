using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreManager.Models
{
    public class PasargadResponseModel
    {
        public bool Result { set; get; }
        public string TraceNumber { set; get; }
        public string ReferenceNumber { set; get; }
        public string ResultMessage { set; get; }
    }
}
