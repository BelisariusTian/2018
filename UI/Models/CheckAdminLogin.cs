using Common;
using Common.Cache;
using IBLL;
using Model;
using Spring.Context;
using Spring.Context.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UI.Controllers;

namespace UI.Models
{
    /// <summary>
    /// 检查是否登录
    /// </summary>
    /// <seealso cref="System.Web.Mvc.ActionFilterAttribute" />
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class CheckAdminLogin : ActionFilterAttribute
    {
        /// <summary>
        /// 在调用操作方法前调用。
        /// </summary>
        /// <param name="filterContext">有关当前请求和操作的信息。</param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            WebController controller = filterContext.Controller as WebController;

            string tokenStr = WebHelper.GetStringParam("token");
            if (string.IsNullOrEmpty(tokenStr))
            {
                filterContext.Result = new ExtJsonResult()
                {
                    Data = new { code = (int)SysEnum.参数错误, msg = "token不能为空，请重新登录" },
                    ContentEncoding = controller.Request.ContentEncoding,
                    ContentType = "application/json",
                    JSONPCallBack = controller.CallBack,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
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
                filterContext.Result = new ExtJsonResult()
                {
                    Data = new { code = (int)SysEnum.参数错误, msg = "token错误，请重新登录" },
                    ContentEncoding = controller.Request.ContentEncoding,
                    ContentType = "application/json",
                    JSONPCallBack = controller.CallBack,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
                return;
            }
            if (!userip.Equals(filterContext.HttpContext.Request.UserHostAddress))
            {
                filterContext.Result = new ExtJsonResult()
                {
                    Data = new { code = (int)SysEnum.IP不匹配, msg = "IP地址发生变化，请重新登录" },
                    ContentEncoding = controller.Request.ContentEncoding,
                    ContentType = "application/json",
                    JSONPCallBack = controller.CallBack,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
                return;
            }
			var strUser = CacheHelper.GetCache(userid.ToString()) as administrator;
            if (strUser == null)
            {
                filterContext.Result = new ExtJsonResult()
                {
                    Data = new { code = (int)SysEnum.登录超时, msg = "登录超时，请重新登录 01" },
                    ContentEncoding = controller.Request.ContentEncoding,
                    ContentType = "application/json",
                    JSONPCallBack = controller.CallBack,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
                return;
            }

            administrator admin = strUser;
            if (admin == null)
            {
                filterContext.Result = new ExtJsonResult()
                {
                    Data = new { code = (int)SysEnum.登录超时, msg = "登录超时，请重新登录 02" },
                    ContentEncoding = controller.Request.ContentEncoding,
                    ContentType = "application/json",
                    JSONPCallBack = controller.CallBack,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
                return;
            }

            //滑动窗口机制
            CacheHelper.SetCache(userid.ToString(), admin, DateTime.Now.AddMinutes(20));

            //校验权限
            //把当前请求对应的权限拿到
            if (admin.login_account == "admin")
            {
                return;
            }
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
                    filterContext.Result = new ExtJsonResult()
                    {
                        Data = new { code = (int)SysEnum.权限不足, msg = "权限不足" },
                        ContentEncoding = controller.Request.ContentEncoding,
                        ContentType = "application/json",
                        JSONPCallBack = controller.CallBack,
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                    return;
                }

				//拿到当前管理员的所有权限id
				//1,获取角色信息 
				var roleList = admin.role.ToList();
				// 2.获取所有角色对应的权限信息
				var adminactionidList = CacheHelper.GetCache($"adminactionidList{admin.id}") as List<int>;
				//将管理员权限缓存起来，当修改权限的时候，记得更新
				if (adminactionidList == null || adminactionidList.Count == 0)
				{
					var actionidList = new List<int>();
					foreach (var item in roleList)
					{
						var acid = item.action.ToList().Select(n => n.id).ToList();
						actionidList.AddRange(acid);
					}
					actionidList.Distinct();
					if (actionidList.Count > 0)
					{
						CacheHelper.AddCache($"adminactionidList{admin.id}", actionidList, DateTime.Now.AddHours(2));
					}
				}
				if (!adminactionidList.Contains(actionInfo.id))
				{
					filterContext.Result = new ExtJsonResult()
					{
						Data = new { code = (int)SysEnum.权限不足, msg = "权限不足" },
						ContentEncoding = controller.Request.ContentEncoding,
						ContentType = "application/json",
						JSONPCallBack = controller.CallBack,
						JsonRequestBehavior = JsonRequestBehavior.AllowGet
					};
					return;
				}
				
            }
        }
    }

}