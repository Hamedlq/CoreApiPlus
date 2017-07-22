using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using CoreManager.GroupManager;
using CoreManager.LogProvider;
using CoreManager.Models;
using CoreManager.Models.RouteModels;
using CoreManager.PaymentManager;
using CoreManager.Resources;
using CoreManager.ResponseProvider;
using CoreManager.RouteManager;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace CoreApi.Controllers
{
    public class PaymentController : ApiController
    {
        private static string Tag = "PaymentController";
        private IPaymentManager _paymentManager;
        private IRouteManager _routeManager;
        private IResponseProvider _responseProvider;
        private ILogProvider _logProvider;
        private ApplicationUserManager _userAppManager;

        public PaymentController(IPaymentManager paymentManager, IResponseProvider responseProvider,
            ILogProvider logProvider,IRouteManager routeManager)
        {
            _paymentManager = paymentManager;
            _responseProvider = responseProvider;
            _logProvider = logProvider;
            _routeManager = routeManager;
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
            get { return _userAppManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
            private set { _userAppManager = value; }
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
                    _responseProvider.SetBusinessMessage(new MessageResponse()
                    {
                        Type = ResponseTypes.Error,
                        Message = getResource.getMessage("UserNotFound")
                    });
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

        [HttpGet]
        [Route("PasargadPay")]
        [AllowAnonymous]
        public HttpResponseMessage PasargadPay([FromUri] PaymentDetailModel model)
        {
            try
            {
                var res = _paymentManager.Getpayment(model.ReqId);
                StringBuilder sb = new StringBuilder();
                sb.Append("<html>");
                sb.AppendFormat(@"<body onload='document.forms[""form""].submit()'>");
                sb.AppendFormat("<form name='form' action='{0}' method='post'>", res.BankLink);
                sb.AppendFormat("<input type='hidden' name='MerchantCode' value='{0}'>", res.MerchantCode);
                sb.AppendFormat("<input type='hidden' name='TerminalCode' value='{0}'>", res.TerminalCode);
                sb.AppendFormat("<input type='hidden' name='InvoiceNumber' value='{0}'>", res.InvoiceNumber);
                sb.AppendFormat("<input type='hidden' name='InvoiceDate' value='{0}'>", res.InvoiceDate);
                sb.AppendFormat("<input type='hidden' name='Amount' value='{0}'>", res.Amount);
                sb.AppendFormat("<input type='hidden' name='RedirectAddress' value='{0}'>", res.RedirectAddress);
                sb.AppendFormat("<input type='hidden' name='TimeStamp' value='{0}'>", res.TimeStamp);
                sb.AppendFormat("<input type='hidden' name='Action' value='{0}'>", res.Action);
                sb.AppendFormat("<input type='hidden' name='Sign' value='{0}'>", res.Sign);
                sb.Append("</form>");
                sb.Append("</body>");
                sb.Append("</html>");

                return new HttpResponseMessage()
                {
                    Content = new StringContent(
                        sb.ToString(),
                        Encoding.UTF8,
                        "text/html"
                    )
                };
            }
            catch (Exception e)
            {
                _logProvider.Log(Tag, "PasargadPay", e.Message);
            }
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }


        [HttpGet]
        [Route("VerifyPasargad")]
        [AllowAnonymous]
        public HttpResponseMessage VerifyPasargad([FromUri]PasargadPaymentModel model)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("<html>");
                sb.AppendFormat(@"<body><head><meta charset=""utf - 8""></ head ><Center>");
                var res = _paymentManager.VerifyPasargadPayment(model);
                if (res.Result)
                {
                    _routeManager.ReserveSeat(model.In);
                    sb.AppendFormat("<H2 style='margin: 0px auto'>{0}</H2>", "تراکنش با موفقیت انجام شد");
                    sb.AppendFormat("<H3>{0}</H3>", res.ResultMessage);
                }
                else
                {
                    sb.AppendFormat("<H2 style='margin: 0px auto'>{0}</H2>", "تراکنش ناموفق");
                    sb.AppendFormat("<H3>{0}</H3>", res.ResultMessage);
                }
                sb.Append("</Center></body>");
                sb.Append("</html>");

                return new HttpResponseMessage()
                {
                    Content = new StringContent(
                        sb.ToString(),
                        Encoding.UTF8,
                        "text/html"
                    )
                };
                //return Json(model);
            }
            catch (Exception e)
            {
                _logProvider.Log(Tag, "VerifyPasargad", e.Message);
            }
            return new HttpResponseMessage()
            {
                Content = new StringContent(
                        "خطای سرور- لطفا با پشتیبانی تماس بگیرید",
                        Encoding.UTF8,
                        "text/html"
                    )
            };
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