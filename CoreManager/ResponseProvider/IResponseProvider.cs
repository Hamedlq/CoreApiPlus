using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.ModelBinding;
using System.Web.Routing;
using CoreManager.Models;
using CoreManager.Models.TrafficAddress;
using CoreManager.Resources;

namespace CoreManager.ResponseProvider
{
    public interface IResponseProvider
    {
        void SetBusinessMessage(MessageResponse model);
        ResponseModel GenerateOKResponse();
        ResponseModel GenerateBadRequestResponse();
        ResponseModel GenerateBadRequestResponse(ModelStateDictionary modelState);
        ResponseModel GenerateUnknownErrorResponse();
        ResponseModel GenerateInternalServerErrorResponse();

        ResponseModel GenerateResponse(object obj, string type);

        //string GenerateInvalidUserPassResponse();
        ResponseModel GenerateRouteResponse(List<RouteResponseModel> list);
        ResponseModel GenerateRouteResponse(List<NotifModel> list);
        ResponseModel GenerateRouteResponse(List<StationModel> list);
        ResponseModel GenerateRouteResponse(List<StationRouteModel> list);
        ResponseModel GenerateRouteResponse(List<FilterTimeModel> list);
        ResponseModel GenerateRouteResponse(List<SubStationModel> list);
        ResponseModel GenerateRouteResponse(List<PersoanlInfoModel> list);
        ResponseModel GenerateRouteResponse(List<PassRouteModel> list);
        ResponseModel GenerateRouteResponse(List<DriverRouteModel> list);
        ResponseModel GenerateRouteResponse(List<GasRank> list);
        ResponseModel GenerateRouteResponse(List<BriefRouteModel> list);
        ResponseModel GenerateDiscountResponse(List<DiscountModel> list);
        ResponseModel GenerateWithdrawResponse(List<WithdrawRequestModel> res);
        ResponseModel GenerateWithdrawResponse(List<WithdrawUserReqModel> res);
        ResponseModel GenerateRouteResponse(List<SuggestBriefRouteModel> list);
        ResponseModel GenerateRouteResponse(List<FilterModel> list);
        ResponseModel GenerateRouteResponse(List<Object> list,string listType);
        ResponseModel GenerateRouteResponse(Object obj,string objType);
        ResponseModel GenerateRouteResponse(RouteSuggestModel routeSuggestModel);
        ResponseModel GenerateRouteSuggestResponse(List<RouteSuggestModel> list);
        ResponseModel GenerateSuggestAcceptResponse(string messageResponse);
        ResponseModel GenerateResponse(List<string> messages, string type);
        ResponseModel GenerateResponse(List<RatingModel> res);
        ResponseModel GenerateResponse(List<CommentModel> res);
        ResponseModel GenerateResponse(AppointmentModel appointmentModel);
        ResponseModel GenerateEventResponse(List<EventModel> response);
        ResponseModel GenerateCityLocResponse(List<CityLoc> response);
        ResponseModel GenerateObjectResponse(List<Object> response, string type);
        ResponseModel GenerateRecommendRoutesResponse(List<PathPoint> res);
        ResponseModel GenerateRouteResponse(List<LocalRouteModel> response);
        ResponseModel GenerateRouteResponse(List<SuggestModel> response);


    }
}
