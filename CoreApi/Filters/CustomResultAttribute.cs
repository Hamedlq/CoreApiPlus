using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CoreManager.ResponseProvider;

namespace CoreApi.Filters
{
    public class CustomResultAttribute : FilterAttribute, IResultFilter
    {
        
        void IResultFilter.OnResultExecuted(ResultExecutedContext filterContext)
        {
            
        }

        void IResultFilter.OnResultExecuting(ResultExecutingContext filterContext)
        {
        }
    }
    public class CustomIActionAttribute : FilterAttribute, IActionFilter
    {
        void IActionFilter.OnActionExecuting(ActionExecutingContext filterContext)
        {
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
        }
    }
}