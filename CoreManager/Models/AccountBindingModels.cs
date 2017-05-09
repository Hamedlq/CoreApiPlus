using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using CoreManager.Resources;

namespace CoreManager.Models
{
    // Models used as parameters to AccountController actions.

    public class AddExternalLoginBindingModel
    {
        [Required]
        [Display(Name = "External access token")]
        public string ExternalAccessToken { get; set; }
    }

    public class ChangePasswordBindingModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    //public class RegisterBindingModel
    //{
    //    [Required]
    //    [Display(Name = "Email")]
    //    public string Email { get; set; }

    //    [Required]
    //    [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
    //    [DataType(DataType.Password)]
    //    [Display(Name = "Password")]
    //    public string Password { get; set; }

    //    [DataType(DataType.Password)]
    //    [Display(Name = "Confirm password")]
    //    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    //    public string ConfirmPassword { get; set; }
    //}
    public abstract class RegisterModel
    {
        [Display(Name = "Name", ResourceType = typeof(Strings))]
        public virtual string Name { get; set; }
        [Display(Name = "Family", ResourceType = typeof(Strings))]
        public virtual string Family { get; set; }
        //[Display(Name = "Email", ResourceType = typeof(Strings))]
        //public virtual string Email { get; set; }
        [Display(Name = "Mobile", ResourceType = typeof(Strings))]
        //[Required(ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "Required")]
        [StringLength(11, MinimumLength = 11, ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "MobileLength")]
        [RegularExpression("09[0-9]*", ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "MobilePattern")]
        public virtual string Mobile { get; set; }
        [Display(Name = "Gender", ResourceType = typeof(Strings))]
        public virtual Gender Gender { get; set; }
        [Display(Name = "Password", ResourceType = typeof(Strings))]
        //[Required(ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "Required")]
        [StringLength(20, MinimumLength = 4, ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "PassLength")]
        [DataType(DataType.Password)]
        public virtual string Password { get; set; }
        [Display(Name = "Code", ResourceType = typeof(Strings))]
        public virtual string Code { get; set; }
        //role of user
        public UserRoles UserRole { get; set; }

    }

    public class OtploginModel : RegisterModel
    {
        [Required(ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "Required")]
        public override string Mobile
        {
            get { return base.Mobile; }
            set { base.Mobile = value; }
        }
    }

    public class UserRegisterModel : RegisterModel
    {
        [Required(ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "Required")]
        public override string Name
        {
            get { return base.Name; }
            set { base.Name = value; }
        }
        [Required(ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "Required")]
        public override string Family
        {
            get { return base.Family; }
            set { base.Family = value; }
        }
        //[Required(ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "Required")]
        //public override string Email {
        //    get { return base.Email; }
        //    set { base.Email = value; }
        //}
        [Required(ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "Required")]
        public override Gender Gender
        {
            get { return base.Gender; }
            set { base.Gender = value; }

        }
    }

    public class UserChangePassModel : RegisterModel
    {
    }

    public class UserSearchModel 
    {
        public  string Mobile { get; set; }
        public string Name { get; set; }
        public string Family { get; set; }
        public Gender Gender{get; set;}
    }

    public class AgencyDriverRegisterModel : RegisterModel
    {
        [Display(Name = "LicenseNo", ResourceType = typeof(Strings))]
        public string LicenseNo { set; get; }
        [Display(Name = "NationalCode", ResourceType = typeof(Strings))]
        public string NationalCode { set; get; }
        [Display(Name = "CarType", ResourceType = typeof(Strings))]
        public string CarType { set; get; }
        [Display(Name = "CarPlate", ResourceType = typeof(Strings))]
        public string CarPlate { set; get; }
        [Display(Name = "CarColor", ResourceType = typeof(Strings))]
        public string CarColor { set; get; }
        [Display(Name = "BankName", ResourceType = typeof(Strings))]
        public string BankName { set; get; }
        [Display(Name = "BankAccount", ResourceType = typeof(Strings))]
        public string BankAccount { set; get; }
        [Display(Name = "BankCardNo", ResourceType = typeof(Strings))]
        public string BankCardNo { set; get; }

    }

    public class RegisterExternalBindingModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class RemoveLoginBindingModel
    {
        [Required]
        [Display(Name = "Login provider")]
        public string LoginProvider { get; set; }

        [Required]
        [Display(Name = "Provider key")]
        public string ProviderKey { get; set; }
    }

    public class SetPasswordBindingModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
