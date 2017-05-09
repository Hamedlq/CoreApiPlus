using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CoreManager.GroupManager;
using CoreManager.LogProvider;
using CoreManager.Models;
using CoreManager.Models.RouteModels;
using CoreManager.Resources;
using CoreManager.ResponseProvider;
using CoreManager.UserManager;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

namespace CoreApi.Controllers
{
    public class GroupController : ApiController
    {
        private static string Tag = "GroupController";
        private ApplicationUserManager _userAppManager;
        private IUserManager _userManager;
        private IGroupManager _groupManager;
        private IResponseProvider _responseProvider;
        private ILogProvider _logProvider;
        public GroupController(ApplicationUserManager appUserManager,
            ISecureDataFormat<AuthenticationTicket> accessTokenFormat)
        {
            AppUserManager = appUserManager;
            AccessTokenFormat = accessTokenFormat;
        }
        public GroupController(IUserManager userManager, IGroupManager groupManager, IResponseProvider responseProvider, ILogProvider logProvider)
        {
            _userManager = userManager;
            _groupManager = groupManager;
            _responseProvider= responseProvider;
            _logProvider = logProvider;
        }

        public GroupController()
        {
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

        public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; private set; }


        [HttpPost]
        [Route("ConfirmAppointment")]
        public IHttpActionResult ConfirmAppointment(AppointmentModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(_responseProvider.GenerateBadRequestResponse(ModelState));
            }
            try
            {
                var res = _groupManager.ConfirmAppointment(int.Parse(User.Identity.GetUserId()), model);

                return Json(_responseProvider.GenerateResponse(new List<string> { res }, "ConfirmAppointment"));
            }
            catch (Exception e)
            {
                _logProvider.Log(Tag, "ConfirmAppointment", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }
        [HttpPost]
        [Route("AppointFinalConfirm")]
        public IHttpActionResult AppointFinalConfirm(AppointConfirmModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(_responseProvider.GenerateBadRequestResponse(ModelState));
            }
            try
            {
                var res = _groupManager.AppointFinalConfirm(int.Parse(User.Identity.GetUserId()), model);

                return Json(_responseProvider.GenerateResponse(new List<string> { res }, "AppointFinalConfirm"));
            }
            catch (Exception e)
            {
                _logProvider.Log(Tag, "AppointFinalConfirm", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }
        [HttpPost]
        [Route("GetGroupComments")]
        public IHttpActionResult GetGroupComments(AppointmentModel model)
        {
            try
            {
                var response = _groupManager.GetGroupComments(int.Parse(User.Identity.GetUserId()), model.GroupId);
                return Json(_responseProvider.GenerateResponse(response));
/*                var withPic = new List<object>();
                foreach (var res in response)
                {
                    if (res.UserPic == null)
                    {
                        withPic.Add(new { res.GroupId, res.NameFamily, res.CommentId, res.TimingString, res.Comment, res.IsDeletable });
                    }
                    else
                    {
                        withPic.Add(new { res.GroupId, res.NameFamily, res.CommentId, res.TimingString, res.Comment, res.IsDeletable , Base64UserPic = Convert.ToBase64String(res.UserPic) });
                    }
                }
                return Json(_responseProvider.GenerateObjectResponse(withPic));*/
            }
            catch (Exception e)
            {
                _logProvider.Log(Tag, "GetGroupComments", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }
        [HttpPost]
        [Route("SubmitComment")]
        public IHttpActionResult SubmitComment(CommentModel model)
        {
            try
            {
                var res = _groupManager.SubmitComment(int.Parse(User.Identity.GetUserId()), model.GroupId,model.Comment);

                return Json(_responseProvider.GenerateResponse(new List<string> { res }, "SubmitComment"));
            }
            catch (Exception e)
            {
                _logProvider.Log(Tag, "SubmitComment", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }
        [HttpPost]
        [Route("SubmitChatByMobile")]
        public IHttpActionResult SubmitChatByMobile(CommentModel model)
        {
            try
            {
                var user = AppUserManager.FindByName(model.Mobile);
                if (user != null)
                {
                    var res = _groupManager.SubmitChat(user.Id, model.Mobile, model.Comment);
                    return Json(_responseProvider.GenerateResponse(new List<string> { res }, "SubmitChatByMobile"));
                }
                else
                {
                    _responseProvider.SetBusinessMessage(new MessageResponse() { Message = getResource.getMessage("UserNotExist"), Type = ResponseTypes.Error });
                    return Json(_responseProvider.GenerateBadRequestResponse());
                }

            }
            catch (Exception e)
            {
                _logProvider.Log(Tag, "SubmitChatByMobile", e.Message);
            }
            
            return Json(_responseProvider.GenerateUnknownErrorResponse());

        }

        [HttpPost]
        [Route("GetPassengerConfirm")]
        public IHttpActionResult GetPassengerConfirm(RouteGroupModel model)
        {
            try
            {
                var res = _groupManager.GetPassengerConfirmInfo(int.Parse(User.Identity.GetUserId()), model.GroupId,
                    model.RouteId);

                return Json(_responseProvider.GenerateResponse(res));
            }
            catch (Exception e)
            {
                _logProvider.Log(Tag, "GetPassengerConfirm", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }
        [HttpPost]
        [Route("GetChatsByMobile")]
        [Authorize(Roles = "AdminUser")]
        public IHttpActionResult GetChatsByMobile(UserSearchModel model)
        {
            try
            {
                var res = new List<CommentModel>();
                var user = AppUserManager.FindByName(model.Mobile);
                if (user != null)
                {
                    res = _groupManager.GetSupportChats(user.Id);
                    return Json(_responseProvider.GenerateResponse(res));
                }
                else
                {
                    _responseProvider.SetBusinessMessage(new MessageResponse() { Message = getResource.getMessage("UserNotExist"), Type = ResponseTypes.Error });
                    return Json(_responseProvider.GenerateBadRequestResponse());
                }
                
                /*                var withPic = new List<object>();
                                foreach (var res in response)
                                {
                                    if (res.UserPic == null)
                                    {
                                        withPic.Add(new { res.GroupId, res.NameFamily, res.CommentId, res.TimingString, res.Comment, res.IsDeletable });
                                    }
                                    else
                                    {
                                        withPic.Add(new { res.GroupId, res.NameFamily, res.CommentId, res.TimingString, res.Comment, res.IsDeletable , Base64UserPic = Convert.ToBase64String(res.UserPic) });
                                    }
                                }
                                return Json(_responseProvider.GenerateObjectResponse(withPic));*/
            }
            catch (Exception e)
            {
                _logProvider.Log(Tag, "GetGroupComments", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }
    }
}
