﻿//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EIS.Console.DAL.Database
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class VendorsConfigEntities : DbContext
    {
        public VendorsConfigEntities()
            : base("name=VendorsConfigEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<productfileconfig> productfileconfigs { get; set; }
        public DbSet<productimage> productimages { get; set; }
        public DbSet<uploadstatu> uploadstatus { get; set; }
        public DbSet<vendorproduct> vendorproducts { get; set; }
        public DbSet<imagefileconfig> imagefileconfigs { get; set; }
        public DbSet<vendor> vendors { get; set; }
    }
}