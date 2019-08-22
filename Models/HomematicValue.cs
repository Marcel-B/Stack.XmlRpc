using System;

namespace com.b_velop.XmlRpc.Models
{
    public class HomematicValue
    {
        public DateTimeOffset Time { get; set; }
        public string Instance { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string Type { get; set; }
        public override string ToString()
        {
            return $"{Time}\n  {Id}\n   {Name}\n   {Value} ({Type})";
        }
    }
}
