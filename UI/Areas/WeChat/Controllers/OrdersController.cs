using Common;
using IBLL;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UI.Controllers;

namespace UI.Areas.WeChat.Controllers
{
	//订单处理
    public class OrdersController : WeiXinBaseController
	{
		//
		// GET: /WeChat/Orders/
		private IorderService OrderService { get; set; }
		private Iconfig_ruleService Config_ruleService { get; set; }

		#region 生成订单
		/// <summary>
		/// 生成订单
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		public ActionResult AddOrder()
		{
			var data = SerializeHelper.SerializeToObject<dynamic>(Request["data"]);
			var o = new order();
			o.product_id = data.product_id;//产品id
			o.count = data.count;//购买数量
			o.order_money = data.order_money; //订单金额
			o.user_remark = data.user_remark??string.Empty;// 用户留言
			o.pay_account = data.pay_account??UserInfo.nickname; //支付账号
			o.user_id = UserInfo.id;
			o.add_time = DateTime.Now;
			o.pay_state = (int)Pay_state.未支付;
			o.order_state = (int)Order_state.等待支付;
			o.order_number = $"T{Guid.NewGuid().ToString().Substring(0, 10)}{DateTime.Now.ToString("yyyyMMdd")}";
			return SaveOrder(o);
		}

		private JsonResult SaveOrder(order order)
		{
			var res = OrderService.AddEntity(order);
			if (res.id > 0)
			{
				return Json(SysEnum.成功, new { res.order_number,order_id=Common.EncryptHelper.Encrypt(res.id.ToString()) }, "添加成功", 1);
			}
			return Json(SysEnum.失败, "添加失败");
		}

		/// <summary>
		/// Users the affirm pay.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <param name="pay_address">The pay address.</param>
		/// <returns></returns>
		public ActionResult UserAffirmPay(int id,string pay_address) {
			var item = OrderService.LoadEntities(n => n.id == id).FirstOrDefault();
			if (item!=null)
			{
				if (item.pay_state==(int)Pay_state.已支付)
				{
					return Json(SysEnum.失败, "您已确认过付款了");
				}
				item.pay_state = (int)Pay_state.已支付;
				item.pay_time = DateTime.Now;
				item.pay_account = pay_address;
				if (OrderService.EditEntity(item))
				{
					return Json(SysEnum.成功, "确认支付成功，等待客服处理");
				}
				return Json(SysEnum.失败, "网络异常");
			}
			return Json(SysEnum.失败, "未找到订单对象");
		}

		//删除订单
		/// <summary>
		/// 订单id
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public ActionResult DelOrder(int id) {
			var item = OrderService.LoadEntities(n => n.id == id).FirstOrDefault();
			if (item!=null)
			{
				if (OrderService.DeleteEntity(id))
				{

				}
			}
			return Json(SysEnum.失败, "未找到对象");
		}
		#endregion

		#region 订单详情
		/// <summary>
		/// 订单详情
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public ActionResult OrderDetail(string id)
		{
            int order_id = ChangeId(id);
            var item = OrderService.LoadEntities(n => n.id == order_id).FirstOrDefault();
			var ruleList = Config_ruleService.LoadEntities(n => n.state == (int)State.可用).ToList();
			if (item != null)
			{
				var data = new
				{
					item.id,
					item.order_money,
					product_name =item.product.name,
					item.product.product_img,
					item.product.product_introduct,
					item.product.Calculate_the_force,
					item.product.period_time,
					item.product.total_score,
					item.product.price,
					item.order_state,
					item.pay_account,
					item.pay_state,
					pay_time = item.pay_time == null ? string.Empty : item.pay_time.ToString(),
					add_time = item.add_time.ToString(),
					add_time2 = (item.add_time.AddMinutes(30)-DateTime.Now).TotalSeconds,
					item.count,
					item.order_number,
					item.refund_number,
					item.wx_order_num,
					item.user_remark,
					bank_account = ruleList.FirstOrDefault(n => n.name == "银行卡号" || n.id == 4) == null ? string.Empty : ruleList.FirstOrDefault(n => n.name == "银行卡号" || n.id == 4).value,
					bank_user = ruleList.FirstOrDefault(n => n.name == "开户人" || n.id == 5) == null ? string.Empty : ruleList.FirstOrDefault(n => n.name == "开户人" || n.id == 5).value,
					bank_bankaddress = ruleList.FirstOrDefault(n => n.name == "开户行" || n.id == 6) == null ? string.Empty : ruleList.FirstOrDefault(n => n.name == "开户行" || n.id == 6).value,
				};
				return Json(SysEnum.成功, data, "获取数据成功");
			}
			return Json(SysEnum.失败, "未找到该订单");
		}

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
		#endregion

		#region 订单到期
		/// <summary>
		/// 订单到期
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public ActionResult ChangeOrderStateWhenTimeout(int id)
		{
			var item = OrderService.LoadEntities(n => n.id == id).FirstOrDefault();
			if (item != null)
			{
				item.order_state = (int)Order_state.订单过期;
				if (OrderService.EditEntity(item))
				{
					return Json(SysEnum.成功, "修改订单状态成功");
				}
				return Json(SysEnum.失败, "修改订单状态失败");
			}
			return Json(SysEnum.失败, "未找到对象");
		} 
		#endregion


	}
}
