using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBLL
{
	public partial interface Iwx_request_ruleService
	{
		bool AddAttentionMsg(wx_request_rule wx_Rule, wx_request_content wx_Content);

		bool AddEntity(wx_request_rule wx_Rule, wx_request_content wx_Content);

		bool AddTeletext(wx_request_rule wx_Rule, wx_request_content wx_Content);

		bool EditEntity(wx_request_rule wx_Rule, wx_request_content wx_Content);

		bool DelEntity(wx_request_content wx_Content);

		bool DelAll(wx_request_rule wx_Rule);

	}
}
