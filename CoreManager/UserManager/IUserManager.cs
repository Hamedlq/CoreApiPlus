using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using CoreManager.Models;

namespace CoreManager.UserManager
{
    public interface IUserManager
    {
        void UpdateUserInfo(ApplicationUser user, RegisterModel model);
        ApplicationUser PopulateUpdateModel(RegisterModel model, ApplicationUser user);
        void UpdatePersoanlInfo(PersoanlInfoModel model, int userId);
        void UpdateUserInfo(UserInfoModel model, int userId);

        PersoanlInfoModel GetPersonalInfo(int userId);
        PersoanlInfoModel GetPersonalInfoByRouteId(int routeId);
        void InsertLicenseInfo(LicenseInfoModel model, int userId);
        void InsertLicensePic(byte[] licensePicModel, int userId);
        void InsertNationalCardPic(byte[] nationalCardPicModel, int userId);
        LicenseInfoModel GetLicenseInfo(int userId);
        void InsertCarInfo(CarInfoModel model, int userId);
        void InsertCarPic(byte[] carPicModel, int userId);
        void InsertCarBackPic(byte[] carPicModel, int userId);
        CarInfoModel GetCarInfo(int userId);
        void UpdatePersoanlPic(byte[] userPicModel, int userId);

        void SubmitContactUs(ContactUsModel model);
        PersoanlInfoModel GetRouteUserImage(int userId, int routeRequestId);
        PersoanlInfoModel GetCommentUserImage(int userId, int commentId);
        void InsertBankInfo(BankInfoModel model, int userId);
        void InsertBankCardPic(byte[] bankCardPic, int userId);
        BankInfoModel GetBankInfo(int userId);
        bool ConfirmMobileNo(string mobile);
        bool SendConfirmMobileSms(ApplicationUser model, MobileValidation mobileModel, string rand);
        List<PersoanlInfoModel> GetAllUsers();
        PersoanlInfoModel GetUserPersonalInfoByMobile(string  mobile);
        LicenseInfoModel GetUserLicenseInfo(string mobile);
        CarInfoModel GetUserCarInfo(string mobile);
        NotificationModel GetNotifications(string mobile);
        List<ContactModel> GetUserContacts(int userId);
        ScoreModel GetUserScores(int userId);
        void InsertGoogleToken(int userId, Gtoken model);
        List<PersoanlInfoModel> GetUserByInfo(UserSearchModel model);
        ImageResponse GetImageById(ImageRequest model);
        //        ImageResponse GetImageByUserId(ImageRequest model);
        long GetUserTrip(int userId);
        void SaveUserTripLocation(int userId, UserLocation userLocation);
        //string AcceptRideShare(int userId, long contactId);
        //string InvokeTrips();

        UserInfoModel GetUserInfo(int userId);
        bool DiscountCodeExist(DiscountModel model);
        void InsertDiscountCode(DiscountModel model, int userId);
        void InsertWithdrawRequest(WithdrawRequestModel model, int userId);
        List<DiscountModel> GetUserDiscount(int userId);
        bool DiscountCodeUsed(DiscountModel model, int userId);
        void InsertAboutUser(AboutUserModel model, int userId);
        AboutUserModel GetUserAboutMe(int userId);
        List<WithdrawRequestModel> GetWithdraw(int userId);
        bool WithdrawlValid(WithdrawRequestModel model, int userId);

        InviteModel GetUserInvite(int userId);
        void ValidatingTry(int id);
        void SendNotif();
        ScoreModel GetUserScoresByContact(int userId, long contactId);
        string InsertUserScore(int userId, ContactScoreModel model);
        UserInitialInfo GetUserInitialInfo(int userId);
        void RegisterUserInfo(ApplicationUser user, PersoanlInfoModel model);
        UserInfoModel UpdateFanapUserInfo(FanapModel model);
        bool RegisterFanap(string nickname);

        UserInitialInfo GetDriverInitialInfo(int userId);
    }
}
