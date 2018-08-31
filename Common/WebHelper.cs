using System;
using System.Web;

namespace Common
{
    public class WebHelper
    {
        /// <summary>
        /// 是否Get请求
        /// </summary>
        /// <returns></returns>
        public static bool IsGet()
        {
            return HttpContext.Current.Request.HttpMethod == "GET";
        }

        /// <summary>
        /// 是否是post请求
        /// </summary>
        /// <returns></returns>
        public static bool IsPost()
        {
            return HttpContext.Current.Request.HttpMethod == "POST";
        }

        /// <summary>
        /// 是否是Ajax请求
        /// </summary>
        /// <returns></returns>
        public static bool IsAjax()
        {
            return HttpContext.Current.Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }

        public static string GetPhsyicalPath(string virtualPath)
        {
            HttpContext context = HttpContext.Current;
            if (context != null) return context.Server.MapPath(virtualPath);
            return System.Web.Hosting.HostingEnvironment.MapPath(virtualPath);

        }

        protected static string GetParam(string key, string defaultValue)
        {
            HttpContext context = HttpContext.Current;
            string value = context.Request.Params[key];

            return string.IsNullOrEmpty(value) ? defaultValue : value;
        }

        public static string GetStringParam(string key, string defaultValue)
        {

            return GetParam(key, defaultValue);

        }
        public static string GetStringParam(string key)
        {

            return GetStringParam(key, string.Empty);
        }

        public static long GetLongParam(string key)
        {
            return GetLongParam(key, 0);
        }

        public static long GetLongParam(string key, long defaultvalue)
        {
            string value = GetStringParam(key, defaultvalue.ToString()).Trim();
            long v = defaultvalue;
            return long.TryParse(value, out v) ? v : defaultvalue;

        }

        public static Guid GetGuidParam(string key, Guid defaultvalue)
        {
            string value = GetStringParam(key, defaultvalue.ToString());
            Guid g = defaultvalue;

            return Guid.TryParse(value, out g) ? g : defaultvalue;


        }

        public static Guid GetGuidParam(string key)
        {
            return GetGuidParam(key, Guid.Empty);
        }


    }
}
