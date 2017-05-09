using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreManager.Resources;

namespace CoreManager.Models
{
    public class RouteRequestModel : TimingModel, IValidatableObject
    {
        public int RouteRequestId { set; get; }
        [StringLength(200, ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "MaxLength")]
        [Display(Name = "SrcGAddress", ResourceType = typeof(Strings))]
        public string SrcGAddress { set; get; }
        [StringLength(200, ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "MaxLength")]
        [Display(Name = "SrcDetailAddress", ResourceType = typeof(Strings))]
        public string SrcDetailAddress { set; get; }
        public string SrcLatitude { set; get; }
        public string SrcLongitude { set; get; }
        [StringLength(200, ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "MaxLength")]
        [Display(Name = "DstGAddress", ResourceType = typeof(Strings))]
        public string DstGAddress { set; get; }
        [StringLength(200, ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "MaxLength")]
        [Display(Name = "DstDetailAddress", ResourceType = typeof(Strings))]
        public string DstDetailAddress { set; get; }
        public string DstLatitude { set; get; }
        public string DstLongitude { set; get; }
//        [Required(ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "Required")]
//        [Display(Name = "AccompanyCount", ResourceType = typeof(Strings))]
//        [Range(0,2, ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "MiximumAccompany")]
        public int AccompanyCount { set; get; }
        [Required(ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "Required")]
        [Display(Name = "IsDrive", ResourceType = typeof(Strings))]
        public bool IsDrive { set; get; }
//        [Required(ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "Required")]
//        [Display(Name = "PriceOption", ResourceType = typeof(Strings))]
        public PricingOptions PriceOption { set; get; }
        public decimal CostMinMax { set; get; }
        public long RecommendPathId { set; get; }
        public ServiceTypes? ServiceType { set; get; }
        public int RouteRequestState { set; get; }
        public Guid? RouteUId { set; get; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(SrcLatitude) || string.IsNullOrWhiteSpace(SrcLongitude))
            {
                yield return new ValidationResult(string.Format(getResource.getMessage("Required"),getResource.getString("SrcLatitude")));
            }

            if (string.IsNullOrWhiteSpace(DstLatitude) || string.IsNullOrWhiteSpace(DstLongitude))
            {
                yield return new ValidationResult(string.Format(getResource.getMessage("Required"), getResource.getString("DstLatitude")));
            }
            if (AccompanyCount < 0 || AccompanyCount >2)
            {
                yield return new ValidationResult(string.Format(getResource.getMessage("MiximumAccompany"), 2));
            }
            if (PriceOption == 0)
            {
                yield return new ValidationResult(string.Format(getResource.getMessage("Required"), getResource.getString("PriceOption")));
            }
            if (PriceOption==PricingOptions.MinMax && !IsDrive && (CostMinMax ==null || CostMinMax==0))
                yield return new ValidationResult(getResource.getMessage("PaymentValueNotValid"));

            //timingmodel validation repeated because it can't be called from child implicitly- it sucks

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
                    if (TheDate.Date < DateTime.Now.Date)
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
