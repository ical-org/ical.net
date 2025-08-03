namespace Ical.Net.Logging
{
    /// <summary>
    /// Represents a type-specific logger.
    /// </summary>
    /// <typeparam name="T">The type for which the logger is created.</typeparam>
    internal interface ILogger<T> : ILogger // Make public when logging is used in library classes
    {
    }
}
