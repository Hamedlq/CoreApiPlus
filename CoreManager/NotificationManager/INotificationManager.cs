using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreDA;
using CoreManager.Models;

namespace CoreManager.NotificationManager
{
    public interface INotificationManager
    {
        void SendSuggestionNotif(List<RouteSuggest> notifSendingSuggests);
        void SendRideShareRequestNotif(int suggestRrUserId);
        void SendRideShareAcceptionNotif(int selfRrUserId);
        void SendChatMessage(int userId);
        void SendTripNotifications(Contact cont);
        void SendTripStartNotifications(int routeRequestUserId);
        void SendDriverRouteNotif();
        void SendNewEvent();
        void SendInviteGiftNotif(int userId);
        void SendNotifToUser(NotifModel notif, int userId);
        void SendNotifToDriver(NotifModel notif, int userId);
        void SendGroupNotif(NotifModel notif, List<int> userIds);
        void SendNotifToAdmins(NotifModel notifModel);

        List<NotifModel> GetUserNotification(int userId, NotificationType notificationType);
    }
}
