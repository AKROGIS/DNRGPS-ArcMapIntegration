using System.Runtime.InteropServices;

/*
 * If the map does not have a coordinate system, should I set it to WGS84, instead of returning an error?
 * Need to protect against bad defaults
 * Should I wrap all methods in a try catch, to ensure that an error in this code will not crash arcMap?
 * Create a NamedLayer object to return instead of string[]
 * Implement all shapes to WKT
 * Add pan/refresh api
 * Refresh() is separate from the Draw and Clear methods, so that they can be combined without multiple refreshes
 */

namespace DNRGarmin_ArcMap
{
    [Guid("4F135A30-DBF2-4511-8091-0B0BA261F23A")]
    public interface IDNRGarminController
    {
        /// <summary>
        /// The reference to the ArcMap Document (the world this object lives in).
        /// </summary>
        /// <remarks>
        /// This must be set correctly by the user, and not changed or else disaster will strike.
        /// This object should run in the ArcMap process, that is it is created by the ArcMap COM object factory.
        /// The code that calls the object factory, has the reference to ArcMap Document.
        /// </remarks>
        ESRI.ArcGIS.ArcMapUI.IMxDocument MxDocument { get; set; }

        /// <summary>
        /// The set of defaults used by this object
        /// </summary>
        /// <remarks>
        /// A default Defaults object will be created if one is not provided.
        /// If one is provided, then it is the user's responsibility to ensure that
        /// all the properties of the Defaults object are valid and appropriate.
        /// *** There is currently no error checking on the Defaults properties ***
        /// </remarks>
        Defaults Defaults { get; set; }

        //Naming conventions is dataFrame +':' + group1 + '/' + group2... + '/' + name
        //If there is only one dataframe in the map, then dataFrame +':' is omitted
        //layer groups can be nested arbitrarily deep, so the path will be arbitrarily long
        //If a layer is not in a layer group, then only the layer name is provided.
        string[] GetFeatureLayerNames();

        //layerName has a specific format.  To ensure correct formatting it
        //should be one of the strings returned by GetFeatureLayerNames()
        //Will return null if layerName is not found
        //If there are more than one layers with the same name, the first found is returned
        //By the layernaming conventions, the name should be unique, however no guarantee.
        System.Data.DataTable GetFeatureLayerData(string layerName);

        /// <summary>
        /// Adds an arrow marker to a special graphics layer in the focus map.
        /// </summary>
        /// <param name="latitude">The latitude in WGS84 decimal degrees -90(south) to +90 (north)</param>
        /// <param name="longitude">The longitude in WGS84 decimal degrees -180(west) to +180 (east)</param>
        /// <param name="direction">The direction of travel.  Angle in degrees with 0 = North and increasing clockwise to 360.</param>
        /// <param name="breadCrumbs">How the trail of historic points should be displayed</param>
        /// <returns>
        /// An integer used to identify this graphic
        /// -1 is returned if there is no map document
        /// -2 is returned if the graphic layer cannot be found/created
        /// -3 is returned if the map does not have a well defined coordinate system (i.e. lat/long cannot be projected onto the map)
        /// </returns>
        int DrawGpsPoint(double latitude, double longitude, double direction, BreadCrumbs breadCrumbs);

        /// <summary>
        /// Adds a graphic group (point marker and circles) to a special graphics layer in the focus map.
        /// </summary>
        /// <param name="latitude">The latitude in WGS84 decimal degrees -90(south) to +90 (north)</param>
        /// <param name="longitude">The longitude in WGS84 decimal degrees -180(west) to +180 (east)</param>
        /// <param name="radii">The radii (in meters) of the probability circles</param>
        /// <returns>
        /// An integer used to identify the newly created graphic group
        /// -1 is returned if there is no map document
        /// -2 is returned if the graphic layer cannot be found/created
        /// -3 is returned if the map does not have well defined linear units (i.e. no geographic CS)
        /// </returns>
        int DrawGpsCep(double latitude, double longitude, double[] radii);

        /// <summary>
        /// Redraws the map display after graphics have been drawn/cleared.  Optionally pans/rotates/zooms the display
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <param name="direction"></param>
        /// <param name="width"></param>
        /// <param name="behavior">How the map should be pan/zoom/rotate when it is refreshed</param>
        /// <remarks>
        /// Refreshes the entire display, even if only a small part changed.  This is slightly inefficient, but
        /// it saves the user from having to manage a refresh rectangle.
        /// </remarks>
        void RefreshDisplay(double latitude, double longitude, double direction, double width, PanBehavior behavior);

        /// <summary>
        /// Removes all graphics created by DrawGpsPoint() and DrawGpsCep() by removing the special graphics layer
        /// </summary>
        /// <remarks>
        /// User is responsible for refreshing the display after graphics are deleted.
        /// </remarks>
        void ClearGpsGraphics();

        /// <summary>
        /// Removes the specified graphic created by DrawGpsPoint() and DrawGpsCep()
        /// </summary>
        /// <remarks>
        /// No action if id is not recognized as a valid graphic.
        /// User is responsible for refreshing the display after graphics are deleted. 
        /// </remarks>
        /// <param name="id">The graphic id created by DrawGpsPoint() or DrawGpsCep()</param>
        void ClearGpsGraphics(int id);

        /// <summary>
        /// Removes an array of graphics created by DrawGpsPoint() and DrawGpsCep()
        /// </summary>
        /// <remarks>
        /// No action if ids is null. Any id is not recognized as a valid graphic is skipped.
        /// User is responsible for refreshing the display after graphics are deleted. 
        /// </remarks>
        /// <param name="ids">An array of graphic ids created by DrawGpsPoint() or DrawGpsCep()</param>
        void ClearGpsGraphics(int[] ids);
    }

    /// <summary>
    /// Describe how the map should pan when new GPS points are added
    /// </summary>
    public enum PanBehavior
    {
        /// <summary>
        /// Map does not pan, points may be invisible (drawn off the display)
        /// </summary>
        None,
        /// <summary>
        /// Map will pan when a point gets within 15% of the edge of the display
        /// </summary>
        PanAtEdge,
        /// <summary>
        /// Map will pan to center each new point.
        /// </summary>
        PanEveryPoint,
        /// <summary>
        /// Map will pan and rotate (direction of travel = up) for each new point.
        /// </summary>
        PanAndRotate,
    }
}
