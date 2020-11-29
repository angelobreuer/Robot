namespace Robot.Communication.Structures
{
    public unsafe struct EstablishPayload
    {
        public fixed byte MagicSequence[6]; // RbtClt
    }
}
