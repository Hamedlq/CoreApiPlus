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
using CoreManager.PaymentManager;
using CoreManager.Resources;
using CoreManager.ResponseProvider;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace CoreApi.Controllers
{
    public class PaymentController : ApiController
    {
        private static string Tag = "PaymentController";
        private IPaymentManager _paymentManager;
        private IResponseProvider _responseProvider;
        private ILogProvider _logProvider;
        private ApplicationUserManager _userAppManager;
        public PaymentController(IPaymentManager paymentManager, IResponseProvider responseProvider, ILogProvider logProvider)
        {
            _paymentManager = paymentManager;
            _responseProvider = responseProvider;
            _logProvider = logProvider;
        }

        public PaymentController(ApplicationUserManager appUserManager)
        {
            AppUserManager = appUserManager;
        }

        public PaymentController()
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

        [HttpPost]
        [Route("ChargeUserAccount")]
        [AllowAnonymous]
        public IHttpActionResult ChargeUserAccount(ChargeModel model)
        {
            try
            {
                var user = AppUserManager.FindByName(model.MobileNo);
                if (user == null)
                {
                    _responseProvider.SetBusinessMessage(new MessageResponse() { Type = ResponseTypes.Error, Message = getResource.getMessage("UserNotFound") });
                    return Json(_responseProvider.GenerateBadRequestResponse());
                }
                else
                {
                    var res = _paymentManager.ChargeAccount(user.Id, model.ChargeValue, GetUserNameFamilyString(user));
                    return Json(res);
                }
            }
            catch (Exception e)
            {
                _logProvider.Log(Tag, "ChargeUserAccount", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }
        [HttpPost]
        [Route("VerifyBankTransaction")]
        [AllowAnonymous]
        public IHttpActionResult VerifyBankTransaction(PaymentDetailModel model)
        {
            try
            {
                    var res = _paymentManager.VerifyPayment(model);
                    return Json(res);
            }
            catch (Exception e)
            {
                _logProvider.Log(Tag, "VerifyBankTransaction", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        private string GetUserNameFamilyString(ApplicationUser user)
        {
            var res = "";
            if (user.Gender == Gender.Man)
            {
                res = " آقای ";
            }
            else if (user.Gender == Gender.Woman)
            {
                res = " خانم ";
            }
            else
            {
                res = "";
            }
            return res + user.Name + " " + user.Family;
        }
    }
}
