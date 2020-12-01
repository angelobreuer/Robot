namespace Robot.Server.Communication.Payloads
{
    public unsafe struct EstablishPayload
    {
        public fixed byte MagicSequence[6]; // RbtClt
    }
}
