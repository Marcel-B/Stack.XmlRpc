using Microsoft.Net.Http.Headers;

namespace com.b_velop.XmlRpc.Models
{
    public class Secrets
    {
        public string ClientId { get; set; }
        public string Scope { get; set; }
        public string Secret { get; set; }
        public string Issuer { get; set; }
        public string HomematicEndpoint { get; set; }
        public string InstanceName { get; set; }
        public string InstanceEndpoint { get; set; }
        public string AlarmLiving { get; set; }
        public string AlarmFloor { get; set; }
    }
}
