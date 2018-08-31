///////////////////////////////////////////////////////////
//  时  间:2018年3月9日19:05:26
//  作  者: Leeseett
///////////////////////////////////////////////////////////
using Common;
using Common.Cache;
using IBLL;
using Model;
using Spring.Context.Support;
using System;
using System.Collections.Specialized;
using System.Text;
using System.Web.Mvc;
using UI.Models;

namespace UI.Controllers
{
    /// <summary>
    /// Web请求基类
    /// </summary>
    /// <seealso cref="System.Web.Mvc.Controller" />
    public class WebController : Controller
    {
        /// <summary>
        /// jsonp回调方法
        /// </summary>
        /// <value>
        /// The call back.
        /// </value>
        public string CallBack { get; private set; }
        /// <summary>
        /// Gets or sets the now manager.
        /// </summary>
        /// <value>
        /// 当前登录的管理员
        /// </value>
        public administrator nowManager { get; set; }
        private readonly string jsonp = "callback";
        /// <summary>
        /// 所有请求参数集
        /// </summary>
        /// <value>
        /// The request parameters.
        /// </value>
        public NameValueCollection RequestParams { get; private set; }
        /// <summary>
        /// 对调用构造函数时可能不可用的数据进行初始化。
        /// </summary>
        /// <param name="requestContext">HTTP 上下文和路由数据。</param>
        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);
            this.CallBack = WebHelper.GetStringParam(jsonp, string.Empty).Trim();
            RequestParams = new NameValueCollection();
            foreach (string key in Request.QueryString.AllKeys)
            {
                RequestParams[key] = Request.QueryString[key];
            }
            foreach (string key in Request.Form.AllKeys)
            {
                RequestParams[key] = Request.Form[key];

            }
            string tokenStr = RequestParams["token"];
            int userid;
            string userip;
            try
            {
                string[] arr = Common.EncryptHelper.Decrypt(tokenStr).Split('|');
                userid = int.Parse(arr[0]);
                userip = arr[1];
                string strUser = CacheHelper.GetCache(userid.ToString()) as string;
                if (strUser != null)
                {
                    nowManager = SerializeHelper.SerializeToObject<administrator>(strUser);
                }

            }
            catch (Exception)
            {

            }
        }
        /// <summary>
        /// 创建一个将指定对象序列化为 JavaScript 对象表示法 (JSON) 格式的 <see cref="T:System.Web.Mvc.JsonResult" /> 对象。
        /// </summary>
        /// <param name="data">要序列化的 JavaScript 对象图。</param>
        /// <param name="contentType">内容类型（MIME 类型）。</param>
        /// <param name="contentEncoding">内容编码。</param>
        /// <returns>
        /// 将指定对象序列化为 JSON 格式的 JSON 结果对象。
        /// </returns>
        protected override JsonResult Json(object data, string contentType, Encoding contentEncoding)
        {
            return Json(data, contentType, contentEncoding, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 创建 <see cref="T:System.Web.Mvc.JsonResult" /> 对象，该对象使用内容类型、内容编码和 JSON 请求行为将指定对象序列化为 JavaScript 对象表示法 (JSON) 格式。
        /// </summary>
        /// <param name="data">要序列化的 JavaScript 对象图。</param>
        /// <param name="contentType">内容类型（MIME 类型）。</param>
        /// <param name="contentEncoding">内容编码。</param>
        /// <param name="behavior">JSON 请求行为</param>
        /// <returns>
        /// 将指定对象序列化为 JSON 格式的结果对象。
        /// </returns>
        protected override JsonResult Json(object data, string contentType, Encoding contentEncoding, JsonRequestBehavior behavior)
        {
            contentEncoding = contentEncoding == null ? Encoding.UTF8 : contentEncoding;
            return new Model.ExtJsonResult() { Data = data, ContentType = contentType, ContentEncoding = contentEncoding, JsonRequestBehavior = behavior, JSONPCallBack = CallBack };
        }
        /// <summary>
        /// Jsons the specified code.
        /// </summary>
        /// <param name="code">返回枚举</param> 
        /// <returns></returns>
        protected JsonResult Json(Enum code)
        {
            return Json(code, code.ToString());
        }
        /// <summary>
        /// Jsons the specified code.
        /// </summary>
        /// <param name="code">状态枚举</param>
        /// <param name="msg">返回消息</param>
        /// <param name="count">总条数</param>
        /// <returns></returns>
        protected JsonResult Json(Enum code, string msg, int count = 0)
        {
            return Json<object>(code, null, msg, count);
        }
        /// <summary>
        /// Jsons the specified code.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="code">状态枚举</param>
        /// <param name="data">返回数据</param>
        /// <param name="msg">返回消息</param>
        /// <param name="count">总条数.</param>
        /// <returns></returns>
        protected JsonResult Json<T>(Enum code, T data, string msg, int count = 0) where T : class
        {
            BaseApiResponse rsp = new BaseApiResponse();
            rsp.code = code;
            rsp.msg = msg;
            rsp.count = count;
            rsp.data = data;
            return Json(rsp, "application/json", Request.ContentEncoding, JsonRequestBehavior.AllowGet);
        }

        #region 日志记录
        /// <summary>
        /// 保存日志
        /// </summary>
        /// <param name="content">日志内容.</param>
        /// <param name="logType">SysLogType 枚举</param>
        /// <param name="userName">记录人名称</param>
        public void  SaveSyslog(string content, SysLogType logType, string userName)
        {
            Isystem_logService system_logService = ContextRegistry.GetContext().GetObject("system_logService") as Isystem_logService;
			system_log log = new system_log
			{
				id = Guid.NewGuid(),
				add_name = userName??"用户",
                content = content??string.Empty,
				add_time = DateTime.Now,
				ip_address = Request.UserHostAddress,
				page_url = Request.Url.ToString(),
                type = (int)logType,
            };
			system_logService.AddEntity(log);
        }
        #endregion
    }
}
