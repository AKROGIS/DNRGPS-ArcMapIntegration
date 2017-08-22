//Color is a structure, so these properties always have a valid value
//Need to protect against nullable types

using System.Drawing;

namespace DnrGps_ArcMap
{
    public class Defaults
    {
        public string GraphicsLayerName { get; set; }

        public Color CurrentGpsPointColor { get; set; }
        public double CurrentGpsPointSize { get; set; }

        public Color PreviousGpsPointColor { get; set; }
        public double PreviousGpsPointSize { get; set; }

        public Color GpsTrackColor { get; set; }
        public double GpsTrackWidth { get; set; }

        public Color CepCenterPointColor { get; set; }
        public double CepCenterPointSize { get; set; }
        public Color[] CepCircleOutlineColors { get; set; }
        public double[] CepCircleOutlineWidths { get; set; }

        public double MarkerSize { get; set; }
        public Color MarkerColor { get; set; }
        public Color MarkerOutlineColor { get; set; }
        public double MarkerOutlineWidth { get; set; }
        
        public Color LineColor { get; set; }
        public double LineWidth { get; set; }

        public Color PolygonFillColor { get; set; }
        public Color PolygonOutlineColor { get; set; }
        public double PolygonOutlineWidth { get; set; }

        public static Defaults GetDefault()
        {
            return new Defaults
                {
                    GraphicsLayerName = "DNRGPS_Realtime_Graphics",

                    CurrentGpsPointColor = Color.Red,
                    CurrentGpsPointSize = 12,

                    PreviousGpsPointColor = Color.FromArgb(175, 0, 0),
                    PreviousGpsPointSize = 4,

                    GpsTrackColor = Color.FromArgb(200, 125, 125),
                    GpsTrackWidth = 0.1,

                    CepCenterPointColor = Color.Blue,
                    CepCenterPointSize = 9,
                    CepCircleOutlineColors = new[]
                                    {
                                        Color.FromArgb(200, 200, 200),
                                        Color.FromArgb(155, 155, 155),
                                        Color.FromArgb(100, 100, 100),
                                        Color.FromArgb(55, 55, 55)
                                    },
                    CepCircleOutlineWidths = new[] { 0.1, 0.2, 0.5, 1.0 },

                    MarkerSize = 7,
                    MarkerColor = Color.FromArgb(0, 255, 85),
                    MarkerOutlineColor = Color.Black,
                    MarkerOutlineWidth = 1,

                    LineColor = Color.Black,
                    LineWidth = 1,

                    PolygonFillColor = Color.FromArgb(255, 255, 190),
                    PolygonOutlineColor = Color.FromArgb(110, 110, 110),
                    PolygonOutlineWidth = 1,

                };
        }

    }

}
