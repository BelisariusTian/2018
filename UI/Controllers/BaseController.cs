///////////////////////////////////////////////////////////
//  时  间:2018年3月9日19:05:26
//  作  者: Leeseett
///////////////////////////////////////////////////////////
using Common;
using Common.Cache;
using IBLL;
using Model;
using Spring.Context;
using Spring.Context.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace UI.Controllers
{
    /// <summary>
    /// 后台基类
    /// </summary>
    /// <seealso cref="UI.Controllers.WebController" />
    public class BaseController : WebController
    {
        /// <summary>
        /// 在调用操作方法前调用。
        /// </summary>
        /// <param name="filterContext">有关当前请求和操作的信息。</param>
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            string tokenStr = RequestParams["token"];
            if (string.IsNullOrEmpty(tokenStr))
            {
                filterContext.Result = Json(SysEnum.参数错误, "缺少token，请重新登录");
                return;
            }
            int userid;
            string userip;
            try
            {
                string[] arr = Common.EncryptHelper.Decrypt(tokenStr).Split('|');
                userid = int.Parse(arr[0]);
                userip = arr[1];
            }
            catch (Exception)
            {
                filterContext.Result = Json(SysEnum.参数错误, "token错误，请重新登录");
                return;
            }
            if (!userip.Equals("::1")&&!userip.Equals(Request.UserHostAddress))
            {
                filterContext.Result = Json(SysEnum.IP不匹配, "IP地址发生变化，请重新登录");
                return;
            }
            var strUser = CacheHelper.GetCache(userid.ToString()) as administrator;
            if (strUser == null)
            {
                filterContext.Result = Json(SysEnum.登录超时, "登录超时，请重新登录 01");
                return;
            }

			administrator admin = strUser;
            if (admin == null)
            {
                filterContext.Result = Json(SysEnum.登录超时, "登录超时，请重新登录 02");
                return;
            }
            nowManager = admin;
            

            //滑动窗口机制
            CacheHelper.SetCache(userid.ToString(), admin, DateTime.Now.AddMinutes(60));

            //校验权限
            //把当前请求对应的权限拿到
            //if (admin.login_account == "admin")
            //{
            //    return;
            //}
            //拿到当前的url和访问方式
            string url = filterContext.HttpContext.Request.Url.AbsolutePath.ToLower();
            string httpMethod = filterContext.HttpContext.Request.HttpMethod.ToLower();

            ////通过Spring.Net容器创建对象
            IApplicationContext ctx = ContextRegistry.GetContext();

            IactionService actionService = ctx.GetObject("actionService") as IactionService;

			IadministratorService administratorService = ctx.GetObject("administratorService") as IadministratorService;

            if (!url.Contains("home"))
            {
                //拿到当前请求对应的权限数据
                var actionInfo = actionService.LoadEntities(a => a.url.ToLower() == url && httpMethod == a.http_method.ToLower()).FirstOrDefault();
                if (actionInfo == null)
                {
                    filterContext.Result = Json(SysEnum.权限不足);
                    return;
                }
				if (actionInfo.type==(int)ActionType.普通权限)
				{
					return;
				}
				//拿到当前管理员的所有权限id
				//1,获取角色信息 
				var roleList = admin.role.ToList();
				// 2.获取所有角色对应的权限信息
				var adminactionidList = CacheHelper.GetCache($"adminactionidList_{nowManager.id}") as List<int>;
				//将管理员权限缓存起来，当修改权限的时候，记得更新
				if (adminactionidList == null|| adminactionidList.Count==0)
				{
					adminactionidList = new List<int>();
					foreach (var item in roleList)
					{
						var acid = item.action.ToList().Select(n => n.id).ToList();
						adminactionidList.AddRange(acid);
					}
					adminactionidList.Distinct();
					if (adminactionidList.Count>0)
					{
						CacheHelper.AddCache($"adminactionidList_{nowManager.id}", adminactionidList, DateTime.Now.AddHours(2));
					}
				}				
				if (!adminactionidList.Contains(actionInfo.id))
				{
					filterContext.Result = Json(SysEnum.权限不足);
					return;
				}
            }

        }

    }
}
