namespace Robot.Management
{
    public readonly struct Payload
    {
        public Payload(OpCode opCode, object? data)
        {
            OpCode = opCode;
            Data = data;
        }

        public object? Data { get; }

        public OpCode OpCode { get; }
    }
}
