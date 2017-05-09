using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreManager.Resources;

namespace CoreManager.Models
{
    public class ConfirmationModel : IValidatableObject
    {
        [Required(ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "Required")]
        [Display(Name = "RouteId", ResourceType = typeof(Strings))]
        public string RouteIdsCommaSeprated { set; get; }

        [Required(ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "Required")]
        [Display(Name = "ConfirmedText", ResourceType = typeof(Strings))]
        public string ConfirmedText { set; get; }
        public List<int> RouteIds
        {
            get
            {
                var routeIds = new List<int>();
                foreach (var s in RouteIdsCommaSeprated.Split(','))
                {
                    int num;
                    if (int.TryParse(s, out num))
                        routeIds.Add(num);
                }
                return routeIds;
            }
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var routeManager = new RouteManager.RouteManager();
            if (!routeManager.CheckConfirmationText(RouteIds.FirstOrDefault(), ConfirmedText))
                yield return new ValidationResult(getResource.getMessage("ConfirmationDenied"));

        }
    }
}
