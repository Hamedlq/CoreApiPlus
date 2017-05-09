using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreManager.Resources;

namespace CoreManager.Models
{
    public class AppointmentModel
    {
        public int GroupId { set; get; }
        public int RouteId { set; get; }
        [Required(ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "Required")]
        [Display(Name = "GAppointGeo", ResourceType = typeof(Strings))]
        public string GAppointLatitude { set; get; }
        public string GAppointLongitude { set; get; }
        [Required(ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "Required")]
        [Display(Name = "GAppointAddress", ResourceType = typeof(Strings))]
        public string GAppointAddress { set; get; }
        [Required(ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "Required")]
        [Display(Name = "AppointTime", ResourceType = typeof(Strings))]
        public DateTime AppointTime { set; get; }
        public string AppointTimeString { set; get; }
        [Required(ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "Required")]
        [Display(Name = "ConfirmedPrice", ResourceType = typeof(Strings))]
        public decimal? ConfirmedPrice { set; get; }
        public string ConfirmedPriceString { set; get; }
        public string ConfirmedPriceMessage { set; get; }
    }

}
