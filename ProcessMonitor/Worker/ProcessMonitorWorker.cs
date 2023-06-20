using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ProcessMonitor.Tests")]
namespace ProcessMonitor.Worker;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProcessMonitor.Interface;
using System.Diagnostics;

/// <summary>
/// Worker.
/// </summary>
/// <seealso cref="ProcessMonitor.Interface.IWorker" />
internal class ProcessMonitorWorker : IWorker
{
    /// <summary>
    /// The input thread
    /// </summary>
    private readonly Thread inputThread;
    /// <summary>
    /// The get input
    /// </summary>
    private readonly AutoResetEvent getInput, gotInput;
    /// <summary>
    /// The logger
    /// </summary>
    private readonly ILogger logger;
    /// <summary>
    /// The use defaults
    /// </summary>
    private bool useDefaults;

    /// <summary>
    /// The input
    /// </summary>
    private string input;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProcessMonitorWorker"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public ProcessMonitorWorker()
    {
        var configuration = new ConfigurationBuilder()
        .AddJsonFile($"appsettings.json").Build();

        this.useDefaults = bool.TryParse(configuration.GetSection("UseDefaults").Value, out bool value) ? value : true;

        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });
        this.logger = loggerFactory.CreateLogger<Program>();

        this.getInput = new AutoResetEvent(false);
        this.gotInput = new AutoResetEvent(false);
        this.inputThread = new Thread(reader);
        this.inputThread.IsBackground = true;
        this.inputThread.Start();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProcessMonitorWorker"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public ProcessMonitorWorker(bool Defaults)
    {
        this.useDefaults = Defaults;

        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });
        this.logger = loggerFactory.CreateLogger<Program>();

        this.getInput = new AutoResetEvent(false);
        this.gotInput = new AutoResetEvent(false);
        this.inputThread = new Thread(reader);
        this.inputThread.IsBackground = true;
        this.inputThread.Start();
    }


    /// <summary>
    /// Readers this instance.
    /// </summary>
    private void reader()
    {
        while (true)
        {
            getInput.WaitOne();
            logger.LogDebug($"Date - {DateTime.Now} - monitor awaiting input");
            input = Console.ReadLine();
            gotInput.Set();
        }
    }

    /// <summary>
    /// Reads the line and wait for q to exit
    /// </summary>
    /// <param name="timeOutMillisecs">The time out millisecs.</param>
    /// <returns></returns>
    private bool ReadLine(int timeOutMillisecs = Timeout.Infinite)
    {
        getInput.Set();
        bool success = gotInput.WaitOne(timeOutMillisecs);
        if (success && input.Equals(ConsoleKey.Q.ToString(), StringComparison.InvariantCultureIgnoreCase))
            return true;

        return false;
    }

    /// <summary>
    /// Executes the specified tuple.
    /// </summary>
    /// <param name="tuple">The tuple.</param>
    /// <param name="logger">The logger.</param>
    public void Execute((string ProcessName, double ProcessMaxLifetime, TimeSpan MonitoringSpan) tuple)
    {

        do
        {
            var process = Process.GetProcessesByName(tuple.ProcessName).FirstOrDefault();

            if (process is null)
            {
                logger.LogInformation($"Date - {DateTime.Now} No process to be killed, monitoring period: {(int)tuple.MonitoringSpan.TotalMinutes} min");
                continue;
            };

            TimeSpan runtime;
            DateTime beginWait = DateTime.Now;
            runtime = beginWait - process.StartTime;

            if (runtime.TotalMinutes >= tuple.ProcessMaxLifetime)
            {
                process.Kill();
                logger.LogInformation($"Date - {DateTime.Now} Process {tuple.ProcessName} killed, duration: {(int)runtime.TotalMinutes}, max-lifetime-allowed {tuple.ProcessMaxLifetime}");
            }
            else
            {
                logger.LogInformation($"Date - {DateTime.Now} Process {tuple.ProcessName} exists, Lifetime remaining(min): {(int)(tuple.ProcessMaxLifetime - runtime.TotalMinutes)}, max-lifetime-allowed {tuple.ProcessMaxLifetime} ");
            }

        } while (!ReadLine((int)tuple.MonitoringSpan.TotalMilliseconds));
    }

    //Check input values
    /// <summary>
    /// Validates the inputs.
    /// </summary>
    /// <param name="args">The arguments.</param>
    /// <param name="logger">The logger.</param>
    /// <returns></returns>
    public (string ProcessName, double ProcessMaxLifetime, TimeSpan MonitoringSpan) ValidateInputs(string[] args)
    {
        TimeSpan monitoringSpan = TimeSpan.Zero;
        double processMaxLifetime = 0;
        var processName = string.Empty;

        try
        {
            processName = args[0];
            processMaxLifetime = TimeSpan.FromMinutes(int.Parse(args[1])).TotalMinutes;
            monitoringSpan = TimeSpan.FromMinutes((int.Parse(args[2])));

        }
        catch (Exception ex)
        {
            if (string.IsNullOrEmpty(processName))
            {
                logger.LogError($"Date - {DateTime.Now} Invalid process name. Cannot be null or empty. Exception: {ex.Message}");
                throw;
            }

            if (processMaxLifetime == 0 && !useDefaults)
            {
                logger.LogError($"Date - {DateTime.Now} Invalid Lifetime value. Cannot be null or 0. Exception: {ex.Message}");
                throw;
            }
            else if (useDefaults)
            {
                processMaxLifetime = 5;
            }

            if (monitoringSpan == TimeSpan.Zero && !useDefaults)
            {
                logger.LogError($"Date - {DateTime.Now} Invalid monitoring period. Cannot be null or empty. Exception: {ex.Message}");
                throw;
            }
            else if (useDefaults)
            {
                monitoringSpan = TimeSpan.FromMinutes(1);
            }
        }

        return (processName, processMaxLifetime, monitoringSpan);
    }
}

