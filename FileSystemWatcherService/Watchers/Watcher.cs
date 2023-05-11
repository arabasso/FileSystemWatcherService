using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Runtime.Caching;

namespace FileSystemWatcherService.Watchers;

public abstract class Watcher : FileSystemWatcher
{
    private readonly MemoryCache _memoryCache;

    protected EventLog EventLog { get; }
    protected WatcherConfig Config { get; }

    protected Watcher(EventLog eventLog, WatcherConfig config) : base(config.Path, config.Filter)
    {
        _memoryCache = CreateCache();

        EventLog = eventLog;
        Config = config;

        IncludeSubdirectories = config.IncludeSubdirectories;
        NotifyFilter = (NotifyFilters)Enum.Parse(typeof(NotifyFilters), config.NotifyFilter);

        if (config.Changed)
        {
            Changed += WatcherEvent;
        }

        if (config.Renamed)
        {
            Renamed += WatcherEvent;
        }

        if (config.Created)
        {
            Created += WatcherEvent;
        }

        if (config.Deleted)
        {
            Deleted += WatcherEvent;
        }

        if (config.Error)
        {
            Error += WatcherError;
        }

        EnableRaisingEvents = true;
    }

    private MemoryCache CreateCache()
    {
        var assembly = typeof(CacheItemPolicy).Assembly;

        var type = assembly.GetType("System.Runtime.Caching.CacheExpires");

        if (type == null) return new MemoryCache("FastExpiringCache");

        var field = type.GetField("_tsPerBucket", BindingFlags.Static | BindingFlags.NonPublic);

        if (field == null || field.FieldType != typeof(TimeSpan)) return new MemoryCache("FastExpiringCache");

        var originalValue = (TimeSpan)field.GetValue(null);

        field.SetValue(null, TimeSpan.FromMilliseconds(1));

        var instance = new MemoryCache("FastExpiringCache");

        field.SetValue(null, originalValue);

        return instance;
    }

    protected abstract string GetKey(FileSystemEventArgs e);

    private void RemovedCallback(
        CacheEntryRemovedArguments args)
    {
        if (args.RemovedReason != CacheEntryRemovedReason.Expired) return;

        DispatchEvent(args.CacheItem.Value);
    }

    private void WatcherError(
        object sender,
        ErrorEventArgs e)
    {
        DispatchEvent(e);
    }

    private void WatcherEvent(
        object sender,
        FileSystemEventArgs e)
    {
        var key = GetKey(e);

        if (key == null) return;

        AddOrGetExistingCache(key, e);
    }

    protected virtual object AddOrGetExistingCache(string key, FileSystemEventArgs args)
    {
        var cacheItemPolicy = new CacheItemPolicy
        {
            RemovedCallback = RemovedCallback,
            SlidingExpiration = TimeSpan.FromMilliseconds(Config.Timeout),
        };

        return _memoryCache.AddOrGetExisting(key, args, cacheItemPolicy);
    }

    private void DispatchEvent(object obj)
    {
        try
        {
            using var ps = PowerShell.Create();

            ps.AddCommand("Set-ExecutionPolicy").AddArgument("Unrestricted");
            ps.Runspace.SessionStateProxy.SetVariable("event", obj);
            ps.AddScript(Config.Script);
            ps.Invoke();

            if (ps.HadErrors && ps.InvocationStateInfo?.Reason != null)
            {
                throw new Exception("PowerShell Exception", ps.InvocationStateInfo.Reason);
            }
        }

        catch (Exception ex)
        {
            EventLog.WriteEntry(ex.ToString(), EventLogEntryType.Error);
        }
    }

    protected override void Dispose(bool disposing)
    {
        _memoryCache?.Dispose();

        base.Dispose(disposing);
    }
}