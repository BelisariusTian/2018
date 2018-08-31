using System;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Model
{
    public class ExtJsonResult : System.Web.Mvc.JsonResult
    {
        public string JSONPCallBack { get; set; }
        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null) { throw new ArgumentNullException("context"); }
            if (Data == null) { throw new ArgumentNullException("data"); }
            if (ContentEncoding == null) ContentEncoding = Encoding.UTF8;
            //if ((this.JsonRequestBehavior == System.Web.Mvc.JsonRequestBehavior.DenyGet) && string.Equals(context.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
            //{
            //    throw new InvalidOperationException("该调用只能使用POST方式进行调用");
            //}

            HttpResponseBase response = context.HttpContext.Response;

            response.ContentEncoding = response.HeaderEncoding = ContentEncoding;
            response.ContentType = ContentType;
            if (!string.IsNullOrEmpty(JSONPCallBack))
            {
                string jsonp = string.Format("{0}({1})", JSONPCallBack, Common.SerializeHelper.SerializeToString(Data));
                response.Write(jsonp);
            }
            else
            {
                response.Write(Common.SerializeHelper.SerializeToString(Data));
            }
        }
    }
}
