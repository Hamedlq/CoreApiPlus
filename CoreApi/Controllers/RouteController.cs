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
using CoreManager.LogProvider;
using CoreManager.Models;
using CoreManager.Models.RouteModels;
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
    public class RouteController : ApiController
    {
        private static string Tag = "RouteController";
        private IRouteManager _routemanager;
        private IRouteGroupManager _routeGroupManager;
        private ILogProvider _logmanager;
        private IResponseProvider _responseProvider;


        public RouteController()
        {
        }
        public RouteController(IRouteManager routeManager,
            ILogProvider logManager,
            IRouteGroupManager routeGroupManager,
            IResponseProvider responseProvider)
        {
            _routemanager = routeManager;
            _logmanager = logManager;
            _routeGroupManager = routeGroupManager;
            _responseProvider = responseProvider;
        }
        [HttpPost]
        [Route("InsertUserRoute")]
        public IHttpActionResult InsertUserRoute(RouteRequestModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(_responseProvider.GenerateBadRequestResponse(ModelState));
                }
                var routeIds = _routemanager.InsertUserRoute(model, int.Parse(User.Identity.GetUserId()));
                var confirmMessage = _routemanager.GetRouteConfirmationMessage(int.Parse(User.Identity.GetUserId()), routeIds);
                string commaSepratedRouteIds = "";
                foreach (var routeId in routeIds)
                {
                    commaSepratedRouteIds += routeId + ",";
                }
                ResponseModel responseModel = new ResponseModel() { Status = HttpStatusCode.OK.ToString(), StatusCode = HttpStatusCode.OK, Count = 1, Type = ResponseTypes.ConfirmMessage.ToString(), Messages = new List<string>() { commaSepratedRouteIds, confirmMessage } };
                return Json(responseModel);
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "InsertUserRoute", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }
        [HttpPost]
        [Route("InsertUserEventRoute")]
        public IHttpActionResult InsertUserEventRoute(RouteEventModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(_responseProvider.GenerateBadRequestResponse(ModelState));
                }
                _routemanager.InsertUserEventRoute(model, int.Parse(User.Identity.GetUserId()));
                ResponseModel responseModel = _responseProvider.GenerateOKResponse();
                return Json(responseModel);
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "InsertUserEventRoute", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }
        [HttpPost]
        [Route("InsertEmployeeRoute")]
        [AllowAnonymous]
        public IHttpActionResult InsertEmployeeRoute(EmployeeRequestModels model)
        {
            try
            {
                var userId = _routemanager.InsertEmployeeModel(model);
                ResponseModel responseModel = _responseProvider.GenerateOKResponse();
                return Json(responseModel);
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "InsertUserRoute", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }
        [HttpPost]
        [Route("InsertEventAttendee")]
        [AllowAnonymous]
        public IHttpActionResult InsertEventAttendee(EventAttendeeModel model)
        {
            try
            {
                var userId = _routemanager.InsertEventAttendeeModel(model);
                ResponseModel responseModel = _responseProvider.GenerateOKResponse();
                return Json(responseModel);
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "InsertUserRoute", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }
        [HttpPost]
        [Route("ValidateTiming")]
        public IHttpActionResult ValidateTiming(TimingModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(_responseProvider.GenerateBadRequestResponse(ModelState));
                }
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "ValidateTiming", e.Message);
            }
            return Json(new ResponseModel());
        }

        [HttpPost]
        [Route("ConfirmRoute")]
        public IHttpActionResult ConfirmRoute(ConfirmationModel model)
        {
            ResponseModel responseModel;
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(_responseProvider.GenerateBadRequestResponse(ModelState));
                }
                var res = _routemanager.ConfirmRoute(model, int.Parse(User.Identity.GetUserId()));
                return Json(_responseProvider.GenerateResponse(new List<string> { res }, "AcceptSuggestedRoute"));
                //responseModel = new ResponseModel() { Status = HttpStatusCode.OK.ToString(), StatusCode = HttpStatusCode.OK, Count = 0, Type = ResponseTypes.ConfirmMessage.ToString() };
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "ConfirmRoute", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("NotConfirmRoute")]
        public IHttpActionResult NotConfirmRoute(ConfirmationModel model)
        {
            ResponseModel responseModel;
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(_responseProvider.GenerateBadRequestResponse(ModelState));
                }
                foreach (var routeId in model.RouteIds)
                {
                    _routemanager.DeleteRoute(int.Parse(User.Identity.GetUserId()), routeId);
                }
                return Json(_responseProvider.GenerateOKResponse());
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "ConfirmRoute", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }
        [HttpPost]
        [Route("GetUserRoutes")]

        public IHttpActionResult GetUserRoutes()
        {
            try
            {
                var list = _routemanager.GetUserRoutes(int.Parse(User.Identity.GetUserId()));
                //پیغام اطلاعات خودرو
                /*var drivings = list.Where(x => x.IsDrive).ToList();
                if (drivings.Count>0)
                {
                    if (drivings.Any(x => x.CarString == ""))
                    {
                        _responseProvider.SetBusinessMessage(new MessageResponse() { Type = ResponseTypes.Info,Message = getResource.getMessage("FillCarInfo") });
                    }
                }*/
                return Json(_responseProvider.GenerateRouteResponse(list));
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "GetUserRoutes", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }
        [HttpPost]
        [Route("GetUserWeekRoutes")]

        public IHttpActionResult GetUserWeekRoutes()
        {
            try
            {
                var list = _routemanager.GetUserWeekRoutes(int.Parse(User.Identity.GetUserId()));
                return Json(_responseProvider.GenerateRouteResponse(list));
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "GetUserWeekRoutes", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }
        [HttpPost]
        [Route("GetAllRoutes")]
        [Authorize(Roles = "AdminUser")]
        public IHttpActionResult GetAllRoutes()
        {
            try
            {
                var list = _routemanager.GetAllRoutes();
                return Json(_responseProvider.GenerateRouteResponse(list));
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "GetAllRoutes", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }
        [HttpPost]
        [Route("GetUserRoutesBymobile")]
        [Authorize(Roles = "AdminUser")]
        public IHttpActionResult GetUserRoutesBymobile(UserSearchModel model)
        {
            try
            {
                var list = _routemanager.GetUserRoutesByMobile(model.Mobile);
                return Json(_responseProvider.GenerateRouteResponse(list));
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "GetUserRoutes", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }
        [HttpPost]
        [Route("GetUserSuggestRoute")]
        [Authorize(Roles = "AdminUser")]
        public IHttpActionResult GetUserSuggestRoute(RouteRequestModel model)
        {
            try
            {
                var res = _routemanager.GetUserSuggestRouteByRouteId(model.RouteRequestId);

                return Json(_responseProvider.GenerateRouteResponse(res));
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "GetUserSuggestRoute", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }
        [HttpPost]
        [Route("GetSuggestRoute")]
        public IHttpActionResult GetSuggestRoute(RouteRequestModel model)
        {
            try
            {
                var res = _routemanager.GetSuggestRouteByRouteId(int.Parse(User.Identity.GetUserId()), model.RouteRequestId);

                return Json(_responseProvider.GenerateRouteResponse(res));
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "GetSuggestRoute", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("GetSuggestWeekRoute")]
        public IHttpActionResult GetSuggestWeekRoute(RouteRequestModel model)
        {
            try
            {
                var res = _routemanager.GetSuggestWeekRouteByRouteId(int.Parse(User.Identity.GetUserId()), model.RouteRequestId);

                return Json(_responseProvider.GenerateRouteResponse(res));
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "GetSuggestWeekRoute", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("CalcRoutes")]
        public IHttpActionResult CalcRoutes()
        {
            try
            {
                _routemanager.DoCalc();
                return Json(_responseProvider.GenerateOKResponse());
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "CalcRoutes", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("GetAcceptedSuggestRoute")]
        public IHttpActionResult GetAcceptedSuggestRoute(ContactModel model)
        {
            try
            {
                var res = _routemanager.GetAcceptedSuggestRouteByContactId(int.Parse(User.Identity.GetUserId()), model.ContactId);
                return Json(_responseProvider.GenerateRouteResponse(res));
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "GetAcceptedSuggestRoute", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("GetSimilarSuggestRoute")]
        public IHttpActionResult GetSimilarSuggestRoute(ContactModel model)
        {
            try
            {
                var res = _routemanager.GetSimilarSuggestRouteByContactId(int.Parse(User.Identity.GetUserId()), model.ContactId);

                return Json(_responseProvider.GenerateRouteResponse(res));
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "GetAcceptedSuggestRoute", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }



        /*[HttpPost]
        [Route("GetRouteSuggests")]
        public IHttpActionResult GetRouteSuggests()
        {
            try
            {
                var list = _routemanager.GetRouteSuggests(int.Parse(User.Identity.GetUserId()));
                return Json(_responseProvider.GenerateRouteSuggestResponse(list));
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "GetRouteSuggests", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }*/
        [HttpPost]
        [Route("AcceptSuggestedRoute")]
        public IHttpActionResult AcceptSuggestedRoute(RouteSuggestRequestModel model)
        {
            try
            {
                var res = _routemanager.AcceptSuggestedRoute(int.Parse(User.Identity.GetUserId()), model.RouteId, model.SelfRouteId);
                return Json(_responseProvider.GenerateResponse(new List<string> { res }, "AcceptSuggestedRoute"));
                //return Json(_responseProvider.GenerateSuggestAcceptResponse(res));
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "AcceptSuggestedRoute", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }
        [HttpPost]
        [Route("RequestRideShare")]

        public IHttpActionResult RequestRideShare(RouteSuggestRequestModel model)
        {
            try
            {
                var res = _routemanager.RequestRideShare(int.Parse(User.Identity.GetUserId()), model.RouteId, model.SelfRouteId);
                var theContact = _routemanager.GetContactByRoutes(model.RouteId, model.SelfRouteId);
                //return Json(theContact);
                return Json(_responseProvider.GenerateResponse(theContact, "sharedContact"));
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "RequestRideShare", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("GetUserScoresByRoute")]
        [Authorize]
        public IHttpActionResult GetUserScoresByRoute(RouteRequestModel model)
        {
            var res = _routemanager.GetUserScoresByRouteId(int.Parse(User.Identity.GetUserId()), model.RouteRequestId);
            return Json(res);
        }

        [HttpPost]
        [Route("JoinGroup")]

        public IHttpActionResult JoinGroup(RouteGroupModel model)
        {
            try
            {
                var res = _routemanager.JoinGroup(int.Parse(User.Identity.GetUserId()), model.RouteId, model.GroupId);

                return Json(_responseProvider.GenerateResponse(new List<string> { res }, "JoinGroup"));
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "JoinGroup", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }
        [HttpPost]
        [Route("LeaveGroup")]

        public IHttpActionResult LeaveGroup(RouteGroupModel model)
        {
            try
            {
                var res = _routemanager.LeaveGroup(int.Parse(User.Identity.GetUserId()), model.RouteId, model.GroupId);

                return Json(_responseProvider.GenerateResponse(new List<string>() { res }, "leavegroup"));
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "LeaveGroup", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }
        [HttpPost]
        [Route("DeleteRoute")]

        public IHttpActionResult DeleteRoute(RouteRequestModel model)
        {
            try
            {
                var res = _routemanager.DeleteRoute(int.Parse(User.Identity.GetUserId()), model.RouteRequestId);
                return Json(_responseProvider.GenerateResponse(new List<string> { res }, "DeleteRoute"));
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "DeleteRoute", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }
        [HttpPost]
        [Route("DeleteRouteSuggest")]

        public IHttpActionResult DeleteRouteSuggest(RouteSuggestRequestModel model)
        {
            try
            {
                var res = _routemanager.DeleteRouteSuggest(int.Parse(User.Identity.GetUserId()), model.SelfRouteId, model.RouteId);
                return Json(_responseProvider.GenerateResponse(new List<string> { res }, "DeleteRouteSuggest"));
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "DeleteRouteSuggest", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }
        [HttpPost]
        [Route("DeleteGroupSuggest")]

        public IHttpActionResult DeleteGroupSuggest(RouteGroupModel model)
        {
            try
            {
                var res = _routemanager.DeleteGroupSuggest(int.Parse(User.Identity.GetUserId()), model.RouteId, model.GroupId);

                return Json(_responseProvider.GenerateResponse(new List<string> { res }, "JoinGroup"));
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "DeleteGroupSuggest", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }
        [HttpPost]
        [Route("ShareRoute")]

        public IHttpActionResult ShareRoute(RouteRequestModel model)
        {
            try
            {
                var res = _routemanager.ShareRoute(int.Parse(User.Identity.GetUserId()), model.RouteRequestId);
                return Json(_responseProvider.GenerateResponse(res, "ShareRoute"));
                //return Json(res);
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "ShareRoute", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }
        [HttpPost]
        [Route("GetMapImageById")]
        [AllowAnonymous]
        public IHttpActionResult GetMapImageById(ImageRequest model)
        {
            if (model != null && User != null)
            {
                var res = _routemanager.GetMapImageById(model);
                if (res.ImageFile != null)
                {
                    return Json(new { res.ImageId, res.ImageType, Base64ImageFile = Convert.ToBase64String(res.ImageFile) });
                }
            }
            return Json(_responseProvider.GenerateBadRequestResponse());
        }
        [HttpPost]
        [Route("GetAllEvents")]
        [AllowAnonymous]
        public IHttpActionResult GetAllEvents()
        {
            try
            {
                var response = _routemanager.GetAllEvents();
                return Json(_responseProvider.GenerateEventResponse(response));
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "GetAllEvents", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("GetCityLocations")]
        [AllowAnonymous]
        public IHttpActionResult GetCityLocations(Point point)
        {
            try
            {
                var response = _routemanager.GetCityLocations(point);
                return Json(_responseProvider.GenerateCityLocResponse(response));
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "GetCityLocations", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }
        [HttpPost]
        [Route("GetRouteRecommends")]
        [AllowAnonymous]
        public IHttpActionResult GetRouteRecommends(SrcDstModel model)
        {
            try
            {
                var wayPoints = JsonConvert.DeserializeObject<List<Point>>(model.WayPoints);
                var res = _routemanager.GetRouteRecommends(model, wayPoints);
                return Json(_responseProvider.GenerateRecommendRoutesResponse(res));
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "GetCityLocations", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }
        [HttpPost]
        [Route("GetLocalRoutes")]
        [AllowAnonymous]
        public IHttpActionResult GetLocalRoutes(Point point)
        {
            try
            {
                var response = _routemanager.GetLocaRoutes(point);
                return Json(_responseProvider.GenerateRouteResponse(response));
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "GetCityLocations", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }
        [HttpPost]
        [Route("GetRouteInfo")]
        public IHttpActionResult GetRouteInfo(LocalRouteModel localRouteModel)
        {
            try
            {
                var response = _routemanager.GetRouteInfo(int.Parse(User.Identity.GetUserId()), localRouteModel.RouteUId);
                return Json(_responseProvider.GenerateResponse(response, "LocalRouteUser"));
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "GetRouteInfo", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }
        [HttpPost]
        [Route("InsertRideRequest")]
        public IHttpActionResult InsertRideRequest(RouteRequestModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(_responseProvider.GenerateBadRequestResponse(ModelState));
                }
                //var routeIds = _routemanager.InsertUserRoute(model, int.Parse(User.Identity.GetUserId()));
                _routemanager.InsertRideRequest(model, int.Parse(User.Identity.GetUserId()));
                //var confirmMessage = _routemanager.GetRouteConfirmationMessage(int.Parse(User.Identity.GetUserId()), routeIds);
                /*string commaSepratedRouteIds = "";
                foreach (var routeId in routeIds)
                {
                    commaSepratedRouteIds += routeId + ",";
                }*/
                //ResponseModel responseModel = new ResponseModel() { Status = HttpStatusCode.OK.ToString(), StatusCode = HttpStatusCode.OK, Count = 1, Type = ResponseTypes.ConfirmMessage.ToString(), Messages = new List<string>() { commaSepratedRouteIds, confirmMessage } };
                ResponseModel responseModel = _responseProvider.GenerateOKResponse();
                return Json(responseModel);
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "InsertUserRoute", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }


        [HttpPost]
        [Route("GetPrice")]
        [AllowAnonymous]
        public IHttpActionResult GetPrice(SrcDstModel model)
        {
            try
            {
                var routePrice = _routemanager.GetPrice(model);
                ResponseModel responseModel = _responseProvider.GenerateResponse(new List<string>() { routePrice }, "price");
                return Json(responseModel);
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "GetRouteAndPrice", e.Message);
            }
            return null;
        }

        [HttpPost]
        [Route("InsertEvent")]
        [AllowAnonymous]
        public IHttpActionResult InsertEvent(EventRequestModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(_responseProvider.GenerateBadRequestResponse(ModelState));
                }
                _routemanager.InsertEvent(model);

                ResponseModel responseModel = _responseProvider.GenerateOKResponse();
                return Json(responseModel);
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "InsertEvent", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }

        [HttpPost]
        [Route("GetTripProfile")]
        [AllowAnonymous]
        public IHttpActionResult GetTripProfile(RouteRequestModel model)
        {
            try
            {
                //var q=_routemanager.GetTripProfile(model.RouteRequestId, int.Parse(User.Identity.GetUserId()));
                //return Json(q);
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "GetTripProfile", e.Message);
            }
            return Json(_responseProvider.GenerateUnknownErrorResponse());
        }
    }
}
