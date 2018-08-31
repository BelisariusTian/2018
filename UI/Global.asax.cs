using Spring.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Common.Log;
using Senparc.Weixin.MP.Containers;
using Senparc.Weixin.MP.TenPayLibV3;
using UI.Models;
using UI.QuartzJob;
using Senparc.Weixin.MP;

namespace UI
{
    // 注意: 有关启用 IIS6 或 IIS7 经典模式的说明，
    // 请访问 http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : SpringMvcApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            LogHelper.SetConfig();
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            //  BundleConfig.RegisterBundles(BundleTable.Bundles);
            #region 微信公众号相关配置
            var appId = ConfigurationManager.AppSettings["WeixinAppId"];
            var secret = ConfigurationManager.AppSettings["WeixinAppSecret"];

			AccessTokenContainer.Register(appId, secret);

			var tenPayV3_MchId = ConfigurationManager.AppSettings["TenPayV3_MchId"];
			var tenPayV3_Key = ConfigurationManager.AppSettings["TenPayV3_Key"];
			var tenPayV3_AppId = ConfigurationManager.AppSettings["TenPayV3_AppId"];
			var tenPayV3_AppSecret = ConfigurationManager.AppSettings["TenPayV3_AppSecret"];
			var tenPayV3_TenpayNotify = ConfigurationManager.AppSettings["TenPayV3_TenpayNotify"];

			var tenPayV3Info = new TenPayV3Info(tenPayV3_AppId, tenPayV3_AppSecret, tenPayV3_MchId, tenPayV3_Key,
				tenPayV3_TenpayNotify);
			TenPayV3InfoCollection.Register(tenPayV3Info);
			#endregion

			RegisterView();//注册视图访问规则
			MyJobScheduler.Run();
        }
        protected void RegisterView()
        {
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new MyViewEngine());
        }
    }
}