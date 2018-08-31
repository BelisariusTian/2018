using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace UI.Models
{
    public class ExceptionAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            base.OnException(filterContext);
            if (!filterContext.HttpContext.Request.Url.AbsolutePath.ToLower().Contains("error"))
            {
                Common.Log.LogHelper.Writelog(filterContext.HttpContext.Request.UserHostAddress + " >>" + filterContext.Exception.ToString());
            }

            filterContext.HttpContext.Application.Lock();
            filterContext.HttpContext.Application["Error"] = "记录时间：" + DateTime.Now + "\t IP：" + filterContext.HttpContext.Request.UserHostAddress + " \t URL：" + filterContext.HttpContext.Request.Url.AbsolutePath + "\r\n" + filterContext.Exception.ToString();
            filterContext.HttpContext.Application.UnLock();
            filterContext.HttpContext.Response.Redirect("/Error");
        }
    }
}