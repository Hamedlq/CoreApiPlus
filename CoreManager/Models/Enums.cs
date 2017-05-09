﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreManager.Models
{
    public enum BooleanValue
    {
        True = 1,
        False = 0
    };

    public enum UserRoles
    {
        AdminApplication,
        TaxiAgencyAdmin,
        TaxiAgencyDriver,
        WebUser,
        MobileUser,

    };

    public enum Gender
    {
        Man = 1,
        Woman = 2
    };

    public enum TimingOptions
    {
        Now = 1,
        Today = 2,
        InDateAndTime = 3,
        Weekly = 4
    };

    public enum PricingOptions
    {
        NoMatter = 1,
        MinMax = 2,
        Free = 3
    };

    public enum MainTabs
    {
        Profile = 0,
        Message = 1,
        Map = 2,
        Event = 3,
        Route = 4,

    };
    public enum WeekDay
    {
        Sat = 1,
        Sun = 2,
        Mon = 3,
        Tue = 4,
        Wed = 5,
        Thu = 6,
        Fri = 7
    }

    public enum RouteRequestType
    {
        ByWebUser = 1
    }

    public enum CarTypes
    {
        Praide = 1,
        Peykan = 2,
        Free = 3
    };

    public enum ResponseTypes
    {
        Success = 2,
        Error = 4,
        Warning = 6,
        Info = 8,
        UnknownError = 10,
        ConfirmMessage = 12,
    }
    public enum EventTypes
    {
        Goto = 1,
        ReturnFrom = 2,
        GoReturn = 3
    }

    public enum VerifiedLevel
    {
        NotVerified=1,
        PassengerVerified=2,
        DriverVerified=3
    }

    public enum CityLocationTypes
    {
        CityPoint = 1,
        Cinema = 2,
        Theaters = 3
    }
    public enum LocalRouteTypes
    {
        Driver = 1,
        Passenger = 2
    }

    public enum FactorType  
    {
        PassengerPay = 1,
        DriverReceipt = 2
    }

    public enum NotificationType
    {
        SuggestRoute=1,
        RideShareRequest=2,
        RideShareAccepted=3,
        NewMessage=4,
        NewTrip=5,
        StartTrip=6,
        NewDriver=7,
        NewEvent=8,
        GiftInvite = 9,
        MoneyTransaction=10,
        TripStateActivated=11,
        AdminNotif=12
    }
    public enum ImageType
    {
        UserPic = 1,
        UserNationalCard = 2,
        LicensePic = 3,
        CarPic = 4,
        CarBckPic = 5,
        BankPic=6,
        MapImage=7
    }

    public enum TripState
    {
        Scheduled = 1,
        CanceledByUser = 3,
        InRiding = 5,
        DriverNotCome = 7,
        PassengerNotCome = 9,
        InProccess = 11,
        Rated = 13,
        Finished = 15,
        Canceled = 17

    }
    public enum TripRouteState
    {
        TripRouteAlerted = 1,
        TripRouteJoined = 2,
        TripRouteFinished = 3,
        TripRouteNotJoined = 4,
        TripRouteCanceled = 5
    }
    public enum TransactionType
    {
        ChargeAccount = 1,
        PayMoney = 2,
        ReceivePay = 3,
        TransferMoney = 4,
        WithdrawMoney = 5,
        GiftChargeAccount = 6
    }

    public enum RouteRequestState
    {
        WaitForDriver=1,
        WaitForPassenger=2,
        Suggested = 3,
        RideShareRequested = 4,
        RideShareAccepted = 5,
        TripHappened = 6
    }
    public enum ServiceTypes
    {
        Private = 1,
        RideShare = 2,
        EventRide = 3,
        RideRequest = 4,
        WorkRequest = 5,
    }

    public enum DiscountStates
    {
        Submitted = 1,
        Used = 2,
        Expired = 3
    }

    public enum WithdrawStates
    {
        Submitted = 1,
        Calculated = 2,
        Payed = 3,
        Cenceled = 4,
    }

    
}