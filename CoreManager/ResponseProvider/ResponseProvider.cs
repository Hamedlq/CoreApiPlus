using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Http.ModelBinding;
using System.Web.Routing;
using System.Web.Script.Serialization;
using CoreManager.Models;
using CoreManager.Models.TrafficAddress;
using CoreManager.Resources;
using Newtonsoft.Json;

namespace CoreManager.ResponseProvider
{
    public class ResponseProvider : IResponseProvider
    {

        private List<MessageResponse> _errors;
        private List<MessageResponse> _infos;
        private List<MessageResponse> _warnings;
        public ResponseProvider()
        {
            _errors = new List<MessageResponse>();
            _infos = new List<MessageResponse>();
            _warnings = new List<MessageResponse>();
        }

        public void SetBusinessMessage(MessageResponse model)
        {
            switch (model.Type)
            {
                case ResponseTypes.Error:
                    _errors.Add(model);
                    break;
                case ResponseTypes.Info:
                    _infos.Add(model);
                    break;
                case ResponseTypes.Warning:
                    _warnings.Add(model);
                    break;
            }
        }
        public ResponseModel GenerateOKResponse()
        {
            var responseModel = new ResponseModel() { Status = HttpStatusCode.OK.ToString(), StatusCode = HttpStatusCode.OK, Count = 0, Type = "Success", Errors = _errors };
            return responseModel;
        }
        public ResponseModel GenerateBadRequestResponse()
        {
            var responseModel = new ResponseModel() { Status = HttpStatusCode.BadRequest.ToString(), StatusCode = HttpStatusCode.BadRequest, Count = _errors.Count, Type = "Error", Errors = _errors,Warnings = _warnings};
            return responseModel;
        }


        public ResponseModel GenerateBadRequestResponse(ModelStateDictionary modelState)
        {
            var errorStrings = modelState.Values.SelectMany(m => m.Errors)
             .Select(e => e.ErrorMessage)
             .ToList();
            var errors = errorStrings.Select(x => new MessageResponse() { Message = x }).ToList();
            var responseModel = new ResponseModel() { Status = HttpStatusCode.BadRequest.ToString(), StatusCode = HttpStatusCode.BadRequest, Count = modelState.Count, Type = "Error", Errors = errors };
            return responseModel;
        }
        public ResponseModel GenerateUnknownErrorResponse()
        {
            var responseModel = new ResponseModel() { Status = HttpStatusCode.BadRequest.ToString(), StatusCode = HttpStatusCode.BadRequest, Count = 0, Type = "UnknownError", Errors = _errors };
            return responseModel;
        }
        public ResponseModel GenerateInternalServerErrorResponse()
        {
            var responseModel = new ResponseModel() { Status = HttpStatusCode.InternalServerError.ToString(), StatusCode = HttpStatusCode.InternalServerError, Count = 0, Type = "InternalServerError" };
            return responseModel;
        }

        public ResponseModel GenerateResponse(object obj, string type)
        {
            var responseModel = new ResponseModel() { Status = HttpStatusCode.OK.ToString(), StatusCode = HttpStatusCode.OK, Count = 1, Type = type, Messages = new List<string> { Json.Encode(obj) },Errors = _errors};
            return responseModel;
        }


        //public string GenerateInvalidUserPassResponse()
        //{
        //    var responseModel = new ResponseModel() { Status = HttpStatusCode.Forbidden.ToString(), StatusCode = HttpStatusCode.Forbidden, Count = 1, Type = "InvalidUserPass", Errors = new List<string>() { getResource.getMessage("InvalidUserPass") } };
        //    return Json.Encode(responseModel);
        //}

        public ResponseModel GenerateRouteResponse(List<RouteResponseModel> list)
        {
            var responseModel = new ResponseModel() { Status = HttpStatusCode.OK.ToString(), StatusCode = HttpStatusCode.OK, Count = list.Count, Type = "RouteResponse", Messages = list.Select(x => Json.Encode(x)).ToList(), Warnings = _warnings, Infos = _infos };
            return responseModel;
        }
        public ResponseModel GenerateRouteResponse(List<StationRouteModel> list)
        {
            var responseModel = new ResponseModel() { Status = HttpStatusCode.OK.ToString(), StatusCode = HttpStatusCode.OK, Count = list.Count, Type = "RouteResponse", Messages = list.Select(x => Json.Encode(x)).ToList(), Warnings = _warnings, Infos = _infos };
            return responseModel;
        }
        public ResponseModel GenerateRouteResponse(List<SubStationModel> list)
        {
            var responseModel = new ResponseModel() { Status = HttpStatusCode.OK.ToString(), StatusCode = HttpStatusCode.OK, Count = list.Count, Type = "SubStationModel", Messages = list.Select(x => Json.Encode(x)).ToList(), Warnings = _warnings, Infos = _infos };
            return responseModel;
        }
        public ResponseModel GenerateRouteResponse(List<PersoanlInfoModel> list)
        {
            //Working solution
            var serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue, RecursionLimit = 100 };

            var responseModel = new ResponseModel() { Status = HttpStatusCode.OK.ToString(), StatusCode = HttpStatusCode.OK, Count = list.Count, Type = "RouteResponse", Messages = list.Select(x => serializer.Serialize(x)).ToList(), Warnings = _warnings, Infos = _infos };
            return responseModel;
        }
        public ResponseModel GenerateRouteResponse(List<PassRouteModel> list)
        {
            //Working solution
            var serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue, RecursionLimit = 100 };
            var responseModel = new ResponseModel() { Status = HttpStatusCode.OK.ToString(), StatusCode = HttpStatusCode.OK, Count = list.Count, Type = "PassRouteModel", Messages = list.Select(x => serializer.Serialize(x)).ToList(), Warnings = _warnings, Infos = _infos };
            return responseModel;
        }

        public ResponseModel GenerateRouteResponse(List<DriverRouteModel> list)
        {
            //Working solution
            var serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue, RecursionLimit = 100 };
            var responseModel = new ResponseModel() { Status = HttpStatusCode.OK.ToString(), StatusCode = HttpStatusCode.OK, Count = list.Count, Type = "PassRouteModel", Messages = list.Select(x => serializer.Serialize(x)).ToList(), Warnings = _warnings, Infos = _infos };
            return responseModel;

        }

        public ResponseModel GenerateRouteResponse(List<BriefRouteModel> list)
        {
            var responseModel = new ResponseModel() { Status = HttpStatusCode.OK.ToString(), StatusCode = HttpStatusCode.OK, Count = list.Count, Type = "SuggestRoute", Messages = list.Select(x => Json.Encode(x)).ToList() };
            return responseModel;
        }
        public ResponseModel GenerateRouteResponse(List<StationModel> list)
        {
            var responseModel = new ResponseModel() { Status = HttpStatusCode.OK.ToString(), StatusCode = HttpStatusCode.OK, Count = list.Count, Type = "StationModel", Messages = list.Select(x => Json.Encode(x)).ToList() };
            return responseModel;
        }

        public ResponseModel GenerateDiscountResponse(List<DiscountModel> list)
        {
            var responseModel = new ResponseModel() { Status = HttpStatusCode.OK.ToString(), StatusCode = HttpStatusCode.OK, Count = list.Count, Type = "Discount", Messages = list.Select(x => Json.Encode(x)).ToList() };
            return responseModel;
        }

        public ResponseModel GenerateWithdrawResponse(List<WithdrawRequestModel> list)
        {
            var responseModel = new ResponseModel() { Status = HttpStatusCode.OK.ToString(), StatusCode = HttpStatusCode.OK, Count = list.Count, Type = "Withdraw", Messages = list.Select(x => Json.Encode(x)).ToList() };
            return responseModel;
        }

        public ResponseModel GenerateWithdrawResponse(List<WithdrawUserReqModel> res)
        {
            var responseModel = new ResponseModel() { Status = HttpStatusCode.OK.ToString(), StatusCode = HttpStatusCode.OK, Count = res.Count, Type = "Withdraw", Messages = res.Select(x => Json.Encode(x)).ToList() };
            return responseModel;
        }

        public ResponseModel GenerateRouteResponse(List<SuggestBriefRouteModel> list)
        {
            var responseModel = new ResponseModel() { Status = HttpStatusCode.OK.ToString(), StatusCode = HttpStatusCode.OK, Count = list.Count, Type = "SuggestBriefRouteModel", Messages = list.Select(x => Json.Encode(x)).ToList() };
            return responseModel;
        }

        public ResponseModel GenerateRouteResponse(List<object> list,string listType)
        {
            var responseModel = new ResponseModel() { Status = HttpStatusCode.OK.ToString(), StatusCode = HttpStatusCode.OK, Count = list.Count, Type = listType, Messages = list.Select(x => Json.Encode(x)).ToList() };
            return responseModel;
        }

        public ResponseModel GenerateRouteResponse(object obj, string objType)
        {
            var responseModel = new ResponseModel() { Status = HttpStatusCode.OK.ToString(), StatusCode = HttpStatusCode.OK, Count = 1, Type = objType, Messages = new List<string> { Json.Encode(obj) }, Infos = _infos,Errors = _errors};
            return responseModel;
        }

        public ResponseModel GenerateRouteResponse(RouteSuggestModel routeSuggestModel)
        {
            var responseModel = new ResponseModel() { Status = HttpStatusCode.OK.ToString(), StatusCode = HttpStatusCode.OK, Count = 1, Type = "RouteResponse", Messages = new List<string> { Json.Encode(routeSuggestModel) } };
            return responseModel;
        }

        public ResponseModel GenerateRouteSuggestResponse(List<RouteSuggestModel> list)
        {
            var responseModel = new ResponseModel() { Status = HttpStatusCode.OK.ToString(), StatusCode = HttpStatusCode.OK, Count = list.Count, Type = "RouteSuggestResponse", Messages = list.Select(x => Json.Encode(x)).ToList() };
            return responseModel;

        }

        public ResponseModel GenerateSuggestAcceptResponse(string messageResponse)
        {
            var message = new List<string> { Json.Encode(messageResponse) };
            var responseModel = new ResponseModel() { Status = HttpStatusCode.OK.ToString(), StatusCode = HttpStatusCode.OK, Count = 1, Type = "AcceptRouteMeesage", Messages = message, Warnings = _warnings, Infos = _infos };
            return responseModel;
        }

        public ResponseModel GenerateResponse(List<string> messages, string type)
        {
            ResponseModel responseModel;
            if (messages.First() == string.Empty)
            {
                responseModel = new ResponseModel() { Status = HttpStatusCode.OK.ToString(), StatusCode = HttpStatusCode.OK, Count = 0, Type = type, Messages = null, Warnings = _warnings, Infos = _infos, Errors = _errors };
            }
            else
            {
                responseModel = new ResponseModel() { Status = HttpStatusCode.OK.ToString(), StatusCode = HttpStatusCode.OK, Count = messages.Count, Type = type, Messages = messages, Warnings = _warnings, Infos = _infos, Errors = _errors };
            }
            return responseModel;
        }

        public ResponseModel GenerateResponse(List<RatingModel> list)
        {
            var responseModel = new ResponseModel() { Status = HttpStatusCode.OK.ToString(), StatusCode = HttpStatusCode.OK, Count = list.Count, Type = "Ratings", Messages = list.Select(x => Json.Encode(x)).ToList() ,Warnings = _warnings, Infos = _infos, Errors = _errors };
            return responseModel;
        }

        public ResponseModel GenerateResponse(List<CommentModel> list)
        {
            var responseModel = new ResponseModel() { Status = HttpStatusCode.OK.ToString(), StatusCode = HttpStatusCode.OK, Count = list.Count, Type = "Comments", Messages = list.Select(x => Json.Encode(x)).ToList() };
            return responseModel;
        }

        public ResponseModel GenerateResponse(AppointmentModel appointmentModel)
        {
            var responseModel = new ResponseModel() { Status = HttpStatusCode.OK.ToString(), StatusCode = HttpStatusCode.OK, Count = 1, Type = "AppointmentInfo", Messages = new List<string> { Json.Encode(appointmentModel) }, Infos = _infos };
            return responseModel;
        }

        public ResponseModel GenerateEventResponse(List<EventModel> list)
        {
            var responseModel = new ResponseModel() { Status = HttpStatusCode.OK.ToString(), StatusCode = HttpStatusCode.OK, Count = list.Count, Type = "Events", Messages = list.Select(x => Json.Encode(x)).ToList() };
            return responseModel;
        }

        public ResponseModel GenerateCityLocResponse(List<CityLoc> list)
        {
            var responseModel = new ResponseModel() { Status = HttpStatusCode.OK.ToString(), StatusCode = HttpStatusCode.OK, Count = list.Count, Type = "CityLocations", Messages = list.Select(x => Json.Encode(x)).ToList() };
            return responseModel;

        }

        public ResponseModel GenerateObjectResponse(List<Object> list, string type)
        {
            var responseModel = new ResponseModel() { Status = HttpStatusCode.OK.ToString(), StatusCode = HttpStatusCode.OK, Count = list.Count, Type = type, Messages = list.Select(x => Json.Encode(x)).ToList() };
            return responseModel;
        }

        public ResponseModel GenerateRecommendRoutesResponse(List<PathPoint> list)
        {
            var responseModel = new ResponseModel() { Status = HttpStatusCode.OK.ToString(), StatusCode = HttpStatusCode.OK, Count = list.Count, Type = "UserContacts", Messages = list.Select(x => Json.Encode(x)).ToList() };
            return responseModel;
        }

        public ResponseModel GenerateRouteResponse(List<LocalRouteModel> list)
        {
            var responseModel = new ResponseModel() { Status = HttpStatusCode.OK.ToString(), StatusCode = HttpStatusCode.OK, Count = list.Count, Type = "UserContacts", Messages = list.Select(x => Json.Encode(x)).ToList() };
            return responseModel;
        }
    }
}
