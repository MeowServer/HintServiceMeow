namespace HintServiceMeow.Core.Enum
{
    public enum ResolutionOption
    {
        None,

        /// <summary>
        /// Give a offset to XCoordinate that push your hint to the edge of the screen if your hint has a alignment of left or right.
        /// </summary>
        Offset,

        /// <summary>
        /// Scale your XCoordinate according to screen aspect ratio, making your hint stays at the same relative position to the center of the screen.
        /// </summary>
        Scale,

        /// <summary>
        /// Give a offset to XCoordinate and scale it according to screen aspect ratio. This method combined Offset and Scale options.
        /// </summary>
        OffsetAndScale,
    }
}
