using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

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

        [XmlAttribute("script")]
        public string ScriptAttribute { get; set; }

        [XmlText]
        public string ScriptText { get; set; }

        public string Script => ScriptText ?? File.ReadAllText(ScriptAttribute);
    }
}
