using Common.Cache;
using IBLL;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace UI.Areas.WeChat.Controllers
{
	/// <summary>
	/// 前端用户积分
	/// </summary>
	/// <seealso cref="System.Web.Mvc.Controller" />
	public class ScoreController : WeiXinBaseController
	{
		//
		// GET: /WeChat/Score/

		private Iuser_score_recordService User_score_recordService { get; set; }
		private IuserService UserService { get; set; }
		private Iuser_addressService User_addressService { get; set; }
		private Iconfig_ruleService Config_ruleService { get; set; }

		private List<user_score_record> UsrCache(string name,int id) {
			if (!(CacheHelper.GetCache(name) is List<user_score_record> list) || list.Count==0){
				list = User_score_recordService.LoadEntities(n =>n.user_id==id).ToList();
				if (list.Any())
				{
					CacheHelper.AddCache(name, list, DateTime.Now.AddMinutes(2));
				}
			}
			return list;
		}

		#region 用户获取赠送积分和推荐购买积分
		/// <summary>
		/// 用户获取赠送积分和推荐购买积分
		/// </summary>
		/// <param name="day"></param>
		/// <param name="page">The page.</param>
		/// <param name="limit">The limit.</param>
		/// <param name="type">The type.</param>
		/// <param name="remark">The remark.</param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult GetUserSR(int day, int page = 1, int limit = 10, int type = (int)Score_type.收益,int remark =(int)Score_source_remark.系统赠送)
		{
			var list = UsrCache($"User_score_record_id={UserInfo.id}", UserInfo.id);
			if (list.Any())
			{
				var date = day == 0 ? DateTime.MinValue : DateTime.Now.AddDays(-day);
				list = list.Where(n => n.type == type && n.add_time > date && n.remark.Equals(remark.ToString())).Skip((page - 1) * limit).Take(limit).ToList();
				if (list.Any())
				{ 
					var data = list.Select(n => new
					{
						add_time = n.add_time.ToString(),
						n.id,
						n.source_name,
						n.score,
					}).ToList();
					return Json(SysEnum.成功, data, "获取数据成功", list.Count);
				}
				return Json(SysEnum.失败, "暂无相关数据");
			}
			return Json(SysEnum.失败, "暂无数据");
		} 
		#endregion

		#region 用户积分数据
		/// <summary>
		/// 积分页数据
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public ActionResult GetScoreDetail()
		{
			var list = UsrCache($"User_score_record_id={UserInfo.id}", UserInfo.id);
			var total = GroupUserScore(list);
			var date = DateTime.Now.AddDays(-1).ToShortDateString();
			var yesterday = GroupUserScore(list.Where(n => n.add_time.ToShortDateString() == date).ToList());
			return Json(SysEnum.成功, new { total, yesterday }, "获取数据成功", 1);
		}

		private dynamic GroupUserScore(List<user_score_record> list)
		{
			var totalScore = list.Where(n => n.type == (int)Score_type.收益).Sum(n => n.score);
			var recommonScore = list.Where(n => n.remark == ((int)Score_source_remark.一级用户购买赠送).ToString() || n.remark == ((int)Score_source_remark.二级用户购买赠送).ToString()).Sum(n => n.score);
			var sysgiveScore = list.Where(n => n.remark == ((int)Score_source_remark.系统赠送).ToString()).Sum(n => n.score);
			var firstScore = list.Where(n => n.remark == ((int)Score_source_remark.一级用户产品间接收益).ToString()).Sum(n => n.score);
			var secondScore = list.Where(n => n.remark == ((int)Score_source_remark.二级用户产品间接收益).ToString()).Sum(n => n.score);
			var productScore = list.Where(n => n.remark == ((int)Score_source_remark.产品直接收益).ToString()).Sum(n => n.score);
			var yetUserScore = list.Where(n => n.type == (int)Score_type.提现&&n.state==(int)User_score_record_state.提现成功).Sum(n => n.score);
			var sysDeduct = list.Where(n => n.type == (int)Score_type.系统扣除).Sum(n => n.score);
			return new { totalScore, recommonScore, sysgiveScore, firstScore, secondScore, productScore, yetUserScore, sysDeduct };
		}
		#endregion

		#region 下级用户积分记录

		/// <summary>
		/// 一级用户的返回积分记录
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public ActionResult UserFirstChildScore()
		{
			var userFirstList = UserService.LoadEntities(n => n.pid == UserInfo.id).ToList();
			if (userFirstList.Any())
			{
				var data = userFirstList.Select(n => new
				{
					n.name,
					n.id,
					recodeList = GetUserScoreList(n.id),
				}).ToList();
				return Json(SysEnum.成功, data, "获取数据成功", userFirstList.Count);
			}
			return Json(SysEnum.失败, "暂无数据");
		}

		private dynamic GetUserScoreList(int id)
		{
			var list = UsrCache($"User_score_record_id={UserInfo.id}", UserInfo.id);
			var res = list.Where(n => n.source_id == id).ToList();
			var data = res.Select(n => new
			{
				time = n.add_time.ToShortDateString(),
				n.score
			}).ToList();
			return data; 
		}

		/// <summary>
		/// 二级用户返回积分记录
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public ActionResult UserSecondChildScore()
		{
			var userFirstList = UserService.LoadEntities(n => n.pid == UserInfo.id).ToList();
			if (userFirstList.Any())
			{
				var userSecondList = new List<dynamic>();
				foreach (var item in userFirstList)
				{
					var se = UserService.LoadEntities(n => n.pid == item.id).ToList();
					foreach (var ss in se)
					{
						var s = new { ss.id, name = $"{ss.name}({item.name})", recodeList = GetUserScoreList(ss.id) };
						userSecondList.Add(s);
					}
				}
				return Json(SysEnum.成功, userSecondList, "获取数据成功", userSecondList.Count);
			}
			return Json(SysEnum.失败, "暂无数据");
		}
		#endregion

		#region 用户申请提现
		/// <summary>
		/// 用户申请提现
		/// </summary>
		/// <param name="address"></param>
		/// <param name="score"></param>
		/// <param name="adds_id"></param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult UserApplyForScore(string address, int score, int? adds_id)
		{
			if (UserInfo.user.usable_score>=score)
			{
				var na = new user_score_record();
				na.id = Guid.NewGuid();
				na.user_id = UserInfo.id;
				na.score = score;
				na.rollout_address_id = adds_id != null ? adds_id : SaveAddress(address, UserInfo);
				na.type = (int)Score_type.提现;
				na.state = (int)User_score_record_state.等待处理;
				na.source_name = $"{UserInfo.nickname}发起了提现了申请";
				na.remark = ((int)Score_source_remark.用户提现).ToString();
				na.add_time = DateTime.Now;
				try
				{
					var res = User_score_recordService.AddEntity(na);
						CacheHelper.RemoveCache($"User_score_record_id={UserInfo.id}");
					return Json(SysEnum.成功, "提交申请成功");
				}
				catch (Exception)
				{
					return Json(SysEnum.失败, "提交申请失败");
				}
			}
			return Json(SysEnum.失败, "提现积分不够");
		}

		/// <summary>
		/// 获取提现页面数据
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public ActionResult UserApplyPage() {
			var rc = Config_ruleService.LoadEntities(n => n.name.Equals("提现费用")).FirstOrDefault();
			var config = Config_ruleService.LoadEntities(n => n.name.Equals("最低提现积分")).FirstOrDefault();
			var data = new {
				usableScore = UserInfo.user.user_score_record.ToList().Where(n => n.type == (int)Score_type.收益).Sum(n => n.score),
				yetUserScore = UserInfo.user.user_score_record.ToList().Where(n => n.type == (int)Score_type.提现 && n.state == (int)User_score_record_state.提现成功).Sum(n=>n.score),
				sysDeduct = UserInfo.user.user_score_record.ToList().Where(n => n.type == (int)Score_type.系统扣除).Sum(n => n.score),
				withdrawals = rc!=null?rc.value:"20",
				remark = rc!=null?rc.remark:"",
				UserInfo.id,
				minScore = config!=null?config.value:"200",
			};
			return Json(SysEnum.成功, data, "获取数据成功", 1);
		}

		private int SaveAddress(string address, wx_user userInfo)
		{
			int id;
			var item = User_addressService.LoadEntities(n => n.address.Equals(address)).FirstOrDefault();
			if (item != null)
			{
				id = item.id;
			}
			else
			{
				var ad = new user_address();
				ad.del_flag = (int)Del_flag.正常;
				ad.state = (int)State.可用;
				ad.address = address;
				ad.user_id = userInfo.id;
				ad.name = userInfo.nickname;
				ad.add_time = DateTime.Now;
				var res = User_addressService.AddEntity(ad);
				id = res.id;
			}
			return id;
		}

		/// <summary>
		///	用户提现记录列表
		/// </summary>
		/// <param name="page">The page.</param>
		/// <param name="limit">The limit.</param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult UserApplyRecord(int page=1,int limit=10) {
			var list = UsrCache($"User_score_record_id={UserInfo.id}", UserInfo.id);
			var config = Config_ruleService.LoadEntities(n => n.name == "提现费用").FirstOrDefault();
			Decimal.TryParse(config.value, out decimal score);
			if (list.Any())
			{
				list = list.Where(n => n.type == (int)Score_type.提现).ToList();
				if (list.Any())
				{
					var data = list.Select(n => new
					{
						add_time = n.add_time.ToString(),
						n.id,
						n.source_name,
						score=n.score-=score,
						n.state,
						state_name = Enum.GetName(typeof(User_score_record_state), n.state),
						n.user_address.address,
					}).ToList();
					return Json(SysEnum.成功, data, "获取数据成功", list.Count);
				}
				return Json(SysEnum.失败, "暂无相关数据");
			}
			return Json(SysEnum.失败, "暂无数据");
		}

		/// <summary>
		/// 某个提现的个体
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult GetApplyDetail(Guid id) {
			var list = UsrCache($"User_score_record_id={UserInfo.id}", UserInfo.id);
			if (list.Any())
			{
				var item = list.FirstOrDefault(n=>n.id==id);
				if (item != null)
				{
					var data = new
					{
						add_time = item.add_time.ToString(),
						item.id,
						item.source_name,
						item.score,
						item.state,
						state_name = Enum.GetName(typeof(User_score_record_state), item.state),
						item.user_address.address,
					};
					return Json(SysEnum.成功, data, "获取数据成功", list.Count);
				}
				return Json(SysEnum.失败, "暂无相关数据");
			}
			return Json(SysEnum.失败, "暂无数据");
		}

		#endregion



	
	}
}
