using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using System.Web.Http.Results;
using System.Web.Script.Serialization;
using CoreManager.LogProvider;
using CoreManager.Models;
using CoreManager.Models.RouteModels;
using CoreManager.NotificationManager;
using CoreManager.Resources;
using CoreManager.ResponseProvider;
using CoreManager.RouteGroupManager;
using CoreManager.RouteManager;
using CoreManager.UserManager;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CoreApi.Controllers
{
    [Authorize]
    public class PassengerController : ApiController
    {
        private static string Tag = "PassengerController";
        private IRouteManager _routemanager;
        private IRouteGroupManager _routeGroupManager;
        private ILogProvider _logmanager;
        private IResponseProvider _responseProvider;
        private IUserManager _userManager;
        private INotificationManager _notificationManager;

        public PassengerController()
        {
        }

        public PassengerController(IRouteManager routeManager,
            ILogProvider logManager,
            IRouteGroupManager routeGroupManager,
            INotificationManager notificationManager,
            IUserManager userManager,
            IResponseProvider responseProvider)
        {
            _routemanager = routeManager;
            _logmanager = logManager;
            _routeGroupManager = routeGroupManager;
            _responseProvider = responseProvider;
            _userManager = userManager;
            _notificationManager = notificationManager;
        }


        [HttpPost]
        [Route("GetPassengerRoutes")]
        public IHttpActionResult GetPassengerRoutes(PassFilterModel model)
        {
            try
            {
                int ff;
                if (User != null && int.TryParse(User.Identity.GetUserId(), out ff))
                {
                    var res = _routemanager.GetPassengerRoutes(ff, model);
                    return Json(_responseProvider.GenerateRouteResponse(res));
                }
                else
                {
                    return
                        ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.Unauthorized,
                            "You are unauthorized to see Info!"));
                }
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    _logmanager.Log(Tag, "GetPassengerRoutes", e.Message + "-" + e.InnerException.Message);
                }
                else
                {
                    _logmanager.Log(Tag, "GetPassengerRoutes", e.Message);
                }
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("GetPassengerTrip")]
        public IHttpActionResult GetPassengerTrip(PassFilterModel model)
        {
            try
            {
                int ff;
                if (User != null && int.TryParse(User.Identity.GetUserId(), out ff))
                {
                    var res = _routemanager.GetPassengerTrip(ff, model.FilteringId);
                    return Json(_responseProvider.GenerateRouteResponse(res, "PassengerTrip"));
                }
                else
                {
                    return
                        ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.Unauthorized,
                            "You are unauthorized to see Info!"));
                }
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    _logmanager.Log(Tag, "GetPassengerTrip", e.Message + "-" + e.InnerException.Message);
                }
                else
                {
                    _logmanager.Log(Tag, "GetPassengerTrip", e.Message);
                }
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("GetPassengerStRoute")]
        [AllowAnonymous]
        public IHttpActionResult GetPassengerStRoute()
        {
            try
            {
                var res = _routemanager.GetPassengerStationRoutes();
                return Json(_responseProvider.GenerateRouteResponse(res));
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "GetStationRoutes", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

/*
                                                                [HttpPost]
                                                                [Route("RequestBooking")]
                                                                public IHttpActionResult RequestBooking(PassRouteModel model)
                                                                {
                                                                    try
                                                                    {
                                                                        int ff;
                                                                        if (User != null && int.TryParse(User.Identity.GetUserId(), out ff))
                                                                        {
                                                                            var res = _routemanager.RequestBooking(ff, model.TripId);
                                                                            return Json(res);
                                                                        }
                                                                        else
                                                                        {
                                                                            return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "You are unauthorized to see Info!"));
                                                                        }
                                                                    }
                                                                    catch (Exception e)
                                                                    {
                                                                        _logmanager.Log(Tag, "RequestBooking", e.Message);
                                                                    }
                                                                    return Json(_responseProvider.GenerateUnknownErrorResponse());
                                                                }
                                                        */

        [HttpPost]
        [Route("RequestPayBooking")]
        public IHttpActionResult RequestPayBooking(PayModel model)
        {
            try
            {
                int ff;
                if (User != null && int.TryParse(User.Identity.GetUserId(), out ff))
                {
                    if (_routemanager.IsPayValid(ff, model))
                    {
                        if (_routemanager.HasCapacity(model))
                        {
                            if (!_routemanager.HasReserved(model, ff))
                            {
                                if (model.ChargeAmount > 0)
                                {
                                    //gotobank
                                    var res = _routemanager.RequestPayBooking(ff, model.TripId, model.ChargeAmount);
                                    return Json(res);
                                    /*var res = _routemanager.RequestBooking(ff, model.TripId, model.ChargeAmount);
                                    return Json(res);*/
                                }
                                else
                                {
                                    var res1 = _routemanager.BookSeat(ff, model);
                                    return Json(res1);
                                }
                            }
                            else
                            {
                                _responseProvider.SetBusinessMessage(new MessageResponse()
                                {
                                    Type = ResponseTypes.Error,
                                    Message = getResource.getMessage("SeatPreviouslyReserved")
                                });
                                return Json(_responseProvider.GenerateBadRequestResponse());
                            }
                        }
                        else
                        {
                            _responseProvider.SetBusinessMessage(new MessageResponse()
                            {
                                Type = ResponseTypes.Error,
                                Message = getResource.getMessage("NoEmptySeat")
                            });
                            return Json(_responseProvider.GenerateBadRequestResponse());
                        }
                    }
                    else
                    {
                        _responseProvider.SetBusinessMessage(new MessageResponse()
                        {
                            Type = ResponseTypes.Error,
                            Message = getResource.getMessage("PricesUnDefined")
                        });
                        return Json(_responseProvider.GenerateBadRequestResponse());
                    }
                }
                else
                {
                    return
                        ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.Unauthorized,
                            "You are unauthorized to see Info!"));
                }
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "RequestPayBooking", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }


        [HttpPost]
        [Route("SetPassLocation")]
        [Authorize]
        public IHttpActionResult SetPassLocation(PassRouteModel model)
        {
            try
            {
                int ff;
                int.TryParse(User.Identity.GetUserId(), out ff);
                var res = _routemanager.SetPassLocation(ff, model);
                return Json(_responseProvider.GenerateRouteResponse(res, "SetPassLocation"));
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "SetPassLocation", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("SetFilter")]
        [Authorize]
        public IHttpActionResult SetFilter(FilterModel model)
        {
            try
            {
                int ff;
                int.TryParse(User.Identity.GetUserId(), out ff);
                var res = _routemanager.SetFilter(ff, model);
                return Json(_responseProvider.GenerateRouteResponse(res, "SetFilter"));
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    _logmanager.Log(Tag, "SetFilter", e.Message + "-" + e.InnerException.Message);
                }
                else
                {
                    _logmanager.Log(Tag, "SetFilter", e.Message);
                }
                //_logmanager.Log(Tag, "SetFilter", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("GetFilters")]
        [Authorize]
        public IHttpActionResult GetFilters()
        {
            try
            {
                int ff;
                int.TryParse(User.Identity.GetUserId(), out ff);
                var res = _routemanager.GetUserFilters(ff);
                return Json(_responseProvider.GenerateRouteResponse(res));
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "GetFilters", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("DeleteFilter")]
        [Authorize]
        public IHttpActionResult DeleteFilter(FilterModel model)
        {
            try
            {
                int ff;
                int.TryParse(User.Identity.GetUserId(), out ff);
                var res = _routemanager.DeleteFilter(ff, model);
                return Json(_responseProvider.GenerateRouteResponse(res, "DeleteFilter"));
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "DeleteFilter", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("CancelFilter")]
        [Authorize]
        public IHttpActionResult CancelFilter(FilterModel model)
        {
            try
            {
                int ff;
                int.TryParse(User.Identity.GetUserId(), out ff);
                var res = _routemanager.CancelFilter(ff, model);
                return Json(_responseProvider.GenerateRouteResponse(res, "CancelFilter"));
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "CancelFilter", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("GetTimes")]
        [Authorize]
        public IHttpActionResult GetTimes(FilterModel model)
        {
            try
            {
                int ff;
                int.TryParse(User.Identity.GetUserId(), out ff);
                var res = _routemanager.GetFilterTimes(ff,model);
                return Json(_responseProvider.GenerateRouteResponse(res));
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "GetTimes", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("GetPassengerInvite")]
        public IHttpActionResult GetPassengerInvite()
        {
            try
            {
                var res = _userManager.GetUserInvite(int.Parse(User.Identity.GetUserId()), InviteTypes.PassInvite);
                //var jsonRes = Json(_responseProvider.GenerateRouteResponse(res,"AboutMe"));
                return Json(res);
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "GetPassengerInvite", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("GetPassengerScores")]
        [Authorize]
        public IHttpActionResult GetPassengerScores(PayModel paymodel)
        {
            var res = _userManager.GetPassScores(int.Parse(User.Identity.GetUserId()), paymodel);
            return Json(res);
        }

        [HttpPost]
        [Route("CancelBooking")]
        [Authorize]
        public IHttpActionResult CancelBooking(PassRouteModel model)
        {
            var res = _routemanager.CancelBooking(int.Parse(User.Identity.GetUserId()), model.TripId);
            return Json(res.IsSubmited);
        }

        [HttpPost]
        [Route("GetIosVersion")]
        [AllowAnonymous]
        public IHttpActionResult GetIosVersion([FromUri]int version)
        {//سپند استفاده میکند اما نمیدونم واسه کجا؟
            var _appVersion = int.Parse(ConfigurationManager.AppSettings["MobileIosVersion"]);
            var _appCriticalVersion = int.Parse(ConfigurationManager.AppSettings["MobileCriticalIosVersion"]);
            if (version < _appVersion)
            {
                return Json(2);
            }
            else if(version < _appCriticalVersion)
            {
                return Json(3);
            }
            else
            {
                return Json(1);
            }
        }

        [HttpGet]
        [Route("IosVersion")]
        [AllowAnonymous]
        public IHttpActionResult IosVersion()
        {//آرش استفاه میکند
            try
            {
                IosVersionModel v = new IosVersionModel();
                v.UrlCode = 1;
                v.VersionCode = 2;
                v.NewUrl = "https://new.sibapp.com/applications/mibarimapp";
                return Json(v);
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    _logmanager.Log(Tag, "IosVersion", e.Message + "-" + e.InnerException.Message);
                }
                else
                {
                    _logmanager.Log(Tag, "IosVersion", e.Message);
                }
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("SubmitPassDiscount")]
        public IHttpActionResult SubmitPassDiscount(DiscountModel model)
        {
            try
            {
                if (model == null)
                {
                    _responseProvider.SetBusinessMessage(new MessageResponse()
                    {
                        Type = ResponseTypes.Error,
                        Message =
                            string.Format(getResource.getMessage("Required"), getResource.getString("VerificationCode"))
                    });
                    return Json(_responseProvider.GenerateBadRequestResponse());
                }
                var res = _userManager.DoDiscount(InviteTypes.PassInvite, model.DiscountCode,
                    int.Parse(User.Identity.GetUserId()));
                if (res)
                {
                    return
                        Json(_responseProvider.GenerateResponse(getResource.getMessage("CodeSubmitted"), "CodeSubmitted"));
                }
                else
                {
                    return Json(_responseProvider.GenerateBadRequestResponse());
                }
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
                }*/
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "SubmitPassDiscount", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }


        [HttpPost]
        [Authorize(Roles = "AdminUser")]
        [Route("GetPassengers")]
        public IHttpActionResult GetPassengers()
        {
            try
            {
                var res = _userManager.GetPassengers();
                return Json(_responseProvider.GenerateRouteResponse(res, "ActiveTrips"));
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "GetPassengers", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpGet]
        [Route("RequestInvoice")]
        [AllowAnonymous]
        public IHttpActionResult RequestInvoice([FromUri]PayModel model)
        {
            try
            {
                if (model.TripId == 90356)
                {

                    //gotobank
                    var res = _routemanager.RequestInvoice(12325, model.ChargeAmount);
                    return Json(res);
                }
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "RequestInvoice", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }



        /*[HttpGet]
        [AllowAnonymous]
        [Route("GetPassengers")]
        public HttpResponseMessage GetPassengers([FromUri] string id)
        {
            var show = "هیچی";
            try
            {
                if (id == "miba")
                {
                    var res = _userManager.GetPassengers();
                    show = "اسم راننده" + " | " + "شماره راننده" + " | " + "ساعت سفر" + " | " + "مبدا" + " | " + "مقصد" +
                           " | " + "صندلی خالی" + " | " + "اسم مسافر" + " | " + "شماره مسافر" + "\n</br>";
                    foreach (var passRouteModel in res)
                    {
                        show += passRouteModel.CarString + " | " + passRouteModel.CarPlate + " | " +
                                passRouteModel.TimingString + " | " + passRouteModel.SrcAddress + " | " +
                                passRouteModel.DstAddress + " | " + passRouteModel.EmptySeats + " | " +
                                passRouteModel.Name +
                                " " + passRouteModel.Family + " | " +
                                passRouteModel.MobileNo + "\n</br>";
                    }
                }
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "GetPassengers", e.Message);
            }
            return new HttpResponseMessage()
            {
                Content = new StringContent(
                    show,
                    Encoding.UTF8,
                    "text/html"
                )
            };
        }*/
    }
}