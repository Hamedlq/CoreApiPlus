using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreDA;
using CoreExternalService.SMSirSentAndReceivedMessages;
using CoreManager.Models;
using CoreManager.NotificationManager;
using CoreManager.Resources;

namespace CoreManager.TransactionManager
{
    public class TransactionManager : ITransactionManager
    {
        private readonly INotificationManager _notifManager;
        #region Constructor
        public TransactionManager()
        {
        }
        public TransactionManager(INotificationManager notifManager)
        {
            _notifManager = notifManager;
        }
        #endregion
        public void ChargeAccount(int userId, int value)
        {
            using (var dataModel = new MibarimEntities())
            {
                var userInfo = dataModel.vwUserInfoes.FirstOrDefault(x => x.UserId == userId);
                var desc = string.Format(getResource.getMessage("PaymentDesc"), GetUserNameFamilyString(userInfo), value);
                var trans = new Tran();
                trans.TransCreateTime = DateTime.Now;
                trans.TransType = (int)TransactionType.ChargeAccount;
                trans.TransUserId = userId;
                trans.TransValue = value;
                trans.TransDescription = desc;
                dataModel.Trans.Add(trans);
                dataModel.SaveChanges();
            }

        }

        public void GiftChargeAccount(int userId, int value)
        {
            using (var dataModel = new MibarimEntities())
            {
                var userInfo = dataModel.vwUserInfoes.FirstOrDefault(x => x.UserId == userId);
                var desc = string.Format(getResource.getMessage("GiftPayDesc"), GetUserNameFamilyString(userInfo), value);
                var trans = new Tran();
                trans.TransCreateTime = DateTime.Now;
                trans.TransType = (int)TransactionType.GiftChargeAccount;
                trans.TransUserId = userId;
                trans.TransValue = value;
                trans.TransDescription = desc;
                dataModel.Trans.Add(trans);
                dataModel.SaveChanges();
            }

        }

        public void PayMoney(int sourceUserId, int destinationUserId, int value)
        {
            using (var dataModel = new MibarimEntities())
            {
                using (var dbContextTransaction = dataModel.Database.BeginTransaction())
                {
                    try
                    {
                        var sourceUserInfo = dataModel.vwUserInfoes.FirstOrDefault(x => x.UserId == sourceUserId);
                        var desUserInfo = dataModel.vwUserInfoes.FirstOrDefault(x => x.UserId == destinationUserId);
                        var desc = string.Format(getResource.getMessage("PayMoneyDesc"), value, GetUserNameFamilyString(sourceUserInfo), GetUserNameFamilyString(desUserInfo));
                        var trans = new Tran();
                        trans.TransCreateTime = DateTime.Now;
                        trans.TransType = (int)TransactionType.PayMoney;
                        trans.TransUserId = sourceUserId;
                        trans.TransValue = (-1) * value;
                        trans.TransDescription = desc;
                        dataModel.Trans.Add(trans);
                        dataModel.SaveChanges();
                        var recTrans = new Tran();
                        recTrans.TransCreateTime = DateTime.Now;
                        recTrans.TransType = (int)TransactionType.ReceivePay;
                        recTrans.TransUserId = destinationUserId;
                        //ServiceWage.Fee = value;
                        recTrans.TransValue = value;//- ServiceWage.Wage;
                        recTrans.TransDescription = desc;
                        recTrans.TransPair = trans.TransId;
                        dataModel.Trans.Add(recTrans);
                        dataModel.SaveChanges();
                        dbContextTransaction.Commit();
                        NotifModel notifModel=new NotifModel();
                        notifModel.Title = getResource.getString("Transaction");
                        notifModel.Body = desc;
                        notifModel.Tab = (int)MainTabs.Profile;
                        notifModel.RequestCode = (int)NotificationType.MoneyTransaction;
                        notifModel.NotificationId = (int)NotificationType.MoneyTransaction;
                        _notifManager.SendNotifToUser(notifModel, sourceUserId);
                        _notifManager.SendNotifToUser(notifModel, destinationUserId);
                    }
                    catch (Exception)
                    {
                        dbContextTransaction.Rollback();
                    }
                }
            }
        }

        public float GetRemain(int userId)
        {
            float remain = 0;
            using (var dataModel = new MibarimEntities())
            {
                var trips = dataModel.Trans.Where(x => x.TransUserId == userId).ToList();
                if (trips.Count > 0)
                {
                    remain = (float)trips.Sum(x => x.TransValue);
                }
            }
            return remain;
        }

        public float GetRemainWithoutGift(int userId)
        {
            float remain = 0;
            using (var dataModel = new MibarimEntities())
            {
                var trips = dataModel.Trans.Where(x => x.TransUserId == userId && x.TransType != (int)TransactionType.GiftChargeAccount).ToList();
                if (trips.Count > 0)
                {
                    remain = (float)trips.Sum(x => x.TransValue);
                }

            }
            return remain;
        }

        private string GetUserNameFamilyString(vwUserInfo user)
        {
            var res = "";
            if (user.Gender == (int)Gender.Man)
            {
                res = " آقای ";
            }
            else if (user.Gender == (int)Gender.Woman)
            {
                res = " خانم ";
            }
            else
            {
                res = "";
            }
            return res + user.Name + " " + user.Family;
        }
    }
}
