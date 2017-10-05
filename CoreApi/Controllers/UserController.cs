using CoreManager.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using CoreApi.Providers;
using CoreManager.LogProvider;
using CoreManager.Resources;
using CoreManager.ResponseProvider;
using CoreManager.UserManager;
using Newtonsoft.Json;

namespace CoreApi.Controllers
{
    public class UserController : ApiController
    {
        private ApplicationUserManager _userAppManager;
        private IUserManager _userManager;
        private ILogProvider _logManager;
        private IResponseProvider _responseProvider;
        private static string Tag = "UserController";
        private string _appVersion = "1";

        public UserController(ApplicationUserManager appUserManager,
            ISecureDataFormat<AuthenticationTicket> accessTokenFormat)
        {
            AppUserManager = appUserManager;
            AccessTokenFormat = accessTokenFormat;
        }

        public UserController(IUserManager userManager, ILogProvider logManager, IResponseProvider responseProvider)
        {
            _userManager = userManager;
            _logManager = logManager;
            _responseProvider = responseProvider;
        }

        public UserController()
        {
        }

        public ApplicationUserManager AppUserManager
        {
            get { return _userAppManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
            private set { _userAppManager = value; }
        }

        public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; private set; }


        // Post api/User/RegisterTaxiAgencyAdmin
        //[AllowAnonymous]
        //[Route("RegisterTaxiAgencyAdmin")]
        //public async Task<IHttpActionResult> RegisterTaxiAgencyAdmin(UserRegisterModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }
        //    model.UserRole = UserRoles.TaxiAgencyAdmin;
        //    var result= await RegisterUser(model);
        //    return result;
        //}

        //[AllowAnonymous]
        //[Route("RegisterAdminApplication")]
        //public async Task<IHttpActionResult> RegisterAdminApplication(UserRegisterModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }
        //    model.UserRole = UserRoles.AdminApplication;
        //    var result = await RegisterUser(model);
        //    return result;
        //}

        //// Post api/User/RegisterTaxiAgencyAdmin
        //[AllowAnonymous]
        //[Route("RegisterTaxiAgencyDriver")]
        //public async Task<IHttpActionResult> RegisterTaxiAgencyDriver(AgencyDriverRegisterModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }
        //    model.UserRole = UserRoles.TaxiAgencyDriver;
        //    var result = await RegisterUser(model);
        //    if (result == Ok())
        //    {

        //    }
        //    return result;
        //}

        [AllowAnonymous]
        [Route("ContactUs")]
        public IHttpActionResult ContactUs(ContactUsModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(_responseProvider.GenerateBadRequestResponse(ModelState));
            }
            _userManager.SubmitContactUs(model);
            return Json(_responseProvider.GenerateOKResponse());
        }

        [AllowAnonymous]
        [Route("RegisterWebUser")]
        public IHttpActionResult RegisterWebUser(UserRegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(_responseProvider.GenerateBadRequestResponse(ModelState));
                //return BadRequest(ModelState);
            }
            model.UserRole = UserRoles.WebUser;
            var result = RegisterUser(model);
            return result;
        }

        [AllowAnonymous]
        [Route("RegisterMobileUser")]
        public IHttpActionResult RegisterMobileUser(OtploginModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(_responseProvider.GenerateBadRequestResponse(ModelState));
                //return BadRequest(ModelState);
            }
            model.UserRole = UserRoles.MobileUser;
            var result = OtploginUser(model);
            return result;
        }

        [AllowAnonymous]
        [Route("RegisterIosUser")]
        public IHttpActionResult RegisterIosUser(OtploginModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(_responseProvider.GenerateBadRequestResponse(ModelState));
                //return BadRequest(ModelState);
            }
            model.UserRole = UserRoles.IosMobileUser;
            var result = OtploginUser(model);
            return result;
        }

        [AllowAnonymous]
        [Route("RegisterIosDriver")]
        public IHttpActionResult RegisterIosDriver(OtploginModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(_responseProvider.GenerateBadRequestResponse(ModelState));
                //return BadRequest(ModelState);
            }
            model.UserRole = UserRoles.IosMobileDriver;
            var result = OtploginUser(model);
            return result;
        }

        [AllowAnonymous]
        [Route("RegisterMobileDriver")]
        public IHttpActionResult RegisterMobileDriver(OtploginModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(_responseProvider.GenerateBadRequestResponse(ModelState));
                //return BadRequest(ModelState);
            }
            model.UserRole = UserRoles.MobileDriver;
            var result = OtploginUser(model);
            return result;
        }

        private IHttpActionResult OtploginUser(OtploginModel model)
        {
            IdentityResult result;
            var userObj = new ApplicationUser() {UserName = model.Mobile, MobileConfirmed = false};
            //var userObj = new ApplicationUser() { Name = model.Name, Family = model.Family, Gender = model.Gender, UserName = model.Mobile,Code=model.Code, MobileConfirmed = false };
            var user = AppUserManager.FindByName(model.Mobile);
            if (user == null)
            {
                //new User
                model.Password = "a2sjKBMusxt0BjM7eBvZ"; //model.Password ?? "1234";
                result = AppUserManager.Create(userObj, model.Password);
                if (result.Succeeded)
                {
                    user = AppUserManager.FindByName(model.Mobile);
                    AppUserManager.AddToRole(user.Id, model.UserRole.ToString());
                }
            }
            return Json(_responseProvider.GenerateOKResponse());
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("SmsReceived")]
        public IHttpActionResult SmsReceived(MobileValidation model)
        {
            var confirmed = _userManager.ConfirmMobileNo(model.MobileBrief());
            return Json(confirmed);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("ChangePassword")]
        public IHttpActionResult ChangePassword([FromUri] UserChangePassModel model)
        {
            string mobile = "";
            if (!ModelState.IsValid)
            {
                return Json(_responseProvider.GenerateBadRequestResponse(ModelState));
                //return BadRequest(ModelState);
            }
            //if (!string.IsNullOrEmpty(model.Mobile))
            //    mobile = model.Mobile.Substring(1);
            //var confirmed = _userManager.ConfirmMobileNo(mobile);
            var confirmed = model.Mobile == "09000000001";
            if (confirmed)
            {
                var user = AppUserManager.FindByName(model.Mobile);
                if (user != null)
                {
                    IdentityResult result = AppUserManager.RemovePassword(user.Id);
                    result = AppUserManager.AddPassword(user.Id, model.Password);
                    if (!result.Succeeded)
                    {
                        return GetErrorResult(result);
                    }
                }
                else
                {
                    _responseProvider.SetBusinessMessage(new MessageResponse()
                    {
                        Message = getResource.getMessage("UserNotExist"),
                        Type = ResponseTypes.Error
                    });
                    return Json(_responseProvider.GenerateBadRequestResponse());
                }
                return Json(_responseProvider.GenerateOKResponse());
            }
            _responseProvider.SetBusinessMessage(new MessageResponse()
            {
                Message = getResource.getMessage("SmsNotReceived"),
                Type = ResponseTypes.Error
            });
            return Json(_responseProvider.GenerateBadRequestResponse());
        }

        // Post api/Account/Register
        //[Authorize(Roles = "AdminApplication,TaxiAgencyAdmin")]
        [AllowAnonymous]
        [Route("GetUsers")]
        public IHttpActionResult GetUsers()
        {
            var context = new ApplicationDbContext();
            var allUsers = context.Users.ToList();
            var jsonData = Json(allUsers);
            return jsonData;
        }

        private IHttpActionResult RegisterUser(RegisterModel model)
        {
            model.Password = System.Web.Security.Membership.GeneratePassword(16, 0);
            IdentityResult result;
            var userObj = new ApplicationUser() {UserName = model.Mobile, MobileConfirmed = false};
            //var userObj = new ApplicationUser() { Name = model.Name, Family = model.Family, Gender = model.Gender, UserName = model.Mobile,Code=model.Code, MobileConfirmed = false };
            var user = AppUserManager.FindByName(model.Mobile);
            if (user == null)
            {
                //new User
                model.Password = model.Password ?? "1234";
                result = AppUserManager.Create(userObj, model.Password);
                if (result.Succeeded)
                    user = AppUserManager.FindByName(model.Mobile);
            }
            else
            {
                _responseProvider.SetBusinessMessage(new MessageResponse()
                {
                    Message = getResource.getMessage("UserAlreadyExist"),
                    Type = ResponseTypes.Error
                });
                return Json(_responseProvider.GenerateBadRequestResponse());
                ////update current user
                //user = _userManager.PopulateUpdateModel(model, user);
                //result =  AppUserManager.Update(user);
            }
            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }
            else
            {
                AppUserManager.AddToRole(user.Id, model.UserRole.ToString());
            }
            //update other user info based on user role
            //_userManager.UpdateUserInfo(user, model);
            return Json(_responseProvider.GenerateOKResponse());
        }


        [Route("InsertPersoanlInfo")]
        public async Task<IHttpActionResult> InsertPersoanlInfo(PersoanlInfoModel model)
        {
            try
            {
                if (model == null)
                {
                    _responseProvider.SetBusinessMessage(new MessageResponse()
                    {
                        Type = ResponseTypes.Error,
                        Message = string.Format(getResource.getMessage("Required"), getResource.getString("Name"))
                    });
                    return Json(_responseProvider.GenerateBadRequestResponse());
                }
                if (!ModelState.IsValid)
                {
                    return Json(_responseProvider.GenerateBadRequestResponse(ModelState));
                }
                var user = await AppUserManager.FindByIdAsync(int.Parse(User.Identity.GetUserId()));
                user.Name = model.Name;
                user.Family = model.Family;
                user.Gender = model.Gender;
                user.Email = model.Email;
                user.Code = model.Code;
                await AppUserManager.UpdateAsync(user);
                _userManager.UpdatePersoanlInfo(model, int.Parse(User.Identity.GetUserId()));
                return Json(_responseProvider.GenerateOKResponse());
            }
            catch (Exception e)
            {
                _logManager.Log(Tag, "InsertPersoanlInfo", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [Route("RegisterUserInfo")]
        public async Task<IHttpActionResult> RegisterUserInfo(PersoanlInfoModel model)
        {
            try
            {
                if (model == null)
                {
                    _responseProvider.SetBusinessMessage(new MessageResponse()
                    {
                        Type = ResponseTypes.Error,
                        Message = string.Format(getResource.getMessage("Required"), getResource.getString("Name"))
                    });
                    return Json(_responseProvider.GenerateBadRequestResponse());
                }
                if (!ModelState.IsValid)
                {
                    return Json(_responseProvider.GenerateBadRequestResponse(ModelState));
                }
                var user = await AppUserManager.FindByIdAsync(int.Parse(User.Identity.GetUserId()));
                user.Name = model.Name;
                user.Family = model.Family;
                user.Gender = model.Gender;
                user.Email = model.Email;
                user.Code = model.Code;
                var res = AppUserManager.Update(user);
                if (res.Succeeded)
                {
                    _userManager.RegisterUserInfo(user, model, InviteTypes.PassInvite);
                    return Json(_responseProvider.GenerateOKResponse());
                }
                else
                {
                    _logManager.Log(Tag, "RegisterUserInfo", getResource.getMessage("ErrorHappened"));
                    _responseProvider.SetBusinessMessage(new MessageResponse()
                    {
                        Type = ResponseTypes.Error,
                        Message = getResource.getMessage("ErrorHappened")
                    });
                    return Json(_responseProvider.GenerateBadRequestResponse());
                }
            }
            catch (Exception e)
            {
                _logManager.Log(Tag, "RegisterUserInfo", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [Route("RegisterDriverInfo")]
        public async Task<IHttpActionResult> RegisterDriverInfo(PersoanlInfoModel model)
        {
            try
            {
                if (model == null)
                {
                    _responseProvider.SetBusinessMessage(new MessageResponse()
                    {
                        Type = ResponseTypes.Error,
                        Message = string.Format(getResource.getMessage("Required"), getResource.getString("Name"))
                    });
                    return Json(_responseProvider.GenerateBadRequestResponse());
                }
                if (!ModelState.IsValid)
                {
                    return Json(_responseProvider.GenerateBadRequestResponse(ModelState));
                }
                var user = await AppUserManager.FindByIdAsync(int.Parse(User.Identity.GetUserId()));
                user.Name = model.Name;
                user.Family = model.Family;
                user.Gender = model.Gender;
                user.Email = model.Email;
                user.Code = model.Code;
                var res = AppUserManager.Update(user);
                if (res.Succeeded)
                {
                    _userManager.RegisterUserInfo(user, model, InviteTypes.DriverInvite);
                    return Json(_responseProvider.GenerateOKResponse());
                }
                else
                {
                    _logManager.Log(Tag, "RegisterUserInfo", getResource.getMessage("ErrorHappened"));
                    _responseProvider.SetBusinessMessage(new MessageResponse()
                    {
                        Type = ResponseTypes.Error,
                        Message = getResource.getMessage("ErrorHappened")
                    });
                    return Json(_responseProvider.GenerateBadRequestResponse());
                }
            }
            catch (Exception e)
            {
                _logManager.Log(Tag, "RegisterUserInfo", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }


        [Route("InsertUserInfo")]
        public async Task<IHttpActionResult> InsertUserInfo(UserInfoModel model)
        {
            try
            {
                if (model == null)
                {
                    _responseProvider.SetBusinessMessage(new MessageResponse()
                    {
                        Type = ResponseTypes.Error,
                        Message = string.Format(getResource.getMessage("Required"), getResource.getString("Name"))
                    });
                    return Json(_responseProvider.GenerateBadRequestResponse());
                }
                if (!ModelState.IsValid)
                {
                    return Json(_responseProvider.GenerateBadRequestResponse(ModelState));
                }
                var user = await AppUserManager.FindByIdAsync(int.Parse(User.Identity.GetUserId()));
                user.Name = model.Name;
                user.Family = model.Family;
                user.Gender = model.Gender;
                user.Email = model.Email;
                user.Code = model.Code;
                await AppUserManager.UpdateAsync(user);
                _userManager.UpdateUserInfo(model, int.Parse(User.Identity.GetUserId()));
                return Json(_responseProvider.GenerateOKResponse());
            }
            catch (Exception e)
            {
                _logManager.Log(Tag, "InsertUserInfo", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [Route("InsertEmailInfo")]
        public async Task<IHttpActionResult> InsertEmailInfo(PersoanlInfoModel model)
        {
            try
            {
                if (model == null)
                {
                    _responseProvider.SetBusinessMessage(new MessageResponse()
                    {
                        Type = ResponseTypes.Error,
                        Message = string.Format(getResource.getMessage("Required"), getResource.getString("Name"))
                    });
                    return Json(_responseProvider.GenerateBadRequestResponse());
                }
                var user = await AppUserManager.FindByIdAsync(int.Parse(User.Identity.GetUserId()));
                user.Email = model.Email;
                await AppUserManager.UpdateAsync(user);
                return Json(_responseProvider.GenerateOKResponse());
            }
            catch (Exception e)
            {
                _logManager.Log(Tag, "GetSuggestRoute", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [Route("InsertPersonalPic")]
        [HttpPost]
        public IHttpActionResult InsertPersonalPic()
        {
            var userPic = HttpContext.Current.Request.Files.Count > 0 ? HttpContext.Current.Request.Files[0] : null;
            if (userPic == null)
            {
                _responseProvider.SetBusinessMessage(new MessageResponse()
                {
                    Type = ResponseTypes.Error,
                    Message = string.Format(getResource.getMessage("Required"), getResource.getString("UserPic"))
                });
                return Json(_responseProvider.GenerateBadRequestResponse());
            }
            byte[] userPicModel;
            using (var binaryReader = new BinaryReader(HttpContext.Current.Request.Files[0].InputStream))
            {
                userPicModel = binaryReader.ReadBytes(HttpContext.Current.Request.Files[0].ContentLength);
            }
            if (!IsImage(userPicModel))
            {
                _responseProvider.SetBusinessMessage(new MessageResponse()
                {
                    Type = ResponseTypes.Error,
                    Message = getResource.getMessage("NotRightFormat")
                });
                return Json(_responseProvider.GenerateBadRequestResponse());
            }
            _userManager.UpdatePersoanlPic(userPicModel, int.Parse(User.Identity.GetUserId()));
            return Json(_responseProvider.GenerateOKResponse());
        }


        [Route("InsertImage")]
        [HttpPost]
        public IHttpActionResult InsertImage(ImageFile imageFile)
        {
            try
            {
                var imageId = _userManager.InsertImage(imageFile, int.Parse(User.Identity.GetUserId()));
                return Json(_responseProvider.GenerateResponse(imageId, "ImageId"));
            }
            catch (Exception e)
            {
                _logManager.Log(Tag, "InsertImage", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("GetUserInfo")]
        [Authorize]
        public IHttpActionResult GetUserInfo()
        {
            var res = _userManager.GetUserInfo(int.Parse(User.Identity.GetUserId()));
            return Json(res);
        }


        [HttpPost]
        [Route("GetUserInitialInfo")]
        [Authorize]
        public IHttpActionResult GetUserInitialInfo()
        {
            var res = _userManager.GetUserInitialInfo(int.Parse(User.Identity.GetUserId()));
            return Json(res);
        }

        [HttpPost]
        [Route("GetUserScoresByContact")]
        [Authorize]
        public IHttpActionResult GetUserScoresByContact(ContactModel contactModel)
        {
            var res = _userManager.GetUserScoresByContact(int.Parse(User.Identity.GetUserId()), contactModel.ContactId);
            return Json(res);
        }


        [HttpPost]
        [Route("GetUserScores")]
        [Authorize]
        public IHttpActionResult GetUserScores()
        {
            var res = _userManager.GetUserScores(int.Parse(User.Identity.GetUserId()));
            return Json(res);
        }

        [HttpPost]
        [Route("GetPersonalInfo")]
        public IHttpActionResult GetPersonalInfo()
        {
            var res = _userManager.GetPersonalInfo(int.Parse(User.Identity.GetUserId()));
            if (res.UserPic != null)
            {
                return
                    Json(
                        new
                        {
                            res.Mobile,
                            res.Name,
                            res.Family,
                            res.Gender,
                            res.NationalCode,
                            res.Email,
                            res.UserImageId,
                            Base64UserPic = Convert.ToBase64String(res.UserPic)
                        });
            }
            return Json(new {res.Mobile, res.Name, res.Family, res.Gender, res.NationalCode, res.Email});
        }

        [HttpPost]
        [Route("GetUserPersonalInfo")]
        [Authorize(Roles = "AdminUser")]
        public IHttpActionResult GetUserPersonalInfo(UserInfoRequest model)
        {
            var res = _userManager.GetUserPersonalInfoByMobile(model.Mobile);
            if (res.UserPic != null)
            {
                return
                    Json(
                        new
                        {
                            res.Mobile,
                            res.Name,
                            res.Family,
                            res.Gender,
                            res.NationalCode,
                            res.Email,
                            res.UserImageId,
                            Base64UserPic = Convert.ToBase64String(res.UserPic)
                        });
            }
            return Json(new {res.Mobile, res.Name, res.Family, res.Gender, res.NationalCode, res.Email});
        }

        [HttpPost]
        [Route("GetRouteUser")]
        [Authorize(Roles = "AdminUser")]
        public IHttpActionResult GetRouteUser(RouteRequestModel model)
        {
            var res = _userManager.GetPersonalInfoByRouteId(model.RouteRequestId);
            if (res.UserPic != null)
            {
                return
                    Json(
                        new
                        {
                            res.Mobile,
                            res.Name,
                            res.Family,
                            res.Gender,
                            res.NationalCode,
                            res.Email,
                            res.UserImageId,
                            Base64UserPic = Convert.ToBase64String(res.UserPic)
                        });
            }
            return Json(new {res.Mobile, res.Name, res.Family, res.Gender, res.NationalCode, res.Email});
        }

        [HttpPost]
        [Route("GetRouteUserImage")]
        public IHttpActionResult GetRouteUserImage(RouteRequestModel model)
        {
            var res = _userManager.GetRouteUserImage(int.Parse(User.Identity.GetUserId()), model.RouteRequestId);
            if (res.UserPic != null)
            {
                return Json(new {Base64UserPic = Convert.ToBase64String(res.UserPic)});
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("GetCommentUserImage")]
        public IHttpActionResult GetCommentUserImage(CommentModel model)
        {
            var res = _userManager.GetCommentUserImage(int.Parse(User.Identity.GetUserId()), (int) model.CommentId);
            if (res.UserPic != null)
            {
                return Json(new {Base64UserPic = Convert.ToBase64String(res.UserPic)});
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("InsertLicenseInfo")]
        public IHttpActionResult InsertLicenseInfo(LicenseInfoModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(_responseProvider.GenerateBadRequestResponse(ModelState));
                }
                _userManager.InsertLicenseInfo(model, int.Parse(User.Identity.GetUserId()));
                return Json(_responseProvider.GenerateOKResponse());
            }
            catch (Exception e)
            {
                _logManager.Log(Tag, "GetSuggestRoute", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("InsertLicensePic")]
        public IHttpActionResult InsertLicensePic()
        {
            var licensePic = HttpContext.Current.Request.Files.Count > 0 ? HttpContext.Current.Request.Files[0] : null;
            if (licensePic == null)
            {
                _responseProvider.SetBusinessMessage(new MessageResponse()
                {
                    Type = ResponseTypes.Error,
                    Message = string.Format(getResource.getMessage("Required"), getResource.getString("LicensePic"))
                });
                return Json(_responseProvider.GenerateBadRequestResponse());
            }
            byte[] licensePicModel;
            using (var binaryReader = new BinaryReader(HttpContext.Current.Request.Files[0].InputStream))
            {
                licensePicModel = binaryReader.ReadBytes(HttpContext.Current.Request.Files[0].ContentLength);
            }
            if (!IsImage(licensePicModel))
            {
                _responseProvider.SetBusinessMessage(new MessageResponse()
                {
                    Type = ResponseTypes.Error,
                    Message = getResource.getMessage("NotRightFormat")
                });
                return Json(_responseProvider.GenerateBadRequestResponse());
            }
            _userManager.InsertLicensePic(licensePicModel, int.Parse(User.Identity.GetUserId()));
            return Json(_responseProvider.GenerateOKResponse());
        }

        [HttpPost]
        [Route("InsertNationalCardPic")]
        public IHttpActionResult InsertNationalCardPic()
        {
            var licensePic = HttpContext.Current.Request.Files.Count > 0 ? HttpContext.Current.Request.Files[0] : null;
            if (licensePic == null)
            {
                _responseProvider.SetBusinessMessage(new MessageResponse()
                {
                    Type = ResponseTypes.Error,
                    Message = string.Format(getResource.getMessage("Required"), getResource.getString("LicensePic"))
                });
                return Json(_responseProvider.GenerateBadRequestResponse());
            }
            byte[] nationalCardPicModel;
            using (var binaryReader = new BinaryReader(HttpContext.Current.Request.Files[0].InputStream))
            {
                nationalCardPicModel = binaryReader.ReadBytes(HttpContext.Current.Request.Files[0].ContentLength);
            }
            if (!IsImage(nationalCardPicModel))
            {
                _responseProvider.SetBusinessMessage(new MessageResponse()
                {
                    Type = ResponseTypes.Error,
                    Message = getResource.getMessage("NotRightFormat")
                });
                return Json(_responseProvider.GenerateBadRequestResponse());
            }
            _userManager.InsertNationalCardPic(nationalCardPicModel, int.Parse(User.Identity.GetUserId()));
            return Json(_responseProvider.GenerateOKResponse());
        }

        [HttpPost]
        [Route("GetLicenseInfo")]
        public IHttpActionResult GetLicenseInfo()
        {
            var res = _userManager.GetLicenseInfo(int.Parse(User.Identity.GetUserId()));
            if (res.LicensePic != null)
            {
                return
                    Json(
                        new
                        {
                            res.LicenseNo,
                            res.LicenseImageId,
                            Base64LicensePic = Convert.ToBase64String(res.LicensePic)
                        });
            }
            return Json(new {res.LicenseNo});
        }

        [HttpPost]
        [Route("GetImageById")]
        public IHttpActionResult GetImageById(ImageRequest model)
        {
            if (model != null && User != null)
            {
                var res = _userManager.GetImageById(model);
                if (res.ImageFile != null)
                {
                    return
                        Json(new {res.ImageId, res.ImageType, Base64ImageFile = Convert.ToBase64String(res.ImageFile)});
                }
            }
            return Json(_responseProvider.GenerateBadRequestResponse());
        }

/*[HttpPost]
                                [Authorize(Roles = "AdminUser")]
                                [Route("GetImageByUserId")]
                                public IHttpActionResult GetImageByUserId(ImageRequest model)
                                {
                                    if (User != null)
                                    {
                                        var res = _userManager.GetImageByUserId(model);
                                        if (res.ImageFile != null)
                                        {
                                            return Json(new { Base64Image = Convert.ToBase64String(res.ImageFile) });
                                        }
                                    }
                                    return Json(_responseProvider.GenerateBadRequestResponse());
                                }*/

        [HttpPost]
        [Route("GetUserLicenseInfo")]
        public IHttpActionResult GetUserLicenseInfo(UserInfoRequest model)
        {
            var res = _userManager.GetUserLicenseInfo(model.Mobile);
            if (res.LicensePic != null)
            {
                return Json(new {res.LicenseNo, res.LicenseImageId, Base64LicensePic = Convert.ToBase64String(res.LicensePic)});
            }
            return Json(new {res.LicenseNo});
        }

        [HttpPost]
        [Route("GetBankInfo")]
        public IHttpActionResult GetBankInfo()
        {
            var res = _userManager.GetBankInfo(int.Parse(User.Identity.GetUserId()));
            if (res.BankCardPic != null)
            {
                return Json(new {res.BankName, res.BankAccountNo, res.BankShaba, res.BankCardImageId, Base64BankCardPic = Convert.ToBase64String(res.BankCardPic)});
            }
            return Json(new {res.BankName, res.BankAccountNo, res.BankShaba});
        }

        [HttpPost]
        [Route("InsertCarInfo")]
        public IHttpActionResult InsertCarInfo(CarInfoModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(_responseProvider.GenerateBadRequestResponse(ModelState));
                }
                if (model == null)
                {
                    _responseProvider.SetBusinessMessage(new MessageResponse() {Type = ResponseTypes.Error, Message = string.Format(getResource.getMessage("Required"), getResource.getString("CarType"))});
                    return Json(_responseProvider.GenerateBadRequestResponse());
                }
                _userManager.InsertCarInfo(model, int.Parse(User.Identity.GetUserId()));
                return Json(_responseProvider.GenerateOKResponse());
            }
            catch (Exception e)
            {
                _logManager.Log(Tag, "InsertCarInfo", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("InsertBankInfo")]
        public IHttpActionResult InsertBankInfo(BankInfoModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(_responseProvider.GenerateBadRequestResponse(ModelState));
                }
                if (model == null)
                {
                    _responseProvider.SetBusinessMessage(new MessageResponse() {Type = ResponseTypes.Error, Message = string.Format(getResource.getMessage("Required"), getResource.getString("BankName"))});
                    return Json(_responseProvider.GenerateBadRequestResponse());
                }
                _userManager.InsertBankInfo(model, int.Parse(User.Identity.GetUserId()));
                return Json(_responseProvider.GenerateOKResponse());
            }
            catch (Exception e)
            {
                _logManager.Log(Tag, "InsertBankInfo", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("InsertAboutMe")]
        public IHttpActionResult InsertAboutMe(AboutUserModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(_responseProvider.GenerateBadRequestResponse(ModelState));
                }
                if (model == null)
                {
                    return Json(_responseProvider.GenerateBadRequestResponse());
                }
                _userManager.InsertAboutUser(model, int.Parse(User.Identity.GetUserId()));
                return Json(_responseProvider.GenerateOKResponse());
            }
            catch (Exception e)
            {
                _logManager.Log(Tag, "InsertAboutMe", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("SubmitUserScore")]
        public IHttpActionResult SubmitUserScore(ContactScoreModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(_responseProvider.GenerateBadRequestResponse(ModelState));
                }
                if (model == null)
                {
                    return Json(_responseProvider.GenerateBadRequestResponse());
                }
                var res = _userManager.InsertUserScore(int.Parse(User.Identity.GetUserId()), model);
                return Json(_responseProvider.GenerateResponse(res, "ScoreModel"));
            }
            catch (Exception e)
            {
                _logManager.Log(Tag, "SubmitUserScore", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        /*[HttpPost]
        [Route("SubmitDiscount")]
        public IHttpActionResult SubmitDiscount(DiscountModel model)
        {
            try
            {
                if (model == null)
                {
                    _responseProvider.SetBusinessMessage(new MessageResponse() { Type = ResponseTypes.Error, Message = string.Format(getResource.getMessage("Required"), getResource.getString("BankName")) });
                    return Json(_responseProvider.GenerateBadRequestResponse());
                }
                _userManager.DoDiscount(,model.DiscountCode, int.Parse(User.Identity.GetUserId()));
                /*if (_userManager.DiscountCodeExist(model))
                {
                    if (_userManager.DiscountCodeUsed(model, int.Parse(User.Identity.GetUserId())))
                    {
                        _responseProvider.SetBusinessMessage(new MessageResponse() { Message = getResource.getMessage("CodeUsed"), Type = ResponseTypes.Error });
                        return Json(_responseProvider.GenerateBadRequestResponse());
                    }
                    else
                    {
                        _userManager.InsertDiscountCode(model, int.Parse(User.Identity.GetUserId()));
                        return Json(_responseProvider.GenerateResponse(getResource.getMessage("CodeSubmitted"), "CodeSubmitted"));
                    }
                }
                else
                {
                    _responseProvider.SetBusinessMessage(new MessageResponse() { Message = getResource.getMessage("CodeNotExist"), Type = ResponseTypes.Error });
                    return Json(_responseProvider.GenerateBadRequestResponse());
                }#1#
            }
            catch (Exception e)
            {
                _logManager.Log(Tag, "SubmitDiscount", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }*/

        [HttpPost]
        [Route("InsertBankCardPic")]
        public IHttpActionResult InsertBankCardPic()
        {
            var licensePic = HttpContext.Current.Request.Files.Count > 0 ? HttpContext.Current.Request.Files[0] : null;
            if (licensePic == null)
            {
                _responseProvider.SetBusinessMessage(new MessageResponse() {Type = ResponseTypes.Error, Message = string.Format(getResource.getMessage("Required"), getResource.getString("BankCardPic"))});
                return Json(_responseProvider.GenerateBadRequestResponse());
            }
            byte[] bankCardPicModel;
            using (var binaryReader = new BinaryReader(HttpContext.Current.Request.Files[0].InputStream))
            {
                bankCardPicModel = binaryReader.ReadBytes(HttpContext.Current.Request.Files[0].ContentLength);
            }
            if (!IsImage(bankCardPicModel))
            {
                _responseProvider.SetBusinessMessage(new MessageResponse() {Type = ResponseTypes.Error, Message = getResource.getMessage("NotRightFormat")});
                return Json(_responseProvider.GenerateBadRequestResponse());
            }
            _userManager.InsertBankCardPic(bankCardPicModel, int.Parse(User.Identity.GetUserId()));
            return Json(_responseProvider.GenerateOKResponse());
        }


        [HttpPost]
        [Route("InsertCarPics")]
        public IHttpActionResult InsertCarPics()
        {
            var licensePic = HttpContext.Current.Request.Files.Count > 0 ? HttpContext.Current.Request.Files[0] : null;
            if (licensePic == null)
            {
                _responseProvider.SetBusinessMessage(new MessageResponse() {Type = ResponseTypes.Error, Message = string.Format(getResource.getMessage("Required"), getResource.getString("CarCardPic"))});
                return Json(_responseProvider.GenerateBadRequestResponse());
            }
            byte[] carPicModel;
            using (var binaryReader = new BinaryReader(HttpContext.Current.Request.Files[0].InputStream))
            {
                carPicModel = binaryReader.ReadBytes(HttpContext.Current.Request.Files[0].ContentLength);
            }
            if (!IsImage(carPicModel))
            {
                _responseProvider.SetBusinessMessage(new MessageResponse() {Type = ResponseTypes.Error, Message = getResource.getMessage("NotRightFormat")});
                return Json(_responseProvider.GenerateBadRequestResponse());
            }
            _userManager.InsertCarPic(carPicModel, int.Parse(User.Identity.GetUserId()));
            return Json(_responseProvider.GenerateOKResponse());
        }

        [HttpPost]
        [Route("InsertCarBackPic")]
        public IHttpActionResult InsertCarBackPic()
        {
            var licensePic = HttpContext.Current.Request.Files.Count > 0 ? HttpContext.Current.Request.Files[0] : null;
            if (licensePic == null)
            {
                _responseProvider.SetBusinessMessage(new MessageResponse() {Type = ResponseTypes.Error, Message = string.Format(getResource.getMessage("Required"), getResource.getString("CarCardPic"))});
                return Json(_responseProvider.GenerateBadRequestResponse());
            }
            byte[] carPicModel;
            using (var binaryReader = new BinaryReader(HttpContext.Current.Request.Files[0].InputStream))
            {
                carPicModel = binaryReader.ReadBytes(HttpContext.Current.Request.Files[0].ContentLength);
            }
            if (!IsImage(carPicModel))
            {
                _responseProvider.SetBusinessMessage(new MessageResponse() {Type = ResponseTypes.Error, Message = getResource.getMessage("NotRightFormat")});
                return Json(_responseProvider.GenerateBadRequestResponse());
            }
            _userManager.InsertCarBackPic(carPicModel, int.Parse(User.Identity.GetUserId()));
            return Json(_responseProvider.GenerateOKResponse());
        }

        [HttpPost]
        [Route("GetDiscount")]
        public IHttpActionResult GetDiscount()
        {
            try
            {
                var res = _userManager.GetUserDiscount(int.Parse(User.Identity.GetUserId()));
                var jsonRes = Json(_responseProvider.GenerateDiscountResponse(res));
                return jsonRes;
            }
            catch (Exception e)
            {
                _logManager.Log(Tag, "GetDiscount", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("GetWithdraw")]
        public IHttpActionResult GetWithdraw()
        {
            try
            {
                var res = _userManager.GetWithdraw(int.Parse(User.Identity.GetUserId()));
                var jsonRes = Json(_responseProvider.GenerateWithdrawResponse(res));
                return jsonRes;
            }
            catch (Exception e)
            {
                _logManager.Log(Tag, "GetWithdraw", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("SubmitWithdraw")]
        public IHttpActionResult SubmitWithdraw(WithdrawRequestModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(_responseProvider.GenerateBadRequestResponse(ModelState));
                }
                if (model == null)
                {
                    _responseProvider.SetBusinessMessage(new MessageResponse() {Type = ResponseTypes.Error, Message = string.Format(getResource.getMessage("Required"), getResource.getString("BankName"))});
                    return Json(_responseProvider.GenerateBadRequestResponse());
                }
                if (_userManager.WithdrawlValid(model, int.Parse(User.Identity.GetUserId())))
                {
                    _userManager.InsertWithdrawRequest(model, int.Parse(User.Identity.GetUserId()));
                    return Json(_responseProvider.GenerateResponse(getResource.getMessage("RequestSubmitted"), "RequestSubmitted"));
                }
                else
                {
                    _responseProvider.SetBusinessMessage(new MessageResponse() {Message = getResource.getMessage("NotRemain"), Type = ResponseTypes.Error});
                    return Json(_responseProvider.GenerateBadRequestResponse());
                }
            }
            catch (Exception e)
            {
                _logManager.Log(Tag, "SubmitDiscount", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }


        [HttpPost]
        [Route("GetAboutMe")]
        public IHttpActionResult GetAboutMe()
        {
            try
            {
                var res = _userManager.GetUserAboutMe(int.Parse(User.Identity.GetUserId()));
                //var jsonRes = Json(_responseProvider.GenerateRouteResponse(res,"AboutMe"));
                return Json(res);
            }
            catch (Exception e)
            {
                _logManager.Log(Tag, "GetAboutMe", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("GetRatings")]
        public IHttpActionResult GetRatings()
        {
            try
            {
                var res = _userManager.GetRatings(int.Parse(User.Identity.GetUserId()));
                var jsonRes = Json(_responseProvider.GenerateResponse(res));
                return jsonRes;
            }
            catch (Exception e)
            {
                _logManager.Log(Tag, "GetRatings", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("SetRatings")]
        public IHttpActionResult SetRatings(RatingModel model)
        {
            try
            {
                if (model.RatingsList != null)
                {
                    List<RatingModel> resp = JsonConvert.DeserializeObject<List<RatingModel>>(model.RatingsList);
                    var res = _userManager.SetRatings(int.Parse(User.Identity.GetUserId()), resp);
                    var jsonRes = Json(_responseProvider.GenerateResponse(res, "setRating"));
                    return jsonRes;
                }
            }
            catch (Exception e)
            {
                _logManager.Log(Tag, "SetRatings", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("GetInvite")]
        public IHttpActionResult GetInvite()
        {
            try
            {
                var res = _userManager.GetUserInvite(int.Parse(User.Identity.GetUserId()), InviteTypes.PassInvite);
                //var jsonRes = Json(_responseProvider.GenerateRouteResponse(res,"AboutMe"));
                return Json(res);
            }
            catch (Exception e)
            {
                _logManager.Log(Tag, "GetInvite", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("GetCarInfo")]
        public IHttpActionResult GetCarInfo()
        {
            var res = _userManager.GetCarInfo(int.Parse(User.Identity.GetUserId()));
            return Json(new {res.CarType, res.CarColor, res.CarPlateNo, res.CarFrontImageId, res.CarBackImageId, Base64CarCardPic = res.CarCardPic != null ? Convert.ToBase64String(res.CarCardPic) : null, Base64CarCardBckPic = res.CarCardBkPic != null ? Convert.ToBase64String(res.CarCardBkPic) : null});
        }


        [HttpPost]
        [Route("GetUserContacts")]
        [Authorize]
        public IHttpActionResult GetUserContacts()
        {
            try
            {
                var response = _userManager.GetUserContacts(int.Parse(User.Identity.GetUserId()));
                var withPic = new List<object>();
                foreach (var res in response)
                {
                    //if (res.UserPic == null)
                    //{
                    withPic.Add(new {res.ContactId, res.Name, res.Family, res.Gender, res.LastMsgTime, res.LastMsg, res.IsRideAccepted, res.IsPassengerAccepted, res.IsSupport, res.IsDriver, res.UserImageId, res.AboutUser});
                    //}
                    //else
                    //{
                    //    withPic.Add(new { res.ContactId, res.Name, res.Family, res.Gender, res.LastMsgTime, res.LastMsg, res.IsSupport, res.IsRideAccepted, res.IsDriver, res.UserImageId, Base64UserPic = Convert.ToBase64String(res.UserPic) });
                    //}
                }
                return Json(_responseProvider.GenerateObjectResponse(withPic, "userContacts"));
            }
            catch (Exception e)
            {
                _logManager.Log(Tag, "GetUserContacts", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("SaveGcmToken")]
        [Authorize]
        public IHttpActionResult SaveGcmToken(Gtoken model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(_responseProvider.GenerateBadRequestResponse(ModelState));
                }
                _userManager.InsertGoogleToken(int.Parse(User.Identity.GetUserId()), model, UserRoles.MobileUser);
                return Json(_responseProvider.GenerateOKResponse());
            }
            catch (Exception e)
            {
                _logManager.Log(Tag, "SaveGcmToken", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("GetUserCarInfo")]
        [Authorize(Roles = "AdminUser")]
        public IHttpActionResult GetUserCarInfo(UserInfoRequest model)
        {
            var res = _userManager.GetUserCarInfo(model.Mobile);
            return Json(new {res.CarType, res.CarColor, res.CarPlateNo, res.CarFrontImageId, res.CarBackImageId, Base64CarCardPic = res.CarCardPic != null ? Convert.ToBase64String(res.CarCardPic) : null, Base64CarCardBckPic = res.CarCardBkPic != null ? Convert.ToBase64String(res.CarCardBkPic) : null});
        }

        [HttpPost]
        [Route("ConfirmMobileNo")]
        public IHttpActionResult ConfirmMobileNo(MobileValidation model)
        {
            var user = AppUserManager.FindById(int.Parse(User.Identity.GetUserId()));
            bool confirmed = user.MobileConfirmed || _userManager.ConfirmMobileNo(model.MobileBrief());
            if (confirmed)
            {
                user.MobileConfirmed = true;
                AppUserManager.Update(user);
            }
            return Json(confirmed);
        }

        [HttpPost]
        [Route("SendConfirmMobileSms")]
        public IHttpActionResult SendConfirmMobileSms(MobileValidation model)
        {
            ApplicationUser user;
            if (User.Identity.GetUserId() == null)
            {
                user = AppUserManager.FindByName(model.Mobile);
            }
            else
            {
                user = AppUserManager.FindById(int.Parse(User.Identity.GetUserId()));
            }
            if (user == null)
            {
                return Json(_responseProvider.GenerateBadRequestResponse());
            }
            string rand = GetDayConfirmNo(user.Id);
            try
            {
                var confirmed = _userManager.SendConfirmMobileSms(user, model, rand);
                return Json(confirmed);
            }
            catch (Exception e)
            {
                _logManager.Log(Tag, "SendConfirmMobileSms", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        private string GetDayConfirmNo(int id)
        {
            string dateTimeStamp = DateTime.Now.ToString("MMdd");
            string res = ((int.Parse(dateTimeStamp))*id%1000).ToString();
            return res;
        }


        [HttpPost]
        [Route("ConfirmMobileSms")]
        public IHttpActionResult ConfirmMobileSms(MobileValidation model)
        {
            ApplicationUser user;
            if (User.Identity.GetUserId() == null)
            {
                user = AppUserManager.FindByName(model.Mobile);
            }
            else
            {
                user = AppUserManager.FindById(int.Parse(User.Identity.GetUserId()));
            }
            if (user == null)
            {
                return Json(_responseProvider.GenerateBadRequestResponse());
            }
            _userManager.ValidatingTry(user.Id);
            var confirmed = GetDayConfirmNo(user.Id) == model.ValidationCode;
            if (confirmed)
            {
                var newPass = System.Web.Security.Membership.GeneratePassword(16, 0);
                IdentityResult result = AppUserManager.RemovePassword(user.Id);
                if (result.Succeeded)
                {
                    result = AppUserManager.AddPassword(user.Id, newPass);
                    if (result.Succeeded)
                    {
                        bool isUserRegistered = !string.IsNullOrEmpty(user.Name);
                        user.MobileConfirmed = true;
                        AppUserManager.Update(user);
                        return Json(_responseProvider.GenerateResponse(new {Password = newPass, Confirmed = true, IsUserRegistered = isUserRegistered},
                            "password"));
                    }
                    return GetErrorResult(result);
                }
                return GetErrorResult(result);
            }
            return Json(_responseProvider.GenerateResponse(new {Password = "", Confirmed = false, IsUserRegistered = false}, "password"));
        }

        [HttpPost]
        [Route("GetUserTripId")]
        [Authorize]
        public IHttpActionResult GetUserTripId()
        {
            try
            {
                var response = _userManager.GetUserTrip(int.Parse(User.Identity.GetUserId()));
                return Json(new ResponseModel() {Messages = new List<string>() {response.ToString()}});
            }
            catch (Exception e)
            {
                _logManager.Log(Tag, "GetUserContacts", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpGet]
        [Route("SendNotif")]
        [AllowAnonymous]
        public IHttpActionResult SendNotif()
        {
            try
            {
                _userManager.SendNotif();
                return Json(_responseProvider.GenerateOKResponse());
            }
            catch (Exception e)
            {
                _logManager.Log(Tag, "GetUserContacts", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }


        [HttpPost]
        [Route("GetAllUsers")]
        [Authorize(Roles = "AdminUser")]
        public IHttpActionResult GetAllUsers()
        {
            try
            {
                var res = _userManager.GetAllUsers();
                var jsonRes = Json(_responseProvider.GenerateRouteResponse(res));
                return jsonRes;
            }
            catch (Exception e)
            {
                _logManager.Log(Tag, "GetAllUsers", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("GetUserByInfo")]
        [Authorize(Roles = "AdminUser")]
        public IHttpActionResult GetUserByInfo(UserSearchModel model)
        {
            try
            {
                var res = _userManager.GetUserByInfo(model);
                var jsonRes = Json(_responseProvider.GenerateRouteResponse(res));
                return jsonRes;
            }
            catch (Exception e)
            {
                _logManager.Log(Tag, "GetUserByInfo", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("GetUserInfoById")]
        [Authorize(Roles = "AdminUser")]
        public IHttpActionResult GetUserInfoById(PersoanlInfoModel model)
        {
            try
            {
                var res = _userManager.GetUserInfoById(model.UserUId.Value);
                var jsonRes = Json(_responseProvider.GenerateResponse(res, "UserInfo"));
                return jsonRes;
            }
            catch (Exception e)
            {
                _logManager.Log(Tag, "GetUserInfoById", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }


        [HttpPost]
        [Route("EditUserInfo")]
        [Authorize(Roles = "AdminUser")]
        public IHttpActionResult EditUserInfo(UserInfoAdminModel model)
        {
            try
            {
                _userManager.EditUserInfo(model);
                return Json(_responseProvider.GenerateOKResponse());
            }
            catch (Exception e)
            {
                _logManager.Log(Tag, "EditUserInfo", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }


        [HttpPost]
        [Route("SendUserTripLocation")]
        public IHttpActionResult SendUserTripLocation(UserLocation userLocation)
        {
            try
            {
                var user = AppUserManager.FindByName(userLocation.Mobile);
                _userManager.SaveUserTripLocation(user.Id, userLocation);
                return Json(_responseProvider.GenerateOKResponse());
            }
            catch (Exception e)
            {
                _logManager.Log(Tag, "SendUserTripLocation", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }


        [HttpPost]
        [Route("AcceptRideShare")]
        public IHttpActionResult AcceptRideShare(ContactModel model)
        {
            try
            {
                //var res = _userManager.AcceptRideShare(int.Parse(User.Identity.GetUserId()), model.ContactId);
                //return Json(_responseProvider.GenerateResponse(new List<string> { res }, "AcceptRideShare"));
                //return Json(_responseProvider.GenerateSuggestAcceptResponse(res));
            }
            catch (Exception e)
            {
                _logManager.Log(Tag, "AcceptRideShare", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        /*[HttpGet]
        [Route("InvokeTrips")]
        public IHttpActionResult InvokeTrips()
        {
            try
            {
                var res = _userManager.InvokeTrips();
                return Json(_responseProvider.GenerateResponse(new List<string> { res }, "InvokeTrips"));
            }
            catch (Exception e)
            {
                _logManager.Log(Tag, "InvokeTrips", e.Message + "-" + e.InnerException.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }*/


        [HttpPost]
        [Route("NotifyEvents")]
        [AllowAnonymous]
        public IHttpActionResult NotifyEvents(UserInfoRequest model)
        {
            try
            {
                //var res = _userManager.GetNotifications(model.Mobile);
                //return Json(res);
                return Json(_responseProvider.GenerateOKResponse());
            }
            catch (Exception e)
            {
                _logManager.Log(Tag, "NotifyEvents", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("GetAppVersion")]
        [AllowAnonymous]
        public IHttpActionResult GetAppVersion()
        {
            _appVersion = ConfigurationManager.AppSettings["MobileAppVersion"];
            return Json(new ResponseModel() {Messages = new List<string>() {_appVersion}});
        }

        private CarInfoModel ReadFromCarRequest(HttpRequest request)
        {
            var ci = new CarInfoModel();
            ci.CarType = request.Form["CarType"];
            ci.CarColor = request.Form["CarColor"];
            ci.CarPlateNo = request.Form["CarPlateNo"];
            using (var binaryReader = new BinaryReader(request.Files[0].InputStream))
            {
                ci.CarCardPic = binaryReader.ReadBytes(request.Files[0].ContentLength);
            }
            return ci;
        }

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return Json(_responseProvider.GenerateInternalServerErrorResponse());
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }
                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return Json(_responseProvider.GenerateUnknownErrorResponse());
                }
                return Json(_responseProvider.GenerateBadRequestResponse(ModelState));
            }
            return null;
        }

        public static bool IsImage(byte[] bytes)
        {
            // see http://www.mikekunz.com/image_file_header.html  
            var bmp = Encoding.ASCII.GetBytes("BM"); // BMP
            var gif = Encoding.ASCII.GetBytes("GIF"); // GIF
            var png = new byte[] {137, 80, 78, 71}; // PNG
            var tiff = new byte[] {73, 73, 42}; // TIFF
            var tiff2 = new byte[] {77, 77, 42}; // TIFF
            var jpeg = new byte[] {255, 216, 255, 224}; // jpeg
            var jpeg2 = new byte[] {255, 216, 255, 225}; // jpeg canon

            if (bmp.SequenceEqual(bytes.Take(bmp.Length)))
                return true; //ImageFormat.bmp;

            if (gif.SequenceEqual(bytes.Take(gif.Length)))
                return true; // ImageFormat.gif;

            if (png.SequenceEqual(bytes.Take(png.Length)))
                return true; // ImageFormat.png;

            if (tiff.SequenceEqual(bytes.Take(tiff.Length)))
                return true; //ImageFormat.tiff;

            if (tiff2.SequenceEqual(bytes.Take(tiff2.Length)))
                return true; // ImageFormat.tiff;

            if (jpeg.SequenceEqual(bytes.Take(jpeg.Length)))
                return true; // ImageFormat.jpeg;

            if (jpeg2.SequenceEqual(bytes.Take(jpeg2.Length)))
                return true; // ImageFormat.jpeg;

            return false;
        }
    }
}