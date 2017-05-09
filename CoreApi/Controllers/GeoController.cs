using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace CoreApi.Controllers
{
    public class GeoController : ApiController
    {
        public string GetAgencies()
        {
            var featureCollection = new FeatureCollection();

            var point = new Point(new GeographicPosition( 40.760471, -73.988106));
            var featureProperties = new Dictionary<string, object> { { "NAME", "حامد لشکری" }, { "TEL", "09358695785" }, { "URL", "http" }, { "ADDRESS1", "سلمان فارسی پ 104" }, { "ADDRESS2", "هفت تیر" }, { "CITY", "تهران" } };
            var feature = new Feature(point, featureProperties);
            featureCollection.Features.Add(feature);
            var serializedData = JsonConvert.SerializeObject(featureCollection, Formatting.Indented,
               new JsonSerializerSettings
               {
                   NullValueHandling = NullValueHandling.Ignore
               });
            return serializedData;
        }
    }
}
