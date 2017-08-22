namespace DnrGps_ArcMap
{
    /// <summary>
    /// Describe how historic (previous) GPS points are displayed
    /// </summary>
    public enum BreadCrumbs
    {
        /// <summary>
        /// Old GPS points are removed from the display, only the current point is drawn
        /// </summary>
        None,
        /// <summary>
        /// Old GPS points are scaled to a smaller size (as specified by the Defaults)
        /// </summary>
        SmallSymbols,
        /// <summary>
        /// Old GPS points are removed, and replaced with a line connnecting adjacent points.
        /// </summary>
        Lines
    }
}