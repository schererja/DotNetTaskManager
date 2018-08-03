using System.Collections.Generic;

namespace Manager.Settings
{
    public class AppSettings
    {
        public string Title { get; set; }
        public List<Components> Components { get; set; }
    }

    public class Components
    {
        public string Name { get; set; }
        public Property Properties { get; set; }
    }

    public class Property
    {
        public bool Enabled { get; set; }
        public string IP { get; set; }
        public int Port { get; set; }
    }
}