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

        public string Type
        {
            set
            {
                if (value.Contains("i4"))
                    ValueType = typeof(int);
                else if (value.Contains("boolean"))
                    ValueType = typeof(bool);
                else if (value.Contains("double"))
                    ValueType = typeof(double);
                else
                    ValueType = typeof(object);
            }
        }

        public string AllId => $"{Id}:{Name}";
        public override string ToString()
        {
            return $"{Time}\n  {Id}\n   {Name}\n   {Value} ({ValueType})";
        }

        public Type ValueType { get; set; }
    }
}
