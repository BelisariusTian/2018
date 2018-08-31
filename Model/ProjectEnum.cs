///////////////////////////////////////
/////作者:		Belisarius	//////////////////
/////日期：	2018-06-01		//////////////////
/////备注：	项目枚举		/////////////////
/////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
	public enum Del_flag : int
	{
		正常 = 0,
		逻辑删除 = 1,
	}

	public enum State
	{
		可用 = 0,
		不可用 = 1
	}

	public enum Isbuy
	{
		未购买 = 0,
		已购买 = 1
	}

	public enum Msg_type
	{
		QA=0,
		提问 = 1,
		回复 = 2,
		公告=3
	}

	public enum Isread {
		未阅读=0,
		已阅读=1
	}

	public enum Product_state {
		运行中=0,
		已过期=1
	}
	public enum Score_type {
		收益=0,
		提现=1,
		系统扣除=2,
	}

	public enum Score_source_remark {
		系统赠送=0,
		一级用户购买赠送=1,
		二级用户购买赠送=7,
		产品直接收益=2,
		一级用户产品间接收益=3,
		二级用户产品间接收益=4,	
		用户提现=5,
		系统扣除积分=6,
	}

	public enum Order_state {
		等待支付=0,
		取消支付=1,
		订单过期=2,
		确认支付=3,
		已完成=4,
		
	}

	public enum Pay_state {
		未支付 = 0,
		已支付 = 1,
		已退款=2
	}

	public enum User_state {
		正常=0,
		冻结=1,
	}

	public enum User_score_record_state {
		已完成=0,
		未完成=1,
		等待处理=2,
		提现失败=3,
		提现成功=4
	}


}
