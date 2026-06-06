using Microsoft.EntityFrameworkCore;

namespace ArchScope.Infrastructure.Persistence;

public class ArchScopeDbContext : DbContext
{
    public ArchScopeDbContext(DbContextOptions<ArchScopeDbContext> options) : base(options) { }

    public DbSet<AnalysisJobEntity> AnalysisJobs => Set<AnalysisJobEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AnalysisJobEntity>(entity =>
        {
            entity.HasKey(e => e.JobId);
            entity.HasIndex(e => e.CreatedAt);
            entity.Property(e => e.JobId).IsRequired();
            entity.Property(e => e.ProjectName).IsRequired();
            entity.Property(e => e.Status).IsRequired();
        });
    }
}
