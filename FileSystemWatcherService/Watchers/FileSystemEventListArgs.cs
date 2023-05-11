using System.Collections.Generic;
using System.IO;

namespace FileSystemWatcherService.Watchers;

public class FileSystemEventListArgs : FileSystemEventArgs
{
    public List<FileSystemEventArgs> List { get; set; } = new();

    public FileSystemEventListArgs(string directory, string name) : base(WatcherChangeTypes.All, directory, name)
    {
    }
}