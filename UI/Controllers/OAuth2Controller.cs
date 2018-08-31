///////////////////////////////////////////////////////////
//  时  间:2018年3月9日19:05:26
//  作  者: Leeseett
///////////////////////////////////////////////////////////
using IBLL;
using Model;
using Senparc.Weixin;
using Senparc.Weixin.Exceptions;
using Senparc.Weixin.HttpUtility;
using Senparc.Weixin.MP;
using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.MP.AdvancedAPIs.OAuth;
using Senparc.Weixin.MP.Helpers;
using Senparc.Weixin.WxOpen.AdvancedAPIs.Sns;
using Senparc.Weixin.WxOpen.Containers;
using Senparc.Weixin.WxOpen.Entities;
using System;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace UI.Controllers
{
	/// <summary>
	/// 微信授权登录控制器
	/// </summary>
	/// <seealso cref="UI.Controllers.WebController" />
	public class OAuth2Controller : WebController
	{
		/// <summary>
		/// 公众号appid
		/// </summary>
		private string appId = ConfigurationManager.AppSettings["WeixinAppId"];
		/// <summary>
		/// 公众号secret
		/// </summary>
		private string secret = ConfigurationManager.AppSettings["WeixinAppSecret"];
		/// <summary>
		/// 网站域名
		/// </summary>
		private string domain = ConfigurationManager.AppSettings["Domain"];
		/// <summary>
		/// 小程序appid
		/// </summary>
		private string wxOpenAppId = ConfigurationManager.AppSettings["WxOpenAppId"];
		/// <summary>
		/// 小程序secret
		/// </summary>
		private string wxOpenAppSecret = ConfigurationManager.AppSettings["WxOpenAppSecret"];

		/// <summary>
		/// 开放平台appid
		/// </summary>
		private string Component_Appid = ConfigurationManager.AppSettings["Component_Appid"];
		/// <summary>
		/// 开放平台secret
		/// </summary>
		private string Component_Secret = ConfigurationManager.AppSettings["Component_Secret"];

		private Iwx_userService Wx_userService { get; set; }
		private Iconfig_ruleService Config_ruleService { get; set; }
		private Iuser_score_recordService User_score_recordService { get; set; }
		//
		// GET: /OAuth2/

		#region 公众号授权入口
		/// <summary>
		/// 公众号授权入口
		/// </summary>
		/// <param name="goUrl">要授权的url地址</param>
		/// <param name="pid"></param>
		/// <returns></returns>
		public ActionResult Index(string goUrl, int pid = 0)
		{
			//goUrl = Common.EncryptHelper.Encrypt(goUrl+"?pid="+ pid);
			goUrl = Common.EncryptHelper.Encrypt(goUrl);
			var state = "JeffreySu-" + DateTime.Now.Millisecond;//随机数，用于识别请求可靠性
			string url = OAuthApi.GetAuthorizeUrl(appId, "http://" + domain + "/OAuth2/UserInfoCallback?goUrl=" + goUrl, state, OAuthScope.snsapi_userinfo);
			return Redirect(url);
		}
		#endregion

		#region 公众号授权回调
		/// <summary>
		/// 公众号授权回调
		/// </summary>
		/// <param name="code">The code.</param>
		/// <param name="state">The state.</param>
		/// <param name="goUrl">要跳转的url地址</param>
		/// <returns>跳转的url地址会带上Token</returns>
		public ActionResult UserInfoCallback(string code, string state, string goUrl)
		{
			goUrl = Common.EncryptHelper.Decrypt(goUrl);
			var temp = Request;
			string parms = string.Empty;
			string strpid = string.Empty;
			if (goUrl.IndexOf('?') > -1)
			{
				parms = goUrl.Substring(goUrl.IndexOf('?'));
				if (parms.Contains("pid"))
				{
					strpid = parms.Substring(parms.IndexOf("pid")).Split('=')[1];
				}
			}
			//var strpid = goUrl.Substring(goUrl.IndexOf('?'), goUrl.Length).Split('=')[1];

			if (string.IsNullOrEmpty(code))
			{
				return Content("您拒绝了授权！");
			}

			OAuthAccessTokenResult result = null;
			string token = string.Empty;
			//通过，用code换取access_token
			try
			{
				result = OAuthApi.GetAccessToken(appId, secret, code);
				token = Common.EncryptHelper.Encrypt(string.Format("{0}|{1}|{2}", result.openid, Request.UserHostAddress, RequestCategory.微信公众号));
			}
			catch (Exception ex)
			{
				if (ex.Message.Contains("40163"))
				{
					return Redirect(goUrl);
				}
				return Content(ex.Message);
			}

			if (result.errcode != ReturnCode.请求成功)
			{
				return Redirect(goUrl);
			}
            try
            {
                var wxUser = OAuthApi.GetUserInfo(result.access_token, result.openid);
                var tempUser = Wx_userService.LoadEntities(u => u.unionid == wxUser.unionid || u.gzh_openid == wxUser.openid).FirstOrDefault();
                if (!int.TryParse(strpid, out int pid))
                {
                    pid = 0;
                }
			
				if (tempUser != null)
                {
                    if (tempUser.state == (int)WXUserState.取消关注)
                    {
                        tempUser.state = (int)WXUserState.未关注;
                        tempUser.add_time = DateTime.Now;
                    }
                    //if (tempUser.unsubscribe_time==null||tempUser.unsubscribe_time<=DateTime.Now.AddDays(-7))
                    if (true)
                    {
                        if (tempUser.user == null)
                        {
                            //新用户
                            var user = new user();
                            user.pid = pid;
                            user.name = wxUser.nickname;
                            user.sex = wxUser.sex;
                            user.state = (int)User_state.正常;
                            user.isbuy = (int)Isbuy.未购买;
                            //首次关注赠送积分
                            var config_ruleItem = Config_ruleService.LoadEntities(n => n.name == "首次关注" && n.state == (int)State.可用).FirstOrDefault();
                            if (config_ruleItem != null)
                            {
                                var usr = new user_score_record();
                                usr.id = Guid.NewGuid();
                                usr.score = config_ruleItem.value != null ? Convert.ToDecimal(config_ruleItem.value) : 0;
                                usr.type = (int)Score_type.收益;
                                usr.state = (int)User_score_record_state.已完成;
                                usr.source_name = "首次关注系统赠送";
                                usr.add_time = DateTime.Now;
                                usr.remark = ((int)Score_source_remark.系统赠送).ToString();
                                user.total_score += usr.score;
                                user.usable_score += user.total_score;
                                user.user_score_record.Add(usr);
                            }
                            tempUser.user = user;
                        }
                        tempUser.unionid = wxUser.unionid;
                        tempUser.gzh_openid = wxUser.openid;
                        tempUser.nickname = wxUser.nickname;
                        tempUser.wx_head_protrait = wxUser.headimgurl;
                        tempUser.sex = wxUser.sex;
                        tempUser.city = wxUser.city;
                        tempUser.province = wxUser.province;
                        tempUser.country = wxUser.country;
                        tempUser.unsubscribe_time = DateTime.Now;
                        Wx_userService.EditEntity(tempUser);
                    }
                }
                else
                {
                    //新用户
                    var user = new user();
                    user.pid = pid;
                    user.name = wxUser.nickname;
                    user.sex = wxUser.sex;
                    user.state = (int)User_state.正常;
                    user.isbuy = (int)Isbuy.未购买;
                    //首次关注赠送积分
                    var config_ruleItem = Config_ruleService.LoadEntities(n => n.name == "首次关注" && n.state == (int)State.可用).FirstOrDefault();
                    if (config_ruleItem != null)
                    {
                        var usr = new user_score_record();
                        usr.id = Guid.NewGuid();
                        usr.score = config_ruleItem.value != null ? Convert.ToDecimal(config_ruleItem.value) : 0;
                        usr.type = (int)Score_type.收益;
                        usr.state = (int)User_score_record_state.已完成;
                        usr.source_name = "首次关注系统赠送";
                        usr.add_time = DateTime.Now;
                        usr.remark = ((int)Score_source_remark.系统赠送).ToString();
                        user.total_score += usr.score;
                        user.usable_score += user.total_score;
                        user.user_score_record.Add(usr);
                    }
                    var newUser = new wx_user()
                    {
                        city = wxUser.city,
                        country = wxUser.country,
                        wx_head_protrait = wxUser.headimgurl,
                        nickname = wxUser.nickname,
                        gzh_openid = wxUser.openid,
                        province = wxUser.province,
                        sex = wxUser.sex,
                        add_time = DateTime.Now,
                        unionid = wxUser.unionid,
                        state = (int)WXUserState.未关注,//未关注
                        user = user,
                    };
                    Wx_userService.AddEntity(newUser);
					//SaveSyslog($"{wxUser.nickname}=>({Url})加入系统,pid={pid}", SysLogType.前台日志, "授权系统");
				}
                var url = $"{goUrl}?state={Guid.NewGuid().ToString().Substring(0, 4)}#token={token}";
                return Redirect(url);
            }
            catch (ErrorJsonResultException ex)
            {
                return Content(ex.Message);
            }
        }
		#endregion

		#region 获取公众号JSSDK注册信息
		/// <summary>
		/// 获取公众号JSSDK注册信息
		/// </summary>
		/// <returns></returns>
		public ActionResult GetWXConfig()
		{
			try
			{
				var s = JSSDKHelper.GetJsSdkUiPackage(appId, secret, Request.UrlReferrer.ToString().Split('#')[0]);
				return Json(SysEnum.成功, s, "获取成功");
			}
			catch (Exception ex)
			{
				return Json(SysEnum.失败, "获取失败：" + ex.ToString());
			}
		}
		#endregion

		//#region 小程序授权入口
		///// <summary>
		///// 小程序授权入口
		///// </summary>
		///// <param name="code">The code.</param>
		///// <returns>返回会话ID</returns>
		//public ActionResult OnLogin(string code)
		//{
		//	var jsonResult = SnsApi.JsCode2Json(wxOpenAppId, wxOpenAppSecret, code);
		//	if (jsonResult.errcode == ReturnCode.请求成功)
		//	{
		//		var sessionBag = SessionContainer.UpdateSession(null, jsonResult.openid, jsonResult.session_key);
		//		dynamic data = new
		//		{
		//			sessionId = sessionBag.Key
		//		};
		//		return Json(SysEnum.成功, data, "登录成功");
		//	}
		//	else
		//	{
		//		return Json(SysEnum.失败);
		//	}
		//}
		//#endregion

		#region 获取小程序用户token
		/// <summary>
		/// 解密用户数据（获取用户token）
		/// </summary>
		/// <param name="sessionId">会话ID</param>
		/// <param name="encryptedData">要解密的数据</param>
		/// <param name="iv">解密向量</param>
		/// <returns>返回Token</returns>
		public ActionResult DecodeEncryptedData(string sessionId, string encryptedData, string iv)
		{
			var userInfoJsonStr = Senparc.Weixin.WxOpen.Helpers.EncryptHelper.DecodeEncryptedDataBySessionId(sessionId, encryptedData, iv);
			var wxUser = Common.SerializeHelper.SerializeToObject<DecodedUserInfo>(userInfoJsonStr);

			var tempUser = Wx_userService.LoadEntities(u => u.unionid == wxUser.unionId || u.xcx_openid == wxUser.openId).FirstOrDefault();
			if (tempUser != null)
			{
				if (tempUser.state == (int)WXUserState.取消关注)
				{
					tempUser.state = (int)WXUserState.未关注;
					tempUser.add_time = DateTime.Now;
				}
				tempUser.unionid = wxUser.unionId;
				tempUser.xcx_openid = wxUser.openId;
				tempUser.nickname = wxUser.nickName;
				tempUser.wx_head_protrait = wxUser.avatarUrl;
				tempUser.sex = wxUser.gender;
				tempUser.city = wxUser.city;
				tempUser.province = wxUser.province;
				tempUser.country = wxUser.country;
				Wx_userService.EditEntity(tempUser);
			}
			else
			{

				var newUser = new wx_user()
				{
					city = wxUser.city,
					country = wxUser.country,
					wx_head_protrait = wxUser.avatarUrl,
					nickname = wxUser.nickName,
					xcx_openid = wxUser.openId,
					province = wxUser.province,
					sex = wxUser.gender,
					add_time = DateTime.Now,
					unionid = wxUser.unionId,
					state = (int)WXUserState.未关注,//未关注
				};

				Wx_userService.AddEntity(newUser);
			}

			dynamic data = new { token = Common.EncryptHelper.Encrypt(string.Format("{0}|{1}|{2}", wxUser.openId, Request.UserHostAddress, RequestCategory.微信小程序)) };
			return Json(SysEnum.成功, data, "获取成功");

		}
		#endregion

		#region 扫码授权入口
		/// <summary>
		///扫码授权入口
		/// </summary>
		/// <param name="goUrl">要授权的url地址</param>
		/// <returns></returns>
		public ActionResult QRLogin(string goUrl)
		{

			var state = "JeffreySu-" + DateTime.Now.Millisecond;//随机数，用于识别请求可靠性

			Senparc.Weixin.Open.OAuthScope[] OAuthScope = new Senparc.Weixin.Open.OAuthScope[1];
			OAuthScope[0] = Senparc.Weixin.Open.OAuthScope.snsapi_login;


			var url = Senparc.Weixin.Open.QRConnect.QRConnectAPI.GetQRConnectUrl(Component_Appid, "http://" + domain + "/OAuth2/QRLoginCallback?goUrl=" + goUrl.UrlEncode(), state, OAuthScope);
			return Redirect(url);
		}
		#endregion

		#region 扫码授权回调
		/// <summary>
		/// 扫码授权回调
		/// </summary>
		/// <param name="code">The code.</param>
		/// <param name="state">The state.</param>
		/// <param name="goUrl">要跳转的url地址.</param>
		/// <returns></returns>
		public ActionResult QRLoginCallback(string code, string state, string goUrl)
		{

			goUrl = Common.EncryptHelper.Decrypt(goUrl);
			var temp = Request;
			string parms = string.Empty;
			string strpid = string.Empty;
			if (goUrl.IndexOf('?') > -1)
			{
				parms = goUrl.Substring(goUrl.IndexOf('?'));
				if (parms.Contains("pid"))
				{
					strpid = parms.Substring(parms.IndexOf("pid")).Split('=')[1];
				}
			}
			//var strpid = goUrl.Substring(goUrl.IndexOf('?'), goUrl.Length).Split('=')[1];

			if (string.IsNullOrEmpty(code))
			{
				return Content("您拒绝了授权！");
			}

			OAuthAccessTokenResult result = null;
			string token = string.Empty;
			//通过，用code换取access_token
			try
			{
				result = OAuthApi.GetAccessToken(appId, secret, code);
				token = Common.EncryptHelper.Encrypt(string.Format("{0}|{1}|{2}", result.openid, Request.UserHostAddress, RequestCategory.微信公众号));
			}
			catch (Exception ex)
			{
				if (ex.Message.Contains("40163"))
				{
					return Redirect(goUrl);
				}
				return Content(ex.Message);
			}

			if (result.errcode != ReturnCode.请求成功)
			{
				return Redirect(goUrl);
			}
			try
			{
				var wxUser = OAuthApi.GetUserInfo(result.access_token, result.openid);
				var tempUser = Wx_userService.LoadEntities(u => u.unionid == wxUser.unionid || u.gzh_openid == wxUser.openid).FirstOrDefault();
				if (!int.TryParse(strpid, out int pid))
				{
					pid = 0;
				}
				if (tempUser != null)
				{
					if (tempUser.state == (int)WXUserState.取消关注)
					{
						tempUser.state = (int)WXUserState.未关注;
						tempUser.add_time = DateTime.Now;
					}
					//if (tempUser.unsubscribe_time==null||tempUser.unsubscribe_time<=DateTime.Now.AddDays(-7))
					if (true)
					{
						if (tempUser.user == null)
						{
							//新用户
							var user = new user();
							user.pid = pid;
							user.name = wxUser.nickname;
							user.sex = wxUser.sex;
							user.state = (int)User_state.正常;
							user.isbuy = (int)Isbuy.未购买;
							//首次关注赠送积分
							var config_ruleItem = Config_ruleService.LoadEntities(n => n.name == "首次关注" && n.state == (int)State.可用).FirstOrDefault();
							if (config_ruleItem != null)
							{
								var usr = new user_score_record();
								usr.id = Guid.NewGuid();
								usr.score = config_ruleItem.value != null ? Convert.ToInt32(config_ruleItem.value) : 0;
								usr.type = (int)Score_type.收益;
								usr.state = (int)User_score_record_state.已完成;
								usr.source_name = "首次关注系统赠送";
								usr.add_time = DateTime.Now;
								usr.remark = ((int)Score_source_remark.系统赠送).ToString();
								user.total_score += usr.score;
								user.usable_score += user.total_score;
								user.user_score_record.Add(usr);
							}
							tempUser.user = user;
						}
						tempUser.unionid = wxUser.unionid;
						tempUser.gzh_openid = wxUser.openid;
						tempUser.nickname = wxUser.nickname;
						tempUser.wx_head_protrait = wxUser.headimgurl;
						tempUser.sex = wxUser.sex;
						tempUser.city = wxUser.city;
						tempUser.province = wxUser.province;
						tempUser.country = wxUser.country;
						tempUser.unsubscribe_time = DateTime.Now;
						Wx_userService.EditEntity(tempUser);
					}
				}
				else
				{
					//新用户
					var user = new user();
					user.pid = pid;
					user.name = wxUser.nickname;
					user.sex = wxUser.sex;
					user.state = (int)User_state.正常;
					user.isbuy = (int)Isbuy.未购买;
					//首次关注赠送积分
					var config_ruleItem = Config_ruleService.LoadEntities(n => n.name == "首次关注" && n.state == (int)State.可用).FirstOrDefault();
					if (config_ruleItem != null)
					{
						var usr = new user_score_record();
						usr.id = Guid.NewGuid();
						usr.score = config_ruleItem.value != null ? Convert.ToInt32(config_ruleItem.value) : 0;
						usr.type = (int)Score_type.收益;
						usr.state = (int)User_score_record_state.已完成;
						usr.source_name = "首次关注系统赠送";
						usr.add_time = DateTime.Now;
						usr.remark = ((int)Score_source_remark.系统赠送).ToString();
						user.total_score += usr.score;
						user.usable_score += user.total_score;
						user.user_score_record.Add(usr);
					}
					var newUser = new wx_user()
					{

						city = wxUser.city,
						country = wxUser.country,
						wx_head_protrait = wxUser.headimgurl,
						nickname = wxUser.nickname,
						gzh_openid = wxUser.openid,
						province = wxUser.province,
						sex = wxUser.sex,
						add_time = DateTime.Now,
						unionid = wxUser.unionid,
						state = (int)WXUserState.未关注,//未关注
						user = user,
					};
					Wx_userService.AddEntity(newUser);
				}
				return Redirect(goUrl + "#token=" + token);
			}
			catch (ErrorJsonResultException ex)
			{
				return Content(ex.Message);
			}

		}
		#endregion
	}
}
