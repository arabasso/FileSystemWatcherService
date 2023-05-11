using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using FileSystemWatcherService.Watchers;

namespace FileSystemWatcherService
{
    [Serializable]
    [XmlRoot("watchers")]
    public class WatchersConfig :
        IConfigurationSectionHandler
    {
        [XmlElement("watcher")]
        public List<WatcherConfig> Watchers { get; set; }

        public object Create(
            object parent,
            object configContext,
            XmlNode section)
        {
            var serializer = new XmlSerializer(typeof(WatchersConfig));

            return serializer.Deserialize(new XmlNodeReader(section));
        }
    }

    [Serializable]
    public class WatcherConfig
    {
        [XmlAttribute("mode")]
        public string Mode { get; set; } = "default";

        [XmlAttribute("path")]
        public string Path { get; set; }

        [XmlAttribute("filter")]
        public string Filter { get; set; } = "*.*";

        [XmlAttribute("includeSubdirectories")]
        public bool IncludeSubdirectories { get; set; }

        [XmlAttribute("notifyFilter")]
        public string NotifyFilter { get; set; }

        [XmlAttribute("changed")]
        public bool Changed { get; set; }

        [XmlAttribute("renamed")]
        public bool Renamed { get; set; }

        [XmlAttribute("created")]
        public bool Created { get; set; }

        [XmlAttribute("deleted")]
        public bool Deleted { get; set; }

        [XmlAttribute("error")]
        public bool Error { get; set; }

        [XmlAttribute("timeout")]
        public int Timeout { get; set; } = 1000;

        [XmlAttribute("script")]
        public string ScriptAttribute { get; set; }

        [XmlText]
        public string ScriptText { get; set; }

        public string Script => ScriptText ?? File.ReadAllText(ScriptAttribute);

        public override string ToString()
        {
            var actions = new List<string>();

            if (Created)
            {
                actions.Add("Created");
            }

            if (Changed)
            {
                actions.Add("Changed");
            }

            if (Renamed)
            {
                actions.Add("Renamed");
            }

            if (Deleted)
            {
                actions.Add("Deleted");
            }

            return $"Mode: {Mode}; Path: {Path}; Filter: {Filter}; Action: {string.Join(",", actions)}; Notify Filter: {NotifyFilter}";
        }

        public FileSystemWatcher Create(EventLog eventLog)
        {
            return Mode.ToLower() switch
            {
                "folder" => new FolderWatcher(eventLog, this),
                "subfolder" => new SubfolderWatcher(eventLog, this),
                "default" => new DefaultWatcher(eventLog, this),
                "" => new DefaultWatcher(eventLog, this),
                _ => throw new NotImplementedException()
            };
        }
    }
}
