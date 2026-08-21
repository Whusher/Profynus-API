namespace Profynus.Domain.Provider.Entities;

public class ProviderRateLimits
{
    public Guid Id { get; set; }
    public string Provider  { get; set; }
    public string WindowType { get; set; }
    public DateTimeOffset WindowStart { get; set; }
    public int RequestCount { get; set; }
    public int LimitCapacity { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}