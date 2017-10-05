using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using System.Web.Http.Results;
using System.Web.Script.Serialization;
using CoreManager.FanapManager;
using CoreManager.LogProvider;
using CoreManager.Models;
using CoreManager.Models.RouteModels;
using CoreManager.Resources;
using CoreManager.ResponseProvider;
using CoreManager.RouteGroupManager;
using CoreManager.RouteManager;
using CoreManager.UserManager;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CoreApi.Controllers
{
    [Authorize]
    public class FanapController : ApiController
    {
        private static string Tag = "FanapController";
        private static string Sso_address = "http://sandbox.fanapium.com";
        private static string Client_Id = "b05741339a41cf30f58ac0e9";
        private static string redirect_Uri = "http://mibarim.ir/loginreturn";
        private IRouteManager _routemanager;
        private IRouteGroupManager _routeGroupManager;
        private ILogProvider _logmanager;
        private IUserManager _userManager;
        private IFanapManager _fanapManager;
        private IResponseProvider _responseProvider;
        private ApplicationUserManager _userAppManager;


        public FanapController()
        {
        }
        public FanapController(IRouteManager routeManager,
            ILogProvider logManager,
            IUserManager userManager,
            IRouteGroupManager routeGroupManager,
            IFanapManager fanapManager,
            IResponseProvider responseProvider)
        {
            _routemanager = routeManager;
            _routemanager = routeManager;
            _logmanager = logManager;
            _userManager = userManager;
            _fanapManager = fanapManager;
            _routeGroupManager = routeGroupManager;
            _responseProvider = responseProvider;
        }

        public ApplicationUserManager AppUserManager
        {
            get
            {
                return _userAppManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userAppManager = value;
            }
        }

/*        [HttpGet]
        [Route("FanapLogin")]
        public HttpResponseMessage FanapLogin()
        {
            try
            {
                var url = Sso_address + "/oauth2/authorize/?client_id=" + Client_Id + "&redirect_uri=" + redirect_Uri + "&response_type=code";
                var response = Request.CreateResponse(HttpStatusCode.Moved);
                response.Headers.Location = new Uri(url);
                return response;
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "FanapLogin", e.Message);
            }
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }

        [HttpGet]
        [Route("FanapLogin")]
        [AllowAnonymous]
        public HttpResponseMessage FanapLogin(string mobileNo)
        {
            try
            {
                //to-do get state from database
                var userInfo=_userManager.GetUserPersonalInfoByMobile(mobileNo);
                var state = userInfo.UserUId;
                var url = Sso_address + "/oauth2/authorize/?client_id="+ Client_Id + "&redirect_uri="+ redirect_Uri + "&response_type=code&state="+ state;
                    var response = Request.CreateResponse(HttpStatusCode.Moved);
                    response.Headers.Location = new Uri(url);
                    return response;
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "FanapLogin", e.Message);
            }
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }*/

        [HttpGet]
        [Route("Loginreturn")]
        [AllowAnonymous]
        public IHttpActionResult Loginreturn([FromUri] FanapModel fanapModel)
        {
            try
            {
                var res = new UserRegisterModel();
                var fModel = _fanapManager.GetFanapUserInfo(fanapModel);
                IdentityResult result;
                var userObj = new ApplicationUser() { UserName = fModel.UserName, Name = fModel.Name, Family = fModel.Family, MobileConfirmed = false, Code ="Fanap" };
                //var userObj = new ApplicationUser() { Name = model.Name, Family = model.Family, Gender = model.Gender, UserName = model.Mobile,Code=model.Code, MobileConfirmed = false };
                var user = AppUserManager.FindByName(fModel.UserName);
                var newPass = System.Web.Security.Membership.GeneratePassword(16, 0);
                if (user == null)
                {
                    result = AppUserManager.Create(userObj, newPass);
                    if (result.Succeeded)
                    {
                        user = AppUserManager.FindByName(fModel.UserName);
                        AppUserManager.AddToRole(user.Id, UserRoles.MobileDriver.ToString());
                        _fanapManager.SaveFanapUser(user.Id, fModel.UserId);
                        res.Mobile = fModel.UserName;
                        res.Password= newPass;
                        return Json(_responseProvider.GenerateResponse(res,"userpass"));
                        /*var url = "http://exitthisactivity";
                        var response = Request.CreateResponse(HttpStatusCode.Moved);
                        response.Headers.Location = new Uri(url);
                        return response;*/

                    }
                }
                else
                {
                    _fanapManager.SaveFanapUser(user.Id, fModel.UserId);
                    result = AppUserManager.RemovePassword(user.Id);
                    if (result.Succeeded)
                    {
                        result = AppUserManager.AddPassword(user.Id, newPass);
                        if (result.Succeeded)
                        {
                            bool isUserRegistered = !string.IsNullOrEmpty(user.Name);
                            user.MobileConfirmed = true;
                            AppUserManager.Update(user);
                            res.Mobile = fModel.UserName;
                            res.Password = newPass;
                            return Json(_responseProvider.GenerateResponse(res, "userpass"));
                        }
                    }
                    
                }
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "Loginreturn", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpGet]
        [Route("RegisterUserbyNickName")]
        [AllowAnonymous]
        public IHttpActionResult RegisterUserbyNickName(string nickname)
        {
            try
            {
                _userManager.RegisterFanap(nickname);
                return Json(_responseProvider.GenerateOKResponse());
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "RegisterUserbyNickName", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("FanapBookTrip")]
        public IHttpActionResult FanapBookTrip(PayModel model)
        {
            try
            {
                int ff;
                if (User != null && int.TryParse(User.Identity.GetUserId(), out ff))
                {
                    var res=_fanapManager.FanapBookTrip(ff, model.TripId);
                    return Json(_responseProvider.GenerateResponse(res, "FanapBookTrip"));
                }
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "FanapBookTrip", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("PayReturn")]
        [AllowAnonymous]
        public IHttpActionResult PayReturn(PaymentDetailModel model)
        {
            try
            {
                    var res = _fanapManager.Payresult(model.ReqId);
                    return Json(_responseProvider.GenerateResponse(res, "PayReturn"));
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "PayReturn", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }
    }
}
