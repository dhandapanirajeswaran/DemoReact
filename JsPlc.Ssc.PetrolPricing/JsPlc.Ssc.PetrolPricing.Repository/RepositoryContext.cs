using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Common;
using JsPlc.Ssc.PetrolPricing.Models;

namespace JsPlc.Ssc.PetrolPricing.Repository
{
    public class RepositoryContext:DbContext,IRepositoryContext
    {
        public IDbSet<Site> Sites { get; set; }
        public IDbSet<SiteToCompetitor> SiteToCompetitors { get; set; }

        public RepositoryContext() : base("name=PetrolPricingRepository") { }

        public RepositoryContext(DbConnection connection) : base(connection, true) { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();

            modelBuilder.Entity<SiteToCompetitor>().HasRequired(m => m.Site).WithMany().HasForeignKey(m => m.SiteId);
            modelBuilder.Entity<SiteToCompetitor>().HasRequired(m => m.Competitor).WithMany().HasForeignKey(m => m.CompetitorId);
        }
    }
}
