using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using AutoMapper;
using CoreDA;
using CoreExternalService;
using CoreExternalService.Models;
using CoreManager.DiscountManager;
using CoreManager.Models;
using CoreManager.NotificationManager;
using CoreManager.Resources;
using CoreManager.ResponseProvider;
using CoreManager.RouteManager;
using CoreManager.TimingService;
using CoreManager.TransactionManager;
using Encoder = System.Text.Encoder;

namespace CoreManager.FanapManager
{
    public class FanapManager : IFanapManager
    {
        private readonly IResponseProvider _responseProvider;
        private readonly IRouteManager _routeManager;
        private readonly INotificationManager _notifManager;
        private readonly ITransactionManager _transactionManager;
        private readonly IDiscountManager _discountManager;

        public FanapManager(IResponseProvider responseProvider, IRouteManager routeManager,
            INotificationManager notifManager, ITransactionManager transactionManager, IDiscountManager discountManager)
        {
            _responseProvider = responseProvider;
            _routeManager = routeManager;
            _notifManager = notifManager;
            _transactionManager = transactionManager;
            _discountManager = discountManager;
        }

        public FanapManager()
        {
        }

        public UserModel GetFanapUserInfo(FanapModel fanapModel)
        {
            var userInfomodel = new UserModel();
            var fanapService = new FanapService();
            var userInfo = new FanapUserInfo();
            using (var dataModel = new MibarimEntities())
            {
                var cont = fanapService.GetAuthenticationToken(fanapModel.Code);
                if (cont.access_token != null)
                {
                    userInfo = fanapService.GetFanapUser(cont.access_token);
                    if (userInfo != null)
                    {
                        var fanap = new Fanap();
                        fanap.userId = 1;
                        fanap.authorization_code = fanapModel.Code;
                        fanap.access_token = cont.access_token;
                        fanap.refresh_token = cont.refresh_token;
                        fanap.nickName = userInfo.nickName;
                        fanap.birthDate = userInfo.birthDate;
                        fanap.fuserId = userInfo.userId;
                        userInfomodel.UserId = userInfo.userId;
                        userInfomodel.Name = userInfo.firstName;
                        userInfomodel.Family = userInfo.lastName;
                        userInfomodel.UserId = userInfo.userId;
                        userInfomodel.UserName = userInfo.cellphoneNumber;
                        userInfomodel.Email = userInfo.email;
                        fanap.score = userInfo.score.ToString();
                        dataModel.Fanaps.Add(fanap);
                        dataModel.SaveChanges();
                        return userInfomodel;
                    }
                }
            }

            return userInfomodel;
        }



        public void SaveFanapUser(int userId, int fModelUserName)
        {
            using (var dataModel = new MibarimEntities())
            {
                var fanapModel = dataModel.Fanaps.FirstOrDefault(x => x.fuserId == fModelUserName);
                fanapModel.userId = userId;
                dataModel.SaveChanges();
            }
        }

        public PaymentDetailModel FanapBookTrip(int userId, long tripId)
        {
            var res=new PaymentDetailModel();
            var fanapService = new FanapService();
            var pr = new PayReq();
            using (var dataModel = new MibarimEntities())
            {
                var trip = dataModel.vwDriverTrips.FirstOrDefault(x => x.TripId == tripId);
                if (trip != null)
                {
                    pr.PayReqCreateTime = DateTime.Now;
                    pr.PayReqUserId = userId;
                    pr.PayReqValue = (double)trip.PassPrice;
                    dataModel.PayReqs.Add(pr);
                    dataModel.SaveChanges();
                    var bookreq = new BookRequest();
                    bookreq.TripId = tripId;
                    bookreq.BrCreateTime = DateTime.Now;
                    bookreq.BookingType = (int)BookingTypes.Fanap;
                    bookreq.UserId = userId;
                    bookreq.IsBooked = false;
                    bookreq.PayReqId = pr.PayReqId;
                    dataModel.BookRequests.Add(bookreq);
                    dataModel.SaveChanges();
                
                var fanapUser = dataModel.Fanaps.FirstOrDefault(x => x.userId==userId);
                var user=dataModel.vwUserInfoes.FirstOrDefault(x=>x.UserId==userId);
                var ott= fanapService.GetOneTimeToken();
                var desc = string.Format(getResource.getMessage("PaymentDesc"), user.Name+" "+ user.Family, trip.PassPrice);
                var factorId= fanapService.IssueInvoice(trip.PassPrice, fanapUser.fuserId, ott, desc);
                res.BankLink = "http://sandbox.fanapium.com:1031/v1/pbc/payinvoice/?invoiceId=" + factorId
                           + "&redirectUri=" + "http://ifanap.mibarim.ir/fanap/PayReturn/?payreqId=" + pr.PayReqId +
                           "&callUri=" + "http://ifanap.mibarim.ir/fanap/PayReturn/?payreqId=" + pr.PayReqId;
                pr.PayReqRefID = factorId;
                dataModel.SaveChanges();
                    }
                return res;
            }
        }

        public PaymentDetailModel Payresult(long modelReqId)
        {
            var fanapService = new FanapService();
            var res = new PaymentDetailModel();
            using (var dataModel = new MibarimEntities())
            {
                var payreq = dataModel.PayReqs.FirstOrDefault(x => x.PayReqId == modelReqId);
                var payres = fanapService.GetPaymentResult(payreq.PayReqRefID);
                if (payres != null)
                {
                    if (payres.payed)
                    {
                        payreq.PayReqStatus = ((int)PaymentStatus.Payed).ToString();
                        res.State = (int) PaymentStatus.Payed;
                        fanapService.CloseInvoice(payreq.PayReqRefID);
                        var reserve = _routeManager.ReserveSeat(payreq.PayReqId);
                    }
                    else if(payres.canceled)
                    {
                        payreq.PayReqStatus = ((int)PaymentStatus.Canceled).ToString();
                        res.State = (int)PaymentStatus.Canceled;
                    }
                    //fanapService.CloseInvoice(payreq.PayReqRefID);
                    dataModel.SaveChanges();
                }
            }
            return res;
        }
    }
}