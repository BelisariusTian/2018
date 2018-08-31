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
    public class UserMsgController : BaseController
	{
		// GET: /UserMsg/

		private Iuser_msg_recordService User_msg_recordService { get; set; }

		/// <summary>
		/// 消息列表
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="page">The page.</param>
		/// <param name="limit">The limit.</param>
		/// <param name="day">多少天之内的数据</param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult GetUserMsgList(string name, int page = 1, int limit = 20,int day=7) {
			if (!(CacheHelper.GetCache($"user_msg_recordType={(int)Msg_type.提问}") is List<user_msg_record> list) || list.Count == 0)
			{
				var d = DateTime.Now.AddDays(-61);
				list = User_msg_recordService.LoadEntities(n => n.msg_type == (int)Msg_type.提问&&n.add_time<d).ToList();//最近61天内的数据
				if (list.Any())
				{
					CacheHelper.AddCache($"user_msg_recordType={(int)Msg_type.提问}", list, DateTime.Now.AddMinutes(20));
				}
			}
			int count = list.Count();
			if (!string.IsNullOrEmpty(name))
			{
				list = list.Where(n => n.user.name.Contains(name) || n.msg_content.Contains(name)).ToList();
			}
			var s = DateTime.Now.AddDays(-day);
			list = list.Where(n => n.add_time <s&& n.msg_pid==0).Skip((page - 1) * limit).Take(limit).ToList();
			if (list.Any())
			{
				var data = list.Select(n => new {
					n.id,
					n.user.name,
					n.user_id,
					n.title,
					n.msg_content,
					n.isread,
					msg_type=Enum.GetName(typeof(Msg_type),n.msg_type),
					add_time = n.add_time.ToString(),
					n.remark,
				}).ToList();
				return Json(SysEnum.成功, data, "获取数据成功", count);
			}
			return Json(SysEnum.失败, "没有任何数据");
		}


		

		/// <summary>
		/// 管理员回复
		/// </summary>
		/// <param name="umr">The umr.</param>
		/// <returns></returns>
		[HttpPost]
		[ValidateInput(false)]
		public ActionResult ReplyUseMsg(user_msg_record umr) {
			var data = Request["data"];
			var umr_data = SerializeHelper.SerializeToObject<dynamic>(data);
			umr.admin_id = nowManager.id;
			umr.isread = (int)Isread.未阅读;
			umr.msg_content = umr_data.msg_content;
			umr.msg_pid = umr_data.msg_pid;
			umr.msg_type = (int)Msg_type.回复;
			umr.read_count = 0;
			umr.remark = umr_data.remark;
			umr.add_time = DateTime.Now;
			umr.user_id = umr_data.user_id;
			var res = User_msg_recordService.AddEntity(umr);
			if (res.id>0)
			{
				SaveSyslog($"{nowManager.name}回复了{umr_data.user_id}的问题(回复id={res.id})", SysLogType.后台日志, nowManager.name);
				return Json(SysEnum.成功,"回复成功");
			}
			return Json(SysEnum.失败, "回复失败");
		}

		/// <summary>
		/// Changes the state of the read.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult ChangeReadState(int id) {
			var item = User_msg_recordService.LoadEntities(n => n.id == id).FirstOrDefault();
			if (item!=null)
			{
				item.isread = (int)Isread.已阅读;
				if (User_msg_recordService.EditEntity(item))
				{
					SaveSyslog($"{nowManager.name}查看了{item.user.name}的问题)", SysLogType.后台日志, nowManager.name);
					return Json(SysEnum.成功, "修改成功");
				}
				return Json(SysEnum.失败, "修改失败");
			}
			return Json(SysEnum.失败, "未找到对象");
		}

		/// <summary>
		/// 获取用户问题及回复
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult GetUseMsgLink(int id) {
			if (!(CacheHelper.GetCache($"user_msg_record") is List<user_msg_record> list) || list.Count == 0)
			{
				var d = DateTime.Now.AddDays(-61);
				list = User_msg_recordService.LoadEntities(n => n.msg_type == (int)Msg_type.提问 && n.add_time < d).ToList();//最近61天内的数据
				if (list.Any())
				{
					CacheHelper.AddCache($"user_msg_record", list, DateTime.Now.AddMinutes(20));
				}
			}
			if (list.Any())
			{
				var data = new List<user_msg_record>();
				data.Add(GetParentMsg(list, id));
				if (data.Any())
				{
					var res = data.OrderBy(n=>n.add_time).Select(n => new {
						n.id,
						user_name = n.user.name,
						n.user_id,
						n.title,
						n.msg_content,
						n.isread,
						msg_type = Enum.GetName(typeof(Msg_type), n.msg_type),
						add_time = n.add_time.ToString(),
						n.remark,
						admin_name = n.administrator == null ? string.Empty : n.administrator.name,
					}).ToList();
					return Json(SysEnum.成功, data, "获取数据成功", list.Count());
				}
				return Json(SysEnum.失败, "没有任何数据");
			}
			return Json(SysEnum.失败, "没有任何数据");
		}

		private user_msg_record GetParentMsg(List<user_msg_record> list, int id) {
			var res = list.FirstOrDefault(n => n.msg_pid == id);
			if (res!=null)
			{
				return GetParentMsg(list, res.id);
			}
			return res;
		}
	}
}
