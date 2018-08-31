using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Common
{
    public class ServerInfoHelper
    {
        #region 返回操作系统信息 .net版本 数据库大小  程序大小等方法

        /// <summary>
        /// 获取服务器系统信息
        /// </summary>
        public static string GetOSVersion()
        {

            OperatingSystem os = Environment.OSVersion;

            return os.ToString();

        }



        /// <summary>
        /// 获取服务器.net版本
        /// </summary>
        /// <returns></returns>
        public static string GetNetVersion()
        {

            return Environment.Version.ToString();

        }



        /// <summary>
        /// 获取数据库大小
        /// </summary>
        /// <returns></returns>
        public static string GetDataBaseLength()
        {

            string fileFullPath = HttpContext.Current.Server.MapPath("~/App_Data/biaochiDB.mdf");

            FileInfo file = new FileInfo(fileFullPath);

            return (Convert.ToDouble(file.Length) / 1024 / 1024).ToString("N") + "M";

        }



        /// <summary>
        /// 递归文件目录，返回目录下所有文件大小
        /// </summary>
        /// <param name="d">传入的路径</param>
        /// <returns></returns>
        public static long DirSize(DirectoryInfo d)
        {

            long Size = 0;

            // 所有文件大小.

            FileInfo[] fis = d.GetFiles();

            foreach (FileInfo fi in fis)
            {

                Size += fi.Length;

            }

            // 遍历出当前目录的所有文件夹.

            DirectoryInfo[] dis = d.GetDirectories();

            foreach (DirectoryInfo di in dis)
            {

                Size += DirSize(di);   //这就用到递归了，调用父方法,注意，这里并不是直接返回值，而是调用父返回来的

            }

            return (Size);

        }



        /// <summary>
        /// 调用DirSize方法
        /// </summary>
        /// <returns></returns>
        public static string GetDirSize()
        {

            string fullPath = HttpContext.Current.Server.MapPath("~/");

            DirectoryInfo d = new DirectoryInfo(fullPath);

            return (Convert.ToDouble(DirSize(d)) / 1024 / 1024).ToString("N") + "M";

        }

        #endregion



        #region 服务器相关属性

        /// <summary>
        /// 服务器名称
        /// </summary>
        public static string MachineName
        {

            get { return HttpContext.Current.Server.MachineName; }

        }



        /// <summary>
        /// 服务器操作系统
        /// </summary>
        public static string ServiveSystem
        {

            get { return GetOSVersion(); }

        }



        /// <summary>
        /// 服务器IP地址
        /// </summary>
        public static string ServiceIP
        {

            get { return HttpContext.Current.Request.ServerVariables["LOCAL_ADDR"]; }

        }



        /// <summary>
        /// 网站端口号
        /// </summary>
        public static string ServicePort
        {

            get { return HttpContext.Current.Request.ServerVariables["SERVER_PORT"]; }

        }



        /// <summary>
        /// 服务器IIS版本
        /// </summary>
        public static string ServiceIIS
        {

            get { return HttpContext.Current.Request.ServerVariables["SERVER_SOFTWARE"]; }

        }



        /// <summary>
        /// 服务器.NET解释引擎版本
        /// </summary>
        public static string ServiceNetVersion
        {

            get { return GetNetVersion(); }

        }



        /// <summary>
        /// 服务器时间
        /// </summary>
        public static string ServiceTime
        {

            get { return DateTime.Now.ToString(); }

        }



        /// <summary>
        /// 网站绝对路径
        /// </summary>
        public static string ServicePath
        {

            get { return HttpContext.Current.Request.ServerVariables["PATH_TRANSLATED"].ToString(); }

        }



        /// <summary>
        /// 数据库大小
        /// </summary>
        public static string DataBaseLength
        {

            get { return GetDataBaseLength(); }

        }



        /// <summary>
        /// 获取程序占用空间大小
        /// </summary>
        public static string GetSystemLength
        {

            get { return GetDirSize(); }

        }

        #endregion
    }
}
