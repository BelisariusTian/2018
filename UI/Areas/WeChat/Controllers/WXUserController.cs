///////////////////////////////////////////////////////////
//  时  间:2018年3月9日19:05:26
//  作  者: Leeseett
///////////////////////////////////////////////////////////
using Model;
using System.Web.Mvc;

namespace UI.Areas.WeChat.Controllers
{
    /// <summary>
    /// 微信用户相关接口
    /// </summary>
    /// <seealso cref="UI.Areas.WeChat.Controllers.WeiXinBaseController" />
    public class WXUserController : WeiXinBaseController
    {
        //
        // GET: /WeChat/WXUser/

        /// <summary>
        /// 获取当前用户信息
        /// </summary>
        /// <returns></returns>
        public ActionResult GetNowUserInfo()
        {
            dynamic data = new
            {
                UserInfo.id,
                UserInfo.nickname,
                UserInfo.sex,
                UserInfo.wx_head_protrait,
				UserInfo.user.qr_code,
            };
            return Json(SysEnum.成功, data, "获取数据成功");
        }
    }
}
