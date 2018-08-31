using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Senparc.Weixin.Entities;
using Senparc.Weixin.Exceptions;
using Senparc.Weixin.MP;
using Senparc.Weixin.MP.CommonAPIs;
using Senparc.Weixin.MP.Containers;
using Senparc.Weixin.MP.Entities.Menu;

namespace UI.Controllers
{
	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="UI.Controllers.BaseController" />
	public class WXMenuController : BaseController
    {
		//
		// GET: /WXMenu/
		/// <summary>
		/// 微信公众号AppId
		/// </summary>
		private string AppId = ConfigurationManager.AppSettings["WeixinAppId"];
		/// <summary>
		/// 微信公众号Secret
		/// </summary>
		private string Secret = ConfigurationManager.AppSettings["WeixinAppSecret"];

		#region 获取微信菜单数据
		/// <summary>
		/// 获取微信菜单数据
		/// </summary>
		/// <returns></returns>
		public ActionResult GetMenu()
		{
			try
			{
				var result = CommonApi.GetMenu(AccessTokenContainer.TryGetAccessToken(AppId, Secret));
				if (result == null)
				{
					return Json(Model.SysEnum.失败, "菜单不存在或验证失败");
				}
				return Json(Model.SysEnum.成功, result, "获取数据成功");
			}
			catch (WeixinMenuException ex)
			{
				return Json(Model.SysEnum.失败, "菜单不存在或验证失败：" + ex.Message);
			}
			catch (Exception ex)
			{
				return Json(Model.SysEnum.失败, "菜单不存在或验证失败：" + ex.Message);
			}
		}
		#endregion


		#region 使用Json数据更新菜单
		/// <summary>
		/// 使用Json数据更新菜单
		/// </summary>
		/// <param name="fullJson">json数据</param>
		/// <returns></returns>
		public ActionResult CreateMenuFromJson(string fullJson)
		{
			try
			{
				GetMenuResultFull resultFull = Newtonsoft.Json.JsonConvert.DeserializeObject<GetMenuResultFull>(fullJson);

				//重新整理按钮信息
				WxJsonResult result = null;
				IButtonGroupBase buttonGroup = null;

				buttonGroup = CommonApi.GetMenuFromJsonResult(resultFull, new ButtonGroup()).menu;
				result = CommonApi.CreateMenu(AccessTokenContainer.TryGetAccessToken(AppId, Secret), buttonGroup);
				return Json(Model.SysEnum.成功, "菜单更新成功");
			}
			catch (Exception ex)
			{
				return Json(Model.SysEnum.失败, string.Format("更新失败：{0}", ex.Message));
			}
		}
		#endregion
	}
}
