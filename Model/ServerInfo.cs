using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class ServerInfo
    {
        //服务器系统
        public string ServiveSystem { get; set; }
        //服务器时间 
        public string ServiceTime { get; set; }
        //网站端口
        public string ServicePort { get; set; }
        //网站绝对路径
        public string ServicePath { get; set; }
        //服务器.NET版本
        public string ServiceNetVersion { get; set; }
        //服务器IP
        public string ServiceIP { get; set; }
        //服务器IIS版本
        public string ServiceIIS { get; set; }
        //服务器名称
        public string MachineName { get; set; }
        //服务器占用空间大小
        public string GetSystemLength { get; set; }
        //数据库大小
        public string DataBaseLength { get; set; }

    }
}
