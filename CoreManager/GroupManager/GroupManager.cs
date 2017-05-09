using System;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreDA;
using CoreManager.Models;
using CoreManager.Resources;
using CoreManager.ResponseProvider;
using CoreManager.TimingService;
using System.Globalization;
using AutoMapper;
using CoreManager.NotificationManager;

namespace CoreManager.GroupManager
{
    public class GroupManager : IGroupManager
    {
        private readonly IResponseProvider _responseProvider;
        private readonly ITimingService _timingService;
        private readonly INotificationManager _notifManager;
        public GroupManager(IResponseProvider responseProvider, ITimingService timingService, INotificationManager notifManager)
        {
            _responseProvider = responseProvider;
            _timingService = timingService;
            _notifManager = notifManager;
        }
        public string ConfirmAppointment(int userid, AppointmentModel model)
        {
            using (var dataModel = new MibarimEntities())
            {
                var validation =
                    dataModel.vwRouteGroups.Where(x => x.UserId == userid && x.GroupId == model.GroupId).ToList();
                if (validation.Count > 0)
                {
                    var group = dataModel.Groups.FirstOrDefault(x => x.GroupId == model.GroupId);
                    if (group != null)
                    {
                        if (group.GIsDriverConfirmed == null || !(bool)group.GIsDriverConfirmed)
                        {
                            group.GRouteRequestId = model.RouteId;
                            group.AppointTime = model.AppointTime;
                            group.AppointTime = model.AppointTime;
                            group.GIsDriverConfirmed = false;
                            group.GAppointAddress = model.GAppointAddress;
                            group.GAppointLatitude = decimal.Parse(model.GAppointLatitude);
                            group.GAppointLongitude = decimal.Parse(model.GAppointLongitude);
                            group.GAppointGeo = CreatePoint(model.GAppointLatitude, model.GAppointLongitude);
                            group.ConfirmedPrice = (decimal)model.ConfirmedPrice;
                            dataModel.SaveChanges();
                            return getResource.getMessage("GroupConfirmed");
                        }
                        else
                        {
                            _responseProvider.SetBusinessMessage(new MessageResponse() { Type = ResponseTypes.Error, Message = getResource.getMessage("AppointAlreadySet") });
                        }
                    }
                }
                else
                {
                    _responseProvider.SetBusinessMessage(new MessageResponse() { Type = ResponseTypes.Warning, Message = getResource.getMessage("UnknownGroup") });
                    return string.Empty;
                }
            }
            return string.Empty;
        }

        public List<CommentModel> GetGroupComments(int userId, int groupId)
        {
            var comments = new List<CommentModel>();
            using (var dataModel = new MibarimEntities())
            {
                var validation =
                    dataModel.Contacts.Where(x => (x.ContactDriverUserId == userId || x.ContactPassengerUserId == userId) && x.ContactId == groupId && !x.ContactIsDeleted).ToList();
                if (validation.Count > 0)
                {
                    var vwcomments = dataModel.vwChats.Where(x => x.ContactId == groupId).OrderBy(x => x.ChatCreateTime).ToList();
                    foreach (var vwcomment in vwcomments)
                    {
                        var comment = new CommentModel();
                        comment.CommentId = vwcomment.ChatId;
                        comment.GroupId = groupId;
                        comment.NameFamily = vwcomment.Name + " " + vwcomment.Family;
                        comment.TimingString = _timingService.GetTimingString(vwcomment.ChatCreateTime);
                        comment.Comment = vwcomment.ChatTxt;
                        comment.IsDeletable = (vwcomment.ChatUserId == userId);
                        //comment.UserPic = vwcomment.UserPic;
                        comment.UserImageId = vwcomment.UserImageId.ToString();
                        comments.Add(comment);
                        /*var notifs =
                            dataModel.Notifications.FirstOrDefault(
                                x => x.NotifUserId == userId && x.NotifCommentId == vwcomment.ChatId);
                        if (notifs != null)
                        {
                            notifs.IsNotificationSeen = true;
                            notifs.IsNotificationSent = true;
                        }*/
                    }
                    dataModel.Chats.Where(x => x.ContactId == groupId && x.ChatUserId != userId)
                        .Each(x => x.IsChatSeen = true);
                    dataModel.SaveChanges();
                }
                else
                {
                    _responseProvider.SetBusinessMessage(new MessageResponse() { Type = ResponseTypes.Warning, Message = getResource.getMessage("UnknownGroup") });
                }
            }
            return comments;
        }

        public string SubmitComment(int userId, int groupId, string comment)
        {
            using (var dataModel = new MibarimEntities())
            {
                var validation =
                    dataModel.Contacts.Where(x => (x.ContactDriverUserId == userId || x.ContactPassengerUserId == userId) && x.ContactId == groupId && !x.ContactIsDeleted).ToList();
                if (validation.Count > 0)
                {
                    var commentModel = new Chat();
                    commentModel.ContactId = groupId;
                    commentModel.ChatCreateTime = DateTime.Now;
                    commentModel.ChatIsDeleted = false;
                    commentModel.ChatUserId = userId;
                    commentModel.ChatTxt = comment;
                    dataModel.Chats.Add(commentModel);
                    dataModel.SaveChanges();
                    var otherUsers = validation.FirstOrDefault();
                    otherUsers.ContactLastMsg = Truncate(comment, 28);
                    otherUsers.ContactLastMsgTime = DateTime.Now;
                    if (otherUsers.ContactDriverUserId == userId)
                    {
                        var user = dataModel.vwUserInfoes.FirstOrDefault(x => x.UserId == userId);
                        NotifModel notifModel = new NotifModel();
                        notifModel.Title = user.Name + " " + user.Family+":";
                        notifModel.Body = Truncate(comment, 70);
                        notifModel.Tab = (int)MainTabs.Message;
                        notifModel.RequestCode = (int)NotificationType.NewMessage;
                        notifModel.NotificationId = (int)NotificationType.NewMessage;
                        DoStuff(notifModel, groupId, otherUsers.ContactDriverUserId, otherUsers.ContactPassengerUserId);
                        /*var notification = new Notification();
                        notification.NotifUserId = otherUsers.ContactUserId;
                        notification.NotifCommentId = commentModel.ChatId;
                        notification.IsNotificationSeen = false;
                        notification.IsNotificationSent = false;
                        dataModel.Notifications.Add(notification);*/
                    }
                    else
                    {
                        var user = dataModel.vwUserInfoes.FirstOrDefault(x => x.UserId == userId);
                        NotifModel notifModel = new NotifModel();
                        notifModel.Title = user.Name + " " + user.Family + ":";
                        notifModel.Body = Truncate(comment, 70);
                        notifModel.Tab = (int)MainTabs.Message;
                        notifModel.RequestCode = (int)NotificationType.NewMessage;
                        notifModel.NotificationId = (int)NotificationType.NewMessage;
                        DoStuff(notifModel, groupId, otherUsers.ContactPassengerUserId, otherUsers.ContactDriverUserId);
                        /*var notification = new Notification();
                        notification.NotifUserId = otherUsers.ContactProUserId;
                        notification.NotifCommentId = commentModel.ChatId;
                        notification.IsNotificationSeen = false;
                        notification.IsNotificationSent = false;
                        dataModel.Notifications.Add(notification);*/
                    }
                    /*foreach (var vwRouteGroup in otherUsers)
                    {
                        
                    }*/
                    dataModel.SaveChanges();
                }
                else
                {
                    _responseProvider.SetBusinessMessage(new MessageResponse() { Type = ResponseTypes.Warning, Message = getResource.getMessage("UnknownGroup") });
                    return string.Empty;
                }
            }
            return string.Empty;
        }

        public string SubmitChat(int userId, string mobile, string comment)
        {
            using (var dataModel = new MibarimEntities())
            {
                var validation =
                    dataModel.Contacts.Where(x => (x.ContactDriverUserId == 1 && x.ContactPassengerUserId == userId) && !x.ContactIsDeleted).ToList();
                if (validation.Count > 0)
                {
                    var contact = validation.FirstOrDefault();
                    var commentModel = new Chat();
                    commentModel.ContactId = contact.ContactId;
                    commentModel.ChatCreateTime = DateTime.Now;
                    commentModel.ChatIsDeleted = false;
                    commentModel.ChatUserId = 1;
                    commentModel.IsChatSeen = false;
                    commentModel.ChatTxt = comment;
                    dataModel.Chats.Add(commentModel);
                    dataModel.SaveChanges();
                    var otherUsers = validation.FirstOrDefault();
                    otherUsers.ContactLastMsg = Truncate(comment, 28);
                    otherUsers.ContactLastMsgTime = DateTime.Now;
                    var user = dataModel.vwUserInfoes.FirstOrDefault(x => x.UserId == userId);
                    NotifModel notifModel = new NotifModel();
                    notifModel.Title = user.Name + " " + user.Family;
                    notifModel.Body = Truncate(comment, 28);
                    notifModel.Tab = (int)MainTabs.Message;
                    notifModel.RequestCode = (int)NotificationType.NewMessage;
                    notifModel.NotificationId = (int)NotificationType.NewMessage;

                    DoStuff(notifModel, contact.ContactId, 1, userId);
                    /*foreach (var vwRouteGroup in otherUsers)
                    {
                        
                    }*/
                    dataModel.SaveChanges();
                }
                else
                {
                    _responseProvider.SetBusinessMessage(new MessageResponse() { Type = ResponseTypes.Warning, Message = getResource.getMessage("UnknownGroup") });
                    return string.Empty;
                }
            }
            return string.Empty;
        }

        public async Task DoStuff(NotifModel notifModel, long contactId, int senderUserId, int receiverUserId)
        {
            await Task.Run(() =>
            {
                var t = Task.Run(async delegate
                {
                    await Task.Delay(15000);
                    await DelayedTask(notifModel,contactId, senderUserId, receiverUserId);
                });
                t.Wait();
            });
        }

        private async Task DelayedTask(NotifModel notifModel, long contactId, int senderUserId, int receiverUserId)
        {
            using (var dataModel = new MibarimEntities())
            {
                if (dataModel.Chats.Any(x => x.ContactId == contactId && x.ChatUserId == senderUserId && x.IsChatSeen==false))
                {
                    _notifManager.SendNotifToUser(notifModel,receiverUserId);
                    dataModel.Chats.Where(x => x.ContactId == contactId && x.ChatUserId == senderUserId).Each(x => x.IsChatSeen = true);
                }
            }
        }

        public AppointmentModel GetPassengerConfirmInfo(int userId, int groupId, long routeId)
        {
            var appointmentModel = new AppointmentModel();
            using (var dataModel = new MibarimEntities())
            {
                var validation =
                    dataModel.vwRouteGroups.Where(x => x.UserId == userId && x.GroupId == groupId && x.RouteRequestId == routeId && x.RGIsConfirmed).ToList();
                if (validation.Count > 0)
                {
                    var groupInfo = dataModel.vwGroups.FirstOrDefault(x => x.GroupId == groupId);
                    if (groupInfo.GIsDriverConfirmed != null && groupInfo.GIsDriverConfirmed.Value)
                    {
                        var price = (decimal)groupInfo.ConfirmedPrice;
                        var cost = price.ToString("N0", new NumberFormatInfo()
                        {
                            NumberGroupSizes = new[] { 3 },
                            NumberGroupSeparator = ","
                        });
                        appointmentModel.ConfirmedPriceString = cost;
                        appointmentModel.ConfirmedPriceMessage = string.Format(
                            getResource.getMessage("paymentMessage"), cost);
                        appointmentModel.AppointTime = groupInfo.AppointTime.Value;
                        appointmentModel.AppointTimeString = groupInfo.AppointTime.Value.ToString("hh:mm");
                        appointmentModel.ConfirmedPrice = groupInfo.ConfirmedPrice;
                        appointmentModel.GAppointAddress = groupInfo.GAppointAddress;
                        appointmentModel.GAppointLatitude = groupInfo.GAppointLatitude.ToString();
                        appointmentModel.GAppointLongitude = groupInfo.GAppointLongitude.ToString();
                    }
                    else
                    {
                        _responseProvider.SetBusinessMessage(new MessageResponse() { Type = ResponseTypes.Info, Message = getResource.getMessage("DriverNotSet") });
                    }
                    return appointmentModel;
                }
                else
                {
                    _responseProvider.SetBusinessMessage(new MessageResponse() { Type = ResponseTypes.Warning, Message = getResource.getMessage("UnknownGroup") });
                    return appointmentModel;
                }
            }
        }

        public string AppointFinalConfirm(int userId, AppointConfirmModel model)
        {
            using (var dataModel = new MibarimEntities())
            {
                var validation =
                    dataModel.vwRouteGroups.Where(x => x.UserId == userId && x.GroupId == model.GroupId).ToList();
                if (validation.Count > 0)
                {
                    var group = dataModel.Groups.FirstOrDefault(x => x.GroupId == model.GroupId);
                    if (group != null)
                    {
                        if (!(bool)group.GIsDriverConfirmed)
                        {
                            if (group.GRouteRequestId == model.RouteId)
                            {
                                group.GIsDriverConfirmed = true;
                                dataModel.SaveChanges();
                                return getResource.getMessage("GroupConfirmed");
                            }
                            else
                            {
                                _responseProvider.SetBusinessMessage(new MessageResponse() { Type = ResponseTypes.Error, Message = getResource.getMessage("UnknowError") });
                                throw new Exception(getResource.getMessage("UnknowError"));
                            }
                        }
                        else
                        {
                            _responseProvider.SetBusinessMessage(new MessageResponse() { Type = ResponseTypes.Error, Message = getResource.getMessage("AppointAlreadySet") });
                        }
                    }
                }
                else
                {
                    _responseProvider.SetBusinessMessage(new MessageResponse() { Type = ResponseTypes.Warning, Message = getResource.getMessage("UnknownGroup") });
                    return string.Empty;
                }
            }
            return string.Empty;
        }

        public List<CommentModel> GetSupportChats(int userId)
        {
            var comments = new List<CommentModel>();
            using (var dataModel = new MibarimEntities())
            {
                var validation =
                    dataModel.Contacts.Where(x => (x.ContactDriverUserId == 1 && x.ContactPassengerUserId == userId) && !x.ContactIsDeleted).ToList();
                if (validation.Count > 0)
                {
                    var contact = validation.FirstOrDefault();
                    var vwcomments = dataModel.vwChats.Where(x => x.ContactId == contact.ContactId).OrderBy(x => x.ChatCreateTime).ToList();
                    foreach (var vwcomment in vwcomments)
                    {
                        var comment = new CommentModel();
                        comment.CommentId = vwcomment.ChatId;
                        comment.GroupId = (int)contact.ContactId;
                        comment.NameFamily = vwcomment.Name + " " + vwcomment.Family;
                        comment.TimingString = _timingService.GetTimingString(vwcomment.ChatCreateTime);
                        comment.Comment = vwcomment.ChatTxt;
                        comment.IsDeletable = true;
                        //comment.UserPic = vwcomment.UserPic;
                        comments.Add(comment);
                        /*var notifs =
                            dataModel.Notifications.FirstOrDefault(
                                x => x.NotifUserId == userId && x.NotifCommentId == vwcomment.ChatId);
                        if (notifs != null)
                        {
                            notifs.IsNotificationSeen = true;
                            notifs.IsNotificationSent = true;
                        }*/
                    }
                    dataModel.Chats.Where(x => x.ContactId == contact.ContactId && x.ChatUserId != userId)
                        .Each(x => x.IsChatSeen = true);
                    dataModel.SaveChanges();
                }
                else
                {
                    _responseProvider.SetBusinessMessage(new MessageResponse() { Type = ResponseTypes.Warning, Message = getResource.getMessage("UnknownGroup") });
                }
            }
            return comments;
        }

        public DbGeography CreatePoint(string latitude, string longitude)
        {
            var text = string.Format("POINT({0} {1})", latitude, longitude);
            // 4326 is most common coordinate system used by GPS/Maps
            return DbGeography.FromText(text, 4326);
        }

        public string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }
}
