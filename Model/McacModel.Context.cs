﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class mcaaEntities : DbContext
    {
        public mcaaEntities()
            : base("name=mcaaEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<action> action { get; set; }
        public DbSet<administrator> administrator { get; set; }
        public DbSet<banner> banner { get; set; }
        public DbSet<config_rule> config_rule { get; set; }
        public DbSet<msg_log> msg_log { get; set; }
        public DbSet<order> order { get; set; }
        public DbSet<product> product { get; set; }
        public DbSet<role> role { get; set; }
        public DbSet<system_log> system_log { get; set; }
        public DbSet<system_msg_record> system_msg_record { get; set; }
        public DbSet<user> user { get; set; }
        public DbSet<user_address> user_address { get; set; }
        public DbSet<user_msg_record> user_msg_record { get; set; }
        public DbSet<user_product> user_product { get; set; }
        public DbSet<user_score_record> user_score_record { get; set; }
        public DbSet<wx_request_content> wx_request_content { get; set; }
        public DbSet<wx_request_rule> wx_request_rule { get; set; }
        public DbSet<wx_user> wx_user { get; set; }
    }
}