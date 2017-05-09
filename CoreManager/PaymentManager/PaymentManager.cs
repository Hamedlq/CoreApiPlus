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
        public PaymentManager(IResponseProvider responseProvider, ITimingService timingService, INotificationManager notifManager, ITransactionManager transactionManager)
        {
            _responseProvider = responseProvider;
            _timingService = timingService;
            _notifManager = notifManager;
            _transactionManager = transactionManager;
            _zarinPalService = new ZarinPalService();
        }

        public PaymentDetailModel ChargeAccount(int userId, int chargeValue, string userNameFamilyString)
        {
            var paymentDetailModel=new PaymentDetailModel();
            var desc = string.Format(getResource.getMessage("PaymentDesc"), userNameFamilyString,chargeValue);
            string authority;
            int status = _zarinPalService.RequestAuthoruty(chargeValue, desc,out authority);
            using (var dataModel = new MibarimEntities())
            {
                var pr = new PayReq();
                pr.PayReqCreateTime=DateTime.Now;
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
            paymentDetailModel.Authority =  authority;
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
                        _transactionManager.ChargeAccount(amount.PayReqUserId, amount.PayReqValue*10);
                    }
                    dataModel.SaveChanges();
                    paymentDetailModel.RefId = refId;
                }
                
            }
            return paymentDetailModel;
        }
    }
}
