namespace DnrGps_ArcMap
{
    /// <summary>
    /// Describes how to interpret a string referencing to a GIS data set 
    /// </summary>
    public enum GisDataType
    {
        /// <summary>
        /// ESRI Shapefile, data provided will be a string containing the path to OS folder
        /// </summary>
        Shapefile,
        /// <summary>
        /// ESRI File Geodatabase, data provided will be a string containing the path to OS folder
        /// containing FGDB (do not append a featuredataset)
        /// </summary>
        FileGeodatabase,
        /// <summary>
        /// ESRI SDE connection string, data provided will be the connection string
        /// as a semicolon (;) separated collection of name=value tuples.
        /// See http://help.arcgis.com/en/sdk/10.0/arcobjects_net/conceptualhelp/index.html#/Connecting_to_a_geodatabase/0001000003s8000000/
        /// for valid names and values.
        /// </summary>
        SdeConnectionString,
    }
}
