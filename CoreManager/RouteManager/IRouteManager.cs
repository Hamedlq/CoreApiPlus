﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreDA;
using CoreManager.Models;
using CoreManager.Models.TrafficAddress;

namespace CoreManager.RouteManager
{
    public interface IRouteManager
    {
        List<long> InsertUserRoute(RouteRequestModel model, int userId);
        void InsertUserEventRoute(RouteEventModel model, int userId);
        List<RouteResponseModel> GetUserRoutes(int userId);
        List<RouteResponseModel> GetUserWeekRoutes(int userId);
        List<RouteResponseModel> GetAllRoutes();
        string GetRouteConfirmationMessage(int userId, List<long> routeId);
        bool CheckConfirmationText(int routeId, string confirmText);
        string ConfirmRoute(ConfirmationModel model, int userId);
//        List<RouteSuggestModel> GetRouteSuggests(int userId);
        List<BriefRouteModel> GetSuggestRouteByRouteId(int userId, int routeId);
        List<BriefRouteModel> GetSuggestWeekRouteByRouteId(int userId, int routeId);
        List<BriefRouteModel> GetUserSuggestRouteByRouteId(int routeId);
        string AcceptSuggestedRoute(int userId, int routeId, int selfRouteId);
        string JoinGroup(int userId, long routeId, int groupId);
        string LeaveGroup(int userId, long routeId, int groupId);
        string DeleteRoute(int userId, int routeRequestId);
        string DeleteGroupSuggest(int userId, long routeId, int routeGroupId);
        string DeleteRouteSuggest(int userId, int selfRouteId, int routeRequestId);
        List<EventModel> GetAllEvents();
        List<CityLoc> GetCityLocations(Point point);
        List<PathPoint> GetRouteRecommends(SrcDstModel model, List<Point> wayPoints);
        List<LocalRouteModel> GetLocaRoutes(Point point);
        string GetPrice(SrcDstModel model);
        PathPriceResponse GetPathPrice(SrcDstModel model);
        string RequestRideShare(int userId, int routeId, int selfRouteId);
        List<SuggestBriefRouteModel> GetAcceptedSuggestRouteByContactId(int userId, long contactId);
        //string AcceptRideShare(int userId, int routeId, int selfRouteId);
        List<SuggestBriefRouteModel> GetSimilarSuggestRouteByContactId(int userId, long contactId);
        TripResponse GetTripInfo(int userId, long tripId);
        //int EndTrip(int userId, long tripId);
        List<RouteResponseModel> GetUserRoutesByMobile(string mobile);

        void InsertEvent(EventRequestModel model);
        ShareResponse ShareRoute(int userId, int modelRouteRequestId);
        ImageResponse GetMapImageById(ImageRequest model);
        LocalRouteUserModel GetRouteInfo(int userId, Guid routeUId);
        void InsertRideRequest(RouteRequestModel model, int userId);
        ContactModel GetContactByRoutes(int routeId, int selfRouteId);
        DriverRouteModel GetRouteInfo(int userId, long driverRouteId);
        void DoCalc();
        ContactStateModel ToggleContactState(int userId, long contactId);
        ScoreModel GetUserScoresByRouteId(int userId, int routeRequestId);
        //UserRouteModel GetTripProfile(int RouteRequestId, int userId);

        List<PassRouteModel> GetPassengerRoutes(int userId, PassFilterModel model);
        PassRouteModel GetPassengerTrip(int userId, long FilterId);
        PaymentDetailModel RequestBooking(int userId, long modelTripId, long modelChargeAmount);
        PaymentDetailModel RequestPayBooking(int userId, long modelTripId, long modelChargeAmount);
        PaymentDetailModel RequestPay(int userId, long modelTripId, long modelChargeAmount);
        List<StationRouteModel> GetStationRoutes();
        List<StationRouteModel> GetPassengerStationRoutes();
        long SetUserRoute(int userId, long modelStRouteId, long stationId);
        long SetRoute(int userId, long srcSubStId, long dstStId);
        List<DriverRouteModel> GetDriverRoutes(int userId);
        List<DriverRouteModel> GetDriverNewRoutes(int userId);
        TripTimeModel SetDriverTrip(int userId, DriverRouteModel model);
        string InvokeTrips();
        string InvokeFilters();
        bool DeleteDriverRoute(int userId, long modelDriverRouteId);
        bool DisableDriverTrip(int userId, DriverRouteModel model);
        DriverTripModel GetUserTrips(int userId);
        DriverTripModel SetTripLocation(int userId, DriverTripModel model);
        PassRouteModel SetPassLocation(int userId, PassRouteModel model);
        List<SubStationModel> GetStations(long stRouteId);
        bool IsPayValid(int userId, PayModel model);
        PaymentDetailModel BookSeat(int userId, PayModel model);
        //PasargadPayModel PayPasargad(int userId, long tripId, long chargeAmount);
        bool ReserveSeat(long payReqId);
        bool HasCapacity(PayModel model);
        bool HasReserved(PayModel model, int userId);
        TripTimeModel CancelBooking(int userId, long tripId);
        long SubmitMainStation(int userId, string modelName, string modelStLat, string modelStLng);
        long SubmitStation(int userId, string modelName, string modelStLat, string modelStLng, long modelMainStationId);
        List<StationModel> GetMainStations();
        List<StationModel> GetAdminMainStations();
        StationRouteModel GetStationRoute(long srcStId, long dstStId);
        List<SubStationModel> GetSubStations(long mainStationId);
        bool MakeStationRoutes();

        PaymentDetailModel RequestInvoice(int userId, long chargeAmount);
        bool InsertEmployeeModel(EmployeeRequestModels model);
        bool InsertEventAttendeeModel(EventAttendeeModel model);
        string SendDriverNotifs();
        List<SubStationModel> GetAllSubStations();
        FilterModel SetFilter(int userId, FilterModel model);
        List<FilterModel> GetUserFilters(int userId);
        List<FilterTimeModel> GetFilterTimes(int userId, FilterModel model);
        bool DeleteFilter(int userId, FilterModel model);
        List<SuggestModel> GetSuggestedRoutes();
        bool MakeStationRoutePlus();
        bool CancelFilter(int userId, FilterModel model);
        TripTimeModel AcceptSuggestRoute(int userId, FilterModel model,bool isAdmin);
        string SetUserNotifications();
        string SendNewBookedMessages();
        //List<ContactPassengersModel> GetPassengersInfo();
        List<ContactPassengersModel> GetPassengersInfo(int userId, DriverRouteModel model);
    }
}
