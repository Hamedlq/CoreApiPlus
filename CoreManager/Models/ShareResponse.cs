using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreManager.Models
{
    public class ShareResponse
    {
        public string ImageId { set; get; }
        public string ImagePath { set; get; }
        //public byte[] ImageFile{ set; get; }
        public string ImageCaption { set; get; }
    }
}
