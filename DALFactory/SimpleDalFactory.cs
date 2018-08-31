 
using IDAL;
using System.Configuration;
using System.Reflection;

namespace DALFactory
{
    public partial class AbstractFactory
    {
      
   
		
	    public static IactionDal CreateactionDal()
        {
            string classFulleName = ConfigurationManager.AppSettings["DalNameSpace"] + ".actionDal";


            //object obj = Assembly.Load(ConfigurationManager.AppSettings["DalAssembly"]).CreateInstance(classFulleName, true);
            var obj  = CreateInstance(ConfigurationManager.AppSettings["DalAssemblyPath"], classFulleName);


            return obj as IactionDal;
        }
		
	    public static IadministratorDal CreateadministratorDal()
        {
            string classFulleName = ConfigurationManager.AppSettings["DalNameSpace"] + ".administratorDal";


            //object obj = Assembly.Load(ConfigurationManager.AppSettings["DalAssembly"]).CreateInstance(classFulleName, true);
            var obj  = CreateInstance(ConfigurationManager.AppSettings["DalAssemblyPath"], classFulleName);


            return obj as IadministratorDal;
        }
		
	    public static IbannerDal CreatebannerDal()
        {
            string classFulleName = ConfigurationManager.AppSettings["DalNameSpace"] + ".bannerDal";


            //object obj = Assembly.Load(ConfigurationManager.AppSettings["DalAssembly"]).CreateInstance(classFulleName, true);
            var obj  = CreateInstance(ConfigurationManager.AppSettings["DalAssemblyPath"], classFulleName);


            return obj as IbannerDal;
        }
		
	    public static Iconfig_ruleDal Createconfig_ruleDal()
        {
            string classFulleName = ConfigurationManager.AppSettings["DalNameSpace"] + ".config_ruleDal";


            //object obj = Assembly.Load(ConfigurationManager.AppSettings["DalAssembly"]).CreateInstance(classFulleName, true);
            var obj  = CreateInstance(ConfigurationManager.AppSettings["DalAssemblyPath"], classFulleName);


            return obj as Iconfig_ruleDal;
        }
		
	    public static Imsg_logDal Createmsg_logDal()
        {
            string classFulleName = ConfigurationManager.AppSettings["DalNameSpace"] + ".msg_logDal";


            //object obj = Assembly.Load(ConfigurationManager.AppSettings["DalAssembly"]).CreateInstance(classFulleName, true);
            var obj  = CreateInstance(ConfigurationManager.AppSettings["DalAssemblyPath"], classFulleName);


            return obj as Imsg_logDal;
        }
		
	    public static IorderDal CreateorderDal()
        {
            string classFulleName = ConfigurationManager.AppSettings["DalNameSpace"] + ".orderDal";


            //object obj = Assembly.Load(ConfigurationManager.AppSettings["DalAssembly"]).CreateInstance(classFulleName, true);
            var obj  = CreateInstance(ConfigurationManager.AppSettings["DalAssemblyPath"], classFulleName);


            return obj as IorderDal;
        }
		
	    public static IproductDal CreateproductDal()
        {
            string classFulleName = ConfigurationManager.AppSettings["DalNameSpace"] + ".productDal";


            //object obj = Assembly.Load(ConfigurationManager.AppSettings["DalAssembly"]).CreateInstance(classFulleName, true);
            var obj  = CreateInstance(ConfigurationManager.AppSettings["DalAssemblyPath"], classFulleName);


            return obj as IproductDal;
        }
		
	    public static IroleDal CreateroleDal()
        {
            string classFulleName = ConfigurationManager.AppSettings["DalNameSpace"] + ".roleDal";


            //object obj = Assembly.Load(ConfigurationManager.AppSettings["DalAssembly"]).CreateInstance(classFulleName, true);
            var obj  = CreateInstance(ConfigurationManager.AppSettings["DalAssemblyPath"], classFulleName);


            return obj as IroleDal;
        }
		
	    public static Isystem_logDal Createsystem_logDal()
        {
            string classFulleName = ConfigurationManager.AppSettings["DalNameSpace"] + ".system_logDal";


            //object obj = Assembly.Load(ConfigurationManager.AppSettings["DalAssembly"]).CreateInstance(classFulleName, true);
            var obj  = CreateInstance(ConfigurationManager.AppSettings["DalAssemblyPath"], classFulleName);


            return obj as Isystem_logDal;
        }
		
	    public static Isystem_msg_recordDal Createsystem_msg_recordDal()
        {
            string classFulleName = ConfigurationManager.AppSettings["DalNameSpace"] + ".system_msg_recordDal";


            //object obj = Assembly.Load(ConfigurationManager.AppSettings["DalAssembly"]).CreateInstance(classFulleName, true);
            var obj  = CreateInstance(ConfigurationManager.AppSettings["DalAssemblyPath"], classFulleName);


            return obj as Isystem_msg_recordDal;
        }
		
	    public static IuserDal CreateuserDal()
        {
            string classFulleName = ConfigurationManager.AppSettings["DalNameSpace"] + ".userDal";


            //object obj = Assembly.Load(ConfigurationManager.AppSettings["DalAssembly"]).CreateInstance(classFulleName, true);
            var obj  = CreateInstance(ConfigurationManager.AppSettings["DalAssemblyPath"], classFulleName);


            return obj as IuserDal;
        }
		
	    public static Iuser_addressDal Createuser_addressDal()
        {
            string classFulleName = ConfigurationManager.AppSettings["DalNameSpace"] + ".user_addressDal";


            //object obj = Assembly.Load(ConfigurationManager.AppSettings["DalAssembly"]).CreateInstance(classFulleName, true);
            var obj  = CreateInstance(ConfigurationManager.AppSettings["DalAssemblyPath"], classFulleName);


            return obj as Iuser_addressDal;
        }
		
	    public static Iuser_msg_recordDal Createuser_msg_recordDal()
        {
            string classFulleName = ConfigurationManager.AppSettings["DalNameSpace"] + ".user_msg_recordDal";


            //object obj = Assembly.Load(ConfigurationManager.AppSettings["DalAssembly"]).CreateInstance(classFulleName, true);
            var obj  = CreateInstance(ConfigurationManager.AppSettings["DalAssemblyPath"], classFulleName);


            return obj as Iuser_msg_recordDal;
        }
		
	    public static Iuser_productDal Createuser_productDal()
        {
            string classFulleName = ConfigurationManager.AppSettings["DalNameSpace"] + ".user_productDal";


            //object obj = Assembly.Load(ConfigurationManager.AppSettings["DalAssembly"]).CreateInstance(classFulleName, true);
            var obj  = CreateInstance(ConfigurationManager.AppSettings["DalAssemblyPath"], classFulleName);


            return obj as Iuser_productDal;
        }
		
	    public static Iuser_score_recordDal Createuser_score_recordDal()
        {
            string classFulleName = ConfigurationManager.AppSettings["DalNameSpace"] + ".user_score_recordDal";


            //object obj = Assembly.Load(ConfigurationManager.AppSettings["DalAssembly"]).CreateInstance(classFulleName, true);
            var obj  = CreateInstance(ConfigurationManager.AppSettings["DalAssemblyPath"], classFulleName);


            return obj as Iuser_score_recordDal;
        }
		
	    public static Iwx_request_contentDal Createwx_request_contentDal()
        {
            string classFulleName = ConfigurationManager.AppSettings["DalNameSpace"] + ".wx_request_contentDal";


            //object obj = Assembly.Load(ConfigurationManager.AppSettings["DalAssembly"]).CreateInstance(classFulleName, true);
            var obj  = CreateInstance(ConfigurationManager.AppSettings["DalAssemblyPath"], classFulleName);


            return obj as Iwx_request_contentDal;
        }
		
	    public static Iwx_request_ruleDal Createwx_request_ruleDal()
        {
            string classFulleName = ConfigurationManager.AppSettings["DalNameSpace"] + ".wx_request_ruleDal";


            //object obj = Assembly.Load(ConfigurationManager.AppSettings["DalAssembly"]).CreateInstance(classFulleName, true);
            var obj  = CreateInstance(ConfigurationManager.AppSettings["DalAssemblyPath"], classFulleName);


            return obj as Iwx_request_ruleDal;
        }
		
	    public static Iwx_userDal Createwx_userDal()
        {
            string classFulleName = ConfigurationManager.AppSettings["DalNameSpace"] + ".wx_userDal";


            //object obj = Assembly.Load(ConfigurationManager.AppSettings["DalAssembly"]).CreateInstance(classFulleName, true);
            var obj  = CreateInstance(ConfigurationManager.AppSettings["DalAssemblyPath"], classFulleName);


            return obj as Iwx_userDal;
        }
	}
	
}