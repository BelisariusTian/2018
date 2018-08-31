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
	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="UI.Controllers.BaseController" />
	public class AdminController : BaseController
    {
		// GET: /Admin/
		private IadministratorService AdministratorService { get; set; }
		private IactionService ActionService { get; set; }
		private IroleService RoleService { get; set; }

		#region 查询
		/// <summary>
		/// 获取管理员
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public ActionResult GetAdminList()
		{
			var roleList = RoleService.LoadEntities(n => n.del_flag ==(int)Del_flag.正常).ToList();
			if (roleList.Any())
			{
				var data =new List<dynamic>();
				foreach (var item in roleList.OrderBy(n=>n.sort).ToList())
				{
					data.Add(new {
						item.role_name,
						role_id = item.id,
						item.sort,
						adminList = item.administrator.Where(s=>s.del_flag== (int)Del_flag.正常).Select(a => new {
							admin_id =a.id,
							admin_name = a.name,
							admin_headimg = a.head_protrait,
						}).ToList()
					});
				}
				return Json(SysEnum.成功, data,"获取数据成功",nowManagerMaxRoleid());
			}
			return Json(SysEnum.失败, "没有任何角色");
		}

		private int nowManagerMaxRoleid()
		{
			return nowManager.role.Max(n => n.sort);
		}

		/// <summary>
		///获取当前登录管理员
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		public ActionResult GetNowLoginAdminInfo() {
			if (nowManager!=null)
			{
				var res = new
				{
					username = nowManager.name,
					id = nowManager.id,
					nowManager.head_protrait,
				};
				return Json(SysEnum.成功, res, "找到数据", 1);
			}
			return Json(SysEnum.失败, "未找到对象");
		}

		

		#endregion

		#region 添加
		/// <summary>
		/// 添加管理员
		/// </summary>
		/// <param name="administrator">The administrator.</param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult AddAdmin(administrator administrator)
		{
			var data = Request["data"];
			var adminData = SerializeHelper.SerializeToObject<dynamic>(data);
			int role_id = adminData.role_id;
			var role = RoleService.LoadEntities(n => n.id == role_id).FirstOrDefault();
			if (role==null)
			{
				role = RoleService.LoadEntities(n => n.id==3).FirstOrDefault();//默认添加超级管理员
			}
			string pwd = adminData.login_pwd; 
			administrator = new administrator();
			administrator.add_time = DateTime.Now;
			administrator.head_protrait = adminData.head_protrait;
			administrator.login_account = adminData.login_account;
			administrator.login_pwd = Common.EncryptHelper.Encrypt(pwd);
			administrator.mod_time = DateTime.Now;
			administrator.sort = adminData.sort;
			administrator.name = adminData.name;
			administrator.role.Add(role);
			if (AdministratorService.AddEntity(administrator).id>0)
			{
				SaveSyslog($"管理员{administrator.name}信息添加成功", SysLogType.后台日志, nowManager.name);
				return Json(SysEnum.成功, "添加成功");
			}
			return Json(SysEnum.失败, "添加管理员失败");
		}

		/// <summary>
		/// 检查账号
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult CheckAccount(string name) {
			var item = AdministratorService.LoadEntities(n => n.login_account.Equals(name) && n.del_flag == (int)Del_flag.正常).FirstOrDefault();
			if (item==null)
			{
				return Json(SysEnum.成功, "账号名可用");
			}
			return Json(SysEnum.失败, "账号名已存在");
		}

		/// <summary>
		/// 检查名称
		/// </summary>
		/// <param name="name">The account.</param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult CheckName(string name)
		{
			var item = AdministratorService.LoadEntities(n => n.name.Equals(name) && n.del_flag == (int)Del_flag.正常).FirstOrDefault();
			if (item == null)
			{
				return Json(SysEnum.成功, "账号名可用");
			}
			return Json(SysEnum.失败, "账号名已存在");
		}

		#endregion

		#region 修改
		/// <summary>
		/// 修改管理员信息.
		/// </summary>
		/// <param name="administrator">The administrator.</param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult EditAdmin(administrator administrator)
		{
			var data = Request["data"];
			var adminData = SerializeHelper.SerializeToObject<dynamic>(data);
			int id = adminData.id;
			administrator = AdministratorService.LoadEntities(n => n.id == id).FirstOrDefault();
			if (administrator!=null)
			{
				string pwd = adminData.login_pwd;
				administrator.mod_time = DateTime.Now;
				administrator.login_pwd = Common.EncryptHelper.Encrypt(pwd);
				administrator.sort = adminData.sort;
				administrator.name = adminData.name;
				administrator.head_protrait = adminData.head_protrait;
				administrator.remark = adminData.remark;
				if (AdministratorService.EditEntity(administrator))
				{
					SaveSyslog($"管理员信息修改成功", SysLogType.后台日志, nowManager.name);
					return Json(SysEnum.成功, "修改成功");
				}
				return Json(SysEnum.失败, "修改失败");
			}
			return Json(SysEnum.失败, "获取对象失败");
		}

		/// <summary>
		/// 获取管理员详细信息
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult EditAdmin(int id) {
			var admin = AdministratorService.LoadEntities(n => n.id == id).FirstOrDefault();
			if (admin!=null)
			{
				var data = new {
					admin.id,
					admin.name,
					admin.login_account,
					login_pwd = EncryptHelper.Decrypt(admin.login_pwd),
					admin.head_protrait,
					admin.sort,
					mod_time = admin.mod_time!=null?admin.mod_time.ToString():string.Empty,
					admin.last_login_IP_address,
					add_time = admin.add_time != null ? admin.add_time.ToString() : string.Empty,
					last_login_time = admin.last_login_time != null ? admin.last_login_time.ToString() : string.Empty,
					roleList = admin.role.Select(d => new {d.role_name,d.id })
				};
				return Json(SysEnum.成功,data, "获取数据成功",1);
			}
			return Json(SysEnum.失败, "为找到对象");
		}

		/// <summary>
		/// 修改密码
		/// </summary>
		/// <param name="login_pwd">The login password.</param>
		/// <param name="newPwd">The new password.</param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult UpdateUserPwd(string login_pwd,string newPwd) {

			var admin = AdministratorService.LoadEntities(u => u.id == nowManager.id).FirstOrDefault();
			if (admin != null)
			{
				if (Common.EncryptHelper.Decrypt(admin.login_pwd) == login_pwd)
				{
					admin.login_pwd = Common.EncryptHelper.Encrypt(newPwd);
					if (AdministratorService.EditEntity(admin))
					{
						SaveSyslog("完成修改密码操作成功", SysLogType.后台日志, nowManager.name);
						return Json(SysEnum.成功, "密码修改成功 !");
					}
					else
					{
						SaveSyslog("修改密码操作失败", SysLogType.后台日志, nowManager.name);
						return Json(SysEnum.失败, "密码修改失败 !");
					}
				}
				else
				{
					return Json(SysEnum.失败, "原始密码输入有误 !");
				}

			}
			else
			{
				return Json(SysEnum.失败, "用户不存在 !");
			}
		}

		#endregion

		#region 删除
		/// <summary>
		/// 删除管理员
		/// </summary>
		/// <param>The administrator.</param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult DelAdmin(int id,int role_id)
		{
			var item = AdministratorService.LoadEntities(n => n.id == id).FirstOrDefault();
			if (item!=null)
			{
				var adminrole = item.role.FirstOrDefault(n => n.id == role_id);
				if (item.role.Count == 1)
				{
					item.del_flag = (int)Del_flag.逻辑删除;
				}
				if (adminrole != null)
				{
					item.role.Remove(adminrole);	
				}
				item.mod_time = DateTime.Now;
				if (AdministratorService.EditEntity(item))
				{
					SaveSyslog($"管理员{item.name}的{adminrole.role_name}角色被删除了",SysLogType.后台日志,nowManager.name);
					return Json(SysEnum.成功, "删除成功");	
				}
				return Json(SysEnum.失败, "删除失败");
			}
			return Json(SysEnum.失败, "删除失败");
		}
		#endregion

		#region 用户菜单
		/// <summary>
		/// 获取菜单
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		public ActionResult GetMenu()
		{
			//获取当前管理员所有菜单信息
			if (!(CacheHelper.GetCache($"adminactionidList_{nowManager.id}") is List<int> adminactionidList) || adminactionidList.Count == 0)
			{
				adminactionidList = new List<int>();
				foreach (var item in nowManager.role.ToList())
				{
					var acid = item.action.ToList().Select(n => n.id).ToList();
					adminactionidList.AddRange(acid);
				}
				adminactionidList.Distinct();
				if (adminactionidList.Count > 0)
				{
					CacheHelper.AddCache($"adminactionidList_{nowManager.id}", adminactionidList, DateTime.Now.AddHours(2));
				}
			}
			if (adminactionidList.Any())
			{
				var actionList = ActionService.LoadEntities(n => adminactionidList.Contains(n.id) && n.del_flag == (int)Del_flag.正常 && n.type == (int)ActionType.菜单权限).ToList();
				List<dynamic> data = new List<dynamic>();
				if (actionList.Any())
				{
					data = GetMenuTree(actionList.OrderBy(n => n.sort).ToList());
				}
				return Json(SysEnum.成功, data, "获取数据成功", data.Count);
			}
			return Json(SysEnum.权限不足, "权限不足");
		}

		/// <summary>
		/// 生成菜单树
		/// </summary>
		/// <param name="actionList">The action list.</param>
		/// <returns></returns>
		private List<dynamic> GetMenuTree(List<action> actionList)
		{
			var data = new List<dynamic>();
			foreach (var item in actionList.Where(n => n.pid == 0).ToList())
			{
				if (actionList.Where(n => n.pid == item.id).Count() > 0)
				{
					data.Add(new
					{
						href = item.url,
						icon = item.action_icon,
						title = item.action_name,
						children = ParseSecondMenu(actionList.Where(n => n.pid == item.id).ToList())
					});
				}
				else
				{
					data.Add(new
					{
						href = item.url,
						icon = item.action_icon,
						title = item.action_name,
					});
				}
			}
			return data;
		}


		private List<dynamic> ParseSecondMenu(List<action> actions)
		{
			List<dynamic> data = new List<dynamic>();
			foreach (var item in actions)
			{
				data.Add(new
				{
					href = item.url,
					icon = item.action_icon,
					title = item.action_name,
				});

			}
			return data;
		} 
		#endregion
	}
}
