namespace Robot.Management.Payloads
{
    using System.Text.Json.Serialization;

    public sealed class StartVideoPayload
    {
        [JsonPropertyName("ip")]
        public string IpAddress { get; set; }

        [JsonPropertyName("port")]
        public ushort Port { get; set; }
    }
}
