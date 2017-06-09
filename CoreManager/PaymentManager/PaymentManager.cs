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
using CoreExternalService;
using CoreManager.NotificationManager;
using CoreManager.TransactionManager;

namespace CoreManager.PaymentManager
{
    public class PaymentManager : IPaymentManager
    {
        private readonly IResponseProvider _responseProvider;
        private readonly ITimingService _timingService;
        private readonly INotificationManager _notifManager;
        private readonly ITransactionManager _transactionManager;
        private readonly IZarinPalService _zarinPalService;

        public PaymentManager(IResponseProvider responseProvider, ITimingService timingService,
            INotificationManager notifManager, ITransactionManager transactionManager)
        {
            _responseProvider = responseProvider;
            _timingService = timingService;
            _notifManager = notifManager;
            _transactionManager = transactionManager;
            _zarinPalService = new ZarinPalService();
        }

        public PaymentDetailModel ChargeAccount(int userId, int chargeValue, string userNameFamilyString)
        {
            var paymentDetailModel = new PaymentDetailModel();
            var desc = string.Format(getResource.getMessage("PaymentDesc"), userNameFamilyString, chargeValue);
            string authority;
            int status = _zarinPalService.RequestAuthoruty(chargeValue, desc, out authority);
            using (var dataModel = new MibarimEntities())
            {
                var pr = new PayReq();
                pr.PayReqCreateTime = DateTime.Now;
                pr.PayReqUserId = userId;
                pr.PayReqValue = chargeValue;
                pr.PayReqStatus = status.ToString();
                if (status == 100)
                {
                    pr.PayReqAuthority = authority;
                }
                dataModel.PayReqs.Add(pr);
                dataModel.SaveChanges();
                paymentDetailModel.ReqId = pr.PayReqId;
            }
            paymentDetailModel.BankLink = "https://www.zarinpal.com/pg/StartPay/" + authority;
            paymentDetailModel.Authority = authority;
            paymentDetailModel.State = status;
            return paymentDetailModel;
        }

        public PaymentDetailModel VerifyPayment(PaymentDetailModel model)
        {
            var paymentDetailModel = new PaymentDetailModel();
            using (var dataModel = new MibarimEntities())
            {
                var amount = dataModel.PayReqs.FirstOrDefault(x => x.PayReqAuthority == model.Authority);
                if (amount != null)
                {
                    long refId;
                    int status = _zarinPalService.VerifyTransaction(model.Authority, amount.PayReqValue, out refId);
                    amount.PayReqStatus = status.ToString();
                    if (status == 100)
                    {
                        amount.PayReqRefID = refId.ToString();
                        var payRoute =
                            dataModel.vwPayRoutes.FirstOrDefault(
                                x => x.PayReqId == amount.PayReqId && x.DrIsDeleted == false);
                        var route =
                            dataModel.vwStationRoutes.FirstOrDefault(x => x.StationRouteId == payRoute.StationRouteId);
                        _transactionManager.PayMoney(amount.PayReqUserId, (int)payRoute.UserId, (int)route.DriverPrice);
                        NotifModel notifModel = new NotifModel();
                        notifModel.Title = getResource.getMessage("SeatReserved");
                        notifModel.Body = string.Format(getResource.getMessage("SeatReservedFor"), route.SrcMainStName,
                            route.DstMainStName, payRoute.TStartTime.ToString("HH:mm"));
                        notifModel.RequestCode = (int) payRoute.PayReqId;
                        notifModel.NotificationId = (int) payRoute.PayReqId;
                        //send passenger notif
                        _notifManager.SendNotifToUser(notifModel, payRoute.PayReqUserId);
                        //send driver notif
                        _notifManager.SendNotifToUser(notifModel, (int) payRoute.UserId);
                        //send passenger sms
                        var user = dataModel.vwUserInfoes.FirstOrDefault(x => x.UserId == payRoute.PayReqUserId);
                        var mobileBrief = user.UserName.Substring(1);
                        string smsBody = string.Format(getResource.getMessage("SeatReservedFor"), route.SrcMainStName,
                            route.DstMainStName, payRoute.TStartTime.ToString("HH:mm"));
                        var smsService = new SmsService();
                        smsService.SendSmsMessages(mobileBrief, smsBody);
                        //send driver sms
                        var driver = dataModel.vwUserInfoes.FirstOrDefault(x => x.UserId == payRoute.UserId);
                        var drivermobileBrief = driver.UserName.Substring(1);
                        string smsBodydriver = string.Format(getResource.getMessage("SeatReservedFor"),
                            route.SrcMainStName, route.DstMainStName, payRoute.TStartTime.ToString("HH:mm"));
                        smsService.SendSmsMessages(drivermobileBrief, smsBodydriver);

                    }
                    dataModel.SaveChanges();
                    paymentDetailModel.RefId = refId;
                }
            }
            return paymentDetailModel;
        }
    }
}