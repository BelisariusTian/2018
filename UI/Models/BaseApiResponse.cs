using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UI.Models
{
    public class BaseApiResponse
    {
        /// <summary>
        /// 响应消息
        /// </summary>
        public string msg { get; set; }
        /// <summary>
        /// 状态码
        /// </summary>
        public Enum code { get; set; }
        /// <summary>
        /// 响应数据
        /// </summary>
        public object data { get; set; }
        /// <summary>
        /// 数据总条数
        /// </summary>
        public int count { get; set; }
    }
}