﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace FileSystemWatcherService.Watchers;

public class SubfolderWatcher : FolderWatcher
{
    public SubfolderWatcher(EventLog eventLog, WatcherConfig config) : base(eventLog, config)
    {
    }

    protected override string GetKey(FileSystemEventArgs e)
    {
        var path = e.FullPath.Replace(Path, "");

        return path.Count(c => c == '\\') > 1
            ? path.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries).First()
            : null;
    }
}