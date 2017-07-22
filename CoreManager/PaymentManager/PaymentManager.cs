using System;
using System.Collections.Generic;
using System.Configuration;
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
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Web.Http;
using AutoMapper;
using CoreExternalService;
using CoreExternalService.Models;
using CoreManager.NotificationManager;
using CoreManager.TransactionManager;
using PasargadPayModel = CoreManager.Models.PasargadPayModel;

namespace CoreManager.PaymentManager
{
    public class PaymentManager : IPaymentManager
    {
        private readonly IResponseProvider _responseProvider;
        private readonly ITimingService _timingService;
        private readonly INotificationManager _notifManager;
        private readonly ITransactionManager _transactionManager;
        private readonly IZarinPalService _zarinPalService;
        private readonly IPasargadService _pasargadService;

        public PaymentManager(IResponseProvider responseProvider, ITimingService timingService,
            INotificationManager notifManager, ITransactionManager transactionManager)
        {
            _responseProvider = responseProvider;
            _timingService = timingService;
            _notifManager = notifManager;
            _transactionManager = transactionManager;
            _zarinPalService = new ZarinPalService();
            _pasargadService = new PasargadService();
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
                    int status = _zarinPalService.VerifyTransaction(model.Authority, (int)amount.PayReqValue, out refId);
                    amount.PayReqStatus = status.ToString();
                    if (status == 100)
                    {
                        amount.PayReqRefID = refId.ToString();
                        var payRoute =
                            dataModel.vwPayRoutes.FirstOrDefault(
                                x => x.PayReqId == amount.PayReqId && x.DrIsDeleted == false);
                        var route =
                            dataModel.vwStationRoutes.FirstOrDefault(x => x.StationRouteId == payRoute.StationRouteId);
                        _transactionManager.ChargeAccount(amount.PayReqUserId, (int) route.PassPrice);
                        if (route.DriverPrice > route.PassPrice)
                        {
                            _transactionManager.GiftChargeAccount(amount.PayReqUserId,
                                (int) (route.DriverPrice - route.PassPrice));
                        }
                        _transactionManager.PayMoney(amount.PayReqUserId, (int) payRoute.UserId, (int) route.DriverPrice);
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
                        _notifManager.SendNotifToAdmins(notifModel);
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
                        smsService.SendSmsMessages("9358695785", smsBody);
                        smsService.SendSmsMessages("9354205407", smsBody);
                    }
                    dataModel.SaveChanges();
                    paymentDetailModel.RefId = refId;
                }
            }
            return paymentDetailModel;
        }

        public PasargadResponseModel VerifyPasargadPayment(PasargadPaymentModel model)
        {
            var responseModel = new PasargadResponseModel();
            using (var dataModel = new MibarimEntities())
            {
/*                long payReqId;
                long.TryParse(model.In,out payReqId);*/
                var payreq = dataModel.PayReqs.FirstOrDefault(x => x.PayReqId == model.In);
                if (payreq != null && payreq.PayReqCreateTime.ToString("yyyy/MM/dd HH:mm")==model.Id)
                {
                    payreq.PayReqRefID = model.Tref;
                    dataModel.SaveChanges();
                    var check=_pasargadService.CheckPayment(payreq.PayReqId, payreq.PayReqRefID,payreq.PayReqCreateTime.ToString("yyyy/MM/dd HH:mm"));
                    payreq.TraceNumber = check.TraceNumber;
                    payreq.ReferenceNumber = check.ReferenceNumber;
                    payreq.PayReqStatus = "1001";
                    dataModel.SaveChanges();
                    responseModel.Result = check.Result;
                    responseModel.ReferenceNumber = check.ReferenceNumber;
                    responseModel.TraceNumber = check.TraceNumber;
                    responseModel.ResultMessage = "شماره تراکنش"+ check.TraceNumber;
                    if (check.Result)
                    {
                        var verify= _pasargadService.VerifyPasargadPayment(payreq.PayReqId, payreq.PayReqValue, payreq.PayReqCreateTime.ToString("yyyy/MM/dd HH:mm"));
                        payreq.PayReqStatus = "100";
                        payreq.PayReqAuthority = check.ReferenceNumber;
                        dataModel.SaveChanges();
                        responseModel.Result = verify.Result;
                        responseModel.ResultMessage = verify.ResultMessage;
                    }
                }
            }
            return responseModel;
        }


        public PasargadPayModel ChargePasargad(int userId, long chargeValue, string userNameFamilyString, long tripId,
            DateTime tripTime)
        {
            var pasargadPayModel = new CoreExternalService.Models.PasargadPayModel();
            var passPayModel = new PasargadPayModel();
            var desc = string.Format(getResource.getMessage("PaymentDesc"), userNameFamilyString, chargeValue);
            //(decimal chargeValue, string desc, long tripId, DateTime tripTime)
            pasargadPayModel = _pasargadService.RequestPayment((long) chargeValue, tripId, tripTime);

            using (var dataModel = new MibarimEntities())
            {
                var pr = new PayReq();
                pr.PayReqCreateTime = DateTime.Now;
                pr.PayReqUserId = userId;
                pr.PayReqValue = (int) chargeValue;
                pr.PayReqAuthority = pasargadPayModel.MerchantCode;
                pr.PayReqStatus = desc;
                dataModel.PayReqs.Add(pr);
                dataModel.SaveChanges();
                passPayModel.ReqId = pr.PayReqId;
            }
            passPayModel.Amount = pasargadPayModel.Amount;
            passPayModel.MerchantCode = pasargadPayModel.MerchantCode;
            passPayModel.TerminalCode = pasargadPayModel.TerminalCode;
            passPayModel.Amount = pasargadPayModel.Amount;
            passPayModel.RedirectAddress = pasargadPayModel.RedirectAddress;
            passPayModel.InvoiceNumber = pasargadPayModel.InvoiceNumber;
            passPayModel.InvoiceDate = pasargadPayModel.InvoiceDate;
            passPayModel.TimeStamp = pasargadPayModel.TimeStamp;
            passPayModel.Sign = pasargadPayModel.Sign;
            passPayModel.BankLink = pasargadPayModel.BankLink;
            return passPayModel;
        }

        public PasargadPayModel Getpayment(long modelReqId)
        {
            var passPayModel = new PasargadPayModel();
            using (var dataModel = new MibarimEntities())
            {
                var pr = dataModel.PayReqs.FirstOrDefault(x => x.PayReqId == modelReqId);
                //var trip = dataModel.BookRequests.FirstOrDefault(x => x.PayReqId == (long)modelReqId);
                var pasargadPayModel = _pasargadService.RequestPayment((double)pr.PayReqValue, modelReqId,
                    pr.PayReqCreateTime);
                //pr.PayReqAuthority = modelReqId.ToString();
                pr.PayReqStatus = "200";
                dataModel.SaveChanges();

                passPayModel.Amount = pasargadPayModel.Amount;
                passPayModel.MerchantCode = pasargadPayModel.MerchantCode;
                passPayModel.TerminalCode = pasargadPayModel.TerminalCode;
                passPayModel.RedirectAddress = pasargadPayModel.RedirectAddress;
                passPayModel.InvoiceNumber = pasargadPayModel.InvoiceNumber;
                passPayModel.InvoiceDate = pasargadPayModel.InvoiceDate;
                passPayModel.TimeStamp = pasargadPayModel.TimeStamp;
                passPayModel.Action= pasargadPayModel.Action;
                passPayModel.Sign = pasargadPayModel.Sign;
                passPayModel.BankLink = pasargadPayModel.BankLink;
                return passPayModel;
            }
        }
    }
}