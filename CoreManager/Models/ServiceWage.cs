using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreManager.Models
{
    public static class ServiceWage
    {
        private static double Payfee;
        public static double Fee
        {
            set { Payfee = value; }
        }
        public static double Wage
        {
            get { return (Math.Floor((Payfee * 0.3) / 1000) * 1000); }
        }
        public static decimal WageDecimal
        {
            get
            {
                return (decimal)(Math.Floor((Payfee * 0.3) / 1000) * 1000);
            }
        }

    }
}
