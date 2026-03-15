namespace HintServiceMeow.Core.Enum
{
    /// <summary>
    /// Defines priority levels used to determine the display order of hints.
    /// Higher numeric values indicate higher priority.
    /// </summary>
    public enum HintPriority
    {
        /// <summary>Highest display priority (192).</summary>
        Highest = 192,

        /// <summary>High display priority (160).</summary>
        High = 160,

        /// <summary>Medium display priority (128).</summary>
        Medium = 128,

        /// <summary>Low display priority (96).</summary>
        Low = 96,

        /// <summary>Lowest display priority (64).</summary>
        Lowest = 64,
    }

    /// <summary>
    /// Specifies the fallback behavior of a dynamic hint when no valid display position is available.
    /// </summary>
    public enum DynamicHintStrategy
    {
        /// <summary>
        /// Hides the dynamic hint when no position is available.
        /// </summary>
        Hide,

        /// <summary>
        /// Keeps the dynamic hint at its target position even when no valid position is available.
        /// </summary>
        StayInPosition,
    }

    /// <summary>
    /// Specifies the horizontal alignment of hint text.
    /// </summary>
    public enum HintAlignment
    {
        /// <summary>Aligns hint text to the left.</summary>
        Left,

        /// <summary>Aligns hint text to the right.</summary>
        Right,

        /// <summary>Centers hint text horizontally.</summary>
        Center,

        /// <summary>Justifies hint text to fill the entire width of the display area.</summary>
        Justified,

        Flush,
    }

    /// <summary>
    /// Specifies the vertical alignment of a hint within its display area.
    /// </summary>
    public enum HintVerticalAlign
    {
        /// <summary>Aligns the hint to the top of the display area.</summary>
        Top,

        /// <summary>Aligns the hint to the vertical middle of the display area.</summary>
        Middle,

        /// <summary>Aligns the hint to the bottom of the display area.</summary>
        Bottom,
    }

    /// <summary>
    /// Specifies how quickly a hint synchronizes its display updates relative to other hints.
    /// Higher numeric values indicate faster synchronization.
    /// </summary>
    public enum HintSyncSpeed
    {
        /// <summary>
        /// Fastest sync speed; the hint is updated as soon as possible.
        /// Using this speed may delay updates of other hints.
        /// </summary>
        Fastest = 192,

        /// <summary>
        /// Schedules an update immediately when the hint is modified.
        /// </summary>
        Fast = 160,

        /// <summary>
        /// Normal update speed.
        /// </summary>
        Normal = 128,

        /// <summary>
        /// Waits for other hints to update before syncing.
        /// </summary>
        Slow = 96,

        /// <summary>
        /// Waits longer than <see cref="Slow"/> before syncing.
        /// </summary>
        Slowest = 64,

        /// <summary>
        /// The hint does not automatically sync when updated.
        /// It will still be synced when other hints are updated.
        /// </summary>
        UnSync = 32,
    }
}