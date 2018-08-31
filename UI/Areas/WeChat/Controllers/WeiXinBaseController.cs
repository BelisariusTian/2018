///////////////////////////////////////////////////////////
//  时  间:2018年3月9日19:05:26
//  作  者: Leeseett
///////////////////////////////////////////////////////////
using IBLL;
using Model;
using Spring.Context;
using Spring.Context.Support;
using System;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;

namespace UI.Areas.WeChat.Controllers
{
    /// <summary>
    /// 微信端数据父类
    /// </summary>
    /// <seealso cref="UI.Controllers.WebController" />
    public class WeiXinBaseController : UI.Controllers.WebController
    {

        /// <summary>
        /// 微信公众号AppId
        /// </summary>
        public string AppId = ConfigurationManager.AppSettings["WeixinAppId"];
        /// <summary>
        /// 微信公众号Secret
        /// </summary>
        public string Secret = ConfigurationManager.AppSettings["WeixinAppSecret"];
        /// <summary>
        /// 网站域名
        /// </summary>
        public string DomainUrl = ConfigurationManager.AppSettings["Domain"];
        /// <summary>
        /// 当前授权登录的微信用户
        /// </summary>
        /// <value>
        /// The user information.
        /// </value>
        public wx_user UserInfo { get; set; }
		/// <summary>
		/// 在调用操作方法前调用。
		/// </summary>
		/// <param name="filterContext">有关当前请求和操作的信息。</param>
		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			IApplicationContext ctx = ContextRegistry.GetContext();
			Iwx_userService wx_userService = ctx.GetObject("wx_userService") as Iwx_userService;

			if (UserInfo == null)
			{
				string tokenStr = RequestParams["token"];

				if (string.IsNullOrEmpty(tokenStr))
				{
					filterContext.Result = Json(SysEnum.未授权登录, "缺少token");
					return;
				}
				string openid;
				string userip;
				RequestCategory RequestCategory;
				try
				{
					string[] arr = Common.EncryptHelper.Decrypt(tokenStr).Split('|');
					openid = arr[0];
					userip = arr[1];
					RequestCategory = (RequestCategory)Enum.Parse(typeof(RequestCategory), arr[2]);
				}
				catch (Exception)
				{
					filterContext.Result = Json(SysEnum.未授权登录, "token错误");
					return;
				}
				if (string.IsNullOrEmpty(openid) || string.IsNullOrEmpty(userip))
				{
					filterContext.Result = Json(SysEnum.未授权登录, "token错误");
					return;
				}
				//if (!userip.Equals(Request.UserHostAddress))
				//{
				//    filterContext.Result = Json(SysEnum.IP不匹配, "IP地址发生变化，请重新登录");
				//    return;
				//}

				switch (RequestCategory)
				{
					case RequestCategory.微信公众号:
						UserInfo = wx_userService.LoadEntities(s => s.gzh_openid == openid).FirstOrDefault();
						break;
					case RequestCategory.微信小程序:
						UserInfo = wx_userService.LoadEntities(s => s.xcx_openid == openid).FirstOrDefault();
						break;
					//case RequestCategory.APP:
					//	UserInfo = wx_userService.LoadEntities(s => s.appopenid == openid).FirstOrDefault();
					//	break;
					//case RequestCategory.WEB:
					//	UserInfo = wx_userService.LoadEntities(s => s.webopenid == openid).FirstOrDefault();
					//	break;
					default:
						filterContext.Result = Json(SysEnum.未授权登录, "token错误");
						return;
				}

				if (UserInfo == null)
				{
					filterContext.Result = Json(SysEnum.未授权登录);
					return;
				}
				else
				{
					if (UserInfo.state != (int)WXUserState.已关注)//数据库中状态未关注
					{
						try
						{
							var user = Senparc.Weixin.MP.AdvancedAPIs.UserApi.Info(AppId, UserInfo.gzh_openid, Senparc.Weixin.Language.zh_CN);//微信获取用户信息 该接口每日可调用 50000000 次 保险起见用try包裹
							if (user.subscribe == (int)WXUserState.已关注)//是否已关注
							{
								UserInfo.wx_head_protrait = user.headimgurl;
								UserInfo.sex = user.sex;
								UserInfo.city = user.city;
								UserInfo.country = user.country;
								UserInfo.state = (int)WXUserState.已关注;
								UserInfo.unionid = user.unionid;
								UserInfo.gzh_openid = user.openid;
								UserInfo.city = user.city;
								UserInfo.nickname = user.nickname;
								wx_userService.EditEntity(UserInfo);//已关注 修改用户状态
							}
						}
						catch (Exception)
						{

						}
					}
				}


				//只允许微信里访问
				//String userAgent = Request.UserAgent;
				//if (userAgent != null && userAgent.IndexOf("MicroMessenger", StringComparison.Ordinal) <= -1)
				//{
				//    HttpContext.Response.Write("请在微信浏览器里访问");
				//    HttpContext.Response.End();

				//}

			}
		}
    }
}
