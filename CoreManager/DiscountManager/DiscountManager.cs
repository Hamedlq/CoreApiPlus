using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreDA;
using CoreExternalService;
using CoreManager.Models;
using CoreManager.Resources;

namespace CoreManager.DiscountManager
{
    public class DiscountManager : IDiscountManager
    {
        public void InviteFriends(DiscountModel model, Invite ui, int userid)
        {
            /*using (var dataModel = new MibarimEntities())
            {
                // اگه قبلا استفاده کرده باشه یا سفر داشته باشه نباید بتونه استفاده کنه

                else
                {
                    if (invite.InviterUserId != null)
                    {
                        //used before messsage
                        return;
                    }
                    else
                    {
                        var book =
                            dataModel.vwBookPays.FirstOrDefault(x => x.PayReqUserId == userid && x.PayReqRefID != null);
                        if (book != null)
                        {
                            //had trip before messsage
                            return;
                        }
                        else
                        {
                            invite.InviterUserId ==
                        }
                    }
                }
                //var dc = dataModel.Discounts.FirstOrDefault(x => x.DiscountCode == model.DiscountCode);
                var discount = new DiscountUser();
                discount.DiscountId = dc.DiscountId;
                discount.UserId = userid;
                discount.DuCreateTime = DateTime.Now;
                discount.DuState = (int) DiscountStates.Submitted;
                dataModel.DiscountUsers.Add(discount);
                dataModel.SaveChanges();
                //send notif sms
                string smsBody = String.Format(getResource.getMessage("TripWizard"), 10, "50,000");
                var smsService = new SmsService();
                var usermobile = dataModel.vwUserInfoes.FirstOrDefault(x => x.UserId == userid);
                var allSms = smsService.SendSmsMessages(usermobile.UserName.Substring(1), smsBody);
            }*/
        }

        public void InvitePassenger(DiscountModel model, Invite ui, int userid)
        {
            throw new NotImplementedException();
        }

        public void FreeCredit(DiscountModel model, Discount dc, int userid)
        {
            throw new NotImplementedException();
        }

        public void FreeSeat(DiscountModel model, Discount dc, int userid)
        {
            throw new NotImplementedException();
        }
    }
}