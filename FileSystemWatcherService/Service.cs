using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Threading;

namespace FileSystemWatcherService
{
    public partial class Service :
        ServiceBase
    {
        private WatchersConfig _config;
        private readonly List<FileSystemWatcher> _watchers = new();
        private readonly FileSystemWatcher _appConfigWatcher;

        public Service()
        {
            var appConfigPath = Path.GetDirectoryName(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
            var appConfigFile = Path.GetFileName(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

            _config = (WatchersConfig)ConfigurationManager.GetSection("watchers");
            _appConfigWatcher = new FileSystemWatcher(appConfigPath ?? ".", appConfigFile)
            {
                NotifyFilter = NotifyFilters.LastWrite,
                EnableRaisingEvents = true,
            };

            _appConfigWatcher.Changed += AppConfigWatcherOnChanged;

            InitializeComponent();
        }

        private void AppConfigWatcherOnChanged(
            object sender,
            FileSystemEventArgs e)
        {
            StopWatchers();

            EventLog.WriteEntry("Restarting watchers...", EventLogEntryType.Information);

            Thread.Sleep(500);

            ConfigurationManager.RefreshSection("watchers");

            _config = (WatchersConfig)ConfigurationManager.GetSection("watchers");

            StartWatchers();
        }

        private void StartWatchers()
        {
            foreach (var config in _config.Watchers)
            {
                try
                {
                    var watcher = config.Create(EventLog);

                    _watchers.Add(watcher);

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
                watcher.Dispose();
            }

            _watchers.Clear();
        }

        protected override void OnStart(
            string[] args)
        {
            StartWatchers();
        }

        protected override void OnStop()
        {
            _appConfigWatcher.Dispose();

            StopWatchers();
        }
    }
}
