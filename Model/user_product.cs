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
    
    public partial class user_product
    {
        public int id { get; set; }
        public int user_id { get; set; }
        public int product_id { get; set; }
        public System.DateTime add_time { get; set; }
        public Nullable<System.DateTime> end_time { get; set; }
        public int state { get; set; }
        public Nullable<int> order_id { get; set; }
    
        public virtual product product { get; set; }
        public virtual user user { get; set; }
    }
}
