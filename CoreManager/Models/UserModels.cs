using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using CoreManager.Resources;

namespace CoreManager.Models
{
    public class PersoanlInfoModel
    {
        [StringLength(300, ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "MaxLength")]
        [Required(ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "Required")]
        [Display(Name = "Name", ResourceType = typeof(Strings))]
        public string Name { set; get; }
        [StringLength(300, ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "MaxLength")]
        [Required(ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "Required")]
        [Display(Name = "Family", ResourceType = typeof(Strings))]
        public string Family { set; get; }
        //[Required(ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "Required")]
        [Display(Name = "Gender", ResourceType = typeof(Strings))]
        public Gender Gender { set; get; }
        [Display(Name = "Email", ResourceType = typeof(Strings))]
        public string Email { set; get; }
        [StringLength(15, ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "MaxLength")]
        //[Required(ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "Required")]
        [Display(Name = "NationalCode", ResourceType = typeof(Strings))]
        public string NationalCode { set; get; }
        //[Required(ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "Required")]
        //[Display(Name = "UserPic", ResourceType = typeof(Strings))]
        public byte[] UserPic { set; get; }
        public Guid? UserImageId { set; get; }
        public string Mobile { set; get; }
        [StringLength(50, ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "MaxLength")]
        public string Code { set; get; }

    }
    public class LicenseInfoModel
    {
        [StringLength(50, ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "MaxLength")]
        [Required(ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "Required")]
        [Display(Name = "LicenseNo", ResourceType = typeof(Strings))]
        public string LicenseNo { set; get; }
        public Guid? LicenseImageId { set; get; }
        public byte[] LicensePic { set; get; }
    }

    public class CarInfoModel
    {
        public int? CarInfoId { set; get; }
        [StringLength(50, ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "MaxLength")]
        [Required(ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "Required")]
        [Display(Name = "CarType", ResourceType = typeof(Strings))]
        public string CarType { set; get; }
        [StringLength(40, ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "MaxLength")]
        //[Required(ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "Required")]
        [Display(Name = "CarPlate", ResourceType = typeof(Strings))]
        public string CarPlateNo { set; get; }
        [StringLength(50, ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "MaxLength")]
        //[Required(ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "Required")]
        [Display(Name = "CarColor", ResourceType = typeof(Strings))]
        public string CarColor { set; get; }
        public byte[] CarCardPic { set; get; }
        public Guid? CarBackImageId { set; get; }
        public byte[] CarCardBkPic { set; get; }
        public Guid? CarFrontImageId { set; get; }
    }
    public class BankInfoModel
    {
        [StringLength(50, ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "MaxLength")]
        [Required(ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "Required")]
        [Display(Name = "BankName", ResourceType = typeof(Strings))]
        public string BankName { set; get; }
        [StringLength(50, ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "MaxLength")]
        [Required(ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "Required")]
        [Display(Name = "BankAccountNo", ResourceType = typeof(Strings))]
        public string BankAccountNo { set; get; }
        [StringLength(50, ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "MaxLength")]
        public string BankShaba { set; get; }
        public Guid BankCardImageId { set; get; }
        public byte[] BankCardPic { set; get; }
    }

    public class MobileValidation
    {
        public string Mobile { set; get; }
        public string ValidationCode { set; get; }
        public int SendCounter { set; get; }
        public string MobileBrief()
        {
            if (!string.IsNullOrEmpty(Mobile))
                return Mobile.Substring(1);
            return string.Empty;
        }
    }
    public class UserInfoRequest
    {
        public string Mobile { set; get; }
    }

}
