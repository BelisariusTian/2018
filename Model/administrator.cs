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
    
    public partial class administrator
    {
        public administrator()
        {
            this.system_msg_record = new HashSet<system_msg_record>();
            this.user_msg_record = new HashSet<user_msg_record>();
            this.role = new HashSet<role>();
        }
    
        public int id { get; set; }
        public string login_account { get; set; }
        public string login_pwd { get; set; }
        public string name { get; set; }
        public int state { get; set; }
        public string head_protrait { get; set; }
        public System.DateTime add_time { get; set; }
        public int del_flag { get; set; }
        public Nullable<System.DateTime> mod_time { get; set; }
        public string remark { get; set; }
        public int sort { get; set; }
        public Nullable<System.DateTime> last_login_time { get; set; }
        public string last_login_IP_address { get; set; }
    
        public virtual ICollection<system_msg_record> system_msg_record { get; set; }
        public virtual ICollection<user_msg_record> user_msg_record { get; set; }
        public virtual ICollection<role> role { get; set; }
    }
}