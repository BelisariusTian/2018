using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
namespace Common.Log
{
    public class Log4NetWriter:ILogWriter
    {
        public void WriteLogInfo(string txt)
        {
           
            ILog logWriter = log4net.LogManager.GetLogger("guest");
            logWriter.Error(txt);
        }
    }
}
