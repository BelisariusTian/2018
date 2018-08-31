///////////////////////////////////////////////////////////
//  时 间：2018年3月12日10:41:31
//  作 者：Leeseett
///////////////////////////////////////////////////////////
using Common;
using Common.Cache;
using IBLL;
using Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace UI.Controllers
{
	/// <summary>
	/// 微信用户管理控制器
	/// </summary>
	/// <seealso cref="UI.Controllers.BaseController" />
	public class WXUserController : BaseController
	{
		private Iwx_userService Wx_userService { get; set; }
		private IuserService UserService { get; set; }
		private Iconfig_ruleService Config_ruleService { get; set; }
		private Iuser_score_recordService User_score_recordService { get; set; }

		#region 分页获取用户数据
		/// <summary>
		/// 分页获取用户数据
		/// </summary>
		/// <param name="page">第几页</param>
		/// <param name="limit">多少条</param>
		/// <param name="searchKey">搜索关键字</param>
		/// <param name="state">用户状态</param>
		/// <returns></returns>
		public ActionResult GetData(int page, int limit, string searchKey, int state = -1)
		{
			List<wx_user> wxuserList;

			int totalCount;
			if (string.IsNullOrEmpty(searchKey))
			{
				wxuserList = Wx_userService.LoadPageEntities(page, limit, out totalCount, s => true, s => s.add_time,
					false).ToList();
			}
			else
			{
				wxuserList = Wx_userService.LoadPageEntities(page, limit, out totalCount, s => true && s.nickname.Contains(searchKey), s => s.add_time,
				 false).ToList();
			}
			if (state != -1)
			{
				wxuserList = Wx_userService.LoadPageEntities(page, limit, out totalCount, s => true && s.state == state, s => s.add_time,
	  false).ToList();
			}
			List<dynamic> dataList = new List<dynamic>();
			if (wxuserList.Any())
			{
				foreach (var w in wxuserList)
				{
					dataList.Add(
						new
						{
							Id = Common.EncryptHelper.Encrypt(w.id.ToString()),
							w.nickname,
							Sex = Enum.GetName(typeof(Sex), w.sex),
							w.city,
							w.wx_head_protrait,
							add_time = w.add_time == null ? "-" : w.add_time.ToString("yyyy-MM-dd"),
							State = Enum.GetName(typeof(WXUserState), w.state),
							totalCount

						});
				}
				return Json(SysEnum.成功, dataList, "获取数据成功", totalCount);
			}
			else
			{
				return Json(SysEnum.失败, "无数据");
			}
		}
		#endregion

		#region 分页获取数据
		/// <summary>
		/// 获取用户
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="page">The page.</param>
		/// <param name="limit">The limit.</param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult GetUserList(string name, int page = 1, int limit = 20)
		{
			int totalCount = 0;
			var item = string.IsNullOrEmpty(name) ? UserService.LoadPageEntities(page, limit, out totalCount, n => true, n => n.wx_user.add_time, true).ToList() : UserService.LoadPageEntities(page, limit, out totalCount, n => n.name.Contains(name) || n.tel.Contains(name), n => n.wx_user.add_time, true).ToList();
			if (item.Any())
			{
				var data = item.Select(n => new
				{
					n.id,
					n.name,
					n.sex,
					pidName = getPidName(n.pid),
					n.total_score,
					n.total_product_count,
					n.total_pay,
					n.usable_score,
					n.yet_use_score,
					add_time = n.wx_user.add_time.ToString(),
					firstlevel_child = GetFirstLevelChild(n.id).Item1.Count(),
					secondlevel_child = GetFirstLevelChild(n.id).Item2.Count(),
					n.isbuy,
					n.state,
				}).ToList();
				return Json(SysEnum.成功, data, "获取数据成功", totalCount);
			}
			return Json(SysEnum.失败, "没有任何数据");
		}

		private string getPidName(int pid)
		{
			if (pid != 0)
			{
				return UserService.LoadEntities(n => n.id == pid).FirstOrDefault().name;
			}
			return string.Empty;
		}
		private Tuple<List<int>, List<int>> GetFirstLevelChild(int id)
		{
			var uf = CacheHelper.GetCache($"user_first_child_{id}") as List<user>;
			var us = CacheHelper.GetCache($"user_second_child_{id}") as List<user>;
			var f = new List<int>();
			var s = new List<int>();
			if (uf == null || uf.Count() == 0)
			{
				uf = UserService.LoadEntities(n => n.pid == id).ToList();
				if (uf.Any())
				{
					CacheHelper.AddCache($"user_first_child_{id}", uf, DateTime.Now.AddMinutes(10));
				}
			}
			f = uf.Select(n => n.id).ToList();

			if (us == null || us.Count() == 0)
			{
				us = UserService.LoadEntities(n => f.Contains(n.pid)).ToList();
				if (us.Any())
				{
					CacheHelper.AddCache($"user_second_child_{id}", us, DateTime.Now.AddMinutes(10));
				}
			}
			s = us.Select(n => n.id).ToList();

			return new Tuple<List<int>, List<int>>(f, s);
		}
		#endregion

		#region 修改用户状态
		/// <summary>
		/// Changes the state.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult ChangeState(int id)
		{
			var item = UserService.LoadEntities(n => n.id == id).FirstOrDefault();
			if (item != null)
			{
				item.state = item.state == 0 ? 1 : 0;
				if (UserService.EditEntity(item))
				{
					CacheHelper.RemoveCache("all_user_list");
					SaveSyslog($"管理员修改用户{item.id}状态成功", SysLogType.后台日志, nowManager.name);
					return Json(SysEnum.成功, "修改状态成功");
				}
				return Json(SysEnum.失败, "修改状态失败");
			}
			return Json(SysEnum.失败, "未找到对象");
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult ChangeBuyState(int id)
		{
			var item = UserService.LoadEntities(n => n.id == id).FirstOrDefault();
			if (item != null)
			{
				item.isbuy = item.isbuy == 0 ? 1 : 0;
				if (UserService.EditEntity(item))
				{
					CacheHelper.RemoveCache("all_user_list");
					SaveSyslog($"管理员修改用户{item.id}购买状态成功", SysLogType.后台日志, nowManager.name);
					return Json(SysEnum.成功, "修改购买状态成功");
				}
				return Json(SysEnum.失败, "修改购买状态失败");
			}
			return Json(SysEnum.失败, "未找到对象");
		}
		#endregion

		#region 获取用户积分记录		
		/// <summary>
		/// Gets the user score list.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <param name="name"></param>
		/// <param name="type"></param>
		/// <param name="day"></param>
		/// <param name="page"></param>
		/// <param name="limit"></param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult GetUser_ScoreList(int id, string name, int type = (int)Score_type.收益, int day = 7, int page = 1, int limit = 20)
		{
			var item = UserService.LoadEntities(n => n.id == id).FirstOrDefault();
			if (item != null)
			{
				var usr = CacheHelper.GetCache($"user_id={id}_srList") as List<user_score_record>;
				if (usr == null || usr.Count == 0)
				{
					usr = item.user_score_record.ToList();
					if (usr.Any())
					{
						CacheHelper.AddCache($"user_id={id}_srList", usr, DateTime.Now.AddMinutes(5));
					}
				}
				if (usr.Any())
				{
					int totalCount = usr.Count;
					if (!string.IsNullOrEmpty(name))
					{
						usr = usr.Where(n => n.source_name.Contains(name)).ToList();
					}
					var date = DateTime.Now.AddDays(-day);
					usr = usr.Where(n => n.add_time >= date && n.type == type).OrderByDescending(n => n.add_time).Skip((page - 1) * limit).Take(limit).ToList();
					if (usr.Any())
					{
						var data = usr.Select(n => new
						{
							add_time = n.add_time.ToString(),
							n.score,
							n.state,
							type = Enum.GetName(typeof(Score_type), n.type),
							n.source_name,
							n.source_id,
							n.remark,
							n.order_id,
						}).ToList();
						return Json(SysEnum.成功, data, "获取数据成功", totalCount);
					}
					return Json(SysEnum.失败, "没有相关记录");
				}
				return Json(SysEnum.失败, "没有任何积分记录");
			}
			return Json(SysEnum.失败, "未找到对象");
		}
		#endregion

		#region 获取下级用户列表

		/// <summary>
		/// 获取下级用户列表
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult GetUser_ChildList(int id)
		{
			var uf = CacheHelper.GetCache($"user_first_child_{id}") as List<user>;
			if (uf == null || uf.Count() == 0)
			{
				uf = UserService.LoadEntities(n => n.pid == id).ToList();
				if (uf.Any())
				{
					CacheHelper.AddCache($"user_first_child_{id}", uf, DateTime.Now.AddMinutes(10));
				}
			}
			if (uf.Any())
			{
				var item = UserService.LoadEntities(n => n.id == id).FirstOrDefault();
				var data = uf.Select(n => new
				{
					n.id,
					n.name,
					n.state,
					n.isbuy,
					children = GetSecondChild(n.id)
				}).ToList();
				var data1 = new
				{
					item.id,
					item.name,
					item.state,
					children = data,
				};
				return Json(SysEnum.成功, data1, "获取数据成功", uf.Count());
			}
			return Json(SysEnum.失败, "未找到对象");
		}
		private List<dynamic> GetSecondChild(int id)
		{
			var result = new List<dynamic>();
			var data = UserService.LoadEntities(n => n.pid == id).ToList();
			if (data.Any())
			{
				foreach (var item in data)
				{
					result.Add(new
					{
						item.id,
						item.name,
						item.isbuy,
						item.state
					});
				}
			}
			return result;
		}


		/// <summary>
		/// Gets the user child.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult GetUserChild(int id)
		{
			var userList = UserService.LoadEntities(n => n.pid != 0).ToList();
			if (userList.Any())
			{
				var item = userList.FirstOrDefault(n => n.id == id);
				if (item != null)
				{
					var data = new { item.id, item.name, children = GetUser(userList, item.id) };
					return Json(SysEnum.成功, data, "获取数据成功", 1);
				}

				return Json(SysEnum.失败, "未找到对象");
			}
			return Json(SysEnum.失败, "未找到对象");
		}

		private List<dynamic> GetUser(List<user> userList, int id)
		{
			var data = new List<dynamic>();
			var childList = userList.Where(n => n.pid == id).ToList();
			if (childList.Count() > 0)
			{
				foreach (var item in childList)
				{
					data.Add(new
					{
						item.id,
						item.name,
						children = GetUser(userList, item.id)
					});
				}
			}
			return data;
		}
		#endregion

		#region 获取用户产品集合

		/// <summary>
		/// Gets the user product list.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <param name="state"></param>
		/// <param name="page"></param>
		/// <param name="limit"></param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult GetUserProductList(int id, int state = (int)Product_state.运行中, int page = 1, int limit = 20)
		{
			var item = UserService.LoadEntities(n => n.id == id).FirstOrDefault();
			if (item != null)
			{
				var res = item.user_product.Where(n => n.state == state).ToList();
				if (res.Any())
				{
					var data = res.OrderByDescending(n => n.add_time).Skip((page - 1) * limit).Take(limit).Select(n => new
					{
						n.id,
						n.product.name,
						add_time = n.add_time.ToString(),
						end_time = n.end_time.ToString(),
						n.state,
						n.product.Calculate_the_force,
					}).ToList();
					return Json(SysEnum.成功, data, "获取数据成功", 1);
				}
				return Json(SysEnum.失败, "该用户没有任何产品");
			}
			return Json(SysEnum.失败, "未找到对象");
		}

		#endregion

		#region 用户关系		
		/// <summary>
		/// Gets the user relation.
		/// </summary>
		/// <returns></returns>
		public ActionResult GetUserRelation()
		{
			var parentList = UserService.LoadEntities(n => n.pid == 0).ToList();//祖级用户列表

			var parentData = parentList.Select(n => new { n.id, n.name,n.isbuy,n.total_pay }).ToList();
			return Json(SysEnum.成功, new { }, "获取数据成功");
		}
		/// <summary>
		/// 用户关系链
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public ActionResult GetParentLink(int id)
		{
			var item = UserService.LoadEntities(n => n.id == id).FirstOrDefault();
			if (item != null)
			{
				var user_list = CacheUser("user_list");
				var name = item.pid != 0 ? user_list.FirstOrDefault(n => n.id == item.pid).name : " ";
				var data = GetChild(user_list, item,name);
				return Json(SysEnum.成功, data, "获取数据成功");
			}
			return Json(SysEnum.失败, "获取对象失败");
		}

		/// <summary>
		/// 递归获取用户关系链
		/// </summary>
		/// <param name="user_list"></param>
		/// <param name="user"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public dynamic GetChild(List<user> user_list, user user,string name)
		{
			dynamic data;
			var childList = user_list.Where(n => n.pid == user.id).ToList();
			if (childList.Any())
			{
				var child = new List<dynamic>();
				foreach (var item in childList)
				{
					child.Add(GetChild(user_list, item,user.name));
				}
				data = new
				{
					user.id,
					user.name,
					user.isbuy,
					user.total_pay,
					user.pid,
					pName = name,
					children = child 
				};
			}
			else
			{
				data = new
				{
					user.id,
					user.name,
					user.isbuy,
					user.pid,
					user.total_pay,
					pName = name,
					children =0,
				};
			}
			return data;
		}
		#endregion

		#region 主页面菜单会员数
		/// <summary>
		/// 主页面菜单会员数
		/// </summary>
		/// <returns></returns>
		public ActionResult Menu_UserCount()
		{
			var user_list = CacheUser("all_user_list");
			dynamic data;
			var year = DateTime.Now.Year;
			var month = DateTime.Now.Month;
			var day = DateTime.Now.Day;
			var start = new DateTime(year, month, day, 0, 0, 0);
			var end = new DateTime(year, month, day, 23, 59, 59);
			//var yesterday = new DateTime(year, month, day - 1);
			var yesterday = DateTime.Now.AddDays(-1);
			var week = GetWeekNumber(DateTime.Now);
			data = new
			{
				totalCount = user_list.Count,
				todayCount = user_list.Where(n => n.wx_user.add_time >= start).Count(),
				yesterdayCount = user_list.Where(n => n.wx_user.add_time.ToShortDateString() == yesterday.ToShortDateString()).Count(),
				weekCount = user_list.Where(n => GetWeekNumber(n.wx_user.add_time) == week).Count(),
				monthCount = user_list.Where(n => n.wx_user.add_time.Month == month && n.wx_user.add_time.Year == year).Count(),
			};
			return Json(SysEnum.成功, data, "获取数据成功");
		}

		/// <summary>
		/// 是第几个星期
		/// </summary>
		/// <param name="date"></param>
		/// <returns></returns>
		public int GetWeekNumber(DateTime date)
		{
			var _cultureInfo = CultureInfo.CurrentCulture;
			return _cultureInfo.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
		}
		#endregion

		#region 主页菜单积分统计数据
		/// <summary>
		/// 主页菜单积分统计数据
		/// </summary>
		/// <returns></returns>
		public ActionResult Menu_Score()
		{
			var config_rule = Config_ruleService.LoadEntities(n => n.name == "总积分数").FirstOrDefault();
			var total_systemScore = config_rule != null ? config_rule.value : "300000000";
			var user_score_recordList = User_score_recordService.LoadEntities(n => n.state == (int)User_score_record_state.已完成).ToList();
			var total_userScore = user_score_recordList.Any() ? user_score_recordList.Where(n => n.type == (int)Score_type.收益).Sum(n => n.score) : 0;
			var total_applyScore = user_score_recordList.Any() ? user_score_recordList.Where(n => n.type == (int)Score_type.提现).Sum(n => n.score) : 0;
			var total_system_deductScore = user_score_recordList.Any() ? user_score_recordList.Where(n => n.type == (int)Score_type.系统扣除).Sum(n => n.score) : 0;
			return Json(SysEnum.成功, new { total_systemScore, total_userScore, total_applyScore, total_system_deductScore }, "获取数据成功");
		}

		#endregion

		#region 用户ECHART图
		/// <summary>
		/// 用户ECHART图
		/// </summary>
		/// <returns></returns>
		public ActionResult Menu_UserCountLink()
		{
			var user_list = CacheUser("all_user_list");
			var time = new List<string>();
			var data = new List<string>();
			for (int i = 30; i > 0; i--)
			{
				var date = DateTime.Now.AddDays(1 - i);
				time.Add(date.ToString("yyyy-MM-dd"));
				data.Add(GetMangDataByDay(user_list, date.ToShortDateString()));
			}
			return Json(SysEnum.成功, new { time, data }, "获取数据成功");
		}

		private string GetMangDataByDay(List<user> user_list, string v)
		{
			var res = string.Empty;
			res = user_list.Where(n => n.wx_user.add_time.ToShortDateString() == v).Count().ToString();
			return res;
		}

		#endregion

		#region 导出Excel

		/// <summary>
		/// 导出用户EXCEL
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public void ExportExcel(string name)
		{
			var user_list = CacheUser("all_user_list");
			if (user_list.Any())
			{
				if (!string.IsNullOrEmpty(name))
				{
					user_list = user_list.Where(n => n.name.Contains(name)).ToList();
				}
				List<dynamic> list = new List<dynamic>();
				if (user_list.Any())
				{
					foreach (var item in user_list)
					{
						var one = new
						{
							item.id,
							item.name,
							item.tel,
							parent_user = item.pid == 0 ? "" : user_list.FirstOrDefault(n => n.id == item.pid).name,
							item.total_pay,
							item.sex,
							state = Enum.GetName(typeof(User_state), item.state),
							item.wx_user.nickname,
							item.wx_user.add_time,
							item.total_product_count,
							item.usable_score,
							item.total_score,
							isbuy = Enum.GetName(typeof(Isbuy), item.isbuy),
							item.wx_user.city,
							item.wx_user.country,
							item.wx_user.province,
						};
						list.Add(one);
					}
					if (list.Any())
					{
						DataTable dt = ListToDataTable.List2DataTable<dynamic>(list);
						if (dt != null && dt.Rows.Count > 0)
						{
							dt.Columns["id"].ColumnName = "id";
							dt.Columns["name"].ColumnName = "名称";
							dt.Columns["nickname"].ColumnName = "微信昵称";
							dt.Columns["tel"].ColumnName = "电话号码";
							dt.Columns["sex"].ColumnName = "性别";
							dt.Columns["city"].ColumnName = "城市";
							dt.Columns["province"].ColumnName = "省份";
							dt.Columns["country"].ColumnName = "国家";
							dt.Columns["add_time"].ColumnName = "加入时间";
							dt.Columns["parent_user"].ColumnName = "推荐人";
							dt.Columns["total_pay"].ColumnName = "总消费";
							dt.Columns["state"].ColumnName = "状态";
							dt.Columns["total_product_count"].ColumnName = "产品数量";
							dt.Columns["total_score"].ColumnName = "总积分";
							dt.Columns["usable_score"].ColumnName = "可用积分";
							dt.Columns["isbuy"].ColumnName = "是否购买";
						}
						bool hh = OfficeHelper.ExportExcelWithAspose(dt, $"用户信息{DateTime.Now.ToString("yyyy年MM月dd日HH时mm分ss秒")}", "", true);
					}
				}
			}
		}
		#endregion

		#region 用户缓存
		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public List<user> CacheUser(string name)
		{
			if (!(CacheHelper.GetCache(name) is List<user> user_list) || user_list.Count == 0)
			{
				user_list = UserService.LoadEntities(n => true).ToList();
				if (user_list.Any())
				{
					CacheHelper.AddCache(name, user_list, DateTime.Now.AddMinutes(2));
				}
			}
			return user_list;
		}

		/// <summary>
		/// 用户积分记录缓存
		/// </summary>
		/// <param name="name"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public List<user_score_record> CacheUserScoreRecord(string name, int type)
		{
			if (!(CacheHelper.GetCache(name) is List<user_score_record> user_Score_list) || user_Score_list.Count == 0)
			{
				user_Score_list = User_score_recordService.LoadEntities(n => n.type == type).ToList();
				if (user_Score_list.Any())
				{
					CacheHelper.AddCache(name, user_Score_list, DateTime.Now.AddMinutes(5));
				}
			}
			return user_Score_list;
		}

		#endregion

		#region 用户提现申请及状态更改

		/// <summary>
		/// 用户提现申请表
		/// </summary>
		/// <param name="name"></param>
		/// <param name="day"></param>
		/// <param name="page"></param>
		/// <param name="limit"></param>
		/// <param name="state"></param>
		/// <returns></returns>
		public ActionResult UserApplyScoreList(string name, int day = 7, int page = 1, int limit = 20, int state = (int)User_score_record_state.等待处理)
		{
			int type = (int)Score_type.提现;
			var use_score_list = CacheUserScoreRecord($"usr_type={type}", type);
			var date = DateTime.Now.AddDays(-day);
			if (!string.IsNullOrEmpty(name))
			{
				use_score_list = use_score_list.Where(n => n.user.name.Contains(name)).ToList();
			}
			var user_apply = use_score_list.Where(s => s.type == (int)Score_type.提现 && s.remark.Equals(((int)Score_source_remark.用户提现).ToString()) && s.state == state && s.add_time >= date).Skip((page - 1) * limit).Take(limit).ToList();
			if (user_apply.Any())
			{
				var data = user_apply.Select(n => new
				{
					n.user.name,
					n.score,
					n.id,
					add_time = n.add_time.ToString(),
					address = n.user_address != null ? n.user_address.address : "没有地址",
					n.user_id,
					address_name = n.user_address != null ? n.user_address.name : "没有名称",
					state_name = Enum.GetName(typeof(User_score_record_state), n.state),
					n.state,
				}).ToList();
				return Json(SysEnum.成功, data, "获取数据成功", user_apply.Count);
			}
			return Json(SysEnum.失败, "没有任何提现申请");
		}
		/// <summary>
		/// 用户提现成功后
		/// </summary>
		/// <param name="id">用户id</param>
		/// <param name="sid"></param>
		/// <returns></returns>
		public ActionResult AfterApplySuccess(int id,Guid sid)
		{
			var userItem = UserService.LoadEntities(n => n.id == id).FirstOrDefault();
			var config = Config_ruleService.LoadEntities(n => n.name == "提现费用").FirstOrDefault();

			var us = User_score_recordService.LoadEntities(n => n.id == sid).FirstOrDefault();
			if (us!=null&&us.score>0)
			{
				userItem.usable_score -= us.score;
			}
			if (config != null)
			{
				Decimal.TryParse(config.value, out decimal score);
				if (userItem != null)
				{
					var nUsr = new user_score_record();
					nUsr.id = Guid.NewGuid();
					nUsr.user_id = id;
					nUsr.type = (int)Score_type.系统扣除;
					nUsr.score = score;
					nUsr.state = (int)User_score_record_state.已完成;
					nUsr.source_name = $"{userItem.name}提现扣除积分{score}";
					nUsr.add_time = DateTime.Now;
					nUsr.remark = ((int)Score_source_remark.系统扣除积分).ToString();

					//userItem.total_score -= nUsr.score;
					userItem.usable_score -= nUsr.score;

					userItem.user_score_record.Add(nUsr);

					if (UserService.EditEntity(userItem))
					{
						CacheHelper.RemoveCache("all_user_list");
						CacheHelper.RemoveCache($"usr_type={nUsr.type}");
						SaveSyslog($"添加了一条用户{userItem.id}的积分记录", SysLogType.后台日志, nowManager.name);
						return Json(SysEnum.成功, "添加扣除积分记录成功");
					}
					return Json(SysEnum.失败, "添加用户总积分失败");
				}
				return Json(SysEnum.失败, "没有找到对象规则");
			}
			return Json(SysEnum.失败, "没有提现费用规则");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="id"></param>
		/// <param name="state"></param>
		/// <returns></returns>
		public ActionResult ChangeUserApplyState(Guid id, int state)
		{
			var config = Config_ruleService.LoadEntities(n => n.name == "提现费用").FirstOrDefault();
			var item = User_score_recordService.LoadEntities(n => n.id == id).FirstOrDefault();
			if (item != null)
			{
				item.state = state;
				if (state==4)
				{
					Decimal.TryParse(config.value, out decimal score);
					item.score -= score;
				}
				if (User_score_recordService.EditEntity(item))
				{
					int type = (int)Score_type.提现;
					CacheHelper.RemoveCache($"usr_type={type}");
					SaveSyslog($"管理员{nowManager.name}修改了{item.user.name}的提现积分记录", SysLogType.后台日志, nowManager.name);

					return Json(SysEnum.成功, new { item.user_id,sid=id }, "修改状态成功", 1);
				}
				return Json(SysEnum.失败, "修改状态失败");
			}
			return Json(SysEnum.失败, "没有找到该对象");
		}
		#endregion

		#region 管理员添加积分记录
		/// <summary>
		/// 添加用户积分记录
		/// </summary>
		/// <returns></returns>
		public ActionResult AddUserScoreRecord()
		{
			var data = Request["data"];
			var usrData = SerializeHelper.SerializeToObject<dynamic>(data);
			int user_id = usrData.user_id;
			var userItem = UserService.LoadEntities(n => n.id == user_id).FirstOrDefault();
			if (userItem != null)
			{
				int remark = usrData.remark;
				var nUsr = new user_score_record();
				nUsr.id = Guid.NewGuid();
				nUsr.user_id = user_id;
				nUsr.type = usrData.type;
				nUsr.score = usrData.score;
				nUsr.state = (int)User_score_record_state.已完成;
				nUsr.source_name = $"管理员{nowManager.name}执行了{Enum.GetName(typeof(Score_source_remark), remark)}选项,原因：{usrData.source_name}";
				nUsr.add_time = DateTime.Now;
				nUsr.remark = usrData.remark;
				if (nUsr.remark == ((int)Score_source_remark.系统扣除积分).ToString())
				{
					userItem.total_score -= nUsr.score;
					userItem.usable_score -= nUsr.score;
				}
				else
				{
					userItem.total_score += nUsr.score;
					userItem.usable_score += nUsr.score;
				}
				userItem.user_score_record.Add(nUsr);
				if (UserService.EditEntity(userItem))
				{
					CacheHelper.RemoveCache("all_user_list");
					CacheHelper.RemoveCache($"user_id={user_id}_srList");
					SaveSyslog($"添加了一条用户{userItem.id}的积分记录", SysLogType.后台日志, nowManager.name);
					return Json(SysEnum.成功, usrData.type, "添加积分记录成功");
				}
				return Json(SysEnum.失败, "添加用户总积分失败");
			}
			return Json(SysEnum.失败, "没有找到该对象");

		}
		#endregion



	}
}
