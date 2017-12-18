using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreManager.Models
{
    public class ContactPassengersModel
    {
        public string ImageId { set; get; }
        public string PassengerName { set; get; }
        public int PayingMethod { set; get; }
        public string Fare { set; get; }
        public string PassengerMobile { set; get; }
        public string Gender { set; get; }
    }
}
