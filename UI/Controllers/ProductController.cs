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
	/// 产品
	/// </summary>
	/// <seealso cref="UI.Controllers.BaseController" />
	public class ProductController : BaseController
	{
		//
		// GET: /Product/
		private IproductService ProductService { get; set; }


		#region 查询
		/// <summary>
		/// 获取产品
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public ActionResult GetProductList(string search,int page=1,int limit=20)
		{
			int i = 0;
			string whereLambda = $"del_flag = @{i}";
			List<dynamic> whereLambdaValues = new List<dynamic>();
			whereLambdaValues.Add((int)Del_flag.正常);
			string whereLambadExt = string.Empty;
			if (!string.IsNullOrEmpty(search))
			{
				i += 1;
				whereLambda += $"&&name.Contains(@{i})";
				whereLambdaValues.Add(search);
			}
			var proList = ProductService.LoadPageEntities(page, limit, out int totalCount, whereLambda, n => n.sort, true, whereLambdaValues.ToArray()).ToList();		
			if (proList.Any())
			{
				var data = proList.Select(n => new {
					n.id,
					n.name,
					n.first_level_earnings_bonuses,
					n.second_level_earnings_bonuses,
					n.first_level_referral_bonuses,
					n.second_level_referral_bonuses,
					n.period_time,
					n.mod_time,
					n.add_admin,
					n.add_time,
					n.Calculate_the_force,
					n.price,
					n.product_count,
					n.product_img,
					n.product_introduct,
					n.product_inventory_count,
					n.product_sold_count,
					n.sort,
					n.total_score,
					n.unit,
					n.remark
				}).ToList();
				return Json(SysEnum.成功,data, "没有任何产品",totalCount);
			}
			return Json(SysEnum.失败, "没有任何产品");
		}

		/// <summary>
		/// 检查名称
		/// </summary>
		/// <param name="name">检查名称</param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult CheckProductName(string name)
		{
			var item = ProductService.LoadEntities(n => n.name.Equals(name)&&n.del_flag==(int)Del_flag.正常).FirstOrDefault();
			if (item==null)
			{
				return Json(SysEnum.成功, "该名称可用");
			}
			return Json(SysEnum.失败, "该产品名称已存在");
		}

		#endregion

		#region 添加
		/// <summary>
		/// 添加产品
		/// </summary>
		/// <param name="product">The Product.</param>
		/// <returns></returns>
		[HttpPost]
		[ValidateInput(false)]
		public ActionResult AddProduct(product product)
		{
			var data = Request["data"];
			var productData = SerializeHelper.SerializeToObject<dynamic>(data);
			product.name = productData.name;
			product.add_time = DateTime.Now;
			product.add_admin = nowManager.name;
			product.period_time = productData.period_time;
			product.price = productData.price;
			product.Calculate_the_force = productData.Calculate_the_force;
			product.product_count = productData.product_count;
			product.product_img = productData.product_img;
			product.product_introduct = productData.product_introduct;
			product.product_inventory_count = product.product_count;
			product.product_sold_count = 0;
			product.remark = productData.remark;
			product.first_level_earnings_bonuses = productData.first_level_earnings_bonuses;
			product.first_level_referral_bonuses = productData.first_level_referral_bonuses;
			product.second_level_earnings_bonuses = productData.second_level_earnings_bonuses;
			product.second_level_referral_bonuses = productData.second_level_referral_bonuses;
			product.sort = productData.sort;
			product.total_score = productData.total_score;
			product.unit = productData.unit??"元";
			product.mod_time = DateTime.Now;
			if (ProductService.AddEntity(product).id>0)
			{
				return Json(SysEnum.成功, "添加成功");
			}
			return Json(SysEnum.失败, "添加失败");
		}


		#endregion

		#region 修改
		/// <summary>
		/// 修改产品信息.
		/// </summary>
		/// <param name="product">The Product.</param>
		/// <returns></returns>
		[HttpPost]
		[ValidateInput(false)]
		public ActionResult EditProduct(product product)
		{
			var data = Request["data"];
			var productData = SerializeHelper.SerializeToObject<dynamic>(data);
			int id = productData.id;
			product = ProductService.LoadEntities(n => n.id == id).FirstOrDefault();
			if (product!=null)
			{
				int newP = productData.product_count;
				int span =  newP - product.product_count;
				product.name = productData.name;
				product.mod_time = DateTime.Now;
				product.add_admin = nowManager.name;
				product.Calculate_the_force = productData.Calculate_the_force;
				product.period_time = productData.period_time;
				product.price = productData.price;
				product.product_count = productData.product_count;
				product.product_img = productData.product_img;
				product.product_introduct = productData.product_introduct;
				product.product_inventory_count = product.product_inventory_count + span;
				product.remark = productData.remark;
				product.first_level_earnings_bonuses = productData.first_level_earnings_bonuses;
				product.first_level_referral_bonuses = productData.first_level_referral_bonuses;
				product.second_level_earnings_bonuses = productData.second_level_earnings_bonuses;
				product.second_level_referral_bonuses = productData.second_level_referral_bonuses;
				product.sort = productData.sort;
				product.total_score = productData.total_score;
				if (ProductService.EditEntity(product))
				{
					return Json(SysEnum.成功, "修改成功");
				}
				return Json(SysEnum.失败, "修改失败");

			}
			return Json(SysEnum.失败, "未找到对象");
		}

		/// <summary>
		/// 获取
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult EditProduct(int id) {
			var item = ProductService.LoadEntities(n => n.id == id).ToList();
			if (item.Any())
			{
				var data = item.Select(n => new {
					n.id,
					mod_time = n.mod_time.ToString(),
					n.add_admin,
					add_time = n.add_time.ToString(),
					n.period_time,
					n.price,
					n.name,
					n.product_count,
					n.product_img,
					n.product_introduct,
					n.product_inventory_count,
					n.product_sold_count,
					n.remark,
					n.second_level_earnings_bonuses,
					n.second_level_referral_bonuses,
					n.sort,
					n.total_score,
					n.unit,
					n.Calculate_the_force,
					n.first_level_earnings_bonuses,
					n.first_level_referral_bonuses,
				}).FirstOrDefault();
				return Json(SysEnum.成功, data,"获取数据成功",1);
			}
			return Json(SysEnum.失败, "未找到对象");
		}

		/// <summary>
		/// 根据字段修改产品信息
		/// </summary>
		/// <param name="field">The field.</param>
		/// <param name="value">The value.</param>
		/// <param name="id">The identifier.</param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult EditProductByFiled(string field, string value, int id) {
			var item = ProductService.LoadEntities(n => n.id == id).FirstOrDefault();
			if (item!=null)
			{
				switch (field)
				{
					case "role_name":
						item.name = value;
						break;
					case "sort":
						item.sort = Convert.ToInt32(value);
						break;
					case "price":
						item.price = Convert.ToDecimal(value);
						break;
					case "period_time":
						item.period_time = Convert.ToInt32(value);
						break;
					case "first_level_earnings_bonuses":
						item.first_level_earnings_bonuses = Convert.ToInt32(value);
						break;
					case "second_level_referral_bonuses":
						item.second_level_referral_bonuses = Convert.ToInt32(value);
						break;
					case "first_level_referral_bonuses":
						item.first_level_referral_bonuses = Convert.ToInt32(value);
						break;
					case "second_level_earnings_bonuses":
						item.second_level_earnings_bonuses = Convert.ToInt32(value);
						break;
					case "Calculate_the_force":
						item.Calculate_the_force = Convert.ToInt32(value);
						break;
				}
				item.mod_time = DateTime.Now;
				if (ProductService.EditEntity(item))
				{
					return Json(SysEnum.成功, "修改成功");
				}
				return Json(SysEnum.失败, "修改失败");
			}
			return Json(SysEnum.失败, "未找到对象");
		}

		#endregion

		#region 删除
		/// <summary>
		/// 删除产品
		/// </summary>
		/// <param>The administrator.</param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult DelProduct(int id)
		{
			var item = ProductService.LoadEntities(n => n.id == id).FirstOrDefault();
			if (item!=null)
			{
				item.del_flag = (int)Del_flag.逻辑删除;
				item.mod_time = DateTime.Now;
				if (ProductService.EditEntity(item))
				{
					return Json(SysEnum.成功, "删除成功");
				}
				return Json(SysEnum.失败, "删除失败");
			}
			return Json(SysEnum.失败, "未找到对象");
		}
		#endregion

	}
}
