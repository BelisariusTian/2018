using Common;
using IBLL;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace UI.Controllers
{
	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="UI.Controllers.BaseController" />
	public class ConfigAndRuleController : BaseController
	{
		//
		// GET: /ConfigAndRule/

		private Iconfig_ruleService Config_ruleService { get; set; }

		#region 查询
		/// <summary>
		/// 获取配置或规则
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public ActionResult GetConfig_ruleList(int page=1,int limit =10)
		{
			var list = Config_ruleService.LoadEntities(n => true).ToList();
			if (list.Any())
			{
				var data = list.Select(n => new {
					n.id,
					n.name,
					n.value,
					n.state,
					n.remark,
					n.add_admin,
					add_time = n.add_time.ToString(),
				}).ToList();
				return Json(SysEnum.成功,data, "获取数据成功",list.Count());
			}
			return Json(SysEnum.失败, "没有任何数据");
		}

		/// <summary>
		/// 检查名称
		/// </summary>
		/// <param name="name">检查名称</param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult CheckConfig_ruleName(string name)
		{
			var item = Config_ruleService.LoadEntities(n => n.name.Equals(name)).FirstOrDefault();
			if (item==null)
			{
				return Json(SysEnum.成功, "该名称可用");
			}
			return Json(SysEnum.失败, "该名称已存在");
		}

		#endregion

		#region 添加
		/// <summary>
		/// 添加配置或规则
		/// </summary>
		/// <param name="config_rule">The config_rule.</param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult AddConfig_rule(config_rule config_rule)
		{
			var data = Request["data"];
			var config_ruleData = SerializeHelper.SerializeToObject<dynamic>(data);
			config_rule.name = config_ruleData.name;
			config_rule.value = config_ruleData.value;
			config_rule.state = config_ruleData.state ?? 1;
			config_rule.add_admin = nowManager.name;
			config_rule.add_time = DateTime.Now;
			config_rule.remark = config_ruleData.remark;
			if (Config_ruleService.AddEntity(config_rule).id>0)
			{
				SaveSyslog("添加配置与规则成功", SysLogType.后台日志, nowManager.name);
				return Json(SysEnum.成功, "添加成功");
			}
			return Json(SysEnum.失败, "添加失败");
		}


		#endregion

		#region 修改
		/// <summary>
		/// 修改配置或规则信息.
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		[ValidateInput(false)]
		public ActionResult EditConfig_rule(int id,string value,int state,string remark)
		{
			var item = Config_ruleService.LoadEntities(n => n.id == id).FirstOrDefault();
			if (item!=null)
			{
				if (!string.IsNullOrEmpty(value))
				{
					item.value = value;
				}
				item.state = state;
				if (!string.IsNullOrEmpty(remark))
				{
					item.remark = remark;
				}
				if (Config_ruleService.EditEntity(item))
				{
					SaveSyslog($"修改配置与规则(id={id})成功", SysLogType.后台日志, nowManager.name);
					return Json(SysEnum.成功, "修改成功");
				}
				return Json(SysEnum.失败, "修改失败");
			}
			return Json(SysEnum.失败, "未找到对象");
		}

		#endregion

		#region 删除
		/// <summary>
		/// 删除配置或规则
		/// </summary>
		/// <param>The administrator.</param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult DelConfig_rule(int id)
		{
			if (Config_ruleService.DeleteEntity(id))
			{
				SaveSyslog($"删除配置与规则(id={id})成功", SysLogType.后台日志, nowManager.name);
				return Json(SysEnum.成功, "删除成功");
			}
			return Json(SysEnum.失败, "删除失败");
		}
		#endregion

	}
}
