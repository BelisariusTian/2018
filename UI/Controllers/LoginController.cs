///////////////////////////////////////////////////////////
//  时  间:2018年3月9日19:05:26
//  作  者: Leeseett
///////////////////////////////////////////////////////////
using Common;
using Common.Cache;
using GeetestSDK;
using IBLL;
using Model;
using System;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
namespace UI.Controllers
{
    /// <summary>
    /// 登录控制器
    /// </summary>
    /// <seealso cref="UI.Controllers.WebController" />
    public class LoginController : WebController
    {
        IadministratorService AdministratorService { get; set; }
        private string publicKey = ConfigurationManager.AppSettings["GeeTestPublicKey"];
        private string privateKey = ConfigurationManager.AppSettings["GeeTestPrivateKey"];

        public ActionResult Index() {
            return Redirect("/admin/login.html");
        }

        #region 验证验证码
        /// <summary>
        /// 验证验证码
        /// </summary>
        /// <returns></returns>
        public string Verification()
        {
            GeetestLib geetest = new GeetestLib(publicKey, privateKey);
            Byte gt_server_status_code = (Byte)Session[GeetestLib.gtServerStatusSessionKey];
            String userID = (String)Session["userID"];
            int result = 0;
            String challenge = Request.Form.Get(GeetestLib.fnGeetestChallenge);
            String validate = Request.Form.Get(GeetestLib.fnGeetestValidate);
            String seccode = Request.Form.Get(GeetestLib.fnGeetestSeccode);
            if (gt_server_status_code == 1) result = geetest.enhencedValidateRequest(challenge, validate, seccode, userID);
            else result = geetest.failbackValidateRequest(challenge, validate, seccode);
            if (result == 1) return "ok";
            else return "sb";
        }
        #endregion

        #region 用户登录
        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="password">用户密码</param>
        /// <returns></returns>
        public ActionResult UserLogin(string userName, string password)
        {
            password = Common.EncryptHelper.Encrypt(password);
            #region 处理验证
            string yzjg = Verification();
            if (string.IsNullOrEmpty(yzjg) || yzjg == "sb")
            {
                return Json(SysEnum.失败, "请点击按钮进行验证");
            }
            #endregion

            #region 处理用户名密码
            var administrator = AdministratorService.LoadEntities(u => u.login_account == userName && u.login_pwd == password).FirstOrDefault();
            if (administrator == null)
            {
                SaveSyslog("用户名或密码错误！", SysLogType.后台日志, userName);
                return Json(SysEnum.用户名或密码错误, "用户名或密码错误");
            }
			#endregion
			administrator.last_login_time = DateTime.Now;
			administrator.last_login_IP_address = Request.UserHostAddress;
			AdministratorService.EditEntity(administrator);

            //添加到缓存里
            CacheHelper.AddCache(administrator.id.ToString(), administrator, DateTime.Now.AddMinutes(60));
            
            SaveSyslog("登陆后台管理", SysLogType.后台日志, userName);

            dynamic data = new { token = Common.EncryptHelper.Encrypt(string.Format("{0}|{1}", administrator.id.ToString(), Request.UserHostAddress)) };
            return Json(SysEnum.成功, data, "登录成功");
        }
        #endregion
        
        #region 解锁
        ///  <summary>
        /// 解锁
        ///  </summary>
        /// <param name="name"></param>
        /// <param name="pwd"></param>
        ///  <returns></returns>
        [HttpPost]
        public ActionResult UnLock(string name, string pwd)
        {
            pwd = EncryptHelper.Encrypt(pwd);
            var administrator = AdministratorService.LoadEntities(u => u.login_account == name && u.login_pwd == pwd).FirstOrDefault();

            if (administrator != null)
            {
                //添加到缓存里
                CacheHelper.AddCache(administrator.id.ToString(), SerializeHelper.SerializeToString(administrator), DateTime.Now.AddMinutes(60));
                dynamic data = new { token = Common.EncryptHelper.Encrypt(string.Format("{0}|{1}", administrator.id.ToString(), Request.UserHostAddress)) };
                return Json(SysEnum.成功, data, "pass");
            }
            else
            {
                return Json(SysEnum.用户名或密码错误, "密码错误！会登陆吗？");
            }
        }
        #endregion

        #region 获取滑动验证码
        /// <summary>
        /// 获取滑动验证码
        /// </summary>
        /// <returns></returns>
        public ActionResult GetCaptcha()
        {
            GeetestLib geetest = new GeetestLib(publicKey, privateKey);
            string userID = "test";
            Byte gtServerStatus = geetest.preProcess(userID, "web", Request.UserHostAddress);
            Session[GeetestLib.gtServerStatusSessionKey] = gtServerStatus;
            Session["userID"] = userID;
            return Content(geetest.getResponseStr());
        } 
        #endregion
    }
}
