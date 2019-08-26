namespace com.b_velop.XmlRpc.Constants
{
    public class Strings
    {
        public static string MyUrl { get; set; }
        public static string InstanceId { get; set; }

        public const string Token = "Token";
        public const string Expiration = "Expiration";
        public const string ActiveMeasurePoints = "ActiveMeasurePoints";
        public const string LastUpload = "LastUpload";
        public const string LastActiveMeasurePointsPull = "LastActiveMeasurePointsPull";
        public const string AlarmActive = "AlarmActive";
        public const string AlarmIds = "AlarmIds";
        public const string Values = "Values";
        public const string MeasurePoints = "MeasurePoints";

        public static string Init => $"<?xml version=\"1.0\"?><methodCall><methodName>init</methodName><params><param><value><string>{MyUrl}</string></value></param><param><value><string>{InstanceId}</string></value></param></params></methodCall>";

        public static string DeInit = $"<?xml version=\"1.0\"?><methodCall><methodName>init</methodName><params><param><value><string>{MyUrl}</string></value></param><param><value><string></string></value></param></params></methodCall>";
        public static string LastConnection = "LastConnection";
    }
}
