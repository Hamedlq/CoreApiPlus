using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreManager.Resources;

namespace CoreManager.Models
{
    public class UserModel
    {
        public string Name { set; get; }
        public string Family { set; get; }
        public string UserName { set; get; }
        public Gender Gender { set; get; }
        public string Email { set; get; }
        public int UserId { set; get; }

    }
}
