using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreManager.Resources;

namespace CoreManager.Models
{
    public class ImageDescription
    {
       public DocState State { set; get; }
        public string RejectionDescription { set; get; }
    }
}
