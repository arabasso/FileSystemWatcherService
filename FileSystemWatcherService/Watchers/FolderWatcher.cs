using System.Diagnostics;
using System.IO;
using System.Linq;

namespace FileSystemWatcherService.Watchers;

public class FolderWatcher : Watcher
{
    private readonly FileSystemEventArgsComparer _comparer = new();

    public FolderWatcher(EventLog eventLog, WatcherConfig config) : base(eventLog, config)
    {
    }

    protected override string GetKey(FileSystemEventArgs e)
    {
        return Path;
    }

    protected override object AddOrGetExistingCache(string key, FileSystemEventArgs e)
    {
        var args = new FileSystemEventListArgs( Path, key);

        args = (FileSystemEventListArgs)base.AddOrGetExistingCache(key, args) ?? args;

        if (!args.List.Contains(e, _comparer))
        {
            args.List.Add(e);
        }

        return args;
    }
}