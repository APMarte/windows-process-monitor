using ProcessMonitor.Interface;
using ProcessMonitor.Worker;

// info
Console.WriteLine("To exit press 'q + enter'");

// Create new Worker
IWorker worker = new ProcessMonitorWorker();

try
{
    // Define defaults
    var tuple = worker.ValidateInputs(args);

    // Run
    worker.Execute(tuple);

}
catch (Exception)
{
    Environment.Exit(1);
}