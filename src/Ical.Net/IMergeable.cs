namespace Ical.Net
{
    /// <inheritdoc cref="MergeWith"/>
    public interface IMergeable
        {
        /// <summary> Merges this object with another. </summary>
        void MergeWith(IMergeable obj);
    }
}