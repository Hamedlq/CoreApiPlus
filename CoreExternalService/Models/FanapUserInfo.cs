using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreExternalService.Models
{
    public class FanapUserInfo
    {
        public string cellphoneNumber { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string name { get; set; }
        public string nickName { get; set; }
        public string birthDate { get; set; }
        public int score { get; set; }
        public int followingCount { get; set; }
        public string joinDate { get; set; }
        public int userId { get; set; }
        public bool guest { get; set; }
        public string email { get; set; }
    }

}
