using System;
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
        PaymentDetailModel RequestBooking(int userId, long modelTripId, long modelChargeAmount);
        PaymentDetailModel RequestPayBooking(int userId, long modelTripId, long modelChargeAmount);
        List<StationRouteModel> GetStationRoutes();
        List<StationRouteModel> GetPassengerStationRoutes();
        bool SetUserRoute(int userId, long modelStRouteId, long stationId);
        List<DriverRouteModel> GetDriverRoutes(int userId);
        TripTimeModel SetDriverTrip(int userId, DriverRouteModel model);
        string InvokeTrips();
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
    }
}
