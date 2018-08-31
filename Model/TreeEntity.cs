using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    /// <summary>
    /// 树形菜单类
    /// </summary>
    public class TreeEntity
    {
        public string eid { get; set; }
        public string epid { get; set; }
        public string name { get; set; }
        public bool spread { get; set; }
        public object children { get; set; }
        public string addtime { get; set; }
        public string remark { get; set; }
    }
}
