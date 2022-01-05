using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.ServiceProcess;
using System.Threading;

namespace FileSystemWatcherService
{
    public partial class Service :
        ServiceBase
    {
        private WatchersConfig _config;
        private readonly Dictionary<FileSystemWatcher, WatcherConfig> _watchers = new();
        private readonly FileSystemWatcher _appConfigWatcher;

        public Service()
        {
            var appConfigPath = Path.GetDirectoryName(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
            var appConfigFile = Path.GetFileName(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

            _config = (WatchersConfig)ConfigurationManager.GetSection("watchers");
            _appConfigWatcher = new FileSystemWatcher(appConfigPath ?? ".", appConfigFile)
            {
                EnableRaisingEvents = true,
            };

            _appConfigWatcher.Changed += AppConfigWatcherOnChanged;

            InitializeComponent();
        }

        private void AppConfigWatcherOnChanged(
            object sender,
            FileSystemEventArgs e)
        {
            EventLog.WriteEntry("Restarting watchers...", EventLogEntryType.Information);

            Thread.Sleep(500);

            ConfigurationManager.RefreshSection("watchers");

            _config = (WatchersConfig)ConfigurationManager.GetSection("watchers");

            Restart();
        }

        private void StartWatchers()
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

                    EventLog.WriteEntry($"Monitoring {config}", EventLogEntryType.Information);
                }

                catch (Exception e)
                {
                    EventLog.WriteEntry(e.Message, EventLogEntryType.Error);
                }
            }
        }

        private void StopWatchers()
        {
            foreach (var watcher in _watchers)
            {
                watcher.Key.Dispose();
            }
        }

        private void Restart()
        {
            StopWatchers();
            StartWatchers();
        }

        protected override void OnStart(
            string[] args)
        {
            StartWatchers();
        }

        private void WatcherError(
            object sender,
            ErrorEventArgs e)
        {
            DispatchEvent((FileSystemWatcher)sender, e);
        }

        private void WatcherEvent(
            object sender,
            FileSystemEventArgs e)
        {
            DispatchEvent((FileSystemWatcher)sender, e);
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

                if (ps.HadErrors)
                {
                    throw new Exception(string.Join(Environment.NewLine, ps.Streams.Error.Select(s => s.Exception.Message)));
                }
            }

            catch (Exception ex)
            {
                EventLog.WriteEntry(ex.Message, EventLogEntryType.Error);
            }
        }

        protected override void OnStop()
        {
            _appConfigWatcher.Dispose();

            StopWatchers();
        }
    }
}
