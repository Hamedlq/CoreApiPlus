using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using CoreManager.Models;

namespace CoreManager.FanapManager
{
    public interface IFanapManager
    {
        UserModel GetFanapUserInfo(FanapModel fanapModel);
        void SaveFanapUser(int userId, int fModelUserName);

        PaymentDetailModel FanapBookTrip(int userId, long tripId);
        PaymentDetailModel Payresult(long modelReqId);
    }
}
