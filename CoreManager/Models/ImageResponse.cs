using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreManager.Models
{
    public class ImageResponse
    {
        public string ImageId { set; get; }
        public string ImageType { set; get; }
        public byte[] ImageFile{ set; get; }
    }
}
