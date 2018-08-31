using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
	public partial class wx_request_ruleService
	{

		public bool AddAttentionMsg(wx_request_rule wx_Rule, wx_request_content wx_Content)
		{

			wx_Rule.response_type = wx_Content.type_msg;
			base.GetDbSession.wx_request_ruleDal.EditEntity(wx_Rule);

			//
			var wx_Contents = wx_Content.type_msg == 3 ? wx_Rule.wx_request_content.Where(w => w.type_msg != 3).ToList() : wx_Rule.wx_request_content.ToList();

			for (int i = 0; i < wx_Contents.Count(); i++)
			{
				var item = wx_Contents[i];
				base.GetDbSession.wx_request_contentDal.DeleteEntity(item);
			}

			wx_Rule.wx_request_content.Add(wx_Content);
			return base.GetDbSession.SaveChanges();
		}

		public bool AddEntity(wx_request_rule wx_Rule, wx_request_content wx_Content)
		{

			base.GetDbSession.wx_request_ruleDal.AddEntity(wx_Rule);
			wx_Rule.wx_request_content.Add(wx_Content);
			return base.GetDbSession.SaveChanges();
		}

		public bool EditEntity(wx_request_rule wx_Rule, wx_request_content wx_Content)
		{

			base.GetDbSession.wx_request_ruleDal.EditEntity(wx_Rule);
			base.GetDbSession.wx_request_contentDal.EditEntity(wx_Content);
			return base.GetDbSession.SaveChanges();
		}

		public bool AddTeletext(wx_request_rule wx_Rule, wx_request_content wx_Content)
		{
			wx_Rule.wx_request_content.Add(wx_Content);
			return base.GetDbSession.SaveChanges();
		}

		public bool DelEntity(wx_request_content wx_Content)
		{
			var wx_Rule = wx_Content.wx_request_rule;
			base.GetDbSession.wx_request_ruleDal.DeleteEntity(wx_Rule);
			base.GetDbSession.wx_request_contentDal.DeleteEntity(wx_Content);
			return base.GetDbSession.SaveChanges();
		}


		public bool DelAll(wx_request_rule wx_Rule)
		{
			var wx_Contents = wx_Rule.wx_request_content.ToList();
			for (int i = 0; i < wx_Contents.Count(); i++)
			{
				var item = wx_Contents[i];
				base.GetDbSession.wx_request_contentDal.DeleteEntity(item);
			}
			base.GetDbSession.wx_request_ruleDal.DeleteEntity(wx_Rule);
			return base.GetDbSession.SaveChanges();
		}

	}
}
