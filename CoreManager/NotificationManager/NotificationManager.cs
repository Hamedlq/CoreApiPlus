using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CoreDA;
using CoreExternalService;
using CoreExternalService.Models;
using CoreManager.LogProvider;
using CoreManager.Models;
using CoreManager.Resources;

namespace CoreManager.NotificationManager
{
    public class NotificationManager : INotificationManager
    {
        private readonly IGoogleService _gService;
        private readonly ILogProvider _logmanager;
        public NotificationManager(ILogProvider logmanager)
        {
            _logmanager = logmanager;
            _gService = new GoogleService();
        }
        public NotificationManager()
        {
        }
        public void SendSuggestionNotif(List<RouteSuggest> notifSendingSuggests)
        {
            using (var dataModel = new MibarimEntities())
            {
                var selfrequestId = notifSendingSuggests.FirstOrDefault().SelfRouteRequestId;
                var selfUser =
                    dataModel.RouteRequests.FirstOrDefault(
                        x => x.RouteRequestId == selfrequestId);
                var gtoken =
                    dataModel.GoogleTokens.Where(x => x.GtokenUserId == selfUser.RouteRequestUserId)
                        .OrderByDescending(x => x.GtokenCreateTime).ToList();
                if (gtoken.Count > 0)
                {
                    _gService.SendNotification(gtoken.FirstOrDefault().GtokenKey, NotificationType.SuggestRoute.ToString());
                    _logmanager.Log("notifLog", "notifLog", gtoken.FirstOrDefault().GtokenKey + ";;" + selfUser.RouteRequestUserId);
                }
                var suggestRouteIds = notifSendingSuggests.Select(x => x.SuggestRouteRequestId);
                var otherUsers =
                    dataModel.RouteRequests.Where(x => suggestRouteIds.Contains(x.RouteRequestId)).GroupBy(x => x.RouteRequestUserId);
                foreach (var otherUser in otherUsers)
                {
                    gtoken =
                    dataModel.GoogleTokens.Where(x => x.GtokenUserId == otherUser.Key)
                        .OrderByDescending(x => x.GtokenCreateTime).ToList();
                    if (gtoken.Count > 0)
                    {
                        _gService.SendNotification(gtoken.FirstOrDefault().GtokenKey, NotificationType.SuggestRoute.ToString());
                        _logmanager.Log("notifotherLog", "notifotherLog", gtoken.FirstOrDefault().GtokenKey + ";;" + otherUser.Key);
                    }
                }
                gtoken =
                    dataModel.GoogleTokens.Where(x => x.GtokenUserId == 1)
                        .OrderByDescending(x => x.GtokenCreateTime).ToList();
                if (gtoken.Count > 0)
                {
                    _gService.SendNotification(gtoken.FirstOrDefault().GtokenKey, NotificationType.SuggestRoute.ToString());
                }
            }
        }

        public void SendRideShareRequestNotif(int suggestRrUserId)
        {

            using (var dataModel = new MibarimEntities())
            {
                var gtoken =
                    dataModel.GoogleTokens.Where(x => x.GtokenUserId == suggestRrUserId)
                        .OrderByDescending(x => x.GtokenCreateTime).ToList();
                if (gtoken.Count > 0)
                {
                    _gService.SendNotification(gtoken.FirstOrDefault().GtokenKey, NotificationType.RideShareRequest.ToString());
                }
                gtoken =
                    dataModel.GoogleTokens.Where(x => x.GtokenUserId == 1)
                        .OrderByDescending(x => x.GtokenCreateTime).ToList();
                if (gtoken.Count > 0)
                {
                    _gService.SendNotification(gtoken.FirstOrDefault().GtokenKey, NotificationType.RideShareRequest.ToString());
                }

            }
        }

        public void SendRideShareAcceptionNotif(int selfRrUserId)
        {
            using (var dataModel = new MibarimEntities())
            {
                var gtoken =
                    dataModel.GoogleTokens.Where(x => x.GtokenUserId == selfRrUserId)
                        .OrderByDescending(x => x.GtokenCreateTime).ToList();
                if (gtoken.Count > 0)
                {
                    _gService.SendNotification(gtoken.FirstOrDefault().GtokenKey, NotificationType.RideShareAccepted.ToString());
                }
                gtoken =
                    dataModel.GoogleTokens.Where(x => x.GtokenUserId == 1)
                        .OrderByDescending(x => x.GtokenCreateTime).ToList();
                if (gtoken.Count > 0)
                {
                    _gService.SendNotification(gtoken.FirstOrDefault().GtokenKey, NotificationType.RideShareRequest.ToString());
                }

            }

        }

        public void SendChatMessage(int userId)
        {
            using (var dataModel = new MibarimEntities())
            {
                var gtoken =
                    dataModel.GoogleTokens.Where(x => x.GtokenUserId == userId)
                        .OrderByDescending(x => x.GtokenCreateTime).ToList();
                if (gtoken.Count > 0)
                {
                    _gService.SendNotification(gtoken.FirstOrDefault().GtokenKey, NotificationType.NewMessage.ToString());
                }
            }
        }

        public void SendTripNotifications(Contact cont)
        {
            using (var dataModel = new MibarimEntities())
            {
                var gtoken =
                    dataModel.GoogleTokens.Where(x => x.GtokenUserId == cont.ContactDriverUserId)
                        .OrderByDescending(x => x.GtokenCreateTime).ToList();
                if (gtoken.Count > 0)
                {
                    _gService.SendNotification(gtoken.FirstOrDefault().GtokenKey, NotificationType.NewTrip.ToString());
                }
                gtoken =
                    dataModel.GoogleTokens.Where(x => x.GtokenUserId == cont.ContactPassengerUserId)
                        .OrderByDescending(x => x.GtokenCreateTime).ToList();
                if (gtoken.Count > 0)
                {
                    _gService.SendNotification(gtoken.FirstOrDefault().GtokenKey, NotificationType.NewTrip.ToString());
                }
                gtoken =
                    dataModel.GoogleTokens.Where(x => x.GtokenUserId == 1)
                        .OrderByDescending(x => x.GtokenCreateTime).ToList();
                if (gtoken.Count > 0)
                {
                    _gService.SendNotification(gtoken.FirstOrDefault().GtokenKey, NotificationType.NewTrip.ToString());
                }
            }
        }

        public void SendTripStartNotifications(int routeRequestUserId)
        {
            using (var dataModel = new MibarimEntities())
            {
                var gtoken =
                    dataModel.GoogleTokens.Where(x => x.GtokenUserId == routeRequestUserId)
                        .OrderByDescending(x => x.GtokenCreateTime).ToList();
                if (gtoken.Count > 0)
                {
                    _gService.SendNotification(gtoken.FirstOrDefault().GtokenKey, NotificationType.StartTrip.ToString());
                }
                gtoken =
                    dataModel.GoogleTokens.Where(x => x.GtokenUserId == 1)
                        .OrderByDescending(x => x.GtokenCreateTime).ToList();
                if (gtoken.Count > 0)
                {
                    _gService.SendNotification(gtoken.FirstOrDefault().GtokenKey, NotificationType.StartTrip.ToString());
                }
            }
        }

        public void SendDriverRouteNotif()
        {
            using (var dataModel = new MibarimEntities())
            {
                var gtoken =
                    dataModel.GoogleTokens.Where(x => x.GtokenUserId == 1)
                        .OrderByDescending(x => x.GtokenCreateTime).ToList();
                if (gtoken.Count > 0)
                {
                    _gService.SendNotification(gtoken.FirstOrDefault().GtokenKey, NotificationType.NewDriver.ToString());
                }
            }
        }

        public void SendNewEvent()
        {
            using (var dataModel = new MibarimEntities())
            {
                var gtoken =
                    dataModel.GoogleTokens.Where(x => x.GtokenUserId == 1)
                        .OrderByDescending(x => x.GtokenCreateTime).ToList();
                if (gtoken.Count > 0)
                {
                    _gService.SendNotification(gtoken.FirstOrDefault().GtokenKey, NotificationType.NewEvent.ToString());
                }
            }
        }

        public void SendInviteGiftNotif(int userId)
        {
            using (var dataModel = new MibarimEntities())
            {
                var gtoken =
                    dataModel.GoogleTokens.Where(x => x.GtokenUserId == userId)
                        .OrderByDescending(x => x.GtokenCreateTime).ToList();
                if (gtoken.Count > 0)
                {
                    _gService.SendNotification(gtoken.FirstOrDefault().GtokenKey, NotificationType.GiftInvite.ToString());
                }
                gtoken =
                    dataModel.GoogleTokens.Where(x => x.GtokenUserId == 1)
                        .OrderByDescending(x => x.GtokenCreateTime).ToList();
                if (gtoken.Count > 0)
                {
                    _gService.SendNotification(gtoken.FirstOrDefault().GtokenKey, NotificationType.GiftInvite.ToString());
                }
            }
        }

        public void SendNotifToUser(NotifModel notif, int userId)
        {
            using (var dataModel = new MibarimEntities())
            {
                var gtoken =
                    dataModel.GoogleTokens.Where(x => x.GtokenUserId == userId)
                        .OrderByDescending(x => x.GtokenCreateTime).ToList();
                if (gtoken.Count > 0)
                {
                    _gService.SendNotification(gtoken.FirstOrDefault().GtokenKey, notif.EncodedTitle, notif.EncodedBody, notif.Action, notif.Tab.ToString(),notif.RequestCode,notif.NotificationId,notif.Url);
                }
                
            }
        }
        public void SendGroupNotif(NotifModel notif, List<int> userIds)
        {
            string[] gTokens;
            using (var dataModel = new MibarimEntities())
            {
                
                var gtokens =
                    dataModel.GoogleTokens.Where(x => userIds.Contains((int)x.GtokenUserId))
                    .GroupBy(x=>x.GtokenUserId,(key,g)=>g.OrderByDescending(e=>e.GtokenCreateTime).FirstOrDefault()).ToList();
                if (gtokens.Count > 0)
                {
                    _gService.SendGroupNotification(gtokens.Select(x=>x.GtokenKey).ToList(), notif.Title, notif.Body, notif.Action, notif.Tab.ToString());
                }

            }
        }
    }
}
