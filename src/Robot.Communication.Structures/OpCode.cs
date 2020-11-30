namespace Robot.Communication.Structures
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
