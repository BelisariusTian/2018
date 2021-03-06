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
    
    public partial class order
    {
        public order()
        {
            this.user_score_record = new HashSet<user_score_record>();
        }
    
        public int id { get; set; }
        public int user_id { get; set; }
        public System.DateTime add_time { get; set; }
        public int product_id { get; set; }
        public int count { get; set; }
        public decimal order_money { get; set; }
        public int pay_state { get; set; }
        public int order_state { get; set; }
        public string pay_account { get; set; }
        public string order_number { get; set; }
        public string refund_number { get; set; }
        public Nullable<System.DateTime> pay_time { get; set; }
        public string user_remark { get; set; }
        public string admin_remark { get; set; }
        public string wx_order_num { get; set; }
    
        public virtual product product { get; set; }
        public virtual user user { get; set; }
        public virtual ICollection<user_score_record> user_score_record { get; set; }
    }
}
