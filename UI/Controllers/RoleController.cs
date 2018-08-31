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
	/// 角色控制器
	/// </summary>
	/// <seealso cref="UI.Controllers.BaseController" />
	public class RoleController : BaseController
	{
		//
		// GET: /Role/
		private IroleService RoleService { get; set; }
		private IactionService ActionService { get; set; }


		#region 查询
		/// <summary>
		/// 获取角色列表
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public ActionResult GetRoleList(int page=1,int limit=20)
		{
			var roleList = RoleService.LoadEntities(n => n.del_flag == (int)Del_flag.正常).ToList();
			if (roleList.Any())
			{
				var result = roleList.Select(n => new
				{
					n.id,
					n.role_name,
					n.sort,
					n.add_admin,
					add_time=n.add_time==null?string.Empty:n.add_time.ToString(),
					n.state,
					n.remark,
					mod_time = n.mod_time == null ? string.Empty : n.mod_time.ToString(),
				}).ToList();
				return Json(SysEnum.成功, result, "获取数据成功", result.Count);
			}
			return Json(SysEnum.失败, "没有任何角色信息");
		}



		#endregion

		#region 添加
		/// <summary>
		/// 添加角色员
		/// </summary>
		/// <param name="role">The role.</param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult AddRole(role role)
		{
			var data = Request["data"];
			var roleData = SerializeHelper.SerializeToObject<dynamic>(data);
			role.add_admin = nowManager.name;
			role.add_time = DateTime.Now;
			role.mod_time = DateTime.Now;
			role.role_name = roleData.role_name;
			role.sort = roleData.sort;
			role.remark = roleData.remark;
			string actionList = roleData.actionList;
			if (!string.IsNullOrEmpty(actionList))
			{
				foreach (var item in GetActionList(actionList))
				{
					role.action.Add(item);
				}
			}
			if (RoleService.AddEntity(role).id > 0)
			{
				return Json(SysEnum.成功, "添加成功");
			}
			return Json(SysEnum.失败, "添加失败");
		}

		/// <summary>
		/// 根据权限id获取权限集合
		/// </summary>
		/// <param name="actionList">The action list.</param>
		/// <returns></returns>
		private List<action> GetActionList(string actionList)
		{
			var strList = actionList.Split(',');
			var idList = new List<int>();
			foreach (var id in strList)
			{
				idList.Add(Convert.ToInt32(id.Split('_')[0]));
				idList.Add(Convert.ToInt32(id.Split('_')[1]));
			}
			idList=idList.Distinct().ToList();
			return ActionService.LoadEntities(n => idList.Contains(n.id)).ToList();
		}

		/// <summary>
		/// 获取菜单权限
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public ActionResult GetMenuActionList()
		{
			var actionList = ActionService.LoadEntities(n => n.type == (int)ActionType.菜单权限&&n.del_flag==(int)Del_flag.正常).ToList();
			if (actionList.Any())
			{
				var result = from a in actionList.Where(s => s.pid == 0) select new {
					a.id,
					a.action_name,
					a.sort,
					childAction = actionList.Where(n => n.pid == a.id).Select(d => new {
						d.id,
						d.action_name,
						d.sort
					}).ToList()
				};
				return Json(SysEnum.成功, result.ToList(), "获取数据成功", RoleService.LoadEntities(n=>n.del_flag==(int)Del_flag.正常).Max(n=>n.sort));
			}
			return Json(SysEnum.失败, "没有任何角色信息");
		}


		/// <summary>
		/// 检查名称
		/// </summary>
		/// <param name="name">检查名称</param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult CheckRoleName(string name)
		{
			var result = RoleService.LoadEntities(n => n.role_name.Equals(name)&&n.del_flag==(int)Del_flag.正常).FirstOrDefault();
			if (result == null)
			{
				return Json(SysEnum.成功, "该名称可用");
			}
			return Json(SysEnum.失败, "该名称已经存在");
		}

		#endregion

		#region 修改
		/// <summary>
		/// 修改角色信息.
		/// </summary>
		/// <param name="role">The role.</param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult EditRole(role role)
		{
			var data = Request["data"];
			var roleData = SerializeHelper.SerializeToObject<dynamic>(data);
			int id = roleData.id;
			role = RoleService.LoadEntities(n => n.id == id).FirstOrDefault();
			if (role!=null)
			{
				role.add_admin = nowManager.name;
				role.mod_time = DateTime.Now;
				role.role_name = roleData.role_time;
				role.sort = roleData.sort;
				role.remark = roleData.remark;
				string actionList = roleData.actionList;
				if (!string.IsNullOrEmpty(actionList))
				{
					role.action.Clear();
					foreach (var item in GetActionList(actionList))
					{
						role.action.Add(item);
					}
				}
				if (RoleService.EditEntity(role))
				{
					return Json(SysEnum.成功, "修改成功");
				}
				return Json(SysEnum.失败, "修改失败");
			}
			return Json(SysEnum.失败, "未找到对象");
		}

		/// <summary>
		/// 修改角色
		/// </summary>
		/// <param name="field">The field.</param>
		/// <param name="value">The value.</param>
		/// <param name="id">The identifier.</param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult EditRole(string filed, string value, int id) {
			var item = RoleService.LoadEntities(n => n.id == id).FirstOrDefault();
			if (item!=null)
			{
				switch (filed)
				{
					case "role_name":
						item.role_name = value;
						break;
					case "sort":
						item.sort =Convert.ToInt32(value);
						break;
					case "remark":
						item.remark = value;
						break;
				}
				item.mod_time = DateTime.Now;
				if (RoleService.EditEntity(item))
				{
					return Json(SysEnum.成功, "修改成功");
				}
				return Json(SysEnum.失败, "修改失败");
			}
			return Json(SysEnum.失败, "未找到对象");
		}

		/// <summary>
		/// Edits the role.
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		public ActionResult EditRoleAction() {
			var data = Request["data"];
			var roleData = SerializeHelper.SerializeToObject<dynamic>(data);
			int id = roleData.id;
			string actionList = roleData.actionList;
			var role = RoleService.LoadEntities(n => n.id == id).FirstOrDefault();
			if (role != null)
			{
				role.mod_time = DateTime.Now;
				if (!string.IsNullOrEmpty(actionList))
				{
					role.action.Clear();
					foreach (var item in GetActionList(actionList))
					{
						role.action.Add(item);
					}
				}
				if (RoleService.EditEntity(role))
				{
					return Json(SysEnum.成功, "修改成功");
				}
				return Json(SysEnum.失败, "修改失败");
			}
			return Json(SysEnum.失败, "未找到对象");
		}

		/// <summary>
		/// 当前用户权限
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public ActionResult GetMenuActionListAndRoleActionList(int id) {
			var ractionList = ActionService.LoadEntities(n => n.type == (int)ActionType.菜单权限 && n.del_flag == (int)Del_flag.正常).ToList();
			if (ractionList.Any())
			{
				var result = from a in ractionList.Where(s => s.pid == 0)
							 select new
							 {
								 a.id,
								 a.action_name,
								 a.sort,
								 childAction = ractionList.Where(n => n.pid == a.id).Select(d => new
								 {
									 d.id,
									 d.action_name,
									 d.sort
								 }).ToList()
							 };
				var item = RoleService.LoadEntities(n => n.id == id).FirstOrDefault();
				if (item != null)
				{
					var actionList = item.action.Select(d => d.id).ToList();
					if (actionList.Any())
					{
						return Json(SysEnum.成功, new { actionList , result }, "修改成功", actionList.Count());
					}
					return Json(SysEnum.失败, new { result }, "没有任何角色权限信息",1);
				}
				return Json(SysEnum.失败, "未找到角色");
			}
			return Json(SysEnum.失败, "未找到任何权限");
		
		}

		#endregion

		#region 删除
		/// <summary>
		/// 删除角色
		/// </summary>
		/// <param>The administrator.</param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult DelRole(int id)
		{
			var role = RoleService.LoadEntities(n => n.id == id).FirstOrDefault();
			if (role!=null)
			{
				role.del_flag = (int)Del_flag.逻辑删除;
				role.mod_time = DateTime.Now;
				if (RoleService.EditEntity(role))
				{
					return Json(SysEnum.成功, "逻辑删除成功");
				}
				return Json(SysEnum.失败, "逻辑删除失败");
			}
			return Json(SysEnum.失败, "未找到对象");
		}
		#endregion

	}
}
