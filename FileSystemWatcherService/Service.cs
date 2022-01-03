using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.ServiceProcess;

namespace FileSystemWatcherService
{
    public partial class Service :
        ServiceBase
    {
        private readonly WatchersConfig _config;
        private readonly Dictionary<FileSystemWatcher, WatcherConfig> _watchers = new();

        public Service(
            WatchersConfig config)
        {
            _config = config;

            InitializeComponent();
        }

        protected override void OnStart(
            string[] args)
        {
            foreach (var config in _config.Watchers)
            {
                try
                {
                    var watcher = new FileSystemWatcher(config.Path, config.Filter)
                    {
                        IncludeSubdirectories = config.IncludeSubdirectories,
                        NotifyFilter = (NotifyFilters)Enum.Parse(typeof(NotifyFilters), config.NotifyFilter),
                    
                    };

                    if (config.Changed)
                    {
                        watcher.Changed += WatcherEvent;
                    }

                    if (config.Renamed)
                    {
                        watcher.Renamed += WatcherEvent;
                    }

                    if (config.Created)
                    {
                        watcher.Created += WatcherEvent;
                    }

                    if (config.Deleted)
                    {
                        watcher.Deleted += WatcherEvent;
                    }

                    if (config.Error)
                    {
                        watcher.Error += WatcherError;
                    }

                    watcher.EnableRaisingEvents = true;

                    _watchers.Add(watcher, config);

                    EventLog.WriteEntry($"Monitoring {config.NotifyFilter} - {config.Path}\\{config.Filter}", EventLogEntryType.Information);
                }

                catch (Exception e)
                {
                    EventLog.WriteEntry(e.Message, EventLogEntryType.Error);
                }
            }
        }

        private void WatcherError(
            object sender,
            ErrorEventArgs e)
        {
            DispatchEvent((FileSystemWatcher) sender, e);
        }

        private void WatcherEvent(
            object sender,
            FileSystemEventArgs e)
        {
            DispatchEvent((FileSystemWatcher) sender, e);
        }

        private void DispatchEvent(
            FileSystemWatcher watcher,
            object obj)
        {
            try
            {
                using var ps = PowerShell.Create();

                ps.AddCommand("Set-ExecutionPolicy").AddArgument("Unrestricted");
                ps.Runspace.SessionStateProxy.SetVariable("event", obj);
                ps.AddScript(_watchers[watcher].Script);
                ps.Invoke();
            }

            catch (Exception ex)
            {
                EventLog.WriteEntry(ex.Message, EventLogEntryType.Error);
            }
        }

        protected override void OnStop()
        {
            foreach (var watcher in _watchers)
            {
                watcher.Key.Dispose();
            }
        }
    }
}
