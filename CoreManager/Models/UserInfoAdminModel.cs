using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreManager.Resources;

namespace CoreManager.Models
{
    public class UserInfoAdminModel: UserInfoModel
    {
        public UserInfoAdminModel()
        {
            NationalCardImage = new ImageDescription();
            LicenseImage = new ImageDescription();
            CarCardImage = new ImageDescription();
            CarCardBckImage = new ImageDescription();
            CarImage = new ImageDescription();
        }

        public string Mobile { set; get; }
        public Guid? NationalCardImageId { set; get; }
        public Guid? LicenseImageId { set; get; }
        public Guid? CarCardImageId { set; get; }
        public Guid? CarCardBckImageId { set; get; }
        public Guid? CarImageId { set; get; }
        public Guid? BankImageId { set; get; }
        public Guid? CompanyImageId { set; get; }
        public Guid? UserUId { set; get; }
    }
}