using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreExternalService.Models
{
    public class FanapBusiness
    {
        public bool hasError { get; set; }

        public string message { get; set; }
        public FanapBusinessResult result { get; set; }
        public string Ott { get; set; }
    }

    public class FanapBusinessResult
    {
        public int id { get; set; }
        public List<GuildResult> Guilds { get; set; }
    }

    public class GuildResult
    {
        public int id { get; set; }
        public string code { get; set; }
    }

}
