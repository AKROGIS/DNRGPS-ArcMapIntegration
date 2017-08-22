/*
 * Outstanding Issues:
 *   Need to check for exceptional conditions in all methods and provide meaningful exception messages.
 *   Need to finalize the API, remove obsolete methods, document parameters, returns, and exceptions
 *   Need to protect against bad defaults
 *   Need to come up with a clean way to support multiple versions of ArcMap
 *   When returning layer data, need to resolve how layers with no projection are handled: 1) throw exception and alert user to set projection, 2) Use the projection of the map (if set), 3) assume data is WGS84 (projection will throw an error if the data is not), 4) silently ignore the request 5)??? - If data has no projection, then map projection is used.  If map has no projection, then coords are returned as is (may or may not be valid wgs84 coordinates)
 * 
 * Bugs:
 *   In some cases (regan's 64bit win7 machine), the client application cannot connect to a running ArcMap process.
 * 
 * Bugs Waiting on ESRI:
 *   Need to resolve speed issues with cursors in GetFeatureLayerData() methods – ESRI support requested
 *   Need to resolve drawing issue with graphics – ESRI support requested
 *   If a layer has a selection set only the selected features should be returned.  Currently all features are being returned. This works when called in-process  – ESRI support requested
 * 
 * Testing:
 *   Need to test to see if multi-polygons are correctly supported. ArcGIS does not differentiate multi-ring polygons and multi-polygons. Each is represented as an array of 'parts' which are simple closed polygons.  A polygon is clockwise for outer, and counterclockwise for inner.  This is different from the Well Known Text Representation.  
 *   Need to test for layers with joined tables.
 *   Need to finish testing all ESRI field types correctly transferred to .Net types in the DataTable.
 *   Need to test setting different graphic defaults from client code
 *   No ability to set the linestyle or marker style
 *   Need to test Loading a SDE dataset
 * 
 * Potential Features:
 *   Support zooming or rotating with map refresh
 *   The column caption should use the field name alias in the layer properties, which may be changed by the user from the default of the alias set in the data source.
 *   When requesting specific fields, the field alias set in the layer properties is not honored. Only the field names/aliases specified in the dataset are searched.
 *   If the user has set fields to not visible in the layer properties, then those fields should not be returned.  Currently all fields in the dataset are returned regardless of the layer properties.
 */

using System;

namespace DnrGps_ArcMap
{
    //[System.Runtime.InteropServices.Guid("4F135A30-DBF2-4511-8091-0B0BA261F23A")]
    public interface IDnrGpsController
    {
        /// <summary>
        /// The reference to the ArcMap Document (the world this object lives in).
        /// </summary>
        /// <remarks>
        /// This must be set correctly by the user, and not changed or else disaster will strike.
        /// This object should run in the ArcMap process, i.e. it is created by the ArcMap COM object factory.
        /// The code that calls the object factory, has the reference to ArcMap Document.
        /// Ideally this would be a readonly construction parameter, but COM constructors are parameterless
        /// </remarks>
        ESRI.ArcGIS.ArcMapUI.IMxDocument MxDocument { get; set; }
        ESRI.ArcGIS.Framework.IObjectFactory ObjectFactory { get; set; }

        /// <summary>
        /// The default settings used by this object
        /// </summary>
        /// <remarks>
        /// A default Defaults object will be created if one is not provided.
        /// If one is provided, then it is the user's responsibility to ensure that
        /// all the properties of the Defaults object are valid and appropriate.
        /// Defaults are read when an object is created.  Therefore if the Defaults are
        /// changed, old graphics will not change, but new graphics will use the new settings.
        /// </remarks>
        Defaults Defaults { get; }


        /// <summary>
        /// Get names and indexes of the layers in the focus map of <see cref="IDnrGpsController.MxDocument"/>
        /// </summary>
        /// <returns>
        /// A dictionary (possibly empty) of fully qualified layer names (as strings).
        /// The dictionary keys are strings (hyphen separated integers for the group/layer indices).
        /// Note: Cannot marshall generic types across COM boundary
        /// </returns>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="MxDocument"/> is null.</exception>
        System.Collections.IDictionary GetFeatureLayers();
        System.Collections.IDictionary GetAllFeatureLayers();


        //Optional parameters could replace the following four methods with this one method,
        //System.Data.DataTable GetFeatureLayerData(string layerName = null, string[] fieldNames = null);
        //However optional parameters are not supported in COM interop.
        

        /// <summary>
        /// Gets the shape (in WGS84 well known text) and all attributes of the selected layer in the active data frame
        /// </summary>
        /// <returns>The layer data</returns>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="MxDocument"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if there is no selected layer in the focus map.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the selected layer is not a feature layer.</exception>
        System.Data.DataTable GetFeatureLayerData();

        /// <summary>
        /// Gets the shape (in WGS84 well known text) and all attributes of the specified layer in the active data frame
        /// </summary>
        /// <param name="indexes">
        /// A hyphenated string of 0 based integers for the group/layer indices of the requested layer.
        /// The dictionary returned by <see cref="GetFeatureLayers"/> uses these indices as keys.
        /// if indexes is null, then the selected layer is returned.
        /// </param>
        /// <remarks>
        /// These indices are specific to a data frame, and the current ordering of the Documents TOC.
        /// If the ArcMap user rearranges the TOC or switches dataframes between calls to <see cref="GetFeatureLayers"/>
        /// and this method, then the results may be unexpected, or an exception may be thrown.
        /// </remarks>
        /// <returns>The layer data</returns>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="MxDocument"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if indexes is null or empty and there is no selected layer in the focus map.</exception>
        /// <exception cref="InvalidOperationException">Thrown if an integer in indexes point inside an empty map or layer group.</exception>
        /// <exception cref="FormatException">Thrown if indexes cannot be converted to an array of integer.</exception>
        /// <exception cref="OverflowException">Thrown if indexes contains a string of digits too large to fit in a Int32.</exception>
        /// <exception cref="ArgumentException">Thrown if indexes contains no integers, or if any of the integer are out of range for the corresponding map or grouplayer.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the specified layer is not a feature layer.</exception>
        /// <exception cref="ArgumentException">Thrown if the specified layer is not found.</exception>
        System.Data.DataTable GetFeatureLayerData(string indexes);

        /// <summary>
        /// Gets the shape (in WGS84 well known text) and selected attributes of the selected layer in the active data frame
        /// </summary>
        /// <param name="fieldNames">
        /// A case-insensitive array of field names (or aliases) to return.
        /// Only field name aliases specified in the data source are searched, not the aliases in the layer properties.
        /// If fieldNames is null, all fields are returned.
        /// </param>
        /// <returns>The layer data</returns>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="MxDocument"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if there is no selected layer in the focus map.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the selected layer is not a feature layer.</exception>
        System.Data.DataTable GetFeatureLayerData(string[] fieldNames);

        /// <summary>
        /// Gets the shape (in WGS84 well known text) and selected attributes of the specified layer in the active data frame
        /// </summary>
        /// <param name="indexes">
        /// A hyphenated string of 0 based integers for the group/layer indices of the requested layer.
        /// The dictionary returned by <see cref="GetFeatureLayers"/> uses these indices as keys.
        /// if indexes is null, then the selected layer is returned.
        /// </param>
        /// <param name="fieldNames">
        /// A case-insensitive array of field names (or aliases) to return.
        /// Only field name aliases specified in the data source are searched, not the aliases in the layer properties
        /// If fieldNames is null, all fields are returned.
        /// </param>
        /// <remarks>
        /// These indices are specific to a data frame, and the current ordering of the Documents TOC.
        /// If the ArcMap user rearranges the TOC or switches dataframes between calls to <see cref="GetFeatureLayers"/>
        /// and this method, then the results may be unexpected, or an exception may be thrown.
        /// </remarks>
        /// <returns>The layer data</returns>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="MxDocument"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if indexes is null or empty and there is no selected layer in the focus map.</exception>
        /// <exception cref="InvalidOperationException">Thrown if an integer in indexes point inside an empty map or layer group.</exception>
        /// <exception cref="FormatException">Thrown if indexes cannot be converted to an array of integer.</exception>
        /// <exception cref="OverflowException">Thrown if indexes contains a string of digits too large to fit in a Int32.</exception>
        /// <exception cref="ArgumentException">Thrown if indexes contains no integers, or if any of the integer are out of range for the corresponding map or grouplayer.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the specified layer is not a feature layer.</exception>
        /// <exception cref="ArgumentException">Thrown if the specified layer is not found.</exception>
        System.Data.DataTable GetFeatureLayerData(string indexes, string[] fieldNames);


        /// <summary>
        /// Asks ArcMap to add the dataset to the focus (active) map
        /// </summary>
        /// <param name="gisDataType">Describes the format of the workspace string</param>
        /// <param name="workspace">The workspace where the data set is located</param>
        /// <param name="dataSet">The name of the data set</param>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="MxDocument"/> is null or link to ArcMap is broken.</exception>
        /// <exception cref="ArgumentException">Thrown if <see cref="workspace"/> or <see cref="dataSet"/> cannot be opened.</exception>
        void LoadDataSet(GisDataType gisDataType, string workspace, string dataSet);


        /// <summary>
        /// Draws a Gps point on a special graphics layer in the focus map.
        /// </summary>
        /// <remarks>
        /// This method does not refresh the display.  Call <see cref="RefreshDisplay()"/> when all drawing is done.
        /// The graphics layer and symbology are controlled by <see cref="Defaults"/>
        /// </remarks>
        /// <param name="latitude">The latitude in WGS84 decimal degrees -90(south) to +90 (north)</param>
        /// <param name="longitude">The longitude in WGS84 decimal degrees -180(west) to +180 (east)</param>
        /// <param name="direction">The direction of travel.  Angle in degrees with 0 = North and increasing clockwise to 360.</param>
        /// <param name="breadCrumbs">How the trail of historic points should be displayed</param>
        /// <returns>
        /// An integer used to uniquely identify this graphic
        /// </returns>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="MxDocument"/> is null or link to ArcMap is broken.</exception>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="Defaults"/>.GraphicsLayerName is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown if unable to access the real time graphics layer</exception>
        int DrawGpsPoint(double latitude, double longitude, double direction, BreadCrumbs breadCrumbs);


        /// <summary>
        /// Adds a graphic group (point marker and circles) to a special graphics layer in the focus map.
        /// </summary>
        /// <remarks>
        /// This method does not refresh the display.  Call <see cref="RefreshDisplay()"/> when all drawing is done. 
        /// The graphics layer and symbology are controlled by <see cref="Defaults"/>
        /// </remarks>
        /// <param name="latitude">The latitude in WGS84 decimal degrees -90(south) to +90 (north)</param>
        /// <param name="longitude">The longitude in WGS84 decimal degrees -180(west) to +180 (east)</param>
        /// <param name="radii">The radii (in meters) of the probability circles</param>
        /// <returns>
        /// An integer used to identify the newly created graphic group
        /// </returns>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="MxDocument"/> is null or link to ArcMap is broken.</exception>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="Defaults"/>.GraphicsLayerName is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown if unable to access the real time graphics layer</exception>
        int DrawGpsCep(double latitude, double longitude, double[] radii);


        /// <summary>
        /// Removes all graphics created by <see cref="DrawGpsPoint"/> and <see cref="DrawGpsCep"/> by removing the special graphics layer
        /// </summary>
        /// <remarks>
        /// User is responsible for refreshing the display after graphics are deleted.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="MxDocument"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="Defaults"/>.GraphicsLayerName is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown if unable to access the real time graphics layer</exception>
        void ClearGpsGraphics();


        /// <summary>
        /// Removes a graphic created by <see cref="DrawGpsPoint"/> or <see cref="DrawGpsCep"/>
        /// </summary>
        /// <remarks>
        /// This method does not refresh the display.  Call <see cref="RefreshDisplay()"/> when all drawing is done. 
        /// No action if id is not recognized as a valid graphic.
        /// User is responsible for refreshing the display after graphics are deleted. 
        /// </remarks>
        /// <param name="id">The graphic id created by DrawGpsPoint() or DrawGpsCep()</param>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="MxDocument"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="Defaults"/>.GraphicsLayerName is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown if unable to access the real time graphics layer</exception>
        void ClearGpsGraphics(int id);


        /// <summary>
        /// Removes an array of graphics created by <see cref="DrawGpsPoint"/> or <see cref="DrawGpsCep"/>
        /// </summary>
        /// <remarks>
        /// This method does not refresh the display.  Call <see cref="RefreshDisplay()"/> when all drawing is done. 
        /// No action if ids is null. Any id not recognized as a valid graphic is skipped.
        /// User is responsible for refreshing the display after graphics are deleted. 
        /// </remarks>
        /// <param name="ids">An array of graphic ids created by DrawGpsPoint() or DrawGpsCep()</param>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="MxDocument"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="Defaults"/>.GraphicsLayerName is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown if unable to access the real time graphics layer</exception>
        void ClearGpsGraphics(int[] ids);

        
        /// <summary>
        /// Redraws the map display.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="MxDocument"/> is null.</exception>
        void RefreshDisplay();


        /// <summary>
        /// If the input point is close to the edge, then the display is panned so point is in the center.
        /// </summary>
        /// <param name="latitude">The latitude (in WGS84) of input point</param>
        /// <param name="longitude">The longitude (in WGS84) of input point</param>
        /// <param name="percent">Acceptable closeness of the input point (0.0, 1.0]. 1.0 = edge, 0.0 = center
        /// Example: If percent = 0.7, then the screen will only pan if the 
        /// input point is more than 70% of the way from the center to the edge.
        /// </param>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="MxDocument"/> is null.</exception>
        void RefreshDisplay(double latitude, double longitude, double percent);

        event EventHandler<TenItemsAddedEventArgs> TenItemsAdded;

        /// <summary>
        /// Gets shape of the selected graphics, or all graphics if nothing is selected,
        /// from the active graphic layer of the active dataframe.
        /// <remarks>
        /// Text elements are ignored.
        /// Lines with curves are treated as simple lines between curve endpoints
        /// circles and ellipses cannot be represented in WKT, so they return a record with an empty shape.
        /// </remarks>
        /// </summary>
        /// <returns>A data table with one column, 'Shape', with WKT in WGS84</returns>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="MxDocument"/> is null.</exception>
        System.Data.DataTable GetGraphics();

        /// <summary>
        /// Creates graphics in the active graphic layer of the active dataframe.
        /// 
        /// </summary>
        /// <param name="dataTable">A data table with a Shape column with WKT in WGS84</param>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="MxDocument"/> is null.</exception>
        void AddGraphics(System.Data.DataTable dataTable);

    }

    public class TenItemsAddedEventArgs : EventArgs
    {
        public TenItemsAddedEventArgs(int items, int total)
        {
            ItemCount = items;
            TotalCount = total;
        }

        public int ItemCount { get; private set; }
        public int TotalCount { get; private set; }
    }
}
