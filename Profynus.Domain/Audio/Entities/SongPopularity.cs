namespace Profynus.Domain.Audio.Entities;

public class SongPopularity
{
    public Guid Id { get; set; }
    public Guid SongId { get; set; }
    public string Period { get; set; }
    public DateTimeOffset PeriodStart { get; set; }
    
    // Play counts
    public int PlayCount { get; set; }
    public int UniqueListeners { get; set; }
    public int CompletePlayCount { get; set; }
    public int SkipCount { get; set; }
    
    // Engagement
    public decimal AverageListenPlayCount { get; set; }
    public int TotalListenSecs { get; set; }
    
    // -- Derived score (can be used for sorting / recommendations)
    public decimal PopularityScore { get; set; }
    public DateTimeOffset ComputedAt { get; set; }
}