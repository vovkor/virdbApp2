﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace virdbApp2.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class virdb2Entities : DbContext
    {
        public virdb2Entities()
            : base("name=virdb2Entities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<botanic> botanic { get; set; }
        public virtual DbSet<collsrc> collsrc { get; set; }
        public virtual DbSet<geography> geography { get; set; }
        public virtual DbSet<INSTITUT> INSTITUT { get; set; }
        public virtual DbSet<KRIS_EXP> KRIS_EXP { get; set; }
        public virtual DbSet<liffom> liffom { get; set; }
        public virtual DbSet<maindb> maindb { get; set; }
        public virtual DbSet<sampstat_full> sampstat_full { get; set; }
        public virtual DbSet<storage> storage { get; set; }
        public virtual DbSet<taxonomy_crop> taxonomy_crop { get; set; }
        public virtual DbSet<taxonomy_family> taxonomy_family { get; set; }
        public virtual DbSet<taxonomy_genus> taxonomy_genus { get; set; }
        public virtual DbSet<maindb_owner> maindb_owner { get; set; }
        public virtual DbSet<audit> audit { get; set; }
        public virtual DbSet<user_roles> user_roles { get; set; }
        public virtual DbSet<users> users { get; set; }
        public virtual DbSet<sd_kind> sd_kind { get; set; }
        public virtual DbSet<sd_status> sd_status { get; set; }
        public virtual DbSet<service_desk> service_desk { get; set; }
    }
}