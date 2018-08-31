using Common;
using Common.Cache;
using IBLL;
using Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace UI.Controllers
{
    public class OrderController : BaseController
	{
		//
		// GET: /Order/

		private IorderService OrderService { get; set; }
		private IuserService UserService { get; set; }
		private IproductService ProductService { get; set; }
		private Iuser_score_recordService User_score_recordService { get; set; }

		#region 获取已完成订单列表
		/// <summary>
		/// 获取订单列表
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="number">The number.</param>
		/// <param name="starttime">The starttime.</param>
		/// <param name="endtime">The endtime.</param>
		/// <param name="pay_state">State of the pay.</param>
		/// <param name="order_state">State of the order.</param>
		/// <param name="page">The page.</param>
		/// <param name="limit">The limit.</param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult GetOrderList(string name, string number, DateTime? starttime, DateTime? endtime, int pay_state = 0, int order_state = 0, int page = 1, int limit = 10)
		{
			var allOrderList = OrderCache("allOrderList", -1);
			if (allOrderList.Any())
			{
				var resultOrderList = pay_state == -1 ? allOrderList : allOrderList.Where(n => n.pay_state == pay_state).ToList();
				resultOrderList = order_state == -1 ? resultOrderList : resultOrderList.Where(n => n.order_state == order_state).ToList();
				if (!string.IsNullOrEmpty(name))
				{
					resultOrderList = resultOrderList.Where(n => n.user.name.Contains(name)).ToList();
				}
				if (!string.IsNullOrEmpty(number))
				{
					resultOrderList = resultOrderList.Where(n => n.order_number.Contains(number)).ToList();
				}
				if (starttime != null)
				{
					var s_time = Convert.ToDateTime(starttime);
					resultOrderList = resultOrderList.Where(n => n.add_time >= s_time).ToList();
				}
				if (endtime != null)
				{
					var e_time = Convert.ToDateTime(endtime);
					resultOrderList = resultOrderList.Where(n => n.add_time <= e_time).ToList();
				}
				var totalCount = resultOrderList.Count();
				resultOrderList = resultOrderList.OrderByDescending(n => n.add_time).Skip((page - 1) * limit).Take(limit).ToList();
				if (resultOrderList.Any())
				{
					var data = resultOrderList.Select(n => new
					{
						n.pay_account,
						n.id,
						pay_state = Enum.GetName(typeof(Pay_state), n.pay_state),
						add_time = n.add_time.ToString(),
						user_name = n.user.name,
						n.order_money,
						n.order_number,
						order_state = Enum.GetName(typeof(Order_state), n.order_state),
						n.admin_remark,
						n.user_remark,
						product_name = n.product.name,
						pay_time = n.pay_time == null ? "" : n.pay_time.ToString(),
						n.count,
					}).ToList();
					return Json(SysEnum.成功, data, "获取订单数据成功", totalCount);
				}
				else
				{
					return Json(SysEnum.失败, "没有任何订单");
				}
			}
			else
			{
				return Json(SysEnum.失败, "没有任何订单");
			}
		}
		#endregion

		#region 修改订单状态
		/// <summary>
		/// 修改订单状态
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <param name="order_state"></param>
		/// <param name="day"></param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult ChangePayState(int id, int order_state,int day=7)
		{
			var item = OrderService.LoadEntities(n => n.id == id).FirstOrDefault();
			if (item != null)
			{
				item.order_state = order_state;
				
				if (OrderService.EditEntity(item))
				{
					CacheHelper.RemoveCache($"orderlist_{day}");
					SaveSyslog($"修改订单(id={id})订单状态成功", SysLogType.后台日志, nowManager.name);
					return Json(SysEnum.成功, "修改订单状态成功");
				}
				return Json(SysEnum.失败, "修改状态失败");
			}
			return Json(SysEnum.失败, "未找到对象");
		}

		/// <summary>
		/// 修改订单备注
		/// </summary>
		/// <param name="id"></param>
		/// <param name="remark"></param>
		/// <returns></returns>
		public ActionResult EditRemark(int id, string remark) {
			var item = OrderService.LoadEntities(n => n.id == id).FirstOrDefault();
			if (item != null)
			{
				item.admin_remark = remark;

				if (OrderService.EditEntity(item))
				{
					SaveSyslog($"修改订单(id={id})订单备注成功", SysLogType.后台日志, nowManager.name);
					return Json(SysEnum.成功, "修改订单备注成功");
				}
				return Json(SysEnum.失败, "修改备注失败");
			}
			return Json(SysEnum.失败, "未找到对象");
		}

		/// <summary>
		///  修改完支付状态后的操作
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <returns></returns>
		public ActionResult AfterChangePayState(int id)
		{
			var order_item = OrderService.LoadEntities(n => n.id == id).FirstOrDefault();
			if (order_item != null)
			{
				if (order_item.pay_state == (int)Pay_state.已支付 && order_item.order_state == (int)Order_state.确认支付)
				{
					order_item.order_state = (int)Order_state.已完成;
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
										return Json(SysEnum.成功, "修改成功");
									}
									else
									{
										SaveSyslog($"order_id={id}的订单回调_修改用户数据产生错误", SysLogType.后台日志, "支付系统");
										return Json(SysEnum.失败, "修改用户数据失败");
									}
								}
								else
								{
									return Json(SysEnum.成功, "修改成功");
								}
							}
							else
							{
								SaveSyslog($"order_id={id}的订单回调_修改用户数据产生错误", SysLogType.后台日志, "支付系统");
								return Json(SysEnum.失败, "修改用户数据失败");
							}

						}
						else
						{
							SaveSyslog($"order_id={id}的订单回调产生错误,未找到用户", SysLogType.后台日志, "支付系统");
							return Json(SysEnum.失败, "订单回调产生错误");
						}

					}
					else {
						SaveSyslog($"order_id={id}的订单回调_修改订单时产生错误", SysLogType.后台日志, "支付系统");
						return Json(SysEnum.失败, "修改订单失败");
					}
				}
			}
			return Json(SysEnum.失败, "未找到对象");
		}


		/// <summary>
		/// 修改产品个数
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <param name="count">The count.</param>
		/// <returns></returns>
		private bool EditProductCount(int id, int count)
		{
			var item = ProductService.LoadEntities(n => n.id == id).FirstOrDefault();
			if (item != null)
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
		private bool AddUserScore(int pid, order order)
		{
			var first = UserService.LoadEntities(n => n.id == pid).FirstOrDefault();
			var res = new List<user_score_record>();
			var useList = new List<user>();
			if (first != null)
			{
				var f = new user_score_record();
				f.add_time = DateTime.Now;
				f.user_id = first.id;
				f.order_id = order.id;
				f.remark = ((int)Score_source_remark.一级用户购买赠送).ToString();
				f.source_id = order.user_id;
				f.score = (int)order.product.first_level_referral_bonuses*order.count;
				f.source_name = $"一级用户{order.user.name}在{DateTime.Now.ToString()}购买{order.product.name}返回收益";
				f.type = (int)Score_type.收益;
				f.id = Guid.NewGuid();
				f.state = (int)User_score_record_state.已完成;
				res.Add(f);

				first.total_score += f.score;
				first.usable_score += f.score;
				useList.Add(first);

				var second = first.pid != 0 ? UserService.LoadEntities(n => n.id == first.pid).FirstOrDefault() : null;
				if (second != null)
				{
					var s = new user_score_record();
					s.add_time = DateTime.Now;
					s.user_id = second.id;
					s.order_id = order.id;
					s.remark = ((int)Score_source_remark.二级用户购买赠送).ToString();
					s.source_id = order.user_id;
					s.score = (int)order.product.second_level_referral_bonuses*order.count;
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
				return true;
			}
			return false;
		}
		#endregion

		#region 获取未处理订单
		/// <summary>
		/// 获取未处理订单
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="order_number">The order number.</param>
		/// <param name="page">The page.</param>
		/// <param name="limit">The limit.</param>
		/// <param name="day">The day.</param>
		/// <returns></returns>
		public ActionResult GetPendingOrderList(string name, string order_number, int page = 1, int limit = 20, int day = 7)
		{
			var orderList = OrderCache($"orderlist_{day}", day);
			//支付状态是已支付，订单状态还是等待支付的 ，表示用户已付钱，等待管理员确认付钱
			var res_orderList = orderList.Where(n => n.order_state == (int)Order_state.等待支付 && n.pay_state == (int)Pay_state.已支付).ToList();
			if (res_orderList.Any())
			{
				res_orderList = !string.IsNullOrEmpty(name) ? res_orderList.Where(n => n.user.name.Contains(name)).ToList() : res_orderList;
				res_orderList = !string.IsNullOrEmpty(order_number) ? res_orderList.Where(n => n.order_number.Contains(order_number)).ToList() : res_orderList;
				var date = day == -1 ? DateTime.MinValue : DateTime.Now.AddDays(-day);
				res_orderList = res_orderList.Where(n => n.add_time >= date).ToList();
				if (res_orderList.Any())
				{
					var data = res_orderList.OrderByDescending(n => n.add_time).Skip((page - 1) * limit).Take(limit).ToList().Select(n => new
					{
						n.pay_account,
						n.id,
						pay_state = Enum.GetName(typeof(Pay_state), n.pay_state),
						add_time = n.add_time.ToString(),
						user_name = n.user.name,
						n.order_money,
						n.order_number,
						n.order_state,
						product_name = n.product.name,
						n.admin_remark,
						pay_time = n.pay_time == null ? "" : n.pay_time.ToString(),
						n.count,
					}).ToList();
					return Json(SysEnum.成功, data, "获取订单数据成功", res_orderList.Count());
				}
				return Json(SysEnum.失败, "没有相关订单");
			}
			return Json(SysEnum.失败, "没有任何待处理订单");
		} 
		#endregion

		#region 订单缓存		
		/// <summary>
		/// Orders the cache.
		/// </summary>
		/// <param name="name">缓存名称</param>
		/// <param name="day">多少天以内的数据</param>
		/// <returns></returns>
		public List<order> OrderCache(string name, int day)
		{
			if (!(CacheHelper.GetCache(name) is List<order> orderList) || orderList.Count == 0)
			{
				var date =day==-1?DateTime.MinValue: DateTime.Now.AddDays(-day);
				orderList = OrderService.LoadEntities(n => n.add_time >= date).ToList();
				if (orderList.Any())
				{
					CacheHelper.AddCache(name, orderList, DateTime.Now.AddMinutes(2));
				}
			}
			return orderList;
		}
		#endregion

		#region 菜单页 订单信息
		/// <summary>
		/// 菜单页 订单信息
		/// </summary>
		/// <returns></returns>
		public ActionResult Menu_Order()
		{
			var orderList = OrderService.LoadEntities(n => true).ToList();
			var data = new
			{
				total_order = orderList.Count,
				total_complete = orderList.Where(n => n.order_state == (int)Order_state.已完成).ToList().Count,
				total_nocomplete = orderList.Where(n => n.order_state == (int)Order_state.取消支付 || n.order_state == (int)Order_state.等待支付 || n.order_state == (int)Order_state.订单过期).ToList().Count,
				total_untreaded = orderList.Where(n => n.pay_state == (int)Pay_state.已支付&&n.order_state==(int)Order_state.等待支付).ToList().Count,
			};
			return Json(SysEnum.成功, data, "获取数据成功");
		}
		#endregion

		#region 导出Excel
		/// <summary>
		/// 导出EXCEL
		/// </summary>
		/// <param name="day"></param>
		/// <param name="pay_state"></param>
		/// <param name="order_state"></param>
		public void ExportExcel(int day,int pay_state,int order_state)
		{
			var order_list = OrderCache($"orderlist_{day}", day);
			if (order_list.Any())
			{
				order_list = order_list.Where(n => n.pay_state == pay_state && n.order_state == order_state).ToList();
				List<dynamic> list = new List<dynamic>();
				if (order_list.Any())
				{
					foreach (var item in order_list)
					{
						var one = new
						{
							item.id,
							product_name=item.product.name,
							pay_state = Enum.GetName(typeof(Pay_state),item.pay_state),
							order_state = Enum.GetName(typeof(Order_state), item.order_state),
							user_name=item.user.name,
							item.order_number,
							item.wx_order_num,
							item.user_remark,
							item.add_time,
							item.pay_time,
							item.refund_number,
							item.pay_account,
							item.count,
							item.product.price,
							item.order_money
						};
						list.Add(one);
					}
					if (list.Any())
					{
						DataTable dt = ListToDataTable.List2DataTable<dynamic>(list);
						if (dt != null && dt.Rows.Count > 0)
						{
							dt.Columns["id"].ColumnName = "id";
							dt.Columns["product_name"].ColumnName = "产品名称";
							dt.Columns["user_name"].ColumnName = "用户名称";
							dt.Columns["count"].ColumnName = "产品个数";
							dt.Columns["price"].ColumnName = "产品价格";
							dt.Columns["order_money"].ColumnName = "订单金额";
							dt.Columns["pay_state"].ColumnName = "支付状态";
							dt.Columns["order_state"].ColumnName = "订单状态";
							dt.Columns["order_number"].ColumnName = "订单号";
							dt.Columns["wx_order_num"].ColumnName = "微信订单号";
							dt.Columns["add_time"].ColumnName = "下单时间";
							dt.Columns["pay_time"].ColumnName = "支付时间";
							dt.Columns["pay_account"].ColumnName = "付款账号";
							dt.Columns["refund_number"].ColumnName = "退单号";
						}
						bool hh = OfficeHelper.ExportExcelWithAspose(dt, $"{day}天内订单信息{DateTime.Now.ToString("yyyy年MM月dd日HH时mm分ss秒")}", "", true);
					}
				}
			}
		} 
		#endregion
	}
}
