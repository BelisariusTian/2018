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
    
    public partial class msg_log
    {
        public System.Guid id { get; set; }
        public string send_name { get; set; }
        public string receive_name { get; set; }
        public Nullable<int> send_id { get; set; }
        public Nullable<int> receive_id { get; set; }
        public string msg_content { get; set; }
        public System.DateTime add_time { get; set; }
        public string msg_error_log { get; set; }
    }
}
