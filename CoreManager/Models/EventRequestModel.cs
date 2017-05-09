using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreManager.Resources;

namespace CoreManager.Models
{
    public class EventRequestModel : IValidatableObject
    {
        [Required(ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "Required")]
        [StringLength(256, ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "MaxLength")]
        [Display(Name = "Name", ResourceType = typeof(Strings))]
        public string Name { set; get; }

        [Required(ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "Required")]
        [StringLength(256, ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "MaxLength")]
        [Display(Name = "Family", ResourceType = typeof(Strings))]
        public string Family { set; get; }

        [Required(ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "Required")]
        [StringLength(200, ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "MaxLength")]
        [Display(Name = "Mobile", ResourceType = typeof(Strings))]
        public string Mobile { set; get; }

        [Required(ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "Required")]
        [StringLength(256, ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "MaxLength")]
        [Display(Name = "EventName", ResourceType = typeof(Strings))]
        public string EventName { set; get; }

        [Required(ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "Required")]
        [StringLength(256, ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "MaxLength")]
        [Display(Name = "EventLink", ResourceType = typeof(Strings))]
        public string EventLink { set; get; }

        [Required(ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "Required")]
        [Display(Name = "Edate", ResourceType = typeof(Strings))]
        public DateTime Edate { set; get; }

        [Required(ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "Required")]
        [Display(Name = "TimeStart", ResourceType = typeof(Strings))]
        public DateTime TimeStart { set; get; }

        [Required(ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "Required")]
        [Display(Name = "TimeEnd", ResourceType = typeof(Strings))]
        public DateTime TimeEnd { set; get; }

        [Required(ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "Required")]
        [Display(Name = "SrcGAddress", ResourceType = typeof(Strings))]
        public string Latitude { set; get; }

        [Required(ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "Required")]
        [Display(Name = "SrcGAddress", ResourceType = typeof(Strings))]
        public string Longitude { set; get; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(Latitude) || string.IsNullOrWhiteSpace(Longitude))
            {
                yield return new ValidationResult(string.Format(getResource.getMessage("Required"),getResource.getString("SrcLatitude")));
            }

        }
    }

    
}
