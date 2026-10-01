namespace CreaturesStarsWatcher.Detection
{
    /// <summary>
    /// Receives the alerts produced by the detection pipeline.
    /// This is the extension point of the mod: adding a chat message, a sound or a
    /// minimap marker only requires another <see cref="IAlertSink"/> implementation.
    /// </summary>
    public interface IAlertSink
    {
        void Enqueue(CreatureAlert alert);
    }
}
