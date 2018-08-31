 
using System.Data.Entity;

namespace IDAL
{
	public partial interface IDBSession
    {
	
		IactionDal actionDal{get;}
	
		IadministratorDal administratorDal{get;}
	
		IbannerDal bannerDal{get;}
	
		Iconfig_ruleDal config_ruleDal{get;}
	
		Imsg_logDal msg_logDal{get;}
	
		IorderDal orderDal{get;}
	
		IproductDal productDal{get;}
	
		IroleDal roleDal{get;}
	
		Isystem_logDal system_logDal{get;}
	
		Isystem_msg_recordDal system_msg_recordDal{get;}
	
		IuserDal userDal{get;}
	
		Iuser_addressDal user_addressDal{get;}
	
		Iuser_msg_recordDal user_msg_recordDal{get;}
	
		Iuser_productDal user_productDal{get;}
	
		Iuser_score_recordDal user_score_recordDal{get;}
	
		Iwx_request_contentDal wx_request_contentDal{get;}
	
		Iwx_request_ruleDal wx_request_ruleDal{get;}
	
		Iwx_userDal wx_userDal{get;}
	}	
}