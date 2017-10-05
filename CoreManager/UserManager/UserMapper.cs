using System;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Globalization;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using CoreDA;
using CoreManager.Models;
using Image = CoreDA.Image;

namespace CoreManager.UserManager
{
    public static class UserMapper
    {
        public static PersoanlInfoModel CastPersonalInfoToModel(vwUserInfo ui)
        {
            var pi = new PersoanlInfoModel();
            pi.Mobile = ui.UserName;
            pi.Name = ui.Name;
            pi.Family = ui.Family;
            pi.Gender = (Gender)ui.Gender;
            pi.NationalCode = ui.NationalCode;
            pi.Email = ui.Email;
            pi.UserPic = ui.UserPic;
            pi.UserImageId = ui.UserImageId;
            pi.UserUId = ui.UserUId;
            return pi;
        }
        public static UserInfoModel CastPersonalInfoToUserInfoModel(vwUserInfo ui)
        {
            var pi = new UserInfoModel();
            pi.Name = ui.Name;
            pi.Family = ui.Family;
            pi.Gender = (Gender)ui.Gender;
            pi.NationalCode = ui.NationalCode;
            pi.Email = ui.Email;
            pi.UserImageId = ui.UserImageId;
            //pi.NationalCardImageId = ui.NationalCardImageId;
            pi.UserImageId = ui.UserImageId;
            return pi;
        }
        public static UserInfoAdminModel CastPersonalInfoToUserInfoAdminModel(vwUserInfo ui)
        {
            var pi = new UserInfoAdminModel();
            pi.Name = ui.Name;
            pi.Mobile = ui.UserName;
            pi.Family = ui.Family;
            pi.Gender = (Gender)ui.Gender;
            pi.NationalCode = ui.NationalCode;
            pi.Email = ui.Email;
            pi.UserImageId = ui.UserImageId;
            //pi.NationalCardImageId = ui.NationalCardImageId;
            pi.UserImageId = ui.UserImageId;
            return pi;
        }
        public static PersoanlInfoModel CastPersonalInfoToModelWithoutPic(vwUserInfo ui)
        {
            var pi = new PersoanlInfoModel();
            pi.Mobile = ui.UserName;
            pi.Name = ui.Name;
            pi.Family = ui.Family;
            pi.Gender = (Gender)ui.Gender;
            pi.NationalCode = ui.NationalCode;
            pi.Email = ui.Email;
            pi.UserUId = ui.UserUId;
            return pi;
        }

        public static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        public static UserInfoModel FillCarInfoInUserInfoModel(UserInfoModel ui, CarInfo cis)
        {
            ui.CarColor = cis.CarColor;
            ui.CarType = cis.CarType;
            ui.CarPlateNo = cis.CarPlateNo;
            //ui.CarCardImageId = cis.CarFrontImageId;
            //ui.CarCardBckImageId = cis.CarBackImageId;
            return ui;
        }

        public static UserInfoAdminModel FillCarInfoInUserInfoModel(UserInfoAdminModel ui, CarInfo cis)
        {
            ui.CarColor = cis.CarColor;
            ui.CarType = cis.CarType;
            ui.CarPlateNo = cis.CarPlateNo;
            //ui.CarCardImageId = cis.CarFrontImageId;
            //ui.CarCardBckImageId = cis.CarBackImageId;
            return ui;
        }

        public static UserInfoModel FillBankInfoInUserInfoModel(UserInfoModel ui, BankInfo bankdb)
        {
            ui.BankName = bankdb.BankName;
            ui.BankAccountNo = bankdb.BankAccountNo;
            ui.BankShaba = bankdb.BankShabaNo;
            //ui.BankImageId = bankdb.BankCardImageId;
            return ui;
        }

        public static byte[] ResizeImage(byte[] myBytes, int newWidth, int newHeight)
        {
            System.IO.MemoryStream myMemStream = new System.IO.MemoryStream(myBytes);
            System.Drawing.Image fullsizeImage = System.Drawing.Image.FromStream(myMemStream);
            System.Drawing.Image newImage = fullsizeImage.GetThumbnailImage(newWidth, newHeight, null, IntPtr.Zero);
            System.IO.MemoryStream myResult = new System.IO.MemoryStream();
            newImage.Save(myResult, System.Drawing.Imaging.ImageFormat.Jpeg);  //Or whatever format you want.
            return myResult.ToArray();
        }

        public static UserInfoModel FillCompanyInfoInUserInfoModel(UserInfoModel ui, vwCompany comp)
        {
            ui.CompanyName = comp.CompanyName;
            ui.Code = comp.Code;
            //ui.CompanyImageId = comp.CompanyImageId;
            return ui;
        }

        public static string PersianNumber(string input)
        {
            if (input.Trim() == "") return "";
            //۰ ۱ ۲ ۳ ۴ ۵ ۶ ۷ ۸ ۹
            input = input.Replace("0", "۰");
            input = input.Replace("1", "۱");
            input = input.Replace("2", "۲");
            input = input.Replace("3", "۳");
            input = input.Replace("4", "۴");
            input = input.Replace("5", "۵");
            input = input.Replace("6", "۶");
            input = input.Replace("7", "۷");
            input = input.Replace("8", "۸");
            input = input.Replace("9", "۹");
            return input;
        }

        public static UserInfoModel SetImageValues(UserInfoModel ui, List<vwImageReject> lis)
        {
            var nationcard = lis.FirstOrDefault(x => x.ImageType == (int)ImageType.UserNationalCard);
            ui.NationalCardImage = new ImageDescription();
            if (nationcard == null)
            {
                ui.NationalCardImage.State = DocState.NotSent;
            }
            else
            {
                if (nationcard.IsVerified != null)
                {
                    ui.NationalCardImage.State = nationcard.IsVerified.Value ? DocState.Accepted : DocState.Rejected;
                    ui.NationalCardImage.RejectionDescription = nationcard.RejectDescription;
                }
                else
                {
                    ui.NationalCardImage.State = DocState.UnderChecking;
                }
            }
            var license = lis.FirstOrDefault(x => x.ImageType == (int)ImageType.LicensePic);
            ui.LicenseImage = new ImageDescription();
            if (license == null)
            {
                ui.LicenseImage.State = DocState.NotSent;
            }
            else
            {
                if (license.IsVerified != null)
                {
                    ui.LicenseImage.State = license.IsVerified.Value ? DocState.Accepted : DocState.Rejected;
                    ui.LicenseImage.RejectionDescription = license.RejectDescription;
                }
                else
                {
                    ui.LicenseImage.State = DocState.UnderChecking;
                }
            }
            var carCard = lis.FirstOrDefault(x => x.ImageType == (int)ImageType.CarCardPic);
            ui.CarCardImage = new ImageDescription();
            if (carCard == null)
            {
                ui.CarCardImage.State = DocState.NotSent;
            }
            else
            {
                if (carCard.IsVerified != null)
                {
                    ui.CarCardImage.State = carCard.IsVerified.Value ? DocState.Accepted : DocState.Rejected;
                    ui.CarCardImage.RejectionDescription = carCard.RejectDescription;
                }
                else
                {
                    ui.CarCardImage.State = DocState.UnderChecking;
                }
            }
            var carCardBck = lis.FirstOrDefault(x => x.ImageType == (int)ImageType.CarCardBckPic);
            ui.CarCardBckImage = new ImageDescription();
            if (carCardBck == null)
            {
                ui.CarCardBckImage.State = DocState.NotSent;
            }
            else
            {
                if (carCardBck.IsVerified != null)
                {
                    ui.CarCardBckImage.State = carCardBck.IsVerified.Value ? DocState.Accepted : DocState.Rejected;
                    ui.CarCardBckImage.RejectionDescription = carCardBck.RejectDescription;
                }
                else
                {
                    ui.CarCardBckImage.State = DocState.UnderChecking;
                }
            }
            var carImg = lis.FirstOrDefault(x => x.ImageType == (int)ImageType.CarPic);
            ui.CarImage = new ImageDescription();
            if (carImg == null)
            {
                ui.CarImage.State = DocState.NotSent;
            }
            else
            {
                if (carImg.IsVerified != null)
                {
                    ui.CarImage.State = carImg.IsVerified.Value ? DocState.Accepted : DocState.Rejected;
                    ui.CarImage.RejectionDescription = carImg.RejectDescription;
                }
                else
                {
                    ui.CarImage.State = DocState.UnderChecking;
                }
            }
            return ui;
        }

        public static UserInfoAdminModel SetImageValues(UserInfoAdminModel ui, List<vwImageReject> lis)
        {
            var nationcard = lis.FirstOrDefault(x => x.ImageType == (int)ImageType.UserNationalCard);
            ui.NationalCardImage = new ImageDescription();
            if (nationcard == null)
            {
                ui.NationalCardImage.State = DocState.NotSent;
            }
            else
            {
                ui.NationalCardImageId = nationcard.ImageId;
                if (nationcard.IsVerified != null)
                {
                    ui.NationalCardImage.State = nationcard.IsVerified.Value ? DocState.Accepted : DocState.Rejected;
                    ui.NationalCardImage.RejectionDescription = nationcard.RejectDescription;
                }
                else
                {
                    ui.NationalCardImage.State = DocState.UnderChecking;
                }
            }
            var license = lis.FirstOrDefault(x => x.ImageType == (int)ImageType.LicensePic);
            ui.LicenseImage = new ImageDescription();
            if (license == null)
            {
                ui.LicenseImage.State = DocState.NotSent;
            }
            else
            {
                ui.LicenseImageId = license.ImageId;
                if (license.IsVerified != null)
                {
                    ui.LicenseImage.State = license.IsVerified.Value ? DocState.Accepted : DocState.Rejected;
                    ui.LicenseImage.RejectionDescription = license.RejectDescription;
                }
                else
                {
                    ui.LicenseImage.State = DocState.UnderChecking;
                }
            }
            var carCard = lis.FirstOrDefault(x => x.ImageType == (int)ImageType.CarCardPic);
            ui.CarCardImage = new ImageDescription();
            if (carCard == null)
            {
                ui.CarCardImage.State = DocState.NotSent;
            }
            else
            {
                ui.CarCardImageId = carCard.ImageId;
                if (carCard.IsVerified != null)
                {
                    ui.CarCardImage.State = carCard.IsVerified.Value ? DocState.Accepted : DocState.Rejected;
                    ui.CarCardImage.RejectionDescription = carCard.RejectDescription;
                }
                else
                {
                    ui.CarCardImage.State = DocState.UnderChecking;
                }
            }
            var carCardBck = lis.FirstOrDefault(x => x.ImageType == (int)ImageType.CarCardBckPic);
            ui.CarCardBckImage = new ImageDescription();
            if (carCardBck == null)
            {
                ui.CarCardBckImage.State = DocState.NotSent;
            }
            else
            {
                ui.CarCardBckImageId = carCardBck.ImageId;
                if (carCardBck.IsVerified != null)
                {
                    ui.CarCardBckImage.State = carCardBck.IsVerified.Value ? DocState.Accepted : DocState.Rejected;
                    ui.CarCardBckImage.RejectionDescription = carCardBck.RejectDescription;
                }
                else
                {
                    ui.CarCardBckImage.State = DocState.UnderChecking;
                }
            }
            var carImg = lis.FirstOrDefault(x => x.ImageType == (int)ImageType.CarPic);
            ui.CarImage = new ImageDescription();
            if (carImg == null)
            {
                ui.CarImage.State = DocState.NotSent;
            }
            else
            {
                ui.CarImageId = carImg.ImageId;
                if (carImg.IsVerified != null)
                {
                    ui.CarImage.State = carImg.IsVerified.Value ? DocState.Accepted : DocState.Rejected;
                    ui.CarImage.RejectionDescription = carImg.RejectDescription;
                }
                else
                {
                    ui.CarImage.State = DocState.UnderChecking;
                }
            }
            return ui;
        }
    }
}
