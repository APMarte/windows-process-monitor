using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

// info
Console.WriteLine("To exit press 'q + enter'");

// Define defaults
var tuple = ValidateInputs(args, logger);

// Run
Execute(tuple, logger);


// Implementation encapsulation
public partial class Program
{
    private static Thread inputThread;
    private static AutoResetEvent getInput, gotInput;
    private static string input;
    private static ILogger logger;
    private static bool useDefaults;

    static Program()
    {
        Setup();
        getInput = new AutoResetEvent(false);
        gotInput = new AutoResetEvent(false);
        inputThread = new Thread(reader);
        inputThread.IsBackground = true;
        inputThread.Start();
    }

    // Await input 
    private static void reader()
    {
        while (true)
        {
            getInput.WaitOne();
            logger.LogDebug($"Date - {DateTime.Now} - monitor awaiting");
            input = Console.ReadLine();
            gotInput.Set();
        }
    }

    static void Execute((string ProcessName, double ProcessMaxLifetime, TimeSpan MonitoringSpan) tuple, ILogger logger)
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

    // Await for q to exit
    static bool ReadLine(int timeOutMillisecs = Timeout.Infinite)
    {
        getInput.Set();
        bool success = gotInput.WaitOne(timeOutMillisecs);
        if (success && input.Equals(ConsoleKey.Q.ToString(), StringComparison.InvariantCultureIgnoreCase))
            return true;

        return false;
    }

    static void Setup()
    {
        var configuration = new ConfigurationBuilder()
        .AddJsonFile($"appsettings.json");

        var config = configuration.Build();
        useDefaults = bool.TryParse(config.GetSection("UseDefaults").Value, out bool value) ? value : true;

        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });

        logger = loggerFactory.CreateLogger<Program>();
    }

    //Check input values
    static (string ProcessName, double ProcessMaxLifetime, TimeSpan MonitoringSpan) ValidateInputs(string[] args, ILogger logger)
    {
        TimeSpan monitoringSpan = TimeSpan.Zero;
        double processMaxLifetime = 0;
        var processName = string.Empty;

        try
        {
            processName = args[0];

            if (int.TryParse(args[1], out var lifetime))
            {
                processMaxLifetime = TimeSpan.FromMinutes(lifetime).TotalMinutes;
            }

            if (int.TryParse(args[2], out var monitoringPeriod))
            {
                monitoringSpan = TimeSpan.FromMinutes(monitoringPeriod);
            }
        }
        catch (IndexOutOfRangeException)
        {
            if (string.IsNullOrEmpty(processName))
            {
                logger.LogError($"Date - {DateTime.Now} Invalid process name. Cannot be null or empty.");
                Environment.Exit(1);
            }

            if (processMaxLifetime == 0 && !useDefaults)
            {
                logger.LogError($"Date - {DateTime.Now} Invalid Lifetime value. Cannot be null or 0.");
                Environment.Exit(1);
            }
            else if (useDefaults)
            {
                processMaxLifetime = 5;
            }

            if (monitoringSpan == TimeSpan.Zero && !useDefaults)
            {
                logger.LogError($"Date - {DateTime.Now} Invalid monitoring period. Cannot be null or empty.");
                Environment.Exit(1);
            }
            else if (useDefaults)
            {
                monitoringSpan = TimeSpan.FromMinutes(1);
            }
        }

        return (processName, processMaxLifetime, monitoringSpan);

    }

}