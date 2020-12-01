namespace Robot.Server.Communication.Payloads
{
    public enum OpCode : byte
    {
        Establish,
        SensorSync,
        Ping,
        Pong,
        Status,
    }
}
