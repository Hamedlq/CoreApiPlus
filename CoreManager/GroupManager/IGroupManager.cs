using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreManager.Models;

namespace CoreManager.GroupManager
{
    public interface IGroupManager
    {
        string ConfirmAppointment(int userid, AppointmentModel model);
        List<CommentModel> GetGroupComments(int userId, int groupId);
        string SubmitComment(int userId, int groupId, string comment);
        string SubmitChat(int userId, string mobile, string comment);
        AppointmentModel GetPassengerConfirmInfo(int userId, int groupId, long routeId);
        string AppointFinalConfirm(int userId, AppointConfirmModel model);
        List<CommentModel> GetSupportChats(int userId);
        
    }
}
