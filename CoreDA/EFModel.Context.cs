﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CoreDA
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class MibarimEntities : DbContext
    {
        public MibarimEntities()
            : base("name=MibarimEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<BankInfo> BankInfoes { get; set; }
        public virtual DbSet<CarInfo> CarInfoes { get; set; }
        public virtual DbSet<CarPic> CarPics { get; set; }
        public virtual DbSet<Chat> Chats { get; set; }
        public virtual DbSet<CityLocation> CityLocations { get; set; }
        public virtual DbSet<Comment> Comments { get; set; }
        public virtual DbSet<ContactU> ContactUs { get; set; }
        public virtual DbSet<EventLog> EventLogs { get; set; }
        public virtual DbSet<Group> Groups { get; set; }
        public virtual DbSet<LicenseInfo> LicenseInfoes { get; set; }
        public virtual DbSet<RecommendPath> RecommendPaths { get; set; }
        public virtual DbSet<RecommendRoute> RecommendRoutes { get; set; }
        public virtual DbSet<RouteGroup> RouteGroups { get; set; }
        public virtual DbSet<RouteSuggest> RouteSuggests { get; set; }
        public virtual DbSet<RRPricing> RRPricings { get; set; }
        public virtual DbSet<RRTiming> RRTimings { get; set; }
        public virtual DbSet<vw_PathRoute> vw_PathRoute { get; set; }
        public virtual DbSet<vwChat> vwChats { get; set; }
        public virtual DbSet<vwCommentNotification> vwCommentNotifications { get; set; }
        public virtual DbSet<vwDoubleSuggest> vwDoubleSuggests { get; set; }
        public virtual DbSet<vwGroup> vwGroups { get; set; }
        public virtual DbSet<vwLicenseInfo> vwLicenseInfoes { get; set; }
        public virtual DbSet<vwRouteGroup> vwRouteGroups { get; set; }
        public virtual DbSet<vwRRPricing> vwRRPricings { get; set; }
        public virtual DbSet<vwRRTiming> vwRRTimings { get; set; }
        public virtual DbSet<vwSuggestNotification> vwSuggestNotifications { get; set; }
        public virtual DbSet<vwTwoRouteSuggest> vwTwoRouteSuggests { get; set; }
        public virtual DbSet<TripRoute> TripRoutes { get; set; }
        public virtual DbSet<Tran> Trans { get; set; }
        public virtual DbSet<vwTripRoute> vwTripRoutes { get; set; }
        public virtual DbSet<MapImage> MapImages { get; set; }
        public virtual DbSet<AspNetUser> AspNetUsers { get; set; }
        public virtual DbSet<vwCompany> vwCompanies { get; set; }
        public virtual DbSet<RouteRequest> RouteRequests { get; set; }
        public virtual DbSet<vwPath> vwPaths { get; set; }
        public virtual DbSet<Contact> Contacts { get; set; }
        public virtual DbSet<Event> Events { get; set; }
        public virtual DbSet<AboutUser> AboutUsers { get; set; }
        public virtual DbSet<RouteRequestGRoute> RouteRequestGRoutes { get; set; }
        public virtual DbSet<RouteRequestGPath> RouteRequestGPaths { get; set; }
        public virtual DbSet<vwComment> vwComments { get; set; }
        public virtual DbSet<vwContactTrip> vwContactTrips { get; set; }
        public virtual DbSet<vwContactScore> vwContactScores { get; set; }
        public virtual DbSet<ContactScore> ContactScores { get; set; }
        public virtual DbSet<Trip> Trips { get; set; }
        public virtual DbSet<DriverRoute> DriverRoutes { get; set; }
        public virtual DbSet<vwStationRoute> vwStationRoutes { get; set; }
        public virtual DbSet<UserInfo> UserInfoes { get; set; }
        public virtual DbSet<vwUserInfo> vwUserInfoes { get; set; }
        public virtual DbSet<Fanap> Fanaps { get; set; }
        public virtual DbSet<Station> Stations { get; set; }
        public virtual DbSet<vwCarInfo> vwCarInfoes { get; set; }
        public virtual DbSet<TripLocation> TripLocations { get; set; }
        public virtual DbSet<vwRouteRequest> vwRouteRequests { get; set; }
        public virtual DbSet<Invite> Invites { get; set; }
        public virtual DbSet<DiscountUser> DiscountUsers { get; set; }
        public virtual DbSet<BookRequest> BookRequests { get; set; }
        public virtual DbSet<vwRouteSuggest> vwRouteSuggests { get; set; }
        public virtual DbSet<vwBookPay> vwBookPays { get; set; }
        public virtual DbSet<PayReq> PayReqs { get; set; }
        public virtual DbSet<vwPayRoute> vwPayRoutes { get; set; }
        public virtual DbSet<vwBookedTrip> vwBookedTrips { get; set; }
        public virtual DbSet<GoogleToken> GoogleTokens { get; set; }
        public virtual DbSet<Withdraw> Withdraws { get; set; }
        public virtual DbSet<MainStation> MainStations { get; set; }
        public virtual DbSet<StationRoute> StationRoutes { get; set; }
        public virtual DbSet<Image> Images { get; set; }
        public virtual DbSet<ImageReject> ImageRejects { get; set; }
        public virtual DbSet<vwImageReject> vwImageRejects { get; set; }
        public virtual DbSet<vwDriverRoute> vwDriverRoutes { get; set; }
        public virtual DbSet<Discount> Discounts { get; set; }
        public virtual DbSet<vwDiscountUser> vwDiscountUsers { get; set; }
        public virtual DbSet<vwMainStation> vwMainStations { get; set; }
        public virtual DbSet<Rating> Ratings { get; set; }
        public virtual DbSet<vwRating> vwRatings { get; set; }
        public virtual DbSet<EventAttendee> EventAttendees { get; set; }
        public virtual DbSet<vwStation> vwStations { get; set; }
        public virtual DbSet<AppsToken> AppsTokens { get; set; }
        public virtual DbSet<StationRoutePlu> StationRoutePlus { get; set; }
        public virtual DbSet<TmLocation> TmLocations { get; set; }
        public virtual DbSet<Notification> Notifications { get; set; }
        public virtual DbSet<Filter> Filters { get; set; }
        public virtual DbSet<vwDriverTrip> vwDriverTrips { get; set; }
        public virtual DbSet<vwFilter> vwFilters { get; set; }
        public virtual DbSet<vwFilterPlu> vwFilterPlus { get; set; }
        public virtual DbSet<FilterRequest> FilterRequests { get; set; }
        public virtual DbSet<vwFilterRequest> vwFilterRequests { get; set; }
        public virtual DbSet<vwActiveTrip> vwActiveTrips { get; set; }
        public virtual DbSet<vwFilterBook> vwFilterBooks { get; set; }
        public virtual DbSet<vwFilterRequestTrip> vwFilterRequestTrips { get; set; }
        public virtual DbSet<Employee> Employees { get; set; }
    
        public virtual ObjectResult<GetLocalRoutes_Result> GetLocalRoutes(Nullable<decimal> lat, Nullable<decimal> lng, Nullable<int> routeDistance, Nullable<int> pointDistance)
        {
            var latParameter = lat.HasValue ?
                new ObjectParameter("Lat", lat) :
                new ObjectParameter("Lat", typeof(decimal));
    
            var lngParameter = lng.HasValue ?
                new ObjectParameter("Lng", lng) :
                new ObjectParameter("Lng", typeof(decimal));
    
            var routeDistanceParameter = routeDistance.HasValue ?
                new ObjectParameter("RouteDistance", routeDistance) :
                new ObjectParameter("RouteDistance", typeof(int));
    
            var pointDistanceParameter = pointDistance.HasValue ?
                new ObjectParameter("PointDistance", pointDistance) :
                new ObjectParameter("PointDistance", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetLocalRoutes_Result>("GetLocalRoutes", latParameter, lngParameter, routeDistanceParameter, pointDistanceParameter);
        }
    
        public virtual ObjectResult<GenerateSimilarRoutes_Result> GenerateSimilarRoutes(Nullable<int> routeId, Nullable<int> distanceCoeffiecient, Nullable<bool> sameGender)
        {
            var routeIdParameter = routeId.HasValue ?
                new ObjectParameter("routeId", routeId) :
                new ObjectParameter("routeId", typeof(int));
    
            var distanceCoeffiecientParameter = distanceCoeffiecient.HasValue ?
                new ObjectParameter("DistanceCoeffiecient", distanceCoeffiecient) :
                new ObjectParameter("DistanceCoeffiecient", typeof(int));
    
            var sameGenderParameter = sameGender.HasValue ?
                new ObjectParameter("SameGender", sameGender) :
                new ObjectParameter("SameGender", typeof(bool));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GenerateSimilarRoutes_Result>("GenerateSimilarRoutes", routeIdParameter, distanceCoeffiecientParameter, sameGenderParameter);
        }
    
        public virtual ObjectResult<Nullable<int>> GetNotificationUsers()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Nullable<int>>("GetNotificationUsers");
        }
    
        public virtual ObjectResult<GetLastEnter_Result> GetLastEnter(Nullable<System.DateTime> from)
        {
            var fromParameter = from.HasValue ?
                new ObjectParameter("from", from) :
                new ObjectParameter("from", typeof(System.DateTime));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetLastEnter_Result>("GetLastEnter", fromParameter);
        }
    
        public virtual ObjectResult<GetDriverRanks_Result> GetDriverRanks(Nullable<System.DateTime> from)
        {
            var fromParameter = from.HasValue ?
                new ObjectParameter("from", from) :
                new ObjectParameter("from", typeof(System.DateTime));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetDriverRanks_Result>("GetDriverRanks", fromParameter);
        }
    
        public virtual ObjectResult<Nullable<long>> GetDriverRate(Nullable<System.DateTime> from, Nullable<int> userId)
        {
            var fromParameter = from.HasValue ?
                new ObjectParameter("from", from) :
                new ObjectParameter("from", typeof(System.DateTime));
    
            var userIdParameter = userId.HasValue ?
                new ObjectParameter("userId", userId) :
                new ObjectParameter("userId", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Nullable<long>>("GetDriverRate", fromParameter, userIdParameter);
        }
    
        public virtual ObjectResult<GetAggregatedFilters_Result> GetAggregatedFilters(Nullable<System.DateTime> from)
        {
            var fromParameter = from.HasValue ?
                new ObjectParameter("from", from) :
                new ObjectParameter("from", typeof(System.DateTime));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetAggregatedFilters_Result>("GetAggregatedFilters", fromParameter);
        }
    
        public virtual ObjectResult<GetAggregatedTimes_Result> GetAggregatedTimes(Nullable<System.DateTime> from, Nullable<long> stationRouteId)
        {
            var fromParameter = from.HasValue ?
                new ObjectParameter("from", from) :
                new ObjectParameter("from", typeof(System.DateTime));
    
            var stationRouteIdParameter = stationRouteId.HasValue ?
                new ObjectParameter("stationRouteId", stationRouteId) :
                new ObjectParameter("stationRouteId", typeof(long));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetAggregatedTimes_Result>("GetAggregatedTimes", fromParameter, stationRouteIdParameter);
        }
    
        public virtual int CopyStationRoutePlus()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("CopyStationRoutePlus");
        }
    }
}
