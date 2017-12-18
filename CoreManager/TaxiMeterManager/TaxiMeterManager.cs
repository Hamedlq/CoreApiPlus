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

namespace CoreManager.TaxiMeterManager
{
    public class TaxiMeterManager : ITaxiMeterManager
    {
        public TmTokensModel GetTokens(TmTokensModel model)
        {
            var res = new TmTokensModel();
            using (var dataModel = new MibarimEntities())
            {
                if (model.SnappTokenStatus == (int) TokenStatus.NotSet ||
                    model.SnappTokenStatus == (int) TokenStatus.Expired)
                {
                    var lastsnapToken =
                        dataModel.AppsTokens.OrderByDescending(x => x.TokenCreateTime)
                            .FirstOrDefault(x => x.TokenApp== (short)TokenApp.SnapApp);
                    if (lastsnapToken != null)
                    {
                        if (lastsnapToken.TokenState != (int) TokenStatus.Expired)
                        {
                            if (model.SnappTokenStatus == (int) TokenStatus.Expired)
                            {
                                if (model.SnappToken == lastsnapToken.Token && lastsnapToken.TokenState != (int)TokenStatus.Expired)
                                {
                                    lastsnapToken.TokenState = (short) model.SnappTokenStatus;
                                    var token = GetNewSnappToken();
                                    var newToken = new AppsToken();
                                    newToken.TokenApp = (short) TokenApp.SnapApp;
                                    newToken.TokenCreateTime = DateTime.Now;
                                    newToken.TokenState = (short) TokenStatus.Valid;
                                    newToken.Token = "Bearer " + token;
                                    dataModel.AppsTokens.Add(newToken);
                                    dataModel.SaveChanges();
                                    res.SnappTokenStatus = (short) TokenStatus.Valid;
                                    res.SnappToken = "Bearer " + token;
                                }
                                else if(lastsnapToken.TokenState != (int)TokenStatus.Expired)
                                {
                                    res.SnappTokenStatus = (short)lastsnapToken.TokenState;
                                    res.SnappToken = lastsnapToken.Token;
                                }
                                else
                                {
                                    res.SnappTokenStatus = (short)TokenStatus.Invalid;
                                }
                            }
                            else
                            {
                                res.SnappTokenStatus = (short) TokenStatus.Valid;
                                res.SnappToken = lastsnapToken.Token;
                            }
                        }
                        else
                        {
                            res.SnappTokenStatus = (short) TokenStatus.Invalid;
                        }
                    }
                    else
                    {
                        var token = GetNewSnappToken();
                        var newToken = new AppsToken();
                        newToken.TokenApp = (short)TokenApp.SnapApp;
                        newToken.TokenCreateTime = DateTime.Now;
                        newToken.TokenState = (short)TokenStatus.Valid;
                        newToken.Token = "Bearer " + token;
                        dataModel.AppsTokens.Add(newToken);
                        dataModel.SaveChanges();
                        res.SnappTokenStatus = (short)TokenStatus.Valid;
                        res.SnappToken = token;
                    }
                }
                if (model.Tap30TokenStatus == (int) TokenStatus.NotSet ||
                    model.Tap30TokenStatus == (int) TokenStatus.Expired)
                {
                    /*var tap30Token =
                        dataModel.AppsTokens.OrderByDescending(x => x.TokenCreateTime)
                            .FirstOrDefault(x => x.TokenApp== (short)TokenApp.Tap30App);
                    if (tap30Token != null)
                    {
                        if (tap30Token.TokenState != (int)TokenStatus.Expired)
                        {
                            if (model.Tap30TokenStatus == (int)TokenStatus.Expired)
                            {
                                if (tap30Token.Token == model.Tap30Token && tap30Token.TokenState != (int)TokenStatus.Expired){ 
                                /*tap30Token.TokenState = (short)model.Tap30TokenStatus;
                                var token = GetNewTap30Token();
                                var newToken = new AppsToken();
                                newToken.TokenApp = (short)TokenApp.Tap30App;
                                newToken.TokenCreateTime = DateTime.Now;
                                newToken.TokenState = (short)TokenStatus.Invalid;
                                newToken.Token = token;
                                dataModel.AppsTokens.Add(newToken);
                                dataModel.SaveChanges();
                                res.Tap30TokenStatus = (short)TokenStatus.Invalid;
                                res.Tap30Token = tap30Token.Token;#1#
                                }
                                else if (tap30Token.TokenState != (int)TokenStatus.Expired)
                                {
                                    res.Tap30TokenStatus = tap30Token.TokenState;
                                    res.Tap30Token = tap30Token.Token;
                                }
                                else
                                {
                                    res.Tap30TokenStatus = (short)TokenStatus.Invalid;
                                }
                            }
                            else
                            {
                                res.Tap30TokenStatus = tap30Token.TokenState;
                                res.Tap30Token = tap30Token.Token;
                            }
                        }
                        else
                        {*/
                    res.Tap30TokenStatus = (short) TokenStatus.Invalid;
                    //}
                //}
                /*else
                {
                    var token = GetNewTap30Token();
                    var newToken = new AppsToken();
                    newToken.TokenApp = (short)TokenApp.Tap30App;
                    newToken.TokenCreateTime = DateTime.Now;
                    newToken.TokenState = (short)TokenStatus.Invalid;
                    newToken.Token = token;
                    dataModel.AppsTokens.Add(newToken);
                    dataModel.SaveChanges();
                    res.Tap30TokenStatus = (short)TokenStatus.Invalid;
                    res.Tap30Token = token;
                }*/
                }
                if (model.CarpinoTokenStatus == (int) TokenStatus.NotSet ||
                    model.CarpinoTokenStatus == (int) TokenStatus.Expired)
                {
                    var carpinoLastToken =
                        dataModel.AppsTokens.OrderByDescending(x => x.TokenCreateTime)
                            .FirstOrDefault(x => x.TokenApp== (short)TokenApp.CarpinoApp);
                    if (carpinoLastToken != null)
                    {
                        if (carpinoLastToken.TokenState != (int)TokenStatus.Expired)
                        {
                            if (model.CarpinoTokenStatus == (int)TokenStatus.Expired)
                            {
                                if (model.CarpinoToken == carpinoLastToken.Token &&
                                    carpinoLastToken.TokenState != (int) TokenStatus.Expired)
                                {
                                    carpinoLastToken.TokenState = (short) model.CarpinoTokenStatus;
                                    var token = GetNewCarpinoToken();
                                    var newToken = new AppsToken();
                                    newToken.TokenApp = (short) TokenApp.CarpinoApp;
                                    newToken.TokenCreateTime = DateTime.Now;
                                    newToken.Token = "Bearer " + token.CarpinoToken;
                                    /*var newrefreshToken = new AppsToken();
                                    newrefreshToken.TokenApp = (short)TokenApp.CarpinoRefreshApp;
                                    newrefreshToken.TokenCreateTime = DateTime.Now;
                                    newrefreshToken.TokenState = (short)TokenStatus.Valid;
                                    newrefreshToken.Token = token.CarpinoToken;
                                    dataModel.AppsTokens.Add(newrefreshToken);*/
                                    if (token.CarpinoToken != null)
                                    {
                                        newToken.TokenState = (short) TokenStatus.Valid;
                                        res.CarpinoTokenStatus = (short) TokenStatus.Valid;
                                        res.CarpinoToken = "Bearer " + token.CarpinoToken;
                                    }
                                    else
                                    {
                                        newToken.TokenState = (short) TokenStatus.Invalid;
                                        res.CarpinoTokenStatus = (short) TokenStatus.Invalid;
                                    }
                                    dataModel.AppsTokens.Add(newToken);
                                    dataModel.SaveChanges();
                                }
                                else if (carpinoLastToken.TokenState != (int)TokenStatus.Expired)
                                {
                                    res.CarpinoTokenStatus = (short)carpinoLastToken.TokenState;
                                    res.CarpinoToken = carpinoLastToken.Token;
                                }
                                else
                                {
                                    res.CarpinoTokenStatus = (short)TokenStatus.Invalid;
                                }
                            }
                            else
                            {
                                res.CarpinoTokenStatus = carpinoLastToken.TokenState;
                                res.CarpinoToken = carpinoLastToken.Token;
                            }
                        }
                        else
                        {
                            res.CarpinoTokenStatus = (short)TokenStatus.Invalid;
                        }
                    }
                    else
                    {
                        var token = GetNewCarpinoToken();
                        var newToken = new AppsToken();
                        newToken.TokenApp = (short)TokenApp.CarpinoApp;
                        newToken.TokenCreateTime = DateTime.Now;
                        newToken.Token = "Bearer " + token.CarpinoToken;
                        if (token.CarpinoToken != null)
                        {
                            newToken.TokenState = (short)TokenStatus.Valid;
                            res.CarpinoTokenStatus = (short)TokenStatus.Valid;
                            res.CarpinoToken = "Bearer " + token.CarpinoToken;
                        }
                        else
                        {
                            newToken.TokenState = (short)TokenStatus.Invalid;
                            res.CarpinoTokenStatus = (short)TokenStatus.Invalid;
                        }
                        dataModel.AppsTokens.Add(newToken);
                        dataModel.SaveChanges();
                    }
                }
                if (model.AloPeykTokenStatus == (int)TokenStatus.NotSet ||
                    model.AloPeykTokenStatus == (int)TokenStatus.Expired)
                {
                    var aloPeykToken =
                        dataModel.AppsTokens.OrderByDescending(x => x.TokenCreateTime)
                            .FirstOrDefault(x => x.TokenApp== (short)TokenApp.AlopeykApp);
                    if (aloPeykToken != null)
                    {
                        if (aloPeykToken.TokenState != (int)TokenStatus.Expired)
                        {
                            if (model.AloPeykTokenStatus == (int)TokenStatus.Expired)
                            {
                                if (aloPeykToken.Token == model.AloPeykToken && aloPeykToken.TokenState != (int)TokenStatus.Expired){ 
                                aloPeykToken.TokenState = (short)model.AloPeykTokenStatus;
                                var token = SendAloPeykSms();
                                var newToken = new AppsToken();
                                newToken.TokenApp = (short)TokenApp.AlopeykApp;
                                newToken.TokenCreateTime = DateTime.Now;
                                newToken.TokenState = (short)TokenStatus.Invalid;
                                newToken.Token = token;
                                dataModel.AppsTokens.Add(newToken);
                                dataModel.SaveChanges();
                                res.AloPeykTokenStatus = (short)TokenStatus.Invalid;
                                res.AloPeykToken = aloPeykToken.Token;
                                }
                                else if (aloPeykToken.TokenState != (int)TokenStatus.Expired)
                                {
                                    res.AloPeykTokenStatus = aloPeykToken.TokenState;
                                    res.AloPeykToken = aloPeykToken.Token;
                                }
                                else
                                {
                                    res.AloPeykTokenStatus = (short)TokenStatus.Invalid;
                                }
                            }
                            else
                            {
                                res.AloPeykTokenStatus = aloPeykToken.TokenState;
                                res.AloPeykToken = aloPeykToken.Token;
                            }
                        }
                        else
                        {
                    res.AloPeykTokenStatus = (short)TokenStatus.Invalid;
                    }
                    }
                    else
                    {
                        var token = SendAloPeykSms();
                        var newToken = new AppsToken();
                        newToken.TokenApp = (short)TokenApp.AlopeykApp;
                        newToken.TokenCreateTime = DateTime.Now;
                        newToken.TokenState = (short)TokenStatus.Invalid;
                        newToken.Token = token;
                        dataModel.AppsTokens.Add(newToken);
                        dataModel.SaveChanges();
                        res.AloPeykTokenStatus = (short)TokenStatus.Invalid;
                        res.AloPeykToken = token;
                    }
                }


                /*var aloPeykToken =
                        dataModel.AppsTokens.OrderByDescending(x => x.TokenCreateTime)
                            .FirstOrDefault(x => x.TokenApp == (short)TokenApp.AlopeykApp);
                res.AloPeykToken = aloPeykToken.Token;
                res.AloPeykTokenStatus = (short)TokenStatus.Valid;*/
            }
            return res;
        }

        public string GetAlopeykToken(string code)
        {
            using (var dataModel = new MibarimEntities())
            {
                var aloPeykToken =
                        dataModel.AppsTokens.OrderByDescending(x => x.TokenCreateTime)
                            .FirstOrDefault(x => x.TokenApp == (short)TokenApp.AlopeykApp && x.TokenState== (short)TokenStatus.Invalid);
                //fasdf
                TaxiMeterService taxiMeterService = new TaxiMeterService();
                var token = taxiMeterService.GetAloPeykToken(code, aloPeykToken.Token);
                var newToken = new AppsToken();
                newToken.TokenApp = (short)TokenApp.AlopeykApp;
                newToken.TokenCreateTime = DateTime.Now;
                newToken.TokenState = (short)TokenStatus.Valid;
                newToken.Token = "Bearer "+token;
                dataModel.AppsTokens.Add(newToken);
                dataModel.SaveChanges();
                return token;
            }
        }

        public PathPriceResponse GetTap30Price(SrcDstModel model)
        {
            var res=new PathPriceResponse();
            using (var dataModel = new MibarimEntities())
            {
                var tap30Token =
    dataModel.AppsTokens.OrderByDescending(x => x.TokenCreateTime)
        .FirstOrDefault(x => x.TokenApp == (short)TokenApp.Tap30App);
                res.Tap30PathPrice = GetPriceFromTap30(model,tap30Token.Token);

            }
            return res;
        }

        public string GetTap30Token(string code)
        {
            using (var dataModel = new MibarimEntities())
            {
                TaxiMeterService taxiMeterService = new TaxiMeterService();
                var token= taxiMeterService.GetTap30Token(code);
                var newToken = new AppsToken();
                newToken.TokenApp = (short)TokenApp.Tap30App;
                newToken.TokenCreateTime = DateTime.Now;
                newToken.TokenState = (short)TokenStatus.Valid;
                newToken.Token = token;
                dataModel.AppsTokens.Add(newToken);
                dataModel.SaveChanges();
                return token;
            }
        }

        public Gtoken GetGoogleApi(string googleApi)
        {
            var t = new Gtoken();
            t.Token = "AIzaSyBsUKE_j1OXlbbtjwNwvmA3N8aeaNvw7jk";
            return t;
        }

        private TmTokensModel GetNewCarpinoToken()
        {
            TmTokensModel model=new TmTokensModel();
              TaxiMeterService taxiMeterService = new TaxiMeterService();
            var ct = taxiMeterService.GetCarpinoToken();
            model.CarpinoToken = ct.authToken;
            model.CarpinoRefreshToken = ct.refreshToken;
            return model;
        }
        
        private string GetNewSnappToken()
        {
            TaxiMeterService taxiMeterService = new TaxiMeterService();
            return taxiMeterService.GetSnapToken();
        }

        private string GetNewTap30Token()
        {
            TaxiMeterService taxiMeterService = new TaxiMeterService();
            return taxiMeterService.SendTap30TokenSms();
        }

        private string GetPriceFromTap30(SrcDstModel model, string tap30Token)
        {
            TaxiMeterService taxiMeterService = new TaxiMeterService();
            return taxiMeterService.GetTap30Price(model.SrcLat,model.SrcLng,model.DstLat,model.DstLng, tap30Token);
        }

        private string SendAloPeykSms()
        {
            TaxiMeterService taxiMeterService = new TaxiMeterService();
            return taxiMeterService.SendAloPeykSms();
        }

    }
}