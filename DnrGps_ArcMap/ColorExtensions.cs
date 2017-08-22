
namespace DnrGps_ArcMap
{
    internal static class ColorExtensions
    {
        /// <summary>
        /// Extentend System.Drawing.Color to return an ESRI Color object
        /// </summary>
        /// <param name="color">The System.Drawing.Color used to initialize the new color</param>
        /// <param name="objectFactory"> </param>
        /// <remarks>
        /// This is a convenience method, since the ESRI color objects are cumbersome to work with
        /// </remarks>
        /// <returns>ESRI.ArcGIS.Display.RgbColorClass cast as IColor</returns>
        internal static ESRI.ArcGIS.Display.IColor ToEsri(this System.Drawing.Color color, ESRI.ArcGIS.Framework.IObjectFactory objectFactory)
        {
            //var newColor = new ESRI.ArcGIS.Display.RgbColorClass();
            var newColor = (ESRI.ArcGIS.Display.IRgbColor)objectFactory.Create("esriDisplay.RgbColor");
            //color is a structure, so there will always be values; default is all zeros
            newColor.Red = color.R;
            newColor.Green = color.G;
            newColor.Blue = color.B;
            newColor.Transparency = color.A;
            return  newColor;
        }
    }
}