using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF; //For ComReleaser; add reference to ESRI.ArcGIS.ADF.Connection.Local
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Display;
using System.Drawing;

namespace DnrGps_ArcMap
{
    //[Guid("2d5eb5e6-efdb-45a6-977e-5b5f6e26e8cd")]
    //[ProgId("DnrGps_ArcMap.DnrGpsController")]
    public class DnrGpsController : IDnrGpsController
    {
        
        private Defaults _defaults;
        private IElement _arrowMarker;
        private int _elementId;


        #region IDnrGpsController Interface members

        public IMxDocument MxDocument { get; set; }


        public Defaults Defaults
        {
            get
            {
                return _defaults ?? (_defaults = Defaults.GetDefault());
            }
        }


        #region methods that only READ from ArcMap

        public IDictionary GetFeatureLayers()
        {
            if (MxDocument == null)
                throw new InvalidOperationException("MxDocument not set");

            return GetSelectedFeatureLayers().ToDictionary(namedLayer => namedLayer.Index, namedLayer => namedLayer.Name);
        }

        public IDictionary GetAllFeatureLayers()
        {
            if (MxDocument == null)
                throw new InvalidOperationException("MxDocument not set");

            return InternalGetAllFeatureLayers().ToDictionary(namedLayer => namedLayer.Index, namedLayer => namedLayer.Name);
        }


        public DataTable GetFeatureLayerData()
        {
            return GetFeatureLayerData(null, null);
        }


        public DataTable GetFeatureLayerData(string layerIndexString)
        {
            return GetFeatureLayerData(layerIndexString, null);
        }


        public DataTable GetFeatureLayerData(string[] fieldNames)
        {
            return GetFeatureLayerData(null, fieldNames);
        }


        public DataTable GetFeatureLayerData(string layerIndexString, string[] fieldNames)
        {
            if (MxDocument == null)
                throw new InvalidOperationException("MxDocument not set");

            ILayer layer;
            if (string.IsNullOrEmpty(layerIndexString))
            {
                layer = GetSelectedLayer();
                if (layer == null)
                    throw new InvalidOperationException("There is no selected Layer");
            }
            else
            {
                layer = LayerUtils.GetLayer(MxDocument.FocusMap, layerIndexString);
                if (layer == null)
                    throw new ArgumentException("Feature layer '" + layerIndexString + "' not found.", "layerIndexString");
            }
            var featureLayer = layer as IGeoFeatureLayer;
            if (featureLayer == null)
                throw new InvalidOperationException("Requested layer is not a feature layer");

            return MakeTableFromFeatureLayer(featureLayer, fieldNames);
        }


        public DataTable GetGraphics()
        {
            if (MxDocument == null)
                throw new InvalidOperationException("MxDocument not set");

            return MakeTableFromGraphics();
        }

        #endregion


        #region Methods that UPDATE existing ArcObjects

        public void ClearGpsGraphics()
        {
            if (MxDocument == null)
                throw new InvalidOperationException("MxDocument not set");

            DeleteRealTimeGraphicsLayer();
            _arrowMarker = null;
            _elementId = 0;
            //Why do a full refresh?, Why do any refresh?
            MxDocument.ActiveView.Refresh();
            //MxDocument.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        }


        public void ClearGpsGraphics(int id)
        {
            if (MxDocument == null)
                throw new InvalidOperationException("MxDocument not set");

            IElement element;
            Graphics.Reset();
            while ((element = Graphics.Next()) != null)
            {
                object property = ((IElementProperties)element).CustomProperty;
                if (property == null || !(property is int) || ((int)property) != id)
                    continue;
                Graphics.DeleteElement(element);
                return;
            }
        }


        public void ClearGpsGraphics(int[] ids)
        {
            if (MxDocument == null)
                throw new InvalidOperationException("MxDocument not set");

            if (ids == null)
                return;

            foreach (var id in ids)
            {
                ClearGpsGraphics(id);
            }
        }


        public void RefreshDisplay()
        {
            if (MxDocument == null)
                throw new InvalidOperationException("MxDocument not set");

            MxDocument.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        }

        #endregion


        #region Methods that CREATE new ArcObjects

        public void LoadDataSet(GisDataType gisDataType, string workspace, string dataSet)
        {
            if (MxDocument == null)
                throw new InvalidOperationException("MxDocument not set");

            GetObjectFactoryOrFail();

            switch (gisDataType)
            {
                case GisDataType.Shapefile:
                    LoadDataSet(ShapefileWorkspaceFromPath(workspace), dataSet);
                    break;
                case GisDataType.FileGeodatabase:
                    LoadDataSet(FileGdbWorkspaceFromPath(workspace), dataSet);
                    break;
                case GisDataType.SdeConnectionString:
                    LoadDataSet(SdeWorkspaceFromConnectionString(workspace), dataSet);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("gisDataType");
            }
        }


        public void AddGraphics(DataTable dataTable)
        {
            if (MxDocument == null)
                throw new InvalidOperationException("MxDocument not set");

            GetObjectFactoryOrFail();

            if (dataTable == null)
                throw new ArgumentNullException("dataTable");

            if (!dataTable.Columns.Contains("Shape"))
                throw new ArgumentException("Parameter must have a column named 'Shape'", "dataTable");

            if (dataTable.Rows.Count == 0)
                return;

            AddGraphicsFromDataTable(dataTable);
        }


        public int DrawGpsPoint(double latitude, double longitude, double direction, BreadCrumbs breadCrumbs)
        {
            if (MxDocument == null)
                throw new InvalidOperationException("MxDocument not set");

            GetObjectFactoryOrFail();

            return AddGpsPointGraphic(latitude, longitude, direction, breadCrumbs);
        }


        public int DrawGpsCep(double latitude, double longitude, double[] radii)
        {
            if (MxDocument == null)
                throw new InvalidOperationException("MxDocument not set");

            GetObjectFactoryOrFail();

            ISpatialReference sr = MxDocument.FocusMap.SpatialReference;

            if (sr == null || !(sr is IProjectedCoordinateSystem))
                throw new InvalidOperationException("Map must have a projected coordinate system to draw a circle");

            esriUnits mapUnits = MxDocument.FocusMap.MapUnits;
            if (mapUnits == esriUnits.esriUnknownUnits ||
                mapUnits == esriUnits.esriDecimalDegrees ||
                mapUnits == esriUnits.esriUnitsLast)
                throw new InvalidOperationException("Map must have a well known units to draw a circle");

            //IUnitConverter uc = new UnitConverterClass();
            var uc = (IUnitConverter)_objectFactory.Create("esriSystem.UnitConverter");
            var mapRadii = from radius in radii
                           select uc.ConvertUnits(radius, esriUnits.esriMeters, mapUnits);
            //IPoint point = new PointClass();
            var point = (IPoint)_objectFactory.Create("esriGeometry.Point");
            point.PutCoords(longitude, latitude);
            point.SpatialReference = Wgs84;
            point.Project(sr);
            if (point.IsEmpty)
                throw new InvalidOperationException("Unable to project CEP coordinates onto the map");

            int id = AddCepGraphic(point, mapRadii.ToList());
            return id;
        }


        public void RefreshDisplay(double latitude, double longitude, double percent)
        {
            if (MxDocument == null)
                throw new InvalidOperationException("MxDocument not set");

            GetObjectFactoryOrFail();

            //IPoint center = new PointClass();
            var center = (IPoint)_objectFactory.Create("esriGeometry.Point");
            center.PutCoords(longitude, latitude);
            center.SpatialReference = Wgs84;
            center.Project(MxDocument.FocusMap.SpatialReference);

            var map = ((IActiveView)MxDocument.FocusMap);
            IEnvelope extents = map.Extent;
            extents.Expand(percent, percent, true); //If percent is less than one it will shrink
            if (((IRelationalOperator)center).Within(extents))
            {
                map.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
            }
            else
            {
                extents = map.Extent;
                extents.CenterAt(center);
                map.Extent = extents;
                map.Refresh();
            }
        }

        #endregion
        #endregion


        #region ObjectFactory

        private void GetObjectFactoryOrFail()
        {
            if (_objectFactory == null)
                _objectFactory = GetObjectFactory();
            if (_objectFactory == null)
                throw new InvalidOperationException("Unable to get control of the ArcMap application");
        }

        private static IObjectFactory GetObjectFactory()
        {
            Type t = Type.GetTypeFromProgID("esriFramework.AppRef");
            object obj = Activator.CreateInstance(t);
            //ESRI.ArcGIS.Framework.IApplication pApp = obj as ESRI.ArcGIS.Framework.IApplication;
            return (IObjectFactory)obj;
        }

        public IObjectFactory ObjectFactory
        {
            get { return _objectFactory; }
            set { _objectFactory = value; }
        }

        private IObjectFactory _objectFactory;

        #endregion


        #region Spatial reference code

        private ISpatialReference Wgs84
        {
            get
            {
                if (_wgs84 == null)
                {
                    var srFactory = (ISpatialReferenceFactory3)new SpatialReferenceEnvironment();
                    _wgs84 = srFactory.CreateGeographicCoordinateSystem((int)esriSRGeoCSType.esriSRGeoCS_WGS1984);
                }
                return _wgs84;
            }
        }
        private ISpatialReference _wgs84;


        #endregion


        #region Load Data Set into Active Map

        private void LoadDataSet(IFeatureWorkspace featureWorkspace, string dataSet)
        {
            //IFeatureLayer featureLayer = new FeatureLayerClass();
            var featureLayer = (IFeatureLayer)_objectFactory.Create("esriCarto.FeatureLayer");
            try
            {
                featureLayer.FeatureClass = featureWorkspace.OpenFeatureClass(dataSet);
            }
            catch(COMException ex)
            {
                throw new ArgumentException("could not open '"+dataSet+"'.", ex);
            }
            ILayer layer = featureLayer;
            layer.Name = featureLayer.FeatureClass.AliasName;
            MxDocument.FocusMap.AddLayer(layer);
        }

        private IFeatureWorkspace ShapefileWorkspaceFromPath(string path)
        {
            //var workspaceFactory = new ShapefileWorkspaceFactoryClass();
            var workspaceFactory = (IWorkspaceFactory)_objectFactory.Create("esriDataSourcesFile.ShapefileWorkspaceFactory");
            var workspace = workspaceFactory.OpenFromFile(path, 0) as IFeatureWorkspace;
            if (workspace == null)
                throw new ArgumentException("Unable to open '" + path + "' for reading shapefiles");
            return workspace;
        }

        private IFeatureWorkspace FileGdbWorkspaceFromPath(string path)
        {
            //var workspaceFactory = new FileGDBWorkspaceFactoryClass();
            var workspaceFactory = (IWorkspaceFactory)_objectFactory.Create("esriDataSourcesGDB.FileGDBWorkspaceFactory");
            var workspace = workspaceFactory.OpenFromFile(path, 0) as IFeatureWorkspace;
            if (workspace == null)
                throw new ArgumentException("Unable to open '" + path + "' as a file geodatabase");
            return workspace;
        }

        private IFeatureWorkspace SdeWorkspaceFromConnectionString(string conn)
        {
            //var workspaceFactory = (IWorkspaceFactory2)new SdeWorkspaceFactory();
            var workspaceFactory = (IWorkspaceFactory2)_objectFactory.Create("esriDataSourcesGDB.SdeWorkspaceFactory");
            var workspace = workspaceFactory.OpenFromString(conn, 0) as IFeatureWorkspace;
            if (workspace == null)
                throw new ArgumentException("Unable to open '" + conn + "' as an SDE connection");
            return workspace;
        }

        #endregion


        #region Graphic drawing

        private void AddGraphic(IGeometry geometry)
        {
            if (geometry.IsEmpty)
                return;

            var graphics = (IGraphicsContainer)MxDocument.FocusMap.ActiveGraphicsLayer;

            //if geometry or FocusMap has no projection then it will not project; no error, it just remains unchanged
            geometry.Project(MxDocument.FocusMap.SpatialReference);

            if (geometry is IPoint)
                AddPointGraphic(geometry, graphics);
            if (geometry is IMultipoint)
                AddMultiPointGraphic(geometry, graphics);
            if (geometry is IPolyline)
                AddPolylineGraphic(geometry, graphics);
            if (geometry is IPolygon)
                AddPolygonGraphic(geometry, graphics);
        }


        private void AddPointGraphic(IGeometry point, IGraphicsContainer graphics)
        {
            //ISimpleMarkerSymbol pointSymbol = new SimpleMarkerSymbolClass();
            var pointSymbol = (ISimpleMarkerSymbol)_objectFactory.Create("esriDisplay.SimpleMarkerSymbol");
            pointSymbol.Outline = true;
            if (Defaults.MarkerOutlineWidth <= 0.0)
                pointSymbol.Outline = false;
            else
            {
                pointSymbol.Outline = true;
                pointSymbol.OutlineSize = Defaults.MarkerOutlineWidth;
                pointSymbol.OutlineColor = Defaults.MarkerOutlineColor.ToEsri(_objectFactory);
            }
            //pointSymbol.Style = esriSimpleMarkerStyle.esriSMSCircle;
            ((IMarkerSymbol)pointSymbol).Color = Defaults.MarkerColor.ToEsri(_objectFactory);
            ((IMarkerSymbol)pointSymbol).Size = Defaults.MarkerSize;

            //IElement element = new MarkerElementClass();
            var element = (IElement)_objectFactory.Create("esriCarto.MarkerElement");
            ((IMarkerElement)element).Symbol = pointSymbol;
            element.Geometry = point;
            graphics.AddElement(element, 0);
        }


        private void AddMultiPointGraphic(IGeometry points, IGraphicsContainer graphics)
        {
            var pointCollection = ((IPointCollection)points);
            var pointCount = pointCollection.PointCount;
            for (int i = 0; i < pointCount; i++)
            {
                AddPointGraphic(pointCollection.Point[i], graphics);
            }
        }


        private void AddPolylineGraphic(IGeometry polyline, IGraphicsContainer graphics)
        {
            //ISimpleLineSymbol lineSymbol = new SimpleLineSymbolClass();
            var lineSymbol = (ISimpleLineSymbol)_objectFactory.Create("esriDisplay.SimpleLineSymbol");
            lineSymbol.Color = Defaults.LineColor.ToEsri(_objectFactory);
            lineSymbol.Width = Defaults.LineWidth;
            //lineSymbol.Style = esriSimpleLineStyle.esriSLSSolid;

            //IElement element = new LineElementClass();
            var element = (IElement)_objectFactory.Create("esriCarto.LineElement");
            ((ILineElement)element).Symbol = lineSymbol;
            element.Geometry = polyline;
            graphics.AddElement(element, 0);
        }


        private void AddPolygonGraphic(IGeometry polyline, IGraphicsContainer graphics)
        {
            //ISimpleFillSymbol polygonSymbol = new SimpleFillSymbolClass();
            var polygonSymbol = (ISimpleFillSymbol)_objectFactory.Create("esriDisplay.SimpleFillSymbol");
            polygonSymbol.Color = Defaults.PolygonFillColor.ToEsri(_objectFactory);
            polygonSymbol.Outline.Color = Defaults.PolygonOutlineColor.ToEsri(_objectFactory);
            polygonSymbol.Outline.Width = Defaults.PolygonOutlineWidth;
            //polygonSymbol.Style = esriSimpleFillStyle.esriSFSSolid;

            //IElement element = new PolygonElementClass();
            var element = (IElement)_objectFactory.Create("esriCarto.PolygonElement");
            ((IFillShapeElement)element).Symbol = polygonSymbol;
            element.Geometry = polyline;
            graphics.AddElement(element, 0);
        }


        private int AddGpsPointGraphic(double latitude, double longitude, double direction, BreadCrumbs breadCrumbs)
        {
            //ESRI symbol angles are counter-clockwise degrees from 0 (East, X) to 360
            //Our direction is an azimuth - degrees clockwise from 0 (North, Y) to 360
            double angle = 360 - direction + 90;
            angle = (angle + 360) % 360;
            //IPoint point = new PointClass();
            var point = (IPoint)_objectFactory.Create("esriGeometry.Point");
            point.PutCoords(longitude, latitude);
            point.SpatialReference = Wgs84;
            //if sr is null or unknown, then point is not changed.
            point.Project(MxDocument.FocusMap.SpatialReference);
            if (point.IsEmpty)
                throw new InvalidOperationException("Unable to project Gps Point onto map; may be out of bounds");

            if (breadCrumbs == BreadCrumbs.None)
            {
                if (_arrowMarker == null)
                    _arrowMarker = CreateArrowMarker(point, angle);
                else
                {
                    _arrowMarker.Move(point);
                    _arrowMarker.Rotate(angle);
                }
            }
            if (breadCrumbs == BreadCrumbs.SmallSymbols)
            {
                if (_arrowMarker == null)
                {
                    _arrowMarker = CreateArrowMarker(point, angle);
                }
                else
                {
                    double scale = Defaults.PreviousGpsPointSize / Defaults.CurrentGpsPointSize;
                    _arrowMarker.Scale(scale); //Scale the old one
                    _arrowMarker = CreateArrowMarker(point, angle); //Create a new one
                }
            }
            if (breadCrumbs == BreadCrumbs.Lines)
            {
                if (_arrowMarker == null)
                    _arrowMarker = CreateArrowMarker(point, angle);
                else
                {
                    var line = AddLineGraphic((IPoint)_arrowMarker.Geometry, point);
                    SwapGraphicIds(line, _arrowMarker);
                    _arrowMarker.Move(point);
                    _arrowMarker.Rotate(angle);
                }
            }

            return (int)((IElementProperties)_arrowMarker).CustomProperty;
        }

        
        private IElement AddLineGraphic(IPoint point1, IPoint point2)
        {
            //ISimpleLineSymbol lineSymbol = new SimpleLineSymbolClass();
            var lineSymbol = (ISimpleLineSymbol)_objectFactory.Create("esriDisplay.SimpleLineSymbol");

            lineSymbol.Color = Defaults.GpsTrackColor.ToEsri(_objectFactory);
            lineSymbol.Width = Defaults.GpsTrackWidth;
            //lineSymbol.Style = esriSimpleLineStyle.esriSLSSolid;

            //IElement line = new LineElementClass();
            var line = (IElement)_objectFactory.Create("esriCarto.LineElement");
            ((ILineElement)line).Symbol = lineSymbol;
            //IPolyline lineGeometry = new PolylineClass();
            var lineGeometry = (IPolyline)_objectFactory.Create("esriGeometry.Polyline");
            lineGeometry.FromPoint = point1;
            lineGeometry.ToPoint = point2;
            line.Geometry = lineGeometry;
            _elementId++;
            ((IElementProperties)line).CustomProperty = _elementId;
            Graphics.AddElement(line, 0);
            return line;
        }


        private IElement CreateArrowMarker(IPoint point, double angle)
        {
            //IMarkerSymbol arrowSymbol = new ArrowMarkerSymbolClass();
            var arrowSymbol = (IMarkerSymbol)_objectFactory.Create("esriDisplay.ArrowMarkerSymbol");
            arrowSymbol.Angle = angle;
            arrowSymbol.Color = Defaults.CurrentGpsPointColor.ToEsri(_objectFactory);
            arrowSymbol.Size = Defaults.CurrentGpsPointSize;

            //IElement marker = new MarkerElementClass();
            var marker = (IElement)_objectFactory.Create("esriCarto.MarkerElement");
            ((IMarkerElement)marker).Symbol = arrowSymbol;
            marker.Geometry = point;
            _elementId++;
            ((IElementProperties)marker).CustomProperty = _elementId;
            Graphics.AddElement(marker, 0);
            return marker;
        }

        private int AddCepGraphic(IPoint point, IList<double> circleRadii)
        {
            //IMarkerSymbol symbol = new SimpleMarkerSymbolClass();
            var symbol = (IMarkerSymbol)_objectFactory.Create("esriDisplay.SimpleMarkerSymbol");
            symbol.Color = Defaults.CepCenterPointColor.ToEsri(_objectFactory);
            symbol.Size = Defaults.CepCenterPointSize;

            //IElement centerPoint = new MarkerElementClass();
            var centerPoint = (IElement)_objectFactory.Create("esriCarto.MarkerElement");
            ((IMarkerElement)centerPoint).Symbol = symbol;
            centerPoint.Geometry = point;

            //Create a graphic group and add the center point graphic
            //IGroupElement group = new GroupElementClass();
            var group = (IGroupElement)_objectFactory.Create("esriCarto.GroupElement");
            group.AddElement(centerPoint);

            if (circleRadii != null)
            {
                //Add the circles to the group
                for (int i = 0; i < circleRadii.Count; i++)
                {
                    //IConstructCircularArc arc = new CircularArcClass();
                    var arc = (IConstructCircularArc)_objectFactory.Create("esriGeometry.CircularArc");
                    arc.ConstructCircle(point, circleRadii[i], false);
                    //IGeometry polygon = new PolygonClass();
                    var polygon = (IGeometry)_objectFactory.Create("esriGeometry.Polygon");
                    ((ISegmentCollection)polygon).AddSegment((ISegment)arc);
                    //IElement circle = new CircleElementClass();
                    var circle = (IElement)_objectFactory.Create("esriCarto.CircleElement");
                    circle.Geometry = polygon;

                    var circleColor = Color.Black;
                    if (Defaults.CepCircleOutlineColors != null)
                    {
                        var circleCount = Defaults.CepCircleOutlineColors.Length;
                        if (circleCount > 0)
                        {
                            circleColor = Defaults.CepCircleOutlineColors[circleCount - 1];
                            if (circleCount > i)
                                circleColor = Defaults.CepCircleOutlineColors[i];
                        }
                    }

                    var circleWidth = 1.0;
                    if (Defaults.CepCircleOutlineWidths != null)
                    {
                        var circleCount = Defaults.CepCircleOutlineWidths.Length;
                        if (circleCount > 0)
                        {
                            circleWidth = Defaults.CepCircleOutlineWidths[circleCount - 1];
                            if (circleCount > i)
                                circleWidth = Defaults.CepCircleOutlineWidths[i];
                        }
                    }

                    ((IFillShapeElement)circle).Symbol = GetCepCircleSymbol(circleColor, circleWidth);
                    group.AddElement(circle);
                }
            }

            //Give it the group an id number and add it to the graphics layer
            _elementId++;
            ((IElementProperties)group).CustomProperty = _elementId;
            Graphics.AddElement((IElement)group, 0);
            return _elementId;
        }

        private IFillSymbol GetCepCircleSymbol(Color lineColor, double lineWidth)
        {
            //ISimpleLineSymbol line = new SimpleLineSymbolClass();
            var line = (ISimpleLineSymbol)_objectFactory.Create("esriDisplay.SimpleLineSymbol");
            line.Color = lineColor.ToEsri(_objectFactory);
            line.Width = lineWidth;
            //line.Style = esriSimpleLineStyle.esriSLSSolid;

            //ISimpleFillSymbol circle = new SimpleFillSymbolClass();
            var circle = (ISimpleFillSymbol)_objectFactory.Create("esriDisplay.SimpleFillSymbol");
            circle.Outline = line;

            //IColor fill = new GrayColorClass();
            var fill = (IColor)_objectFactory.Create("esriDisplay.GrayColor");
            fill.Transparency = 0;
            circle.Color = fill;
            return circle;
        }

        private static void SwapGraphicIds(IElement element1, IElement element2)
        {
            object obj = ((IElementProperties)element1).CustomProperty;
            ((IElementProperties)element1).CustomProperty = ((IElementProperties)element2).CustomProperty;
            ((IElementProperties)element2).CustomProperty = obj;
        }

        #endregion


        #region RealTime Graphics Layer
        //Graphics are created and deleted from a special graphics layer in the
        //focus map (the active data frame).
        //The special graphics layer is discovered/created on each operation, and 
        //it cannot be cached, since the ArcMap user may delete it at any time.
        //It is possible to have multiple special graphics layers with various
        //graphics on each if the ArcMap user changes the active data frame between
        //these calls.
        //The graphic ids created are unique across all maps, however, only only the
        //current focus map is seached when deleting graphics.  Therefore is if you
        //created a graphic when dataframe1 is active, you can only delete it when
        //dataframe1 is active.
        //The ArcMap user may delete the graphic layer or any of the graphics on it, so
        //it is not an error for these functions to not find the layer or a graphic.
        //This code cannot tell when the ArcMap user changes the focus map,
        //or deletes layers or graphics.

        private IGraphicsContainer Graphics
        {
            get
            { 
                var graphics = FindRealTimeGraphicsLayer() ?? CreateRealTimeGraphicsLayer();
                if (graphics == null)
                    throw new InvalidOperationException("Unable to create the real time graphics layer.");
                return graphics;
            }
        }

        private IGraphicsContainer FindRealTimeGraphicsLayer()
        {
            if (string.IsNullOrEmpty(Defaults.GraphicsLayerName))
                throw new InvalidOperationException("Graphics layer name is null or empty");

            var gLayers = (ICompositeGraphicsLayer)MxDocument.FocusMap.BasicGraphicsLayer;
            try
            {
                return gLayers.FindLayer(Defaults.GraphicsLayerName) as IGraphicsContainer;
            }
            catch (COMException)
            {
                //FindLayer throws a COM exception if layer name is not found
                return null;
            }
        }

        private IGraphicsContainer CreateRealTimeGraphicsLayer()
        {
            if (string.IsNullOrEmpty(Defaults.GraphicsLayerName))
                throw new InvalidOperationException("Graphics layer name is null or empty");

            var gLayers = (ICompositeGraphicsLayer)MxDocument.FocusMap.BasicGraphicsLayer;
            return (IGraphicsContainer)gLayers.AddLayer(Defaults.GraphicsLayerName, null);
        }

        private void DeleteRealTimeGraphicsLayer()
        {
            if (string.IsNullOrEmpty(Defaults.GraphicsLayerName))
                throw new InvalidOperationException("Graphics layer name is null or empty");

            var gLayers = (ICompositeGraphicsLayer)MxDocument.FocusMap.BasicGraphicsLayer;
            try
            {
                gLayers.DeleteLayer(Defaults.GraphicsLayerName);
            }
            catch (COMException)
            {
                //DeleteLayer throws a COM exception if layer name is not found
            }
            //catch {             }
        }

        #endregion


        #region Layer methods

        private ILayer GetSelectedLayer()
        {
            return MxDocument.SelectedLayer;
        }

        private IEnumerable<NamedLayer> InternalGetFeatureLayers2()
        {
            return GetLayersFromFocusMap("{40A9E885-5533-11d0-98BE-00805F7CED21}"); // IFeatureLayer
        }


        private IEnumerable<NamedLayer> InternalGetAllFeatureLayers()
        {
            return LayerUtils.SearchFocusToc(MxDocument.FocusMap, typeof(IGeoFeatureLayer));
        }


        private IEnumerable<NamedLayer> GetSelectedFeatureLayers()
        {
            if (MxDocument.SelectedLayer is IGeoFeatureLayer)
            {
                var layer = MxDocument.SelectedLayer;
                return new List<NamedLayer>
                           {
                               new NamedLayer
                                   {
                                       Name = LayerUtils.GetFullName(MxDocument.FocusMap, layer),
                                       Index = LayerUtils.GetIndexString(MxDocument.FocusMap, layer),
                                       Layer = layer
                                   }
                           };
            }
            if (MxDocument.SelectedLayer is IGroupLayer)
            {
                var layer = MxDocument.SelectedLayer;
                var layers = new List<NamedLayer>();
                    LayerUtils.SearchLayer(layer as ICompositeLayer, typeof(IGeoFeatureLayer),layers,  
                        LayerUtils.GetIndexString(MxDocument.FocusMap, layer) + "-",
                        LayerUtils.GetFullName(MxDocument.FocusMap, layer) + "/");
                return layers.Count > 0 ? layers : InternalGetAllFeatureLayers();
            }
            IContentsViewSelection selection = GetSelection();
            if (selection != null && selection.SelectedItems.Count > 1)
            {
                var layers = GetAllSelectedFeatureLayers(selection.SelectedItems);
                return layers.Any() ? layers : InternalGetAllFeatureLayers();
            }
            return InternalGetAllFeatureLayers();
        }

        private IEnumerable<NamedLayer> GetAllSelectedFeatureLayers(ISet iSet)
        {
            var layers = new List<NamedLayer>();
            iSet.Reset();
            object obj;
            while ((obj = iSet.Next()) != null )
            {
                if (!(obj is IGeoFeatureLayer))
                    continue;
                var layer = obj as ILayer;
                layers.Add(
                    new NamedLayer
                        {
                            Name = LayerUtils.GetFullName(MxDocument.FocusMap, layer),
                            Index = LayerUtils.GetIndexString(MxDocument.FocusMap, layer),
                            Layer = layer
                        }
                    );
            }
            return layers;
        }

        private IContentsViewSelection GetSelection()
        {
            for (int i = 0; i < MxDocument.ContentsViewCount; i++)
            {
                if (MxDocument.ContentsView[i] is TOCDisplayView)
                    return MxDocument.ContentsView[i] as IContentsViewSelection;
            }
            return null;
        }

        private IEnumerable<NamedLayer> GetLayersFromFocusMap(string type)
        {
            return from layer in LayerUtils.GetAllLayers(MxDocument.FocusMap, type)
                   let name = LayerUtils.GetFullName(MxDocument.FocusMap, layer)
                   let index = LayerUtils.GetIndexString(MxDocument.FocusMap, layer)
                   select new NamedLayer
                   {
                       Name = name,
                       Index = index,
                       Layer = layer
                   };
        }

        #endregion


        #region DataTable Methods

        private DataTable MakeTableFromFeatureLayer(IGeoFeatureLayer layer, string[] fieldNames)
        {
            if (layer == null)
                return null;
            var table = new DataTable();
            ISelectionSet selection = ((IFeatureSelection)layer).SelectionSet;

            //var comReleaser = (ComReleaser)_objectFactory.Create("esriADF.ComReleaser");
            //var comReleaser = new ComReleaser();
            using (var comReleaser = new ComReleaser())
            //using (comReleaser)
            {
                int totalFeatures;
                ICursor cursor;
                if (selection != null && selection.Count > 0)
                {
                    selection.Search(null, true, out cursor);
                    totalFeatures = selection.Count;
                }
                else
                {
                    cursor = (ICursor)layer.SearchDisplayFeatures(null, true);
                    totalFeatures = layer.DisplayFeatureClass.FeatureCount(null);
                }
                comReleaser.ManageLifetime(cursor);

                //var fields2 = (ILayerFields)layer;
                //FIXME - using all fields in FC, not the fields in the display settings

                var fields = cursor.Fields;
                Type[] types = GetTypes(fields);
                var indexes = new List<int>();
                string shapename = layer.FeatureClass.ShapeFieldName.ToLower();

                //Set up table structure
                for (int i = 0; i < fields.FieldCount; i++)
                {
                    string name = fields.Field[i].Name;
                    string alias = fields.Field[i].AliasName; //((IFieldInfo)fields.Field[i]).Alias;
                    //FIXME - get the layer properties to see if this field is visible
                    //bool visible = true; // ((IFieldInfo)fields.Field[i]).Visible;
                    bool useThisField = false;
                    if (fieldNames == null || name == shapename || fieldNames.Length == 0)
                        useThisField = true;
                    else
                    {
                        if (//visible && 
                            fieldNames.Any(fieldName =>
                            String.Compare(fieldName, name, StringComparison.OrdinalIgnoreCase) == 0 || 
                            String.Compare(fieldName, alias, StringComparison.OrdinalIgnoreCase) == 0))
                        {
                            useThisField = true;
                        }
                        
                    }
                    if (!useThisField)
                        continue;
                    var column = new DataColumn
                                     {
                                         ColumnName = name,
                                         Caption = alias,
                                         DataType = types[i]
                                     };
                    table.Columns.Add(column);
                    indexes.Add(i);
                }

                //Populate Table
                IRow row;
                int featureCount = 0;
                OnTenItemsAdded(featureCount, totalFeatures);
                while ((row = cursor.NextRow()) != null)
                {
                    DataRow newRow = table.NewRow();
                    foreach (var index in indexes)
                    {
                        if (row.Fields.Field[index].Type == esriFieldType.esriFieldTypeGeometry)
                            newRow[row.Fields.Field[index].Name] = GetWktFromGeometry((IGeometry)row.Value[index]);
                        else
                            newRow[row.Fields.Field[index].Name] = row.Value[index];
                    }
                    table.Rows.Add(newRow);

                    featureCount++;
                    if (featureCount % 10 == 0)
                        OnTenItemsAdded(featureCount, totalFeatures);
                }

            }  //Done with cursor

            return table;
        }


        private static Type[] GetTypes(IFields fields)
        {
            //FIXME - Finish testing
            var types = new Type[fields.FieldCount];
            for (int i = 0; i < fields.FieldCount; i++)
            {
                Type type = null;
                switch (fields.Field[i].Type)
                {
                    case esriFieldType.esriFieldTypeBlob:  //Untested
                    case esriFieldType.esriFieldTypeRaster:  //Untested
                        type = typeof (byte[]);
                        break;
                    case esriFieldType.esriFieldTypeSingle:  //Untested
                        type = typeof (float);
                        break;
                    case esriFieldType.esriFieldTypeDouble:
                        type = typeof (double);
                        break;
                    case esriFieldType.esriFieldTypeDate:
                        type = typeof (DateTime);
                        break;
                    case esriFieldType.esriFieldTypeGUID:
                    case esriFieldType.esriFieldTypeGlobalID:
                        type = typeof (Guid);
                        break;
                    case esriFieldType.esriFieldTypeInteger:
                    case esriFieldType.esriFieldTypeOID:
                    case esriFieldType.esriFieldTypeSmallInteger:  //Untested
                        type = typeof (int);
                        break;
                    case esriFieldType.esriFieldTypeGeometry:
                    case esriFieldType.esriFieldTypeString:
                    case esriFieldType.esriFieldTypeXML:  //Untested
                        type = typeof (string);
                        break;
                }
                types[i] = type;
            }
            return types;
        }


        private string GetWktFromGeometry(IGeometry geometry)
        {
            if (geometry.IsEmpty)
                return string.Empty;
            //if geometry has no projection then it will not project; no error, it just remains unchanged
            //TODO - if geometry.sr == null/unknown, and map.sr != null/unknown, use the maps projection
            geometry.Project(Wgs84);
            return geometry.ToWellKnownText();
        }

        #endregion


        #region Get/Add Graphics Collections

        private DataTable MakeTableFromGraphics()
        {
            var table = new DataTable();
            var column = new DataColumn
            {
                ColumnName = "Shape",
                DataType = typeof(string)
            };
            table.Columns.Add(column);

            var graphicLayer = MxDocument.FocusMap.ActiveGraphicsLayer;
            var selectedGraphics = (IGraphicsContainerSelect)graphicLayer;
            bool unselectRequired = false;
            if (selectedGraphics.ElementSelectionCount == 0)
            {
                // No graphics are selected.  I select them all, so I can get the count, then unselect them later
                selectedGraphics.SelectAllElements();
                unselectRequired = true;
            }
            int featureCount = 0;
            int totalFeatures = selectedGraphics.ElementSelectionCount;
            OnTenItemsAdded(featureCount, totalFeatures);
            for (int i = 0; i < totalFeatures; i++)
            {
                IElement graphic = selectedGraphics.SelectedElement(i);
                if (graphic is ITextElement)
                    continue;
                DataRow newRow = table.NewRow();
                string shape = null;
                try
                {
                    shape = GetWktFromGeometry(graphic.Geometry);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.Print("Exception converting geometry to wkt: {0}",ex.Message);
                }
                if (shape != null)
                {
                    newRow["Shape"] = shape;
                    table.Rows.Add(newRow);
                    featureCount = i + 1;
                }
                if (featureCount % 10 == 0)
                    OnTenItemsAdded(featureCount, totalFeatures);
            }
            if (unselectRequired)
                selectedGraphics.UnselectAllElements();

            OnTenItemsAdded(featureCount, totalFeatures);
            return table;
        }

        private void AddGraphicsFromDataTable(DataTable dataTable)
        {
            int featureCount = 0;
            int totalFeatures = dataTable.Rows.Count;
            OnTenItemsAdded(featureCount, totalFeatures);
            foreach (DataRow row in dataTable.Rows)
            {
                var wkt = ((string)row["Shape"]);
                IGeometry geometry = null;
                try
                {
                    geometry = wkt.ToGeometry(_objectFactory);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.Print("Exception converting geometry to wkt: {0}",ex.Message);
                }
                if (geometry != null)
                {
                    geometry.SpatialReference = Wgs84;
                    AddGraphic(geometry);
                    featureCount++;
                }
                if (featureCount % 10 == 0)
                    OnTenItemsAdded(featureCount, totalFeatures);
            }
            OnTenItemsAdded(featureCount, totalFeatures);
        }

        #endregion


        #region Events

        public event EventHandler<TenItemsAddedEventArgs> TenItemsAdded;

        protected virtual void OnTenItemsAdded(int items, int total)
        {
            EventHandler<TenItemsAddedEventArgs> handler = TenItemsAdded;
            if (handler != null)
            {
                handler(this, new TenItemsAddedEventArgs(items, total));
            }
        }

        #endregion
    }

}