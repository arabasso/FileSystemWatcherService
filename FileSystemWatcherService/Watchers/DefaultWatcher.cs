using System.Diagnostics;
using System.IO;

namespace FileSystemWatcherService.Watchers;

public class DefaultWatcher : Watcher
{
    public DefaultWatcher(EventLog eventLog, WatcherConfig config) : base(eventLog, config)
    {
    }

    protected override string GetKey(FileSystemEventArgs e)
    {
        return $"{e.ChangeType}|{e.FullPath}|{e.Name}";
    }
}
