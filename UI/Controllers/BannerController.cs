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
	public class BannerController : BaseController
	{
		//
		// GET: /Banner/

		private IbannerService BannerService { get; set; }
		private IproductService ProductService { get; set; }

		#region 查询
		/// <summary>
		/// 获取广告图
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public ActionResult GetBannerList()
		{
			var bList = BannerService.LoadEntities(n => n.del_flag == (int)Del_flag.正常).ToList();
			var data = bList.OrderBy(n => n.sort).Select(n => new {
				n.id,
				n.img_url,
				n.link_address,
				n.thumbnail_url,
				n.sort,
				n.state,
				n.add_admin,
				add_time=n.add_time.ToString(),
				productName = GetProductName(n.remark)
			}).ToList();
			return Json(SysEnum.成功, data, "获取数据成功",bList.Count);
		}


		private string GetProductName(string sid) {
			if (int.TryParse(sid, out int id))
			{
				return ProductService.LoadEntities(n => n.id == id).FirstOrDefault().name;
			}
			return string.Empty;
		}

		/// <summary>
		/// 检查名称
		/// </summary>
		/// <param name="name">检查名称</param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult CheckBannerName(string name)
		{
			return Json(SysEnum.成功, "添加成功");
		}

		#endregion

		#region 添加
		/// <summary>
		/// 添加banner图
		/// </summary>
		/// <param name="banner">The Banner.</param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult AddBanner(banner banner)
		{
			var data = Request["data"];
			var bannerData = SerializeHelper.SerializeToObject<dynamic>(data);
			banner.img_url = bannerData.img_url;
			banner.link_address = bannerData.link_address;
			banner.remark = bannerData.remark;
			banner.sort = bannerData.sort;
			banner.add_time = DateTime.Now;
			banner.add_admin = nowManager.name;
			//banner.thumbnail_url = bannerData.thumbnail_url;
			banner.state = bannerData.state ?? 1;
			var res = BannerService.AddEntity(banner);
			if (res.id>0)
			{
				SaveSyslog($"添加广告图(id={res.id})成功", SysLogType.后台日志, nowManager.name);
				return Json(SysEnum.成功, "添加成功");
			}
			return Json(SysEnum.失败, "添加失败");
		}


		#endregion

		#region 修改
		/// <summary>
		/// 修改角色信息.
		/// </summary>
		/// <param name="banner">The Banner.</param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult EditBanner(banner banner)
		{
			var data = Request["data"];
			var bannerData = SerializeHelper.SerializeToObject<dynamic>(data);
			int id = bannerData.id;
			banner = BannerService.LoadEntities(n => n.id == id).FirstOrDefault();
			if (banner!=null)
			{
				banner.img_url = bannerData.img_url;
				banner.link_address = bannerData.link_address;
				banner.remark = bannerData.remark;
				banner.sort = bannerData.sort;
				
				//banner.thumbnail_url = bannerData.thumbnail_url;
				if (BannerService.EditEntity(banner))
				{
					SaveSyslog($"修改广告图(id={id})成功", SysLogType.后台日志, nowManager.name);
					return Json(SysEnum.成功, "修改成功");
				}
				return Json(SysEnum.失败, "修改失败");
			}
			return Json(SysEnum.失败, "未找到对象");
		}


		/// <summary>
		/// Edits the banner.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult EditBanner(int id) {
			var data = BannerService.LoadEntities(n => n.id == id).ToList();
			if (data.Any())
			{
				var result = data.Select(n => new
				{
					n.id,
					n.img_url,
					n.thumbnail_url,
					n.link_address,
					n.sort,
					n.remark,
					n.state

				}).FirstOrDefault();
				return Json(SysEnum.成功,result, "获取数据成功",1);
			}
			return Json(SysEnum.失败, "未找到对象");
		}

		/// <summary>
		/// Changes the state.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult ChangeState(int id) {
			var item = BannerService.LoadEntities(n => n.id == id).FirstOrDefault();
			if (item!=null)
			{
				item.state = item.state == 0 ? 1 : 0;
				if (BannerService.EditEntity(item))
				{
					SaveSyslog($"修改广告图状态{Enum.GetName(typeof(State),item.state)}(id={id})成功", SysLogType.后台日志, nowManager.name);
					return Json(SysEnum.成功, "修改成功");
				}
				return Json(SysEnum.失败, "修改失败");
			}
			return Json(SysEnum.失败, "未找到对象");
		}

		/// <summary>
		/// 修改排序
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <param name="sort">The value.</param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult ChangeSort(int id,int sort)
		{
			var item = BannerService.LoadEntities(n => n.id == id).FirstOrDefault();
			if (item != null)
			{
				item.sort = sort;
				if (BannerService.EditEntity(item))
				{
					SaveSyslog($"修改广告图排序(id={id})成功", SysLogType.后台日志, nowManager.name);
					return Json(SysEnum.成功, "修改成功");
				}
				return Json(SysEnum.失败, "修改失败");
			}
			return Json(SysEnum.失败, "未找到对象");
		}


		#endregion

		#region 删除
		/// <summary>
		/// 删除角色
		/// </summary>
		/// <param>The administrator.</param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult DelBanner(int id)
		{
			if (BannerService.DeleteEntity(id))
			{
				SaveSyslog($"删除广告图(id={id})成功", SysLogType.后台日志, nowManager.name);
				return Json(SysEnum.成功, "删除成功");
			}
			return Json(SysEnum.失败, "删除失败");
		}
		#endregion

	}
}
