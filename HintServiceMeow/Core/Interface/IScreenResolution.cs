namespace HintServiceMeow.Core.Interface
{
    using System.ComponentModel;

    /// <summary>
    /// Represents a screen resolution's xy ratio.
    /// </summary>
    public interface IScreenResolution : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the value of the X coordinate divided by Y coordinate.
        /// </summary>
        /// <returns>A floating-point value representing the ratio of the X coordinate to the Y coordinate.</returns>
        float XyRatio { get; }
    }
}
