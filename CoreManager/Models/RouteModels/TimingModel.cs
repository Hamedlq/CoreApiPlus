using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreManager.Resources;

namespace CoreManager.Models
{
    public class TimingModel : IValidatableObject
    {
//        [Required(ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "Required")]
//        [Display(Name = "TimingOption", ResourceType = typeof(Strings))]
        public TimingOptions TimingOption { set; get; }
        public DateTime TheTime { set; get; }
        public DateTime TheDate { set; get; }
        public DateTime SatDatetime { set; get; }
        public DateTime SunDatetime { set; get; }
        public DateTime MonDatetime { set; get; }
        public DateTime TueDatetime { set; get; }
        public DateTime WedDatetime { set; get; }
        public DateTime ThuDatetime { set; get; }
        public DateTime FriDatetime { set; get; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (TimingOption == 0)
            {
                yield return new ValidationResult(string.Format(getResource.getMessage("Required"), getResource.getString("TimingOption")));
            }
            switch (TimingOption)
            {
                case TimingOptions.Today:
                    if (TheTime == null || TheTime == DateTime.MinValue)
                        yield return new ValidationResult(getResource.getMessage("TimeNotValid"));
                    break;
                case TimingOptions.InDateAndTime:
                    if ((TheTime == null || TheTime == DateTime.MinValue) || (TheDate == null || TheDate == DateTime.MinValue))
                        yield return new ValidationResult(getResource.getMessage("TimeNotValid"));
                    if (TheDate.Date <DateTime.Now.Date)
                        yield return new ValidationResult(getResource.getMessage("TimePassed"));
                    break;
                case TimingOptions.Weekly:
                    if ((SatDatetime == null || SatDatetime == DateTime.MinValue)
                        && (SunDatetime == null || SunDatetime == DateTime.MinValue)
                        && (MonDatetime == null || MonDatetime == DateTime.MinValue)
                        && (TueDatetime == null || TueDatetime == DateTime.MinValue)
                        && (ThuDatetime == null || ThuDatetime == DateTime.MinValue)
                        && (WedDatetime == null || WedDatetime == DateTime.MinValue)
                        && (FriDatetime == null || FriDatetime == DateTime.MinValue)
                        )
                        yield return new ValidationResult(getResource.getMessage("TimeNotValid"));
                    break;
            }
        }

    }
}
