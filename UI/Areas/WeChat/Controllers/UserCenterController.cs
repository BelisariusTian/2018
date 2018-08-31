using Common;
using Common.Cache;
using IBLL;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace UI.Areas.WeChat.Controllers
{
	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="UI.Areas.WeChat.Controllers.WeiXinBaseController" />
	public class UserCenterController : WeiXinBaseController
	{
		//
		// GET: /WeChat/UserCenter/

		private Iwx_userService Wx_userService { get; set; }
		private Iuser_addressService User_addressService { get; set; }
		private Iuser_msg_recordService User_msg_recordService { get; set; }
		private Isystem_msg_recordService System_msg_recordService { get; set; }

		#region 用户地址管理	

		#region 获取列表
		/// <summary>
		/// 获取用户地址列表
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public ActionResult GetUserAddressList()
		{
			var list = User_addressService.LoadEntities(n => n.user_id == UserInfo.id && n.del_flag == (int)Del_flag.正常).ToList();
			if (list.Any())
			{
				var data = list.Select(n => new
				{
					n.id,
					n.name,
					n.address,
					add_time = n.add_time.ToString(),
					n.state,
				}).ToList();
				return Json(SysEnum.成功, data, "获取数据成功", list.Count);
			}
			return Json(SysEnum.失败, "暂无数据");
		}
		#endregion

		#region 添加

		/// <summary>
		/// 添加地址
		/// </summary>
		/// <param name="address">The address.</param>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult AddUserAddress(string address, string name)
		{
			var ua = new user_address();
			ua.address = address;
			ua.user_id = UserInfo.id;
			ua.name = name ?? UserInfo.nickname;
			ua.add_time = DateTime.Now;
			ua.del_flag = (int)Del_flag.正常;
			ua.state = (int)State.可用;
			var res = User_addressService.AddEntity(ua);
			if (res.id > 0)
			{
				return Json(SysEnum.成功, "添加地址成功");
			}
			return Json(SysEnum.失败, "添加地址失败");
		}
		#endregion

		#region 修改

		/// <summary>
		/// Edits the user address.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult EditUserAddress(int? id)
		{
			if (id!=null)
			{
				var item = User_addressService.LoadEntities(n => n.id == id && n.del_flag == (int)Del_flag.正常).FirstOrDefault();
				if (item != null)
				{
					var data = new
					{
						item.id,
						item.name,
						item.address,
						item.state
					};
					return Json(SysEnum.成功, data, "获取地址成功", 1);
				}
				return Json(SysEnum.失败, "未找到对象");
			}
			return Json(SysEnum.失败, "参数传递不正确");
		}
			

		/// <summary>
		/// Edits the user address.
		/// </summary>
		/// <param name="address">The address.</param>
		/// <param name="name">The name.</param>
		/// <param name="id">The identifier.</param>
		/// <param name="state">The state.</param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult EditUserAddress(string address, string name, int id, int state = (int)State.可用)
		{
			var item = User_addressService.LoadEntities(n => n.id == id && n.del_flag == (int)Del_flag.正常).FirstOrDefault();
			if (item != null)
			{
				item.address = address;
				item.name = name;
				item.state = state;
				if (User_addressService.EditEntity(item))
				{
					return Json(SysEnum.成功, "修改成功");
				}
				return Json(SysEnum.失败, "修改失败");
			}
			return Json(SysEnum.失败, "未找到对象");
		}

		#endregion

		#region 删除
		/// <summary>
		/// Deletes the user address.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult DelUserAddress(int id)
		{
			var item = User_addressService.LoadEntities(n => n.id == id && n.del_flag == (int)Del_flag.正常).FirstOrDefault();
			if (item != null)
			{
				item.del_flag = (int)Del_flag.逻辑删除;
				if (User_addressService.EditEntity(item))
				{
					return Json(SysEnum.成功, "删除地址成功");
				}
				return Json(SysEnum.失败, "删除地址失败");
			}
			return Json(SysEnum.失败, "未找到对象");
		}
		#endregion
		#endregion

		#region 系统消息		
		/// <summary>
		/// 系统QA
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public ActionResult GetSystemQAMsg()
		{
			var list = SystemMsgCache("sysmsg_list").Where(n => n.type == (int)Msg_type.QA).OrderByDescending(n => n.add_time).ToList();
			if (list.Any())
			{
				var data = list.Select(n => new
				{
					add_time = n.add_time.ToString(),
					n.title,
					n.content,
					n.id,
					n.sort,
					n.administrator.name,
					n.read_count,
				}).ToList();
				return Json(SysEnum.成功, data, "获取数据成功", list.Count);
			}
			return Json(SysEnum.失败, "暂无数据");
		}

		/// <summary>
		/// 系统公告
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public ActionResult GetSystemNoticeMsg()
		{
			var list = SystemMsgCache("sysmsg_list").Where(n => n.type == (int)Msg_type.公告).OrderByDescending(n => n.add_time).ToList();
			if (list.Any())
			{
				var data = list.Select(n => new
				{
					add_time = n.add_time.ToString(),
					n.title,
					n.content,
					n.id,
					n.sort,
					n.administrator.name,
					n.read_count,
				}).ToList();
				return Json(SysEnum.成功, data, "获取数据成功", list.Count);
			}
			return Json(SysEnum.失败, "暂无数据");
		}

		/// <summary>
		/// Gets the system MSG detail.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult GetSystemMsgDetail(int id)
		{
			var item = SystemMsgCache("sysmsg_list").FirstOrDefault(n => n.id == id);
			if (item != null)
			{
				var data = new
				{
					item.id,
					item.title,
					item.content,
					add_time = item.add_time.ToString(),
					item.remark,
					item.read_count,
					item.sort,
					item.administrator.name
				};
				item.read_count += 1;
				SaveReadCount(item);
				return Json(SysEnum.成功, data, "获取数据成功", 1);
			}
			return Json(SysEnum.失败, "未找到对象");
		}

		/// <summary>
		/// 产品缓存
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		private List<system_msg_record> SystemMsgCache(string name)
		{
			if (!(CacheHelper.GetCache(name) is List<system_msg_record> sysmsg_list) || sysmsg_list.Count == 0)
			{
				sysmsg_list = System_msg_recordService.LoadEntities(n => n.del_flag == (int)Del_flag.正常).ToList();
				if (sysmsg_list.Any())
				{
					CacheHelper.AddCache(name, sysmsg_list, DateTime.Now.AddMinutes(5));
				}
			}
			return sysmsg_list;
		}

		private void SaveReadCount(system_msg_record smr)
		{
			if (System_msg_recordService.EditEntity(smr))
			{
				var sysmsg_list = System_msg_recordService.LoadEntities(n => n.del_flag == (int)Del_flag.正常).ToList();
				if (sysmsg_list.Any())
				{
					CacheHelper.AddCache("sysmsg_list", sysmsg_list, DateTime.Now.AddMinutes(5));
				}
			}
		}
		#endregion

		#region 设备页面		
		/// <summary>
		/// 用户产品
		/// </summary>
		/// <param name="state">产品状态</param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult UserProduct(int state = (int)Product_state.运行中)
		{
			var list = UserInfo.user.user_product.Where(n => n.state == state).ToList();
			if (list.Any())
			{
				var totalCal = list.Sum(n => n.product.Calculate_the_force);
				var data = list.Select(n => new
				{
					n.id,
					n.product.name,
					n.product.product_img,
					n.product.Calculate_the_force,
					n.product.total_score,
					add_time = n.add_time.ToString(),
					end_time = n.end_time.ToString(),
				}).ToList();

				return Json(SysEnum.成功, new { data, totalCal }, "获取数据成功", list.Count);
			}
			return Json(SysEnum.失败, "暂无数据");
		}
		#endregion

		#region 用户二维码
		/// <summary>
		/// 获取用户二维码
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public ActionResult GetUserQrCode()
		{
			var uqr = UserInfo.user.qr_code;
			dynamic data;
			string link = $"http://{Request.Url.Host}:{Request.Url.Port}/mcac/index.html?pid={UserInfo.id}";//首页地址
			if (uqr == null)
			{
				string path = $"/UploadFiles/ExtensionQR/{UserInfo.id}.png";
				Common.ImageClass.Generate2Code2(Server.MapPath(path), link);
				data = new
				{
					link,
					qr = path
				};
				UserInfo.user.qr_code = path;
				Wx_userService.EditEntity(UserInfo);
			}
			else
			{
				data = new
				{
					link,
					qr = uqr
				};
			}
			return Json(SysEnum.成功, data, "成功");
		}
		#endregion

		#region 用户订单列表
		/// <summary>
		/// 用户订单列表
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public ActionResult GetUserOrder()
		{
			var list = UserInfo.user.order.ToList();
			if (list.Any())
			{
				var data = list.OrderByDescending(n => n.add_time).Select(n => new
				{
					n.id,
					Id = Common.EncryptHelper.Encrypt(n.id.ToString()),
					n.product.name,
					add_time = n.add_time.ToString(),
					add_time2 = (n.add_time.AddMinutes(30) - DateTime.Now).TotalSeconds,
					add_time3 = n.add_time.AddMinutes(30).ToString(),
					n.product.product_img,
					n.pay_state,
					pay_state_name = Enum.GetName(typeof(Pay_state), n.pay_state),
					n.order_state,
					order_state_name = Enum.GetName(typeof(Order_state), n.order_state),
					n.count,
					n.order_money,
					n.pay_time,
					n.pay_account,
					n.refund_number,
					n.order_number
				}).ToList();
				return Json(SysEnum.成功, data, "获取数据成功", list.Count);
			}
			return Json(SysEnum.失败, "暂无数据");
		}

		/// <summary>
		/// 订单详情
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult GetOrderDetail(int id)
		{
			var list = UserInfo.user.order.ToList();
			if (list.Any())
			{
				var item = list.FirstOrDefault(n => n.id == id);
				if (item != null)
				{
					var data = new
					{
						item.id,
						item.product.name,
						item.product.product_img,
						item.add_time,
						add_time2 = (item.add_time.AddMinutes(30) - DateTime.Now).TotalSeconds,
						item.pay_state,
						pay_state_name = Enum.GetName(typeof(Pay_state), item.pay_state),
						item.order_state,
						order_state_name = Enum.GetName(typeof(Order_state), item.order_state),
						item.count,
						item.order_money,
						item.pay_time,
						item.pay_account,
						item.refund_number,
						item.order_number,
						item.product.product_introduct,
						item.product.period_time,
						item.product.product_inventory_count,
						item.product.second_level_earnings_bonuses,
						item.product.second_level_referral_bonuses,
						item.product.first_level_earnings_bonuses,
						item.product.first_level_referral_bonuses
					};
					return Json(SysEnum.成功, data, "获取数据成功", list.Count);
				}
				return Json(SysEnum.失败, "网络异常");
			}
			return Json(SysEnum.失败, "暂无数据");
		}


		#endregion

		#region 用户个人信息		
		/// <summary>
		/// Gets the user information.
		/// </summary>
		/// <returns></returns>
		public ActionResult GetUserInfo()
		{
			if (UserInfo != null)
			{
				var data = new
				{
					UserInfo.id,
					UserInfo.wx_head_protrait,
					UserInfo.state,
					UserInfo.nickname,
					UserInfo.user.qr_code,
				};
				return Json(SysEnum.成功, data, "获取数据成功", 1);
			}
			return Json(SysEnum.未授权登录);
		}
		#endregion

		#region 用户团队		
		/// <summary>
		/// 获取用户下级用户信息
		/// </summary>
		/// <returns></returns>
		public ActionResult UserTeam(){
			var user_first = Wx_userService.LoadEntities(n => n.user.pid == UserInfo.id).ToList().Select(n => new {
				n.id,
				n.nickname,
				n.user.isbuy,
			}).ToList();
			var user_second = new List<dynamic>();
			if (user_first.Any())
			{
				foreach (var item in user_first)
				{
					var second = Wx_userService.LoadEntities(n => n.user.pid == item.id).ToList().Select(n => new {
						n.id,
						nickname=$"{n.nickname}({item.nickname})",
						n.user.isbuy,
					}).ToList();
					user_second.AddRange(second);
				}
			}
			return Json(SysEnum.成功, new { UserInfo.nickname, totalCount = user_first.Count + user_second.Count, user_first, user_second }, "获取数据成功", 1);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public ActionResult UserTeamDetail(int id) {
			var item = Wx_userService.LoadEntities(n => n.id == id).FirstOrDefault();
			if (item!=null)
			{
				var data = new {
					item.nickname,
					add_time = item.add_time.ToString(),
					item.user.total_score,
					item.user.total_product_count,
					item.user.total_pay,
					item.wx_head_protrait,
				};
				return Json(SysEnum.成功, data, "获取数据成功", 1);
			}
			return Json(SysEnum.失败, "未找到对象");
		}

		#endregion

		#region 用户提问接口
		/// <summary>
		/// 用户提问接口
		/// </summary>
		/// <param name="title">问题</param>
		/// <param name="content">详情</param>
		/// <returns></returns>
		[HttpPost]
		[ValidateInput(false)]
		public ActionResult UserApplyQuestion(string title, string content)
		{
			var umr = new user_msg_record();
			umr.user_id = UserInfo.id;
			umr.isread = (int)Isread.未阅读;
			umr.title = title;
			umr.msg_content = content;
			umr.msg_pid = 0;
			umr.msg_type = (int)Msg_type.提问;
			umr.read_count = 0;
			umr.add_time = DateTime.Now;
			var res = User_msg_recordService.AddEntity(umr);
			if (res.id > 0)
			{
				return Json(SysEnum.成功, "提问成功");
			}
			return Json(SysEnum.失败, "提问失败");
		}

		#endregion

	}
}
