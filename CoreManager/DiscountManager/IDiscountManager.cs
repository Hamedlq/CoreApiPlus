using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreDA;
using CoreManager.Models;

namespace CoreManager.DiscountManager
{
    public interface IDiscountManager
    {
        void InviteFriends(DiscountModel model, Invite ui, int userid);
        void InvitePassenger(DiscountModel model, Invite ui, int userid);
        void FreeCredit(DiscountModel model, Discount dc, int userid);
        void FreeSeat(DiscountModel model, Discount dc, int userid);

    }
}
