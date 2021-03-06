﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using System.Web.Http.Results;
using System.Web.Script.Serialization;
using CoreManager.LogProvider;
using CoreManager.Models;
using CoreManager.Models.RouteModels;
using CoreManager.Resources;
using CoreManager.ResponseProvider;
using CoreManager.RouteGroupManager;
using CoreManager.RouteManager;
using CoreManager.TaxiMeterManager;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CoreApi.Controllers
{
    [AllowAnonymous]
    public class TaxiMeterController : ApiController
    {
        private static string Tag = "TaxiMeterController";
        private IRouteManager _routemanager;
        private ITaxiMeterManager _taxiMeterManager;
        private ILogProvider _logmanager;
        private IResponseProvider _responseProvider;

        
        public TaxiMeterController()
        {
        }
        public TaxiMeterController(IRouteManager routeManager, 
            ILogProvider logManager,
            ITaxiMeterManager taxiMeterManager,
            IResponseProvider responseProvider)
        {
            _routemanager = routeManager;
            _logmanager = logManager;
            _taxiMeterManager = taxiMeterManager;
            _responseProvider = responseProvider;
        }

        [HttpPost]
        [Route("GetPathPrice")]
        [AllowAnonymous]
        public IHttpActionResult GetPathPrice(SrcDstModel model)
        {
            try
            {
                var routePrice = _routemanager.GetPathPrice(model);
                ResponseModel responseModel = _responseProvider.GenerateResponse( routePrice , "pathprice");
                return Json(responseModel);
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "GetPathPrice", e.Message);
            }
            return null;
        }

        [HttpPost]
        [Route("GetTap30Price")]
        [AllowAnonymous]
        public IHttpActionResult GetTap30Price(SrcDstModel model)
        {
            try
            {
                var token = _taxiMeterManager.GetTap30Price(model);
                return Json(token);
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "GetTap30Price", e.Message);
            }
            return null;
        }

        [HttpPost]
        [Route("GetTokens")]
        [AllowAnonymous]
        public IHttpActionResult GetTokens(TmTokensModel model)
        {
            try
            {
                //model=new TmTokensModel();
                var tokens = _taxiMeterManager.GetTokens(model);
                ResponseModel responseModel = _responseProvider.GenerateResponse(tokens, "Tokens");
                return Json(responseModel);
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "GetTokens", e.Message);
            }
            return null;
        }

        [HttpGet]
        [Route("GetTap30Token")]
        [AllowAnonymous]
        public IHttpActionResult GetTap30Token(string code)
        {
            try
            {
                var token =_taxiMeterManager.GetTap30Token(code);
                return Json(token);
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "GetTap30Token", e.Message);
            }
            return null;
        }

        [HttpGet]
        [Route("GetAlopeykToken")]
        [AllowAnonymous]
        public IHttpActionResult GetAlopeykToken(string code)
        {
            try
            {
                var token = _taxiMeterManager.GetAlopeykToken(code);
                return Json(token);
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "GetTap30Token", e.Message);
            }
            return null;
        }


        [HttpPost]
        [Route("GetGoogleApi")]
        [AllowAnonymous]
        public IHttpActionResult GetGoogleApi(Gtoken model)
        {
            try
            {
                var token = _taxiMeterManager.GetGoogleApi(model.Token);
                return Json(_responseProvider.GenerateRouteResponse(token, "GoogleApi"));
            }
            catch (Exception e)
            {
                _logmanager.Log(Tag, "GetGoogleApi", e.Message);
            }
            return null;
        }

    }
}
