using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ProcessMonitor.Tests")]
namespace ProcessMonitor.Interface;

/// <summary>
/// Worker interface.
/// </summary>
internal interface IWorker
{
    /// <summary>
    /// Executes the specified tuple.
    /// </summary>
    /// <param name="tuple">The tuple.</param>
    /// <param name="logger">The logger.</param>
    void Execute((string ProcessName, double ProcessMaxLifetime, TimeSpan MonitoringSpan) tuple);

    /// <summary>
    /// Validates the inputs.
    /// </summary>
    /// <param name="args">The arguments.</param>
    /// <returns></returns>
    public (string ProcessName, double ProcessMaxLifetime, TimeSpan MonitoringSpan) ValidateInputs(string[] args);
}

