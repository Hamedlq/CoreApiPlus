using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreExternalService.Models
{
    public class FanapBusiness
    {
        public FanapBusinessResult Result { get; set; }
        public string Ott { get; set; }
    }

    public class FanapBusinessResult
    {
        public int Id { get; set; }
        public List<string> Guilds { get; set; }
    }

}
