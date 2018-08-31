using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UI.Controllers
{
    public class MenuInfo
    {
        public int MenuID { get; set; }
        public string title { get; set; }
        public string href { get; set; }
        public int? ParentID { get; set; }
        public bool spread { get; set; }
        public string icon { get; set; }
        public object children { get; set; }
    }
}
