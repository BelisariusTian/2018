 
using IBLL;
using Model;

namespace BLL
{
	
	public partial class actionService :BaseService<action>,IactionService
    {
        public override void SetCurrentDal()
        {
            CurrentDal = this.GetDbSession.actionDal;
        }
    }   
	
	public partial class administratorService :BaseService<administrator>,IadministratorService
    {
        public override void SetCurrentDal()
        {
            CurrentDal = this.GetDbSession.administratorDal;
        }
    }   
	
	public partial class bannerService :BaseService<banner>,IbannerService
    {
        public override void SetCurrentDal()
        {
            CurrentDal = this.GetDbSession.bannerDal;
        }
    }   
	
	public partial class config_ruleService :BaseService<config_rule>,Iconfig_ruleService
    {
        public override void SetCurrentDal()
        {
            CurrentDal = this.GetDbSession.config_ruleDal;
        }
    }   
	
	public partial class msg_logService :BaseService<msg_log>,Imsg_logService
    {
        public override void SetCurrentDal()
        {
            CurrentDal = this.GetDbSession.msg_logDal;
        }
    }   
	
	public partial class orderService :BaseService<order>,IorderService
    {
        public override void SetCurrentDal()
        {
            CurrentDal = this.GetDbSession.orderDal;
        }
    }   
	
	public partial class productService :BaseService<product>,IproductService
    {
        public override void SetCurrentDal()
        {
            CurrentDal = this.GetDbSession.productDal;
        }
    }   
	
	public partial class roleService :BaseService<role>,IroleService
    {
        public override void SetCurrentDal()
        {
            CurrentDal = this.GetDbSession.roleDal;
        }
    }   
	
	public partial class system_logService :BaseService<system_log>,Isystem_logService
    {
        public override void SetCurrentDal()
        {
            CurrentDal = this.GetDbSession.system_logDal;
        }
    }   
	
	public partial class system_msg_recordService :BaseService<system_msg_record>,Isystem_msg_recordService
    {
        public override void SetCurrentDal()
        {
            CurrentDal = this.GetDbSession.system_msg_recordDal;
        }
    }   
	
	public partial class userService :BaseService<user>,IuserService
    {
        public override void SetCurrentDal()
        {
            CurrentDal = this.GetDbSession.userDal;
        }
    }   
	
	public partial class user_addressService :BaseService<user_address>,Iuser_addressService
    {
        public override void SetCurrentDal()
        {
            CurrentDal = this.GetDbSession.user_addressDal;
        }
    }   
	
	public partial class user_msg_recordService :BaseService<user_msg_record>,Iuser_msg_recordService
    {
        public override void SetCurrentDal()
        {
            CurrentDal = this.GetDbSession.user_msg_recordDal;
        }
    }   
	
	public partial class user_productService :BaseService<user_product>,Iuser_productService
    {
        public override void SetCurrentDal()
        {
            CurrentDal = this.GetDbSession.user_productDal;
        }
    }   
	
	public partial class user_score_recordService :BaseService<user_score_record>,Iuser_score_recordService
    {
        public override void SetCurrentDal()
        {
            CurrentDal = this.GetDbSession.user_score_recordDal;
        }
    }   
	
	public partial class wx_request_contentService :BaseService<wx_request_content>,Iwx_request_contentService
    {
        public override void SetCurrentDal()
        {
            CurrentDal = this.GetDbSession.wx_request_contentDal;
        }
    }   
	
	public partial class wx_request_ruleService :BaseService<wx_request_rule>,Iwx_request_ruleService
    {
        public override void SetCurrentDal()
        {
            CurrentDal = this.GetDbSession.wx_request_ruleDal;
        }
    }   
	
	public partial class wx_userService :BaseService<wx_user>,Iwx_userService
    {
        public override void SetCurrentDal()
        {
            CurrentDal = this.GetDbSession.wx_userDal;
        }
    }   
	
}