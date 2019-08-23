namespace com.b_velop.XmlRpc.Models
{
    public class Point
    {
        public string Id { get; set; }
        public string ExternId { get; set; }
    }
    public class ActiveMeasurePoint
    {
        public string Id { get; set; }
        public bool IsActive { get; set; }
        public Point Point { get; set; }
        public string ExternId => Point?.ExternId;
        public string HomematicId { get 
        {
                var idx = Point.ExternId.LastIndexOf(':');
                if(idx > 0)
                {
                    return Point.ExternId.Substring(0, idx);
                }
                return Point.ExternId;
            }
        }
    }
}
