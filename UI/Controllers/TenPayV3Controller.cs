///////////////////////////////////////////////////////////
//  时  间:2018年3月9日19:05:26
//  作  者: Leeseett
///////////////////////////////////////////////////////////
using IBLL;
using Model;
using Senparc.Weixin.Helpers;
using Senparc.Weixin.MP;
using Senparc.Weixin.MP.TenPayLibV3;
using System;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web.Mvc;
using System.Xml.Linq;
using ZXing;
using ZXing.Common;
using System.Linq;
using Spring.Context.Support;
using Common;
using System.Configuration;
using Senparc.Weixin.MP.AdvancedAPIs.TemplateMessage;

namespace UI.Controllers
{
    /// <summary>
    /// 微信支付等相关操作控制器
    /// </summary>
    /// <seealso cref="System.Web.Mvc.Controller" />
    public class TenPayV3Controller : Controller
    {
        private static TenPayV3Info _tenPayV3Info;
		private IorderService OrderService { get; set; }
		private IuserService UserService { get; set; }
		private IproductService ProductService { get; set; }
		private Iuser_score_recordService User_score_recordService { get; set; }

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

		private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            if (errors == SslPolicyErrors.None)
                return true;
            return false;
        }

        public static TenPayV3Info TenPayV3Info
        {
            get
            {
                if (_tenPayV3Info == null)
                {
                    _tenPayV3Info =
                        TenPayV3InfoCollection.Data[System.Configuration.ConfigurationManager.AppSettings["TenPayV3_MchId"]];
                }
                return _tenPayV3Info;
            }
        }


        #region JsApi支付

        /// <summary>
        /// 订单支付操作
        /// </summary>
        /// <param name="orderNo">加密后的订单号</param>
        /// <returns></returns>
        public ActionResult JsApi(string orderNo)
        {
			int order_id = ChangeId(orderNo);
			if (order_id <= 0)
			{
				return Json(new { result = -1, msg = "订单号错误" }, JsonRequestBehavior.AllowGet);
			}
			var order_item = OrderService.LoadEntities(n => n.id == order_id).FirstOrDefault();
			if (order_item == null)
			{
				return Json(new { result = -1, msg = "未找到该订单" }, JsonRequestBehavior.AllowGet);
			}
			if (order_item.pay_state == (int)Pay_state.已支付)
			{
				return Json(new { result = -1, msg = "该订单已支付过" }, JsonRequestBehavior.AllowGet);
			}

			string body = $"感谢您购买{order_item.product.name}";
			if (order_item.product.name.Length > 20)
			{
				body = $"感谢您购买{order_item.product.name.Substring(0, 17)}";
			}

			int price = (int)(order_item.order_money * 100);
			var timeStamp = TenPayV3Util.GetTimestamp();
			var nonceStr = TenPayV3Util.GetNoncestr();
			var openId = order_item.user.wx_user.gzh_openid;
			TenPayV3UnifiedorderRequestData xmlDataInfo = new TenPayV3UnifiedorderRequestData(TenPayV3Info.AppId, TenPayV3Info.MchId, body, order_item.order_number, price, Request.UserHostAddress, TenPayV3Info.TenPayV3Notify, TenPayV3Type.JSAPI, openId, TenPayV3Info.Key, nonceStr, "WEB", DateTime.Now, null, "");
			var result = TenPayV3.Unifiedorder(xmlDataInfo);//调用统一订单接口
			if (result.return_code.Equals("SUCCESS") && result.result_code.Equals("SUCCESS"))
			{
				//var package = $"prepay_id={result.prepay_id}";
				var package = string.Format("prepay_id={0}", result.prepay_id);
				string paySign = string.Empty;
				paySign = TenPayV3.GetJsPaySign(TenPayV3Info.AppId, timeStamp, nonceStr, package,
				TenPayV3Info.Key);
				PayEntity payEntity = new PayEntity()
				{
					goodsId = order_item.product_id.ToString(),
					category = "0",
					userId = openId,
					appId = TenPayV3Info.AppId,
					timeStamp = timeStamp,
					nonceStr = nonceStr,
					package = package,
					paySign = paySign,
					prepay_id = result.prepay_id,
					orderNo = orderNo//加密后的orderid
				};
				order_item.wx_order_num = result.prepay_id;
				OrderService.EditEntity(order_item);
				return Json(new { result = 0, msg = "下单成功", data = payEntity }, JsonRequestBehavior.AllowGet);
			}

			SaveSyslog("用户微信下单发送错误：" + SerializeHelper.SerializeToString(result), SysLogType.前台日志, "支付系统");

			return Json(new { result = -1, msg = "微信下单时发生错误，请联系客服~！", data = "??" }, JsonRequestBehavior.AllowGet);

		}

        /// <summary>
        /// 支付成功后回调通知
        /// </summary>
        /// <returns></returns>
        public ActionResult PayNotifyUrl()
        {
            ResponseHandler resHandler = new ResponseHandler(null);

            string return_code = resHandler.GetParameter("return_code");
            string return_msg = resHandler.GetParameter("return_msg");

            string res = null;

            resHandler.SetKey(TenPayV3Info.Key);
            //验证请求是否从微信发过来（安全）
            if (resHandler.IsTenpaySign())
            {
                res = "success";
				//正确的订单处理
				string orderno = resHandler.GetParameter("out_trade_no");
				var item = OrderService.LoadEntities(n =>n.order_number==orderno).FirstOrDefault();
				if (item != null)
				{
					PaySuccess(item.id);
				}
				else {
					SaveSyslog($"order_number={orderno}的订单回调产生错误_未找到订单对象", SysLogType.前台日志, "支付系统");
				}
			}
            else
            {
                res = "wrong";

				//错误的订单处理
				SaveSyslog($"用户付款失败", SysLogType.前台日志, "支付系统");
			}

			try
			{
				var fileStream = System.IO.File.OpenWrite(Server.MapPath("~/1.txt"));
				fileStream.Write(Encoding.Default.GetBytes(res), 0, Encoding.Default.GetByteCount(res));
				fileStream.Close();	
			}
			catch (Exception)
			{
			}
			string xml = string.Format(@"<xml>
   <return_code><![CDATA[{0}]]></return_code>
   <return_msg><![CDATA[{1}]]></return_msg>
</xml>", return_code, return_msg);

			return Content(xml, "text/xml");
        }

		/// <summary>
		/// Pays the success.
		/// </summary>
		/// <param name="id">The identifier.</param>
		private void PaySuccess(int id) {
			var order_item = OrderService.LoadEntities(n => n.id == id).FirstOrDefault();
			if (order_item != null)
			{
				order_item.pay_state = (int)Pay_state.已支付;
				order_item.pay_time = DateTime.Now;
				order_item.order_state = (int)Order_state.已完成;
				order_item.pay_account = "微信账号支付";
				if (OrderService.EditEntity(order_item))
				{
					var user = UserService.LoadEntities(n => n.id == order_item.user_id).FirstOrDefault();
					if (user != null)
					{
						for (int i = 0; i < order_item.count; i++)
						{
							var up = new user_product();
							up.order_id = id;
							up.product_id = order_item.product_id;
							up.user_id = order_item.user_id;
							up.add_time = DateTime.Now;
							up.end_time = DateTime.Now.AddDays(order_item.product.period_time);
							up.state = (int)Product_state.运行中;
							user.user_product.Add(up);
						}

						user.total_product_count += order_item.count;
						user.total_pay += order_item.order_money;
						user.isbuy = (int)Isbuy.已购买;

						if (UserService.EditEntity(user))
						{
							if (user.pid != 0)
							{
								if (AddUserScore(user.pid, order_item))
								{
									EditProductCount(order_item.product_id, order_item.count);
								}
								else
								{
									SaveSyslog($"order_id={id}的订单回调_修改用户数据产生错误", SysLogType.前台日志, "支付系统");
								}
							}
						}
						else
						{
							SaveSyslog($"order_id={id}的订单回调_修改用户数据产生错误", SysLogType.前台日志, "支付系统");
						}

					}
					else
					{
						SaveSyslog($"order_id={id}的订单回调产生错误,未找到用户", SysLogType.前台日志, "支付系统");
					}

				}
				else
				{
					SaveSyslog($"order_id={id}的订单回调_修改订单时产生错误", SysLogType.前台日志, "支付系统");
				}

			}
			else {
				SaveSyslog($"order_id={id}的订单回调产生错误,未找到订单", SysLogType.前台日志, "支付系统");
			}
			
		}

		/// <summary>
		/// 修改产品个数
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <param name="count">The count.</param>
		/// <returns></returns>
		private bool EditProductCount(int id, int count) {
			var item = ProductService.LoadEntities(n => n.id == id).FirstOrDefault();
			if (item!=null)
			{
				item.product_sold_count += count;
				if (ProductService.EditEntity(item))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// 给上级添加积分记录
		/// </summary>
		/// <returns></returns>
		private bool AddUserScore(int pid,order order) {
			var first = UserService.LoadEntities(n => n.id == pid).FirstOrDefault();
			var res = new List<user_score_record>();
			var useList = new List<user>();
			if (first!=null)
			{
				var f = new user_score_record();
				f.add_time = DateTime.Now;
				f.user_id = first.id;
				f.order_id = order.id;
				f.remark = ((int)Score_source_remark.一级用户购买赠送).ToString();
				f.source_id = order.user_id;
				f.score =order.product.first_level_referral_bonuses*order.count;
				f.source_name = $"一级用户{order.user.name}在{DateTime.Now.ToString()}购买{order.product.name}返回收益";
				f.type = (int)Score_type.收益;
				f.id = Guid.NewGuid();
				f.state = (int)User_score_record_state.已完成;
				res.Add(f);

				first.total_score += f.score;
				first.usable_score += f.score;
				useList.Add(first);

				var second = first.pid != 0 ? UserService.LoadEntities(n => n.id == first.pid).FirstOrDefault() : null;
				if (second!=null)
				{
					var s = new user_score_record();
					s.add_time = DateTime.Now;
					s.user_id = second.id;
					s.order_id = order.id;
					s.remark = ((int)Score_source_remark.二级用户购买赠送).ToString();
					s.source_id = order.user_id;
					s.score = order.product.second_level_referral_bonuses*order.count;
					s.source_name = $"二级用户{order.user.name}在{DateTime.Now.ToString()}购买{order.product.name}返回收益";
					s.type = (int)Score_type.收益;
					s.id = Guid.NewGuid();
					s.state = (int)User_score_record_state.已完成;
					res.Add(s);

					second.total_score += s.score;
					second.usable_score += s.score;
					useList.Add(second);
				}
				User_score_recordService.AddEnties(res);
				UserService.EditEntities(useList);
				return true;
			}
			return false;
		}

		/// <summary>
		/// 发送微信模板消息
		/// </summary>
		/// <param name="order"></param>
		public void SendTemplateMsg(order order) {
			//var taskTemplateId = "-4bevOcQOuncVMTk8qQ5sScwdfkDG8X3JGOtGPyVwp4";
			//var taskInformData = new
			//{
			//	first = new TemplateDataItem($"您众筹的项目已成功"),
			//	keyword1 = new TemplateDataItem($"{}"),
			//	keyword2 = new TemplateDataItem($"{}"),
			//	keyword3 = new TemplateDataItem($"{}"),
			//	remark = new TemplateDataItem($"感谢您的信任与支持")
			//};
			//try
			//{
				
			//	//给用户发送模板消息
			//	Senparc.Weixin.MP.AdvancedAPIs.TemplateApi.SendTemplateMessage(AppId, rmr.User_Activity_Role.User.wx_user.FirstOrDefault().gzhopenid,
			//		taskTemplateId, $"http://{DomainUrl}/Immortal/html/project/project_detail.html?uarid={rmr.uar_id}&pid={rmr.User_Activity_Role.user_id}", taskInformData);
			//}
			//catch (Exception ex)
			//{
			//	SaveSyslog("给用户发送模板信息失败" + ex.Message, SysLogType.前台日志, "admin");
			//}
		}

		#endregion

		#region 订单及退款

		/// <summary>
		/// 订单查询
		/// </summary>
		/// <returns></returns>
		public ActionResult OrderQuery()
        {
            string nonceStr = TenPayV3Util.GetNoncestr();
            RequestHandler packageReqHandler = new RequestHandler(null);

            //设置package订单参数
            packageReqHandler.SetParameter("appid", TenPayV3Info.AppId);		  //公众账号ID
            packageReqHandler.SetParameter("mch_id", TenPayV3Info.MchId);		  //商户号
            packageReqHandler.SetParameter("transaction_id", "");       //填入微信订单号 
            packageReqHandler.SetParameter("out_trade_no", "");         //填入商家订单号
            packageReqHandler.SetParameter("nonce_str", nonceStr);             //随机字符串
            string sign = packageReqHandler.CreateMd5Sign("key", TenPayV3Info.Key);
            packageReqHandler.SetParameter("sign", sign);	                    //签名

            string data = packageReqHandler.ParseXML();

            var result = TenPayV3.OrderQuery(data);
            var res = XDocument.Parse(result);
            string openid = res.Element("xml").Element("sign").Value;

            return Content(openid);
        }

        /// <summary>
        /// 关闭订单接口
        /// </summary>
        /// <returns></returns>
        public ActionResult CloseOrder()
        {
            string nonceStr = TenPayV3Util.GetNoncestr();
            RequestHandler packageReqHandler = new RequestHandler(null);

            //设置package订单参数
            packageReqHandler.SetParameter("appid", TenPayV3Info.AppId);		  //公众账号ID
            packageReqHandler.SetParameter("mch_id", TenPayV3Info.MchId);		  //商户号
            packageReqHandler.SetParameter("out_trade_no", "");                 //填入商家订单号
            packageReqHandler.SetParameter("nonce_str", nonceStr);              //随机字符串
            string sign = packageReqHandler.CreateMd5Sign("key", TenPayV3Info.Key);
            packageReqHandler.SetParameter("sign", sign);	                    //签名

            string data = packageReqHandler.ParseXML();

            var result = TenPayV3.CloseOrder(data);
            var res = XDocument.Parse(result);
            string openid = res.Element("xml").Element("openid").Value;

            return Content(openid);
        }

        /// <summary>
        /// 退款申请接口
        /// </summary>
        /// <returns></returns>
        public ActionResult Refund()
        {
            string nonceStr = TenPayV3Util.GetNoncestr();
            RequestHandler packageReqHandler = new RequestHandler(null);

            //设置package订单参数
            packageReqHandler.SetParameter("appid", TenPayV3Info.AppId);		  //公众账号ID
            packageReqHandler.SetParameter("mch_id", TenPayV3Info.MchId);		  //商户号
            packageReqHandler.SetParameter("out_trade_no", "");                 //填入商家订单号
            packageReqHandler.SetParameter("out_refund_no", "");                //填入退款订单号
            packageReqHandler.SetParameter("total_fee", "");               //填入总金额
            packageReqHandler.SetParameter("refund_fee", "");               //填入退款金额
            packageReqHandler.SetParameter("op_user_id", TenPayV3Info.MchId);   //操作员Id，默认就是商户号
            packageReqHandler.SetParameter("nonce_str", nonceStr);              //随机字符串
            string sign = packageReqHandler.CreateMd5Sign("key", TenPayV3Info.Key);
            packageReqHandler.SetParameter("sign", sign);	                    //签名
            //退款需要post的数据
            string data = packageReqHandler.ParseXML();

            //退款接口地址
            string url = "https://api.mch.weixin.qq.com/secapi/pay/refund";
            //本地或者服务器的证书位置（证书在微信支付申请成功发来的通知邮件中）
            string cert = AppDomain.CurrentDomain.BaseDirectory + "/bin/apiclient_cert.p12";
			//私钥（在安装证书时设置）
			string password = "";
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
            //调用证书
            X509Certificate2 cer = new X509Certificate2(cert, password, X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.MachineKeySet);

            #region 发起post请求
            HttpWebRequest webrequest = (HttpWebRequest)HttpWebRequest.Create(url);
            webrequest.ClientCertificates.Add(cer);
            webrequest.Method = "post";

            byte[] postdatabyte = Encoding.UTF8.GetBytes(data);
            webrequest.ContentLength = postdatabyte.Length;
            Stream stream;
            stream = webrequest.GetRequestStream();
            stream.Write(postdatabyte, 0, postdatabyte.Length);
            stream.Close();

            HttpWebResponse httpWebResponse = (HttpWebResponse)webrequest.GetResponse();
            StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream());
            string responseContent = streamReader.ReadToEnd();
            #endregion

            var res = XDocument.Parse(responseContent);
            string openid = res.Element("xml").Element("out_refund_no").Value;

            return Content(openid);
        }
        #endregion

        #region 红包

        /// <summary>
        /// 目前支持向指定微信用户的openid发放指定金额红包
        /// 注意total_amount、min_value、max_value值相同
        /// total_num=1固定
        /// 单个红包金额介于[1.00元，200.00元]之间
        /// </summary>
        /// <returns></returns>
        public ActionResult SendRedPack()
        {
            string nonceStr;//随机字符串
            string paySign;//签名
            var sendNormalRedPackResult = RedPackApi.SendNormalRedPack(
                TenPayV3Info.AppId, TenPayV3Info.MchId, TenPayV3Info.Key,
                @"F:\apiclient_cert.p12",     //证书物理地址
                "接受收红包的用户的openId",   //接受收红包的用户的openId
                "红包发送者名称",             //红包发送者名称
                Request.UserHostAddress,      //IP
                100,                          //付款金额，单位分
                "红包祝福语",                 //红包祝福语
                "活动名称",                   //活动名称
                "备注信息",                   //备注信息
                out nonceStr,
                out paySign,
                null,                         //场景id（非必填）
                null,                         //活动信息（非必填）
                null                          //资金授权商户号，服务商替特约商户发放时使用（非必填）
                );

            SerializerHelper serializerHelper = new SerializerHelper();
            return Content(serializerHelper.GetJsonString(sendNormalRedPackResult));
        }
        #endregion

        #region 裂变红包

        /// <summary>
        /// 目前支持向指定微信用户的openid发放指定金额红包
        /// 注意total_amount、min_value、max_value值相同
        /// total_num=1固定
        /// 单个红包金额介于[1.00元，200.00元]之间
        /// </summary>
        /// <returns></returns>
        public ActionResult SendGroupRedPack()
        {
            string mchbillno = DateTime.Now.ToString("HHmmss") + TenPayV3Util.BuildRandomStr(28);

            string nonceStr = TenPayV3Util.GetNoncestr();
            RequestHandler packageReqHandler = new RequestHandler(null);

            packageReqHandler.SetParameter("nonce_str", nonceStr);              //随机字符串
            packageReqHandler.SetParameter("wxappid", TenPayV3Info.AppId);		  //公众账号ID
            packageReqHandler.SetParameter("mch_id", TenPayV3Info.MchId);		  //商户号
            packageReqHandler.SetParameter("mch_billno", mchbillno);                 //填入商家订单号
            packageReqHandler.SetParameter("send_name", "商户名称");                 //红包发送者名称
            packageReqHandler.SetParameter("re_openid", "接受收红包的用户的openId");                 //接受收红包的用户的openId
            packageReqHandler.SetParameter("total_amount", "300");                //付款金额，单位分
            packageReqHandler.SetParameter("total_num", "3");               //红包发放总人数  必须介于(包括)3到20之间
            packageReqHandler.SetParameter("wishing", "红包祝福语");               //红包祝福语
            packageReqHandler.SetParameter("amt_type", "ALL_RAND");               //红包金额设置方式ALL_RAND—全部随机,商户指定总金额和红包发放总人数，由微信支付随机计算出各红包金额
            //packageReqHandler.SetParameter("amt_list", "各红包具体金额");               //各红包具体金额，自定义金额时必须设置，单位分  示例值"200|100|100"
            packageReqHandler.SetParameter("act_name", "活动名称");   //活动名称
            packageReqHandler.SetParameter("remark", "备注信息");   //备注信息
            string sign = packageReqHandler.CreateMd5Sign("key", TenPayV3Info.Key);
            packageReqHandler.SetParameter("sign", sign);	                    //签名
            //发红包需要post的数据
            string data = packageReqHandler.ParseXML();

            //发红包接口地址
            string url = "https://api.mch.weixin.qq.com/mmpaymkttransfers/sendgroupredpack";
            //本地或者服务器的证书位置（证书在微信支付申请成功发来的通知邮件中）
            string cert = @"F:\apiclient_cert.p12";
            //私钥（在安装证书时设置）
            string password = "";
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
            //调用证书
            X509Certificate2 cer = new X509Certificate2(cert, password, X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.MachineKeySet);

            #region 发起post请求
            HttpWebRequest webrequest = (HttpWebRequest)HttpWebRequest.Create(url);
            webrequest.ClientCertificates.Add(cer);
            webrequest.Method = "post";

            byte[] postdatabyte = Encoding.UTF8.GetBytes(data);
            webrequest.ContentLength = postdatabyte.Length;
            Stream stream;
            stream = webrequest.GetRequestStream();
            stream.Write(postdatabyte, 0, postdatabyte.Length);
            stream.Close();

            HttpWebResponse httpWebResponse = (HttpWebResponse)webrequest.GetResponse();
            StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream());
            string responseContent = streamReader.ReadToEnd();
            #endregion

            return Content(responseContent);
        }
        #endregion

        #region 红包查询接口

        public ActionResult GetHBInfo(string mchbillno)
        {
            string nonceStr = TenPayV3Util.GetNoncestr();
            RequestHandler packageReqHandler = new RequestHandler(null);

            packageReqHandler.SetParameter("nonce_str", nonceStr);              //随机字符串
            packageReqHandler.SetParameter("appid", TenPayV3Info.AppId);		  //公众账号ID
            packageReqHandler.SetParameter("mch_id", TenPayV3Info.MchId);		  //商户号
            packageReqHandler.SetParameter("mch_billno", mchbillno);                 //填入商家订单号
            packageReqHandler.SetParameter("bill_type", "MCHT");                 //红包发送者名称
            string sign = packageReqHandler.CreateMd5Sign("key", TenPayV3Info.Key);
            packageReqHandler.SetParameter("sign", sign);	                    //签名
            //发红包需要post的数据
            string data = packageReqHandler.ParseXML();

            //发红包接口地址
            string url = "https://api.mch.weixin.qq.com/mmpaymkttransfers/gethbinfo";
            //本地或者服务器的证书位置（证书在微信支付申请成功发来的通知邮件中）
            string cert = @"F:\apiclient_cert.p12";
            //私钥（在安装证书时设置）
            string password = "";
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
            //调用证书
            X509Certificate2 cer = new X509Certificate2(cert, password, X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.MachineKeySet);

            #region 发起post请求
            HttpWebRequest webrequest = (HttpWebRequest)HttpWebRequest.Create(url);
            webrequest.ClientCertificates.Add(cer);
            webrequest.Method = "post";

            byte[] postdatabyte = Encoding.UTF8.GetBytes(data);
            webrequest.ContentLength = postdatabyte.Length;
            Stream stream;
            stream = webrequest.GetRequestStream();
            stream.Write(postdatabyte, 0, postdatabyte.Length);
            stream.Close();

            HttpWebResponse httpWebResponse = (HttpWebResponse)webrequest.GetResponse();
            StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream());
            string responseContent = streamReader.ReadToEnd();
            #endregion

            return Content(responseContent);
        }

        #endregion



        #region 查询订单
        /// <summary>
        /// 查询订单
        /// </summary>
        /// <param name="orderno">订单号</param>
        /// <param name="transaction_id">微信统一下单单号</param>
        /// <returns></returns>
        public bool OrderQuery(string orderno, ref string transaction_id)
        {
            string nonceStr = TenPayV3Util.GetNoncestr();
            RequestHandler packageReqHandler = new RequestHandler(null);

            //设置package订单参数 
            packageReqHandler.SetParameter("appid", TenPayV3Info.AppId); //公众账号ID
            packageReqHandler.SetParameter("mch_id", TenPayV3Info.MchId); //商户号
            packageReqHandler.SetParameter("transaction_id", ""); //填入微信订单号    二选一
            packageReqHandler.SetParameter("out_trade_no", orderno); //填入商家订单号      二选一
            packageReqHandler.SetParameter("nonce_str", nonceStr); //随机字符串
            string sign = packageReqHandler.CreateMd5Sign("key", TenPayV3Info.Key);
            packageReqHandler.SetParameter("sign", sign); //签名

            string data = packageReqHandler.ParseXML();

            var result = TenPayV3.OrderQuery(data);
            var res = XDocument.Parse(result);
            string return_code = res.Element("xml").Element("return_code").Value;
            if (return_code.ToUpper() == "SUCCESS")
            {
                string result_code = res.Element("xml").Element("result_code").Value;
                if (result_code.ToUpper() == "SUCCESS")
                {
                    string trade_state = res.Element("xml").Element("trade_state").Value;//交易状态   https://pay.weixin.qq.com/wiki/doc/api/jsapi.php?chapter=9_2
                    if (trade_state.ToUpper() == "SUCCESS")
                    {
                        transaction_id = res.Element("xml").Element("transaction_id").Value;
                        return true;
                    }
                }
            }

            return false;
        }

		#endregion


		#region 援助
		/// <summary>
		/// 转换  加密后string 类型到int
		/// </summary>
		/// <param name="bid">The bid.</param>
		/// <returns>转换后的id</returns>
		protected int ChangeId(string bid)
		{
			var result = 0;
			try
			{
				bid = Common.EncryptHelper.Decrypt(bid);
				if (int.TryParse(bid, out var id))
				{
					result = id;
				}
			}
			catch (Exception)
			{
				result = 0;
			}
			return result;
		}

		/// <summary>  
		/// 获取时间戳  
		/// </summary>  
		/// <returns></returns>  
		private string GetTimeStamp()
		{
			TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
			return Convert.ToInt64(ts.TotalSeconds).ToString();
		}


		/// <summary>
		/// 保存日志
		/// </summary>
		/// <param name="content">日志内容.</param>
		/// <param name="logType">SysLogType 枚举</param>
		/// <param name="userName">记录人名称</param>
		public void SaveSyslog(string content, SysLogType logType, string userName)
		{
			Isystem_logService system_logService = ContextRegistry.GetContext().GetObject("system_logService") as Isystem_logService;
			system_log log = new system_log
			{
				id = Guid.NewGuid(),
				add_name = userName,
				content = content,
				add_time = DateTime.Now,
				ip_address = Request.UserHostAddress,
				page_url = Request.Url.ToString(),
				type = (int)logType,
			};
			system_logService.AddEntity(log);

		} 
		#endregion

	}

    public class PayEntity
    {
        /// <summary>
        /// 商品ID
        /// </summary>
        public string goodsId { get; set; }
        /// <summary>
        /// 下单用户ID
        /// </summary>
        public string userId { get; set; }
        public string appId { get; set; }
        public string timeStamp { get; set; }
        public string nonceStr { get; set; }
        public string package { get; set; }
        public string paySign { get; set; }
        /// <summary>
        /// 商品类别 0 课程
        /// </summary>
        public string category { get; set; }
        /// <summary>
        /// 微信统一下单订单号
        /// </summary>
        public string prepay_id { get; set; }
        /// <summary>
        /// 商户订单号
        /// </summary>
        public string orderNo { get; set; }
    }
}
