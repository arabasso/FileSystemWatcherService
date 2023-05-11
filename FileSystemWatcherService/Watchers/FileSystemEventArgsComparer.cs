using System.Collections.Generic;
using System.IO;

namespace FileSystemWatcherService.Watchers;

public class FileSystemEventArgsComparer : IEqualityComparer<FileSystemEventArgs>
{
    public bool Equals(FileSystemEventArgs x, FileSystemEventArgs y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.ChangeType == y.ChangeType && x.FullPath == y.FullPath && x.Name == y.Name;
    }

    public int GetHashCode(FileSystemEventArgs obj)
    {
        unchecked
        {
            var hashCode = (int)obj.ChangeType;
            hashCode = (hashCode * 397) ^ (obj.FullPath != null ? obj.FullPath.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (obj.Name != null ? obj.Name.GetHashCode() : 0);
            return hashCode;
        }
    }
}