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

namespace CoreManager.UserManager
{
    public class UserManager : IUserManager
    {
        private readonly IResponseProvider _responseProvider;
        private readonly ITimingService _timingService;
        private readonly INotificationManager _notifManager;
        private readonly ITransactionManager _transactionManager;
        private readonly IDiscountManager _discountManager;

        public UserManager(IResponseProvider responseProvider, ITimingService timingService,
            INotificationManager notifManager, ITransactionManager transactionManager, IDiscountManager discountManager)
        {
            _responseProvider = responseProvider;
            _timingService = timingService;
            _notifManager = notifManager;
            _transactionManager = transactionManager;
            _discountManager = discountManager;
        }

        public UserManager()
        {
        }

        public void UpdateUserInfo(ApplicationUser user, RegisterModel model)
        {
            switch (model.UserRole)
            {
                case UserRoles.TaxiAgencyDriver:
                    //AgencyDriver agencyDriver=new AgencyDriver();
                    //agencyDriver.UpdateUserInfo(user,model);
                    break;
            }
            CreateSupportContact(user);
            HandleInvite(user, model.Code);
        }

        private void HandleInvite(ApplicationUser user, string code)
        {
            using (var dataModel = new MibarimEntities())
            {
                var invite = dataModel.Invites.FirstOrDefault(x => x.InviteCode == code);
                if (invite != null)
                {
                    /*_transactionManager.GiftChargeAccount((int)invite.UserId,100000);
                    _transactionManager.GiftChargeAccount(user.Id, 100000);*/
                    var thisInvite = new Invite();
                    thisInvite.CreateTime = DateTime.Now;
                    thisInvite.UserId = user.Id;
                    thisInvite.InviterUserId = invite.UserId;
                    thisInvite.InviterId = invite.InviteId;
                    thisInvite.InviteCode = InviteCodeGenerator();
                    dataModel.Invites.Add(thisInvite);
                    dataModel.SaveChanges();
                    //_notifManager.SendInviteGiftNotif((int) invite.UserId);
                    //_notifManager.SendInviteGiftNotif(user.Id);
                }
            }
        }

        private void CreateSupportContact(ApplicationUser user)
        {
            using (var dataModel = new MibarimEntities())
            {
                Contact contact = new Contact();
                contact.ContactDriverUserId = 1;
                contact.ContactPassengerUserId = user.Id;
                contact.ContactCreateTime = DateTime.Now;
                contact.ContactIsDeleted = false;
                contact.ContactLastMsgTime = DateTime.Now;
                contact.ContactLastMsg = getResource.getMessage("Welcome");
                dataModel.Contacts.Add(contact);
                dataModel.SaveChanges();
                Chat chat = new Chat();
                chat.ChatCreateTime = DateTime.Now;
                chat.ContactId = contact.ContactId;
                chat.ChatIsDeleted = false;
                chat.ChatUserId = 1;
                chat.ChatTxt = getResource.getMessage("Welcome");
                dataModel.Chats.Add(chat);
                dataModel.SaveChanges();
            }
        }

        public ApplicationUser PopulateUpdateModel(RegisterModel model, ApplicationUser user)
        {
            //user.Name = model.Name;
            //user.Family = model.Family;
            //user.Gender = model.Gender;
            user.UserRole = model.UserRole.ToString();
            return user;
        }

        public void UpdatePersoanlInfo(PersoanlInfoModel model, int userId)
        {
            using (var dataModel = new MibarimEntities())
            {
                var uis = dataModel.UserInfoes.FirstOrDefault(x => x.UserId == userId && !x.UserInfoIsDeleted);
                if (uis != null)
                {
                    uis.NationalCode = model.NationalCode;
                    /*                    if (uis.UserPic == null)
                                        {
                                            _responseProvider.SetBusinessMessage(new MessageResponse() { Type = ResponseTypes.Error, Message = getResource.getMessage("UserPicNotUploaded") });
                                        }*/
                }
                else
                {
                    //_responseProvider.SetBusinessMessage(new MessageResponse() { Type = ResponseTypes.Error, Message = getResource.getMessage("UserPicNotUploaded") });
                    var ui = new UserInfo()
                    {
                        NationalCode = model.NationalCode,
                        UserInfoCreateTime = DateTime.Now,
                        UserId = userId,
                        UserInfoIsDeleted = false
                    };
                    dataModel.UserInfoes.Add(ui);
                }
                dataModel.SaveChanges();
            }
        }

        public void UpdateUserInfo(UserInfoModel model, int userId)
        {
            using (var dataModel = new MibarimEntities())
            {
                var uis = dataModel.UserInfoes.FirstOrDefault(x => x.UserId == userId && !x.UserInfoIsDeleted);
                if (uis != null)
                {
                    uis.NationalCode = model.NationalCode;
                }
                else
                {
                    var ui = new UserInfo()
                    {
                        NationalCode = model.NationalCode,
                        UserInfoCreateTime = DateTime.Now,
                        UserId = userId,
                        UserInfoIsDeleted = false
                    };
                    dataModel.UserInfoes.Add(ui);
                }
                var ucar = dataModel.CarInfoes.FirstOrDefault(x => x.UserId == userId);
                if (ucar != null)
                {
                    ucar.CarColor = model.CarColor;
                    ucar.CarPlateNo = model.CarPlateNo;
                    ucar.CarType = model.CarType;
                }
                else
                {
                    var newUcar = new CarInfo()
                    {
                        CarInfoCreateTime = DateTime.Now,
                        UserId = userId,
                        CarInfoIsDeleted = false,
                        CarColor = model.CarColor,
                        CarType = model.CarType,
                        CarPlateNo = model.CarPlateNo
                    };
                    dataModel.CarInfoes.Add(newUcar);
                }
                var ubank = dataModel.BankInfoes.FirstOrDefault(x => x.BankUserId == userId);
                if (ubank != null)
                {
                    ubank.BankName = model.BankName;
                    ubank.BankAccountNo = model.BankAccountNo;
                    ubank.BankShabaNo = model.BankShaba;
                }
                else
                {
                    var bi = new BankInfo()
                    {
                        BankCreateTime = DateTime.Now,
                        BankUserId = userId,
                        BankIsDeleted = false,
                        BankName = model.BankName,
                        BankAccountNo = model.BankAccountNo,
                        BankShabaNo = model.BankShaba
                    };
                    dataModel.BankInfoes.Add(bi);
                }
                dataModel.SaveChanges();
            }
        }

        public PersoanlInfoModel GetPersonalInfo(int userId)
        {
            var pi = new PersoanlInfoModel();
            using (var dataModel = new MibarimEntities())
            {
                var uis = dataModel.vwUserInfoes.FirstOrDefault(x => x.UserId == userId);
                if (uis != null)
                {
                    pi = UserMapper.CastPersonalInfoToModel(uis);
                }
            }
            return pi;
        }

        public PersoanlInfoModel GetPersonalInfoByRouteId(int routeId)
        {
            var pi = new PersoanlInfoModel();
            using (var dataModel = new MibarimEntities())
            {
                var user = dataModel.RouteRequests.FirstOrDefault(x => x.RouteRequestId == routeId);
                if (user != null)
                {
                    var uis = dataModel.vwUserInfoes.FirstOrDefault(x => x.UserId == user.RouteRequestUserId);
                    if (uis != null)
                    {
                        pi = UserMapper.CastPersonalInfoToModel(uis);
                    }
                }
            }
            return pi;
        }

        public void InsertLicenseInfo(LicenseInfoModel model, int userId)
        {
            using (var dataModel = new MibarimEntities())
            {
                var uis = dataModel.LicenseInfoes.FirstOrDefault(x => x.UserId == userId && !x.IsDeleted);
                if (uis != null)
                {
                    if (model != null && model.LicenseNo != null)
                    {
                        uis.LicenseNo = model.LicenseNo;
                    }
                    if (uis.LicensePic == null)
                    {
                        _responseProvider.SetBusinessMessage(new MessageResponse()
                        {
                            Type = ResponseTypes.Error,
                            Message = getResource.getMessage("LicensePicNotUploaded")
                        });
                    }
                }
                else
                {
                    _responseProvider.SetBusinessMessage(new MessageResponse()
                    {
                        Type = ResponseTypes.Error,
                        Message = getResource.getMessage("LicensePicNotUploaded")
                    });
                    if (model != null && model.LicenseNo != null)
                    {
                        var li = new LicenseInfo()
                        {
                            LicenseNo = model.LicenseNo,
                            UserLicenseCreateTime = DateTime.Now,
                            UserId = userId,
                            IsDeleted = false
                        };
                        dataModel.LicenseInfoes.Add(li);
                    }
                }
                dataModel.SaveChanges();
            }
        }

        public void InsertLicensePic(byte[] userLicensepic, int userId)
        {
            using (var dataModel = new MibarimEntities())
            {
                //new method
                var image = new Image();
                image.ImageId = Guid.NewGuid();
                image.ImageCreateTime = DateTime.Now;
                image.ImageType = (int) ImageType.LicensePic;
                image.ImageUserId = userId;
                image.ImageFile = userLicensepic;
                dataModel.Images.Add(image);
                dataModel.SaveChanges();
                //old method
                var ul = dataModel.LicenseInfoes.FirstOrDefault(x => x.UserId == userId && !x.IsDeleted);
                if (ul != null)
                {
                    ul.LicenseImageId = image.ImageId;
                    ul.LicensePic = userLicensepic;
                }
                else
                {
                    var ui = new LicenseInfo()
                    {
                        UserLicenseCreateTime = DateTime.Now,
                        LicensePic = userLicensepic,
                        UserId = userId,
                        IsDeleted = false,
                        LicenseImageId = image.ImageId
                    };
                    dataModel.LicenseInfoes.Add(ui);
                }

                dataModel.SaveChanges();
            }
        }

        public void InsertNationalCardPic(byte[] nationalCardPicModel, int userId)
        {
            using (var dataModel = new MibarimEntities())
            {
                //new method
                var image = new Image();
                image.ImageId = Guid.NewGuid();
                image.ImageCreateTime = DateTime.Now;
                image.ImageType = (int) ImageType.UserNationalCard;
                image.ImageUserId = userId;
                image.ImageFile = nationalCardPicModel;
                dataModel.Images.Add(image);
                dataModel.SaveChanges();
                var ul = dataModel.UserInfoes.FirstOrDefault(x => x.UserId == userId && !x.UserInfoIsDeleted);
                if (ul != null)
                {
                    ul.NationalCardImageId = image.ImageId;
                }
                else
                {
                    var ui = new UserInfo()
                    {
                        UserInfoCreateTime = DateTime.Now,
                        UserId = userId,
                        UserInfoIsDeleted = false,
                        UserImageId = image.ImageId
                    };
                    dataModel.UserInfoes.Add(ui);
                }
                dataModel.SaveChanges();
            }
        }

        public LicenseInfoModel GetLicenseInfo(int userId)
        {
            var li = new LicenseInfoModel();
            using (var dataModel = new MibarimEntities())
            {
                var lis = dataModel.vwLicenseInfoes.FirstOrDefault(x => x.UserId == userId);
                if (lis != null)
                {
                    li.LicenseNo = lis.LicenseNo;
                    li.LicensePic = lis.LicensePic;
                    li.LicenseImageId = lis.LicenseImageId;
                }
            }
            return li;
        }

        public void InsertCarInfo(CarInfoModel model, int userId)
        {
            using (var dataModel = new MibarimEntities())
            {
                var uis = dataModel.CarInfoes.FirstOrDefault(x => x.UserId == userId && !x.CarInfoIsDeleted);
                if (uis != null)
                {
                    uis.CarColor = model.CarColor;
                    uis.CarPlateNo = model.CarPlateNo;
                    uis.CarType = model.CarType;
                }
                else
                {
                    var ci = new CarInfo()
                    {
                        CarInfoCreateTime = DateTime.Now,
                        UserId = userId,
                        CarInfoIsDeleted = false,
                        CarColor = model.CarColor,
                        CarType = model.CarType,
                        CarPlateNo = model.CarPlateNo
                    };
                    dataModel.CarInfoes.Add(ci);
                }
                dataModel.SaveChanges();
                /*                var pics = dataModel.CarPics.Where(x => x.CarCardUserId == userId && !x.CarCardIsDeleted).ToList();
                                if (pics.Count < 2)
                                {
                                    _responseProvider.SetBusinessMessage(new MessageResponse() { Type = ResponseTypes.Error, Message = getResource.getMessage("CarPicNotUploaded") });
                                }*/
            }
        }

        public void InsertCarPic(byte[] carPicModel, int userId)
        {
            using (var dataModel = new MibarimEntities())
            {
                //new image save Method
                var carInfo = dataModel.CarInfoes.FirstOrDefault(x => x.UserId == userId);
                var newImg = new Image();
                newImg.ImageId = Guid.NewGuid();
                newImg.ImageCreateTime = DateTime.Now;
                newImg.ImageType = (int) ImageType.CarPic;
                newImg.ImageUserId = userId;
                newImg.ImageFile = carPicModel;
                dataModel.Images.Add(newImg);
                dataModel.SaveChanges();
                if (carInfo != null)
                {
                    carInfo.CarFrontImageId = newImg.ImageId;
                }
                else
                {
                    var newUcar = new CarInfo()
                    {
                        CarInfoCreateTime = DateTime.Now,
                        UserId = userId,
                        CarInfoIsDeleted = false,
                        CarFrontImageId = newImg.ImageId
                    };
                    dataModel.CarInfoes.Add(newUcar);
                }

                //old image save method
                var ci = dataModel.CarPics.Where(x => x.CarCardUserId == userId && !x.CarCardIsDeleted).ToList();
                if (ci.Count == 2)
                {
                    var last = ci.OrderBy(x => x.CarPicCreateTime);
                    last.FirstOrDefault().CarCardIsDeleted = true;
                }
                var nci = new CarPic()
                {
                    CarPicCreateTime = DateTime.Now,
                    CarCardUserId = userId,
                    CarCardIsDeleted = false,
                    CarCardPic = carPicModel
                };
                dataModel.CarPics.Add(nci);

                dataModel.SaveChanges();
            }
        }

        public void InsertCarBackPic(byte[] carPicModel, int userId)
        {
            using (var dataModel = new MibarimEntities())
            {
                //new image save Method
                var carInfo = dataModel.CarInfoes.FirstOrDefault(x => x.UserId == userId);
                var newImg = new Image();
                newImg.ImageId = Guid.NewGuid();
                newImg.ImageCreateTime = DateTime.Now;
                newImg.ImageType = (int) ImageType.CarBckPic;
                newImg.ImageUserId = userId;
                newImg.ImageFile = carPicModel;
                dataModel.Images.Add(newImg);
                dataModel.SaveChanges();
                if (carInfo != null)
                {
                    carInfo.CarBackImageId = newImg.ImageId;
                }
                else
                {
                    var newUcar = new CarInfo()
                    {
                        CarInfoCreateTime = DateTime.Now,
                        UserId = userId,
                        CarInfoIsDeleted = false,
                        CarBackImageId = newImg.ImageId
                    };
                    dataModel.CarInfoes.Add(newUcar);
                }
                //old image save method
                var ci = dataModel.CarPics.Where(x => x.CarCardUserId == userId && !x.CarCardIsDeleted).ToList();
                if (ci.Count == 2)
                {
                    var last = ci.OrderBy(x => x.CarPicCreateTime);
                    last.FirstOrDefault().CarCardIsDeleted = true;
                }
                var nci = new CarPic()
                {
                    CarPicCreateTime = DateTime.Now,
                    CarCardUserId = userId,
                    CarCardIsDeleted = false,
                    CarCardPic = carPicModel
                };
                dataModel.CarPics.Add(nci);
                dataModel.SaveChanges();
            }
        }

        public CarInfoModel GetCarInfo(int userId)
        {
            var ci = new CarInfoModel();
            using (var dataModel = new MibarimEntities())
            {
                int i = 0;
                var cis = dataModel.vwCarInfoes.Where(x => x.UserId == userId).ToList();
                if (cis.Count > 0)
                {
                    foreach (var vwCarInfo in cis)
                    {
                        if (i == 0)
                        {
                            ci.CarColor = vwCarInfo.CarColor;
                            ci.CarType = vwCarInfo.CarType;
                            ci.CarPlateNo = vwCarInfo.CarPlateNo;
                            ci.CarCardPic = vwCarInfo.CarCardPic;
                            ci.CarFrontImageId = vwCarInfo.CarFrontImageId;
                        }
                        else
                        {
                            ci.CarCardBkPic = vwCarInfo.CarCardPic;
                            ci.CarBackImageId = vwCarInfo.CarBackImageId;
                        }
                        i++;
                    }
                }
            }
            return ci;
        }

        public void UpdatePersoanlPic(byte[] userPicModel, int userId)
        {
            using (var dataModel = new MibarimEntities())
            {
                //new method
                var image = new Image();
                image.ImageId = Guid.NewGuid();
                image.ImageCreateTime = DateTime.Now;
                image.ImageType = (int) ImageType.UserPic;
                image.ImageUserId = userId;
                image.ImageFile = userPicModel;
                dataModel.Images.Add(image);
                dataModel.SaveChanges();
                //old method
                var uis = dataModel.UserInfoes.FirstOrDefault(x => x.UserId == userId && !x.UserInfoIsDeleted);
                if (uis != null)
                {
                    uis.UserPic = userPicModel;
                    uis.UserImageId = image.ImageId;
                }
                else
                {
                    var ui = new UserInfo()
                    {
                        UserInfoCreateTime = DateTime.Now,
                        UserPic = userPicModel,
                        UserId = userId,
                        UserInfoIsDeleted = false,
                        UserImageId = image.ImageId
                    };
                    dataModel.UserInfoes.Add(ui);
                }

                dataModel.SaveChanges();
            }
        }

        public void SubmitContactUs(ContactUsModel model)
        {
            using (var dataModel = new MibarimEntities())
            {
                var cmodel = new ContactU();
                cmodel.ContactName = model.Name;
                cmodel.ContactEmail = model.Email;
                cmodel.ContactTxt = model.Text;
                cmodel.ContactCreateTime = DateTime.Now;
                dataModel.ContactUs.Add(cmodel);
                dataModel.SaveChanges();
            }
        }

        public PersoanlInfoModel GetRouteUserImage(int userId, int routeRequestId)
        {
            var pi = new PersoanlInfoModel();
            using (var dataModel = new MibarimEntities())
            {
                var uis = dataModel.vwRouteRequests.FirstOrDefault(x => x.RouteRequestId == routeRequestId);
                if (uis != null)
                {
                    var upic = dataModel.vwUserInfoes.FirstOrDefault(x => x.UserId == uis.UserId);
                    pi.UserPic = upic.UserPic;
                    pi.UserImageId = upic.UserImageId;
                }
            }
            return pi;
        }

        public PersoanlInfoModel GetCommentUserImage(int userId, int commentId)
        {
            var pi = new PersoanlInfoModel();
            using (var dataModel = new MibarimEntities())
            {
                var uis = dataModel.vwChats.FirstOrDefault(x => x.ChatId == commentId);
                if (uis != null)
                {
                    var upic = dataModel.vwUserInfoes.FirstOrDefault(x => x.UserId == uis.ChatUserId);
                    pi.UserPic = upic.UserPic;
                    pi.UserImageId = upic.UserImageId;
                }
            }
            return pi;
        }

        public void InsertBankInfo(BankInfoModel model, int userId)
        {
            using (var dataModel = new MibarimEntities())
            {
                var uis = dataModel.BankInfoes.FirstOrDefault(x => x.BankUserId == userId && !x.BankIsDeleted);
                if (uis != null)
                {
                    uis.BankName = model.BankName;
                    uis.BankAccountNo = model.BankAccountNo;
                    uis.BankShabaNo = model.BankShaba;
                    //uis.BankCardImageId = model.BankCardImageId;
                }
                else
                {
                    var bi = new BankInfo()
                    {
                        BankCreateTime = DateTime.Now,
                        BankUserId = userId,
                        BankIsDeleted = false,
                        BankName = model.BankName,
                        BankAccountNo = model.BankAccountNo,
                        BankCardNo = model.BankShaba
                    };
                    dataModel.BankInfoes.Add(bi);
                }
                dataModel.SaveChanges();
            }
        }

        public void InsertBankCardPic(byte[] bankCardPic, int userId)
        {
            using (var dataModel = new MibarimEntities())
            {
                //new method
                var image = new Image();
                image.ImageId = Guid.NewGuid();
                image.ImageCreateTime = DateTime.Now;
                image.ImageType = (int) ImageType.BankPic;
                image.ImageUserId = userId;
                image.ImageFile = bankCardPic;
                dataModel.Images.Add(image);
                dataModel.SaveChanges();
                //old method
                var ul = dataModel.BankInfoes.FirstOrDefault(x => x.BankUserId == userId && !x.BankIsDeleted);
                if (ul != null)
                {
                    ul.BankCardPic = bankCardPic;
                    ul.BankCardImageId = image.ImageId;
                }
                else
                {
                    var ui = new BankInfo()
                    {
                        BankCreateTime = DateTime.Now,
                        BankCardPic = bankCardPic,
                        BankUserId = userId,
                        BankIsDeleted = false,
                        BankCardImageId = image.ImageId
                    };
                    dataModel.BankInfoes.Add(ui);
                }

                dataModel.SaveChanges();
            }
        }

        public BankInfoModel GetBankInfo(int userId)
        {
            var bank = new BankInfoModel();
            using (var dataModel = new MibarimEntities())
            {
                var bankdb = dataModel.BankInfoes.FirstOrDefault(x => x.BankUserId == userId && !x.BankIsDeleted);
                if (bankdb != null)
                {
                    bank.BankName = bankdb.BankName;
                    bank.BankAccountNo = bankdb.BankAccountNo;
                    bank.BankShaba = bankdb.BankShabaNo;
                    bank.BankCardPic = bankdb.BankCardPic;
                    bank.BankCardImageId = (Guid) bankdb.BankCardImageId;
                }
            }
            return bank;
        }

        public bool ConfirmMobileNo(string mobile)
        {
            var smsService = new SmsService();
            var allSms = smsService.GetReceivedSmsMessages();
            foreach (var smsMessage in allSms)
            {
                if (smsMessage.MobileNo == mobile)
                {
                    return true;
                }
            }
            return false;
        }

        public bool SendConfirmMobileSms(ApplicationUser model, MobileValidation mobileModel, string rand)
        {
            using (var dataModel = new MibarimEntities())
            {
                var vuis = dataModel.vwUserInfoes.FirstOrDefault(x => x.UserName == mobileModel.Mobile);
                if (vuis != null)
                {
                    var uis = dataModel.UserInfoes.FirstOrDefault(x => x.UserId == vuis.UserId);
                    if (uis != null)
                    {
                        if (uis.Smscount > 10)
                        {
                            sendAdmin(mobileModel.MobileBrief(), rand);
                        }
                        else
                        {
                            if (mobileModel.SendCounter == 1)
                            {
                                sendSmsKav(mobileModel.MobileBrief(), rand);
                            }
                            if (mobileModel.SendCounter == 2)
                            {
                                sendSmsir(mobileModel.MobileBrief(), rand);
                            }
                            if (mobileModel.SendCounter == 3)
                            {
                                sendSoundKav(mobileModel.MobileBrief(), rand);
                            }
                        }
                        if (uis.Smscount == null)
                        {
                            uis.Smscount = 1;
                        }
                        else
                        {
                            uis.Smscount++;
                        }
                        dataModel.SaveChanges();
                    }
                    else
                    {
                        var ui = new UserInfo()
                        {
                            UserInfoCreateTime = DateTime.Now,
                            UserId = model.Id,
                            UserInfoIsDeleted = false,
                            UserUId = Guid.NewGuid(),
                            Smscount = 1
                        };
                        dataModel.UserInfoes.Add(ui);
                        dataModel.SaveChanges();
                        sendSmsir(mobileModel.MobileBrief(), rand);
                    }
                }
            }
            return true;
        }

        private void sendAdmin(string mobileBrief, string rand)
        {
            var notifModel = new NotifModel();
            notifModel.Title = mobileBrief;
            notifModel.Body = rand;
            notifModel.Tab = 1;
            notifModel.NotificationId = 9;
            notifModel.RequestCode = 9;
            _notifManager.SendNotifToUser(notifModel, 1);
            /*var smsService = new KavenegarService();
            smsService.SendAdminSms(mobileBrief, rand);*/
        }

        private void sendSoundKav(string mobileBrief, string rand)
        {
            var smsService = new KavenegarService();
            smsService.SendVoiceMessages(mobileBrief, rand);
        }

        private void sendSmsKav(string mobileBrief, string rand)
        {
            var smsService = new KavenegarService();
            smsService.SendSmsMessages(mobileBrief, rand);
        }

        private void sendSmsir(string mobileBrief, string rand)
        {
            string smsBody = "کد تایید : " + rand + "\r\n Mibarim.com";
            var smsService = new SmsService();
            var allSms = smsService.SendSmsMessages(mobileBrief, smsBody);
        }

        public List<PersoanlInfoModel> GetAllUsers()
        {
            var pi = new List<PersoanlInfoModel>();
            using (var dataModel = new MibarimEntities())
            {
                var uinfo = dataModel.vwUserInfoes.Take(30).OrderByDescending(x => x.UserId).ToList();
                foreach (var vwUserInfo in uinfo)
                {
                    pi.Add(UserMapper.CastPersonalInfoToModel(vwUserInfo));
                }
            }
            return pi;
        }

        public PersoanlInfoModel GetUserPersonalInfoByMobile(string mobile)
        {
            var pi = new PersoanlInfoModel();
            using (var dataModel = new MibarimEntities())
            {
                var uis = dataModel.vwUserInfoes.FirstOrDefault(x => x.UserName == mobile);
                if (uis != null)
                {
                    pi = UserMapper.CastPersonalInfoToModel(uis);
                }
            }
            return pi;
        }

        public LicenseInfoModel GetUserLicenseInfo(string mobile)
        {
            var li = new LicenseInfoModel();
            using (var dataModel = new MibarimEntities())
            {
                var uis = dataModel.vwUserInfoes.FirstOrDefault(x => x.UserName == mobile);
                if (uis != null)
                {
                    var lis = dataModel.vwLicenseInfoes.FirstOrDefault(x => x.UserId == uis.UserId);
                    if (lis != null)
                    {
                        li.LicenseNo = lis.LicenseNo;
                        li.LicensePic = lis.LicensePic;
                        li.LicenseImageId = lis.LicenseImageId;
                    }
                }
            }
            return li;
        }

        public CarInfoModel GetUserCarInfo(string mobile)
        {
            var ci = new CarInfoModel();
            using (var dataModel = new MibarimEntities())
            {
                var uis = dataModel.vwUserInfoes.FirstOrDefault(x => x.UserName == mobile);
                if (uis != null)
                {
                    int i = 0;
                    var cis = dataModel.vwCarInfoes.Where(x => x.UserId == uis.UserId).ToList();
                    if (cis.Count > 0)
                    {
                        foreach (var vwCarInfo in cis)
                        {
                            if (i == 0)
                            {
                                ci.CarColor = vwCarInfo.CarColor;
                                ci.CarType = vwCarInfo.CarType;
                                ci.CarPlateNo = vwCarInfo.CarPlateNo;
                                ci.CarCardPic = vwCarInfo.CarCardPic;
                                ci.CarFrontImageId = vwCarInfo.CarFrontImageId;
                            }
                            else
                            {
                                ci.CarCardBkPic = vwCarInfo.CarCardPic;
                                ci.CarBackImageId = vwCarInfo.CarBackImageId;
                            }
                            i++;
                        }
                    }
                }
            }
            return ci;
        }

        public NotificationModel GetNotifications(string mobile)
        {
            var notifModel = new NotificationModel();
            using (var dataModel = new MibarimEntities())
            {
                var notif = dataModel.vwSuggestNotifications.FirstOrDefault(x => x.UserName == mobile);
                if (notif != null)
                {
                    notifModel.IsNewRouteSuggest = true;
                    notifModel.SuggestRouteRequestId = notif.RouteRequestId;
                    var notifTable =
                        dataModel.RouteSuggests.FirstOrDefault(x => x.RouteSuggestId == notif.RouteSuggestId);
                    if (notifTable != null) notifTable.IsSuggestSent = true;
                    dataModel.SaveChanges();
                }
                var msg = dataModel.vwCommentNotifications.FirstOrDefault(x => x.UserName == mobile);
                if (msg != null)
                {
                    notifModel.IsNewMessage = true;
                    notifModel.MessageRouteRequestId = msg.RouteRequestId;
                    var notificationTable =
                        dataModel.Notifications.FirstOrDefault(x => x.NotificationId == msg.NotificationId);
                    if (notificationTable != null) notificationTable.IsNotificationSent = true;
                    dataModel.SaveChanges();
                }
            }
            return notifModel;
        }

        public List<ContactModel> GetUserContacts(int userId)
        {
            var pilist = new List<ContactModel>();
            var contactModel = new ContactModel();
            using (var dataModel = new MibarimEntities())
            {
                var contacts =
                    dataModel.Contacts.Where(
                        x =>
                            (x.ContactPassengerUserId == userId || x.ContactDriverUserId == userId) &&
                            !x.ContactIsDeleted).OrderByDescending(x => x.ContactLastMsgTime).Take(10);
                var userIds =
                    contacts.Where(x => x.ContactDriverUserId == userId).Select(x => x.ContactPassengerUserId).ToList();
                userIds.AddRange(
                    contacts.Where(x => x.ContactPassengerUserId == userId).Select(x => x.ContactDriverUserId).ToList());
                //var chats = dataModel.Chats.Where(x => contacts.Select(y => y.ContactId).Contains(x.ContactId)).OrderByDescending(x=>x.ChatCreateTime);
                var users = dataModel.vwUserInfoes.Where(x => userIds.Contains(x.UserId));
                foreach (var contact in contacts)
                {
                    contactModel = new ContactModel();
                    var user =
                        users.FirstOrDefault(
                            x =>
                                (x.UserId == contact.ContactPassengerUserId ||
                                 x.UserId == contact.ContactDriverUserId) && x.UserId != userId);
                    contactModel.ContactId = contact.ContactId;
                    contactModel.Name = user.Name;
                    contactModel.Family = user.Family;
                    contactModel.Gender = (Gender) user.Gender;
                    if (contact.ContactLastMsgTime != null)
                    {
                        contactModel.LastMsgTime = _timingService.GetTimingString((DateTime) contact.ContactLastMsgTime);
                    }
                    contactModel.IsSupport = 0;
                    if (contact.ContactDriverUserId == 1)
                    {
                        contactModel.IsSupport = 1;
                    }
                    contactModel.IsDriver = contact.ContactDriverUserId == userId ? 1 : 0;
                    contactModel.IsRideAccepted = contact.ContactIsRideAccepted == null
                        ? 0
                        : ((bool) contact.ContactIsRideAccepted ? 1 : 0);
                    contactModel.IsPassengerAccepted = contact.IsPassengerAccepted == null
                        ? 0
                        : ((bool) contact.IsPassengerAccepted ? 1 : 0);


                    contactModel.LastMsg = contact.ContactLastMsg.Replace("\r\n", " ")
                        .Replace("\n", " ")
                        .Replace("\r", " ");
                    if (contactModel.UserPic != null)
                    {
                        contactModel.UserPic = UserMapper.ResizeImage(user.UserPic, 200, 160);
                    }
                    contactModel.UserImageId = user.UserImageId;
                    contactModel.AboutUser = GetUserAboutMe(user.UserId).Desc;
                    contactModel.NewChats =
                        dataModel.Chats.Count(
                            x =>
                                x.ChatUserId == contact.ContactPassengerUserId ||
                                x.ChatUserId == contact.ContactPassengerUserId && !x.IsChatSeen);
                    pilist.Add(contactModel);
                }
            }
            return pilist;
        }

        public ScoreModel GetUserScores(int userId)
        {
            var s = new ScoreModel();
            var remain = _transactionManager.GetRemain(userId);
            s.CreditMoney = (long) remain;
            s.CreditMoneyString = remain.ToString("N0", new NumberFormatInfo()
            {
                NumberGroupSizes = new[] {3},
                NumberGroupSeparator = ","
            });

            using (var dataModel = new MibarimEntities())
            {
                s.Score =
                    dataModel.vwTripRoutes.Where(
                            x => x.RouteRequestUserId == userId && x.TrState == (int) TripRouteState.TripRouteFinished)
                        .ToList()
                        .Count;
            }
            s.MoneySave = 0;
            return s;
        }

        public ScoreModel GetPassScores(int userId, PayModel paymodel)
        {
            var s = new ScoreModel();
            var remain = _transactionManager.GetRemain(userId);
            using (var dataModel = new MibarimEntities())
            {
                s.Score =
                    dataModel.vwTripRoutes.Where(
                            x => x.RouteRequestUserId == userId && x.TrState == (int) TripRouteState.TripRouteFinished)
                        .ToList()
                        .Count;
                var discount =
                    dataModel.vwDiscountUsers.FirstOrDefault(
                        x =>
                            x.UserId == userId && x.DiscountEndTime > DateTime.Now && x.DuEndTime > DateTime.Now &&
                            x.DuState == (int) DiscountStates.Submitted);
                if (discount != null)
                {
                    //elecomp 50 discount
                    var trips = dataModel.BookRequests.Count(x => x.IsBooked.Value && x.UserId == userId);
                    if (trips > 0)
                    {
                        remain = remain + Convert.ToSingle((paymodel.SeatPrice)*0.5);
                    }
                    else
                    {
                        switch (discount.DiscountType)
                        {
                            case (int) DiscountTypes.EndlessFirstFreeTrip:
                            case (int) DiscountTypes.FirstFreeTrip:
                            case (int) DiscountTypes.EndlessFreeSeat:
                            case (int) DiscountTypes.FreeSeat:
                            case (int) DiscountTypes.AlwaysFreeSeat:
                                remain = remain + paymodel.SeatPrice;
                                break;
                        }
                    }
                } //elecomp
                else
                {
                    var dc = dataModel.Discounts.FirstOrDefault(x => x.DiscountCode == "elecomprequest");
                    var elediscount =
                        dataModel.vwDiscountUsers.FirstOrDefault(
                            x =>
                                    x.UserId == userId && x.DiscountId == dc.DiscountId);
                    if (elediscount == null)
                    {
                        var discountUser = new DiscountUser();
                        discountUser.UserId = userId;
                        discountUser.DiscountId = dc.DiscountId;
                        discountUser.DuCreateTime = DateTime.Now;
                        discountUser.DuEndTime = DateTime.Now.AddDays(6);
                        discountUser.DuState = (int) DiscountStates.Submitted;
                        dataModel.DiscountUsers.Add(discountUser);
                        dataModel.SaveChanges();
                        remain = remain + paymodel.SeatPrice;
                    }
                }
            }
            s.CreditMoney = (long) remain;
            s.CreditMoneyString = remain.ToString("N0", new NumberFormatInfo()
            {
                NumberGroupSizes = new[] {3},
                NumberGroupSeparator = ","
            });
            s.MoneySave = 0;
            return s;
        }

        public void InsertGoogleToken(int userId, Gtoken model)
        {
            using (var dataModel = new MibarimEntities())
            {
                var googletoken = new GoogleToken();
                googletoken.GtokenCreateTime = DateTime.Now;
                googletoken.GtokenUserId = userId;
                googletoken.GtokenKey = model.Token;
                dataModel.GoogleTokens.Add(googletoken);
                dataModel.SaveChanges();
            }
        }

        public List<PersoanlInfoModel> GetUserByInfo(UserSearchModel model)
        {
            var pi = new List<PersoanlInfoModel>();
            using (var dataModel = new MibarimEntities())
            {
                var uinfo =
                    dataModel.vwUserInfoes.Where(
                        x => (!string.IsNullOrEmpty(x.UserName) && x.UserName == model.Mobile) ||
                             (!string.IsNullOrEmpty(x.Name) && x.Name == model.Name) ||
                             (!string.IsNullOrEmpty(x.Family) && x.Family == model.Family));
                foreach (var vwUserInfo in uinfo)
                {
                    pi.Add(UserMapper.CastPersonalInfoToModelWithoutPic(vwUserInfo));
                }
            }
            return pi;
        }

        public ImageResponse GetImageById(ImageRequest model)
        {
            var img = new ImageResponse();
            using (var dataModel = new MibarimEntities())
            {
                var res = dataModel.Images.FirstOrDefault(x => x.ImageId == model.ImageId);
                if (res != null)
                {
                    img.ImageId = res.ImageId.ToString();
                    img.ImageFile = UserMapper.ResizeImage(res.ImageFile, 200, 160);
                    img.ImageType = res.ImageType.ToString();
                }
            }
            return img;
        }

        public long GetUserTrip(int userId)
        {
            using (var dataModel = new MibarimEntities())
            {
                var res = dataModel.UserInfoes.FirstOrDefault(x => x.UserId == userId);
                if (res.TripId != null)
                {
                    return (long) res.TripId;
                }
            }
            return 0;
        }


        public void SaveUserTripLocation(int userId, UserLocation point)
        {
            using (var dataModel = new MibarimEntities())
            {
                var loc = new TripLocation();
                loc.TlCreateTime = DateTime.Now;
                loc.TlUserId = userId;
                loc.TlLat = decimal.Parse(point.Lat);
                loc.TlLng = decimal.Parse(point.Lng);
                loc.TlGeo = RouteMapper.CreatePoint(point.Lat, point.Lng);
                dataModel.TripLocations.Add(loc);
                dataModel.SaveChanges();
            }
        }

        //public string AcceptRideShare(int userId, long contactId)
        //{
        //    using (var dataModel = new MibarimEntities())
        //    {
        //        var isChat = dataModel.Chats.Where(x => x.ContactId == contactId).Select(x => x.ChatUserId).Distinct().Count();
        //        if (isChat == 1)
        //        {
        //            return getResource.getMessage("AppointmentNotSet");
        //        }

        //        var cont = dataModel.Contacts.FirstOrDefault(x => x.ContactId == contactId);
        //        if (cont != null && cont.ContactDriverUserId == userId)
        //        {
        //            cont.ContactIsRideAccepted = true;
        //            //cont.ContactIsRideAccepted = true;
        //            IList<vwTwoRouteSuggest> similarTrips =
        //                dataModel.vwTwoRouteSuggests.Where(x =>
        //                        x.SelfRRUserId == userId && x.SuggestRRUserId == cont.ContactPassengerUserId &&
        //                        !x.IsSuggestRejected).ToList();

        //            foreach (var vwTwoRouteSuggest in similarTrips)
        //            {
        //                var selfRouteRequest =
        //                        dataModel.RouteRequests.FirstOrDefault(
        //                            x => x.RouteRequestId == (int)vwTwoRouteSuggest.SelfRouteRequestId);
        //                selfRouteRequest.RouteRequestState = (int)RouteRequestState.RideShareAccepted;

        //                var time =
        //                    dataModel.RRTimings.FirstOrDefault(
        //                        x => x.RouteRequestId == vwTwoRouteSuggest.SelfRouteRequestId);
        //                var newTrip = dataModel.Trips.FirstOrDefault(x => x.DriverRouteRequestId == vwTwoRouteSuggest.SelfRouteRequestId && x.TripState == (int)TripState.Scheduled);
        //                if (newTrip == null)
        //                {
        //                    newTrip = new Trip();
        //                }
        //                newTrip.TripCreateTime = DateTime.Now;
        //                newTrip.TripStartTime = _timingService.GetNextOccurance(time);
        //                newTrip.DriverRouteRequestId = vwTwoRouteSuggest.SelfRouteRequestId;
        //                newTrip.TripState = (int)TripState.Scheduled;
        //                dataModel.Trips.Add(newTrip);
        //                dataModel.SaveChanges();
        //                var driverTripRoute = new TripRoute();
        //                driverTripRoute.TrCreateTime = DateTime.Now;
        //                driverTripRoute.TrModifyTime = DateTime.Now;
        //                driverTripRoute.TrTripId = newTrip.TripId;
        //                driverTripRoute.TrRouteRequestId = vwTwoRouteSuggest.SelfRouteRequestId;
        //                dataModel.TripRoutes.Add(driverTripRoute);
        //                var passengerTripRoute = new TripRoute();
        //                passengerTripRoute.TrCreateTime = DateTime.Now;
        //                passengerTripRoute.TrModifyTime = DateTime.Now;
        //                passengerTripRoute.TrTripId = newTrip.TripId;
        //                passengerTripRoute.TrRouteRequestId = vwTwoRouteSuggest.SuggestRouteRequestId;
        //                dataModel.TripRoutes.Add(passengerTripRoute);
        //                dataModel.SaveChanges();
        //            }
        //            var chat = new Chat();
        //            chat.ChatCreateTime = DateTime.Now;
        //            chat.ChatIsDeleted = false;
        //            chat.ContactId = cont.ContactId;
        //            chat.ChatUserId = userId;
        //            chat.ChatTxt = getResource.getMessage("TripAccepted");
        //            dataModel.Chats.Add(chat);
        //            cont.ContactLastMsgTime = DateTime.Now;
        //            cont.ContactLastMsg = UserMapper.Truncate(getResource.getMessage("TripAccepted"), 29);
        //            dataModel.SaveChanges();
        //            _notifManager.SendTripNotifications(cont);
        //            //ScheduleTripTimes();
        //        }

        //    }
        //    return "";
        //}

        //public string InvokeTrips()
        //{
        //    using (var dataModel = new MibarimEntities())
        //    {
        //        var currentTrips = dataModel.vwContactTrips;
        //        var contactTripRequestIds = currentTrips.Select(x => x.RouteRequestId).ToList();
        //        var timings = _timingService.GetRequestTimings(contactTripRequestIds);
        //        foreach (var vwRrTiming in timings)
        //        {
        //            if (_timingService.IsCurrentTiming(vwRrTiming, 20))
        //            {
        //                var theTrip = currentTrips.FirstOrDefault(x => x.RouteRequestId == vwRrTiming.RouteRequestId);
        //                var driveUser = dataModel.UserInfoes.FirstOrDefault(x => x.UserId == theTrip.ContactDriverUserId);
        //                //add trip
        //                var newTrip = new Trip();
        //                if (driveUser.TripId == null || driveUser.TripId == 0)
        //                {
        //                    newTrip.TripCreateTime = DateTime.Now;
        //                    newTrip.TripStartTime = DateTime.Now;
        //                    newTrip.DriverRouteRequestId = vwRrTiming.RouteRequestId;
        //                    newTrip.TripState = (int)TripState.Scheduled;
        //                    dataModel.Trips.Add(newTrip);
        //                    dataModel.SaveChanges();
        //                }
        //                else
        //                {
        //                    newTrip.TripId = (long)driveUser.TripId;
        //                }

        //                var driverTripRoute = new TripRoute();
        //                driverTripRoute.TrCreateTime = DateTime.Now;
        //                driverTripRoute.TrModifyTime = DateTime.Now;
        //                driverTripRoute.TrTripId = newTrip.TripId;
        //                driverTripRoute.TrRouteRequestId = vwRrTiming.RouteRequestId;
        //                dataModel.TripRoutes.Add(driverTripRoute);

        //                var passenger =
        //                        dataModel.Contacts.FirstOrDefault(x => x.ContactId == theTrip.ContactId);
        //                var thePassenger = dataModel.vwTwoRouteSuggests.FirstOrDefault(x =>
        //                    x.SelfRRUserId == theTrip.ContactDriverUserId && x.SelfRouteRequestId == theTrip.RouteRequestId
        //                    && x.SuggestRRUserId == passenger.ContactPassengerUserId);
        //                if (thePassenger != null)
        //                {
        //                    var passengerTripRoute = new TripRoute();
        //                    passengerTripRoute.TrCreateTime = DateTime.Now;
        //                    passengerTripRoute.TrModifyTime = DateTime.Now;
        //                    passengerTripRoute.TrTripId = newTrip.TripId;
        //                    passengerTripRoute.TrRouteRequestId = thePassenger.SuggestRouteRequestId;
        //                    dataModel.TripRoutes.Add(passengerTripRoute);
        //                    dataModel.SaveChanges();
        //                }


        //                //then invoke
        //                newTrip.TripState = (int)TripState.Alerted;
        //                var routes = dataModel.vwTripRoutes.Where(x => x.TrTripId == newTrip.TripId);
        //                foreach (var vwTripRoute in routes)
        //                {
        //                    _notifManager.SendTripStartNotifications(vwTripRoute.RouteRequestUserId);
        //                    var user = dataModel.UserInfoes.FirstOrDefault(x => x.UserId == vwTripRoute.RouteRequestUserId);
        //                    if (user != null)
        //                    {
        //                        user.TripId = newTrip.TripId;
        //                    }
        //                    else
        //                    {
        //                        var ui = new UserInfo() { TripId = newTrip.TripId, UserInfoCreateTime = DateTime.Now, UserId = vwTripRoute.RouteRequestUserId, UserInfoIsDeleted = false };
        //                        dataModel.UserInfoes.Add(ui);
        //                    }
        //                    var tripRoute = dataModel.TripRoutes.FirstOrDefault(x => x.TripRouteId == vwTripRoute.TripRouteId);
        //                    if (tripRoute != null)
        //                    {
        //                        tripRoute.TrState = (int)TripRouteState.TripRouteAlerted;
        //                        tripRoute.TrModifyTime = DateTime.Now;
        //                    }
        //                }
        //                dataModel.SaveChanges();
        //            }

        //        }
        //        //pay money after half an hour
        //        var alertedTrip = dataModel.vwTripRoutes.Where(x => x.TrState == (int)TripRouteState.TripRouteAlerted).ToList();
        //        if (alertedTrip.Count > 0)
        //        {
        //            foreach (var tripRoute in alertedTrip)
        //            {
        //                if (tripRoute.TrModifyTime < DateTime.Now.AddMinutes(-30))
        //                {
        //                    if (!tripRoute.IsDrive)
        //                    {
        //                        var route = dataModel.TripRoutes.FirstOrDefault(x => x.TripRouteId == tripRoute.TripRouteId);
        //                        //finish it
        //                        route.TrState = (int)TripRouteState.TripRouteFinished;
        //                        route.TrModifyTime = DateTime.Now;
        //                        var user = dataModel.UserInfoes.FirstOrDefault(x => x.UserId == tripRoute.RouteRequestUserId);
        //                        user.TripId = 0;
        //                        dataModel.SaveChanges();
        //                        var tripDriver =
        //                            dataModel.vwTripRoutes.FirstOrDefault(x => x.TrTripId == tripRoute.TrTripId && x.IsDrive);
        //                        var pay = dataModel.RRPricings.FirstOrDefault(x => x.RouteRequestId == tripRoute.RouteRequestId);
        //                        _transactionManager.PayMoney(tripRoute.RouteRequestUserId, tripDriver.RouteRequestUserId, (int)pay.RRPricingMinMax);
        //                    }
        //                    else
        //                    {
        //                        var trip = dataModel.Trips.FirstOrDefault(x => x.TripId == tripRoute.TrTripId);
        //                        trip.TripEndTime = DateTime.Now;
        //                        trip.TripState = (int)TripState.Finished;
        //                        var route = dataModel.TripRoutes.FirstOrDefault(x => x.TripRouteId == tripRoute.TripRouteId);
        //                        route.TrState = (int)TripRouteState.TripRouteFinished;
        //                        route.TrModifyTime = DateTime.Now;
        //                        var user = dataModel.UserInfoes.FirstOrDefault(x => x.UserId == tripRoute.RouteRequestUserId);
        //                        user.TripId = 0;
        //                        dataModel.SaveChanges();
        //                    }
        //                }

        //            }
        //        }
        //        //half joined
        //        var joinedTrip = dataModel.vwTripRoutes.Where(x => x.TrState == (int)TripRouteState.TripRouteJoined).ToList();
        //        if (joinedTrip.Count > 0)
        //        {
        //            foreach (var tripRoute in joinedTrip)
        //            {
        //                if (tripRoute.TrModifyTime < DateTime.Now.AddMinutes(-30))
        //                {
        //                    var hamsafars = dataModel.vwTripRoutes.Where(x => x.TrTripId == tripRoute.TrTripId).ToList();
        //                    var joinedhamsafars = hamsafars.Where(x => x.TrState == (int)TripRouteState.TripRouteJoined).ToList();
        //                    if (hamsafars.Count != joinedhamsafars.Count)
        //                    {
        //                        NotifModel notifModel = new NotifModel();
        //                        notifModel.Title = "ye safar naghes";
        //                        notifModel.Body = "tripId" + tripRoute.TrTripId.ToString();
        //                        notifModel.Tab = (int)MainTabs.Route;
        //                        notifModel.RequestCode = (int)NotificationType.AdminNotif;
        //                        notifModel.NotificationId = (int)NotificationType.AdminNotif;
        //                        _notifManager.SendNotifToUser(notifModel, 1);
        //                    }
        //                    if (tripRoute.TrModifyTime < DateTime.Now.AddMinutes(-70))
        //                    {
        //                        var route = dataModel.TripRoutes.FirstOrDefault(x => x.TripRouteId == tripRoute.TripRouteId);
        //                        //finish it
        //                        route.TrState = (int)TripRouteState.TripRouteNotJoined;
        //                        route.TrModifyTime = DateTime.Now;
        //                        var user = dataModel.UserInfoes.FirstOrDefault(x => x.UserId == tripRoute.RouteRequestUserId);
        //                        user.TripId = 0;
        //                        dataModel.SaveChanges();
        //                    }

        //                }

        //            }

        //        }
        //    }

        //    return "Done";
        //}


        public UserInfoModel GetUserInfo(int userId)
        {
            var ui = new UserInfoModel();
            using (var dataModel = new MibarimEntities())
            {
                var uis = dataModel.vwUserInfoes.FirstOrDefault(x => x.UserId == userId);
                if (uis != null)
                {
                    ui = UserMapper.CastPersonalInfoToUserInfoModel(uis);
                }
                if (uis == null || string.IsNullOrEmpty(uis.Name))
                {
                    var fanap = dataModel.Fanaps.FirstOrDefault(x => x.userId == userId);
                    if (fanap != null)
                    {
                        ui.IsUserRegistered = true;
                    }
                    else
                    {
                        ui.IsUserRegistered = false;
                    }
                }
                else
                {
                    ui.IsUserRegistered = true;
                }
                var lis = dataModel.LicenseInfoes.FirstOrDefault(x => x.UserId == userId && !x.IsDeleted);
                if (lis != null)
                {
                    ui.LicenseImageId = lis.LicenseImageId;
                }
                var cis = dataModel.CarInfoes.FirstOrDefault(x => x.UserId == userId && !x.CarInfoIsDeleted);
                if (cis != null)
                {
                    ui = UserMapper.FillCarInfoInUserInfoModel(ui, cis);
                }
                var bankdb = dataModel.BankInfoes.FirstOrDefault(x => x.BankUserId == userId && !x.BankIsDeleted);
                if (bankdb != null)
                {
                    ui = UserMapper.FillBankInfoInUserInfoModel(ui, bankdb);
                }
                var comp = dataModel.vwCompanies.FirstOrDefault(x => x.Id == userId);
                if (comp != null)
                {
                    ui = UserMapper.FillCompanyInfoInUserInfoModel(ui, comp);
                }
            }
            return ui;
        }

        public bool DiscountCodeExist(DiscountModel model)
        {
            using (var dataModel = new MibarimEntities())
            {
                var dc = dataModel.Discounts.Where(x => x.DiscountCode == model.DiscountCode).ToList();
                return dc.Count > 0;
            }
        }

        public void InsertDiscountCode(DiscountModel model, int userId)
        {
            using (var dataModel = new MibarimEntities())
            {
                var dc = dataModel.Discounts.FirstOrDefault(x => x.DiscountCode == model.DiscountCode);
                var discount = new DiscountUser();
                discount.DiscountId = dc.DiscountId;
                discount.UserId = userId;
                discount.DuCreateTime = DateTime.Now;
                discount.DuState = (int) DiscountStates.Submitted;
                dataModel.DiscountUsers.Add(discount);
                dataModel.SaveChanges();
            }
        }

        public void InsertWithdrawRequest(WithdrawRequestModel model, int userId)
        {
            var remain = _transactionManager.GetRemain(userId);
            using (var dataModel = new MibarimEntities())
            {
                var withdraw = new Withdraw();
                withdraw.WithdrawAmount = model.WithdrawAmount;
                withdraw.CreateTime = DateTime.Now;
                withdraw.LastModification = DateTime.Now;
                withdraw.UserBalance = remain;
                withdraw.UserId = userId;
                withdraw.WithdrawState = (int) WithdrawStates.Submitted;
                dataModel.Withdraws.Add(withdraw);
                dataModel.SaveChanges();
            }
        }

        public List<DiscountModel> GetUserDiscount(int userId)
        {
            var res = new List<DiscountModel>();
            using (var dataModel = new MibarimEntities())
            {
                var discounts =
                    dataModel.vwDiscountUsers.Where(x => x.UserId == userId)
                        .OrderByDescending(x => x.DuCreateTime)
                        .ToList();
                foreach (var vwDiscountUser in discounts)
                {
                    var dis = new DiscountModel();
                    dis.DiscountCode = vwDiscountUser.DiscountCode;
                    dis.DiscountTitle = vwDiscountUser.DiscountTitle;
                    dis.DiscountStateString = getResource.getString(((DiscountStates) vwDiscountUser.DuState).ToString());
                    res.Add(dis);
                }
            }
            return res;
        }

        public bool DiscountCodeUsed(DiscountModel model, int userId)
        {
            using (var dataModel = new MibarimEntities())
            {
                var codeUsed =
                    dataModel.vwDiscountUsers.Where(x => x.UserId == userId && x.DiscountCode == model.DiscountCode)
                        .ToList();
                return codeUsed.Count > 0;
            }
        }

        public void InsertAboutUser(AboutUserModel model, int userId)
        {
            using (var dataModel = new MibarimEntities())
            {
                var about = new AboutUser();
                about.AboutCreateTime = DateTime.Now;
                about.UserId = userId;
                about.AboutDesc = model.Desc;
                dataModel.AboutUsers.Add(about);
                dataModel.SaveChanges();
            }
        }

        public AboutUserModel GetUserAboutMe(int userId)
        {
            var res = new AboutUserModel();

            using (var dataModel = new MibarimEntities())
            {
                var aboutMe =
                    dataModel.AboutUsers.Where(x => x.UserId == userId)
                        .OrderByDescending(x => x.AboutCreateTime)
                        .FirstOrDefault();
                if (aboutMe != null)
                {
                    res.Desc = aboutMe.AboutDesc;
                }
                return res;
            }
        }

        public List<WithdrawRequestModel> GetWithdraw(int userId)
        {
            var res = new List<WithdrawRequestModel>();
            using (var dataModel = new MibarimEntities())
            {
                var withdrawReqs =
                    dataModel.Withdraws.Where(x => x.UserId == userId).OrderByDescending(x => x.CreateTime).ToList();
                foreach (var withdrawReq in withdrawReqs)
                {
                    var with = new WithdrawRequestModel();
                    with.WithdrawAmount = withdrawReq.WithdrawAmount;
                    with.WithdrawDate = _timingService.GetTimingString(withdrawReq.CreateTime);
                    with.WithdrawState = (WithdrawStates) withdrawReq.WithdrawState;
                    with.WithdrawStateString =
                        getResource.getString(((WithdrawStates) withdrawReq.WithdrawState).ToString());
                    res.Add(with);
                }
            }
            return res;
        }

        public bool WithdrawlValid(WithdrawRequestModel model, int userId)
        {
            var remain = _transactionManager.GetRemainWithoutGift(userId);
            if (model.WithdrawAmount == 0)
            {
                return false;
            }
            if (model.WithdrawAmount > remain)
            {
                return false;
            }
            using (var dataModel = new MibarimEntities())
            {
                double withdraw = 0;
                var withdrawquery =
                    dataModel.Withdraws.Where(
                        x => x.UserId == userId && x.WithdrawState == (int) WithdrawStates.Submitted).ToList();
                if (withdrawquery.Count > 0)
                {
                    withdraw = withdrawquery.Sum(x => x.WithdrawAmount);
                }
                if (model.WithdrawAmount > remain - withdraw)
                {
                    return false;
                }
            }
            return true;
        }

        public InviteModel GetUserInvite(int userId, InviteTypes inviteType)
        {
            var res = new InviteModel();

            using (var dataModel = new MibarimEntities())
            {
                var invite =
                    dataModel.Invites.FirstOrDefault(x => x.UserId == userId && x.InviteType == (int) inviteType);
                if (invite != null)
                {
                    res.InviteCode = invite.InviteCode;
                    if (inviteType == InviteTypes.PassInvite)
                    {
                        res.InviteLink = String.Format(getResource.getMessage("InviteLink"), invite.InviteCode);
                    }
                    else
                    {
                        res.InviteLink = String.Format(getResource.getMessage("InviteLink"), invite.InviteCode);
                    }
                }
                else
                {
                    invite = new Invite();
                    invite.CreateTime = DateTime.Now;
                    invite.UserId = userId;
                    invite.InviteType = (short?) inviteType;
                    invite.InviteCode = InviteCodeGenerator();
                    dataModel.Invites.Add(invite);
                    dataModel.SaveChanges();
                    //dataModel.SaveChanges();
                    res.InviteCode = invite.InviteCode;
                    res.InviteLink = String.Format(getResource.getMessage("InviteLink"), invite.InviteCode);
                    //UserMapper.PersianNumber();
                }
                if (inviteType == InviteTypes.PassInvite)
                {
                    res.InvitePassTitle = getResource.getMessage("InvitePassPassTitle");
                    res.InvitePassenger = getResource.getMessage("InvitePassPass");
                    /*res.InviteDriverTitle = getResource.getMessage("InvitePassDriverTitle");
                    res.InviteDriver = getResource.getMessage("InvitePassDriver");*/
                    res.InviteDriverTitle = "";
                    res.InviteDriver = "";
                }
                else
                {
                    res.InvitePassTitle = getResource.getMessage("InviteDriverPassTitle");
                    res.InvitePassenger = getResource.getMessage("InviteDriverPass");
                    res.InviteDriverTitle = getResource.getMessage("InviteDriverDriverTitle");
                    res.InviteDriver = getResource.getMessage("InviteDriverDriver");
                }
                return res;
            }
        }


        public void ValidatingTry(int id)
        {
            using (var dataModel = new MibarimEntities())
            {
                var uis = dataModel.UserInfoes.FirstOrDefault(x => x.UserId == id && !x.UserInfoIsDeleted);
                if (uis != null)
                {
                    if (uis.ValidatingTry == null)
                    {
                        uis.ValidatingTry = 1;
                    }
                    else
                    {
                        uis.ValidatingTry++;
                    }
                }
                else
                {
                    var ui = new UserInfo()
                    {
                        UserInfoCreateTime = DateTime.Now,
                        UserId = id,
                        UserInfoIsDeleted = false,
                        ValidatingTry = 1
                    };
                    dataModel.UserInfoes.Add(ui);
                }
                dataModel.SaveChanges();
            }
        }

        public void SendNotif()
        {
            var userIds = new List<int> {27, 1, 12325}; //, 181
            using (var dataModel = new MibarimEntities())
            {
                var nu = dataModel.GetNotificationUsers();
                userIds.AddRange(nu.Select(s => Convert.ToInt32(s)));
                var notif = new NotifModel();
                notif.Title = "اولین سفر به نمایشگاه الکامپ را مهمان ما باشید";
                notif.Body = "سفر رایگان به الکامپ را به دوستان خود اطلاع دهید";
                notif.Url =
                    "http://mibarim.com/elecomp96?utm_source=push_elecomp96&utm_campaign:elecomp96&utm_medium:pushnotif";
                notif.RequestCode = 8;
                notif.NotificationId = 8;
                notif.Tab = 1;
                notif.Action = "test";
                _notifManager.SendGroupNotif(notif, userIds);
                /*var smsService = new SmsService();
                string smsBody = "رمز عبور جدید شما : " + "3690" + " می باشد "+ "\r\n Mibarim.com";
                smsService.SendSmsMessages("9123803348",smsBody);*/
            }
        }

        public ScoreModel GetUserScoresByContact(int userId, long contactId)
        {
            var s = new ScoreModel();
            using (var dataModel = new MibarimEntities())
            {
                var contact = dataModel.Contacts.FirstOrDefault(x => x.ContactId == contactId);
                if (contact != null && contact.ContactPassengerUserId == userId)
                {
                    var average =
                        dataModel.vwContactScores.Where(
                            x =>
                                (x.ContactDriverUserId == contact.ContactDriverUserId ||
                                 x.ContactPassengerUserId == contact.ContactDriverUserId) &&
                                x.CsUserId != x.ContactDriverUserId).Average(x => x.CsScore);
                    if (average !=
                        null)
                        s.ContactScore = (int) average;
                    var aboutMe = dataModel.AboutUsers.FirstOrDefault(x => x.UserId == contact.ContactDriverUserId);
                    if (aboutMe != null) s.AboutMe = aboutMe.AboutDesc;
                    s.Score =
                        dataModel.vwTripRoutes.Where(
                            x =>
                                x.RouteRequestUserId == contact.ContactDriverUserId &&
                                x.TrState == (int) TripRouteState.TripRouteFinished).ToList().Count;
                }
                else if (contact != null && contact.ContactDriverUserId == userId)
                {
                    var average =
                        dataModel.vwContactScores.Where(
                            x =>
                                (x.ContactDriverUserId == contact.ContactPassengerUserId ||
                                 x.ContactPassengerUserId == contact.ContactPassengerUserId) &&
                                x.CsUserId != x.ContactPassengerUserId).Average(x => x.CsScore);
                    if (average !=
                        null)
                        s.ContactScore = (int) average;
                    var aboutMe = dataModel.AboutUsers.FirstOrDefault(x => x.UserId == contact.ContactDriverUserId);
                    if (aboutMe != null) s.AboutMe = aboutMe.AboutDesc;
                    s.Score =
                        dataModel.vwTripRoutes.Where(
                            x =>
                                x.RouteRequestUserId == contact.ContactPassengerUserId &&
                                x.TrState == (int) TripRouteState.TripRouteFinished).ToList().Count;
                }
            }
            s.MoneySave = 0;
            return s;
        }

        public string InsertUserScore(int userId, ContactScoreModel model)
        {
            using (var dataModel = new MibarimEntities())
            {
                var contact = dataModel.Contacts.FirstOrDefault(x => x.ContactId == model.ContactId);
                var driverTripIds =
                    dataModel.vwTripRoutes.Where(
                        x =>
                            x.RouteRequestUserId == contact.ContactDriverUserId &&
                            x.TrState == (int) TripRouteState.TripRouteFinished).ToList();
                var passTripIds =
                    dataModel.vwTripRoutes.Where(
                        x =>
                            x.RouteRequestUserId == contact.ContactPassengerUserId &&
                            x.TrState == (int) TripRouteState.TripRouteFinished).ToList();
                var sameTrip = driverTripIds.Any(x => passTripIds.Select(y => y.TrTripId).Contains(x.TrTripId));
                if (sameTrip)
                {
                    var hasScore =
                        dataModel.ContactScores.FirstOrDefault(
                            x => x.CsContactId == model.ContactId && x.CsUserId == userId);
                    int res = 0;
                    if (hasScore != null)
                    {
                        int.TryParse(model.ContactScore.ToString(), out res);
                        hasScore.CsScore = (short?) res;
                    }
                    else
                    {
                        var score = new ContactScore();
                        score.CsUserId = userId;
                        score.CsCreateTime = DateTime.Now;
                        score.CsContactId = model.ContactId;
                        int.TryParse(model.ContactScore.ToString(), out res);
                        score.CsScore = (short?) res;
                        dataModel.ContactScores.Add(score);
                    }
                    dataModel.SaveChanges();
                    return getResource.getMessage("ScoreSubmited");
                }
                else
                {
                    _responseProvider.SetBusinessMessage(new MessageResponse()
                    {
                        Type = ResponseTypes.Error,
                        Message = getResource.getMessage("NoTrip")
                    });
                    return string.Empty;
                }
            }
        }

        public UserInitialInfo GetUserInitialInfo(int userId)
        {
            var res = new UserInitialInfo();
            using (var dataModel = new MibarimEntities())
            {
                var userd = dataModel.vwUserInfoes.FirstOrDefault(x => x.UserId == userId);
                if (userd == null || string.IsNullOrEmpty(userd.Name))
                {
                    res.IsUserRegistered = false;
                }
                else
                {
                    res.IsUserRegistered = true;
                    var passengerContacts = dataModel.Contacts.Where(x => x.ContactPassengerUserId == userId);
                    foreach (var passengerContact in passengerContacts)
                    {
                        res.ChatCount +=
                            dataModel.Chats.Count(
                                x => x.ChatUserId == passengerContact.ContactDriverUserId && !x.IsChatSeen);
                    }
                    var driverContacts = dataModel.Contacts.Where(x => x.ContactDriverUserId == userId);
                    foreach (var driverContact in driverContacts)
                    {
                        res.ChatCount +=
                            dataModel.Chats.Count(
                                x => x.ChatUserId == driverContact.ContactPassengerUserId && !x.IsChatSeen);
                    }
                }
            }
            return res;
        }

        public void RegisterUserInfo(ApplicationUser user, PersoanlInfoModel model, InviteTypes inviteType)
        {
            CreateSupportContact(user);
            //HandleInvite(user, model.Code);
            //HandleInvitation(model.Code,user.Id);
            if (model.Code != null)
            {
                DoDiscount(inviteType, model.Code, user.Id);
            }
        }

        public UserInfoModel UpdateFanapUserInfo(FanapModel model)
        {
            var userInfomodel = new UserInfoModel();
            var fanapService = new FanapService();
            using (var dataModel = new MibarimEntities())
            {
                var userfanap = dataModel.vwUserInfoes.FirstOrDefault(x => x.UserUId == model.StateGUid);
                if (userfanap != null)
                {
                    var fanap = dataModel.Fanaps.FirstOrDefault(x => x.userId == userfanap.UserId);
                    if (fanap == null)
                    {
                        fanap = new Fanap();
                    }
                    fanap.userId = userfanap.UserId;
                    fanap.authorization_code = model.Code;
                    var cont = fanapService.GetAuthenticationToken(model.Code);
                    fanap.access_token = cont.access_token;
                    fanap.refresh_token = cont.refresh_token;
                    dataModel.Fanaps.Add(fanap);
                    dataModel.SaveChanges();
                    if (cont.access_token != null)
                    {
                        var userInfo = fanapService.RegisterWithSso(cont.access_token, "mibarim_" + fanap.FanapId);
                        fanap.nickName = userInfo.nickName;
                        fanap.birthDate = userInfo.birthDate;
                        fanap.fuserId = userInfo.userId;
                        userInfomodel.Name = userInfo.firstName;
                        userInfomodel.Family = userInfo.lastName;
                        userInfomodel.UserId = fanap.userId;
                        fanap.score = userInfo.score.ToString();
                        dataModel.SaveChanges();
                    }
                    return userInfomodel;
                }
            }

            return userInfomodel;
        }

        public bool RegisterFanap(string nickname)
        {
            var fanapService = new FanapService();
            var fanaptokenService = fanapService.RegisterUserToFanapPlatform(nickname);
            return true;
        }

        public UserInitialInfo GetDriverInitialInfo(int userId)
        {
            var res = new UserInitialInfo();
            using (var dataModel = new MibarimEntities())
            {
                var userd = dataModel.vwUserInfoes.FirstOrDefault(x => x.UserId == userId);
                if (userd == null || string.IsNullOrEmpty(userd.Name))
                {
                    res.IsUserRegistered = false;
                }
                else
                {
                    res.IsUserRegistered = true;
                    var istrip =
                        dataModel.vwDriverTrips.Where(x => x.UserId == userId && x.TState == (int) TripState.InRiding);
                }
            }
            return res;
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
                    userInfo = fanapService.RegisterWithSso(cont.access_token, "mibarim_");
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

        public bool DoDiscount(InviteTypes intype, string discountCode, int userid)
        {
            using (var dataModel = new MibarimEntities())
            {
                var dc = dataModel.Discounts.FirstOrDefault(x => x.DiscountCode == discountCode);
                if (dc != null)
                {
                    var dcu =
                        dataModel.DiscountUsers.FirstOrDefault(
                            x => x.DiscountId == dc.DiscountId && x.UserId == userid);
                    if (dcu != null && dc.DiscountType != (int) DiscountTypes.AlwaysFreeSeat)
                    {
                        _responseProvider.SetBusinessMessage(new MessageResponse()
                        {
                            Type = ResponseTypes.Error,
                            Message = getResource.getMessage("CodeUsed")
                        });
                        return false;
                    }
                    var discountUser = new DiscountUser();
                    switch (dc.DiscountType)
                    {
                        case (int) DiscountTypes.EndlessFirstFreeTrip:
                            discountUser.UserId = userid;
                            discountUser.DiscountId = dc.DiscountId;
                            discountUser.DuCreateTime = DateTime.Now;
                            discountUser.DuEndTime = null;
                            discountUser.DuState = (int) DiscountStates.Submitted;
                            dataModel.DiscountUsers.Add(discountUser);
                            dataModel.SaveChanges();
                            break;
                        case (int) DiscountTypes.FirstFreeTrip:
                            discountUser.UserId = userid;
                            discountUser.DiscountId = dc.DiscountId;
                            discountUser.DuCreateTime = DateTime.Now;
                            discountUser.DuEndTime = DateTime.Now.AddMonths(1);
                            discountUser.DuState = (int) DiscountStates.Submitted;
                            dataModel.DiscountUsers.Add(discountUser);
                            dataModel.SaveChanges();
                            break;
                        case (int) DiscountTypes.FreeSeat:
                            discountUser.UserId = userid;
                            discountUser.DiscountId = dc.DiscountId;
                            discountUser.DuCreateTime = DateTime.Now;
                            discountUser.DuEndTime = DateTime.Now.AddMonths(1);
                            discountUser.DuState = (int) DiscountStates.Submitted;
                            dataModel.DiscountUsers.Add(discountUser);
                            dataModel.SaveChanges();
                            break;
                        case (int) DiscountTypes.EndlessFreeSeat:
                            discountUser.UserId = userid;
                            discountUser.DiscountId = dc.DiscountId;
                            discountUser.DuCreateTime = DateTime.Now;
                            discountUser.DuEndTime = null;
                            discountUser.DuState = (int) DiscountStates.Submitted;
                            dataModel.DiscountUsers.Add(discountUser);
                            dataModel.SaveChanges();
                            break;
                        case (int) DiscountTypes.AlwaysFreeSeat:
                            discountUser.UserId = userid;
                            discountUser.DiscountId = dc.DiscountId;
                            discountUser.DuCreateTime = DateTime.Now;
                            discountUser.DuEndTime = null;
                            discountUser.DuState = (int) DiscountStates.Submitted;
                            dataModel.DiscountUsers.Add(discountUser);
                            dataModel.SaveChanges();
                            break;
                    }
                    return true;
                }
                else
                {
                    var ui = dataModel.Invites.FirstOrDefault(x => x.InviteCode == discountCode);
                    if (ui != null)
                    {
                        var discount =
                            dataModel.Discounts.FirstOrDefault(
                                x => x.DiscountCode == "InviteFirstFreeTrip");

                        if (ui.InviteType == (int) InviteTypes.PassInvite)
                        {
                            if (intype == InviteTypes.PassInvite)
                            {
                                var invite =
                                    dataModel.Invites.FirstOrDefault(
                                        x => x.UserId == userid && x.InviteType == (int) InviteTypes.PassInvite);
                                if (invite == null)
                                {
                                    var thisInvite = new Invite();
                                    thisInvite.CreateTime = DateTime.Now;
                                    thisInvite.UserId = userid;
                                    thisInvite.InviteType = (int) InviteTypes.PassInvite;
                                    thisInvite.InviterUserId = ui.UserId;
                                    thisInvite.InviterId = ui.InviteId;
                                    thisInvite.InviteCode = InviteCodeGenerator();
                                    dataModel.Invites.Add(thisInvite);
                                    dataModel.SaveChanges();
                                    var discountUser = new DiscountUser();
                                    discountUser.UserId = userid;
                                    discountUser.DiscountId = discount.DiscountId;
                                    discountUser.DuCreateTime = DateTime.Now;
                                    discountUser.DuEndTime = DateTime.Now.AddMonths(1);
                                    discountUser.DuState = (int) DiscountStates.Submitted;
                                    dataModel.DiscountUsers.Add(discountUser);
                                    dataModel.SaveChanges();
                                }
                                else
                                {
                                    if (invite.InviterUserId != null)
                                    {
                                        _responseProvider.SetBusinessMessage(new MessageResponse()
                                        {
                                            Type = ResponseTypes.Error,
                                            Message = getResource.getMessage("CodeUsed")
                                        });
                                        return false;
                                    }
                                    else
                                    {
                                        invite.InviterUserId = ui.UserId;
                                        invite.InviterId = ui.InviteId;
                                        //invite.CreateTime = DateTime.Now;
                                        dataModel.SaveChanges();
                                        var discountUser = new DiscountUser();
                                        discountUser.UserId = userid;
                                        discountUser.DiscountId = discount.DiscountId;
                                        discountUser.DuCreateTime = DateTime.Now;
                                        discountUser.DuEndTime = DateTime.Now.AddMonths(1);
                                        discountUser.DuState = (int) DiscountStates.Submitted;
                                        dataModel.DiscountUsers.Add(discountUser);
                                        dataModel.SaveChanges();
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (intype == InviteTypes.PassInvite)
                            {
                                var invite =
                                    dataModel.Invites.FirstOrDefault(
                                        x => x.UserId == userid && x.InviteType == (int) InviteTypes.PassInvite);
                                if (invite == null)
                                {
                                    var thisInvite = new Invite();
                                    thisInvite.CreateTime = DateTime.Now;
                                    thisInvite.UserId = userid;
                                    thisInvite.InviteType = (int) InviteTypes.DriverInvite;
                                    thisInvite.InviterUserId = ui.UserId;
                                    thisInvite.InviterId = ui.InviteId;
                                    thisInvite.InviteCode = InviteCodeGenerator();
                                    dataModel.Invites.Add(thisInvite);
                                    dataModel.SaveChanges();
                                    var discountUser = new DiscountUser();
                                    discountUser.UserId = userid;
                                    discountUser.DiscountId = discount.DiscountId;
                                    discountUser.DuCreateTime = DateTime.Now;
                                    discountUser.DuEndTime = DateTime.Now.AddMonths(1);
                                    discountUser.DuState = (int) DiscountStates.Submitted;
                                    dataModel.DiscountUsers.Add(discountUser);
                                    dataModel.SaveChanges();
                                }
                                else
                                {
                                    if (invite.InviterUserId != null)
                                    {
                                        _responseProvider.SetBusinessMessage(new MessageResponse()
                                        {
                                            Type = ResponseTypes.Error,
                                            Message = getResource.getMessage("CodeUsed")
                                        });
                                        return false;
                                    }
                                    else
                                    {
                                        invite.InviterUserId = ui.UserId;
                                        invite.InviterId = ui.InviteId;
                                        //invite.CreateTime = DateTime.Now;
                                        dataModel.SaveChanges();
                                        var discountUser = new DiscountUser();
                                        discountUser.UserId = userid;
                                        discountUser.DiscountId = discount.DiscountId;
                                        discountUser.DuCreateTime = DateTime.Now;
                                        discountUser.DuEndTime = DateTime.Now.AddMonths(1);
                                        discountUser.DuState = (int) DiscountStates.Submitted;
                                        dataModel.DiscountUsers.Add(discountUser);
                                        dataModel.SaveChanges();
                                    }
                                }
                            }
                            else
                            {
                                var invite =
                                    dataModel.Invites.FirstOrDefault(
                                        x => x.UserId == userid && x.InviteType == (int) InviteTypes.DriverInvite);
                                if (invite == null)
                                {
                                    var thisInvite = new Invite();
                                    thisInvite.CreateTime = DateTime.Now;
                                    thisInvite.UserId = userid;
                                    thisInvite.InviteType = (int) InviteTypes.DriverInvite;
                                    thisInvite.InviterUserId = ui.UserId;
                                    thisInvite.InviterId = ui.InviteId;
                                    thisInvite.InviteCode = InviteCodeGenerator();
                                    dataModel.Invites.Add(thisInvite);
                                    dataModel.SaveChanges();
                                }
                                else
                                {
                                    if (invite.InviterUserId != null)
                                    {
                                        _responseProvider.SetBusinessMessage(new MessageResponse()
                                        {
                                            Type = ResponseTypes.Error,
                                            Message = getResource.getMessage("CodeUsed")
                                        });
                                        return false;
                                    }
                                    else
                                    {
                                        invite.InviterUserId = ui.UserId;
                                        invite.InviterId = ui.InviteId;
                                        //invite.CreateTime = DateTime.Now;
                                        dataModel.SaveChanges();
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        _responseProvider.SetBusinessMessage(new MessageResponse()
                        {
                            Type = ResponseTypes.Error,
                            Message = getResource.getMessage("CodeNotExist")
                        });
                        return false;
                    }
                    return true;
                }
            }
        }

        public List<PassRouteModel> GetPassengers()
        {
            var res=new List<PassRouteModel>();
            using (var dataModel = new MibarimEntities())
            {
                var time = DateTime.Now.AddHours(-1);
                var models = dataModel.vwBookedTrips.Where(x => x.TStartTime>time).OrderByDescending(x=>x.BrCreateTime);
                foreach (var vwBookedTrip in models)
                {
                    var rm=new PassRouteModel();
                    rm.SrcAddress = vwBookedTrip.srcName;
                    rm.DstAddress = vwBookedTrip.dstName;
                    rm.CarString =vwBookedTrip.DriverName +" "+ vwBookedTrip.DriverFamily;
                    rm.CarPlate = vwBookedTrip.DriverMobile;
                    rm.Name = vwBookedTrip.Passname;
                    rm.Family = vwBookedTrip.PassFamily;
                    rm.MobileNo = vwBookedTrip.PassMobile;
                    rm.EmptySeats= vwBookedTrip.TEmptySeat;
                    rm.TimingString= vwBookedTrip.TStartTime.ToString("HH:mm");
                    res.Add(rm);
                }
                return res;
            }
        }

        private string InviteCodeGenerator()
        {
            string[] str = new[] {"mb", "mi", "mr", "ba", "mm", "ma", "mb", "mib"};
            using (var dataModel = new MibarimEntities())
            {
                var invitecode = dataModel.Invites.Count();
                var indict = invitecode%7;
                var number = invitecode/7;

                return str[indict] + number;
            }
            /*var random = new Random();
            string[] str = new[] {"mb", "mi", "mr", "ba", "mm"};
            var rndMember = str[random.Next(str.Length)];
            return rndMember + inviteId;*/
        }

        /*public ImageResponse GetImageByUserId(ImageRequest model)
        {
            var img = new ImageResponse();
            using (var dataModel = new MibarimEntities())
            {
                ImageType imageType;
                Enum.TryParse(model.ImageType, out imageType);
                var res = dataModel.Images.OrderByDescending(x => x.ImageCreateTime).FirstOrDefault(x => x.ImageUserId == model.UserId && x.ImageType==(int)imageType);
                img.ImageId = res.ImageId.ToString();
                img.ImageFile = res.ImageFile;
                img.ImageType = (ImageType)res.ImageType;
            }
            return img;
        }*/

        private
            string BriefChat(string supportChatTxt)
        {
            throw new NotImplementedException();
        }
    }
}