using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreDA;
using CoreExternalService;
using CoreExternalService.SMSirSentAndReceivedMessages;
using CoreManager.LogProvider;
using CoreManager.Models;
using CoreManager.NotificationManager;
using CoreManager.Resources;

namespace CoreManager.DriverManager
{
    public class DriverManager : IDriverManager
    {
        private static string Tag = "DriverManager";
        private readonly INotificationManager _notifManager;
        private ILogProvider _logmanager;

        #region Constructor

        public DriverManager()
        {
        }

        public DriverManager(INotificationManager notifManager, ILogProvider logManager)
        {
            _logmanager = logManager;
            _notifManager = notifManager;
        }

        #endregion

        public GasScore GetGasScores(Guid userUId)
        {
            using (var dataModel = new MibarimEntities())
            {
                var user = dataModel.vwUserInfoes.FirstOrDefault(x => x.UserUId == userUId);
                var score = new GasScore();
                var begining = DateTime.Parse("2017-08-30");
                //var trips = dataModel.vwDriverTrips.Where(x => x.UserId == user.UserId && x.TCreateTime > begining && x.TState != 5).ToList();
                var trips = dataModel.GetDriverRate(begining, user.UserId).ToList();

                if (trips.Count > 0)
                {
                    score.RouteCount = trips.Count;
                    foreach (var trip in trips)
                    {
                        long minDistance=trip.Value;
                            score.DistanceRouted += (long)(minDistance * 0.001);
                            score.Payment += (long)(minDistance * 0.1);
                    }
                }
                return score;
            }
        }

        public List<GasRank> GasRanks(Guid uid)
        {
            var res=new List<GasRank>();
            using (var dataModel = new MibarimEntities())
            {
                var begining = DateTime.Parse("2017-08-30");
                var ranks = dataModel.GetDriverRanks(begining).ToList();
                foreach (var gasRes in ranks)
                {
                    var gasrank=new GasRank();
                    gasrank.Name = gasRes.Name;
                    gasrank.Family = gasRes.Family;
                    gasrank.Rank = (int)gasRes.trtime;
                    var payed= (int)(gasRes.distance * 0.1) / 100000;
                    var paying = (int)(gasRes.distance * 0.1) - (((int)(gasRes.distance * 0.1) / 100000) * 100000);
                    gasrank.Payment = paying.ToString();
                    gasrank.Payed = payed.ToString();
                    res.Add(gasrank);
                }
            }
            return res;
        }

        private double RemoveDecimalToman(double? priceValue)
        {
            if (priceValue != null)
            {
                return Math.Round((priceValue.Value) / 1000, 0) * 100;
            }
            return 0;
        }
    }
}
