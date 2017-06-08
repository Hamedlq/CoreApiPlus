using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreManager.Resources;

namespace CoreManager.Models
{
    public class UserInfoModel
    {
        [StringLength(300, ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "MaxLength")]
        [Required(ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "Required")]
        [Display(Name = "Name", ResourceType = typeof(Strings))]
        public string Name { set; get; }
        [StringLength(300, ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "MaxLength")]
        [Required(ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "Required")]
        [Display(Name = "Family", ResourceType = typeof(Strings))]
        public string Family { set; get; }
        [Required(ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "Required")]
        [Display(Name = "Gender", ResourceType = typeof(Strings))]
        public Gender Gender { set; get; }
        [Display(Name = "Email", ResourceType = typeof(Strings))]
        public string Email { set; get; }
        [StringLength(15, ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "MaxLength")]
        [Display(Name = "NationalCode", ResourceType = typeof(Strings))]
        public string NationalCode { set; get; }
        [StringLength(50, ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "MaxLength")]
        [Display(Name = "CarType", ResourceType = typeof(Strings))]
        public string CarType { set; get; }
        [StringLength(40, ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "MaxLength")]
        [Display(Name = "CarPlate", ResourceType = typeof(Strings))]
        public string CarPlateNo { set; get; }
        [StringLength(50, ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "MaxLength")]
        [Display(Name = "CarColor", ResourceType = typeof(Strings))]
        public string CarColor { set; get; }
        [StringLength(50, ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "MaxLength")]
        [Display(Name = "BankName", ResourceType = typeof(Strings))]
        public string BankName { set; get; }
        [StringLength(50, ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "MaxLength")]
        [Display(Name = "BankAccountNo", ResourceType = typeof(Strings))]
        public string BankAccountNo { set; get; }
        [StringLength(50, ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "MaxLength")]
        public string BankShaba { set; get; }
        [StringLength(50, ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "MaxLength")]
        public string CompanyName { set; get; }
        [StringLength(50, ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "MaxLength")]
        public string Code { set; get; }

        public Guid? UserImageId;
        public Guid? NationalCardImageId;
        public Guid? LicenseImageId;
        public Guid? CarCardImageId;
        public Guid? CarCardBckImageId;
        public Guid? BankImageId;
        public Guid? CompanyImageId;

        public bool IsUserRegistered { set; get; }
        public int UserId { set; get; }

    }
}
