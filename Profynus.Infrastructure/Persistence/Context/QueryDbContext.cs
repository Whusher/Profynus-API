using Microsoft.EntityFrameworkCore;
using Profynus.Domain.Auth.Entities;
using Profynus.Domain.User.Entities;
using Profynus.Domain.Audio.Entities;
using Profynus.Domain.Provider.Entities;
using Profynus.Domain.Subscription.Entities;
using Profynus.Domain.User.Entities;

namespace Profynus.Infrastructure.Persistence.Context;

public class QueryDbContext : DbContext
{
    public QueryDbContext(DbContextOptions<QueryDbContext> options) : base(options) {}

    public DbSet<User>       Users       => Set<User>();
    public DbSet<Credential> Credentials => Set<Credential>();
    public DbSet<Device>     Devices     => Set<Device>();
    public DbSet<Session>    Sessions    => Set<Session>();
    public DbSet<AuthEvent>  AuthEvents  => Set<AuthEvent>();
    public DbSet<MfaConfig>  MfaConfigs => Set<MfaConfig>();
    
    // Song platform specific typos
    public DbSet<DownloadQueue>  DownloadQueues => Set<DownloadQueue>();
    public DbSet<ListeningEvent>  ListeningEvents => Set<ListeningEvent>();
    public DbSet<ProviderRateLimits> ProviderRateLimits => Set<ProviderRateLimits>();
    public DbSet<Song> Songs => Set<Song>();
    public DbSet<SongPopularity> SongPopularities => Set<SongPopularity>();
    public DbSet<SongShareUrl> SongShareUrls => Set<SongShareUrl>();
    public DbSet<UserDownloadLimits> UserDownloadLimits => Set<UserDownloadLimits>();
    public  DbSet<UserSubscription> UserSubscriptions => Set<UserSubscription>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(QueryDbContext).Assembly);
    }
}