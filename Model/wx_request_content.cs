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
    
    public partial class wx_request_content
    {
        public string req_title { get; set; }
        public string req_content { get; set; }
        public string detail_url { get; set; }
        public string pic_url { get; set; }
        public string media_url { get; set; }
        public string remark { get; set; }
        public int sort { get; set; }
        public System.DateTime add_time { get; set; }
        public int rule_id { get; set; }
        public int type_msg { get; set; }
        public int id { get; set; }
    
        public virtual wx_request_rule wx_request_rule { get; set; }
    }
}