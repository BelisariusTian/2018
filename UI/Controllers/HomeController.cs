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
    /// 后台首页控制器
    /// </summary>
    /// <seealso cref="UI.Controllers.BaseController" />
    public class HomeController : BaseController
    {
        //
        // GET: /Home/


        #region 退出登录
        /// <summary>
        /// 退出登录
        /// </summary>
        /// <returns></returns>
        public ActionResult LogOut()
        {
            string userID = nowManager.id.ToString();
            Response.Cookies["userLoginId"].Value = null;
            SaveSyslog("退出登录！",SysLogType.后台日志,nowManager.login_account);
            //清除缓存
            CacheHelper.SetCache(userID, nowManager, DateTime.MinValue);
            return Json(SysEnum.成功);
        }
		#endregion


		/// <summary>
		/// 手工结算
		/// </summary>
		/// <returns></returns>
		public ActionResult ExcuateCalculate_earnings() {
			IApplicationContext ctx = ContextRegistry.GetContext();
			var user_score_recordService = ctx.GetObject("user_score_recordService") as Iuser_score_recordService;
			try
			{
				var date = Convert.ToDateTime(DateTime.Now.ToShortDateString());
				var item_list = user_score_recordService.LoadEntities(n => n.add_time == date).ToList();
				if (item_list.Any())
				{
					return Json(SysEnum.成功, "今日已结算");
				}
				else {
					Calculate_earnings(ctx);
					return Json(SysEnum.成功, "结算成功");
				}
			}
			catch (Exception e)
			{

				return Json(SysEnum.失败, e.Message);
			}
		}

		//每个用户的每个运行中的产品每天都需要计算积分收益
		//每个用户的每个运行中的产品每天都需要计算积分收益
		private void Calculate_earnings(IApplicationContext ctx)
		{
			var user_productService = ctx.GetObject("user_productService") as Iuser_productService;
			var userService = ctx.GetObject("userService") as IuserService;
			var user_score_recordService = ctx.GetObject("user_score_recordService") as Iuser_score_recordService;

			var date = Convert.ToDateTime(DateTime.Now.ToShortDateString());

			if (user_score_recordService.LoadEntities(n => n.add_time == date).Any())
			{
				return;
			}

			var yesterday = date.AddDays(-1);
			var recodeList = new List<user_score_record>();//要添加的记录集合
			var userList = new List<user>();//要修改的用户集合
											// 1 获取所有运行中的用户产品信息
			var user_productList = user_productService.LoadEntities(n => n.state == (int)Product_state.运行中 || (n.state == (int)Product_state.已过期 && n.end_time > yesterday)).ToList();
			var user_productInfoList = user_productList.Select(n => new
			{
				n.user_id,
				n.user,
				n.state,
				n.product,
				n.add_time,
				n.end_time,
				n.user.pid,
				userPid = n.user.pid == 0 ? null : userService.LoadEntities(d => d.id == n.user.pid).FirstOrDefault(),
				userPPid = GetUserPPid(n.user.pid, userService),
			}).ToList();

			//2 针对每个用户的每个产品进行计算(给用户，用户的上级，用户的上上级 计算积分收益)
			foreach (var item in user_productInfoList)
			{
				TimeSpan ts;
				if (item.state == (int)Product_state.运行中)
				{
					ts = date - item.add_time;
				}
				else
				{
					ts = Convert.ToDateTime(item.end_time.ToString()) - yesterday;
				}
				Tuple<List<user_score_record>, Dictionary<user, decimal>> res;
				if (ts.Days >= 1)
				{
					//大于24小时，按整天计算
					res = AddScoreRecord(item.user, item.userPid, item.userPPid, item.product, 24, date);
				}
				else
				{
					//小于24小时，按小时计算
					res = AddScoreRecord(item.user, item.userPid, item.userPPid, item.product, ts.Hours, date);
				}
				recodeList.AddRange(res.Item1);

				foreach (var Item2 in res.Item2)
				{
					var dd = userList.FirstOrDefault(n => n.id == Item2.Key.id);
					if (dd != null)
					{
						dd.total_score += Item2.Value;
						dd.usable_score += Item2.Value;
					}
					else
					{
						Item2.Key.total_score += Item2.Value;
						Item2.Key.usable_score += Item2.Value;
						userList.Add(Item2.Key);
					}
				}
			}
			//3 将计算结果存入用户积分记录表
			user_score_recordService.AddEnties(recodeList);
			userService.EditEntities(userList);
		}

		private Tuple<List<user_score_record>, Dictionary<user, decimal>> AddScoreRecord(user user, user puser, user ppuser, product pro, int h, DateTime date)
		{
			var res = new List<user_score_record>();
			var itms = new List<user>();
			var userR = new user_score_record();
			var res_items = new Dictionary<user, decimal>();
			decimal totalscore = pro.Calculate_the_force * h;
			decimal ps = puser == null ? 0 : (totalscore * pro.first_level_earnings_bonuses) / 100;
			decimal pps = ppuser == null ? 0 : (totalscore * pro.second_level_earnings_bonuses) / 100;
			userR.id = Guid.NewGuid();
			userR.user_id = user.id;
			//userR.score = totalscore - ps - pps;//从用户收益扣除
			userR.score = totalscore;//从系统奖励
			userR.type = (int)Score_type.收益;
			userR.state = (int)User_score_record_state.已完成;
			userR.source_name = $"{pro.name}在{date.AddDays(-1).ToShortDateString()}的收益";
			userR.add_time = date;
			userR.remark = ((int)Score_source_remark.产品直接收益).ToString();
			res.Add(userR);

			//user.total_score += userR.score;
			//user.usable_score += userR.score;
			//itms.Add(user);
			res_items.Add(user, userR.score);

			if (ps > 0)
			{
				var userPR = new user_score_record();
				userPR.id = Guid.NewGuid();
				userPR.user_id = puser.id;
				userPR.type = (int)Score_type.收益;
				userPR.score = ps;
				userPR.state = (int)User_score_record_state.已完成;
				userPR.source_name = $"一级用户{user.name}的{pro.name}在{date.AddDays(-1).ToShortDateString()}的返回的收益";
				userPR.add_time = date;
				userPR.source_id = user.id;
				userPR.remark = ((int)Score_source_remark.一级用户产品间接收益).ToString();
				res.Add(userPR);
				//puser.total_score += userPR.score;
				//puser.usable_score += userPR.score;
				//itms.Add(user);
				res_items.Add(puser, userPR.score);
			}
			if (pps > 0)
			{
				var userPPR = new user_score_record();
				userPPR.id = Guid.NewGuid();
				userPPR.user_id = ppuser.id;
				userPPR.type = (int)Score_type.收益;
				userPPR.score = pps;
				userPPR.state = (int)User_score_record_state.已完成;
				userPPR.source_name = $"二级用户{user.name}的{pro.name}在{date.AddDays(-1).ToShortDateString()}的返回的收益";
				userPPR.remark = ((int)Score_source_remark.二级用户产品间接收益).ToString();
				userPPR.add_time = date;
				userPPR.source_id = user.id;
				res.Add(userPPR);

				//ppuser.total_score += userPPR.score;
				//ppuser.usable_score += userPPR.score;
				//itms.Add(user);
				res_items.Add(ppuser, userPPR.score);
			}
			return new Tuple<List<user_score_record>, Dictionary<user, decimal>>(res, res_items);
		}

		private user GetUserPPid(int pid, IuserService userService)
		{
			if (pid != 0)
			{
				var item = userService.LoadEntities(n => n.id == pid).FirstOrDefault();
				if (item != null && item.pid != 0)
				{
					return userService.LoadEntities(n => n.id == item.pid).FirstOrDefault();
				}
				return null;
			}
			return null;
		}
	}
}
