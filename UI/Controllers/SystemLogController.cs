using Common.Cache;
using IBLL;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace UI.Controllers
{
    public class SystemLogController : BaseController
	{
		//
		// GET: /SystemLog/

		private Isystem_logService System_logService { get; set; }

		/// <summary>
		/// Gets the data.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="time">The time.</param>
		/// <param name="type">The type.</param>
		/// <param name="page">The page.</param>
		/// <param name="limit">The limit.</param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult GetData(string name, string time,int type=(int)SysLogType.后台日志, int page = 1, int limit = 20) {
			var list = CacheHelper.GetCache($"system_log_type={type}") as List<system_log>;
			if (list==null||list.Count==0)
			{
				list = System_logService.LoadEntities(n => n.type == type).ToList();
				if (list.Any())
				{
					CacheHelper.AddCache($"system_log_type={type}", list, DateTime.Now.AddMinutes(10));
				}
			}
			if (list.Any())
			{
				if (!string.IsNullOrEmpty(name))
				{
					list = list.Where(n => n.add_name.Contains(name)||n.content.Contains(name)).ToList();
				}
				if (!string.IsNullOrEmpty(time))
				{
					list = list.Where(n => n.add_time.ToString("yyyy-MM-dd").Equals(time)).ToList();
				}

				if (list.Any())
				{
					var result = list.OrderByDescending(n => n.add_time).Skip((page - 1) * limit).Take(limit).ToList().Select(n => new {
						n.id,
						add_time = n.add_time.ToString(),
						n.add_name,
						n.content,
						n.page_url,
						type = Enum.GetName(typeof(SysLogType), n.type),
					}).ToList();
					return Json(SysEnum.成功, result,"获取数据成功",list.Count());
				}
				return Json(SysEnum.失败, "没有任何数据");
			}
			return Json(SysEnum.失败, "没有任何数据");
		}


	}
}
