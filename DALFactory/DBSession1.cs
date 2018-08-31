 
using DAL;
using IDAL;
using System.Data.Entity;

namespace DALFactory
{
	public partial class DBSession : IDBSession
    {
	
		private IactionDal _actionDal;
        public IactionDal actionDal
        {
            get
            {
                if(_actionDal == null)
                {
                    _actionDal = AbstractFactory.CreateactionDal();
                }
                return _actionDal;
            }
        }
	
		private IadministratorDal _administratorDal;
        public IadministratorDal administratorDal
        {
            get
            {
                if(_administratorDal == null)
                {
                    _administratorDal = AbstractFactory.CreateadministratorDal();
                }
                return _administratorDal;
            }
        }
	
		private IbannerDal _bannerDal;
        public IbannerDal bannerDal
        {
            get
            {
                if(_bannerDal == null)
                {
                    _bannerDal = AbstractFactory.CreatebannerDal();
                }
                return _bannerDal;
            }
        }
	
		private Iconfig_ruleDal _config_ruleDal;
        public Iconfig_ruleDal config_ruleDal
        {
            get
            {
                if(_config_ruleDal == null)
                {
                    _config_ruleDal = AbstractFactory.Createconfig_ruleDal();
                }
                return _config_ruleDal;
            }
        }
	
		private Imsg_logDal _msg_logDal;
        public Imsg_logDal msg_logDal
        {
            get
            {
                if(_msg_logDal == null)
                {
                    _msg_logDal = AbstractFactory.Createmsg_logDal();
                }
                return _msg_logDal;
            }
        }
	
		private IorderDal _orderDal;
        public IorderDal orderDal
        {
            get
            {
                if(_orderDal == null)
                {
                    _orderDal = AbstractFactory.CreateorderDal();
                }
                return _orderDal;
            }
        }
	
		private IproductDal _productDal;
        public IproductDal productDal
        {
            get
            {
                if(_productDal == null)
                {
                    _productDal = AbstractFactory.CreateproductDal();
                }
                return _productDal;
            }
        }
	
		private IroleDal _roleDal;
        public IroleDal roleDal
        {
            get
            {
                if(_roleDal == null)
                {
                    _roleDal = AbstractFactory.CreateroleDal();
                }
                return _roleDal;
            }
        }
	
		private Isystem_logDal _system_logDal;
        public Isystem_logDal system_logDal
        {
            get
            {
                if(_system_logDal == null)
                {
                    _system_logDal = AbstractFactory.Createsystem_logDal();
                }
                return _system_logDal;
            }
        }
	
		private Isystem_msg_recordDal _system_msg_recordDal;
        public Isystem_msg_recordDal system_msg_recordDal
        {
            get
            {
                if(_system_msg_recordDal == null)
                {
                    _system_msg_recordDal = AbstractFactory.Createsystem_msg_recordDal();
                }
                return _system_msg_recordDal;
            }
        }
	
		private IuserDal _userDal;
        public IuserDal userDal
        {
            get
            {
                if(_userDal == null)
                {
                    _userDal = AbstractFactory.CreateuserDal();
                }
                return _userDal;
            }
        }
	
		private Iuser_addressDal _user_addressDal;
        public Iuser_addressDal user_addressDal
        {
            get
            {
                if(_user_addressDal == null)
                {
                    _user_addressDal = AbstractFactory.Createuser_addressDal();
                }
                return _user_addressDal;
            }
        }
	
		private Iuser_msg_recordDal _user_msg_recordDal;
        public Iuser_msg_recordDal user_msg_recordDal
        {
            get
            {
                if(_user_msg_recordDal == null)
                {
                    _user_msg_recordDal = AbstractFactory.Createuser_msg_recordDal();
                }
                return _user_msg_recordDal;
            }
        }
	
		private Iuser_productDal _user_productDal;
        public Iuser_productDal user_productDal
        {
            get
            {
                if(_user_productDal == null)
                {
                    _user_productDal = AbstractFactory.Createuser_productDal();
                }
                return _user_productDal;
            }
        }
	
		private Iuser_score_recordDal _user_score_recordDal;
        public Iuser_score_recordDal user_score_recordDal
        {
            get
            {
                if(_user_score_recordDal == null)
                {
                    _user_score_recordDal = AbstractFactory.Createuser_score_recordDal();
                }
                return _user_score_recordDal;
            }
        }
	
		private Iwx_request_contentDal _wx_request_contentDal;
        public Iwx_request_contentDal wx_request_contentDal
        {
            get
            {
                if(_wx_request_contentDal == null)
                {
                    _wx_request_contentDal = AbstractFactory.Createwx_request_contentDal();
                }
                return _wx_request_contentDal;
            }
        }
	
		private Iwx_request_ruleDal _wx_request_ruleDal;
        public Iwx_request_ruleDal wx_request_ruleDal
        {
            get
            {
                if(_wx_request_ruleDal == null)
                {
                    _wx_request_ruleDal = AbstractFactory.Createwx_request_ruleDal();
                }
                return _wx_request_ruleDal;
            }
        }
	
		private Iwx_userDal _wx_userDal;
        public Iwx_userDal wx_userDal
        {
            get
            {
                if(_wx_userDal == null)
                {
                    _wx_userDal = AbstractFactory.Createwx_userDal();
                }
                return _wx_userDal;
            }
        }
	}	
}