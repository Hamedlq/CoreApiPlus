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
            pi.NationalCardImageId = ui.NationalCardImageId;
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
            ui.CarCardImageId = cis.CarFrontImageId;
            ui.CarCardBckImageId = cis.CarBackImageId;
            return ui;
        }

        public static UserInfoModel FillBankInfoInUserInfoModel(UserInfoModel ui, BankInfo bankdb)
        {
            ui.BankName = bankdb.BankName;
            ui.BankAccountNo = bankdb.BankAccountNo;
            ui.BankShaba = bankdb.BankShabaNo;
            ui.BankImageId = bankdb.BankCardImageId;
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
            ui.CompanyImageId = comp.CompanyImageId;
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
    }
}
