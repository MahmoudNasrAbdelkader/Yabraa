using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using YabraaEF.Models;

namespace YabraaEF
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
        {
        }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public virtual DbSet<Service> Services { get; set; }
        public virtual DbSet<Package> Packages { get; set; }
        public virtual DbSet<PackageItem> PackageItems { get; set; }
        public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<VisitDetails> VisitDetails { get; set; }
        public virtual DbSet<Invoice> Invoices { get; set; }
        public virtual DbSet<InvoiceDetails> InvoiceDetails { get; set; }
        public virtual DbSet<Country> Countries { get; set; }
        public virtual DbSet<FamilyRelationship> FamilyRelationships { get; set; }
        public virtual DbSet<UserFamily> UserFamilies { get; set; }
        public virtual DbSet<Gallery> Gallery { get; set; }
        public virtual DbSet<StartPages> StartPages { get; set; }
        public virtual DbSet<VisitStatus> VisitStatuses { get; set; }
        public virtual DbSet<VisitAttachments> VisitAttachments { get; set; }
        public virtual DbSet<VisitNotes> VisitNotes { get; set; }
        public virtual DbSet<ServiceType> ServiceTypes { get; set; }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Data Source=SQL5063.site4now.net;Initial Catalog=db_a95d57_yabraa;User Id=db_a95d57_yabraa_admin;Password=Aa@938462;");
                // optionsBuilder.UseSqlServer("Data Source=SQL5105.site4now.net;Initial Catalog=db_a95d57_yabraatest;User Id=db_a95d57_yabraatest_admin;Password=Aa@938462;");
                // optionsBuilder.UseSqlServer("Data Source=.;Initial Catalog=db_a95d57_yabraa;Integrated Security=True");

            }
         
        }
        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
            
        //    modelBuilder.Ignore<VisitDetails>();
        //}
    }
}
