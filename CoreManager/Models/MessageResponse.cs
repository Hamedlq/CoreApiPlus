using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreManager.Models
{
    public class MessageResponse
    {
        public string Title { set; get; }
        public string Message { set; get; }
        public ResponseTypes Type { set; get; }
    }
}
