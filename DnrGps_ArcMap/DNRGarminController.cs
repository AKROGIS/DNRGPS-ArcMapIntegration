using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;
using NPS.AKRO.ArcGIS.Common;
using ESRI.ArcGIS.Display;

namespace DNRGarmin_ArcMap
{
    [Guid("2d5eb5e6-efdb-45a6-977e-5b5f6e26e8cd")]
    [ProgId("DNRGarmin_ArcMap.DNRGarminController")]
    public class DNRGarminController : IDNRGarminController
    {
        public IMxDocument MxDocument { get; set; }

        public DataTable GetFeatureLayerData(string layerName)
        {
            ILayer layer = GetLayerFromName(layerName);
            return MakeTableFromFeatureLayer(layer as IGeoFeatureLayer);
        }

        public string[] GetFeatureLayerNames()
        {
            if (MxDocument == null)
                return new string[0];
            return GetFeatureLayers().Select(x => x.Name).ToArray();
        }


        public int DrawGpsPoint(double latitude, double longitude, double direction, BreadCrumbs breadCrumbs)
        {
            if (MxDocument == null)
                return -1;
            if (GraphicsLayer == null)
                return -2;

            //ESRI symbol angles are counter-clockwise degrees from 0 (East, X) to 360
            //Our direction is an azimuth - degrees clockwise from 0 (North, Y) to 360
            double angle = 360 - direction + 90;
            angle = (angle + 360) % 360;
            IPoint point = new PointClass();
            point.PutCoords(longitude, latitude);
            point.SpatialReference = Wgs84;
            point.Project(MxDocument.FocusMap.SpatialReference);
            if (point.IsEmpty)
                return -3;

            if (breadCrumbs == BreadCrumbs.None)
            {
                if (_marker == null)
                    _marker = AddArrow(point, angle);
                else
                {
                    _marker.Move(point);
                    _marker.Rotate(angle);
                }
            }
            if (breadCrumbs == BreadCrumbs.SmallSymbols)
            {
                if (_marker == null)
                {
                    _marker = AddArrow(point, angle);
                }
                else
                {
                    double scale = Defaults.PreviousGpsPointSize / Defaults.CurrentGpsPointSize;
                    _marker.Scale(scale);  //Scale the old one
                    _marker = AddArrow(point, angle); //Create a new one
                }
            }
            if (breadCrumbs == BreadCrumbs.Lines)
            {
                if (_marker == null)
                    _marker = AddArrow(point, angle);
                else
                {
                    var line = AddLineGraphic((IPoint)_marker.Geometry, point);
                    SwapGraphicIds(line, _marker);
                    _marker.Move(point);
                    _marker.Rotate(angle);
                }
            }

            return (int)((IElementProperties)_marker).CustomProperty;
        }

        private IElement _marker;
        private int _elementId;


        public int DrawGpsCep(double latitude, double longitude, double[] radii)
        {
            if (MxDocument == null)
                return -1;
            if (GraphicsLayer == null)
                return -2;
            ISpatialReference sr = MxDocument.FocusMap.SpatialReference;

            if (sr == null || sr is IGeographicCoordinateSystem)
                return -3;
            esriUnits mapUnits = MxDocument.FocusMap.MapUnits;
            if (mapUnits == esriUnits.esriUnknownUnits ||
                mapUnits == esriUnits.esriDecimalDegrees ||
                mapUnits == esriUnits.esriUnitsLast)
                return -3;
            IUnitConverter uc = new UnitConverterClass();
            var mapRadii = from radius in radii
                           select uc.ConvertUnits(radius, esriUnits.esriMeters, mapUnits);
            IPoint point = new PointClass();
            point.PutCoords(longitude, latitude);
            point.SpatialReference = Wgs84;
            point.Project(sr);
            if (point.IsEmpty)
                return -3;

            int id = AddCepGraphic(point, mapRadii.ToList());
            return id;
        }

        public void RefreshDisplay(double latitude, double longitude, double direction, double width, PanBehavior behavior)
        {
            if (MxDocument == null)
                return;
            //FIXME - implement all behavior
            MxDocument.ActiveView.Refresh();
        }

        public void ClearGpsGraphics()
        {
            if (MxDocument == null)
                return;
            DeleteRealTimeGraphicsLayer();
        }

        public void ClearGpsGraphics(int id)
        {
            if (MxDocument == null)
                return;
            IElement element;
            ((IGraphicsContainer)GraphicsLayer).Reset();
            while ((element = ((IGraphicsContainer)GraphicsLayer).Next()) != null)
                if (((int)((IElementProperties)element).CustomProperty) == id)
                {
                    ((IGraphicsContainer)GraphicsLayer).DeleteElement(element);
                    return;
                }
        }

        public void ClearGpsGraphics(int[] ids)
        {
            if (MxDocument == null)
                return;
            foreach (var id in ids)
            {
                ClearGpsGraphics(id);
            }
        }

        public Defaults Defaults
        {
            get
            {
                return _defaults ?? (_defaults = new Defaults
                                                     {
                                                         GraphicsLayerName = "DNRGPS_Realtime_Graphics",
                                                         CepCenterPointColor = new Color(0, 0, 255),
                                                         CepCircleOutlineColors = new[]
                                                                                      {
                                                                                          new Color(200, 200, 200),
                                                                                          new Color(155, 155, 155),
                                                                                          new Color(100, 100, 100),
                                                                                          new Color(55, 55, 55)
                                                                                      },
                                                         GpsTrackColor = new Color(200, 125, 125),
                                                         CurrentGpsPointColor = new Color(255, 0, 0),
                                                         PreviousGpsPointColor = new Color(175, 0, 0),
                                                         CepCenterPointSize = 9,
                                                         CurrentGpsPointSize = 12,
                                                         PreviousGpsPointSize = 4,
                                                         CepCircleOutlineWidths = new[] { 0.1, 0.2, 0.5, 1.0 },
                                                         GpsTrackWidth = .1,
                                                     });
            }
            set { _defaults = value; }
        }
        private Defaults _defaults;

        private IElement AddLineGraphic(IPoint point1, IPoint point2)
        {
            //Create a new graphic element
            ILineSymbol lineSymbol = new SimpleLineSymbolClass();
            lineSymbol.Color = EsriColor(Defaults.GpsTrackColor);
            lineSymbol.Width = Defaults.GpsTrackWidth;

            IElement line = new LineElementClass();
            ((ILineElement)line).Symbol = lineSymbol;
            IPolyline lineGeometry = new PolylineClass();
            lineGeometry.FromPoint = point1;
            lineGeometry.ToPoint = point2;
            line.Geometry = lineGeometry;
            _elementId++;
            ((IElementProperties)line).CustomProperty = _elementId;
            ((IGraphicsContainer)GraphicsLayer).AddElement(line, 0);
            return line;
        }

        private IElement AddArrow(IPoint point, double angle)
        {
            //Create a new graphic element
            IMarkerSymbol arrowSymbol = new ArrowMarkerSymbolClass();
            arrowSymbol.Angle = angle;
            arrowSymbol.Color = EsriColor(Defaults.CurrentGpsPointColor);
            arrowSymbol.Size = Defaults.CurrentGpsPointSize;
            IElement marker = new MarkerElementClass();
            ((IMarkerElement)marker).Symbol = arrowSymbol;
            marker.Geometry = point;
            _elementId++;
            ((IElementProperties)marker).CustomProperty = _elementId;
            ((IGraphicsContainer)GraphicsLayer).AddElement(marker, 0);
            return marker;
        }

        private int AddCepGraphic(IPoint point, IList<double> circleRadii)
        {
            //Create a new center point graphic
            IMarkerSymbol symbol = new SimpleMarkerSymbolClass();
            symbol.Color = EsriColor(Defaults.CepCenterPointColor);
            symbol.Size = Defaults.CepCenterPointSize;
            IElement centerPoint = new MarkerElementClass();
            ((IMarkerElement)centerPoint).Symbol = symbol;
            centerPoint.Geometry = point;

            //Create a graphic group and add the center point graphic
            IGroupElement group = new GroupElementClass();
            group.AddElement(centerPoint);

            //Add the circles to the group
            for (int i = 0; i < circleRadii.Count; i++)
            {
                IConstructCircularArc arc = new CircularArcClass();
                arc.ConstructCircle(point, circleRadii[i],false);
                IGeometry polygon = new PolygonClass();
                ((ISegmentCollection)polygon).AddSegment((ISegment)arc);
                IElement circle = new CircleElementClass();
                circle.Geometry = polygon;
                Color circleColor = Defaults.CepCircleOutlineColors.Length > i
                                  ? Defaults.CepCircleOutlineColors[i]
                                  : Defaults.CepCircleOutlineColors[Defaults.CepCircleOutlineColors.Length - 1];
                double circleWidth = Defaults.CepCircleOutlineWidths.Length > i
                                   ? Defaults.CepCircleOutlineWidths[i]
                                   : Defaults.CepCircleOutlineWidths[Defaults.CepCircleOutlineWidths.Length - 1];
                ((IFillShapeElement)circle).Symbol = GetCircleSymbol(circleColor, circleWidth);
                group.AddElement(circle);
            }

            //Give it the group an id number and add it to the graphics layer
            _elementId++;
            ((IElementProperties)group).CustomProperty = _elementId;
            ((IGraphicsContainer)GraphicsLayer).AddElement((IElement)group, 0);
            return _elementId;
        }

        private static IFillSymbol GetCircleSymbol(Color lineColor, double lineWidth)
        {
            IFillSymbol circle = new SimpleFillSymbolClass();
            ILineSymbol line = new SimpleLineSymbolClass();
            line.Color = EsriColor(lineColor);
            line.Width = lineWidth;
            circle.Outline = line;
            IColor fill = new GrayColorClass();
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

        private static IColor EsriColor(Color color)
        {
            IRgbColor newColor = new RgbColorClass();
            newColor.Red = color.Red;
            newColor.Green = color.Green;
            newColor.Blue = color.Blue;
            return  newColor;
        }


        #region RealTime Graphics Layer
        
        private IGraphicsLayer GraphicsLayer
        {
            get
            { 
                if (_graphicsLayer == null)
                    _graphicsLayer = FindRealTimeGraphicsLayer();
                if (_graphicsLayer == null)
                    _graphicsLayer = CreateRealTimeGraphicsLayer();
                return _graphicsLayer;
            }
        }
        private IGraphicsLayer _graphicsLayer;

        private IGraphicsLayer FindRealTimeGraphicsLayer()
        {
            //Only look in the focus map (active data frame)
            //FindLayer throws a COM exception if layer is not found
            var gLayers = (ICompositeGraphicsLayer)MxDocument.FocusMap.BasicGraphicsLayer;
            try
            {
                return gLayers.FindLayer(Defaults.GraphicsLayerName);
            }
            catch (COMException)
            {
                return null;
            }
        }

        private IGraphicsLayer CreateRealTimeGraphicsLayer()
        {
            //Only create in the focus map (active data frame)
            var gLayers = (ICompositeGraphicsLayer)MxDocument.FocusMap.BasicGraphicsLayer;
            return gLayers.AddLayer(Defaults.GraphicsLayerName, null);
        }

        private void DeleteRealTimeGraphicsLayer()
        {
            if (_graphicsLayer == null)
                return;
            //Only delete from the focus map (active data frame)
            var gLayers = (ICompositeGraphicsLayer)MxDocument.FocusMap.BasicGraphicsLayer;
            gLayers.DeleteLayer(Defaults.GraphicsLayerName);
            _graphicsLayer = null;
        }

        #endregion


        private IEnumerable<NamedLayer> GetFeatureLayers()
        {
            return GetLayers("{40A9E885-5533-11d0-98BE-00805F7CED21}"); // IFeatureLayer
        }

        private IEnumerable<NamedLayer> GetLayers(string type)
        {
            return from layer in LayerUtils.GetAllLayers(MxDocument, type)
                   let name = MxDocument.Maps.Count > 1
                                  ? LayerUtils.GetFullName(MxDocument, layer)
                                  : LayerUtils.GetFullName(MxDocument.Maps.Item[0], layer)
                   select new NamedLayer
                   {
                       Name = name,
                       Layer = layer
                   };
        }

        private ILayer GetLayerFromName(string name)
        {
            try
            {
                return GetFeatureLayers().Where(x => x.Name == name).First().Layer;
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        /// <summary>
        /// Create a System.DataTable from the selected features in the layer
        /// </summary>
        /// <param name="layer"></param>
        /// <remarks>
        /// Uses the properties of the layer (field names/order, selection set, definition query)
        /// and not the underlying feature class.
        /// </remarks>
        /// <returns>System.DataTable</returns>
        private DataTable MakeTableFromFeatureLayer(IGeoFeatureLayer layer)
        {
            if (layer == null)
                return null;
            var table = new DataTable();
            ISelectionSet selection = ((IFeatureSelection)layer).SelectionSet;
            ICursor cursor;
            if (selection.Count > 0)
            {
                selection.Search(null, true, out cursor);
            }
            else
            {
                cursor = (ICursor)layer.SearchDisplayFeatures(null, true);
            }
            //var fields2 = (ILayerFields)layer;
            //FIXME - using all fields in FC, not the fields in the display settings
            //FIXME - if I can use the layer properties, make sure I get the shape column.
            //FIXME - Caption (Alias) is not set correctly, or is not being used
            var fields = cursor.Fields;
            Type[] types = GetTypes(fields);
            for (int i = 0; i < fields.FieldCount; i++)
            {
                var column = new DataColumn
                {
                    ColumnName = fields.Field[i].Name,
                    Caption = fields.Field[i].AliasName,
                    DataType = types[i]
                };
                table.Columns.Add(column);
            }
            IRow row;
            int fieldCount = cursor.Fields.FieldCount;
            while ((row = cursor.NextRow()) != null)
            {
                DataRow newRow = table.NewRow();
                for (int i = 0; i < fieldCount; i++)
                {
                    if (row.Fields.Field[i].Type == esriFieldType.esriFieldTypeGeometry)
                        newRow[row.Fields.Field[i].Name] = GetWktFromGeometry((IGeometry)row.Value[i]);
                    else
                        newRow[row.Fields.Field[i].Name] = row.Value[i];
                }
                table.Rows.Add(newRow);
            }
            return table;
        }

        private string GetWktFromGeometry(IGeometry geometry)
        {
            if (geometry.IsEmpty)
                return string.Empty;
            geometry.Project(Wgs84);
            //FIXME - finish implementing the geometry converter.
            //if (geometry.GeometryType == esriGeometryType.esriGeometryAny)
            if (geometry is IPoint)
                return string.Format("POINT ({0} {1})", ((IPoint)geometry).X, ((IPoint)geometry).Y);
            //FIXME - how do I distinguish between a linestring and a multiline string
            if (geometry is IPolyline)
                return string.Format("LINESTRING ({0} {1}, {2} {3})", ((IPolyline)geometry).FromPoint.X, ((IPolyline)geometry).FromPoint.Y, ((IPolyline)geometry).ToPoint.X, ((IPolyline)geometry).ToPoint.Y);
            if (geometry is IPolygon)
                return string.Format("POLYGON (({0} {1}))", ((IPolygon)geometry).FromPoint.X, ((IPolygon)geometry).FromPoint.Y);
            return string.Empty;
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

    }

}