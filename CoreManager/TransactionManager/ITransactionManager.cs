using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreManager.Models;

namespace CoreManager.TransactionManager
{
    public interface ITransactionManager
    {
        void ChargeAccount(int userId,int value);
        void GiftChargeAccount(int userId, int value);
        void PayMoney(int sourceUserId,int destinationUserId ,int value);
        float GetRemain(int userId);
        float GetRemainWithoutGift(int userId);
    }
}
