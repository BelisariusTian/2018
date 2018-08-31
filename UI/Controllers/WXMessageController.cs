using BLL;
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
    public class WXMessageController : BaseController
	{
		//
		// GET: /WXMessage/
		private Iwx_request_ruleService Wx_RequestRuleService { get; set; }
		private Iwx_request_contentService Wx_RequestContentService { get; set; }


		#region 关注时回复

		/// <summary>
		/// 获取数据
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		[ValidateInput(false)]
		public ActionResult GetAttentionData()
		{
			try
			{
				int typeMsg = int.Parse(Request["typeMsg"]);
				int requstType = int.Parse(Request["requstType"]);

				var requestRule = Wx_RequestRuleService.LoadEntities(r => r.request_type == requstType).FirstOrDefault();
				List<dynamic> dataList = new List<dynamic>();

				if (requestRule == null)
				{
					return Json(SysEnum.失败, "暂无数据");
				}

				var wx_RequestContentList = requestRule.wx_request_content.Where(w => w.type_msg == typeMsg).OrderByDescending(w => w.sort);
				if (wx_RequestContentList.Any())
				{
					foreach (var x in wx_RequestContentList)
					{
						dataList.Add(new
						{
							RId = x.rule_id,
							Id = x.id,
							ReqTitle = x.req_title,
							ReqContent = Server.HtmlEncode(x.req_content),
							DetailUrl = x.detail_url,
							PicUrl = x.pic_url,
							MediaUrl = Server.HtmlEncode(x.media_url),
							Sort = x.sort,
							Remark = Server.HtmlEncode(x.remark)
						});
					}
					SaveSyslog("查找微信消息数据成功", SysLogType.后台日志, nowManager.name);
					return Json(SysEnum.成功, dataList, "查找数据成功", dataList.Count());
				}
				else
				{
					return Json(SysEnum.失败, "暂无数据");
				}
			}
			catch (Exception)
			{
				return Json(SysEnum.失败, "系统异常");
			}
		}

		/// <summary>
		/// 添加关注时回复
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		[HttpPost]
		[ValidateInput(false)]
		public ActionResult AddAttention(wx_request_content entity)
		{
			try
			{
				var requestRule = Wx_RequestRuleService.LoadEntities(r => r.request_type == (int)WXRequestState.关注时回复).FirstOrDefault();

				entity.add_time = DateTime.Now;


				if (Wx_RequestRuleService.AddAttentionMsg(requestRule, entity))
				{
					SaveSyslog("添加关注时回复成功", SysLogType.后台日志, nowManager.name);
					return Json(SysEnum.成功, "添加成功");
				}
				else
				{
					SaveSyslog("添加关注时回复失败", SysLogType.后台日志, nowManager.name);
					return Json(SysEnum.失败, "添加失败");
				}
			}
			catch (Exception)
			{
				return Json(SysEnum.失败, "系统异常");
			}
		}

		#endregion

		/// <summary>
		/// 获取关键字数据
		/// </summary>
		/// <param name="search">The search.</param>
		/// <param name="page">The page.</param>
		/// <param name="limit">The limit.</param>
		/// <returns></returns>
		[HttpPost]
		[ValidateInput(false)]
		public ActionResult GetKeywords(string search, int page = 1, int limit = 10)
		{
			try
			{
				int ResponseType = int.Parse(Request["ResponseType"]);
				int RequestType = int.Parse(Request["RequestType"]);
				List<dynamic> dataList = new List<dynamic>();
				List<wx_request_rule> wx_RequestRules = new List<wx_request_rule>();
				int count;

				if (string.IsNullOrEmpty(search))
				{
					wx_RequestRules = Wx_RequestRuleService.LoadPageEntities(page, limit, out count, w => w.request_type == RequestType && w.response_type == ResponseType, w => w.add_time, false).ToList();
				}
				else
				{

					wx_RequestRules = Wx_RequestRuleService.LoadPageEntities(page, limit, out count, w => w.req_keywords.Contains(search) && w.request_type == RequestType && w.response_type == ResponseType, w => w.add_time, false).ToList();
				}

				if (wx_RequestRules.Any())
				{
					foreach (var item in wx_RequestRules)
					{
						dataList.Add(new
						{
							rId = item.id,
							item.rule_name,
							item.req_keywords,
							item.request_type,
							item.response_type,
							item.sort,
							item.add_time
						});
					}
					return Json(SysEnum.成功, dataList, "查找数据成功", count);
				}
				else
				{
					return Json(SysEnum.失败, "暂无数据");
				}

			}
			catch (Exception)
			{
				return Json(SysEnum.失败, "系统异常");
			}

		}

		#region 关键字回复
		/// <summary>
		/// 获取关键字和回复内容数据
		/// </summary>
		/// <param name="search">The search.</param>
		/// <param name="page">The page.</param>
		/// <param name="limit">The limit.</param>
		/// <returns></returns>
		[HttpPost]
		[ValidateInput(false)]
		public ActionResult GetKeywordData(string search, int page = 1, int limit = 10)
		{
			try
			{
				int ResponseType = int.Parse(Request["ResponseType"]);
				int RequestType = int.Parse(Request["RequestType"]);
				List<wx_request_rule> wx_RequestRules = new List<wx_request_rule>();
				List<dynamic> dataList = new List<dynamic>();
				int count;

				if (!string.IsNullOrEmpty(search))
				{
					wx_RequestRules = Wx_RequestRuleService.LoadEntities(r => r.req_keywords.Contains(search) && r.request_type == (int)WXRequestState.关键字回复 && r.request_type == RequestType && r.response_type == ResponseType).ToList();
				}
				else
				{
					wx_RequestRules = Wx_RequestRuleService.LoadEntities(r => r.request_type == (int)WXRequestState.关键字回复 && r.request_type == RequestType && r.response_type == ResponseType).ToList();
				}

				if (wx_RequestRules.Any())
				{

					List<int> rIds = wx_RequestRules.Select(s => s.id).ToList();

					var wx_Contents = Wx_RequestContentService.LoadPageEntities(page, limit, out count, w => rIds.Contains(w.rule_id), s => s.add_time, false);

					if (wx_Contents.Any())
					{
						foreach (var x in wx_Contents)
						{
							dataList.Add(new
							{
								rId = x.wx_request_rule.id,
								keywords = x.wx_request_rule.req_keywords,
								Id = x.id,
								ReqTitle = x.req_title,
								ReqContent = Server.HtmlEncode(x.req_content),
								DetailUrl = x.detail_url,
								PicUrl = x.pic_url,
								MediaUrl = Server.HtmlEncode(x.media_url),
								AddTime = x.add_time,
								Remark = Server.HtmlEncode(x.remark)
							});
						}
						SaveSyslog("获取关键字回复数据成功", SysLogType.后台日志, nowManager.name);
						return Json(SysEnum.成功, dataList, "查找数据成功", count);
					}
					else
					{

						return Json(SysEnum.失败, "暂无数据");
					}
				}
				else
				{
					return Json(SysEnum.失败, "暂无数据");
				}
			}
			catch (Exception)
			{
				return Json(SysEnum.失败, "系统异常");
			}

		}




		/// <summary>
		/// 根据ID获取回复内容数据
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		[ValidateInput(false)]
		public ActionResult GetDataID()
		{

			try
			{
				int id = int.Parse(Request["id"]);
				var entity = Wx_RequestContentService.LoadEntities(w => w.id == id).ToList().Select(s => new
				{
					Keyword = s.wx_request_rule.req_keywords,
					s.id,
					s.req_title,
					s.req_content,
					s.detail_url,
					s.pic_url,
					s.media_url,
					s.remark,
					s.sort,
					s.add_time,
					s.rule_id,
					s.type_msg

				}).FirstOrDefault();

				if (entity == null)
				{
					return Json(SysEnum.失败, "数据不存在");
				}

				return Json(SysEnum.成功, entity, "查找数据成功");
			}
			catch (Exception)
			{
				return Json(SysEnum.失败, "系统异常");
			}
		}




		#region 只操作关键字
		/// <summary>
		/// 根据ID获取关键字数据
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		[ValidateInput(false)]
		public ActionResult GetRuleDataID()
		{

			try
			{
				int rId = int.Parse(Request["id"]);
				var entity = Wx_RequestRuleService.LoadEntities(w => w.id == rId).FirstOrDefault();

				if (entity == null)
				{
					return Json(SysEnum.失败, "数据不存在");
				}

				return Json(SysEnum.成功, entity, "查找数据成功");
			}
			catch (Exception)
			{
				return Json(SysEnum.失败, "系统异常");
			}
		}

		/// <summary>
		/// 只添加关键字
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		public ActionResult AddAloneKeyword(wx_request_rule entity)
		{
			try
			{
				var wx_Reque = Wx_RequestRuleService.LoadEntities(w => w.req_keywords.Equals(entity.req_keywords)).FirstOrDefault();
				if (wx_Reque != null)
				{
					return Json(SysEnum.失败, "此关键字已存在");
				}
				entity.add_time = DateTime.Now;

				if (Wx_RequestRuleService.AddEntity(entity) != null)
				{
					SaveSyslog("添加关键字成功", SysLogType.后台日志, nowManager.name);
					return Json(SysEnum.成功, "添加成功");
				}
				else
				{
					SaveSyslog("添加关键字失败", SysLogType.后台日志, nowManager.name);
					return Json(SysEnum.成功, "添加失败");
				}

			}
			catch (Exception)
			{
				return Json(SysEnum.失败, "系统异常");
			}

		}

		/// <summary>
		/// 修改关键字
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		public ActionResult EditAloneKeyword(wx_request_rule entity)
		{
			try
			{
				var wx_Reque = Wx_RequestRuleService.LoadEntities(w => w.id == entity.id).FirstOrDefault();
				if (wx_Reque == null)
				{
					return Json(SysEnum.失败, "数据错误");
				}

				var wx_RequeTwo = Wx_RequestRuleService.LoadEntities(w => w.req_keywords.Equals(entity.req_keywords) && w.id != entity.id).FirstOrDefault();
				if (wx_RequeTwo != null)
				{
					return Json(SysEnum.失败, "此关键字已存在");
				}

				wx_Reque.req_keywords = entity.req_keywords;
				if (Wx_RequestRuleService.EditEntity(wx_Reque))
				{
					SaveSyslog("修改关键字成功", SysLogType.后台日志, nowManager.name);
					return Json(SysEnum.成功, "修改成功");
				}
				else
				{
					SaveSyslog("修改关键字失败", SysLogType.后台日志, nowManager.name);
					return Json(SysEnum.成功, "修改失败");
				}
			}
			catch (Exception)
			{
				return Json(SysEnum.失败, "系统异常");
			}
		}
		#endregion

		/// <summary>
		/// 添加关键字回复
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		[HttpPost]
		[ValidateInput(false)]
		public ActionResult AddKeyword(wx_request_content entity)
		{

			string keywords = Request["keywords"];
			var wx_Reque = Wx_RequestRuleService.LoadEntities(w => w.req_keywords.Equals(keywords)).FirstOrDefault();
			if (wx_Reque != null)
			{
				return Json(SysEnum.失败, "此关键字已存在");
			}

			wx_request_rule newWx_Rule = new wx_request_rule();
			newWx_Rule.rule_name = Request["RuleName"];
			newWx_Rule.req_keywords = keywords;
			newWx_Rule.request_type = (int)WXRequestState.关键字回复;
			newWx_Rule.response_type = int.Parse(Request["ResponseType"]);
			newWx_Rule.add_time = DateTime.Now;


			wx_request_content newWx_Content = new wx_request_content();
			newWx_Content.req_title = entity.req_title;
			newWx_Content.detail_url = entity.detail_url;
			newWx_Content.pic_url = entity.pic_url;
			newWx_Content.media_url = entity.media_url;
			newWx_Content.req_content = entity.req_content;
			newWx_Content.add_time = DateTime.Now;
			newWx_Content.type_msg = int.Parse(Request["ResponseType"]);
			newWx_Content.remark = entity.remark;
			//newWx_Content.RId = newWx_Rule.RId;                                      


			if (Wx_RequestRuleService.AddEntity(newWx_Rule, newWx_Content))
			{
				SaveSyslog("添加微信消息关键字成功", SysLogType.后台日志, nowManager.name);
				return Json(SysEnum.成功, "添加成功");
			}
			else
			{
				SaveSyslog("添加微信消息关键字失败", SysLogType.后台日志, nowManager.name);
				return Json(SysEnum.失败, "添加失败");
			}
		}


		/// <summary>
		/// 修改关键字与回复
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		public ActionResult EditKeyword(wx_request_content entity)
		{
			var wx_Reque = Wx_RequestRuleService.LoadEntities(w => w.id == entity.rule_id).FirstOrDefault();
			if (wx_Reque == null)
			{
				return Json(SysEnum.失败, "数据不存在");
			}
			var keywords = Request["keywords"];
			var wx_RequeTwo = Wx_RequestRuleService.LoadEntities(w => w.req_keywords.Equals(keywords) && w.id != entity.rule_id).FirstOrDefault();
			if (wx_RequeTwo != null)
			{
				return Json(SysEnum.失败, "此关键字已存在");
			}

			wx_Reque.req_keywords = keywords;


			var wx_Content = Wx_RequestContentService.LoadEntities(w => w.id == entity.id).FirstOrDefault();
			wx_Content.req_title = entity.req_title;
			wx_Content.detail_url = entity.detail_url;
			wx_Content.pic_url = entity.pic_url;
			wx_Content.media_url = entity.media_url;
			wx_Content.req_content = entity.req_content;
			wx_Content.remark = entity.remark;
			if (Wx_RequestRuleService.EditEntity(wx_Reque, wx_Content))
			{
				SaveSyslog("修改微信消息关键字成功", SysLogType.后台日志, nowManager.name);
				return Json(SysEnum.成功, "修改成功");
			}
			else
			{
				SaveSyslog("修改微信消息关键字失败", SysLogType.后台日志, nowManager.name);
				return Json(SysEnum.失败, "修改失败");
			}
		}


		/// <summary>
		/// 删除关键字与下面图文
		/// </summary>
		/// <returns></returns>
		public ActionResult DelAll()
		{

			try
			{
				int rId = int.Parse(Request["rId"]);
				var entity = Wx_RequestRuleService.LoadEntities(w => w.id == rId).FirstOrDefault();
				if (entity == null)
				{
					return Json(SysEnum.成功, "删除成功");
				}

				if (Wx_RequestRuleService.DelAll(entity))
				{
					SaveSyslog("删除关键字成功", SysLogType.后台日志, nowManager.name);
					return Json(SysEnum.成功, "删除成功");
				}
				else
				{
					SaveSyslog("删除关键字失败", SysLogType.后台日志, nowManager.name);
					return Json(SysEnum.失败, "修改失败");
				}

			}
			catch (Exception e)
			{
				return Json(SysEnum.失败, "系统异常");
			}
		}

		/// <summary>
		/// 删除关键字
		/// </summary>
		/// <returns></returns>
		public ActionResult DelKeyword()
		{

			try
			{
				int id = int.Parse(Request["id"]);
				var entity = Wx_RequestContentService.LoadEntities(w => w.id == id).FirstOrDefault();
				if (entity == null)
				{
					return Json(SysEnum.成功, "删除成功");
				}

				if (Wx_RequestRuleService.DelEntity(entity))
				{
					SaveSyslog("删除关键字成功", SysLogType.后台日志, nowManager.name);
					return Json(SysEnum.成功, "删除成功");
				}
				else
				{
					SaveSyslog("删除关键字失败", SysLogType.后台日志, nowManager.name);
					return Json(SysEnum.失败, "修改失败");
				}

			}
			catch (Exception)
			{
				return Json(SysEnum.失败, "系统异常");
			}
		}
		#endregion

		#region 图文添加修改
		/// <summary>
		/// 获取某个关键字下面的图文
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		[ValidateInput(false)]
		public ActionResult GetRuleTeletext()
		{
			try
			{
				int rId = int.Parse(Request["rId"]);
				List<dynamic> dataList = new List<dynamic>();
				var wx_Reque = Wx_RequestRuleService.LoadEntities(w => w.id == rId).FirstOrDefault();
				if (wx_Reque == null)
				{
					return Json(SysEnum.失败, "数据不存在");
				}

				if (wx_Reque.wx_request_content.Any())
				{
					var wx_Contents = wx_Reque.wx_request_content.OrderByDescending(w => w.sort);

					foreach (var item in wx_Contents)
					{
						dataList.Add(new
						{
							keywords = item.wx_request_rule.req_keywords,
							Id = item.id,
							ReqTitle = item.req_title,
							DetailUrl = item.detail_url,
							PicUrl = item.pic_url,
							MediaUrl = item.media_url,
							ReqContent = item.req_content,
							AddTime = item.add_time,
							Sort = item.sort,
							typeMsg = item.type_msg
						});
					}

					SaveSyslog("获取某个关键字的图文数据成功", SysLogType.后台日志, nowManager.name);
					return Json(SysEnum.成功, dataList, "获取数据成功");
				}
				else
				{
					return Json(SysEnum.失败, "暂无数据");
				}
			}
			catch (Exception)
			{
				return Json(SysEnum.失败, "ID错误");
			}
		}




		/// <summary>
		/// 添加图文
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		public ActionResult AddTeletext(wx_request_content entity)
		{

			var wx_Reque = Wx_RequestRuleService.LoadEntities(w => w.id == entity.rule_id).FirstOrDefault();

			wx_request_content newWx_Content = new wx_request_content();
			newWx_Content.req_title = entity.req_title;
			newWx_Content.detail_url = entity.detail_url;
			newWx_Content.pic_url = entity.pic_url;
			newWx_Content.media_url = entity.media_url;
			newWx_Content.req_content = entity.req_content;
			newWx_Content.add_time = DateTime.Now;
			newWx_Content.sort = entity.sort;
			newWx_Content.type_msg = (int)WXRequestType.图文;

			if (Wx_RequestRuleService.AddTeletext(wx_Reque, newWx_Content))
			{
				SaveSyslog("添加微信消息关键字图文成功", SysLogType.后台日志, nowManager.name);
				return Json(SysEnum.成功, "添加成功");
			}
			else
			{
				SaveSyslog("添加微信消息关键字图文失败", SysLogType.后台日志, nowManager.name);
				return Json(SysEnum.失败, "添加失败");
			}
		}

		/// <summary>
		/// 修改内容图文
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		public ActionResult EditTeletext(wx_request_content entity)
		{
			var srcWX_Content = Wx_RequestContentService.LoadEntities(w => w.id == entity.id).FirstOrDefault();
			if (srcWX_Content == null)
			{
				return Json(SysEnum.失败, "数据错误");
			}

			srcWX_Content.req_title = entity.req_title;
			srcWX_Content.detail_url = entity.detail_url;
			srcWX_Content.pic_url = entity.pic_url;
			srcWX_Content.media_url = entity.media_url;
			srcWX_Content.req_content = entity.req_content;
			srcWX_Content.add_time = DateTime.Now;
			srcWX_Content.sort = entity.sort;

			if (Wx_RequestContentService.EditEntity(srcWX_Content))
			{
				SaveSyslog("修改微信消息图文成功", SysLogType.后台日志, nowManager.name);
				return Json(SysEnum.成功, "修改成功");
			}
			else
			{
				SaveSyslog("修改微信消息图文失败", SysLogType.后台日志, nowManager.name);
				return Json(SysEnum.失败, "修改失败");
			}
		}

		/// <summary>
		/// 删除图文数据
		/// </summary>
		/// <returns></returns>
		public ActionResult DelTeletext()
		{
			try
			{
				int id = int.Parse(Request["id"]);

				var wx_Content = Wx_RequestContentService.LoadEntities(w => w.id == id).FirstOrDefault();
				if (wx_Content == null)
				{
					return Json(SysEnum.成功, "数据已删除");
				}

				if (Wx_RequestContentService.DeleteEntity(wx_Content))
				{
					SaveSyslog("删除图文数据成功", SysLogType.后台日志, nowManager.name);
					return Json(SysEnum.成功, "删除成功");
				}
				else
				{
					SaveSyslog("删除图文数据失败", SysLogType.后台日志, nowManager.name);
					return Json(SysEnum.失败, "删除失败");
				}
			}
			catch (Exception)
			{
				return Json(SysEnum.失败, "系统异常");
			}

		}

		#endregion

	}
}
