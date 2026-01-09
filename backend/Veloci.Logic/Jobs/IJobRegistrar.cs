namespace Veloci.Logic.Jobs;

/// <summary>
/// Interface for vertical slice features to register their recurring Hangfire jobs.
/// Implement in your feature's Jobs folder and register as scoped service.
/// </summary>
public interface IJobRegistrar
{
    /// <summary>
    /// Registers all recurring Hangfire jobs for this feature.
    /// </summary>
    void RegisterJobs();
}