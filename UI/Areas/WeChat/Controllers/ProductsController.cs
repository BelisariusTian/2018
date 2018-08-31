using Common.Cache;
using IBLL;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UI.Controllers;

namespace UI.Areas.WeChat.Controllers
{
	/// <summary>
	/// 首页 及产品页
	/// </summary>
	public class ProductsController : WeiXinBaseController
	{
		//
		// GET: /WeChat/Products/
		private IproductService ProductService { get; set; }
		private IbannerService BannerService { get; set; }

		#region 首页轮播图
		/// <summary>
		/// 获取轮播图
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public ActionResult GetBanner()
		{
			var list = BannerService.LoadEntities(n => n.del_flag == (int)Del_flag.正常 && n.state == 0).OrderBy(n => n.sort).ToList();
			if (list.Any())
			{
				var data = list.Select(n => new
				{
					n.id,
					n.link_address,
					n.img_url,
				}).ToList();
				return Json(SysEnum.成功, data, "获取banner成功", data.Count);
			}
			return Json(SysEnum.失败, "未找到任何对象");
		}
		#endregion

		#region 首页产品
		/// <summary>
		/// 首页产品
		/// </summary>
		/// <param name="page">The page.</param>
		/// <param name="limit">The limit.</param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult GetProductList(int page = 1, int limit = 10)
		{
			var product_list = ProductCache("home_product");
			if (product_list.Any())
			{
				var data = product_list.OrderBy(n => n.sort).Skip((page - 1) * limit).Take(limit).Select(n => new
				{
					n.id,
					n.product_img,
					n.name,
					n.Calculate_the_force,
					n.price,
					n.period_time,
					n.sort,
					n.total_score,
				}).ToList();
				return Json(SysEnum.成功, data, "获取数据成功", product_list.Count);
			}
			return Json(SysEnum.失败, "暂无数据");
		}
		#endregion

		#region 产品详细信息

		/// <summary>
		/// 产品详细信息
		/// </summary>
		/// <param name="id">产品id</param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult GetProductDetailInfo(int id)
		{
			var product_list = ProductCache("home_product");
			if (product_list.Any())
			{
				var item = product_list.FirstOrDefault(n => n.id == id);
				if (item != null)
				{
					var data = new
					{
						item.id,
						item.name,
						item.period_time,
						item.price,
						item.product_img,
						item.product_introduct,
						item.product_sold_count,
						item.product_count,
						item.add_admin,
						item.Calculate_the_force,
						item.total_score,
						item.second_level_earnings_bonuses,
						item.first_level_earnings_bonuses,
						item.second_level_referral_bonuses,
						item.first_level_referral_bonuses,
						item.remark,
						item.sort,
					};
					return Json(SysEnum.成功, data, "获取数据成功", 1);
				}
				return Json(SysEnum.失败, "未找到对象");
			}
			return Json(SysEnum.失败, "暂无数据");
		}
		#endregion

		#region 产品缓存
		/// <summary>
		/// 产品缓存
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		private List<product> ProductCache(string name)
		{
			if (!(CacheHelper.GetCache(name) is List<product> product_list) || product_list.Count == 0)
			{
				product_list = ProductService.LoadEntities(n => n.del_flag == (int)Del_flag.正常).ToList();
				if (product_list.Any())
				{
					CacheHelper.AddCache(name, product_list, DateTime.Now.AddMinutes(2));
				}
			}
			return product_list;
		} 
		#endregion



	}
}
