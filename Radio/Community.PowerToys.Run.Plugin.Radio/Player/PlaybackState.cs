namespace Community.PowerToys.Run.Plugin.Radio.Player
{
    /// <summary>
    /// Represents the playback state of the radio player.
    /// </summary>
    public enum PlaybackState
    {
        /// <summary>
        /// No media loaded, player is idle.
        /// </summary>
        Idle,

        /// <summary>
        /// Media is loading/buffering.
        /// </summary>
        Loading,

        /// <summary>
        /// Media is currently playing.
        /// </summary>
        Playing,

        /// <summary>
        /// Playback is paused.
        /// </summary>
        Paused,

        /// <summary>
        /// An error occurred during playback.
        /// </summary>
        Error
    }
}
