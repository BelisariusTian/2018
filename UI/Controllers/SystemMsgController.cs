using Common;
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
    public class SystemMsgController : BaseController
	{
		//
		// GET: /SystemMsg/
		private Isystem_msg_recordService System_msg_recordService { get; set; }

		/// <summary>
		/// 获取系统消息
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="page">The page.</param>
		/// <param name="limit">The limit.</param>
		/// <param name="type"></param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult GetSysQAList(string name,int page = 1, int limit = 20,int type = (int)Msg_type.QA) {
			var	list = System_msg_recordService.LoadEntities(n => n.type == type&&n.del_flag==(int)Del_flag.正常).ToList();
			if (!string.IsNullOrEmpty(name))
			{
				list = list.Where(n => n.content.Contains(name) || n.title.Contains(name)).ToList();
			}
			list = list.Skip((page - 1) * limit).Take(limit).ToList();
			if (list.Any())
			{
				var data = list.Select(n => new {
					n.id,
					n.administrator.name,
					n.content,
					n.title,
					n.sort,
					type = Enum.GetName(typeof(Msg_type), n.type),
					add_time = n.add_time.ToString(),
					n.remark,
				}).ToList();
				return Json(SysEnum.成功, data, "获取数据成功", list.Count());
			}
			return Json(SysEnum.失败, "没有任何数据");
		}

		/// <summary>
		/// 添加一条QA或系统消息
		/// </summary>
		/// <param name="smr">The SMR.</param>
		/// <returns></returns>
		[HttpPost]
		[ValidateInput(false)]
		public ActionResult AddSysMsg(system_msg_record smr) {
			var data = Request["data"];
			var smr_data = SerializeHelper.SerializeToObject<dynamic>(data);
			smr.add_time = DateTime.Now;
			smr.admin_id = nowManager.id;
			smr.mod_time = DateTime.Now;
			smr.remark = string.Empty;
			smr.sort = smr_data.sort;
			smr.title = smr_data.title;
			smr.type = smr_data.type;
			smr.del_flag = (int)Del_flag.正常;
			smr.content = smr_data.content;
			var res = System_msg_recordService.AddEntity(smr);
			if (res.id > 0)
			{
				SaveSyslog($"{nowManager.name}添加了QA(id={res.id})", SysLogType.后台日志, nowManager.name);
				return Json(SysEnum.成功, "添加成功");
			}
			return Json(SysEnum.失败, "添加失败");
		}


		/// <summary>
		/// Checks the title.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="type">The type.</param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult CheckTitle(string name, int type) {
			var item = System_msg_recordService.LoadEntities(n => n.title.Equals(name) && n.type == type).FirstOrDefault();
			if (item==null)
			{
				return Json(SysEnum.成功,"标题可用");
			}
			return Json(SysEnum.失败, "标题不可用");
		}

		/// <summary>
		/// 删除QA或
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult DelSysMsg(int id) {
			if (System_msg_recordService.DeleteEntity(id))
			{
				return Json(SysEnum.成功, "删除成功");
			}
			return Json(SysEnum.失败, "删除失败");
		}

		/// <summary>
		/// 修改字段值
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <param name="filed">The filed.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult EditSystemMsg(int id, string filed, string value) {
			var item = System_msg_recordService.LoadEntities(n => n.id == id).FirstOrDefault();
			if (item != null)
			{
				switch (filed)
				{
					case "title":
						item.title = value;
						break;
					case "sort":
						item.sort = Convert.ToInt32(value);
						break;
					case "remark":
						item.remark = value;
						break;
				}
				item.mod_time = DateTime.Now;
				if (System_msg_recordService.EditEntity(item))
				{
					return Json(SysEnum.成功, "修改成功");
				}
				return Json(SysEnum.失败, "修改失败");
			}
			return Json(SysEnum.失败, "未找到对象");
		}

		/// <summary>
		/// 提交修改
		/// </summary>
		/// <param name="smr">The SMR.</param>
		/// <returns></returns>
		[HttpPost]
		[ValidateInput(false)]
		public ActionResult EditSysMsg(system_msg_record smr) {
			var data = Request["data"];
			var smr_data = SerializeHelper.SerializeToObject<dynamic>(data);
			int id = smr_data.id;
			var item = System_msg_recordService.LoadEntities(n => n.id == id).FirstOrDefault();
			if (item!=null)
			{
				item.mod_time = DateTime.Now;
				item.content = smr_data.content;
				item.sort = smr_data.sort;
				item.title = smr_data.title;
				item.admin_id = nowManager.id;
				if (System_msg_recordService.EditEntity(item))
				{
					return Json(SysEnum.成功, "修改成功");
				}
				return Json(SysEnum.失败, "修改失败");
			}
			return Json(SysEnum.失败, "未找到对象");
		}
		/// <summary>
		/// 获取修改数据
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult EditSysMsg(int id) {
			var item = System_msg_recordService.LoadEntities(n => n.id == id).ToList();
			if (item!=null)
			{
				var data = item.Select(n => new {
					n.id,
					n.content,
					n.title,
					n.sort
				}).FirstOrDefault();
				return Json(SysEnum.成功, data,"未找到对象",1);
			}
			return Json(SysEnum.失败, "未找到对象");
		}
	}
}
