//------------------------------------------------------------------------------
// <auto-generated>
//    此代码是根据模板生成的。
//
//    手动更改此文件可能会导致应用程序中发生异常行为。
//    如果重新生成代码，则将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class action
    {
        public action()
        {
            this.role = new HashSet<role>();
        }
    
        public int id { get; set; }
        public string url { get; set; }
        public string http_method { get; set; }
        public string action_icon { get; set; }
        public string action_name { get; set; }
        public int type { get; set; }
        public Nullable<int> pid { get; set; }
        public int sort { get; set; }
        public int state { get; set; }
        public int del_flag { get; set; }
        public string remark { get; set; }
        public System.DateTime add_time { get; set; }
        public System.DateTime mod_time { get; set; }
        public string add_admin { get; set; }
    
        public virtual ICollection<role> role { get; set; }
    }
}