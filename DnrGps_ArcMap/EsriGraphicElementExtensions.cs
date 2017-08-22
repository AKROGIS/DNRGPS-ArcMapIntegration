using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;

namespace DnrGps_ArcMap
{
    internal static class EsriGraphicElementExtensions
    {
        internal static void Rotate(this IElement element, double angle)
        {
            var marker = element as IMarkerElement;
            if (marker == null)
                return;
            //marker.Symbol.Angle = angle;  // doesn't work; getter returns a copy?
            var symbol = marker.Symbol;
            symbol.Angle = angle;
            marker.Symbol = symbol;
        }

        internal static void Move(this IElement element, IPoint point)
        {
            element.Geometry = point;
        }

        internal static void Scale(this IElement element, double scale)
        {
            var marker = element as IMarkerElement;
            if (marker == null)
                return;
            //marker.Symbol.Size = marker.Symbol.Size * scale; // doesn't work; getter returns a copy?
            var symbol = marker.Symbol;
            symbol.Size = symbol.Size * scale;
            marker.Symbol = symbol;
        }

    }
}
