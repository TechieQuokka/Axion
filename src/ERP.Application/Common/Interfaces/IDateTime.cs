namespace ERP.Application.Common.Interfaces
{
    /// <summary>
    /// Provides abstraction for accessing the current local and UTC date and time.
    /// Useful for testing and avoiding direct dependencies on DateTime.Now or DateTime.UtcNow.
    /// </summary>
    public interface IDateTime
    {
        /// <summary>
        /// Gets the current local time.
        /// </summary>
        DateTime Now { get; }

        /// <summary>
        /// Gets the current UTC time.
        /// </summary>
        DateTime UtcNow { get; }
    }
}