#  Windows Monitor!

Job - kill processes that run more than a defined lifetime.

# To Run

Publish: Dotnet publish on root

Fields:
- ProcessName - string
- MaxProcessLifetime - int (minutes)
- MonitorPeriod - int (minutes)

Example:
> .\ProcessMonitor.exe Notepad 5 1

## Configuration

To use MaxProcessLifetime and MonitorPeriod defaults, change flag **UseDefaults** on appsettings to true and run without set MaxProcessLifetime or MonitorPeriod.
> "UseDefaults" : true
> .\ProcessMonitor.exe Notepad


## Logger 
Log level configuration on appsettings.  
>"Default": "Information"

**Just log to console**


