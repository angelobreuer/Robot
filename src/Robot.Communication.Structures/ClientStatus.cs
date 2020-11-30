namespace Robot.Communication.Structures
{
    public enum ClientStatus : byte
    {
        /// <summary>
        ///     Denotes that the client is currently initializing.
        /// </summary>
        Init,

        /// <summary>
        ///     Denotes that the client is in idle.
        /// </summary>
        Idle,

        /// <summary>
        ///     Denotes that the client is under load.
        /// </summary>
        Load,
    }
}
