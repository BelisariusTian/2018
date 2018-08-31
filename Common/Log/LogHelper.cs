using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;


namespace Common.Log
{
    public class LogHelper
    {
        public static Queue<string> ExceptionStringQueue = new Queue<string>();
        public static List<ILogWriter> LogWriterList = new List<ILogWriter>();

        static LogHelper()
        {
            LogWriterList.Add(new Log4NetWriter());

            ThreadPool.QueueUserWorkItem(o =>
           {
               while (true)
               {
                   if (ExceptionStringQueue.Count > 0)
                   {
                       lock (ExceptionStringQueue)
                       {
                           string str = ExceptionStringQueue.Dequeue();
                           foreach (var item in LogWriterList)
                           {
                               item.WriteLogInfo(str);
                           }
                       }
                   }
                   else
                   {
                       Thread.Sleep(50);
                   }
               }
           });

        }

        public static void Writelog(string excetionText)
        {
            lock (ExceptionStringQueue)
            {
                ExceptionStringQueue.Enqueue(excetionText);
            }
        }

        public static void SetConfig()
        {
            log4net.Config.XmlConfigurator.Configure();
        }
    }
}
